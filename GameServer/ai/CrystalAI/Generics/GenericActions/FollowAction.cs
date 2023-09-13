using System;
using Crystal;
using DOL.GS;


public class FollowAction : ActionBase<CompanionContextBase> {
    public static readonly string Name = "Follow";

    public override IAction Clone() {
        return new FollowAction(this);
    }

    protected override void OnExecute(CompanionContextBase context) {
        context.Report(Name);
        if (context.EnemyTarget == null) context.EnemyTarget = context.NearestLiving;
        if (context.Body is GameNPC npc)
        {
            npc.Follow(context.EnemyTarget, 50, 10000);
            context.MinDistance = 100 * (1f - (float)context.Body.GetDistanceTo(context.NearestLiving)/context.DISTANCE_TO_CHECK);
            Console.WriteLine($"CrystalAI {context.Body?.Name} following {context.EnemyTarget?.Name}! | Distance: {context.MinDistance} Owner: {context.PlayerOwner?.Name}");
        }
        
        EndInSuccess(context);
        
    }
    

    public FollowAction() {
    }

    FollowAction(FollowAction other) : base(other) {
    }

    public FollowAction(IActionCollection collection) : base(Name, collection) {
    }
}