using Crystal;
using CrystalQuickStart;

namespace DOL.GS;

public class WarriorConstructor : AiConstructor
{
    protected override void DefineActions()
    {
        A = new FollowAction(Actions);
        A = new WarriorIdleAction(Actions);
    }

    protected override void DefineConsiderations()
    {
        C = new DistanceConsideration(Considerations);
    }

    protected override void DefineOptions() {
        O = new Option("Follow", Options);
        IsOkay(O.SetAction(FollowAction.Name));
        IsOkay(O.AddConsideration(DistanceConsideration.Name));

        O = new ConstantUtilityOption("WarriorIdle", Options);
        IsOkay(O.SetAction(WarriorIdleAction.Name));
        O.DefaultUtility = new Utility(0.01f, 1f);
    }

    protected override void DefineBehaviours() {
        B = new Crystal.Behaviour("DefaultBehaviour", Behaviours);
        IsOkay(B.AddOption("Follow"));
        IsOkay(B.AddOption("WarriorIdle"));
    }

    protected override void ConfigureAi() {
        Ai = new UtilityAi("WarriorAI", AIs);
        IsOkay(Ai.AddBehaviour("DefaultBehaviour"));
    }

    public WarriorConstructor() : base(AiCollectionConstructor.Create())
    {
    }
}