using Crystal;

namespace DOL.GS;

public class AttackerConsideration : ConsiderationBase<CompanionContextBase>
{
    private IEvaluator _evaluator;
    public static readonly string Name = "AttackerConsideration";
    
    public override void Consider(CompanionContextBase context)
    {
        Utility = new Utility(_evaluator.Evaluate(context.MinDistance), Weight);
    }

    public override IConsideration Clone()
    {
        return new AttackerConsideration(this);
    }
    
    public AttackerConsideration() {
        Initialize();
    }

    // A copy constructor must be present in every consideration.
    AttackerConsideration(AttackerConsideration other) : base(other) {
        Initialize();
    }

    public AttackerConsideration(IConsiderationCollection collection)
        : base(Name, collection) {
        Initialize();
    }

    void Initialize() {
        // Point "a" in the interactive plots below.
        var ptA = new Pointf(0f, 0f);
        // Point "b" in the plots below.
        var ptB = new Pointf(100f, 1f);
        
        _evaluator = new PowerEvaluator(ptA, ptB, 10f);
    }
}