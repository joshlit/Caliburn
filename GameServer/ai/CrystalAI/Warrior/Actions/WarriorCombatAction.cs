using System;
using Crystal;
using DOL.GS;

public class WarriorCombatAction : WarriorActionBase
{
    private IEvaluator _evaluator;
    public static readonly string Name = "WarriorCombat";

    public WarriorCombatAction() {
    }

    public WarriorCombatAction(IActionCollection collection) : base(Name, collection) {
    }
    
    protected override void OnExecute(WarriorContext warrior) {
        warrior.Report(Name);

        if (warrior.PriorityDefensiveTarget == null || CanSwitchDefensiveTarget(warrior))
        {
            //find priority defensive target if cooldown is up
            warrior.PriorityDefensiveTarget = DeterminePriorityDefensiveTarget(warrior);
        }
        
        warrior.DistanceFromDefensiveTarget = 100 * (1f - (float)warrior.Body.GetDistanceTo(warrior.PriorityDefensiveTarget)/warrior.DISTANCE_TO_CHECK);
        
        /*
        if (warrior.Body is GameNPC npc)
        {
            npc.Follow(warrior.PriorityDefensiveTarget, 50, 10000);
        }*/
            
        EndInSuccess(warrior);
    }
}