using DOL.Database;
using DOL.GS;
using DOL.GS.API;
using DOL.GS.Effects;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;
using DOL.GS.Scripts;
using DOL.GS.ServerProperties;
using DOL.GS.SkillHandler;
using DOL.GS.Spells;
using DOL.GS.Styles;
using DOL.Language;
using log4net;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using static DOL.AI.Brain.StandardMobBrain;
using static DOL.GS.Styles.Style;

namespace DOL.AI.Brain
{
    public class MimicBrain : ABrain, IOldAggressiveBrain
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override bool IsActive => Body != null && Body.IsAlive && Body.ObjectState == GameObject.eObjectState.Active;

        public bool IsMainPuller { get { return Body.Group?.MimicGroup.MainPuller == Body; } }

        public bool IsMainTank { get { return Body.Group?.MimicGroup.MainTank == Body; } }

        public bool IsMainLeader { get { return Body.Group?.MimicGroup.MainLeader == Body; } }

        public bool IsMainCC { get { return Body.Group?.MimicGroup.MainCC == Body; } }

        public bool IsMainAssist { get { return Body.Group?.MimicGroup.MainAssist == Body; } }

        private MimicNPC _mimicBody;

        public MimicNPC MimicBody
        {
            get { return _mimicBody; }
            set { _mimicBody = value; }
        }

        public const int MAX_AGGRO_DISTANCE = 3600;
        public const int MAX_AGGRO_LIST_DISTANCE = 6000;
        private const int EFFECTIVE_AGGRO_AMOUNT_CALCULATION_DISTANCE_THRESHOLD = 500;

        public bool PreventCombat;
        public bool PvPMode;
        public bool Defend;
        public bool Roam;
        public bool IsFleeing;
        public bool IsPulling;
        public bool Debug;

        public GameObject LastTargetObject;
        public bool IsFlanking;
        public Point2D TargetFlankPosition;

        public Point3D TargetFleePosition;

        // Used for AmbientBehaviour "Seeing" - maintains a list of GamePlayer in range
        public List<GamePlayer> PlayersSeen = new();

        /// <summary>
        /// Constructs a new MimicBrain
        /// </summary>
        public MimicBrain() : base()
        {
            FSM = new FSM();
            FSM.Add(new MimicState_Idle(this));
            FSM.Add(new MimicState_WakingUp(this));
            FSM.Add(new MimicState_Aggro(this));
            FSM.Add(new MimicState_ReturnToSpawn(this));
            FSM.Add(new MimicState_Patrolling(this));
            FSM.Add(new MimicState_Roaming(this));
            FSM.Add(new MimicState_FollowLeader(this));
            FSM.Add(new MimicState_Camp(this));
            FSM.Add(new MimicState_Duel(this));
            FSM.Add(new MimicState_Dead(this));

            FSM.SetCurrentState(eFSMStateType.WAKING_UP);
        }

        /// <summary>
        /// Returns the string representation of the MimicBrain
        /// </summary>
        public override string ToString()
        {
            return base.ToString() + ", AggroLevel=" + AggroLevel.ToString() + ", AggroRange=" + AggroRange.ToString();
        }

        public override bool Stop()
        {
            // tolakram - when the brain stops, due to either death or no players in the vicinity, clear the aggro list
            if (base.Stop())
            {
                ClearAggroList();
                return true;
            }

            return false;
        }

        public override void KillFSM()
        {
            FSM.KillFSM();
        }

        #region AI

        public override void Think()
        {
            FSM.Think();
        }

        public virtual void OnLeaderAggro()
        { }
        public virtual void OnEnterAggro()
        { }

        public virtual void OnExitAggro()
        { }

        public virtual void OnEnterRoam()
        { }

        public virtual void OnExitRoam()
        { }

        public virtual void OnLevelUp()
        { }

        public virtual void OnRefreshSpecDependantSkills()
        { }

        public void OnGroupMemberAttacked(AttackData ad)
        {
            if (FSM.GetState(eFSMStateType.CAMP) == FSM.GetCurrentState())
            {
                if (!Body.IsWithinRadius(ad.Attacker, AggroRange))
                    return;
            }

            switch (ad.AttackResult)
            {
                case eAttackResult.Blocked:
                case eAttackResult.Evaded:
                case eAttackResult.Fumbled:
                case eAttackResult.HitStyle:
                case eAttackResult.HitUnstyled:
                case eAttackResult.Missed:
                case eAttackResult.Parried:
                AddToAggroList(ad.Attacker, 1);
                break;
            }

            if (FSM.GetState(eFSMStateType.AGGRO) != FSM.GetCurrentState())
                FSM.SetCurrentState(eFSMStateType.AGGRO);
        }

        public virtual bool CheckProximityAggro(int aggroRange)
        {
            //FireAmbientSentence();

            if (PvPMode || AggroLevel > 0 && AggroRange > 0 && !HasAggro && Body.CurrentSpellHandler == null)
            {
                CheckPlayerAggro();
                CheckNPCAggro(aggroRange);
            }

            // Some calls rely on this method to return if there's something in the aggro list, not necessarily to perform a proximity aggro check.
            // But this doesn't necessarily return whether or not the check was positive, only the current state (LoS checks take time).
            return HasAggro;
        }

        public virtual bool HasPatrolPath()
        {
            return Body.MaxSpeedBase > 0 &&
                Body.CurrentSpellHandler == null &&
                !Body.IsMoving &&
                !Body.attackComponent.AttackState &&
                !Body.InCombat &&
                !Body.IsMovingOnPath &&
                !string.IsNullOrEmpty(Body.PathID);
        }

        /// <summary>
        /// Check for aggro against players
        /// </summary>
        protected virtual void CheckPlayerAggro()
        {
            foreach (GamePlayer player in Body.GetPlayersInRadius((ushort)AggroRange))
            {
                if (!CanAggroTarget(player))
                    continue;

                if (player.IsStealthed || player.Steed != null)
                    continue;

                if (player.effectListComponent.ContainsEffectForEffectType(eEffect.Shade))
                    continue;

                if (Properties.CHECK_LOS_BEFORE_AGGRO)
                    // We don't know if the LoS check will be positive, so we have to ask other players
                    player.Out.SendCheckLos(Body, player, new CheckLosResponse(LosCheckForAggroCallback));
                else
                {
                    AddToAggroList(player, 1);
                    return;
                }
            }
        }

        /// <summary>
        /// Check for aggro against close NPCs
        /// </summary>
        protected virtual void CheckNPCAggro(int aggroRange)
        {
            List<GameNPC> npcsInRadius = Body.GetNPCsInRadius((ushort)aggroRange);

            if (npcsInRadius.Count > 1)
            {
                int startIndex = Util.Random(0, npcsInRadius.Count - 1);

                for (int i = 0; i < npcsInRadius.Count; i++)
                {
                    int index = startIndex + i;

                    if (index >= npcsInRadius.Count)
                        index = i - (npcsInRadius.Count - startIndex);

                    if (!CanAggroTarget(npcsInRadius[index]))
                        continue;

                    if (npcsInRadius[index] is IGamePlayer player && player.IsStealthed && !MimicBody.CanDetect(player))
                        continue;

                    if (npcsInRadius[index] is GameTaxi or GameTrainingDummy)
                        continue;

                    if (Properties.CHECK_LOS_BEFORE_AGGRO)
                    {
                        // Check LoS if either the target or the current mob is a pet
                        if (npcsInRadius[index].Brain is ControlledMobBrain theirControlledMobBrain && theirControlledMobBrain.GetPlayerOwner() is GamePlayer theirOwner)
                        {
                            theirOwner.Out.SendCheckLos(Body, npcsInRadius[index], new CheckLosResponse(LosCheckForAggroCallback));
                            continue;
                        }
                    }

                    AddToAggroList(npcsInRadius[index], 1);

                    return;
                }
            }

            //foreach (GameNPC npc in Body.GetNPCsInRadius((ushort)aggroRange))
            //{
            //    if (!CanAggroTarget(npc))
            //        continue;

            //    if (npc is IGamePlayer player && player.IsStealthed && !MimicBody.CanDetect(player))
            //        continue;

            //    if (npc is GameTaxi or GameTrainingDummy)
            //        continue;

            //    if (Properties.CHECK_LOS_BEFORE_AGGRO)
            //    {
            //        // Check LoS if either the target or the current mob is a pet
            //        if (npc.Brain is ControlledMobBrain theirControlledMobBrain && theirControlledMobBrain.GetPlayerOwner() is GamePlayer theirOwner)
            //        {
            //            theirOwner.Out.SendCheckLos(Body, npc, new CheckLosResponse(LosCheckForAggroCallback));
            //            continue;
            //        }
            //    }

            //    AddToAggroList(npc, 1);

            //    return;
            //}
        }

