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
using ZzukBot.Game.Frames;
using System.Windows.Forms;

[Export(typeof(IBotBase))]
public class SimpleGrinder : IBotBase
{
    public static Profile CurrentProfile;
    public MainForm GUI;
    public ClassId LoadedClass;
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
            if(Npc.IsMob && Npc.IsUntouched && !Npc.IsKilledAndLooted && Npc.Health == Npc.MaxHealth && CurrentProfile.FactionsContains(Npc.FactionId))
            {
                float Distance = Npc.DistanceToPlayer;
                // Applying z axis bias here, so the bot will more likely target mobs on the same level of a building or whereever
                Distance = Distance + Math.Abs(Local.Position.Z - Npc.Position.Z) * 2;
                if(Distance < BestDistance)
                {
                    BestUnit = Npc;
                    BestDistance = Distance;
                }
            }
        }

        return BestUnit;
    }

    public WoWUnit GetNpcByName(string Name)
    {
        WoWUnit Result = null;

        foreach(WoWUnit Npc in ObjectManager.Instance.Npcs)
        {
            if(Npc.Name == Name)
            {
                Result = Npc;
                break;
            }
        }

        return Result;
    }

 public WoWUnit GetClosestSkinnable(float Radius)
    {
        WoWUnit BestUnit = null;
        float BestDistance = Radius;

        foreach(WoWUnit Npc in UnitInfo.Instance.Skinable)
        {
            float Distance = Npc.DistanceToPlayer;

            if(Distance<BestDistance)
            {
                BestUnit = Npc;
                BestDistance = Distance;
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
            Util.DebugMsg("Using CustomClass: [" + CC.Author + " " + CC.Name + " " + CC.Class.ToString() + "]");
            if (CC.Class == LocalClass)
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

        Local.AntiAfk();
        if(LoadedClass != Local.Class)
        {
            if(LoadCustomClass(Local.Class))
            {
                LoadedClass = Local.Class;
                Util.DebugMsg("Using CustomClass: [" + CurrentCC.Author + " " + CurrentCC.Name + " " + CurrentCC.Class.ToString() + "]");
            }
            else
            {
                Util.DebugMsg("Failed to find a CustomClass that matched requirements.");
                return 100;
            }
        }

        if (Local.IsDead)
        {
            TickDead();
        } else if (Local.InGhostForm)
        {
            TickGhostForm();
        } else if (CurrentProfile.RepairNpcPosition != null && Inventory.Instance.CountFreeSlots(false) <= 3)
        {
            return TickVendoring();
        } else
        {
            TickLiving();
        }
        return 100;
    }

    private int TickVendoring()
    {
        WoWUnit Vendor = null;
        if (MerchantFrame.IsOpen && MerchantFrame.Instance.CanRepair)
        {
                MerchantFrame.Instance.RepairAll();
        } else if (CurrentProfile.RepairNpcPosition.GetDistanceTo(Local.Position) <= 3)
        {
            Vendor = GetNpcByName(CurrentProfile.RepairNpcName);
        }

        if (Vendor != null)
        {
            Vendor.Interact(false);
            return 500;
        }

        Util.DebugMsg("Generate path to vendor: " + CurrentProfile.RepairNpcPosition.GetDistanceTo(Local.Position));
        ProfileVendorPath = new PathManager(new Location[] { CurrentProfile.RepairNpcPosition });
        return 100;
    }

    private void TickGhostForm()
    {
        RecalculatePath = true;

        if (WasAlive)
        {
            WasAlive = false;

            if (UserSetGhostPath)
            {
                ProfileGhostPath.Reset(true);
            }
            else
            {
                ProfileGhostPath = new PathManager(new Location[] { Local.CorpsePosition });
            }
        }

        if (ProfileGhostPath.ReachedWaypoint())
        {
            if (ProfileGhostPath.HasNext())
            {
                Location Next = ProfileGhostPath.Next();
                if (Next != null)
                {
                    Util.DebugMsg("next CTM " + ProfileGhostPath.CurrentPath.Remaining() + " to corpse: " + Next.X + " " + Next.Y + " " + Next.Z);
                    Local.CtmTo(Next);
                }
            }
            else
            {
                // TODO: Reached destination!
                if (Local.TimeUntilResurrect == 0)
                {
                    Lua.Instance.Execute("RetrieveCorpse();");
                    Util.DebugMsg("RetrieveCorpse();");
                }
            }
        }
        else
        {
            Local.CtmTo(ProfileGhostPath.CurrentDest);
            Util.DebugMsg("CTM current" + ": " + ProfileGhostPath.CurrentDest.X + " " + ProfileGhostPath.CurrentDest.Y + " " + ProfileGhostPath.CurrentDest.Z);
        }
    }

    private void TickLiving()
    {
        WasAlive = true;

        if (Local.IsInCombat && NpcAttackers.Count > 0)
        {
            TickCombat();
            return;
        }

        if (ReadyToPull)
        {
            if (HasNoTarget())
            {
                Util.DebugMsg("No current Target, aquiring one...");
                WoWUnit BestUnit = GetClosestMob(Settings.Instance.SearchMobRange);

                if (BestUnit != null)
                {
                    Local.SetTarget(BestUnit);
                }
            }

            if (Target != null)
            {
                TickLivingTarget();
            }
            else
            {
                TickLivingHotSpot();
            }
        }
        // NOTE: Stand idle if we have rez sickness unless attacked.
        if (Local.GotDebuff("Resurrection Sickness"))
            return;

        else
        {
            WoWUnit LootUnit = GetClosestLootable(Settings.Instance.SearchMobRange);
            if (LootUnit != null)
            {
                TickLivingLoot();
            }
            else
            {
                TickLivingRest();
            }
        }
    }

    private void TickLivingRest()
    {
        if (Local.HealthPercent < Settings.Instance.EatAt ||
                   (Util.IsManaClass(Local.Class) && Local.ManaPercent < Settings.Instance.DrinkAt))
        {
            if (!Local.IsEating || !Local.IsDrinking)
            {
                CurrentCC.OnRest();

                bool ForceDrink = false;
                if (Settings.Instance.Food != null &&
                    !Local.IsEating &&
                    Local.HealthPercent < Settings.Instance.EatAt)
                {
                    Local.Eat(Settings.Instance.Food);
                    ForceDrink = Settings.Instance.AlwaysDrinkWhenEating;
                }

                if (Settings.Instance.Drink != null &&
                    !Local.IsDrinking &&
                    (ForceDrink || Local.ManaPercent < Settings.Instance.DrinkAt) &&
                    Util.IsManaClass(Local.Class))
                {
                    Local.Drink(Settings.Instance.Drink);
                }
            }
        }
        else if (CurrentCC.OnBuff())
        {
            ReadyToPull = true;
        }
    }

    private void TickLivingLoot()
    {
        Util.DebugMsg("TickLivingLoot");
        WoWUnit LootUnit = GetClosestLootable(Settings.Instance.SearchMobRange);
        if (LootUnit.DistanceToPlayer <= 3)
        {
            CurrentTargetPath = null;
            Local.CtmStopMovement();
            LootUnit.Interact(true);
            return;
        }

        if (CurrentTargetPath == null)
        {
            ProfileGrindPath.CalculatePathToTarget(LootUnit);
            CurrentTargetPath = ProfileGrindPath.CurrentPath;
            RecalculatePath = true;
        }
        if (Local.IsCtmIdle)
        {
            Location Next = ProfileGrindPath.Next();
            if (Next != null)
            {
                Util.DebugMsg("next CTM " + CurrentTargetPath.Remaining() + " to LootUnit: " + Next.X + " " + Next.Y + " " + Next.Z);
                Local.CtmTo(Next);
            }
            else
            {
                Local.CtmTo(LootUnit.Position);
            }
        }
    }

    private void TickLivingHotSpot()
    {
        if (RecalculatePath)
        {
            ProfileGrindPath.CalculatePath();
            RecalculatePath = false;
        }

        if (ProfileGrindPath.ReachedWaypoint(2.0f))
        {
            Location Next = ProfileGrindPath.Next();
            if (Next != null)
            {
                Util.DebugMsg("CtmTo to next Hostspot: " + Next.X + " " + Next.Y + " " + Next.Z);
                Local.CtmTo(Next);
            }
        }
    }

    private void TickLivingTarget()
    {
        if (IsTargetReachable())
        {
            Util.DebugMsg("In range for Combat, cancelling movement for now");
            CurrentTargetPath = null;
            Local.CtmStopMovement();
            Local.Face(Target);
            CurrentCC.OnPull();
        }
        else
        {
            ApproachTarget();
        }
    }

    private void ApproachTarget()
    {
        if (Local.InLosWith(Target))
        {
            Local.CtmTo(Target.Position);
            return;
        }

        if (TargetMovedFromItsLocation(1.0f))
        {
            Util.DebugMsg("Target moved from where we last calculated waypoints to, resetting waypoints");
            CurrentTargetPath = null;
        }

        if (CurrentTargetPath == null)
        {
            Util.DebugMsg("no path to current target yet, calculating one");
            ProfileGrindPath.CalculatePathToTarget(Target);
            CurrentTargetPath = ProfileGrindPath.CurrentPath;
            RecalculatePath = true;
        }

        if (ProfileGrindPath.ReachedWaypoint(1.0f))
        {
            Location Next = CurrentTargetPath.Next();
            if (Next == null)
            {
                Util.DebugMsg("reached end of CTM path to target but not quite there yet... recalculating new path on next tick.");
                CurrentTargetPath = null;
                return;
            }

            Util.DebugMsg("next CTM " + CurrentTargetPath.Remaining() + " to target: " + Next.X + " " + Next.Y + " " + Next.Z);
            Local.CtmTo(Next);
        }
    }

    private bool TargetMovedFromItsLocation(float radius = 3.0f)
    {
        if (Target == null || CurrentTargetPath == null)
            return false;
        return Target.Position.GetDistanceTo(CurrentTargetPath.GetFinalDestination()) >= radius;
    }

    private bool IsTargetReachable()
    {
        // Applying a z-axis bias here to level out mobs above/below the Player that cant really be targetted
        return (Target.DistanceToPlayer + Math.Abs(Local.Position.Z - Target.Position.Z)) <= CurrentCC.CombatDistance
            && Local.InLosWith(Target.Position);
    }

    private bool HasNoTarget()
    {
        return (Target == null || Target.Health < 1 || (Target.IsInCombat && !Target.IsFleeing && Target.TargetGuid != Local.Guid && (!Local.HasPet || Target.TargetGuid != LocalPet.Guid)));
    }

    private void TickCombat()
    {
        RecalculatePath = true;
        ReadyToPull = false;

        if (Target == null || Target.Health <= 0)
        {
            WoWUnit BestUnit = GetBestAttacker();

            if (BestUnit != null)
            {
                Local.SetTarget(BestUnit);
            }
        }

        if (Target != null)
        {
            Local.Face(Target);

            if (Target.DistanceToPlayer <= CurrentCC.CombatDistance)
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

    private void TickDead()
    {
        Lua.Instance.Execute("RepopMe();");
        Util.DebugMsg("RepopMe();");
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

                    if (Time != -1)
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
            catch (Exception Ex)
            {
                Util.DebugMsg(Ex.ToString());
            }
        }
    }

    public bool Start(Action StopCallback)
    {
        if (!ObjectManager.Instance.IsIngame)
        {
            MessageBox.Show("Must be in game to start SimpleGrinder.");
            return false;
        }

        if (this.StopCallback != null)
        {
            Util.DebugMsg("StopCallback was not null.");
            return false;
        }
        if (Settings.Instance.ProfileFilePath == null)
        {
            MessageBox.Show("Must load a profile to start SimpleGrinder.");
            return false;
        }

        CurrentProfile = Profile.ParseV1Profile(Settings.Instance.ProfileFilePath);
        if (CurrentProfile == null)
        {
            MessageBox.Show("Unable to load selected profile.");
            Settings.Instance.ProfileFilePath = null;
            Settings.SaveSettings();
            GUI.ProfileNameLabel.Text = "";
            return false;
        }

        if (CurrentProfile.Hotspots != null)
        {
            Util.DebugMsg("Parsed " + CurrentProfile.Hotspots.Length + " hotspot(s).");
        }
        if (CurrentProfile.VendorHotspots != null)
        {
            Util.DebugMsg("Parsed " + CurrentProfile.VendorHotspots.Length + " vendor hotspot(s).");
        }
        if (CurrentProfile.GhostHotspots != null)
        {
            Util.DebugMsg("Parsed " + CurrentProfile.GhostHotspots.Length + " ghost hotspot(s).");
        }


        if (LoadCustomClass(Local.Class))
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
        ProfileGrindPath = new PathManager(CurrentProfile.Hotspots, true);
        ProfileGrindPath.Reset(true); // TODO: Add into constructor
        DirectX.Instance.OnEndSceneExecution += Hook_EndScene;
        Local.EnableCtm();
        Spell.UpdateSpellbook();
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
    public Path CurrentTargetPath = null;
    public LocalPlayer Local {get{return ObjectManager.Instance.Player;}}
    public LocalPet LocalPet {get{return ObjectManager.Instance.Pet;}}
    public string Author { get { return "Blacknight"; } }
    public string Name { get { return "SimpleGrinder"; } }
    public int Version { get { return 1; } }
}
