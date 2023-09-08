using Crystal;
using CrystalQuickStart;

namespace DOL.GS;

public class TestGuyConstructor : AiConstructor
{
    protected override void DefineActions()
    {
        A = new FleeAction(Actions);
        A = new IdleAction(Actions);
    }

    protected override void DefineConsiderations()
    {
        C = new DistanceConsideration(Considerations);
    }

    protected override void DefineOptions() {
        O = new Option("Flee", Options);
        IsOkay(O.SetAction(FleeAction.Name));
        IsOkay(O.AddConsideration(DistanceConsideration.Name));

        O = new ConstantUtilityOption("Idle", Options);
        IsOkay(O.SetAction(IdleAction.Name));
        O.DefaultUtility = new Utility(0.01f, 1f);
    }

    protected override void DefineBehaviours() {
        B = new Crystal.Behaviour("DefaultBehaviour", Behaviours);
        IsOkay(B.AddOption("Flee"));
        IsOkay(B.AddOption("Idle"));
    }

    protected override void ConfigureAi() {
        Ai = new UtilityAi("TestGuyAI", AIs);
        IsOkay(Ai.AddBehaviour("DefaultBehaviour"));
    }

    public TestGuyConstructor() : base(AiCollectionConstructor.Create())
    {
    }
}