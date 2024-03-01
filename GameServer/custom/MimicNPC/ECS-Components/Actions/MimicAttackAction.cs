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

        // Check interval (upper bound) in ms of entities around this NPC when its main target is out of range. Used to attack other entities on its path.
        private const int NPC_VICINITY_CHECK_INTERVAL = 1000;
        private const int PET_LOS_CHECK_INTERVAL = 1000;

        private MimicNPC _mimicOwner;
        // Next check for NPCs in attack range to hit while on the way to main target.
        private long _nextVicinityCheck = 0;
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

            _combatStyle = StyleComponent.NPCGetStyleToUse();

            if (!base.PrepareMeleeAttack())
                return false;

            // The target isn't in melee range yet. Check if another target is in range to attack on the way to the main target.
            if (_target != null &&
                _mimicOwner.Brain is not IControlledBrain &&
                _mimicOwner.Brain is StandardMobBrain npcBrain &&
                npcBrain.AggroTable.Count > 0 &&
                !_mimicOwner.IsWithinRadius(_target, AttackComponent.AttackRange))
            {
                GameLiving possibleTarget = null;
                long maxaggro = 0;
                long aggro;

                foreach (GamePlayer playerInRadius in _mimicOwner.GetPlayersInRadius((ushort)AttackComponent.AttackRange))
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

                    foreach (GameNPC npcInRadius in _mimicOwner.GetNPCsInRadius((ushort)AttackComponent.AttackRange))
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
