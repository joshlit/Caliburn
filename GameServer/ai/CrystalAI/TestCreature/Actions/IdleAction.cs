using System;
using Crystal;
using DOL.GS;


public class IdleAction : ActionBase<TestContext> {
        public static readonly string Name = "Idle";
    
        
        public override IAction Clone() {
            return new IdleAction(this);
        }

        protected override void OnExecute(TestContext context) {
            context.Report(Name);
            context.NearestPlayer = GetNearestLiving(context);
            context.MinDistance = 100 * (1f - (float)context.Owner.GetDistanceTo(context.NearestPlayer)/context.DISTANCE_TO_CHECK);
            EndInSuccess(context);
            Console.WriteLine($"{Name} OnExecute | Nearest Player {context.NearestPlayer?.Name} | Distance: {context.MinDistance}");
        }

        private GameLiving GetNearestLiving(TestContext context)
        {
            
            int dist = -1;
            GameLiving nearest = null;
            foreach (var player in context.Owner.GetPlayersInRadius((ushort)context.DISTANCE_TO_CHECK))
            {
                var distTo = context.Owner.GetDistanceTo(player);
                if (dist == -1 || distTo < dist)
                {
                    dist = distTo;
                    nearest = player;
                }
            }

            return nearest;
        }

        public IdleAction() {
        }

        IdleAction(IdleAction other) : base(other) {
        }

        public IdleAction(IActionCollection collection) : base(Name, collection) {
        }
    }
