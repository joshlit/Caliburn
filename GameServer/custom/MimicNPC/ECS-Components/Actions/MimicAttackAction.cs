﻿using DOL.AI.Brain;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.GS.Scripts;
using DOL.GS.ServerProperties;
using System;
using DOL.Database;
using static DOL.GS.GameObject;
using static DOL.GS.NpcAttackAction;

namespace DOL.GS
{
    public class MimicAttackAction : AttackAction
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int MIN_HEALTH_PERCENT_FOR_MELEE_SWITCH_ON_INTERRUPT = 70;
        private MimicNPC _mimicOwner;
        private bool _hasLos;

        private CheckLosTimer _checkLosTimer;
        private GameObject _losCheckTarget;

        private static int LosCheckInterval => Properties.CHECK_LOS_DURING_RANGED_ATTACK_MINIMUM_INTERVAL;
        private bool HasLosOnCurrentTarget => _losCheckTarget == _target && _hasLos;


        public MimicAttackAction(MimicNPC owner) : base(owner)
        {
            _mimicOwner = owner;
        }

        protected override bool PrepareMeleeAttack()
        {
            if (StyleComponent.NextCombatStyle == null)
                _combatStyle = StyleComponent.GetStyleToUse();
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
            // TODO: Get LOS working for archers.
            //if (Properties.CHECK_LOS_BEFORE_NPC_RANGED_ATTACK)
            //{
            //    if (_checkLosTimer == null)
            //        _checkLosTimer = new CheckLosTimer(_mimicOwner, _target, LosCheckCallback);
            //    else
            //        _checkLosTimer.ChangeTarget(_target);

            //    if (!HasLosOnCurrentTarget)
            //    {
            //        _interval = TICK_INTERVAL_FOR_NON_ATTACK;
            //        return false;
            //    }
            //}
            //else
                _hasLos = true;

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
            // Switch to melee if range to target is less than 200.
            if (_mimicOwner != null &&
                _mimicOwner.TargetObject != null &&
                _mimicOwner.IsWithinRadius(_target, 200))
            {
                SwitchToMeleeAndTick();
                _interval = 0;
                return false;
            }

            return base.FinalizeRangedAttack();
        }
        
        private void SwitchToMeleeAndTick()
        {
            if (_mimicOwner.ActiveWeaponSlot is not eActiveWeaponSlot.Distance)
                return;

            _mimicOwner.StartAttackWithMeleeWeapon(_target);
        }

        public override void OnAimInterrupt(GameObject attacker)
        {
            if (_mimicOwner.HealthPercent < MIN_HEALTH_PERCENT_FOR_MELEE_SWITCH_ON_INTERRUPT)
                SwitchToMeleeAndTick();
        }

        protected override void CleanUp()
        {
            if (_mimicOwner.Brain is NecromancerPetBrain necromancerPetBrain)
                necromancerPetBrain.ClearAttackSpellQueue();

            if (_checkLosTimer != null)
            {
                _checkLosTimer.Stop();
                _checkLosTimer = null;
            }

            base.CleanUp();
        }

        private void LosCheckCallback(GamePlayer player, eLosCheckResponse response, ushort sourceOID, ushort targetOID)
        {
            _hasLos = response is eLosCheckResponse.TRUE;
            _losCheckTarget = _mimicOwner.CurrentRegion.GetObject(targetOID);

            if (_losCheckTarget == null)
                return;

            if (_hasLos)
            {
                _mimicOwner.TurnTo(_losCheckTarget);
                return;
            }

            if (_mimicOwner.attackComponent.AttackState)
            {
                SwitchToMeleeAndTick();
                _interval = 0;
            }
        }

        public class CheckLosTimer : ECSGameTimerWrapperBase
        {
            private GameNPC _npcOwner;
            private GameObject _target;
            private CheckLosResponse _callback;
            private GamePlayer _losChecker;

            public CheckLosTimer(GameObject owner, GameObject target, CheckLosResponse callback) : base(owner)
            {
                _npcOwner = owner as GameNPC;
                _callback = callback;
                ChangeTarget(target);
            }

            public void ChangeTarget(GameObject newTarget)
            {
                if (newTarget == null)
                {
                    Stop();
                    return;
                }

                if (_target != newTarget)
                {
                    _target = newTarget;

                    if (_npcOwner.Brain is IControlledBrain brain)
                        _losChecker = brain.GetPlayerOwner();
                    if (_target is GamePlayer targetPlayer)
                        _losChecker = targetPlayer;
                    else if (_target is GameNPC npcTarget && npcTarget.Brain is IControlledBrain targetBrain)
                        _losChecker = targetBrain.GetPlayerOwner();
                }

                // Don't bother starting the timer if there's no one to perform the LoS check.
                if (_losChecker == null)
                {
                    _callback(null, eLosCheckResponse.TRUE, 0, 0);
                    return;
                }

                if (!IsAlive && _losChecker != null)
                {
                    Start(1);
                    Interval = LosCheckInterval;
                }
            }

            protected override int OnTick(ECSGameTimer timer)
            {
                // We normally rely on `AttackActon.CleanUp()` to stop this timer.
                if (!_npcOwner.attackComponent.AttackState || _npcOwner.ObjectState is not eObjectState.Active)
                    return 0;

                _losChecker.Out.SendCheckLos(_npcOwner, _target, new CheckLosResponse(_callback));
                return LosCheckInterval;
            }
        }
    }
}
