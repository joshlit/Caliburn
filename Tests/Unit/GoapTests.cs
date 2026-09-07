using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DOL.AI.Brain;
using DOL.GS;
using DOL.GS.ReGoap.Core;
using DOL.GS.ReGoap.Mimic;
using DOL.GS.ReGoap.Mimic.Sensors;
using DOL.GS.Scripts;
using DOL.GS.Scripts.ReGoap;
using NUnit.Framework;

namespace DOL.Tests.Unit
{
    [TestFixture]
    public class GoapTests
    {
        private static ReGoapState<string, object> State(params (string Key, object Value)[] values)
        {
            var state = new ReGoapState<string, object>();
            foreach (var value in values) state.Set(value.Key, value.Value);
            return state;
        }

        private sealed class Goal : ReGoapGoal<string, object>
        {
            public float Priority;
            public Goal(string effect, float priority)
            {
                goalState = State((effect, true));
                Priority = priority;
            }
            public override float GetPriority(ReGoapState<string, object> state) => Priority;
            public override ReGoapState<string, object> GetGoalState() => goalState;
        }

        private sealed class Step : ReGoapAction<string, object>
        {
            public int Runs;
            public bool Fail, Running, ReturnOnly, Available = true;
            public float Cost = 1;
            public Step(string effect, string requires = null)
            {
                name = effect;
                effects.Set(effect, true);
                if (requires != null) preconditions.Set(requires, true);
            }
            public override float GetCost(IReGoapAgent<string, object> agent, ReGoapState<string, object> state) => Cost;
            public override bool CheckPreconditions(IReGoapAgent<string, object> agent, ReGoapState<string, object> state)
                => Available && base.CheckPreconditions(agent, state);
            public override bool Run(IReGoapAgent<string, object> agent,
                Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail)
            {
                Runs++;
                if (Running) return false;
                if (!ReturnOnly) { if (Fail) fail(this); else done(this); }
                return true;
            }
        }

        [Test]
        public void WorldStatesCompareByValueRegardlessOfInsertionOrder()
        {
            var a = State(("a", true), ("b", 1));
            var b = State(("b", 1), ("a", true));
            Assert.That(a, Is.EqualTo(b));
            Assert.That(new HashSet<ReGoapState<string, object>> { a }.Contains(b), Is.True);
        }

        [Test]
        public void PlannerFindsCheaperMultiStepPathAndTerminatesUnreachableCycles()
        {
            var agent = new ReGoapAgent<string, object>();
            var prepare = new Step("ready") { Cost = 0.1f };
            var use = new Step("finished", "ready") { Cost = 0.1f };
            agent.AddAction(new Step("finished") { Cost = 5 });
            agent.AddAction(prepare);
            agent.AddAction(use);
            var planner = new ReGoapPlanner<string, object>();
            Assert.That(planner.Plan(agent, State(), State(("finished", true))), Is.EqualTo(new[] { prepare, use }));
            Assert.That(planner.Plan(agent, State(), State(("unreachable", true))), Is.Null);
        }

        [Test]
        public void UnachievableAndZeroPriorityGoalsDoNotStarvePossibleWork()
        {
            var agent = new ReGoapAgent<string, object>();
            agent.AddGoal(new Goal("missing", 100));
            agent.AddGoal(new Goal("inactive", 0));
            var reachable = new Goal("damage", 2);
            agent.AddGoal(reachable);
            agent.AddAction(new Step("damage"));
            agent.AddAction(new Step("inactive"));
            Assert.That(agent.TryPlan(new()), Is.True);
            Assert.That(agent.GetCurrentGoal(), Is.SameAs(reachable));
        }

