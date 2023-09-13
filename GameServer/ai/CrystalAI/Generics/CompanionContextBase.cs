using Crystal;
using DOL.AI.Brain;

namespace DOL.GS;

public class CompanionContextBase : IContext
{
    public string Name;
    public int DISTANCE_TO_CHECK = 250;
    
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
        set { _distanceFromOwner = value; }
    }

    private float _minDistance;
    public float MinDistance
    {
        get { return _minDistance; }
        set { _minDistance = value; }
    }

    public GameLiving NearestLiving;
    public GameLiving EnemyTarget;
    
    private float _distanceFromEnemyTarget;
    public float DistanceFromEnemyTarget
    {
        get { return _distanceFromEnemyTarget; }
        set { _distanceFromEnemyTarget = value; }
    }

    public bool OwnerInCombat => PlayerOwner is {InCombat: true};
    
    string _lastAction;
    public void Report(string what) {
        _lastAction = what;
    }

    public CompanionContextBase(GameLiving body) {
        _minDistance = 0f;
        Body = body;
    }

    public void CheckDefensiveSpells()
    {
        if (Body is not Companion {Brain: CrystalBrain cb}) return;
    }
}