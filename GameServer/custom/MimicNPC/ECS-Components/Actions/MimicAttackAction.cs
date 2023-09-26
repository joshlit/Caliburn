using System;
using System.Linq;
using DOL.AI.Brain;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.GS.ServerProperties;
using DOL.Language;

namespace DOL.GS.Scripts
{
    public class MimicAttackAction : AttackAction
    {
        private MimicNPC _mimicOwner;

        public MimicAttackAction(MimicNPC mimicOwner) : base(mimicOwner)
        {
            _mimicOwner = mimicOwner;

            _isGuardArcher = _npcOwner is GuardArcher;

            if (Properties.ALWAYS_CHECK_PET_LOS && mimicOwner.Brain is IControlledBrain npcOwnerBrain)
            {
                _npcOwnerOwner = npcOwnerBrain.GetPlayerOwner();
                new ECSGameTimer(_npcOwner, new ECSGameTimer.ECSTimerCallback(CheckLos), 1);
            }
            else
                _hasLos = true;
        }

        //public static new AttackAction Create(GameLiving gameLiving)
        //{
        //    if (gameLiving is MimicNPC mimicNPC)
        //        return new MimicAttackAction(mimicNPC);
        //    else if (gameLiving is GameNPC gameNpc)
        //        return new NpcAttackAction(gameNpc);
        //    else if (gameLiving is GamePlayer gamePlayer)
        //        return new PlayerAttackAction(gamePlayer);

        //    return null;
        //}

        private const int MIN_HEALTH_PERCENT_FOR_MELEE_SWITCH_ON_INTERRUPT = 70;
        // Check interval (upper bound) in ms of entities around this NPC when its main target is out of range. Used to attack other entities on its path.
        private const int NPC_VICINITY_CHECK_INTERVAL = 1000;
        private const int PET_LOS_CHECK_INTERVAL = 1000;

        private GameNPC _npcOwner;
        private bool _isGuardArcher;
        // Next check for NPCs in attack range to hit while on the way to main target.
        private long _nextVicinityCheck = 0;
        private GamePlayer _npcOwnerOwner;
        private int _petLosCheckInterval = PET_LOS_CHECK_INTERVAL;
        private bool _hasLos;

        public override void OnAimInterrupt(GameObject attacker)
        {
            // Guard archers shouldn't switch to melee when interrupted, otherwise they fall from the wall.
            // They will still switch to melee if their target is in melee range.
            if (!_isGuardArcher && _npcOwner.HealthPercent < MIN_HEALTH_PERCENT_FOR_MELEE_SWITCH_ON_INTERRUPT)
                _npcOwner.SwitchToMelee(_target);
        }

        protected override bool PrepareMeleeAttack()
        {
            if (!_hasLos)
            {
                _interval = TICK_INTERVAL_FOR_NON_ATTACK;
                return false;
            }

            // NPCs try to switch to their ranged weapon whenever possible.
            //if (!_npcOwner.IsBeingInterrupted &&
            //    _npcOwner.Inventory?.GetItem(eInventorySlot.DistanceWeapon) != null &&
            //    !_npcOwner.IsWithinRadius(_target, 500))
            //{
            //    _npcOwner.SwitchToRanged(_target);
            //    _interval = _attackComponent.AttackSpeed(_weapon);
            //    return false;
            //}

            _combatStyle = _styleComponent.NPCGetStyleToUse();

            if (!MimimcPrepareMeleeAttack())
                return false;

            // The target isn't in melee range yet. Check if another target is in range to attack on the way to the main target.
            if (_target != null &&
                _npcOwner.Brain is not IControlledBrain &&
                _npcOwner.Brain is StandardMobBrain npcBrain &&
                npcBrain.AggroTable.Count > 0 &&
                !_npcOwner.IsWithinRadius(_target, _attackComponent.AttackRange))
            {
                GameLiving possibleTarget = null;
                long maxaggro = 0;
                long aggro;

                foreach (GamePlayer playerInRadius in _npcOwner.GetPlayersInRadius((ushort)_attackComponent.AttackRange))
                {
                    if (npcBrain.AggroTable.ContainsKey(playerInRadius))
                    {
                        aggro = npcBrain.GetAggroAmountForLiving(playerInRadius);

                        if (aggro <= 0)
                            continue;

                        if (aggro > maxaggro)
                        {
                            possibleTarget = playerInRadius;
                            maxaggro = aggro;
                        }
                    }
                }

                // Check for NPCs in attack range. Only check if the NPCNextNPCVicinityCheck is less than the current GameLoop Time.
                if (_nextVicinityCheck < GameLoop.GameLoopTime)
                {
                    // Set the next check for NPCs. Will be in a range from 100ms -> NPC_VICINITY_CHECK_DELAY.
                    _nextVicinityCheck = GameLoop.GameLoopTime + Util.Random(100, NPC_VICINITY_CHECK_INTERVAL);

                    foreach (GameNPC npcInRadius in _npcOwner.GetNPCsInRadius((ushort)_attackComponent.AttackRange))
                    {
                        if (npcBrain.AggroTable.ContainsKey(npcInRadius))
                        {
                            aggro = npcBrain.GetAggroAmountForLiving(npcInRadius);

                            if (aggro <= 0)
                                continue;

                            if (aggro > maxaggro)
                            {
                                possibleTarget = npcInRadius;
                                maxaggro = aggro;
                            }
                        }
                    }
                }

                if (possibleTarget == null)
                {
                    _interval = TICK_INTERVAL_FOR_NON_ATTACK;
                    return false;
                }
                else
                    _target = possibleTarget;
            }

            return true;
        }

