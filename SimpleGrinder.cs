using System;
using System.ComponentModel.Composition;
using ZzukBot.ExtensionFramework.Interfaces;
using System.Threading;
using ZzukBot.Game.Statics;
using ZzukBot.Objects;
using ZzukBot.ExtensionFramework;
using ZzukBot.ExtensionFramework.Classes;
using static ZzukBot.Constants.Enums;
using System.Collections.Generic;

[Export(typeof(IBotBase))]
public class SimpleGrinder : IBotBase
{
    public Thread MainThread;

    public Profile CurrentProfile;
    
    public static void DebugMsg(string String)
    {
        ZzukBot.ExtensionMethods.StringExtensions.Log(String, "SimpleGrinder.txt", false, true);
    }

    public bool IsManaClass(ClassId Class)
    {
        return Class != ClassId.Warrior && Class != ClassId.Rogue;
    }

    public WoWUnit GetClosestLootable(float Radius)
    {
        WoWUnit BestUnit = null;
        float BestDistance = Radius;

        foreach(WoWUnit Npc in NpcLootables)
        {
            if(Npc.CanBeLooted)
            {
                float Distance = Npc.DistanceToPlayer;

                if(Distance < BestDistance)
                {
                    BestUnit = Npc;
                    BestDistance = Distance;
                }
            }
        }

        return BestUnit;
    }
    
    public WoWUnit GetClosestMob(float Radius)
    {
        WoWUnit BestUnit = null;
        float BestDistance = Radius;//float.MaxValue;
        foreach(WoWUnit Npc in ObjectManager.Instance.Npcs)
        {
            if(Npc.IsMob && Npc.IsUntouched && 
                (Npc.Reaction == UnitReaction.Hostile || 
                Npc.Reaction == UnitReaction.Hostile2 ||
                CurrentProfile.FactionsContains(Npc.FactionId)))
            {
                float Distance = Npc.DistanceToPlayer;
                if(Distance < BestDistance)
                {
                    BestUnit = Npc;
                    BestDistance = Distance;
                }
            }
        }

        return BestUnit;
    }

    public WoWUnit GetBestAttacker()
    {
        WoWUnit BestUnit = null;
        float BestDistance = float.MaxValue;
        foreach(WoWUnit Npc in NpcAttackers)
        {
            float Distance = Npc.DistanceToPlayer;
            if(Distance < BestDistance)
            {
                BestUnit = Npc;
                BestDistance = Distance;
            }
        }

        return BestUnit;
    }
    
    public bool LoadCustomClass(ClassId LocalClass)
    {
        bool Loaded = false;
        
        CustomClasses.Instance.Refresh();

        int CustomClassIndex = 0;
        foreach (CustomClass CC in CustomClasses.Instance.Enumerator)
        {
            if (CC.Class == LocalClass)
            {
                DebugMsg("Using CustomClass: [" + CC.Author + " " + CC.Name + " " + CC.Class.ToString() + "]");
                CustomClasses.Instance.SetCurrent(CustomClassIndex);
                Loaded = true;
                break;
            }

            ++CustomClassIndex;
        }

        return Loaded;
    }
    
