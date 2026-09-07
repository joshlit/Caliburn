using System.Linq;
using DOL.GS.Scripts;
using DOL.GS.Scripts.ReGoap;

namespace DOL.GS.ReGoap.Mimic.Sensors
{
    /// <summary>Complete per-decision facts for the registered tactical goals.
    /// Reads health directly so planning never depends on CheckHeals having run first.</summary>
    public sealed class TacticalSensor : MimicSensor
    {
        public const string SupportAvailable = "supportAvailable";
        public const string BuffsAvailable = "buffsAvailable";
        public const string ControlAvailable = "controlAvailable";
        public const string OffenseAvailable = "offenseAvailable";
        public const string AttackAvailable = "attackAvailable";
        public const string CureNeeded = "cureNeeded";

        public override void UpdateSensor()
        {
            var context = ((MimicReGoapAgent)agent).Context;
            bool canAct = IsBodyValid() && !_body.IsStunned && !_body.IsMezzed;
            bool combat = context == MimicDecisionContext.Combat && !_brain.PreventCombat && !_brain.IsHealer;
            bool targetValid = combat && _body.TargetObject is GameLiving target && target.IsAlive
                && target.ObjectState == GameObject.eObjectState.Active && target.CurrentRegion == _body.CurrentRegion
                && !target.IsMezzed && !target.IsRooted && _brain.CanAggroTarget(target);

            // HasAggro is sufficient before the first attack: Body.InCombat is not yet set.
            SetBool(MimicWorldStateKeys.IN_COMBAT, _body.InCombat || (combat && _brain.HasAggro));
            SetBool(MimicWorldStateKeys.HAS_AGGRO, _brain.HasAggro);
            SetBool(MimicWorldStateKeys.HAS_TARGET, targetValid);
            SetBool(MimicWorldStateKeys.TARGET_MATCHES_MAIN_ASSIST,
                _body.Group == null || _body.Group.MimicGroup.CurrentTarget == _body.TargetObject);
            SetBool(MimicWorldStateKeys.IS_MAIN_ASSIST, _brain.IsMainAssist);

            var members = _body.Group == null ? new GameLiving[] { _body }
                : _body.Group.GetMembersInTheGroup().Cast<GameLiving>().ToArray();
            var reachable = members.Where(m => m.IsAlive && m.ObjectState == GameObject.eObjectState.Active
                && m.CurrentRegion == _body.CurrentRegion && _body.IsWithinRadius(m, WorldMgr.VISIBILITY_DISTANCE)).ToArray();
            int injured = reachable.Count(m => m.HealthPercent < (_brain.IsHealer ? 100 : MimicGroup.HealThreshold));
            SetInt(MimicWorldStateKeys.GROUP_SIZE, reachable.Length);
            SetInt(MimicWorldStateKeys.NUM_NEED_HEALING, injured);
            SetInt(MimicWorldStateKeys.NUM_EMERGENCY_HEALING, reachable.Count(m => m.HealthPercent < MimicGroup.EmergencyThreshold));
            SetInt(MimicWorldStateKeys.NUM_CRITICAL_HEALTH, reachable.Count(m => m.HealthPercent < 25));
            SetFloat(MimicWorldStateKeys.AVG_HEALTH_DEFICIT_PERCENT,
                reachable.Length == 0 ? 0 : (float)reachable.Average(m => 100 - m.HealthPercent));
            SetBool(CureNeeded, reachable.Any(m =>
                (m.IsMezzed && _body.CureMezz != null) ||
                (m.IsDiseased && (_body.CureDisease != null || _body.CureDiseaseGroup != null)) ||
                (m.IsPoisoned && (_body.CurePoison != null || _body.CurePoisonGroup != null))));

            var ccTargets = _body.Group?.MimicGroup.CCTargets;
            int adds = ccTargets?.Count(m => m != null && m.IsAlive && !m.IsMezzed && !m.IsStunned
                && !m.IsRooted && m != _body.Group.MimicGroup.CurrentTarget
                && m.CurrentRegion == _body.CurrentRegion && _brain.CanAggroTarget(m)) ?? 0;
            SetInt(MimicWorldStateKeys.NUM_CONTROLLABLE_ADDS, adds);
            SetInt(MimicWorldStateKeys.NUM_ENEMIES, _brain.GetOrderedAggroList().Count);
            SetBool(MimicWorldStateKeys.IS_MAIN_CC, _brain.IsMainCC);
            SetBool(MimicWorldStateKeys.BUFFS_MAINTAINED, false);
            SetBool(SupportAvailable, canAct && !_body.IsSilenced && _body.CanCastHealSpells);
            SetBool(BuffsAvailable, canAct && context == MimicDecisionContext.Support && !_body.InCombat
                && !_body.IsMoving && !_body.IsSilenced);
            SetBool(ControlAvailable, canAct && context != MimicDecisionContext.Healing && !_brain.PreventCombat
                && _brain.IsMainCC && adds > 0 && _body.CanCastCrowdControlSpells && !_body.IsSilenced && !_body.IsCasting);
            SetBool(OffenseAvailable, canAct && targetValid && !_body.IsSilenced && !_brain.IsFleeing);
            SetBool(AttackAvailable, canAct && targetValid && !_body.IsCasting);
        }
    }
}
