using ZzukBot.Game.Statics;
using ZzukBot.Objects;

public class Path
{
    private bool ReverseTravesale;
    private Location[] Waypoints;
    private int CurrentWaypointIndex;
    public Location Current;

    public bool Repeat;
    public bool RepeatCircleAround;
    
    public Path(Location[] Waypoints, bool Repeat, bool RepeatCircleAround)
    {
        this.Waypoints = Waypoints;
        this.Current = Waypoints[0];
        this.Repeat = Repeat;
        this.RepeatCircleAround = RepeatCircleAround;
    }

    int GetNearestWaypointIndex()
    {
        int BestIndex = -1;
        float BestDistance = float.MaxValue;

        for(int WaypointIndex = 0; WaypointIndex < Waypoints.Length; ++WaypointIndex)
        {
            Location Waypoint = Waypoints[WaypointIndex];

            float Distance = Local.Position.GetDistanceTo(Waypoint);

            if(Distance < BestDistance)
            {
                BestIndex = WaypointIndex;
                BestDistance = Distance;
            }
        }

        return BestIndex;
    }

    public void Reset(bool NearestWaypoint=false)
    {
        ReverseTravesale = false;

        if(NearestWaypoint)
        {
            CurrentWaypointIndex = GetNearestWaypointIndex();
        }
        else
        {
            CurrentWaypointIndex = 0;
        }

        this.Current = Waypoints[CurrentWaypointIndex];
    }

    public bool HasNext()
    {
        return Repeat || CurrentWaypointIndex < Waypoints.Length;
    }

    public Location Next()
    {
        Current = Waypoints[CurrentWaypointIndex];

        if(ReverseTravesale)
        {
            --CurrentWaypointIndex;
        }
        else
        {
            ++CurrentWaypointIndex;
        }

        if(Repeat && (CurrentWaypointIndex == Waypoints.Length || CurrentWaypointIndex < 0))
        {
            if(RepeatCircleAround)
            {
                CurrentWaypointIndex = 0;
            }
            else
            {
                ReverseTravesale = !ReverseTravesale;
                CurrentWaypointIndex = ReverseTravesale ? Waypoints.Length - 1 : 0;
            }
        }

        return Current;
    }

    public LocalPlayer Local {get{return ObjectManager.Instance.Player;}}
}
