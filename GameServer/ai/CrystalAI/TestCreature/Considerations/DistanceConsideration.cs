using Crystal;

namespace DOL.GS;

public class DistanceConsideration : ConsiderationBase<TestContext>
{
    private IEvaluator _evaluator;
    public static readonly string Name = "DistanceConsideration";
    
    public override void Consider(TestContext context)
    {
        Utility = new Utility(_evaluator.Evaluate(context.MinDistance), Weight);
    }

    public override IConsideration Clone()
    {
        return new DistanceConsideration(this);
    }
    
    public DistanceConsideration() {
        Initialize();
    }

    // A copy constructor must be present in every consideration.
    DistanceConsideration(DistanceConsideration other) : base(other) {
        Initialize();
    }

    public DistanceConsideration(IConsiderationCollection collection)
        : base(Name, collection) {
        Initialize();
    }

    void Initialize() {
        // Point "a" in the interactive plots below.
        var ptA = new Pointf(0f, 0f);
        // Point "b" in the plots below.
        var ptB = new Pointf(100f, 1f);
        // This says that as the value of the Bladder property approaches 100, it 
        // becomes increasingly more important to do something about it. If this 
        // was a LinearEvaluator, that would ignore the sense of urgency, that is
        // quite familiar to everyone with a bladder, to take action when their 
        // bladder is nearly full.
        _evaluator = new PowerEvaluator(ptA, ptB, 10f);
    }
}