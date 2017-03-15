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

    public void Reset(bool NearestWaypoint=false)
    {
        HotspotPath.Reset(NearestWaypoint);
        CalculatePath();
    }
    
    public void CalculatePath()
    {
        Location[] Path = Navigation.Instance.CalculatePath(Local.MapId, Local.Position, HotspotPath.Current, true);
        CurrentPath = new Path(Path, false, false);
    }

    public bool ReachedWaypoint()
    {
        return Local.IsCtmIdle/* && (CurrentDest == null || CurrentDest.GetDistanceTo(Local.Position) < ???)*/;
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
            if(!HotspotPath.HasNext())
            {
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