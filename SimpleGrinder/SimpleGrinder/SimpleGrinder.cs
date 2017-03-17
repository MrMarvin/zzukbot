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
    public bool UserSetVendorPath;
        
    public bool RecalculatePath;
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

        foreach(WoWUnit Npc in UnitInfo.Instance.Lootable)
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

    public WoWUnit GetClosestSkinnable(float Radius)
    {
        WoWUnit BestUnit = null;
        float BestDistance = Radius;

        foreach(WoWUnit Npc in UnitInfo.Instance.Skinable)
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
    
    public WoWUnit GetClosestMob(float Radius)
    {
        WoWUnit BestUnit = null;
        float BestDistance = Radius;//float.MaxValue;
        foreach(WoWUnit Npc in ObjectManager.Instance.Npcs)
        {
            if(Npc.IsMob && Npc.IsUntouched && !Npc.IsKilledAndLooted && Npc.Health == Npc.MaxHealth &&
                Math.Abs(Local.Level - Npc.Level) <= Settings.Instance.MaxLevelDifference &&
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

    public PathManager CurrentPath;
    public float CurrentPathAcceptableDistance;

    public int Tick()
    {
        int DelayTime = 0;
        
        if(Local.IsDead)
        {
            Local.RepopMe();
            return 500;
        }

        if(Local.InGhostForm && CurrentPath == null)
        {
            RecalculatePath = true;

            if(Local.CorpsePosition.GetDistanceTo(Local.Position) <= 11)
            {
                if(Local.TimeUntilResurrect == 0)
                {
                    Local.RetrieveCorpse();
                }
            }
            else
            {
                if(ProfileGhostPath != null)
                {
                    CurrentPath = ProfileGhostPath;
                    CurrentPath.Reset(true);
                }
                else
                {
                    CurrentPath = new PathManager(new Location[] {Local.CorpsePosition});
                }

                CurrentPathAcceptableDistance = 10;
            }
        }

        if(Local.IsInCombat && NpcAttackers.Count > 0)
        {
            CurrentPath = null;
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
        else if(CurrentPath != null)
        {
            if(CurrentPath.DistanceToDestination <= CurrentPathAcceptableDistance)
            {
                Local.CtmStopMovement();
                CurrentPath = null;
                CurrentPathAcceptableDistance = 0;
            }
            else if(CurrentPath.ReachedWaypoint())
            {
                if(CurrentPath.HasNext())
                {
                    Location Next = CurrentPath.Next();
                    if(Next != null)
                    {
                        Local.CtmTo(Next);
                        //Util.DebugMsg("CTM next" + ": " + Next.X + " " + Next.Y + " " + Next.Z);
                    }
                }
                else
                {
                    // NOTE: Reached destination!
                    Local.CtmStopMovement();
                    CurrentPath = null;
                    CurrentPathAcceptableDistance = 0;
                }
            }
            else
            {
                Local.CtmTo(CurrentPath.CurrentDest);
            }
        }
        else if(ReadyToPull)
        {
            if(Target == null || Target.Health < 1 || (Target.IsInCombat && !Target.IsFleeing && Target.TargetGuid != Local.Guid && (!Local.HasPet || Target.TargetGuid != LocalPet.Guid)))
            {
                WoWUnit BestUnit = GetClosestMob(Settings.Instance.SearchMobRange);

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
            if(Local.HealthPercent < Settings.Instance.EatAt || 
                (Util.IsManaClass(Local.Class) && Local.ManaPercent < Settings.Instance.DrinkAt))
            {
                if(!Local.IsEating || !Local.IsDrinking)
                {
                    Util.DebugMsg("Regen");
                    CurrentCC.OnRest();
                    DelayTime = 100;
                                                
                    bool ForceDrink = false;
                    if(Settings.Instance.Food != null && 
                        !Local.IsEating &&
                        Local.HealthPercent < Settings.Instance.EatAt)
                    {
                        Local.Eat(Settings.Instance.Food);
                        ForceDrink = Settings.Instance.AlwaysDrinkWhenEating;
                    }

                    if(Settings.Instance.Drink != null && 
                        !Local.IsDrinking &&
                        (ForceDrink || Local.ManaPercent < Settings.Instance.DrinkAt) && 
                        Util.IsManaClass(Local.Class))
                    {
                        Local.Drink(Settings.Instance.Drink);
                    }
                }
            }
            else
            {
                WoWUnit InteractUnit = null;

                if(Settings.Instance.Looting)
                {
                    InteractUnit = GetClosestLootable(50.0f);
                }

                if(Settings.Instance.Skinning && InteractUnit == null)
                {
                    InteractUnit = GetClosestSkinnable(50.0f);
                }
                    
                if(InteractUnit != null)
                {
                    if(InteractUnit.DistanceToPlayer <= 3)
                    {
                        InteractUnit.Interact(true);
                        DelayTime = 100;
                    }
                    else
                    {
                        CurrentPath = new PathManager(new Location[] {InteractUnit.Position});
                        CurrentPathAcceptableDistance = 3;
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

        if(Settings.Instance.ProfileFilePath == null)
        {
            MessageBox.Show("Must load a profile to start SimpleGrinder.");
            return false;
        }
        
        CurrentProfile = Profile.ParseV1Profile(Settings.Instance.ProfileFilePath);

        if(CurrentProfile == null)
        {
            MessageBox.Show("Unable to load selected profile.");
            Settings.Instance.ProfileFilePath = null;
            Settings.SaveSettings();
            GUI.ProfileNameLabel.Text = "";
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
        RecalculatePath = true;
        ReadyToPull = false;
        ProfileVendorPath = null;
        ProfileGrindPath = null;
        ProfileGhostPath = null;
        CurrentPath = null;
        
        ProfileGrindPath = new PathManager(CurrentProfile.Hotspots, true);
        ProfileGrindPath.Reset(true); // TODO: Add into constructor

        /*
        if(CurrentProfile.GhostHotspots.Length > 0)
        {
            ProfileGhostPath = new PathManager(CurrentProfile.GhostHotspots);
        }
        */
        
        if(CurrentProfile.VendorHotspots.Length > 0)
        {
            ProfileVendorPath = new PathManager(CurrentProfile.VendorHotspots);
        }

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
    public List<WoWUnit> NpcAttackers {get{return UnitInfo.Instance.NpcAttackers;}}
    public WoWUnit Target {get{return ObjectManager.Instance.Target;}}
    public LocalPlayer Local {get{return ObjectManager.Instance.Player;}}
    public LocalPet LocalPet {get{return ObjectManager.Instance.Pet;}}
    public string Author { get { return "Blacknight"; } }
    public string Name { get { return "SimpleGrinder"; } }
    public int Version { get { return 1; } }
}