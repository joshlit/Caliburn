using System;
using Crystal;
using DOL.GS;


public class WarriorFollowAction : ActionBase<CompanionContextBase> {
    public static readonly string Name = "WarriorFollow";

    public override IAction Clone() {
        return new WarriorFollowAction(this);
    }

    protected override void OnExecute(CompanionContextBase context) {
        context.Report(Name);
        if (context.Body is GameNPC npc)
        {
            npc.Follow(context.Target, 50, 10000);
            context.MinDistance = 100 * (1f - (float)context.Body.GetDistanceTo(context.NearestLiving)/context.DISTANCE_TO_CHECK);
            Console.WriteLine($"CrystalAI {context.Body?.Name} following {npc.TargetObject?.Name}! | Distance: {context.MinDistance}");
        }
        
        EndInSuccess(context);
        
    }
    

    public WarriorFollowAction() {
    }

    WarriorFollowAction(WarriorFollowAction other) : base(other) {
    }

    public WarriorFollowAction(IActionCollection collection) : base(Name, collection) {
    }
}