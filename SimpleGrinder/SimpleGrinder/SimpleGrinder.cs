using System;
using System.ComponentModel.Composition;
using ZzukBot.ExtensionFramework.Interfaces;
using ZzukBot.Game.Statics;
using ZzukBot.Objects;
using ZzukBot.ExtensionFramework;
using ZzukBot.ExtensionFramework.Classes;
using static ZzukBot.Constants.Enums;
using System.Collections.Generic;
using GUI;
using System.Runtime.InteropServices;
using System.Windows.Forms;

[Export(typeof(IBotBase))]
public class SimpleGrinder : IBotBase
{
    public Profile CurrentProfile;
    public MainForm GUI;
    public bool UserSetGhostPath;
    public bool UserSetVendorPath;
        
    public bool RecalculatePath;
    public bool WasAlive;
    public bool ReadyToPull;
    
    public Action StopCallback;
    public PathManager ProfileGhostPath;
    public PathManager ProfileVendorPath;
    public PathManager ProfileGrindPath;
    public int NextTickTime;

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
            if(Npc.IsMob && Npc.IsUntouched && !Npc.IsKilledAndLooted && Npc.Health == Npc.MaxHealth &&
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
        foreach(CustomClass CC in CustomClasses.Instance.Enumerator)
        {
            if(CC.Class == LocalClass)
            {
                CustomClasses.Instance.SetCurrent(CustomClassIndex);
                Loaded = true;
                break;
            }

            ++CustomClassIndex;
        }

        return Loaded;
    }
    