        public virtual void FireAmbientSentence()
        {
            if (Body.ambientTexts != null && Body.ambientTexts.Any(item => item.Trigger == "seeing"))
            {
                // Check if we can "see" players and fire off ambient text
                List<GamePlayer> currentPlayersSeen = new();

                foreach (GamePlayer player in Body.GetPlayersInRadius((ushort)AggroRange))
                {
                    if (!PlayersSeen.Contains(player))
                    {
                        Body.FireAmbientSentence(GameNPC.eAmbientTrigger.seeing, player);
                        PlayersSeen.Add(player);
                    }

                    currentPlayersSeen.Add(player);
                }

                for (int i = 0; i < PlayersSeen.Count; i++)
                {
                    if (!currentPlayersSeen.Contains(PlayersSeen[i]))
                        PlayersSeen.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// The interval for thinking, min 1.5 seconds
        /// 10 seconds for 0 aggro mobs
        /// </summary>
        public override int ThinkInterval
        {
            get
            {
                return 500;
            }
        }

        /// <summary>
        /// If this brain is part of a formation, it edits it's values accordingly.
        /// </summary>
        /// <param name="x">The x-coordinate to refer to and change</param>
        /// <param name="y">The x-coordinate to refer to and change</param>
        /// <param name="z">The x-coordinate to refer to and change</param>
        public virtual bool CheckFormation(ref int x, ref int y, ref int z)
        {
            return false;
        }

        /// <summary>
        /// Checks the Abilities
        /// </summary>
        public virtual void CheckDefensiveAbilities()
        {
            if (Body.Abilities == null || Body.Abilities.Count <= 0 || Body.Group == null)
                return;

            foreach (Ability ab in Body.GetAllAbilities())
            {
                switch (ab.KeyName)
                {
                    case Abilities.Intercept:
                    {
                        //if (Body.Group != null)
                        //{
                        //    GameLiving interceptTarget;
                        //    List<GameLiving> interceptTargets = new List<GameLiving>();

                        //    foreach (GameLiving groupMember in Body.Group.GetMembersInTheGroup())
                        //    {
                        //        if (groupMember is MimicNPC mimic)
                        //        {
                        //            if (mimic.CharacterClass.ID == (int)eCharacterClass.Cleric ||
                        //                mimic.CharacterClass.ID == (int)eCharacterClass.Druid ||
                        //                mimic.CharacterClass.ID == (int)eCharacterClass.Healer ||
                        //                mimic.CharacterClass.ID == (int)eCharacterClass.Friar ||
                        //                mimic.CharacterClass.ID == (int)eCharacterClass.Bard ||
                        //                mimic.CharacterClass.ID == (int)eCharacterClass.Shaman)
                        //            {
                        //                interceptTargets.Add(groupMember);
                        //            }
                        //        }
                        //    }
                        //}
                        break;
                    }

                    case Abilities.Guard:
                    {
                        break;
                    }
                        
                    case Abilities.Protect:
                    {
                        break;
                    }
                }
            }
        }

        public void CheckOffensiveAbilities()
        {
            if (Body.Abilities == null || Body.Abilities.Count <= 0)
                return;

            if (CanUseAbility())
            {
                foreach (Ability ab in Body.GetAllAbilities())
                {
                    if (Body.GetSkillDisabledDuration(ab) == 0)
                    {
                        switch (ab.KeyName)
                        {
                            case Abilities.Berserk:
                            {
                                if (Body.TargetObject is GameLiving target)
                                {
                                    if (Body.IsWithinRadius(Body.TargetObject, Body.MeleeAttackRange) &&
                                        GameServer.ServerRules.IsAllowedToAttack(Body, target, true))
                                    {
                                        new BerserkECSGameEffect(new ECSGameEffectInitParams(Body, 20000, 1));
                                        Body.DisableSkill(ab, 420000);
                                    }
                                }

                                break;
                            }

                            case Abilities.Stag:
                            {
                                if (Body.TargetObject is GameLiving target)
                                {
                                    if (Body.IsWithinRadius(Body.TargetObject, Body.MeleeAttackRange) &&
                                        GameServer.ServerRules.IsAllowedToAttack(Body, target, true) || Body.HealthPercent < 75)
                                    {
                                        new StagECSGameEffect(new ECSGameEffectInitParams(Body, 30000, 1), ab.Level);
                                        Body.DisableSkill(ab, 900000);
                                    }
                                }

                                break;
                            }

                            case Abilities.Triple_Wield:
                            {
                                if (Body.TargetObject is GameLiving target)
                                {
                                    if (Body.IsWithinRadius(Body.TargetObject, Body.MeleeAttackRange) &&
                                        GameServer.ServerRules.IsAllowedToAttack(Body, target, true))
                                    {
                                        new TripleWieldECSGameEffect(new ECSGameEffectInitParams(Body, 30000, 1));
                                        Body.DisableSkill(ab, 420000);
                                    }
                                }

                                break;
                            }

                            case Abilities.DirtyTricks:
                            {
                                if (Body.TargetObject is GameLiving target)
                                {
                                    IGamePlayer gamePlayer = target as IGamePlayer;

                                    if (gamePlayer != null && gamePlayer.CharacterClass.ClassType == eClassType.ListCaster)
                                        break;

                                    if (Body.IsWithinRadius(Body.TargetObject, Body.MeleeAttackRange) &&
                                        GameServer.ServerRules.IsAllowedToAttack(Body, target, true))
                                    {
                                        new DirtyTricksECSGameEffect(new ECSGameEffectInitParams(Body, 30000, 1));
                                        Body.DisableSkill(ab, 420000);
                                    }
                                }

                                break;
                            }

                            case Abilities.ChargeAbility:
                            {
                                if (Body.TargetObject is GameLiving target &&
                                    GameServer.ServerRules.IsAllowedToAttack(Body, target, true) &&
                                    !Body.IsWithinRadius(target, 500))
                                {
                                    ChargeAbility charge = Body.GetAbility<ChargeAbility>();

                                    if (charge != null && Body.GetSkillDisabledDuration(charge) <= 0)
                                        charge.Execute(Body);
                                }

                                break;
                            }
                        }
                    }
                }
            }
        }

        private bool CanUseAbility()
        {
            if (!Body.IsAlive ||
                Body.IsMezzed ||
                Body.IsStunned ||
                Body.IsSitting)
                return false;

            return true;
        }

        public bool SetGuard(GameLiving target, out bool ourEffect)
        {
            if (target != null)
            {
                GuardAbilityHandler.CheckExistingEffectsOnTarget(Body, target, true, out bool foundOurEffect, out GuardECSGameEffect existingEffectFromAnotherSource);

                ourEffect = foundOurEffect;

                if (foundOurEffect)
                    return false;

                if (existingEffectFromAnotherSource != null)
                    return false;

                GuardAbilityHandler.CancelOurEffectThenAddOnTarget(Body, target);

                return true;
            }

            ourEffect = false;
            return false;
        }

        #endregion AI

        #region MimicGroup AI

        #region MainPuller

        public void CheckPuller()
        {
            if (IsPulling && Body.TargetObject != null && Body.TargetObject.ObjectState == GameObject.eObjectState.Active)
            {
                if (CheckResetPuller())
                {
                    Body.ReturnToSpawnPoint(Body.MaxSpeed);

                    if ((MimicBody.CharacterClass.ID != (int)eCharacterClass.Hunter ||
                        MimicBody.CharacterClass.ID != (int)eCharacterClass.Ranger ||
                        MimicBody.CharacterClass.ID != (int)eCharacterClass.Scout) &&
                        MimicBody.CharacterClass.ClassType != eClassType.ListCaster)
                    {
                        if (MimicBody.MimicSpec.Is2H)
                            Body.SwitchWeapon(eActiveWeaponSlot.TwoHanded);
                        else
                            Body.SwitchWeapon(eActiveWeaponSlot.Standard);
                    }

                    return;
                }
            }

            if (!Body.InCombat)
            {
                if (CheckDelayPull())
                {
                    Body.StopAttack();
                    Body.StopFollowing();
                }
                else
                {
                    GameLiving pullTarget = GetPullTarget();
                    PerformPull(pullTarget);
                }
            }
        }

        public bool CheckDelayPull()
        {
            if (LastTargetObject != null && LastTargetObject.ObjectState == GameObject.eObjectState.Active)
                return true;

            if (CheckSpells(eCheckSpellType.Defensive) || MimicBody.Sit(CheckStats(75)))
                return true;

            if (Body.Group != null &&
                Body.Group.GetMembersInTheGroup().Any(groupMember => groupMember.IsCasting || groupMember.IsSitting))
                return true;

            return false;
        }

        public GameLiving GetPullTarget()
        {
            if (!Body.IsAttacking && !Body.IsCasting && !Body.IsSitting)
            {
                if (Body.Group.MimicGroup.CCTargets.Count > 0)
                    return Body.Group.MimicGroup.CCTargets[Util.Random(Body.Group.MimicGroup.CCTargets.Count - 1)];

                CheckProximityAggro(3600);

                if (AggroList.Count > 0)
                {
                    GameLiving closestTarget;

                    if (Body.Group.MimicGroup.PullFromPoint != null)
                        closestTarget = AggroList.Where(pair => Body.GetConLevel(pair.Key) >= Body.Group.MimicGroup.ConLevelFilter).
                                                   OrderBy(pair => pair.Key.GetDistance(Body.Group.MimicGroup.PullFromPoint)).
                                                   ThenBy(pair => Body.GetDistanceTo(pair.Key)).First().Key;
                    else
                        closestTarget = AggroList.Where(pair => Body.GetConLevel(pair.Key) > Body.Group.MimicGroup.ConLevelFilter).
                                                   OrderBy(pair => Body.GetDistanceTo(pair.Key)).First().Key;

                    return closestTarget;
                }
            }

            return null;
        }

        private bool CheckResetPuller()
        {
            if (Body.TargetObject is GameNPC npcTarget && npcTarget.Brain is StandardMobBrain mobBrain && mobBrain.HasAggro)
            {
                LastTargetObject = Body.TargetObject;
                IsPulling = false;
                Body.StopAttack();
                ClearAggroList();

                return true;
            }

            return false;
        }

        public void PerformPull(GameLiving target)
        {
            if (target == null)
                return;

            IsPulling = true;

            if (Body.Inventory.GetItem(eInventorySlot.DistanceWeapon) != null)
            {
                Body.SwitchWeapon(eActiveWeaponSlot.Distance);
                Body.StartAttack(target);
            }
            else
            {
                //if (Body.CanCastInstantHarmfulSpells)
                //{
                //    foreach(Spell spell in Body.InstantHarmfulSpells)

                //}
                //if (!Body.IsWithinRadius(Body.TargetObject, spell.Range))
                //{
                //    Body.Follow(Body.TargetObject, spell.Range - 100, 5000);
                //    QueuedOffensiveSpell = spell;
                //    return false;
                //}
            }
        }

        #endregion MainPuller

        #region MainLeader

        public bool CheckDelayRoam()
        {
            if (Body.IsCasting || CheckSpells(eCheckSpellType.Defensive) || MimicBody.Sit(CheckStats(75)))
                return true;

            if (Body.Group != null &&
                Body.Group.GetMembersInTheGroup().Any(groupMember => groupMember.IsCasting || groupMember.IsSitting || (groupMember is MimicNPC mimic &&
                                                      mimic.MimicBrain.FSM.GetCurrentState() == mimic.MimicBrain.FSM.GetState(eFSMStateType.FOLLOW_THE_LEADER) &&
                                                      !Body.IsWithinRadius(groupMember, 1000))))
                return true;

            return false;
        }

        #endregion MainLeader

        #region MainCC

        public void CheckMainCC()
        {
            if (Body.Group.MimicGroup.CCTargets.Count > 0)
            {
                if (CheckSpells(eCheckSpellType.CrowdControl))
                    return;
            }

            if (!Body.InCombat && Body.Group.MimicGroup.CCTargets.Count > 0)
            {
                Body.Group.MimicGroup.CCTargets = ValidateCCList(Body.Group.MimicGroup.CCTargets);
            }
        }

        // Test for bad lists. Might not be needed.
        private List<GameLiving> ValidateCCList(List<GameLiving> ccList)
        {
            List<GameLiving> validatedList = new List<GameLiving>();

            if (ccList.Count != 0)
            {
                foreach (GameLiving cc in ccList)
                {
                    if (cc is GameNPC npc && npc != null && npc.IsAlive && ((StandardMobBrain)npc.Brain).HasAggro)
                    {
                        validatedList.Add(cc);
                    }
                }
            }

            return validatedList;
        }

        #endregion MainCC

        #region MainTank

        public bool CheckMainTankTarget()
        {
            if (!IsMainTank)
                return false;

            GameLiving target = null;
            List<GameLiving> listOfTargets = null;

            if (AggroList.Count > 0)
            {
                listOfTargets = (AggroList.Keys.Where(key => key.TargetObject is GameLiving livingTarget && livingTarget != Body &&
                                                             !livingTarget.IsMezzed && !livingTarget.IsRooted).ToList());
            }

            if (listOfTargets != null && listOfTargets.Count > 0)
                target = listOfTargets[Util.Random(listOfTargets.Count - 1)];

            if (target != null)
            {
                Body.TargetObject = target;

                return true;
            }

            return false;
        }

        #endregion MainTank

        public bool CheckStats(short threshold)
        {
            if (Body.HealthPercent < threshold || (Body.MaxMana > 0 && Body.ManaPercent < threshold) || Body.EndurancePercent < threshold)
                return true;

            return false;
        }

        #endregion MimicGroup AI

        #region Aggro

        protected int _aggroRange;

        /// <summary>
        /// Max Aggro range in that this npc searches for enemies
        /// </summary>
        public virtual int AggroRange
        {
            get => Math.Min(_aggroRange, MAX_AGGRO_DISTANCE);
            set => _aggroRange = value;
        }

        /// <summary>
        /// Aggressive Level in % 0..100, 0 means not Aggressive
        /// </summary>
        public virtual int AggroLevel { get; set; }

        protected ConcurrentDictionary<GameLiving, AggroAmount> AggroList { get; } = new();
        protected List<(GameLiving, long)> OrderedAggroList { get; private set; } = [];
        public GameLiving LastHighestThreatInAttackRange { get; private set; }

        public class AggroAmount(long @base = 0)
        {
            public long Base { get; set; } = @base;
            public long Effective { get; set; }
            public long Temporary { get; set; }
        }

        /// <summary>
        /// Checks whether living has someone on its aggrolist
        /// </summary>
        public virtual bool HasAggro => !AggroList.IsEmpty;

        /// <summary>
        /// Add aggro table of this brain to that of another living.
        /// </summary>
        public void AddAggroListTo(StandardMobBrain brain)
        {
            if (!brain.Body.IsAlive)
                return;

            foreach (var pair in AggroList)
                brain.AddToAggroList(pair.Key, pair.Value.Base);
        }

        public virtual void AddToAggroList(GameLiving living, long aggroAmount)
        {
            if (Body.IsConfused || !Body.IsAlive || living == null)
                return;

            AggroList.AddOrUpdate(living, Add, Update, aggroAmount);

            if (living is IGamePlayer player)
            {
                // Add the whole group to the aggro list.
                if (player.Group != null)
                {
                    foreach (IGamePlayer playerInGroup in player.Group.GetIPlayersInTheGroup())
                    {
                        if (playerInGroup != living)
                            AggroList.TryAdd((GameLiving)playerInGroup, new());
                    }
                }

            }

            static AggroAmount Add(GameLiving key, long arg)
            {
                return new(Math.Max(0, arg));
            }

            static AggroAmount Update(GameLiving key, AggroAmount oldValue, long arg)
            {
                oldValue.Base = Math.Max(0, oldValue.Base + arg);
                return oldValue;
            }
        }

        public virtual void RemoveFromAggroList(GameLiving living)
        {
            AggroList.TryRemove(living, out _);
        }

        public List<(GameLiving, long)> GetOrderedAggroList()
        {
            // Potentially slow, so we cache the result.
            lock (((ICollection)OrderedAggroList).SyncRoot)
            {
                if (OrderedAggroList.Count == 0)
                    OrderedAggroList = AggroList.OrderByDescending(x => x.Value.Effective).Select(x => (x.Key, x.Value.Effective)).ToList();

                return OrderedAggroList.ToList();
            }
        }
        public long GetBaseAggroAmount(GameLiving living)
        {
            return AggroList.TryGetValue(living, out AggroAmount aggroAmount) ? aggroAmount.Base : 0;
        }

        /// <summary>
        /// Remove all livings from the aggrolist.
        /// </summary>
        public virtual void ClearAggroList()
        {
            AggroList.Clear();

            lock (((ICollection)OrderedAggroList).SyncRoot)
            {
                OrderedAggroList.Clear();
            }

            LastHighestThreatInAttackRange = null;
        }

        /// <summary>
        /// Selects and attacks the next target or does nothing.
        /// </summary>
        public virtual void AttackMostWanted()
        {
            if (!IsActive)
                return;

            //if (PvPMode || CheckAssist == null)

            if (!CheckMainTankTarget())
                Body.TargetObject = CalculateNextAttackTarget();

            if (Body.TargetObject != null)
            {
                if (Body.ControlledBrain != null)
                    Body.ControlledBrain.Attack(Body.TargetObject);

                if (!IsFleeing && CheckSpells(eCheckSpellType.Offensive))
                {
                    Body.StopAttack();
                }
                else
                {
                    CheckOffensiveAbilities();

                    if (MimicBody.CharacterClass.ClassType == eClassType.ListCaster && MimicBody.CharacterClass.ID != (int)eCharacterClass.Valewalker)
                    {
                        ECSGameAbilityEffect quickCast = EffectListService.GetAbilityEffectOnTarget(Body, eEffect.QuickCast);

                        if (quickCast != null)
                        {
                            CheckSpells(eCheckSpellType.Offensive);
                            return;
                        }

                        // Don't flee if in a group for now. Need better control over when and where and how.
                        if (Body.Group == null)
                        {
                            if ((TargetFleePosition == null && !IsFleeing && Body.IsBeingInterrupted && quickCast == null))
                            {
                                //TODO: Get dynamic distances based on circumstances. Maybe rethink the whole thing.
                                int fleeDistance = 2000 - Body.GetDistance(Body.TargetObject);

                                Flee(fleeDistance);

                                return;
                            }

                            if (Body.IsDestinationValid)
                                return;
                            else if (TargetFleePosition != null)
                            {
                                if (Body.GetDistance(TargetFleePosition) < 5)
                                {
                                    IsFleeing = false;
                                    TargetFleePosition = null;

                                    if (Body.IsWithinRadius(Body.TargetObject, 400))
                                    {
                                        Flee(1800);
                                        return;
                                    }

                                    if (Body.TargetObject != Body)
                                        Body.TurnTo(Body.TargetObject);
                                }
                            }
                        }

                        return;
                    }

                    if (Body.TargetObject != LastTargetObject)
                        ResetFlanking();

                    if (Body.ActiveWeapon?.Item_Type != (int)eInventorySlot.DistanceWeapon && Body.IsWithinRadius(Body.TargetObject, Body.attackComponent.AttackRange))
                    {
                        if (MimicBody.CanUsePositionalStyles && !IsMainTank && Body.ActiveWeapon != null)
                        {
                            if (Body.TargetObject is GameLiving livingTarget)
                            {
                                if (livingTarget.IsMoving || livingTarget.TargetObject == Body)
                                    ResetFlanking();

                                if (TargetFlankPosition == null && !IsFlanking && !livingTarget.IsMoving && livingTarget.TargetObject != Body)
                                {
                                    LastTargetObject = Body.TargetObject;
                                    TargetFlankPosition = GetStylePositionPoint(livingTarget, GetPositional());
                                    Body.StopFollowing();
                                    Body.StopAttack();
                                    Body.WalkTo(new Point3D(TargetFlankPosition.X, TargetFlankPosition.Y, livingTarget.Z), Body.MaxSpeed);
                                    return;
                                }

                                if (Body.IsDestinationValid)
                                {
                                    if (TargetFlankPosition == null)
                                        Body.Follow(Body.TargetObject, 75, 5000);
                                    else
                                        return;
                                }
                                else if (TargetFlankPosition != null)
                                {
                                    if (Body.GetDistance(TargetFlankPosition) < 5)
                                    {
                                        IsFlanking = true;
                                        TargetFlankPosition = null;
                                    }
                                }
                            }
                        }
                    }

                    if ((MimicBody.CharacterClass.ID == (int)eCharacterClass.Minstrel ||
                        (MimicBody.CharacterClass.ID == (int)eCharacterClass.Bard && Body.Group == null)) &&
                        Body.ActiveWeaponSlot != eActiveWeaponSlot.Standard)
                        Body.SwitchWeapon(eActiveWeaponSlot.Standard);

                    Body.StartAttack(Body.TargetObject);

                    LastTargetObject = Body.TargetObject;
                }
            }
        }

        private void Flee(int distance)
        {
            TargetFleePosition = GetFleePoint(distance);

            if (TargetFleePosition != null)
            {
                IsFleeing = true;
                MimicBody.Sprint(true);

                Body.PathTo(TargetFleePosition, Body.MaxSpeed);
            }
            else
            {
                IsFleeing = false;
            }
        }

        public void ResetFlanking()
        {
            IsFlanking = false;
            TargetFlankPosition = null;
        }

        private GameObject CheckAssist()
        {
            if (Body.Group != null && Body.Group.MimicGroup.MainAssist.InCombat)
            {
                GameObject assistTarget = Body.Group.MimicGroup.CurrentTarget;
                GameObject target = null;

                if (assistTarget != null && CanAggroTarget((GameLiving)assistTarget))
                    target = assistTarget;

                return target;
            }

            return null;

            //if (Body.Group != null)
            //{
            //    foreach (GameLiving groupMember in Body.Group.GetMembersInTheGroup())
            //    {
            //        if (groupMember is GameLiving living)
            //            foreach (var attacker in living.attackComponent.Attackers)
            //                AddToAggroList(attacker.Key, 1);
            //    }
            //}
        }

        private Point3D GetFleePoint(int fleeDistance)
        {
            ushort heading;

            if (Body.IsObjectInFront(Body.TargetObject, 120))
                heading = (ushort)(Body.Heading - 2048);
            else
                heading = Body.Heading;

            if (heading < 0)
                heading += 4096;

            if (heading > 4096)
                heading -= 4096;

            Point2D point = Body.GetPointFromHeading(heading, fleeDistance);

            if (Body.CurrentRegion.GetZone(point.X, point.Y) == null)
            {
                Point2D validPoint = null;

                for (int i = 0; i < 8; i++)
                {
                    heading += 512;

                    if (heading > 4096)
                        heading -= 4096;

                    validPoint = Body.GetPointFromHeading(heading, fleeDistance);

                    if (Body.CurrentRegion.GetZone(validPoint.X, validPoint.Y) != null)
                    {
                        point = validPoint;
                        break;
                    }
                }

                if (point == null)
                    return null;
            }

            if (PathingMgr.Instance.HasNavmesh(Body.CurrentZone))
            {
                Vector3? target = PathingMgr.Instance.GetClosestPointAsync(Body.CurrentZone, new Vector3(point.X, point.Y, Body.Z));

                if (target.HasValue)
                    return new Point3D(target.Value.X, target.Value.Y, target.Value.Z);
            }

            return new Point3D(point.X, point.Y, Body.Z);
        }

        private eOpeningPosition GetPositional()
        {
            eOpeningPosition positional = 0;

            if (MimicBody.CanUseSideStyles && MimicBody.CanUseBackStyles)
            {
                if (Util.RandomBool())
                    positional = eOpeningPosition.Back;
                else
                    positional = eOpeningPosition.Side;
            }
            else if (MimicBody.CanUseSideStyles)
                positional = eOpeningPosition.Side;
            else if (MimicBody.CanUseBackStyles)
                positional = eOpeningPosition.Back;

            return positional;
        }

        private Point2D GetStylePositionPoint(GameLiving living, eOpeningPosition positional)
        {
            ushort heading = 0;

            switch (positional)
            {
                case eOpeningPosition.Side:
                if (Util.RandomBool())
                    heading = (ushort)(living.Heading - 1024);
                else
                    heading = (ushort)(living.Heading + 1024);
                break;

                case eOpeningPosition.Back:
                heading = (ushort)(living.Heading - 2048);
                break;

                case eOpeningPosition.Front:
                heading = living.Heading;
                break;
            }

            if (heading < 0)
                heading += 4096;

            if (heading > 4096)
                heading -= 4096;

            Point2D point = living.GetPointFromHeading(heading, 75);

            return point;
        }

        private long _isHandlingAdditionToAggroListFromLosCheck;
        private bool StartAddToAggroListFromLosCheck => Interlocked.Exchange(ref _isHandlingAdditionToAggroListFromLosCheck, 1) == 0; // Returns true the first time it's called.


        protected virtual void LosCheckForAggroCallback(GamePlayer player, eLosCheckResponse response, ushort sourceOID, ushort targetOID)
        {
            // Make sure only one thread can enter this block to prevent multiple entities from being added to the aggro list.
            // Otherwise mobs could kill one player and immediately go for another one.
            if (response is eLosCheckResponse.TRUE && StartAddToAggroListFromLosCheck)
            {
                if (!HasAggro)
                {
                    GameObject gameObject = Body.CurrentRegion.GetObject(targetOID);

                    if (gameObject is GameLiving gameLiving)
                        AddToAggroList(gameLiving, 1);
                }

                _isHandlingAdditionToAggroListFromLosCheck = 0;
            }
        }

        protected virtual bool ShouldBeRemovedFromAggroList(GameLiving living)
        {
            // Keep Necromancer shades so that we can attack them if their pets die.
            return !living.IsAlive ||
                   living.ObjectState != GameObject.eObjectState.Active ||
                   living.IsStealthed ||
                   living.CurrentRegion != Body.CurrentRegion ||
                   !Body.IsWithinRadius(living, MAX_AGGRO_LIST_DISTANCE) ||
                   (!GameServer.ServerRules.IsAllowedToAttack(Body, living, true) && !living.effectListComponent.ContainsEffectForEffectType(eEffect.Shade));

        }

        protected virtual bool ShouldBeIgnoredFromAggroList(GameLiving living)
        {
            // We're keeping shades in the aggro list so that mobs attack them after their pet dies, so they need to be filtered out here.
            return living.effectListComponent.ContainsEffectForEffectType(eEffect.Shade);
        }

        protected virtual GameLiving CleanUpAggroListAndGetHighestModifiedThreat()

        {
            // Clear cached ordered aggro list.
            // It isn't built here because ordering all entities in the aggro list can be expensive, and we typically don't need it.
            // It's built on demand, when `GetOrderedAggroList` is called.
            OrderedAggroList.Clear();

            int attackRange = Body.attackComponent.AttackRange;
            GameLiving highestThreat = null;
            KeyValuePair<GameLiving, AggroAmount> currentTarget = default;
            long highestEffectiveAggro = -1; // Assumes that negative aggro amounts aren't allowed in the list.
            long highestEffectiveAggroInAttackRange = -1; // Assumes that negative aggro amounts aren't allowed in the list.

            foreach (var pair in AggroList)
            {
                GameLiving living = pair.Key;

                if (Body.TargetObject == living)
                    currentTarget = pair;

                if (ShouldBeRemovedFromAggroList(living))
                {
                    AggroList.TryRemove(living, out _);
                    continue;
                }

                if (ShouldBeIgnoredFromAggroList(living))
                    continue;

                // Livings further than `EFFECTIVE_AGGRO_AMOUNT_CALCULATION_DISTANCE_THRESHOLD` units away have a reduced effective aggro amount.
                // Using `Math.Ceiling` helps differentiate between 0 and 1 base aggro amount.
                AggroAmount aggroAmount = pair.Value;
                double distance = Body.GetDistanceTo(living);
                aggroAmount.Effective = distance > EFFECTIVE_AGGRO_AMOUNT_CALCULATION_DISTANCE_THRESHOLD ?
                                        (long)Math.Ceiling(aggroAmount.Base * (EFFECTIVE_AGGRO_AMOUNT_CALCULATION_DISTANCE_THRESHOLD / distance)) :
                                        aggroAmount.Base;

                if (aggroAmount.Effective > highestEffectiveAggroInAttackRange)
                {
                    if (distance <= attackRange)
                    {
                        highestEffectiveAggroInAttackRange = aggroAmount.Effective;
                        LastHighestThreatInAttackRange = living;
                    }

                    if (aggroAmount.Effective > highestEffectiveAggro)
                    {
                        highestEffectiveAggro = aggroAmount.Effective;
                        highestThreat = living;
                    }
                }
            }

            if (highestThreat != null)
            {
                // Don't change target if our new found highest threat has the same effective aggro.
                // This helps with BAF code to make mobs actually go to their intended target.
                if (currentTarget.Key != null && currentTarget.Key != highestThreat && currentTarget.Value.Effective >= highestEffectiveAggro)
                    highestThreat = currentTarget.Key;
            }
            else
            {
                // The list seems to be full of shades. It could mean we added a shade to the aggro list instead of its pet.
                // Ideally, this should never happen, but it currently can be caused by the way `AddToAggroList` propagates aggro to group members.
                // When that happens, don't bother checking aggro amount and simply return the first pet in the list.
                return AggroList.FirstOrDefault().Key?.ControlledBrain?.Body;
            }

            return highestThreat;
        }

        /// <summary>
        /// Returns the best target to attack from the current aggro list.
        /// </summary>
        protected virtual GameLiving CalculateNextAttackTarget()
        {
            return CleanUpAggroListAndGetHighestModifiedThreat();
        }

        public virtual bool CanAggroTarget(GameLiving target)
        {
            if (!GameServer.ServerRules.IsAllowedToAttack(Body, target, true))
                return false;

            // Get owner if target is pet or subpet
            GameLiving realTarget = target;

            if (realTarget is GameNPC npcTarget && npcTarget.Brain is IControlledBrain npcTargetBrain)
                realTarget = npcTargetBrain.GetLivingOwner();

            // Only attack if target is green+
            if (Body.IsObjectGreyCon(realTarget))
                return false;

            if (!PvPMode && FSM.GetCurrentState() == FSM.GetState(eFSMStateType.ROAMING))
            {
                ConColor conLimit = (ConColor)Body.GetConLevel(realTarget);

                if (conLimit >= ConColor.PURPLE)
                    return false;

                if (Body.Group == null && conLimit >= ConColor.ORANGE)
                    return false;

                if (realTarget is GameNPC npc && npc.Brain is StandardMobBrain brain && brain.HasAggro)
                    return false;
            }

            if (realTarget is IGamePlayer && realTarget.Realm != Body.Realm)
                return true;

            if (realTarget is GameKeepGuard && realTarget.Realm != Body.Realm)
                return true;

            if (realTarget is GameNPC && realTarget is not MimicNPC && realTarget is not GameKeepGuard && PvPMode)
                return false;

            // We put this here to prevent aggroing non-factions npcs
            return (Body.Realm != eRealm.None || realTarget is not GameNPC) && AggroLevel > 0;
        }

        public virtual void OnAttackedByEnemy(AttackData ad)
        {
            if (!Body.IsAlive || Body.ObjectState != GameObject.eObjectState.Active)
                return;

            if (FSM.GetCurrentState() == FSM.GetState(eFSMStateType.PASSIVE))
                return;

            ConvertDamageToAggroAmount(ad.Attacker, Math.Max(1, ad.Damage + ad.CriticalDamage));

            if (!Body.attackComponent.AttackState && FSM.GetCurrentState() != FSM.GetState(eFSMStateType.AGGRO))
            {
                FSM.SetCurrentState(eFSMStateType.AGGRO);
                Think();
            }

            if (Body.Group != null)
            {
                foreach (GameLiving groupMember in Body.Group.GetMembersInTheGroup())
                {
                    if (groupMember is MimicNPC mimic && groupMember != Body)
                        ((MimicBrain)mimic.Brain).OnGroupMemberAttacked(ad);
                }
            }
        }

        /// <summary>
        /// Converts a damage amount into an aggro amount, and splits it between the pet and its owner if necessary.
        /// Assumes damage to be superior than 0.
        /// </summary>
        protected virtual void ConvertDamageToAggroAmount(GameLiving attacker, int damage)
        {
            if (attacker is GameNPC NpcAttacker && NpcAttacker.Brain is ControlledMobBrain controlledBrain)
            {
                damage = controlledBrain.ModifyDamageWithTaunt(damage);

                // Aggro is split between the owner (15%) and their pet (85%).
                int aggroForOwner = (int)(damage * 0.15);

                // We must ensure that the same amount of aggro isn't added for both, otherwise an out-of-combat mob could attack the owner when their pet engages it.
                // The owner must also always generate at least 1 aggro.
                // This works as long as the split isn't 50 / 50.
                if (aggroForOwner == 0)
                {
                    AddToAggroList(controlledBrain.Owner, 1);
                    AddToAggroList(NpcAttacker, Math.Max(2, damage));
                }
                else
                {
                    AddToAggroList(controlledBrain.Owner, aggroForOwner);
                    AddToAggroList(NpcAttacker, damage - aggroForOwner);
                }
            }
            else
                AddToAggroList(attacker, damage);
        }

        #endregion Aggro

        #region Spells

        public enum eCheckSpellType
        {
            Offensive,
            Defensive,
            CrowdControl
        }

        /// <summary>
        /// Checks if any spells need casting
        /// </summary>
        /// <param name="type">Which type should we go through and check for?</param>
        public virtual bool CheckSpells(eCheckSpellType type)
        {
            if (Body == null || Body.Spells == null || Body.Spells.Count <= 0)
                return false;

            bool casted = false;
            List<Spell> spellsToCast = new();

            // Healers should heal whether in combat or out of it.
            if (!casted && Body.CanCastHealSpells)
            {
                GameLiving livingToHeal = null;

                int healThreshold = Properties.NPC_HEAL_THRESHOLD;
                int emergencyThreshold = healThreshold / 2;

                short numNeedHealing = 0;
                bool singleEmergency = false;
                bool groupEmergency = false;

                if (Body.Group != null)
                {
                    short healthPercent = 100;
                    short numEmergency = 0;

                    foreach (GameLiving groupMember in Body.Group.GetMembersInTheGroup())
                    {
                        if (groupMember.HealthPercent < healThreshold)
                        {
                            if (groupMember.HealthPercent < healthPercent)
                            {
                                healthPercent = groupMember.HealthPercent;
                                livingToHeal = groupMember;
                                numNeedHealing++;

                                if (groupMember.HealthPercent < emergencyThreshold)
                                    numEmergency++;
                            }
                        }
                    }

                    if (numEmergency == 1)
                        singleEmergency = true;
                    else if (numEmergency > Body.Group.GetMembersInTheGroup().Count / 2)
                        groupEmergency = true;
                }
                else
                {
                    if (Body.HealthPercent < healThreshold)
                    {
                        if (Body.HealthPercent < emergencyThreshold)
                            singleEmergency = true;

                        livingToHeal = Body;
                    }
                }

                Spell spellTocast;

                if ((singleEmergency || groupEmergency) && Body.CanCastInstantHealSpells)
                    spellTocast = Body.InstantHealSpells[Util.Random(Body.InstantHealSpells.Count - 1)];
                else
                {
                    Spell cureDisease = null;

                    if (livingToHeal != null && livingToHeal.IsDiseased && (cureDisease = Body.HealSpells.FirstOrDefault(spell => spell.SpellType == eSpellType.CureDisease)) != null)
                    {
                        spellTocast = cureDisease;
                    }
                    else
                        spellTocast = Body.HealSpells[Util.Random(Body.HealSpells.Count - 1)];
                }

                casted = CheckHealSpells(spellTocast, numNeedHealing, singleEmergency, groupEmergency, livingToHeal);
            }

            if (!casted && type == eCheckSpellType.CrowdControl)
            {
                if (MimicBody.CanCastCrowdControlSpells)
                {
                    Body.TargetObject = MimicBody.Group.MimicGroup.CCTargets[Util.Random(MimicBody.Group.MimicGroup.CCTargets.Count - 1)];

                    foreach (Spell spell in MimicBody.CrowdControlSpells)
                    {
                        if (CanCastOffensiveSpell(spell) && !LivingHasEffect((GameLiving)Body.TargetObject, spell))
                            spellsToCast.Add(spell);
                    }

                    if (spellsToCast.Count > 0)
                    {
                        Spell spell = spellsToCast[Util.Random(spellsToCast.Count - 1)];

                        casted = Body.CastSpell(spell, m_mobSpellLine);

                        if (casted)
                            MimicBody.Group.MimicGroup.CCTargets.Remove((GameLiving)Body.TargetObject);
                    }
                }
            }
            else if (!casted && type == eCheckSpellType.Defensive)
            {
                if (Body.CanCastMiscSpells)
                    casted = CheckDefensiveSpells(Body.MiscSpells);
                //if (Body.CanCastMiscSpells)
                //{
                //    foreach (Spell spell in Body.MiscSpells)
                //    {
                //        if (CheckDefensiveSpells(spell))
                //        {
                //            casted = true;
                //            break;
                //        }
                //    }
                //}
            }
            else if (!casted && type == eCheckSpellType.Offensive)
            {
                if (MimicBody.CharacterClass.ID == (int)eCharacterClass.Cleric)
                {
                    if (!Util.Chance(Math.Max(5, Body.ManaPercent - 50)))
                        return false;
                }

                // Check instant spells, but only cast one to prevent spamming
                if (Body.CanCastInstantHarmfulSpells)
                {
                    foreach (Spell spell in Body.InstantHarmfulSpells)
                    {
                        if (CheckInstantOffensiveSpells(spell))
                            break;
                    }
                }

                if (Body.CanCastInstantMiscSpells)
                {
                    foreach (Spell spell in Body.InstantMiscSpells)
                    {
                        if (CheckInstantDefensiveSpells(spell))
                            break;
                    }
                }

                // TODO: Better nightshade casting logic. For now just make them melee but still use instants.
                if (MimicBody.CharacterClass.ID == (int)eCharacterClass.Nightshade)
                    return false;

                // TODO: This makes Thane and Valewalker use melee when in range rather than cast in all situations.
                //        but still use instants. Need to include other exceptions like maybe low health or endurance.
                if ((MimicBody.CanUsePositionalStyles || MimicBody.CanUseAnytimeStyles) && (Body.IsWithinRadius(Body.TargetObject, 550) || Body.ManaPercent <= 10))
                    return false;

                if (MimicBody.CanCastCrowdControlSpells)
                {
                    int ccChance = 50;

                    GameLiving livingTarget = Body.TargetObject as GameLiving;

                    if (livingTarget?.TargetObject == Body && Body.IsWithinRadius(Body.TargetObject, 500))
                        ccChance = 95;

                    if (Body.Group?.MimicGroup.CurrentTarget == Body.TargetObject)
                        ccChance = 0;

                    if (Util.Chance(ccChance))
                    {
                        foreach (Spell spell in MimicBody.CrowdControlSpells)
                        {
                            // Prevent Minstrel from spamming their AoE mez for now.
                            if (spell.CastTime < 5)
                                if (CanCastOffensiveSpell(spell) && !LivingHasEffect((GameLiving)Body.TargetObject, spell))
                                    spellsToCast.Add(spell);
                        }
                    }
                }

                if (MimicBody.CanCastBolts && spellsToCast.Count < 1)
                {
                    foreach (Spell spell in MimicBody.BoltSpells)
                    {
                        if (CanCastOffensiveSpell(spell))
                            spellsToCast.Add(spell);
                    }
                }

                if (spellsToCast.Count < 1)
                {
                    if (Body.CanCastHarmfulSpells)
                    {
                        foreach (Spell spell in Body.HarmfulSpells)
                        {
                            if (spell.SpellType == eSpellType.Charm ||
                                spell.SpellType == eSpellType.Amnesia ||
                                spell.SpellType == eSpellType.Confusion ||
                                spell.SpellType == eSpellType.Taunt)
                                continue;

                            if (CanCastOffensiveSpell(spell))
                                spellsToCast.Add(spell);
                        }
                    }
                }

                if (spellsToCast.Count > 0)
                {
                    Spell spellToCast = spellsToCast[Util.Random(spellsToCast.Count - 1)];

                    if (spellToCast.Uninterruptible || !Body.IsBeingInterrupted)
                        casted = CheckOffensiveSpells(spellToCast);
                    else if (!spellToCast.Uninterruptible && Body.IsBeingInterrupted)
                    {
                        if (MimicBody.CharacterClass.ClassType == eClassType.ListCaster)
                        {
                            Ability quickCast = Body.GetAbility(Abilities.Quickcast);

                            if (quickCast != null)
                            {
                                if (Body.GetSkillDisabledDuration(quickCast) <= 0)
                                {
                                    // Give mimics a small bump in duration, they don't use it as well as humans.
                                    new QuickCastECSGameEffect(new ECSGameEffectInitParams(Body, QuickCastECSGameEffect.DURATION + 1000, 1));
                                    Body.DisableSkill(quickCast, 180000);

                                    casted = CheckOffensiveSpells(spellToCast);
                                }
                            }
                        }
                    }
                }
            }

            return casted || Body.IsCasting;
        }

        protected bool CanCastOffensiveSpell(Spell spell)
        {
            if (Body.GetSkillDisabledDuration(spell) <= 0)
            {
                if (spell.CastTime > 0)
                {
                    if (spell.Target is eSpellTarget.ENEMY or eSpellTarget.AREA or eSpellTarget.CONE)
                        return true;
                }
            }

            return false;
        }

        protected bool CanCastDefensiveSpell(Spell spell)
        {
            if (spell == null || spell.IsHarmful)
                return false;

            // Make sure we're currently able to cast the spell.
            if (spell.CastTime > 0 && Body.IsBeingInterrupted && !spell.Uninterruptible)
                return false;

            // Make sure the spell isn't disabled.
            if (Body.GetSkillDisabledDuration(spell) > 0)
                return false;

            return true;
        }

        protected bool CheckHealSpells(Spell spell, short numNeedHealing, bool singleEmergency, bool groupEmergency, GameLiving livingToHeal)
        {
            if (!CanCastDefensiveSpell(spell))
                return false;

            GameObject lastTarget = Body.TargetObject;
            Body.TargetObject = null;

            if (livingToHeal != null)
            {
                switch (spell.SpellType)
                {
                    case eSpellType.CureDisease:
                    case eSpellType.CombatHeal:
                    case eSpellType.Heal:
                    case eSpellType.HealOverTime:
                    case eSpellType.MercHeal:
                    case eSpellType.OmniHeal:
                    case eSpellType.PBAoEHeal:
                    case eSpellType.SpreadHeal:

                    if (spell.IsInstantCast)
                    {
                        if (Body.IsWithinRadius(livingToHeal, spell.Range))
                            Body.TargetObject = livingToHeal;
                        break;
                    }

                    if (spell.Target == eSpellTarget.GROUP && numNeedHealing < 2)
                        break;

                    if (spell.Target == eSpellTarget.SELF && numNeedHealing < 2)
                        break;

                    if (!LivingHasEffect(livingToHeal, spell) && Body.IsWithinRadius(livingToHeal, spell.Range))
                    {
                        Body.TargetObject = livingToHeal;
                        break;
                    }

                    break;
                }
            }

            if (Body.TargetObject != null)
            {
                //log.Info("Tried to cast " + spell.Name + " " + spell.SpellType.ToString());
                Body.CastSpell(spell, m_mobSpellLine);
                return true;
            }

            Body.TargetObject = lastTarget;
            return false;
        }

        /// <summary>
        /// Checks defensive spells. Handles buffs, heals, etc.
        /// </summary>
        protected bool CheckDefensiveSpells(Spell spell)
        {
            if (!CanCastDefensiveSpell(spell))
                return false;

            bool casted = false;

            Body.TargetObject = null;

            // TODO: Instrument classes need special logic.
            if (spell.NeedInstrument)
            {
                return false;
                switch (spell.SpellType)
                {
                    case eSpellType.PowerRegenBuff:
                    {
                        if (!Body.InCombat && !Body.IsMoving)
                        {
                            if (Body.Group != null)
                            {
                                if (Body.Group.GetMembersInTheGroup().Any(groupMember => groupMember.MaxMana > 0 && groupMember.ManaPercent < 80) && !LivingHasEffect(Body, spell))
                                {
                                    Body.SwitchWeapon(eActiveWeaponSlot.Distance);
                                    Body.TargetObject = Body;
                                }
                            }
                            else if (Body.ManaPercent < 75 && !LivingHasEffect(Body, spell))
                            {
                                Body.SwitchWeapon(eActiveWeaponSlot.Distance);
                                Body.TargetObject = Body;
                            }
                            else if (LivingHasEffect(Body, spell))
                            {
                                Body.TargetObject = Body;
                            }
                        }
                    }
                    break;

                    case eSpellType.HealthRegenBuff:

                    if (!Body.InCombat && !Body.IsMoving && !LivingHasEffect(Body, spell))
                    {
                        ECSGameEffect powerRegen = EffectListService.GetEffectOnTarget(Body, eEffect.Pulse, eSpellType.PowerRegenBuff);

                        if (powerRegen == null)
                        {
                            if (!Body.InCombat && !Body.IsMoving)
                            {
                                if (Body.Group != null)
                                {
                                    if (Body.Group.GetMembersInTheGroup().Any(groupMember => groupMember.HealthPercent < 80) && !LivingHasEffect(Body, spell))
                                    {
                                        Body.SwitchWeapon(eActiveWeaponSlot.Distance);
                                        Body.TargetObject = Body;
                                    }
                                }
                                else if (Body.HealthPercent < 80 && !LivingHasEffect(Body, spell))
                                {
                                    Body.SwitchWeapon(eActiveWeaponSlot.Distance);
                                    Body.TargetObject = Body;
                                }
                                else if (LivingHasEffect(Body, spell))
                                {
                                    Body.TargetObject = Body;
                                }
                            }
                        }
                    }
                    break;

                    case eSpellType.EnduranceRegenBuff:

                    if (Body.InCombat)
                    {
                        if (Body.Group != null)
                        {
                            if (Body.Group.GetMembersInTheGroup().Any(groupMember => groupMember.EndurancePercent < 95) && !LivingHasEffect(Body, spell))
                            {
                                Body.SwitchWeapon(eActiveWeaponSlot.Distance);
                                Body.TargetObject = Body;
                            }
                        }
                    }

                    break;

                    case eSpellType.SpeedEnhancement:
                    {
                        if (!Body.InCombat && Body.IsMoving && !LivingHasEffect(Body, spell))
                        {
                            Body.SwitchWeapon(eActiveWeaponSlot.Distance);
                            Body.TargetObject = Body;
                        }
                    }
                    break;
                }
            }
            //else
            //{
            switch (spell.SpellType)
            {
                #region Summon

                case eSpellType.SummonMinion:

                if (Body.ControlledBrain != null)
                {
                    IControlledBrain[] icb = Body.ControlledBrain.Body.ControlledNpcList;
                    int numberofpets = 0;

                    for (int i = 0; i < icb.Length; i++)
                    {
                        if (icb[i] != null)
                            numberofpets++;
                    }

                    if (numberofpets >= icb.Length)
                        break;

                    int cumulativeLevel = 0;

                    foreach (var petBrain in Body.ControlledBrain.Body.ControlledNpcList)
                    {
                        cumulativeLevel += petBrain != null && petBrain.Body != null ? petBrain.Body.Level : 0;
                    }

                    byte newpetlevel = (byte)(Body.Level * spell.Damage * -0.01);

                    if (newpetlevel > spell.Value)
                        newpetlevel = (byte)spell.Value;

                    if (cumulativeLevel + newpetlevel > 75)
                        break;

                    Body.TargetObject = Body;
                }

                break;

                case eSpellType.SummonCommander:
                case eSpellType.SummonUnderhill:
                case eSpellType.SummonDruidPet:
                case eSpellType.SummonSimulacrum:
                case eSpellType.SummonSpiritFighter:

                if (Body.ControlledBrain != null)
                    break;

                Body.TargetObject = Body;

                break;

                case eSpellType.PetSpell:
                break;

                #endregion Summon

                #region Pulse

                case eSpellType.SpeedEnhancement when spell.IsPulsing:

                if (!LivingHasEffect(Body, spell))
                    Body.TargetObject = Body;

                break;

                case eSpellType.Bladeturn when spell.IsPulsing:
                break;

                case eSpellType.MesmerizeDurationBuff when spell.IsPulsing:
                break;

                #endregion Pulse

                #region Buffs
                case eSpellType.WaterBreathing:
                break;

                case eSpellType.SpeedEnhancement when spell.IsInstantCast:
                break;

                case eSpellType.SpeedEnhancement when spell.Target == eSpellTarget.PET:
                case eSpellType.CombatSpeedBuff when spell.Duration > 20:
                case eSpellType.CombatSpeedBuff when spell.IsConcentration:
                case eSpellType.MesmerizeDurationBuff when !spell.IsPulsing:
                case eSpellType.Bladeturn when !spell.IsPulsing:
                case eSpellType.BodySpiritEnergyBuff:
                case eSpellType.HeatColdMatterBuff:
                case eSpellType.SpiritResistBuff:
                case eSpellType.EnergyResistBuff:
                case eSpellType.HeatResistBuff:
                case eSpellType.ColdResistBuff:
                case eSpellType.BodyResistBuff:
                case eSpellType.MatterResistBuff:
                case eSpellType.AllMagicResistBuff:
                case eSpellType.EnduranceRegenBuff:
                case eSpellType.PowerRegenBuff:
                case eSpellType.AblativeArmor:
                case eSpellType.AcuityBuff:
                case eSpellType.AFHitsBuff:
                case eSpellType.ArmorAbsorptionBuff:
                case eSpellType.ArmorFactorBuff:
                case eSpellType.Buff:
                case eSpellType.CelerityBuff:
                case eSpellType.ConstitutionBuff:
                case eSpellType.CourageBuff:
                case eSpellType.CrushSlashTrustBuff:
                case eSpellType.DexterityBuff:
                case eSpellType.DexterityQuicknessBuff:
                case eSpellType.EffectivenessBuff:
                case eSpellType.FatigueConsumptionBuff:
                case eSpellType.FlexibleSkillBuff:
                case eSpellType.HasteBuff:
                case eSpellType.HealthRegenBuff:
                case eSpellType.HeroismBuff:
                case eSpellType.KeepDamageBuff:
                case eSpellType.MagicResistBuff:
                case eSpellType.MeleeDamageBuff:
                case eSpellType.MLABSBuff:
                case eSpellType.PaladinArmorFactorBuff:
                case eSpellType.ParryBuff:
                case eSpellType.PowerHealthEnduranceRegenBuff:
                case eSpellType.StrengthBuff:
                case eSpellType.StrengthConstitutionBuff:
                case eSpellType.SuperiorCourageBuff:
                case eSpellType.ToHitBuff:
                case eSpellType.WeaponSkillBuff:
                case eSpellType.DamageAdd:
                case eSpellType.OffensiveProc:
                case eSpellType.DefensiveProc:
                case eSpellType.DamageShield:
                case eSpellType.OffensiveProcPvE:
                case eSpellType.BothAblativeArmor:
                {
                    if (spell.IsConcentration)
                    {
                        if (spell.Concentration > Body.Concentration)
                            break;

                        if (Body.effectListComponent.ConcentrationEffects.Count >= 20)
                            break;
                    }

                    if (spell.Target == eSpellTarget.PET)
                    {
                        // TODO: Add logic for damage shield use
                        if (spell.SpellType == eSpellType.DamageShield)
                            return false;

                        if (Body.ControlledBrain?.Body != null)
                        {
                            if (!LivingHasEffect(Body.ControlledBrain.Body, spell))
                                Body.TargetObject = Body.ControlledBrain.Body;
                        }

                        break;
                    }

                    // Buff self
                    if (!LivingHasEffect(Body, spell))
                    {
                        Body.TargetObject = Body;
                        break;
                    }

                    if (Body.Group != null)
                    {
                        if (spell.Target == eSpellTarget.REALM || spell.Target == eSpellTarget.GROUP)
                        {
                            foreach (GameLiving groupMember in Body.Group.GetMembersInTheGroup())
                            {
                                if (groupMember != Body)
                                {
                                    if (!LivingHasEffect(groupMember, spell) && Body.IsWithinRadius(groupMember, spell.Range) && groupMember.IsAlive)
                                    {
                                        Body.TargetObject = groupMember;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                break;

                #endregion Buffs

                #region Cure Disease/Poison/Mezz

                case eSpellType.CureDisease:

                //Cure self
                if (Body.IsDiseased)
                {
                    Body.TargetObject = Body;
                    break;
                }

                // Cure group members
                if (Body.Group != null)
                {
                    foreach (GameLiving groupMember in Body.Group.GetMembersInTheGroup())
                    {
                        if (groupMember != Body)
                        {
                            if (groupMember.IsDiseased && Body.IsWithinRadius(groupMember, spell.Range))
                            {
                                Body.TargetObject = groupMember;
                                break;
                            }
                        }
                    }
                }
                break;

                case eSpellType.CurePoison:
                //Cure self
                if (Body.IsPoisoned)
                {
                    Body.TargetObject = Body;
                    break;
                }

                // Cure group members
                if (Body.Group != null)
                {
                    foreach (GameLiving groupMember in Body.Group.GetMembersInTheGroup())
                    {
                        if (groupMember != Body)
                        {
                            if (groupMember.IsPoisoned && Body.IsWithinRadius(groupMember, spell.Range))
                            {
                                Body.TargetObject = groupMember;
                                break;
                            }
                        }
                    }
                }
                break;

                case eSpellType.CureMezz:
                if (Body.Group != null)
                {
                    foreach (GameLiving groupMember in Body.Group.GetMembersInTheGroup())
                    {
                        if (groupMember != Body)
                        {
                            if (groupMember.IsMezzed && Body.IsWithinRadius(groupMember, spell.Range))
                            {
                                Body.TargetObject = groupMember;
                                break;
                            }
                        }
                    }
                }
                break;

                case eSpellType.CureNearsightCustom:
                if (Body.Group != null)
                {
                    foreach (GameLiving groupMember in Body.Group.GetMembersInTheGroup())
                    {
                        if (groupMember != Body)
                        {
                            if (LivingHasEffect(groupMember, spell) && Body.IsWithinRadius(groupMember, spell.Range))
                            {
                                Body.TargetObject = groupMember;
                                break;
                            }
                        }
                    }
                }
                break;

                #endregion Cure Disease/Poison/Mezz

                #region Charms

                case eSpellType.Charm:
                break;

                #endregion Charms

                case eSpellType.Resurrect:

                if (Body.Group != null)
                {
                    foreach (GameLiving groupMember in Body.Group.GetMembersInTheGroup())
                    {
                        if (!groupMember.IsAlive && Body.IsWithinRadius(groupMember, spell.Range))
                        {
                            Body.TargetObject = groupMember;
                            break;
                        }
                    }
                }
                break;

                case eSpellType.LifeTransfer:

                if (Body.Group != null)
                {
                    if (Body.HealthPercent > 50)
                    {
                        GameLiving livingToHeal = null;
                        int threshold = Properties.NPC_HEAL_THRESHOLD / 2;
                        int lowestHealth = 100;

                        foreach (GameLiving groupMember in Body.Group.GetMembersInTheGroup())
                        {
                            if (groupMember.HealthPercent < threshold)
                            {
                                if (groupMember.HealthPercent < lowestHealth)
                                {
                                    livingToHeal = groupMember;
                                    lowestHealth = groupMember.HealthPercent;
                                }
                            }
                        }

                        if (livingToHeal != null && livingToHeal.IsAlive)
                            Body.TargetObject = livingToHeal;
                    }
                }

                break;

                case eSpellType.PetConversion:
                break;

                case eSpellType.FocusShell:
                break;

                default:
                log.Warn($"CheckDefensiveSpells() encountered an unknown spell type [{spell.SpellType}] for {Body?.Name}");
                break;
            }

            if (Body?.TargetObject != null)
            {
                //log.Info(Body.Name + " tried to cast " + spell.Name + " " + spell.SpellType.ToString() + " on " + Body.TargetObject.Name);
                //log.Info(Body.TargetObject.Name + " effect is " + LivingHasEffect((GameLiving)Body.TargetObject, spell));

                casted = Body.CastSpell(spell, m_mobSpellLine);
            }

            return casted;
        }

        bool CheckDefensiveSpells(List<Spell> spells)
        {
            // Contrary to offensive spells, we don't start with a valid target.
            // So the idea here is to find a target, switch before calling `CastDefensiveSpell`, then retrieve our previous target.
            List<(Spell, GameLiving)> spellsToCast = new(spells.Count);

            foreach (Spell spell in spells)
            {
                if (CanCastDefensiveSpell(spell, out GameLiving target))
                    spellsToCast.Add((spell, target));
            }

            if (spellsToCast.Count == 0)
                return false;

            GameObject oldTarget = Body.TargetObject;
            (Spell spell, GameLiving target) spellToCast = spellsToCast[Util.Random(spellsToCast.Count - 1)];
            Body.TargetObject = spellToCast.target;
            bool cast = Body.CastSpell(spellToCast.spell, m_mobSpellLine);

            if (Debug)
            {
                if (cast)
                    log.Info(Body.Name + " tried to cast " + spellToCast.spell.Name + " on " + spellToCast.target.Name + " and cast == true");
                else
                    log.Info(Body.Name + " tried to cast " + spellToCast.spell.Name + " on " + spellToCast.target.Name + " and cast == false");

                if (LivingHasEffect(spellToCast.target, spellToCast.spell))
                    log.Info(spellToCast.target.Name + " has the effect already.");
            }

            Body.TargetObject = oldTarget;
            return cast;

            bool CanCastDefensiveSpell(Spell spell, out GameLiving target)
            {
                target = null;

                // TODO: Handle instrument spells
                if (spell.NeedInstrument || (!spell.Uninterruptible && Body.IsBeingInterrupted) ||
                    (spell.HasRecastDelay && Body.GetSkillDisabledDuration(spell) > 0))
                {
                    return false;
                }

                target = FindTargetForDefensiveSpell(spell);
                return target != null;
            }
        }

        protected virtual GameLiving FindTargetForDefensiveSpell(Spell spell)
        {
            GameLiving target = null;

            switch (spell.SpellType)
            {
                #region Pulse

                case eSpellType.SpeedEnhancement when spell.IsPulsing:

                if (!LivingHasEffect(Body, spell))
                    target = Body;

                break;

                case eSpellType.Bladeturn when spell.IsPulsing:
                break;

                case eSpellType.MesmerizeDurationBuff when spell.IsPulsing:
                break;

                #endregion Pulse

                #region Buffs

                // TODO: WaterBreathing and Druid BothAblative and Healer Celerity
                case eSpellType.WaterBreathing:
                case eSpellType.BothAblativeArmor when spell.Duration <= 15:
                case eSpellType.CombatSpeedBuff when spell.Duration <= 20:
                break;

                case eSpellType.SpeedEnhancement when spell.IsInstantCast:
                break;

                case eSpellType.SpeedEnhancement when spell.IsPulsing:
                case eSpellType.SpeedEnhancement when spell.Target == eSpellTarget.PET:
                case eSpellType.CombatSpeedBuff when spell.Duration > 20:
                case eSpellType.CombatSpeedBuff when spell.IsConcentration:
                case eSpellType.MesmerizeDurationBuff when !spell.IsPulsing:
                case eSpellType.Bladeturn when !spell.IsPulsing:

                case eSpellType.AcuityBuff:
                case eSpellType.AFHitsBuff:
                case eSpellType.AllMagicResistBuff:
                case eSpellType.ArmorAbsorptionBuff:
                case eSpellType.ArmorFactorBuff:
                case eSpellType.PaladinArmorFactorBuff:
                case eSpellType.BodyResistBuff:
                case eSpellType.BodySpiritEnergyBuff:
                case eSpellType.Buff:
                case eSpellType.CelerityBuff:
                case eSpellType.ColdResistBuff:
                case eSpellType.CombatSpeedBuff:
                case eSpellType.ConstitutionBuff:
                case eSpellType.CourageBuff:
                case eSpellType.CrushSlashTrustBuff:
                case eSpellType.DexterityBuff:
                case eSpellType.DexterityQuicknessBuff:
                case eSpellType.EffectivenessBuff:
                case eSpellType.EnduranceRegenBuff:
                case eSpellType.EnergyResistBuff:
                case eSpellType.FatigueConsumptionBuff:
                case eSpellType.FlexibleSkillBuff:
                case eSpellType.HasteBuff:
                case eSpellType.HealthRegenBuff:
                case eSpellType.HeatColdMatterBuff:
                case eSpellType.HeatResistBuff:
                case eSpellType.HeroismBuff:
                case eSpellType.KeepDamageBuff:
                case eSpellType.MagicResistBuff:
                case eSpellType.MatterResistBuff:
                case eSpellType.MeleeDamageBuff:
                case eSpellType.MesmerizeDurationBuff:
                case eSpellType.MLABSBuff:
                case eSpellType.ParryBuff:
                case eSpellType.PowerHealthEnduranceRegenBuff:
                case eSpellType.PowerRegenBuff:
                case eSpellType.SavageCombatSpeedBuff:
                case eSpellType.SavageCrushResistanceBuff:
                case eSpellType.SavageDPSBuff:
                case eSpellType.SavageParryBuff:
                case eSpellType.SavageSlashResistanceBuff:
                case eSpellType.SavageThrustResistanceBuff:
                case eSpellType.SpiritResistBuff:
                case eSpellType.StrengthBuff:
                case eSpellType.StrengthConstitutionBuff:
                case eSpellType.SuperiorCourageBuff:
                case eSpellType.ToHitBuff:
                case eSpellType.WeaponSkillBuff:
                case eSpellType.DamageAdd:
                case eSpellType.OffensiveProc:
                case eSpellType.DefensiveProc:
                case eSpellType.DamageShield:
                case eSpellType.Bladeturn:
                case eSpellType.OffensiveProcPvE:
                {
                    // TODO: PBAoE Buffs
                    if (spell.IsPBAoE)
                        break;

                    if (spell.IsConcentration)
                    {
                        if (spell.Concentration > Body.Concentration)
                            break;

                        if (Body.effectListComponent.ConcentrationEffects.Count >= 20)
                            break;
                    }

                    if (!LivingHasEffect(Body, spell) && !Body.attackComponent.AttackState && spell.Target != eSpellTarget.PET)
                    {
                        target = Body;
                        break;
                    }

                    if (Body.ControlledBrain != null && Body.ControlledBrain.Body != null && Body.GetDistanceTo(Body.ControlledBrain.Body) <= spell.Range && !LivingHasEffect(Body.ControlledBrain.Body, spell) && spell.Target != eSpellTarget.SELF)
                    {
                        if (spell.SpellType == eSpellType.DamageShield)
                            break;

                        target = Body.ControlledBrain.Body;
                        break;
                    }

                    if (Body.Group != null)
                    {
                        if (spell.Target == eSpellTarget.REALM || spell.Target == eSpellTarget.GROUP)
                        {
                            foreach (GameLiving groupMember in Body.Group.GetMembersInTheGroup())
                            {
                                if (groupMember != Body)
                                {
                                    if (!LivingHasEffect(groupMember, spell) && !Body.attackComponent.AttackState && groupMember.IsAlive)
                                    {
                                        target = groupMember;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    break;
                }

                #endregion Buffs

                #region Summon

                case eSpellType.Summon:
                {
                    target = Body;
                    break;
                }

                case eSpellType.SummonMinion:
                {
                    if (Body.ControlledBrain != null)
                    {
                        IControlledBrain[] icb = Body.ControlledBrain.Body.ControlledNpcList;
                        int numberofpets = 0;

                        for (int i = 0; i < icb.Length; i++)
                        {
                            if (icb[i] != null)
                                numberofpets++;
                        }

                        if (numberofpets >= icb.Length)
                            break;

                        int cumulativeLevel = 0;

                        foreach (var petBrain in Body.ControlledBrain.Body.ControlledNpcList)
                        {
                            cumulativeLevel += petBrain != null && petBrain.Body != null ? petBrain.Body.Level : 0;
                        }

                        byte newpetlevel = (byte)(Body.Level * spell.Damage * -0.01);

                        if (newpetlevel > spell.Value)
                            newpetlevel = (byte)spell.Value;

                        if (cumulativeLevel + newpetlevel > 75)
                            break;

                        target = Body;
                    }

                    break;
                }

                #endregion Summon

                #region Heals

                case eSpellType.CombatHeal:
                case eSpellType.Heal:
                case eSpellType.HealOverTime:
                case eSpellType.MercHeal:
                case eSpellType.OmniHeal:
                case eSpellType.PBAoEHeal:
                case eSpellType.SpreadHeal:
                {
                    if (Body.ControlledBrain != null && Body.ControlledBrain.Body != null
                        && Body.GetDistanceTo(Body.ControlledBrain.Body) <= spell.Range
                        && Body.ControlledBrain.Body.HealthPercent < Properties.NPC_HEAL_THRESHOLD
                        && spell.Target != eSpellTarget.SELF)
                    {
                        target = Body.ControlledBrain.Body;
                        break;
                    }

                    break;
                }

                #endregion

                case eSpellType.SummonCommander:
                case eSpellType.SummonDruidPet:
                case eSpellType.SummonHunterPet:
                case eSpellType.SummonNecroPet:
                case eSpellType.SummonUnderhill:
                case eSpellType.SummonSimulacrum:
                case eSpellType.SummonSpiritFighter:
                {
                    if (Body.ControlledBrain != null)
                        break;

                    target = Body;
                    break;
                }

                case eSpellType.Resurrect:
                {
                    if (Body.Group != null)
                    {
                        foreach (GameLiving groupMember in Body.Group.GetMembersInTheGroup())
                        {
                            if (!groupMember.IsAlive)
                            {
                                target = groupMember;
                                break;
                            }
                        }
                    }

                    break;
                }

                default:
                break;
            }

            if (target != null)
                target = HandleExceptions(target, spell);

            return target;
        }

        private GameLiving HandleExceptions(GameLiving target, Spell spell)
        {
            if (target is IGamePlayer playerTarget)
            {
                switch (spell.SpellType)
                {
                    case eSpellType.AcuityBuff:
                    {
                        if (playerTarget.CharacterClass.ClassType != eClassType.ListCaster)
                            target = null;

                        break;
                    }

                    case eSpellType.StrengthBuff when playerTarget.CharacterClass.ID != (int)eCharacterClass.Valewalker:
                    case eSpellType.ArmorFactorBuff:
                    {
                        if (spell.IsConcentration && playerTarget.CharacterClass.ClassType == eClassType.ListCaster)
                            target = null;

                        break;
                    }

                    default:
                    break;
                }
            }

            return target;
        }

        /// <summary>
        /// Checks offensive spells.  Handles dds, debuffs, etc.
        /// </summary>
        protected virtual bool CheckOffensiveSpells(Spell spell, bool quickCast = false)
        {
            //if (spell.NeedInstrument && Body.ActiveWeaponSlot != eActiveWeaponSlot.Distance)
                //Body.SwitchWeapon(eActiveWeaponSlot.Distance);

            bool casted = false;

            if (Body.TargetObject is GameLiving living && (spell.Duration == 0 || !LivingHasEffect(living, spell) || spell.SpellType == eSpellType.DirectDamageWithDebuff || spell.SpellType == eSpellType.DamageSpeedDecrease))
            {
                casted = Body.CastSpell(spell, m_mobSpellLine);

                //log.Info(Body.Name + " tried to cast " + spell.Name + " " + spell.SpellType.ToString() + " on " + Body.TargetObject.Name);
                //log.Info(Body.TargetObject.Name + " effect is " + LivingHasEffect((GameLiving)Body.TargetObject, spell));
            }

            return casted;
        }

        protected virtual bool CheckInstantDefensiveSpells(Spell spell)
        {
            if (spell.HasRecastDelay && Body.GetSkillDisabledDuration(spell) > 0)
                return false;

            bool castSpell = false;

            switch (spell.SpellType)
            {
                // TODO: Stealth archer using speed to get away or attack
                //case eSpellType.SpeedEnhancement:

                case eSpellType.SavageCrushResistanceBuff:
                case eSpellType.SavageSlashResistanceBuff:
                case eSpellType.SavageThrustResistanceBuff:
                case eSpellType.SavageCombatSpeedBuff:
                case eSpellType.SavageDPSBuff:
                case eSpellType.SavageParryBuff:
                case eSpellType.SavageEvadeBuff:

                if (spell.SpellType == eSpellType.SavageCrushResistanceBuff ||
                    spell.SpellType == eSpellType.SavageSlashResistanceBuff ||
                    spell.SpellType == eSpellType.SavageThrustResistanceBuff &&
                    !CheckSavageResistSpell(spell.SpellType))
                    break;

                if (!LivingHasEffect(Body, spell))
                    castSpell = true;

                break;

                case eSpellType.BodySpiritEnergyBuff:
                case eSpellType.HeatColdMatterBuff:
                case eSpellType.SpiritResistBuff:
                case eSpellType.EnergyResistBuff:
                case eSpellType.HeatResistBuff:
                case eSpellType.ColdResistBuff:
                case eSpellType.BodyResistBuff:
                case eSpellType.MatterResistBuff:
                {
                    // Temp to stop Paladins/Skalds from spamming.
                    // TODO: Smarter use of resist chants.
                    if (spell.Pulse > 0)
                        break;

                    break;
                }

                case eSpellType.CombatHeal:
                case eSpellType.DamageAdd:
                case eSpellType.ArmorFactorBuff:
                case eSpellType.DexterityQuicknessBuff:
                case eSpellType.EnduranceRegenBuff:
                case eSpellType.CombatSpeedBuff:
                case eSpellType.AblativeArmor:
                case eSpellType.Bladeturn:
                case eSpellType.OffensiveProc:
                case eSpellType.SummonHunterPet:

                if (spell.SpellType == eSpellType.CombatSpeedBuff)
                {
                    if (Body.TargetObject != null && !Body.IsWithinRadius(Body.TargetObject, Body.MeleeAttackRange))
                        break;
                }

                if (!LivingHasEffect(Body, spell))
                    castSpell = true;

                break;
            }

            if (castSpell)
                Body.CastSpell(spell, m_mobSpellLine);

            return castSpell;
        }

        /// <summary>
        /// Checks Instant Spells.  Handles Taunts, shouts, stuns, etc.
        /// </summary>
        protected virtual bool CheckInstantOffensiveSpells(Spell spell)
        {
            if (spell.HasRecastDelay && Body.GetSkillDisabledDuration(spell) > 0)
                return false;

            bool castSpell = false;

            switch (spell.SpellType)
            {
                #region Enemy Spells

                case eSpellType.Taunt:

                if (Body.Group?.MimicGroup.MainTank == Body)
                    castSpell = true;

                break;

                case eSpellType.DirectDamage:
                case eSpellType.NightshadeNuke:
                case eSpellType.Lifedrain:
                case eSpellType.DexterityDebuff:
                case eSpellType.DexterityQuicknessDebuff:
                case eSpellType.StrengthDebuff:
                case eSpellType.StrengthConstitutionDebuff:
                case eSpellType.CombatSpeedDebuff:
                case eSpellType.DamageOverTime:
                case eSpellType.MeleeDamageDebuff:
                case eSpellType.AllStatsPercentDebuff:
                case eSpellType.CrushSlashThrustDebuff:
                case eSpellType.EffectivenessDebuff:
                case eSpellType.Disease:
                case eSpellType.Stun:
                case eSpellType.Mez:
                case eSpellType.Mesmerize:

                if (spell.IsPBAoE && !Body.IsWithinRadius(Body.TargetObject, spell.Radius))
                    break;

                // Try to limit the debuffs cast to save mana and time spent doing so.
                if (MimicBody.CharacterClass.ClassType == eClassType.ListCaster)
                {
                    if (!Util.Chance(Math.Max(5, Body.ManaPercent - 75)))
                        break;
                }

                if (!LivingHasEffect((GameLiving)Body.TargetObject, spell) && Body.IsWithinRadius(Body.TargetObject, spell.Range))
                    castSpell = true;

                break;

                #endregion Enemy Spells
            }

            ECSGameEffect pulseEffect = EffectListService.GetPulseEffectOnTarget(Body, spell);

            if (pulseEffect != null)
                return false;

            if (castSpell)
            {
                Body.CastSpell(spell, m_mobSpellLine);
                return true;
            }

            return false;
        }

        protected virtual bool CheckSavageResistSpell(eSpellType spellType)
        {
            eDamageType damageType = eDamageType.Natural;

            switch (spellType)
            {
                case eSpellType.SavageCrushResistanceBuff:
                damageType = eDamageType.Crush;
                break;

                case eSpellType.SavageSlashResistanceBuff:
                damageType = eDamageType.Slash;
                break;

                case eSpellType.SavageThrustResistanceBuff:
                damageType = eDamageType.Thrust;
                break;
            }

            if (Body.attackComponent.Attackers.Count > 0)
            {
                foreach (var attacker in Body.attackComponent.Attackers)
                {
                    if (attacker.Key.ActiveWeapon != null)
                    {
                        if (attacker.Key.ActiveWeapon.Type_Damage != 0 && (int)damageType == attacker.Key.ActiveWeapon.Type_Damage)
                            return true;
                    }
                    else if (attacker.Key is GameNPC npc)
                    {
                        if (npc.MeleeDamageType == damageType)
                            return true;
                    }
                }
            }

            return false;
        }

        protected static SpellLine m_mobSpellLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells);
        //protected static SpellLine m_MimicSpellLine = SkillBase.GetSpellLine("MimicSpellLine");

        /// <summary>
        /// Checks if the living target has a spell effect.
        /// Only to be used for spell casting purposes.
        /// </summary>
        /// <returns>True if the living has the effect of will receive it by our current spell.</returns>
        public bool LivingHasEffect(GameLiving target, Spell spell)
        {
            if (target == null)
                return true;

            eEffect spellEffect = EffectService.GetEffectFromSpell(spell);

            // Ignore effects that aren't actually effects (may be incomplete).
            if (spellEffect is eEffect.DirectDamage or eEffect.Pet or eEffect.Unknown)
                return false;

            ISpellHandler spellHandler = Body.castingComponent.SpellHandler;

            // If we're currently casting 'spell' on 'target', assume it already has the effect.
            // This allows spell queuing while preventing casting on the same target more than once.
            if (spellHandler != null && spellHandler.Spell.ID == spell.ID && spellHandler.Target == target)
                return true;

            ISpellHandler queuedSpellHandler = Body.castingComponent.QueuedSpellHandler;

            // Do the same for our queued up spell.
            // This can happen on charmed pets having two buffs that they're trying to cast on their owner.
            if (queuedSpellHandler != null && queuedSpellHandler.Spell.ID == spell.ID && queuedSpellHandler.Target == target)
                return true;

            // May not be the right place for that, but without that check NPCs with more than one offensive or defensive proc will only buff themselves once.
            if (spell.SpellType is eSpellType.OffensiveProc or eSpellType.DefensiveProc)
            {
                if (target.effectListComponent.Effects.TryGetValue(EffectService.GetEffectFromSpell(spell), out List<ECSGameEffect> existingEffects))
                {
                    if (existingEffects.FirstOrDefault(e => e.SpellHandler.Spell.ID == spell.ID || (spell.EffectGroup > 0 && e.SpellHandler.Spell.EffectGroup == spell.EffectGroup)) != null)
                        return true;
                }

                return false;
            }

            ECSGameEffect pulseEffect = EffectListService.GetPulseEffectOnTarget(target, spell);

            if (pulseEffect != null)
                return true;

            // True if the target has the effect, or the immunity effect for this effect.
            // Treat NPC immunity effects as full immunity effects.
            return EffectListService.GetEffectOnTarget(target, spellEffect) != null || HasImmunityEffect(EffectService.GetImmunityEffectFromSpell(spell)) || HasImmunityEffect(EffectService.GetNpcImmunityEffectFromSpell(spell));

            bool HasImmunityEffect(eEffect immunityEffect)
            {
                return immunityEffect != eEffect.Unknown && EffectListService.GetEffectOnTarget(target, immunityEffect) != null;
            }
        }

        #endregion Spells

        #region DetectDoor

        public virtual void DetectDoor()
        {
            ushort range = (ushort)(ThinkInterval / 800 * Body.CurrentWaypoint.MaxSpeed);

            foreach (GameDoorBase door in Body.CurrentRegion.GetDoorsInRadius(Body, range))
            {
                if (door is GameKeepDoor)
                {
                    if (Body.Realm != door.Realm)
                        return;

                    door.Open();
                    //Body.Say("GameKeep Door is near by");
                    //somebody can insert here another action for GameKeep Doors
                    return;
                }
                else
                {
                    door.Open();
                    return;
                }
            }

            return;
        }

        #endregion DetectDoor
    }
}