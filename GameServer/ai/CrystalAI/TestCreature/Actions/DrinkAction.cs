using System;
using Crystal;
using DOL.GS;
public class DrinkAction : ActionBase<TestContext> {
    public static readonly string Name = "Drink";

    public override IAction Clone() {
        return new DrinkAction(this);
    }

    protected override void OnExecute(TestContext context) {
        context.Report(Name);
        context.Thirst -= 90f;
        EndInSuccess(context);
        Console.WriteLine($"CrystalAI {context.Name} got a drink! | Hunger: {context.Hunger} Thirst: {context.Thirst}");
    }

    public DrinkAction() {
    }

    DrinkAction(DrinkAction other) : base(other) {
    }

    public DrinkAction(IActionCollection collection) : base(Name, collection) {
    }
}
    