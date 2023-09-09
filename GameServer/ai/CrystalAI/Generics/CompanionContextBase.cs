using Crystal;

namespace DOL.GS;

public class CompanionContextBase : IContext
{
    public string Name;
    public int DISTANCE_TO_CHECK = 1000;
    
    public GameLiving Body;

    private GamePlayer _playerOwner;
    public GamePlayer PlayerOwner
    {
        get { return _playerOwner; }
        set { _playerOwner= value; }
    }

    private float _distanceFromOwner;
    public float DistanceFromOwner
    {
        get { return _distanceFromOwner; }
        set { _distanceFromOwner = value.Clamp(0f, 100f); }
    }

    private float _minDistance;
    public float MinDistance
    {
        get { return _minDistance; }
        set { _minDistance = value.Clamp(0f, 100f); }
    }

    public GameLiving NearestLiving;
    public GameLiving Target;
    
    string _lastAction;
    public void Report(string what) {
        _lastAction = what;
    }

    public CompanionContextBase(GameLiving body) {
        _minDistance = 0f;
        Body = body;
    }
}