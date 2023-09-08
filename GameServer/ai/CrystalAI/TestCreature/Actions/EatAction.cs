using System;
using Crystal;
using DOL.GS;


public class EatAction : ActionBase<TestContext>
{
    public static readonly string Name = "Eat";

    public override IAction Clone()
    {
        return new EatAction(this);
    }

    protected override void OnExecute(TestContext context)
    {
        context.Report(Name);
        context.Hunger -= 80f;
        EndInSuccess(context);
        Console.WriteLine($"CrystalAI {context.Name} ate some food! | Hunger: {context.Hunger} Thirst: {context.Thirst}");
    }

    public EatAction()
    {
    }

    EatAction(EatAction other) : base(other)
    {
    }

    public EatAction(IActionCollection collection) : base(Name, collection)
    {
    }
}