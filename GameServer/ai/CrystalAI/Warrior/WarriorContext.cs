using Crystal;
using DOL.GS;

public class WarriorContext : CompanionContextBase
{
    public WarriorContext(GameLiving body) : base(body)
    {
    }

    private GameLiving _priorityDefensiveTarget;
    public GameLiving PriorityDefensiveTarget
    {
        get => _priorityDefensiveTarget;
        set
        {
            if (value != _priorityDefensiveTarget)
                LastTargetChangeTick = GameLoop.GameLoopTime;
            _priorityDefensiveTarget = value;
        }
    }
    
    public float DistanceFromDefensiveTarget { get; set; }

    public float LastTargetChangeTick { get; set; } = -1f;
    public int MINIMUM_TARGET_PROTECTION_TIME = 10; //in seconds
}