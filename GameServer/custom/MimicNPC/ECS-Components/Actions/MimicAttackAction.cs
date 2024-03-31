using DOL.AI.Brain;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.GS.Scripts;
using DOL.GS.ServerProperties;
using System;

namespace DOL.GS
{
    public class MimicAttackAction : AttackAction
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int MIN_HEALTH_PERCENT_FOR_MELEE_SWITCH_ON_INTERRUPT = 70;
        private const int PET_LOS_CHECK_INTERVAL = 1000;
        private MimicNPC _mimicOwner;
        private int _petLosCheckInterval = PET_LOS_CHECK_INTERVAL;
        private bool _hasLos;

        public MimicAttackAction(MimicNPC mimicOwner) : base(mimicOwner)
        {
            _mimicOwner = mimicOwner;
            _hasLos = true;
        }

        protected override bool PrepareMeleeAttack()
        {
            if (!_hasLos)
            {
                _interval = TICK_INTERVAL_FOR_NON_ATTACK;
                return false;
            }

            if (StyleComponent.NextCombatStyle == null)
                _combatStyle = StyleComponent.NPCGetStyleToUse();
            else
                _combatStyle = StyleComponent.NextCombatStyle;

            if (!base.PrepareMeleeAttack())
                return false;

            int attackRange = AttackComponent.AttackRange;

            // The target isn't in melee range yet. Check if another target is in range to attack on the way to the main target.
            if (!_mimicOwner.IsWithinRadius(_target, attackRange) &&
                 _mimicOwner.Brain is MimicBrain mimicBrain)
            {
                _target = mimicBrain.LastHighestThreatInAttackRange;

                if (_target == null || !_mimicOwner.IsWithinRadius(_target, attackRange))
                {
                    _interval = TICK_INTERVAL_FOR_NON_ATTACK;
                    return false;
                }
            }

            return true;
        }

        protected override bool PrepareRangedAttack()
        {
            if (!_hasLos)
            {
                _interval = TICK_INTERVAL_FOR_NON_ATTACK;
                return false;
            }

            if (base.PrepareRangedAttack())
            {
                // This is also done in weaponAction.Execute(), but we must unstealth immediately if the call is delayed.
                if (_ticksToTarget > 0)
                    _mimicOwner.Stealth(false);

                return true;
            }

            return false;
        }

        protected override void PerformRangedAttack()
        {
            _mimicOwner.rangeAttackComponent.RemoveEnduranceAndAmmoOnShot();
            base.PerformRangedAttack();
        }

        protected override bool FinalizeRangedAttack()
        {
            // Switch to melee if range to target is less than 350.
            if (_mimicOwner != null &&
                _mimicOwner.TargetObject != null &&
                _mimicOwner.IsWithinRadius(_target, 350))
            {
                _mimicOwner.SwitchToMelee(_target);
                _interval = 1;
                return false;
            }
            else
                return base.FinalizeRangedAttack();
        }

        public override void OnAimInterrupt(GameObject attacker)
        {
            if (_mimicOwner.HealthPercent < MIN_HEALTH_PERCENT_FOR_MELEE_SWITCH_ON_INTERRUPT)
                _mimicOwner.SwitchToMelee(_target);
        }

        public override void CleanUp()
        {
            _petLosCheckInterval = 0;

            if (_mimicOwner.Brain is NecromancerPetBrain necromancerPetBrain)
                necromancerPetBrain.ClearAttackSpellQueue();

            base.CleanUp();
        }

        private int CheckLos(ECSGameTimer timer)
        {
            if (_target == null)
                _hasLos = false;
            else if (_mimicOwner.ActiveWeaponSlot != eActiveWeaponSlot.Distance)
                _hasLos = true;
            else if (_target is GamePlayer || (_target is GameNPC _targetNpc &&
                                              _targetNpc.Brain is IControlledBrain _targetNpcBrain &&
                                              _targetNpcBrain.GetPlayerOwner() != null))
                // Target is either a player or a pet owned by a player.
                _mimicOwner.Out.SendCheckLos(_mimicOwner, _target, new CheckLosResponse(LosCheckCallback));
            else
                _hasLos = true;

            return _petLosCheckInterval;
        }

        private void LosCheckCallback(GamePlayer player, eLosCheckResponse response, ushort sourceOID, ushort targetOID)
        {
            if (targetOID == 0)
                return;

            _hasLos = response is eLosCheckResponse.TRUE;
        }
    }
}