        private bool MimimcPrepareMeleeAttack()
        {
            bool clearOldStyles = false;

            if (_lastAttackData != null)
            {
                switch (_lastAttackData.AttackResult)
                {
                    case eAttackResult.Fumbled:
                    {
                        // Skip this attack if the last one fumbled.
                        _styleComponent.NextCombatStyle = null;
                        _styleComponent.NextCombatBackupStyle = null;
                        _lastAttackData.AttackResult = eAttackResult.Missed;
                        _interval = _attackComponent.AttackSpeed(_weapon) * 2;
                        return false;
                    }
                    case eAttackResult.OutOfRange:
                    case eAttackResult.TargetNotVisible:
                    case eAttackResult.NotAllowed_ServerRules:
                    case eAttackResult.TargetDead:
                    {
                        clearOldStyles = true;
                        break;
                    }
                }
            }

            if (_combatStyle != null && _combatStyle.WeaponTypeRequirement == (int)eObjectType.Shield)
                _weapon = _leftWeapon;

            int mainHandAttackSpeed = _attackComponent.AttackSpeed(_weapon);

            if (clearOldStyles || _styleComponent.NextCombatStyleTime + mainHandAttackSpeed < GameLoop.GameLoopTime)
            {
                // Cancel the styles if they were registered too long ago.
                // Nature's Shield stays active forever and falls back to a non-backup style.
                if (_styleComponent.NextCombatBackupStyle?.ID == 394)
                    _styleComponent.NextCombatStyle = _styleComponent.NextCombatBackupStyle;
                else if (_styleComponent.NextCombatStyle?.ID != 394)
                    _styleComponent.NextCombatStyle = null;

                _styleComponent.NextCombatBackupStyle = null;
            }

            // Styles must be checked before the target.
            if (_target == null)
            {
                _interval = TICK_INTERVAL_FOR_NON_ATTACK;
                return false;
            }

            // Damage is doubled on sitting players, but only with melee weapons; arrows and magic do normal damage.
            if (_target is GamePlayer playerTarget && playerTarget.IsSitting ||
                _target is MimicNPC mimicTarget && mimicTarget.IsSitting)
                _effectiveness *= 2;

            _interruptDuration = mainHandAttackSpeed;
            return true;
        }

        protected override void PerformMeleeAttack()
        {
            _attackComponent.weaponAction = new MimicWeaponAction(_mimicOwner, _target, _weapon, _leftWeapon, _effectiveness, _interruptDuration, _combatStyle);
            _attackComponent.weaponAction.Execute();
            _lastAttackData = _mimicOwner.TempProperties.GetProperty<AttackData>("LastAttackData", null);
        }

        protected override bool PrepareRangedAttack()
        {
            if (!_hasLos)
            {
                _interval = TICK_INTERVAL_FOR_NON_ATTACK;
                return false;
            }

            return base.PrepareRangedAttack();
        }

        protected override bool FinalizeRangedAttack()
        {
            // Switch to melee if range to target is less than 200.
            if (_npcOwner != null &&
                _npcOwner.TargetObject != null &&
                _npcOwner.IsWithinRadius(_target, 200))
            {
                _npcOwner.SwitchToMelee(_target);
                _interval = 1;
                return false;
            }
            else
                return base.FinalizeRangedAttack();
        }

        public override void CleanUp()
        {
            _petLosCheckInterval = 0;

            if (_npcOwner.Brain is NecromancerPetBrain necromancerPetBrain)
                necromancerPetBrain.ClearAttackSpellQueue();

            base.CleanUp();
        }

        private int CheckLos(ECSGameTimer timer)
        {
            if (_target == null)
                _hasLos = false;
            else if (_npcOwner.ActiveWeaponSlot != eActiveWeaponSlot.Distance)
                _hasLos = true;
            else if (_target is GamePlayer || (_target is GameNPC _targetNpc &&
                                              _targetNpc.Brain is IControlledBrain _targetNpcBrain &&
                                              _targetNpcBrain.GetPlayerOwner() != null))
                // Target is either a player or a pet owned by a player.
                _npcOwnerOwner.Out.SendCheckLOS(_npcOwner, _target, new CheckLOSResponse(LosCheckCallback));
            else
                _hasLos = true;

            return _petLosCheckInterval;
        }

        private void LosCheckCallback(GamePlayer player, ushort response, ushort targetOID)
        {
            if (targetOID == 0)
                return;

            _hasLos = (response & 0x100) == 0x100;
        }
    }
}