    public void Run(Action StopCallback)
    {
        //ZzukBot.Mem.MainThread.Instance.Invoke(()=> { Lua.Instance.Execute("AcceptResurrect();");});
        
        try
        {
            CurrentProfile = Profile.ParseProfile("D:\\[GRINDING] 8-10 Human (Near Goldshire).xml");

            if(CurrentProfile != null)
            {
                DebugMsg("Parsed " + CurrentProfile.Hotspots.Length + " hotspot(s).");
                DebugMsg("Parsed " + CurrentProfile.VendorHotspots.Length + " vendor hotspot(s).");
                DebugMsg("Parsed " + CurrentProfile.GhostHotspots.Length + " ghost hotspot(s).");

                CustomClass CurrentCC = new Warrior();
                // TODO: Call CustomClass#Load
            
                ClassId LoadedClass = 0;
                bool ReadyToPull = false;

                //bool UserSetGhostPath = CurrentProfile.GhostHotspots.Length > 0;
                bool UserSetGhostPath = false; // TODO: Implement, not 100% how user set waypoints are suppose work.
                bool UserSetVendorPath = CurrentProfile.VendorHotspots.Length > 0;

                PathManager ProfileGrindPath = new PathManager(CurrentProfile.Hotspots, true);
                ProfileGrindPath.Reset(true);

                PathManager ProfileGhostPath = UserSetGhostPath ? new PathManager(CurrentProfile.GhostHotspots) : null;
                PathManager ProfileVendorPath = UserSetVendorPath ? new PathManager(CurrentProfile.VendorHotspots) : null;
            
                bool RecalculatePath = true;
                bool WasAlive = true; // NOTE: Important that this be true, even if the bot was started when dead.
                while(MainThread != null)
                {
					// TODO: Wait for CustomClasses.Refresh fix.
                    /*if(LoadedClass != LocalPlayer.Class)
                    {
                        if(LoadCustomClass(LocalPlayer.Class))
                        {
                            LoadedClass = LocalPlayer.Class;

                            CustomClass CC = CustomClasses.Instance.Current;
                            DebugMsg("Current CustomClass: [" + CC.Author + " " + CC.Name + " " + CC.Class.ToString() + "]");
                        }
                        else
                        {
                            DebugMsg("Failed to find a CustomClass that matched requirements.");
                            Thread.Sleep(100);
                            continue;
                        }
                    }*/
            
                    if(ObjectManager.Instance.IsIngame && !Local.IsDead)
                    {
                        bool IsDead = Local.IsDead || Local.InGhostForm;

                        if(!IsDead)
                        {
                            WasAlive = true;

                            if(Local.IsInCombat && NpcAttackers.Count > 0)
                            {
                                RecalculatePath = true;
                                ReadyToPull = false;

                                if(Target == null || Target.Health <= 0)
                                {
                                    WoWUnit BestUnit = GetBestAttacker();

                                    if(BestUnit != null)
                                    {
                                        Local.SetTarget(BestUnit);
                                    }
                                }

                                if(Target != null)
                                {
                                    Local.Face(Target);
                                    
                                    if(Target.DistanceToPlayer <= CurrentCC.CombatDistance)
                                    {
                                        Local.CtmStopMovement();
                                        CurrentCC.OnFight();
                                    }
                                    else
                                    {
                                        Local.CtmTo(Target.Position);
                                    }
                                }
                            }
                            else
                            {
                                if(ReadyToPull)
                                {
                                    // TODO: Check if mob is targetting our pet.
                                    if(Target == null || Target.Health <= 0 || (!Target.IsFleeing && Target.IsInCombat && Target.TargetGuid != Local.Guid))
                                    {
                                        WoWUnit BestUnit = GetClosestMob(Settings.SearchMobRange);

                                        if(BestUnit != null)
                                        {
                                            Local.SetTarget(BestUnit);
                                        }
                                    }

                                    if(Target != null)
                                    {
                                        if(Target.DistanceToPlayer <= CurrentCC.CombatDistance)
                                        {
                                            Local.CtmStopMovement();
                                        }
                                        else
                                        {
                                            Local.CtmTo(Target.Position);
                                        }

                                        if(Target != null) // ???
                                        {
                                            Local.Face(Target);
                                            CurrentCC.OnPull();
                                        }

                                        RecalculatePath = true;
                                    }
                                    else
                                    {
                                        if(Local.IsCtmIdle)
                                        {
                                            if(RecalculatePath)
                                            {
                                                ProfileGrindPath.CalculatePath();
                                                //DebugMsg("recalc");
                                                RecalculatePath = false;
                                            }
                                            
                                            Location Next = ProfileGrindPath.Next();
                                            if(Next != null)
                                            {
                                                //DebugMsg("CtmTo: " + Next.X + " " + Next.Y + " " + Next.Z);
                                                //DebugMsg("Local: " + Local.Position.X + " " + Local.Position.Y + " " + Local.Position.Z);
                                                Local.CtmTo(Next);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    WoWUnit LootUnit = GetClosestLootable(100.0f);

                                    if(LootUnit != null)
                                    {
                                        if(LootUnit.DistanceToPlayer <= 3)
                                        {
                                            Local.CtmStopMovement();
                                            LootUnit.Interact(true);
                                        }
                                        else if(Local.IsCtmIdle)
                                        {
                                            Local.CtmTo(LootUnit.Position);
                                        }
                                    }
                                    else
                                    {
                                        CurrentCC.OnRest();
                                        if(CurrentCC.OnBuff())
                                        {
                                            if(Local.Health >= Settings.EatAt && 
                                                (!IsManaClass(Local.Class) || Local.Mana >= Settings.DrinkAt))
                                            {
                                                ReadyToPull = true;
                                            }
                                            else
                                            {
                                                if(!Local.IsEating && Local.Health < Settings.EatAt && Settings.Food != null)
                                                {
                                                    Local.Eat(Settings.Food);
                                                }

                                                if(IsManaClass(Local.Class))
                                                {
                                                    if(!Local.IsDrinking && Local.Mana < Settings.DrinkAt && Settings.Drink != null)
                                                    {
                                                        Local.Drink(Settings.Drink);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if(Local.IsDead)
                        {
                            //Lua.Instance.Execute("RetrieveCorpse();");

                        }
                        else if(Local.InGhostForm)
                        {
                            RecalculatePath = true;

                            if(WasAlive)
                            {
                                WasAlive = false;
                                
                                if(UserSetGhostPath)
                                {
                                    ProfileGhostPath.Reset(true);
                                }
                                else
                                {
                                    ProfileGhostPath = new PathManager(new Location[] {Local.CorpsePosition});
                                }
                            }
                            
                            if(ProfileGhostPath.ReachedWaypoint())
                            {
                                if(ProfileGhostPath.HasNext())
                                {
                                    Location Next = ProfileGhostPath.Next();
                                    if(Next != null)
                                    {
                                        Local.CtmTo(Next);
                                        //DebugMsg("CTM next" + ": " + Next.X + " " + Next.Y + " " + Next.Z);
                                    }
                                }
                                else
                                {
                                    // TODO: Reached destination!
                                    if(Local.TimeUntilResurrect == 0)
                                    {
                                        //Lua.Instance.Execute("AcceptResurrect();");
                                    }
                                }
                            }
                            else
                            {
                                Local.CtmTo(ProfileGhostPath.CurrentDest);
                                //DebugMsg("CTM current" + ": " + ProfileGhostPath.CurrentDest.X + " " + ProfileGhostPath.CurrentDest.Y + " " + ProfileGhostPath.CurrentDest.Z);
                            }
                        }
                    }

                    Thread.Sleep(100);
                }
            }
        }
        catch (Exception e)
        {
            DebugMsg(e.ToString());
        }
        
        MainThread = null;
        StopCallback();
    }

    public void PauseBotbase(Action onPauseCallback)
    {

    }

    public bool ResumeBotbase()
    {
        return true;
    }

    public void ShowGui()
    {

    }

    public bool Start(Action StopCallback)
    {
        Local.EnableCtm();
        MainThread = new Thread(() => Run(StopCallback));
        MainThread.Start();
        return true;
    }

    public void Stop()
    {
        MainThread = null;
    }

    public void Dispose()
    {

    }
    
    public List<WoWUnit> NpcLootables {get{return UnitInfo.Instance.Lootable;}}
    public List<WoWUnit> NpcAttackers {get{return UnitInfo.Instance.NpcAttackers;}}
    public WoWUnit Target {get{return ObjectManager.Instance.Target;}}
    public LocalPlayer Local {get{return ObjectManager.Instance.Player;}}
    public string Author { get { return "aswerw"; } }
    public string Name { get { return "SimpleGrinder"; } }
    public int Version { get { return 1; } }
}