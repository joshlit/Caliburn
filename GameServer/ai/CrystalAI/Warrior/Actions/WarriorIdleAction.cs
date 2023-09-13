using System;
using Crystal;
using DOL.GS;


public class WarriorIdleAction : WarriorActionBase {
        public static readonly string Name = "WarriorIdle";
        
        public override IAction Clone() {
            return new WarriorIdleAction();
        }

        protected override void OnExecute(WarriorContext context) {
            context.Report(Name);

            if (context.PriorityDefensiveTarget == null || CanSwitchDefensiveTarget(context))
            {
                //find priority defensive target if cooldown is up
                context.PriorityDefensiveTarget = DeterminePriorityDefensiveTarget(context);
            }
            context.DistanceFromOwner = 100 * (1f - (float)context.Body.GetDistanceTo(context.PlayerOwner)/context.DISTANCE_TO_CHECK);
            
            if (context.Body is Companion npc)
            {
                npc.Follow(context.PriorityDefensiveTarget, 50, 10000);
            }
            
            EndInSuccess(context);
        }

        public WarriorIdleAction() {
        }

        public WarriorIdleAction(IActionCollection collection) : base(Name, collection) {
        }
    }
