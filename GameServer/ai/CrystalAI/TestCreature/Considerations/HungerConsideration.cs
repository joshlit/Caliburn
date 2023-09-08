using Crystal;
using DOL.GS;

namespace CrystalQuickStart {


    public class HungerConsideration : ConsiderationBase<TestContext> {
        IEvaluator _evaluator;
        public static readonly string Name = "HungerConsideration";

        public override void Consider(TestContext context) {
            Utility = new Utility(_evaluator.Evaluate(context.Hunger), Weight);
        }

        public override IConsideration Clone() {
            return new HungerConsideration(this);
        }

        public HungerConsideration() {
            Initialize();
        }

        HungerConsideration(HungerConsideration other) : base(other) {
            Initialize();
        }

        public HungerConsideration(IConsiderationCollection collection)
            : base(Name, collection) {
            Initialize();
        }

        void Initialize() {
            var ptA = new Pointf(35f, 0f);
            var ptB = new Pointf(100f, 1f);
            _evaluator = new LinearEvaluator(ptA, ptB);
        }
    }


}