    public int Tick()
    {
        int DelayTime = 0;

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
                        DelayTime = 100;
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
                    if(Target == null || Target.Health < 1 || (Target.IsInCombat && !Target.IsFleeing && Target.TargetGuid != Local.Guid && (!Local.HasPet || Target.TargetGuid != LocalPet.Guid)))
                    {
                        WoWUnit BestUnit = GetClosestMob(Settings.SearchMobRange);

                        if(BestUnit != null)
                        {
                            Local.SetTarget(BestUnit);
                        }
                    }

                    if(Target != null)
                    {
                        Local.Face(Target);
                        CurrentCC.OnPull();

                        if(Target.DistanceToPlayer <= CurrentCC.CombatDistance)
                        {
                            Local.CtmStopMovement();
                        }
                        else
                        {
                            Local.CtmTo(Target.Position);
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
                // NOTE: Stand idle if we have rez sickness unless attacked.
                else if(!Local.GotDebuff("Resurrection Sickness"))
                {
                    WoWUnit LootUnit = GetClosestLootable(100.0f);

                    if(LootUnit != null)
                    {
                        if(LootUnit.DistanceToPlayer <= 3)
                        {
                            Local.CtmStopMovement();
                            LootUnit.Interact(true);
                            DelayTime = 100;
                        }
                        else if(Local.IsCtmIdle)
                        {
                            Local.CtmTo(LootUnit.Position);
                        }
                    }
                    else
                    {
                        if(Local.HealthPercent < Settings.EatAt || 
                            (Util.IsManaClass(Local.Class) && Local.ManaPercent < Settings.DrinkAt))
                        {
                            if(!Local.IsEating || !Local.IsDrinking)
                            {
                                CurrentCC.OnRest();
                                DelayTime = 100;
                                                
                                bool ForceDrink = false;
                                if(Settings.Food != null && 
                                    !Local.IsEating &&
                                    Local.HealthPercent < Settings.EatAt)
                                {
                                    Local.Eat(Settings.Food);
                                    ForceDrink = Settings.AlwaysDrinkWhenEating;
                                }

                                if(Settings.Drink != null && 
                                    !Local.IsDrinking &&
                                    (ForceDrink || Local.ManaPercent < Settings.DrinkAt) && 
                                    Util.IsManaClass(Local.Class))
                                {
                                    Local.Drink(Settings.Drink);
                                }
                            }
                        }
                        else if(CurrentCC.OnBuff())
                        {
                            Util.DebugMsg("OnBuff success");
                            ReadyToPull = true;
                            DelayTime = 100;
                        }
                    }
                }
            }
        }
        else if(Local.IsDead)
        {
            Local.RepopMe();
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
                        Util.DebugMsg("CTM next" + ": " + Next.X + " " + Next.Y + " " + Next.Z);
                    }
                }
                else
                {
                    // TODO: Reached destination!
                    if(Local.TimeUntilResurrect == 0)
                    {
                        Local.RetrieveCorpse();
                    }
                }
            }
            else
            {
                Local.CtmTo(ProfileGhostPath.CurrentDest);
                Util.DebugMsg("CTM current" + ": " + ProfileGhostPath.CurrentDest.X + " " + ProfileGhostPath.CurrentDest.Y + " " + ProfileGhostPath.CurrentDest.Z);
            }
        }
        
        return DelayTime;
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
        if(GUI == null)
            GUI = new MainForm();

        GUI.Visible = !GUI.Visible;
    }
    
    public void Hook_EndScene(IntPtr Device)
    {
        if(ObjectManager.Instance.IsIngame)
        {
            try
            {
                if(Environment.TickCount >= NextTickTime)
                {
                    int Time = Tick();

                    if(Time != -1)
                    {
                        NextTickTime = Environment.TickCount + Time;
                    }
                    else
                    {
                        Stop();
                    }
                }
                else
                {
                    //Thread.Sleep(Environment.TickCount - NextTickTime);
                }
            }
            catch(Exception Ex)
            {
                Util.DebugMsg(Ex.ToString());
            }
        }
    }

    public bool Start(Action StopCallback)
    {
        if(!ObjectManager.Instance.IsIngame)
        {
            MessageBox.Show("Must be in game to start SimpleGrinder.");
            return false;
        }

        if(this.StopCallback != null)
        {
            Util.DebugMsg("StopCallback was not null.");
            return false;
        }

        if(Settings.ProfileFilePath == null)
        {
            MessageBox.Show("Must load a profile to start SimpleGrinder.");
            return false;
        }
        
        CurrentProfile = Profile.ParseV1Profile(Settings.ProfileFilePath);

        if(CurrentProfile == null)
        {
            MessageBox.Show("Unable to load selected profile.");
            return false;
        }
        
        Util.DebugMsg("Parsed " + CurrentProfile.Hotspots.Length + " hotspot(s).");
        Util.DebugMsg("Parsed " + CurrentProfile.VendorHotspots.Length + " vendor hotspot(s).");
        Util.DebugMsg("Parsed " + CurrentProfile.GhostHotspots.Length + " ghost hotspot(s).");
        
        if(LoadCustomClass(Local.Class))
        {
            Util.DebugMsg("Using CustomClass: [" + CurrentCC.Author + " " + CurrentCC.Name + " " + CurrentCC.Class.ToString() + "]");
        }
        else
        {
            MessageBox.Show("Failed to load CustomClass (missing or multiple of same class)");
            return false;
        }
        
        this.StopCallback = StopCallback;

        NextTickTime = Environment.TickCount;
        WasAlive = true;  // NOTE: Important that this be true, even if the bot was started when dead.
        RecalculatePath = true;
        ReadyToPull = false;
        
        ProfileGrindPath = new PathManager(CurrentProfile.Hotspots, true);
        ProfileGrindPath.Reset(true); // TODO: Add into constructor

        ProfileGhostPath = UserSetGhostPath ? new PathManager(CurrentProfile.GhostHotspots) : null;
        ProfileVendorPath = UserSetVendorPath ? new PathManager(CurrentProfile.VendorHotspots) : null;
            
        // UserSetGhostPath = CurrentProfile.GhostHotspots.Length > 0; // TODO: Implement, not really sure how to handle.
        UserSetVendorPath = CurrentProfile.VendorHotspots.Length > 0;

        DirectX.Instance.OnEndSceneExecution += Hook_EndScene;

        Local.EnableCtm();
        return true;
    }

    public void Stop()
    {
        if(StopCallback != null)
        {
            DirectX.Instance.OnEndSceneExecution -= Hook_EndScene;
            StopCallback();
            StopCallback = null;
        }
    }

    public void Dispose()
    {
        if(GUI != null)
        {
            GUI.Dispose();
            GUI = null;
        }
    }
    
    public CustomClass CurrentCC {get{return CustomClasses.Instance.Current;}}
    public List<WoWUnit> NpcLootables {get{return UnitInfo.Instance.Lootable;}}
    public List<WoWUnit> NpcAttackers {get{return UnitInfo.Instance.NpcAttackers;}}
    public WoWUnit Target {get{return ObjectManager.Instance.Target;}}
    public LocalPlayer Local {get{return ObjectManager.Instance.Player;}}
    public LocalPet LocalPet {get{return ObjectManager.Instance.Pet;}}
    public string Author { get { return "Blacknight"; } }
    public string Name { get { return "SimpleGrinder"; } }
    public int Version { get { return 1; } }
}