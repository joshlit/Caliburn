using System;
using Crystal;
using DOL.GS;


public class FleeAction : ActionBase<TestContext> {
    public static readonly string Name = "Flee";

    public override IAction Clone() {
        return new FleeAction(this);
    }

    protected override void OnExecute(TestContext context) {
        context.Report(Name);
        var fleeTarget = context.NearestPlayer;
        var owner = context.Owner;
        CalculateFleeTarget(fleeTarget, owner);
        context.MinDistance = 100 * (1f - (float)context.Owner.GetDistanceTo(context.NearestPlayer)/context.DISTANCE_TO_CHECK);
        EndInSuccess(context);
        Console.WriteLine($"CrystalAI {owner.Name} running from {fleeTarget.Name}! | Distance: {context.MinDistance}");
    }
    
    protected static void CalculateFleeTarget(GameLiving fleeTarget, GameLiving owner)
    {
        if (owner is not GameNPC npc)
            return;
        
        ushort TargetAngle = (ushort)((npc.GetHeading(fleeTarget) + 2048) % 4096);

        Point2D fleePoint = owner.GetPointFromHeading(TargetAngle, 300);
        npc.StopFollowing();
        npc.StopAttack();
        npc.WalkTo(new Point3D(fleePoint.X, fleePoint.Y, npc.Z), npc.MaxSpeed);
    }

    public FleeAction() {
    }

    FleeAction(FleeAction other) : base(other) {
    }

    public FleeAction(IActionCollection collection) : base(Name, collection) {
    }
}