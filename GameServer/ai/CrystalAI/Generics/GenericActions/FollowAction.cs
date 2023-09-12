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
        if (context.Target == null) context.Target = context.NearestLiving;
        if (context.Body is GameNPC npc)
        {
            npc.Follow(context.Target, 50, 10000);
            context.MinDistance = 100 * (1f - (float)context.Body.GetDistanceTo(context.NearestLiving)/context.DISTANCE_TO_CHECK);
            Console.WriteLine($"CrystalAI {context.Body?.Name} following {context.Target?.Name}! | Distance: {context.MinDistance} Owner: {context.PlayerOwner?.Name}");
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