        [Test]
        public void PlannerSkipsCheapActionWithFailedRuntimePrecondition()
        {
            var agent = new ReGoapAgent<string, object>();
            agent.AddAction(new Step("damage") { Available = false });
            var valid = new Step("damage") { Cost = 2 };
            agent.AddAction(valid);
            Assert.That(new ReGoapPlanner<string, object>().Plan(agent, State(), State(("damage", true)))?.Peek(), Is.SameAs(valid));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void FinalRunningActionRemainsAPlanUntilCompletion(bool returnOnly)
        {
            var agent = new ReGoapAgent<string, object>();
            var action = new Step("damage") { Running = true, ReturnOnly = returnOnly };
            agent.SetPlan(new(new[] { action }));
            agent.ExecuteCurrentAction();
            Assert.That(agent.HasPlan(), Is.True);
            action.Running = false;
            agent.ExecuteCurrentAction();
            Assert.That(agent.HasPlan(), Is.False);
            Assert.That(agent.GetCurrentAction(), Is.Null);
        }

        // Goal/factory construction needs identity only, not a database-backed NPC.
        // These tests never execute real game mechanics against this placeholder.
        private static MimicNPC Identity() => (MimicNPC)RuntimeHelpers.GetUninitializedObject(typeof(MimicNPC));

        [TestCase("DealDamageGoal", "CastOffensiveSpell")]
        [TestCase("EmergencyHealGoal", "HealOrCure")]
        [TestCase("HealGroupGoal", "HealOrCure")]
        [TestCase("CureGroupGoal", "HealOrCure")]
        [TestCase("ControlAddsGoal", "ControlAdd")]
        [TestCase("BuffMaintenanceGoal", "MaintainBuffs")]
        public void EveryRegisteredGoalHasAnActionWithMatchingEffects(string goalName, string actionName)
        {
            var agent = new MimicReGoapAgent(Identity(), new MimicBrain());
            agent.Initialize();
            agent.Initialize();
            Assert.That(agent.GetGoals(), Has.Count.EqualTo(6));
            Assert.That(agent.GetActions(), Has.Count.EqualTo(5));
            Assert.That(agent.GetSensors(), Has.Count.EqualTo(6));
            var goal = agent.GetGoals().Single(g => g.GetName() == goalName);
            var state = State((TacticalSensor.SupportAvailable, true), (TacticalSensor.BuffsAvailable, true),
                (TacticalSensor.ControlAvailable, true), (TacticalSensor.OffenseAvailable, true), (TacticalSensor.AttackAvailable, true));
            var plan = new ReGoapPlanner<string, object>().Plan(agent, state, goal.GetGoalState());
            Assert.That(plan, Is.Not.Null);
            Assert.That(plan.Peek().GetName(), Is.EqualTo(actionName));
        }

        [Test]
        public void OneRemainingAddStillActivatesControlGoal()
        {
            var agent = new MimicReGoapAgent(Identity(), new MimicBrain());
            agent.Initialize();
            var goal = agent.GetGoals().Single(g => g.GetName() == "ControlAddsGoal");
            Assert.That(goal.GetPriority(State((TacticalSensor.ControlAvailable, true), (MimicWorldStateKeys.NUM_CONTROLLABLE_ADDS, 1))), Is.GreaterThan(0));
        }

        private sealed class ActiveBrain : MimicBrain
        {
            public ActiveBrain() { IsHealer = true; } // No live target selection in controller-only tests.
            public override bool IsActive => true;
        }

        private sealed class ScriptedAgent : MimicReGoapAgent
        {
            public bool BrokenSensor;
            private sealed class TestSensor : ReGoapSensor<string, object>
            {
                public override void UpdateSensor() { }
            }
            public ScriptedAgent() : base(Identity(), new ActiveBrain()) { AddSensor(new TestSensor()); }
            public override void Initialize() { }
            public override void UpdateSensors()
            {
                if (BrokenSensor) throw new InvalidOperationException("Test sensor failure");
            }
        }

        [Test]
        public void ControllerExecutesGoapAndSwitchesToEmergencyOnNextTick()
        {
            var agent = new ScriptedAgent();
            var damage = new Step("damage");
            var heal = new Step("heal");
            var emergency = new Goal("heal", 0);
            agent.AddGoal(new Goal("damage", 2));
            agent.AddGoal(emergency);
            agent.AddAction(damage);
            agent.AddAction(heal);
            Assert.That(agent.TryThink(MimicDecisionContext.Combat), Is.True);
            emergency.Priority = 100;
            Assert.That(agent.TryThink(MimicDecisionContext.Combat), Is.True);
            Assert.That(damage.Runs, Is.EqualTo(1));
            Assert.That(heal.Runs, Is.EqualTo(1));
            Assert.That(agent.GetMemory().GetWorldState().Has("heal"), Is.False, "Predictions must not fabricate live health");
        }

        [Test]
        public void FailedCastYieldsToAnotherActionWithoutFsmOrRecursiveReplanning()
        {
            var agent = new ScriptedAgent();
            var cast = new Step("damage") { Fail = true };
            var melee = new Step("damage") { Cost = 2 };
            agent.AddGoal(new Goal("damage", 2));
            agent.AddAction(cast);
            agent.AddAction(melee);
            Assert.That(agent.TryThink(MimicDecisionContext.Combat), Is.True);
            Assert.That(cast.Runs, Is.EqualTo(1));
            Assert.That(melee.Runs, Is.EqualTo(1));
            Assert.That(agent.Fallbacks, Is.Zero);
            Assert.That(agent.GetActions(), Has.Count.EqualTo(2), "Failure exclusions last for one decision only");
        }

        [Test]
        public void FailureFallsBackAndRecoversOnNextDecision()
        {
            var agent = new ScriptedAgent();
            var step = new Step("damage") { Fail = true };
            agent.AddGoal(new Goal("damage", 2));
            agent.AddAction(step);
            Assert.That(agent.TryThink(MimicDecisionContext.Combat), Is.False);
            Assert.That(agent.HasPlan(), Is.False);
            step.Fail = false;
            Assert.That(agent.TryThink(MimicDecisionContext.Combat), Is.True);
            agent.GetSensors().Clear();
            Assert.That(agent.TryThink(MimicDecisionContext.Combat), Is.False);
        }

        [Test]
        public void MissingConfigurationDisabledAndBrokenSensorsFallBack()
        {
            var agent = new ScriptedAgent();
            Assert.That(agent.TryThink(MimicDecisionContext.Support), Is.False);
            agent.AddGoal(new Goal("damage", 2));
            var step = new Step("damage");
            agent.AddAction(step);
            agent.Disable();
            Assert.That(agent.TryThink(MimicDecisionContext.Combat), Is.False);
            agent.Enable();
            agent.BrokenSensor = true;
            Assert.That(agent.TryThink(MimicDecisionContext.Combat), Is.False);
            Assert.That(step.Runs, Is.Zero);
            agent.BrokenSensor = false;
            agent.Enable();
            Assert.That(agent.TryThink(MimicDecisionContext.Combat), Is.True);
        }

        private sealed class RoutingBrain : MimicBrain
        {
            public bool Handled;
            public int GoapCalls, FsmCalls;
            public override bool TryGoap(MimicDecisionContext context) { GoapCalls++; return Handled; }
            public override bool CheckSpells(eCheckSpellType type) { FsmCalls++; return false; }
        }

        [TestCase(true, 0)]
        [TestCase(false, 1)]
        public void LiveBrainThinkRoutesToGoapBeforeLegacyDecision(bool handled, int fallbackCalls)
        {
            var brain = new RoutingBrain { Handled = handled };
            brain.FSM.SetCurrentState(eFSMStateType.IDLE);
            brain.Think();
            Assert.That(brain.GoapCalls, Is.EqualTo(1));
            Assert.That(brain.FsmCalls, Is.EqualTo(fallbackCalls));
        }
    }
}
