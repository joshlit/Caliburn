using System;
using Crystal;
using DOL.GS;


public class WarriorIdleAction : ActionBase<CompanionContextBase> {
        public static readonly string Name = "WarriorIdle";
    
        
        public override IAction Clone() {
            return new WarriorIdleAction(this);
        }

        protected override void OnExecute(CompanionContextBase context) {
            context.Report(Name);
            context.NearestLiving = GetNearestLiving(context);
            context.MinDistance = 100 * (1f - (float)context.Body.GetDistanceTo(context.NearestLiving)/context.DISTANCE_TO_CHECK);
            if(context.Body is Companion {RootOwner: null} companion) companion.Die(companion);
            EndInSuccess(context);
            Console.WriteLine($"{Name} OnExecute | Nearest Player {context.NearestLiving?.Name} | Distance: {context.MinDistance}");
        }

        private GameLiving GetNearestLiving(CompanionContextBase context)
        {
            
            int dist = -1;
            GameLiving nearest = null;
            foreach (var player in context.Body.GetPlayersInRadius((ushort)context.DISTANCE_TO_CHECK))
            {
                if (player == context.Body) continue;
                var distTo = context.Body.GetDistanceTo(player);
                if (dist == -1 || distTo < dist)
                {
                    dist = distTo;
                    nearest = player;
                }
            }
            
            foreach (var gameNpc in context.Body.GetNPCsInRadius((ushort)context.DISTANCE_TO_CHECK))
            {
                if (gameNpc == context.Body) continue;
                var distTo = context.Body.GetDistanceTo(gameNpc);
                if (dist == -1 || distTo < dist)
                {
                    dist = distTo;
                    nearest = gameNpc;
                }
            }

            return nearest;
        }

        public WarriorIdleAction() {
        }

        WarriorIdleAction(WarriorIdleAction other) : base(other) {
        }

        public WarriorIdleAction(IActionCollection collection) : base(Name, collection) {
        }
    }
