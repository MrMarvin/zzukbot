using ZzukBot.Game.Statics;
using ZzukBot.Objects;

public class PathManager
{
    public Location CurrentDest;
    public Path HotspotPath;
    public Path CurrentPath;
    
    public PathManager(Location[] Hotspots, bool Repeat=false, bool RepeatCircleAround=false)
    {
        HotspotPath = new Path(Hotspots, Repeat, RepeatCircleAround);
    }

    public float DistanceToDestination {get{return CurrentPath.GetFinalDestination().GetDistanceTo(Local.Position);}}

    public void Reset(bool NearestWaypoint=false)
    {
        HotspotPath.Reset(NearestWaypoint);
        CalculatePath();
    }
    
    public void CalculatePathToTarget(WoWUnit target)
    {
        Util.DebugMsg("Calculating Path to target: " + target.Name);
        Util.DebugMsg("target is at: " + target.Position.X + " " + target.Position.Y + " " + target.Position.Z);
        CalculatePath(target.Position);
    }

    public void CalculatePath(Location targetLocation = null)
    {
        if (targetLocation == null)
        {
            targetLocation = HotspotPath.Current;
        }
        Location[] Path = Navigation.Instance.CalculatePath(Local.MapId, Local.Position, targetLocation, true);
        Util.DebugMsg("Got " + Path.Length + " waypoints to Location");
        Util.DebugMsg("CtmTo: " + Path[1].X + " " + Path[1].Y + " " + Path[1].Z);
        Util.DebugMsg("Local: " + Local.Position.X + " " + Local.Position.Y + " " + Local.Position.Z);
        CurrentPath = new Path(Path, false, false);
    }

    public bool ReachedWaypoint()
    {
        return Local.IsCtmIdle || (CurrentDest == null/* || CurrentDest.GetDistanceTo(Local.Position) < 5.0f*/);
    }

    public bool HasNext()
    {
        if(CurrentPath == null)
            CalculatePath();

        return HotspotPath.HasNext() || CurrentPath.HasNext();
    }
    
    public Location Next()
    {
        if(CurrentPath == null)
            CalculatePath();

        if(!CurrentPath.HasNext())
        {
            Util.DebugMsg("CurrentPath is empty, falling back to Hotspots");
            if (!HotspotPath.HasNext())
            {
                // NOTE: We have already reached our final destination.
                // HasNext should have returned false.
                return (CurrentDest = null);
            }
            else
            {
                Location NextHotspot = HotspotPath.Next();
                CalculatePath();
            }
        }
        
        CurrentDest = CurrentPath.Next();
        return CurrentDest;
    }

    public LocalPlayer Local {get{return ObjectManager.Instance.Player;}}
}
