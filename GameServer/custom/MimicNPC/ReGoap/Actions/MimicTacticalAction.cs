using System;
using DOL.GS.ReGoap.Core;
using DOL.GS.ReGoap.Mimic.Sensors;
using DOL.GS.Scripts.ReGoap;
using DOL.AI.Brain;

namespace DOL.GS.ReGoap.Mimic.Actions
{
    /// <summary>Uses the live class-aware mechanics instead of maintaining a second
    /// copy of spell selection, power/cooldown checks, styles and healing coordination.</summary>
    public sealed class MimicTacticalAction : ReGoapAction<string, object>
    {
        private readonly Func<bool> _execute;
        private readonly float _cost;

        private MimicTacticalAction(string actionName, string requirement, string effect,
            Func<bool> execute, float cost = 1f)
        {
            name = actionName;
            preconditions.Set(requirement, true);
            effects.Set(effect, true);
            _execute = execute;
            _cost = cost;
        }

        public static void Register(MimicReGoapAgent agent)
        {
            var brain = agent.Brain;
            agent.AddAction(new MimicTacticalAction("HealOrCure", TacticalSensor.SupportAvailable,
                "targetHealed", () => brain.CheckHeals()));
            agent.AddAction(new MimicTacticalAction("MaintainBuffs", TacticalSensor.BuffsAvailable,
                MimicWorldStateKeys.BUFFS_MAINTAINED,
                () => brain.ExecuteGoapSpell(MimicBrain.eCheckSpellType.Defensive)));
            agent.AddAction(new MimicTacticalAction("ControlAdd", TacticalSensor.ControlAvailable,
                MimicWorldStateKeys.ADDS_CONTROLLED,
                () => brain.ExecuteGoapSpell(MimicBrain.eCheckSpellType.CrowdControl)));
            agent.AddAction(new MimicTacticalAction("CastOffensiveSpell", TacticalSensor.OffenseAvailable,
                "targetDamaged", () => brain.ExecuteGoapSpell(MimicBrain.eCheckSpellType.Offensive)));
            agent.AddAction(new MimicTacticalAction("EngageTarget", TacticalSensor.AttackAvailable,
                "targetDamaged", () => brain.ExecuteGoapEngagement(), 2f));
        }

        public override float GetCost(IReGoapAgent<string, object> agent, ReGoapState<string, object> state)
            => _cost;

        public override bool Run(IReGoapAgent<string, object> agent,
            Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail)
        {
            if (_execute()) done(this);
            else fail(this);
            return true;
        }
    }
}
