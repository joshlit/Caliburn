﻿using DOL.AI.Brain;
using DOL.Database;
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
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using static DOL.GS.GameObject;

namespace DOL.GS
{
    public class AttackComponent : IManagedEntity
    {
        public const int CHECK_ATTACKERS_INTERVAL = 1000;
        public const double INHERENT_WEAPON_SKILL = 15.0;
        public const double INHERENT_ARMOR_FACTOR = 12.5;

        public GameLiving owner;
        public WeaponAction weaponAction;
        public AttackAction attackAction;
        public EntityManagerId EntityManagerId { get; set; } = new(EntityManager.EntityType.AttackComponent, false);

        /// <summary>
        /// Returns the list of attackers
        /// </summary>
        public ConcurrentDictionary<GameLiving, long> Attackers { get; } = new();

        private AttackersCheckTimer _attackersCheckTimer;
        private BlockRoundHandler _blockRoundHandler;

        public void AddAttacker(AttackData attackData)
        {
            if (attackData.Attacker == owner)
                return;

            lock (_attackersCheckTimer.Lock)
            {
                if (!_attackersCheckTimer.IsAlive)
                {
                    _attackersCheckTimer.Interval = CHECK_ATTACKERS_INTERVAL;
                    _attackersCheckTimer.Start();
                }
            }

            long duration = attackData.Interval;

            if (duration <= 0)
                duration = Properties.SPELL_INTERRUPT_DURATION;

            Attackers.AddOrUpdate(attackData.Attacker, Add, Update, GameLoop.GameLoopTime + duration);

            static long Add(GameLiving key, long arg)
            {
                return arg;
            }

            static long Update(GameLiving key, long oldValue, long arg)
            {
                return arg;
            }
        }

        /// <summary>
        /// The target that was passed when 'StartAttackRequest' was called and the request accepted.
        /// </summary>
        private GameObject _startAttackTarget;

        /// <summary>
        /// Actually a boolean. Use 'StartAttackRequested' to preserve thread safety.
        /// </summary>
        private int _startAttackRequested;

        public bool StartAttackRequested
        {
            get => Interlocked.CompareExchange(ref _startAttackRequested, 0, 0) == 1;
            set => Interlocked.Exchange(ref _startAttackRequested, Convert.ToInt32(value));
        }

        public AttackComponent(GameLiving owner)
        {
            this.owner = owner;
            attackAction = AttackAction.Create(owner);
            _attackersCheckTimer = AttackersCheckTimer.Create(owner);
            _blockRoundHandler = new(owner);
        }

        public void Tick()
        {
            if (StartAttackRequested)
            {
                StartAttackRequested = false;
                StartAttack();
            }

            if (!attackAction.Tick())
                EntityManager.Remove(this);
        }

        /// <summary>
        /// The chance for a critical hit
        /// </summary>
        public int CalculateCriticalChance(WeaponAction action)
        {
            switch (owner.ActiveWeaponSlot)
            {
                default:
                case eActiveWeaponSlot.Standard:
                case eActiveWeaponSlot.TwoHanded:
                    return owner.GetModified(eProperty.CriticalMeleeHitChance);
                case eActiveWeaponSlot.Distance:
                    return action?.RangedAttackType is eRangedAttackType.Critical ? 0 : owner.GetModified(eProperty.CriticalArcheryHitChance);
            }
        }

        public DbInventoryItem GetAttackAmmo()
        {
            // Returns the ammo used by the current `WeaponAction` if there's any.
            // The currently active ammo otherwise.
            DbInventoryItem ammo = weaponAction?.Ammo;
            ammo ??= owner.rangeAttackComponent.Ammo;
            return ammo;
        }

        /// <summary>
        /// Returns the damage type of the current attack
        /// </summary>
        /// <param name="weapon">attack weapon</param>
        public eDamageType AttackDamageType(DbInventoryItem weapon)
        {
            IGamePlayer playerOwner = owner as IGamePlayer;

            if (playerOwner != null || owner is CommanderPet)
            {
                if (weapon == null)
                    return eDamageType.Natural;

                switch ((eObjectType)weapon.Object_Type)
                {
                    case eObjectType.Crossbow:
                    case eObjectType.Longbow:
                    case eObjectType.CompositeBow:
                    case eObjectType.RecurvedBow:
                    case eObjectType.Fired:
                    {
                        DbInventoryItem ammo = GetAttackAmmo();
                        return (eDamageType) (ammo == null ? weapon.Type_Damage : ammo.Type_Damage);
                    }
                    case eObjectType.Shield:
                    return eDamageType.Crush; // TODO: shields do crush damage (!) best is if Type_Damage is used properly
                    default:
                    return (eDamageType)weapon.Type_Damage;
                }
            }
            else if (owner is GameNPC npcOwner)
                return npcOwner.MeleeDamageType;
            else
                return eDamageType.Natural;
        }

        private bool _attackState;

        public virtual bool AttackState
        {
            get => _attackState || StartAttackRequested;
            set => _attackState = value;
        }

        /// <summary>
        /// Gets which weapon was used for the last dual wield attack
        /// 0: right (or non dual wield user), 1: left, 2: both
        /// </summary>
        public int UsedHandOnLastDualWieldAttack { get; set; }

        /// <summary>
        /// Returns this attack's range
        /// </summary>
        public int AttackRange
        {
            /* tested with:
            staff					= 125-130
            sword			   		= 126-128.06
            shield (Numb style)		= 127-129
            polearm	(Impale style)	= 127-130
            mace (Daze style)		= 127.5-128.7
            Think it's safe to say that it never changes; different with mobs. */

            get
            {
                if (owner is IGamePlayer player)
                {
                    DbInventoryItem weapon = owner.ActiveWeapon;

                    if (weapon == null)
                        return 0;

                    GameLiving target = player.TargetObject as GameLiving;

                    // TODO: Change to real distance of bows.
                    if (weapon.SlotPosition == (int)eInventorySlot.DistanceWeapon)
                    {
                        double range;

                        switch ((eObjectType)weapon.Object_Type)
                        {
                            case eObjectType.Longbow:
                            range = 1760;
                            break;

                            case eObjectType.RecurvedBow:
                            range = 1680;
                            break;

                            case eObjectType.CompositeBow:
                            range = 1600;
                            break;

                            case eObjectType.Thrown:
                            range = 1160;
                            if (weapon.Name.ToLower().Contains("weighted"))
                                range = 1450;
                            break;

                            default:
                            range = 1200;
                            break; // Shortbow, crossbow, throwing.
                        }

                        range = Math.Max(32, range * player.GetModified(eProperty.ArcheryRange) * 0.01);
                        DbInventoryItem ammo = GetAttackAmmo();

                        if (ammo != null)
                            switch ((ammo.SPD_ABS >> 2) & 0x3)
                            {
                                case 0:
                                range *= 0.85;
                                break; // Clout -15%
                                //case 1:
                                //  break; // (none) 0%
                                case 2:
                                range *= 1.15;
                                break; // Doesn't exist on live
                                case 3:
                                range *= 1.25;
                                break; // Flight +25%
                            }

                        if (target != null)
                            range += Math.Min((owner.Z - target.Z) / 2.0, 500);
                        if (range < 32)
                            range = 32;

                        return (int)range;
                    }

                    return owner.MeleeAttackRange;
                }
                else
                {
                    return owner.ActiveWeaponSlot is eActiveWeaponSlot.Distance
                        ? Math.Max(32, (int) (2000.0 * owner.GetModified(eProperty.ArcheryRange) * 0.01))
                        : owner.MeleeAttackRange;
                }
            }
        }

        /// <summary>
        /// Gets the current attackspeed of this living in milliseconds
        /// </summary>
        /// <returns>effective speed of the attack. average if more than one weapon.</returns>
        public int AttackSpeed(DbInventoryItem mainWeapon, DbInventoryItem leftWeapon = null)
        {
            if (owner is IGamePlayer player)
            {
                if (mainWeapon == null)
                    return 0;

                double speed = 0;
                bool bowWeapon = false;

                // If leftWeapon is null even on a dual wield attack, use the mainWeapon instead
                switch (UsedHandOnLastDualWieldAttack)
                {
                    case 2:
                    speed = mainWeapon.SPD_ABS;
                    if (leftWeapon != null)
                    {
                        speed += leftWeapon.SPD_ABS;
                        speed /= 2;
                    }
                    break;

                    case 1:
                    speed = leftWeapon != null ? leftWeapon.SPD_ABS : mainWeapon.SPD_ABS;
                    break;

                    case 0:
                    speed = mainWeapon.SPD_ABS;
                    break;
                }

                if (speed == 0)
                    return 0;

                switch (mainWeapon.Object_Type)
                {
                    case (int)eObjectType.Fired:
                    case (int)eObjectType.Longbow:
                    case (int)eObjectType.Crossbow:
                    case (int)eObjectType.RecurvedBow:
                    case (int)eObjectType.CompositeBow:
                    bowWeapon = true;
                    break;
                }

                int qui = Math.Min(250, player.Quickness); //250 soft cap on quickness

                if (bowWeapon)
                {
                    if (Properties.ALLOW_OLD_ARCHERY)
                    {
                        //Draw Time formulas, there are very many ...
                        //Formula 2: y = iBowDelay * ((100 - ((iQuickness - 50) / 5 + iMasteryofArcheryLevel * 3)) / 100)
                        //Formula 1: x = (1 - ((iQuickness - 60) / 500 + (iMasteryofArcheryLevel * 3) / 100)) * iBowDelay
                        //Table a: Formula used: drawspeed = bowspeed * (1-(quickness - 50)*0.002) * ((1-MoA*0.03) - (archeryspeedbonus/100))
                        //Table b: Formula used: drawspeed = bowspeed * (1-(quickness - 50)*0.002) * (1-MoA*0.03) - ((archeryspeedbonus/100 * basebowspeed))

                        //For now use the standard weapon formula, later add ranger haste etc.
                        speed *= (1.0 - (qui - 60) * 0.002);
                        double percent = 0;
                        // Calcul ArcherySpeed bonus to substract
                        percent = speed * 0.01 * owner.GetModified(eProperty.ArcherySpeed);
                        // Apply RA difference
                        speed -= percent;
                        //log.Debug("speed = " + speed + " percent = " + percent + " eProperty.archeryspeed = " + GetModified(eProperty.ArcherySpeed));

                        if (owner.rangeAttackComponent.RangedAttackType is eRangedAttackType.Critical)
                            speed = speed * 2 - (player.GetAbilityLevel(Abilities.Critical_Shot) - 1) * speed / 10;
                        else if (owner.rangeAttackComponent.RangedAttackType is eRangedAttackType.RapidFire)
                            speed *= RangeAttackComponent.RAPID_FIRE_ATTACK_SPEED_MODIFIER;
                    }
                    else
                    {
                        // no archery bonus
                        speed *= (1.0 - (qui - 60) * 0.002);
                    }
                }
                else
                {
                    // TODO use haste
                    //Weapon Speed*(1-(Quickness-60)/500]*(1-Haste)
                    speed *= ((1.0 - (qui - 60) * 0.002) * 0.01 * owner.GetModified(eProperty.MeleeSpeed));
                    //Console.WriteLine($"Speed after {speed} quiMod {(1.0 - (qui - 60) * 0.002)} melee speed {0.01 * p.GetModified(eProperty.MeleeSpeed)} together {(1.0 - (qui - 60) * 0.002) * 0.01 * p.GetModified(eProperty.MeleeSpeed)}");
                }

                return (int) Math.Max(1500, speed * 100);
            }
            else
            {
                double speed = NpcWeaponSpeed(mainWeapon) * 100 * (1.0 - (owner.GetModified(eProperty.Quickness) - 60) / 500.0);

                if (owner is GameSummonedPet pet)
                {
                    if (pet != null)
                    {
                        switch (pet.Name)
                        {
                            case "amber simulacrum": speed *= (owner.GetModified(eProperty.MeleeSpeed) * 0.01) * 1.45; break;
                            case "emerald simulacrum": speed *= (owner.GetModified(eProperty.MeleeSpeed) * 0.01) * 1.45; break;
                            case "ruby simulacrum": speed *= (owner.GetModified(eProperty.MeleeSpeed) * 0.01) * 0.95; break;
                            case "sapphire simulacrum": speed *= (owner.GetModified(eProperty.MeleeSpeed) * 0.01) * 0.95; break;
                            case "jade simulacrum": speed *= (owner.GetModified(eProperty.MeleeSpeed) * 0.01) * 0.95; break;
                            default: speed *= owner.GetModified(eProperty.MeleeSpeed) * 0.01; break;
                        }
                        //return (int)speed;
                    }
                }
                else
                {
                    if (owner.ActiveWeaponSlot is eActiveWeaponSlot.Distance)
                    {
                        // Old archery uses archery speed, but new archery uses casting speed
                        if (Properties.ALLOW_OLD_ARCHERY)
                            speed *= 1.0 - owner.GetModified(eProperty.ArcherySpeed) * 0.01;
                        else
                            speed *= 1.0 - owner.GetModified(eProperty.CastingSpeed) * 0.01;
                    }
                    else
                    {
                        speed *= owner.GetModified(eProperty.MeleeSpeed) * 0.01;
                    }
                }

                return (int) Math.Max(500, speed);
            }
        }

        /// <summary>
        /// InventoryItem.SPD_ABS isn't set for NPCs, so this method must be used instead.
        /// </summary>
        public static int NpcWeaponSpeed(DbInventoryItem weapon)
        {
            return weapon?.SlotPosition switch
            {
                Slot.TWOHAND => 40,
                Slot.RANGED => 45,
                _ => 30,
            };
        }

        public double AttackDamage(DbInventoryItem weapon, out double damageCap)
        {
            double effectiveness = 1;
            damageCap = 0;

            if (owner is IGamePlayer player)
            {
                if (weapon == null)
                    return 0;

                damageCap = player.WeaponDamageWithoutQualityAndCondition(weapon) * weapon.SPD_ABS * 0.1 * CalculateSlowWeaponDamageModifier(weapon);

                if (weapon.Item_Type == Slot.RANGED)
                {
                    damageCap *= CalculateTwoHandedDamageModifier(weapon);
                    DbInventoryItem ammo = GetAttackAmmo();

                    if (ammo != null)
                    {
                        switch ((ammo.SPD_ABS) & 0x3)
                        {
                            case 0:
                            damageCap *= 0.85;
                            break; // Blunt (light) -15%.
                            case 1:
                            break; // Bodkin (medium) 0%.
                            case 2:
                            damageCap *= 1.15;
                            break; // Doesn't exist on live.
                            case 3:
                            damageCap *= 1.25;
                            break; // Broadhead (X-heavy) +25%.
                        }
                    }

                    if ((eObjectType) weapon.Object_Type is eObjectType.Longbow or eObjectType.RecurvedBow or eObjectType.CompositeBow)
                    {
                        if (Properties.ALLOW_OLD_ARCHERY)
                            effectiveness += owner.GetModified(eProperty.RangedDamage) * 0.01;
                        else
                        {
                            effectiveness += owner.GetModified(eProperty.RangedDamage) * 0.01;
                            effectiveness += owner.GetModified(eProperty.SpellDamage) * 0.01;
                        }
                    }
                    else
                        effectiveness += owner.GetModified(eProperty.RangedDamage) * 0.01;
                }
                else if (weapon.Item_Type is Slot.RIGHTHAND or Slot.LEFTHAND or Slot.TWOHAND)
                {
                    effectiveness += owner.GetModified(eProperty.MeleeDamage) * 0.01;

                    if (weapon.Item_Type == Slot.TWOHAND)
                        damageCap *= CalculateTwoHandedDamageModifier(weapon);
                    else if (player.ActiveLeftWeapon != null)
                        damageCap *= CalculateLeftAxeModifier();
                }

                damageCap *= effectiveness;
                double damage = GamePlayer.ApplyWeaponQualityAndConditionToDamage(weapon, damageCap);
                damageCap *= 3;
                return damage;
            }
            else
            {
                double damage = (1.0 + owner.Level / Properties.PVE_MOB_DAMAGE_F1 + owner.Level * owner.Level / Properties.PVE_MOB_DAMAGE_F2) * NpcWeaponSpeed(weapon) * 0.1;

                if (owner is GameNPC npc)
                    damage *= npc.DamageFactor;

                if (weapon == null ||
                    weapon.SlotPosition == Slot.RIGHTHAND ||
                    weapon.SlotPosition == Slot.LEFTHAND ||
                    weapon.SlotPosition == Slot.TWOHAND)
                {
                    effectiveness += owner.GetModified(eProperty.MeleeDamage) * 0.01;
                }
                else if (weapon.SlotPosition is Slot.RANGED)
                {
                    if ((eObjectType) weapon.Object_Type is eObjectType.Longbow or eObjectType.RecurvedBow or eObjectType.CompositeBow)
                    {
                        if (Properties.ALLOW_OLD_ARCHERY)
                            effectiveness += owner.GetModified(eProperty.RangedDamage) * 0.01;
                        else
                        {
                            effectiveness += owner.GetModified(eProperty.RangedDamage) * 0.01;
                            effectiveness += owner.GetModified(eProperty.SpellDamage) * 0.01;
                        }
                    }
                    else
                        effectiveness += owner.GetModified(eProperty.RangedDamage) * 0.01;
                }

                damage *= effectiveness;
                damageCap = damage * 3;

                if (owner is GameEpicBoss)
                    damageCap *= Properties.SET_EPIC_ENCOUNTER_WEAPON_DAMAGE_CAP;

                return damage;
            }
        }

        public void RequestStartAttack(GameObject attackTarget = null)
        {
            _startAttackTarget = attackTarget ?? owner.TargetObject;
            StartAttackRequested = true;
            EntityManager.Add(this);
        }

        private void StartAttack()
        {
            if (owner is GamePlayer player)
            {
                if (!player.CharacterClass.StartAttack(_startAttackTarget))
                    return;

                if (!player.IsAlive)
                {
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.StartAttack.YouCantCombat"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                    return;
                }

                // Necromancer with summoned pet cannot attack.
                if (player.ControlledBrain?.Body is NecromancerPet)
                {
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.StartAttack.CantInShadeMode"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                    return;
                }

                if (player.IsStunned)
                {
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.StartAttack.CantAttackStunned"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                    return;
                }

                if (player.IsMezzed)
                {
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.StartAttack.CantAttackmesmerized"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                    return;
                }

                long vanishTimeout = player.TempProperties.GetProperty<long>(VanishEffect.VANISH_BLOCK_ATTACK_TIME_KEY);

                if (vanishTimeout > 0)
                {
                    if (vanishTimeout > GameLoop.GameLoopTime)
                    {
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.StartAttack.YouMustWaitAgain", (vanishTimeout - GameLoop.GameLoopTime + 1000) / 1000), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        return;
                    }

                    long changeTime = GameLoop.GameLoopTime - vanishTimeout;

                    if (changeTime < 30000)
                    {
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.StartAttack.YouMustWait", ((30000 - changeTime) / 1000).ToString()), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        return;
                    }
                }

                if (player.IsOnHorse)
                    player.IsOnHorse = false;

                if (player.Steed is GameSiegeRam)
                {
                    player.Out.SendMessage("You can't attack while using a ram!", eChatType.CT_YouHit,eChatLoc.CL_SystemWindow);
                    return;
                }

                if (player.IsDisarmed)
                {
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.StartAttack.CantDisarmed"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                    return;
                }

                if (player.IsSitting)
                    player.Sit(false);

                DbInventoryItem attackWeapon = owner.ActiveWeapon;

                if (attackWeapon == null)
                {
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.StartAttack.CannotWithoutWeapon"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                    return;
                }

                if ((eObjectType) attackWeapon.Object_Type is eObjectType.Instrument)
                {
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.StartAttack.CannotMelee"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                    return;
                }

                if (player.ActiveWeaponSlot is eActiveWeaponSlot.Distance)
                {
                    if (!Properties.ALLOW_OLD_ARCHERY)
                    {
                        if ((eCharacterClass) player.CharacterClass.ID is eCharacterClass.Scout or eCharacterClass.Hunter or eCharacterClass.Ranger)
                        {
                            // There is no feedback on live when attempting to fire a bow with arrows.
                            return;
                        }
                    }

                    // Check arrows for ranged attack.
                    if (player.rangeAttackComponent.UpdateAmmo(attackWeapon) == null)
                    {
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.StartAttack.SelectQuiver"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        return;
                    }

                    // Check if selected ammo is compatible for ranged attack.
                    if (!player.rangeAttackComponent.IsAmmoCompatible)
                    {
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.StartAttack.CantUseQuiver"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        return;
                    }

                    if (EffectListService.GetAbilityEffectOnTarget(player, eEffect.SureShot) != null)
                        player.rangeAttackComponent.RangedAttackType = eRangedAttackType.SureShot;

                    if (EffectListService.GetAbilityEffectOnTarget(player, eEffect.RapidFire) != null)
                        player.rangeAttackComponent.RangedAttackType = eRangedAttackType.RapidFire;

                    if (EffectListService.GetAbilityEffectOnTarget(player, eEffect.TrueShot) != null)
                        player.rangeAttackComponent.RangedAttackType = eRangedAttackType.Long;

                    if (player.rangeAttackComponent?.RangedAttackType is eRangedAttackType.Critical &&
                        player.Endurance < RangeAttackComponent.CRITICAL_SHOT_ENDURANCE_COST)
                    {
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.StartAttack.TiredShot"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        return;
                    }

                    if (player.Endurance < RangeAttackComponent.DEFAULT_ENDURANCE_COST)
                    {
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.StartAttack.TiredUse", attackWeapon.Name), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        return;
                    }

                    if (player.IsStealthed)
                    {
                        // -Chance to unstealth while nocking an arrow = stealth spec / level
                        // -Chance to unstealth nocking a crit = stealth / level  0.20
                        int stealthSpec = player.GetModifiedSpecLevel(Specs.Stealth);
                        int stayStealthed = stealthSpec * 100 / player.Level;

                        if (player.rangeAttackComponent?.RangedAttackType is eRangedAttackType.Critical)
                            stayStealthed -= 20;

                        if (!Util.Chance(stayStealthed))
                            player.Stealth(false);
                    }
                }
                else
                {
                    if (_startAttackTarget == null)
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.StartAttack.CombatNoTarget"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                    else
                    {
                        if (_startAttackTarget is GameNPC npcTarget)
                            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.StartAttack.CombatTarget", _startAttackTarget.GetName(0, false, player.Client.Account.Language, npcTarget)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        else
                            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.StartAttack.CombatTarget", _startAttackTarget.GetName(0, false)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

                        // Unstealth right after entering combat mode if anything is targeted.
                        // A timer is used to allow any potential opener to be executed.
                        if (player.IsStealthed)
                            new ECSGameTimer(player, Unstealth, 1);

                        int Unstealth(ECSGameTimer timer)
                        {
                            player.Stealth(false);
                            return 0;
                        }
                    }
                }

                /*
                if (p.CharacterClass is PlayerClass.ClassVampiir)
                {
                    GameSpellEffect removeEffect = SpellHandler.FindEffectOnTarget(p, "VampiirSpeedEnhancement");
                    if (removeEffect != null)
                        removeEffect.Cancel(false);
                }
                else
                {
                    // Bard RR5 ability must drop when the player starts a melee attack
                    IGameEffect DreamweaverRR5 = p.EffectList.GetOfType<DreamweaverEffect>();
                    if (DreamweaverRR5 != null)
                        DreamweaverRR5.Cancel(false);
                }*/

                bool oldAttackState = AttackState;

                if (LivingStartAttack())
                {
                    if (player.castingComponent.SpellHandler?.Spell.Uninterruptible == false)
                    {
                        player.StopCurrentSpellcast();
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.StartAttack.SpellCancelled"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }

                    if (player.ActiveWeaponSlot is not eActiveWeaponSlot.Distance)
                    {
                        if (oldAttackState != AttackState)
                            player.Out.SendAttackMode(AttackState);
                    }
                    else
                    {
                        string typeMsg = (eObjectType) attackWeapon.Object_Type is eObjectType.Thrown ? "throw" : "shot";
                        string targetMsg;

                        if (_startAttackTarget != null)
                        {
                            targetMsg = player.IsWithinRadius(_startAttackTarget, AttackRange)
                                ? LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.StartAttack.TargetInRange")
                                : LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.StartAttack.TargetOutOfRange");
                        }
                        else
                            targetMsg = string.Empty;

                        int speed = AttackSpeed(attackWeapon) / 100;

                        if (!player.effectListComponent.ContainsEffectForEffectType(eEffect.Volley))
                            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.StartAttack.YouPrepare", typeMsg, speed / 10, speed % 10, targetMsg), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                    }
                }
            }
            else if (owner is GameNPC)
                NpcStartAttack();
            else
                LivingStartAttack();
        }

        private bool LivingStartAttack()
        {
            if (owner.IsIncapacitated)
                return false;

            if (owner.IsEngaging)
                owner.CancelEngageEffect();

            if (owner.ActiveWeaponSlot is eActiveWeaponSlot.Distance)
            {
                if (owner.rangeAttackComponent.RangedAttackState is not eRangedAttackState.Aim && attackAction.CheckInterruptTimer())
                    return false;

                owner.rangeAttackComponent.AttackStartTime = GameLoop.GameLoopTime;
            }

            AttackState = true;
            return true;
        }

        private void NpcStartAttack()
        {
            GameNPC npc = owner as GameNPC;
            npc.FireAmbientSentence(GameNPC.eAmbientTrigger.fighting, _startAttackTarget);

            if (npc.Brain is IControlledBrain brain)
            {
                if (brain.AggressionState is eAggressionState.Passive)
                    return;
            }

            // NPCs aren't allowed to prepare their ranged attack while moving or out of range.
            // If we have a running `AttackAction`, let it decide what to do. Not every NPC should start following their target and this allows us to react faster.
            if (npc.ActiveWeaponSlot is eActiveWeaponSlot.Distance)
            {
                if (!npc.IsWithinRadius(_startAttackTarget, AttackRange - 30))
                {
                    if (attackAction == null || !attackAction.OnOutOfRangeOrNoLosRangedAttack())
                    {
                        // Default behavior. If `AttackAction` doesn't handle it, tell the NPC to get closer to its target.
                        StopAttack();
                        npc.Follow(_startAttackTarget, npc.StickMinimumRange, npc.StickMaximumRange);
                    }

                    return;
                }

                if (npc.IsMoving)
                    npc.StopMoving();
            }

            if (LivingStartAttack())
            {
                npc.TargetObject = _startAttackTarget;

                if (_startAttackTarget != npc.FollowTarget)
                {
                    if (npc.IsMoving)
                        npc.StopMoving();

                    if (npc.ActiveWeaponSlot is eActiveWeaponSlot.Distance)
                        npc.TurnTo(_startAttackTarget);

                    npc.Follow(_startAttackTarget, npc.StickMinimumRange, npc.StickMaximumRange);
                }
            }
            else if (npc.ActiveWeaponSlot is eActiveWeaponSlot.Distance)
                npc.TurnTo(_startAttackTarget);
        }

        public void StopAttack()
        {
            StartAttackRequested = false;

            if (owner.ActiveWeaponSlot is eActiveWeaponSlot.Distance)
            {
                if (AttackState)
                {
                    // Only cancel the animation if the ranged ammo isn't released already and we aren't preparing another shot.
                    // If `weaponAction` is null, no attack was performed yet.
                    // If `weaponAction.ActiveWeaponSlot` isn't `eActiveWeaponSlot.Distance`, the instance is outdated.
                    if (weaponAction == null || weaponAction.ActiveWeaponSlot is not eActiveWeaponSlot.Distance || weaponAction.HasAmmoReachedTarget)
                    {
                        foreach (GamePlayer player in owner.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                            player.Out.SendInterruptAnimation(owner);
                    }
                }

                RangeAttackComponent rangeAttackComponent = owner.rangeAttackComponent;
                rangeAttackComponent.RangedAttackType = eRangedAttackType.Normal;

                if (rangeAttackComponent.RangedAttackState is not eRangedAttackState.None)
                {
                    attackAction.OnRangedAttackStop();
                    rangeAttackComponent.RangedAttackState = eRangedAttackState.None;
                }
            }

            bool oldAttackState = AttackState;
            AttackState = false;
            owner.CancelEngageEffect();
            owner.styleComponent.NextCombatStyle = null;
            owner.styleComponent.NextCombatBackupStyle = null;

            if (owner is GamePlayer playerOwner)
            {
                if (playerOwner.IsAlive && oldAttackState)
                    playerOwner.Out.SendAttackMode(AttackState);
            }
            else if (owner is MimicNPC mimic && mimic.MimicBrain != null && !mimic.MimicBrain.HasAggro)
            {
                if (mimic.CharacterClass.ID == (int)eCharacterClass.Hunter ||
                    mimic.CharacterClass.ID == (int)eCharacterClass.Ranger ||
                    mimic.CharacterClass.ID == (int)eCharacterClass.Scout)
                {
                    mimic.SwitchWeapon(eActiveWeaponSlot.Distance);
                }
            }
            else if (owner is not MimicNPC && owner is GameNPC npcOwner)
            {
                // Force NPCs to switch back to their ranged weapon if they have any and their aggro list is empty.
                if (npcOwner.Inventory?.GetItem(eInventorySlot.DistanceWeapon) != null &&
                    npcOwner.ActiveWeaponSlot is not eActiveWeaponSlot.Distance &&
                    npcOwner.Brain is StandardMobBrain brain &&
                    !brain.HasAggro)
                {
                    npcOwner.SwitchWeapon(eActiveWeaponSlot.Distance);
                }
            }
        }

        /// <summary>
        /// Called whenever a single attack strike is made
        /// </summary>
        public AttackData MakeAttack(WeaponAction action, GameObject target, DbInventoryItem weapon, Style style, double effectiveness, int interval, bool dualWield)
        {
            if (owner is IGamePlayer playerOwner)
            {
                if (playerOwner is GamePlayer gamePlayer)
                {
                    if (gamePlayer.IsCrafting)
                    {
                        gamePlayer.Out.SendMessage(LanguageMgr.GetTranslation(playerOwner.Client.Account.Language, "GamePlayer.Attack.InterruptedCrafting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        gamePlayer.craftComponent.StopCraft();
                        gamePlayer.CraftTimer = null;
                        gamePlayer.Out.SendCloseTimerWindow();
                    }

                    if (gamePlayer.IsSalvagingOrRepairing)
                    {
                        gamePlayer.Out.SendMessage(LanguageMgr.GetTranslation(playerOwner.Client.Account.Language, "GamePlayer.Attack.InterruptedCrafting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        gamePlayer.CraftTimer.Stop();
                        gamePlayer.CraftTimer = null;
                        gamePlayer.Out.SendCloseTimerWindow();
                    }
                }

                AttackData ad = LivingMakeAttack(action, target, weapon, style, effectiveness * playerOwner.Effectiveness, interval, dualWield);

                switch (ad.AttackResult)
                {
                    case eAttackResult.HitStyle:
                    case eAttackResult.HitUnstyled:
                    {
                        // Keep component.
                        if ((ad.Target is GameKeepComponent || ad.Target is GameKeepDoor || ad.Target is GameSiegeWeapon) &&
                            ad.Attacker is IGamePlayer && ad.Attacker.GetModified(eProperty.KeepDamage) > 0)
                        {
                            int keepdamage = (int)Math.Floor(ad.Damage * ((double)ad.Attacker.GetModified(eProperty.KeepDamage) / 100));
                            int keepstyle = (int)Math.Floor(ad.StyleDamage * ((double)ad.Attacker.GetModified(eProperty.KeepDamage) / 100));
                            ad.Damage += keepdamage;
                            ad.StyleDamage += keepstyle;
                        }

                        // Vampiir.
                        if (playerOwner.CharacterClass is PlayerClass.ClassVampiir &&
                            target is not GameKeepComponent and not GameKeepDoor and not GameSiegeWeapon)
                        {
                            int perc = Convert.ToInt32((double)(ad.Damage + ad.CriticalDamage) / 100 * (55 - playerOwner.Level));
                            perc = (perc < 1) ? 1 : ((perc > 15) ? 15 : perc);
                            playerOwner.Mana += Convert.ToInt32(Math.Ceiling((decimal)(perc * playerOwner.MaxMana) / 100));
                        }

                        break;
                    }
                }

                switch (ad.AttackResult)
                {
                    case eAttackResult.Blocked:
                    case eAttackResult.Fumbled:
                    case eAttackResult.HitStyle:
                    case eAttackResult.HitUnstyled:
                    case eAttackResult.Missed:
                    case eAttackResult.Parried:
                    {
                        // Condition percent can reach 70%.
                        // Durability percent can reach 0%.

                        if (weapon is GameInventoryItem weaponItem)
                            weaponItem.OnStrikeTarget((GameLiving)playerOwner, target);

                        // Camouflage will be disabled only when attacking a GamePlayer or ControlledNPC of a GamePlayer.
                        if ((target is IGamePlayer && playerOwner.HasAbility(Abilities.Camouflage)) ||
                            (target is GameNPC targetNpc && targetNpc.Brain is IControlledBrain targetNpcBrain && targetNpcBrain.GetPlayerOwner() != null))
                        {
                            CamouflageECSGameEffect camouflage = (CamouflageECSGameEffect)EffectListService.GetAbilityEffectOnTarget((GameLiving)playerOwner, eEffect.Camouflage);

                            if (camouflage != null)
                                EffectService.RequestImmediateCancelEffect(camouflage, false);

                            playerOwner.DisableSkill(SkillBase.GetAbility(Abilities.Camouflage), CamouflageSpecHandler.DISABLE_DURATION);
                        }

                        // Multiple Hit check.
                        if (ad.AttackResult is eAttackResult.HitStyle)
                        {
                            List<GameObject> extraTargets = new();
                            List<GameObject> listAvailableTargets = new();
                            DbInventoryItem attackWeapon = owner.ActiveWeapon;
                            DbInventoryItem leftWeapon = owner.ActiveLeftWeapon;

                            int numTargetsCanHit = style.ID switch
                            {
                                374 => 1, // Tribal Assault: Hits 2 targets.
                                377 => 1, // Clan's Might: Hits 2 targets.
                                379 => 2, // Totemic Wrath: Hits 3 targets.
                                384 => 3, // Totemic Sacrifice: Hits 4 targets.
                                600 => 255, // Shield Swipe: No cap.
                                _ => 0
                            };

                            if (numTargetsCanHit <= 0)
                                break;

                            bool IsNotShieldSwipe = style.ID != 600;

                            if (IsNotShieldSwipe)
                            {
                                foreach (GamePlayer playerInRange in owner.GetPlayersInRadius((ushort)AttackRange))
                                {
                                    if (GameServer.ServerRules.IsAllowedToAttack(owner, playerInRange, true))
                                        listAvailableTargets.Add(playerInRange);
                                }
                            }

                            foreach (GameNPC npcInRange in owner.GetNPCsInRadius((ushort)AttackRange))
                            {
                                if (GameServer.ServerRules.IsAllowedToAttack(owner, npcInRange, true))
                                    listAvailableTargets.Add(npcInRange);
                            }

                            // Remove primary target.
                            listAvailableTargets.Remove(target);

                            if (numTargetsCanHit >= listAvailableTargets.Count)
                                extraTargets = listAvailableTargets;
                            else
                            {
                                int index;
                                GameObject availableTarget;

                                for (int i = numTargetsCanHit; i > 0; i--)
                                {
                                    index = Util.Random(listAvailableTargets.Count - 1);
                                    availableTarget = listAvailableTargets[index];
                                    listAvailableTargets.SwapRemoveAt(index);
                                    extraTargets.Add(availableTarget);
                                }
                            }

                            foreach (GameObject extraTarget in extraTargets)
                            {
                                if (extraTarget is IGamePlayer player && player.IsSitting)
                                    effectiveness *= 2;

                                // TODO: Figure out why Shield Swipe is handled differently here.
                                if (IsNotShieldSwipe)
                                {
                                    weaponAction = new WeaponAction((GameLiving)playerOwner, extraTarget, attackWeapon, leftWeapon, effectiveness, AttackSpeed(attackWeapon), null);
                                    weaponAction.Execute();
                                }
                                else
                                    LivingMakeAttack(action, extraTarget, attackWeapon, null, 1, Properties.SPELL_INTERRUPT_DURATION, false);
                            }
                        }

                        break;
                    }
                }

                return ad;
            }
            else
            {
                if (owner is NecromancerPet necromancerPet)
                    ((NecromancerPetBrain)necromancerPet.Brain).CheckAttackSpellQueue();
                else
                    effectiveness = 1;

                return LivingMakeAttack(action, target, weapon, style, effectiveness, interval, dualWield);
            }
        }

        /// <summary>
        /// This method is called to make an attack, it is called from the
        /// attacktimer and should not be called manually
        /// </summary>
        /// <returns>the object where we collect and modifiy all parameters about the attack</returns>
        public AttackData LivingMakeAttack(WeaponAction action, GameObject target, DbInventoryItem weapon, Style style, double effectiveness, int interval, bool dualWield, bool ignoreLOS = false)
        {
            AttackData ad = new()
            {
                Attacker = owner,
                Target = target as GameLiving,
                Style = style,
                DamageType = AttackDamageType(weapon),
                Weapon = weapon,
                Interval = interval,
                IsOffHand = weapon != null && weapon.SlotPosition is Slot.LEFTHAND
            };

            // Asp style range add.
            IEnumerable<(Spell, int, int)> rangeProc = style?.Procs.Where(x => x.Item1.SpellType is eSpellType.StyleRange);
            int addRange = rangeProc?.Any() == true ? (int)(rangeProc.First().Item1.Value - AttackRange) : 0;

            ad.AttackType = AttackData.GetAttackType(weapon, dualWield, ad.Attacker);

            // No target.
            if (ad.Target == null)
            {
                ad.AttackResult = (target == null) ? eAttackResult.NoTarget : eAttackResult.NoValidTarget;
                SendAttackingCombatMessages(action, ad);
                return ad;
            }

            // Region / state check.
            if (ad.Target.CurrentRegionID != owner.CurrentRegionID || ad.Target.ObjectState is not eObjectState.Active)
            {
                ad.AttackResult = eAttackResult.NoValidTarget;
                SendAttackingCombatMessages(action, ad);
                return ad;
            }

            // LoS / in front check.
            if (!ignoreLOS && ad.AttackType is not AttackData.eAttackType.Ranged && owner is GamePlayer &&
                ad.Target is not GameKeepComponent &&
                !(owner.IsObjectInFront(ad.Target, 120) && owner.TargetInView))
            {
                ad.AttackResult = eAttackResult.TargetNotVisible;
                SendAttackingCombatMessages(action, ad);
                return ad;
            }

            // Target is already dead.
            if (!ad.Target.IsAlive)
            {
                ad.AttackResult = eAttackResult.TargetDead;
                SendAttackingCombatMessages(action, ad);
                return ad;
            }

            // Melee range check (ranged is already done at this point).
            if (ad.AttackType is not AttackData.eAttackType.Ranged)
            {
                if (!owner.IsWithinRadius(ad.Target, AttackRange + addRange))
                {
                    ad.AttackResult = eAttackResult.OutOfRange;
                    SendAttackingCombatMessages(action, ad);
                    return ad;
                }
            }

            if (!GameServer.ServerRules.IsAllowedToAttack(ad.Attacker, ad.Target, GameLoop.GameLoopTime - attackAction.RoundWithNoAttackTime <= 1500))
            {
                ad.AttackResult = eAttackResult.NotAllowed_ServerRules;
                SendAttackingCombatMessages(action, ad);
                return ad;
            }

            if (ad.Target.IsSitting)
                effectiveness *= 2;

            // Apply Mentalist RA5L.
            SelectiveBlindnessEffect SelectiveBlindness = owner.EffectList.GetOfType<SelectiveBlindnessEffect>();
            if (SelectiveBlindness != null)
            {
                GameLiving EffectOwner = SelectiveBlindness.EffectSource;
                if (EffectOwner == ad.Target)
                {
                    if (owner is GamePlayer)
                        ((GamePlayer)owner).Out.SendMessage(
                            string.Format(
                                LanguageMgr.GetTranslation(((GamePlayer)owner).Client.Account.Language,
                                    "GameLiving.AttackData.InvisibleToYou"), ad.Target.GetName(0, true)),
                            eChatType.CT_Missed, eChatLoc.CL_SystemWindow);

                    ad.AttackResult = eAttackResult.NoValidTarget;

                    SendAttackingCombatMessages(action, ad);
                    return ad;
                }
            }

            // DamageImmunity Ability.
            if ((GameLiving)target != null && ((GameLiving)target).HasAbility(Abilities.DamageImmunity))
            {
                //if (ad.Attacker is GamePlayer) ((GamePlayer)ad.Attacker).Out.SendMessage(string.Format("{0} can't be attacked!", ad.Target.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                ad.AttackResult = eAttackResult.NoValidTarget;
                SendAttackingCombatMessages(action, ad);
                return ad;
            }

            // Calculate our attack result and attack damage.
            ad.AttackResult = ad.Target.attackComponent.CalculateEnemyAttackResult(action, ad, weapon, ref effectiveness);

            IGamePlayer playerOwner = owner as IGamePlayer;

            // Strafing miss.
            if (playerOwner != null && playerOwner.IsStrafing && ad.Target is IGamePlayer && Util.Chance(30))
            {
                // Used to tell the difference between a normal miss and a strafing miss.
                // Ugly, but we shouldn't add a new field to 'AttackData' just for that purpose.
                ad.MissChance = 0;
                ad.AttackResult = eAttackResult.Missed;
            }

            switch (ad.AttackResult)
            {
                // Calculate damage only if we hit the target.
                case eAttackResult.HitUnstyled:
                case eAttackResult.HitStyle:
                {
                    double damage = AttackDamage(weapon, out double baseDamageCap) * effectiveness;
                    DbInventoryItem armor = null;

                    if (ad.Target.Inventory != null)
                        armor = ad.Target.Inventory.GetItem((eInventorySlot)ad.ArmorHitLocation);

                    double weaponSkill = CalculateWeaponSkill(weapon, ad.Target, out int spec, out (double, double) varianceRange, out double specModifier, out double baseWeaponSkill);
                    double armorMod = CalculateTargetArmor(ad.Target, ad.ArmorHitLocation, out double armorFactor, out double absorb);
                    double damageMod = weaponSkill / armorMod;

                    if (action.RangedAttackType is eRangedAttackType.Critical)
                        baseDamageCap *= 2; // This may be incorrect. Critical shot doesn't double damage on >yellow targets.

                    // Badge Of Valor Calculation 1+ absorb or 1- absorb
                    // if (ad.Attacker.EffectList.GetOfType<BadgeOfValorEffect>() != null)
                    //     damage *= 1.0 + Math.Min(0.85, ad.Target.GetArmorAbsorb(ad.ArmorHitLocation));
                    // else
                    //     damage *= 1.0 - Math.Min(0.85, ad.Target.GetArmorAbsorb(ad.ArmorHitLocation));

                    if (ad.IsOffHand)
                        damage *= 1 + owner.GetModified(eProperty.OffhandDamage) * 0.01;

                    // If the target is another player's pet, shouldn't 'PVP_MELEE_DAMAGE' be used?
                    if (owner is IGamePlayer || (owner is GameNPC npcOwner && npcOwner.Brain is IControlledBrain && owner.Realm != 0))
                    {
                        if (target is IGamePlayer)
                            damage *= Properties.PVP_MELEE_DAMAGE;
                        else if (target is GameNPC)
                            damage *= Properties.PVE_MELEE_DAMAGE;
                    }

                    damage *= damageMod;
                    double conversionMod = CalculateTargetConversion(ad.Target);
                    double primarySecondaryResistMod = CalculateTargetResistance(ad.Target, ad.DamageType, armor);
                    double primarySecondaryResistConversionMod = primarySecondaryResistMod * conversionMod;
                    double preResistBaseDamage = damage;
                    damage = Math.Min(baseDamageCap, preResistBaseDamage * primarySecondaryResistConversionMod);
                    // This makes capped unstyled hits have weird modifiers and no longer match the actual damage reduction from resistances; for example 150 (-1432) an a naked target.
                    // But inaccurate modifiers when the cap is hit appear to be live like.
                    double modifier = damage - preResistBaseDamage;

                    if (StyleProcessor.ExecuteStyle(owner, ad.Target, ad.Style, weapon, preResistBaseDamage, baseDamageCap, ad.ArmorHitLocation, ad.StyleEffects, out double styleDamage, out double styleDamageCap, out int animationId))
                    {
                        double preResistStyleDamage = styleDamage;
                        ad.StyleDamage = (int) preResistStyleDamage; // We show uncapped and unmodified by resistances style damage. This should only be used by the combat log.
                        // We have to calculate damage reduction again because `ExecuteStyle` works with pre resist base damage. Static growth styles also don't use it.
                        styleDamage = preResistStyleDamage * primarySecondaryResistConversionMod;

                        if (styleDamageCap > 0)
                            styleDamage = Math.Min(styleDamageCap, styleDamage);

                        damage += styleDamage;
                        modifier += styleDamage - preResistStyleDamage;
                        ad.AnimationId = animationId;
                        ad.AttackResult = eAttackResult.HitStyle;
                    }

                    ad.Damage = (int) damage;
                    ad.Modifier = (int) Math.Floor(modifier);
                    ad.CriticalChance = CalculateCriticalChance(action);
                    ad.CriticalDamage = CalculateCriticalDamage(ad);

                    if (conversionMod < 1)
                    {
                        double conversionAmount = conversionMod > 0 ? damage / conversionMod - damage : damage;
                        ApplyTargetConversionRegen(ad.Target, (int)conversionAmount);
                    }

                    if (playerOwner != null && playerOwner is GamePlayer gPlayerOwner && gPlayerOwner.UseDetailedCombatLog)
                        PrintDetailedCombatLog(gPlayerOwner, armorFactor, absorb, armorMod, baseWeaponSkill, varianceRange, specModifier, weaponSkill, damageMod, baseDamageCap, styleDamageCap);

                    if (target is GamePlayer targetPlayer && targetPlayer.UseDetailedCombatLog)
                        PrintDetailedCombatLog(targetPlayer, armorFactor, absorb, armorMod, baseWeaponSkill, varianceRange, specModifier, weaponSkill, damageMod, baseDamageCap, styleDamageCap);

                    break;
                }
                case eAttackResult.Blocked:
                case eAttackResult.Evaded:
                case eAttackResult.Parried:
                case eAttackResult.Missed:
                {
                    // Reduce endurance by half the style's cost if we missed.
                    if (ad.Style != null && playerOwner != null && weapon != null)
                        playerOwner.Endurance -= StyleProcessor.CalculateEnduranceCost((GameLiving)playerOwner, ad.Style, weapon.SPD_ABS) / 2;

                    break;
                }

                static void PrintDetailedCombatLog(GamePlayer player, double armorFactor, double absorb, double armorMod, double baseWeaponSkill, (double lowerLimit, double upperLimit) varianceRange, double specModifier, double weaponSkill, double damageMod, double baseDamageCap, double styleDamageCap)
                {
                    StringBuilder stringBuilder = new();
                    stringBuilder.Append($"BaseWS: {baseWeaponSkill:0.##} | SpecMod: {specModifier:0.##} ({varianceRange.lowerLimit:0.00}~{varianceRange.upperLimit:0.00}) | WS: {weaponSkill:0.##}\n");
                    stringBuilder.Append($"AF: {armorFactor:0.##} | ABS: {absorb * 100:0.##}% | AF/ABS: {armorMod:0.##}\n");
                    stringBuilder.Append($"DamageMod: {damageMod:0.##} | BaseDamageCap: {baseDamageCap:0.##}");

                    if (styleDamageCap > 0)
                        stringBuilder.Append($" | StyleDamageCap: {styleDamageCap:0.##}");

                    player.Out.SendMessage(stringBuilder.ToString(), eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);
                }
            }

            // Attacked living may modify the attack data. Primarily used for keep doors and components.
            ad.Target.ModifyAttack(ad);

            string message = string.Empty;
            bool broadcast = true;

            ArrayList excludes = new()
            {
                ad.Attacker,
                ad.Target
            };

            switch (ad.AttackResult)
            {
                case eAttackResult.Parried:
                    message = $"{ad.Attacker.GetName(0, true)} attacks {ad.Target.GetName(0, false)} and is parried!";
                break;

                case eAttackResult.Evaded:
                    message = $"{ad.Attacker.GetName(0, true)} attacks {ad.Target.GetName(0, false)} and is evaded!";
                break;

                case eAttackResult.Fumbled:
                    message = $"{ad.Attacker.GetName(0, true)} fumbled!";
                break;

                case eAttackResult.Missed:
                    message = $"{ad.Attacker.GetName(0, true)} attacks {ad.Target.GetName(0, false)} and misses!";
                break;

                case eAttackResult.Blocked:
                {
                    message = $"{ad.Attacker.GetName(0, true)} attacks {ad.Target.GetName(0, false)} and is blocked!";

                    // Guard.
                    if (target != null && target != ad.Target)
                    {
                        excludes.Add(target);

                        // Another player blocked for real target.
                        if (target is GamePlayer playerTarget)
                            playerTarget.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(playerTarget.Client.Account.Language, "GameLiving.AttackData.BlocksYou") + $" ({ad.BlockChance:0.0}%)", ad.Target.GetName(0, true), ad.Attacker.GetName(0, false)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);

                        // Blocked for another player.
                        if (ad.Target is GamePlayer playerTarget2)
                        {
                            playerTarget2.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(playerTarget2.Client.Account.Language, "GameLiving.AttackData.YouBlock") + $" ({ad.BlockChance:0.0}%)", ad.Attacker.GetName(0, false), target.GetName(0, false)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                            playerTarget2.Stealth(false);
                        }
                    }
                    else if (ad.Target is GamePlayer playerTarget)
                        playerTarget.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(playerTarget.Client.Account.Language, "GameLiving.Attack.Block") + $" ({ad.BlockChance:0.0}%)", ad.Attacker.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);

                    break;
                }
                case eAttackResult.HitUnstyled:
                case eAttackResult.HitStyle:
                {
                    if (ad.AttackResult is eAttackResult.HitStyle)
                    {
                        if (playerOwner != null)
                        {
                            string damageAmount = $" (+{ad.StyleDamage}, GR: {ad.Style.GrowthRate})";
                            message = LanguageMgr.GetTranslation(playerOwner.Client.Account.Language, "StyleProcessor.ExecuteStyle.PerformPerfectly",ad.Style.Name, damageAmount);
                            playerOwner.Out.SendMessage(message, eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        }
                        else if (owner is GameNPC ownerNpc && ownerNpc.Brain is ControlledMobBrain attackerBrain)
                        {
                            GamePlayer player = attackerBrain.GetPlayerOwner();

                            if (player != null)
                            {
                                string damageAmount = $" (+{ad.StyleDamage}, GR: {ad.Style.GrowthRate})";
                                message = LanguageMgr.GetTranslation(player.Client.Account.Language, "StyleProcessor.ExecuteStyle.PerformsPerfectly", owner.Name, ad.Style.Name, damageAmount);
                                player.Out.SendMessage(message, eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                            }
                        }
                    }

                    if (target != null && target != ad.Target)
                    {
                        message = string.Format("{0} attacks {1} but hits {2}!", ad.Attacker.GetName(0, true),
                            target.GetName(0, false), ad.Target.GetName(0, false));
                        excludes.Add(target);

                        // intercept for another player
                        if (target is GamePlayer)
                            ((GamePlayer)target).Out.SendMessage(
                                string.Format(
                                    LanguageMgr.GetTranslation(((GamePlayer)target).Client.Account.Language,
                                        "GameLiving.AttackData.StepsInFront"), ad.Target.GetName(0, true)),
                                eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

                        // intercept by player
                        if (ad.Target is GamePlayer)
                            ((GamePlayer)ad.Target).Out.SendMessage(
                                string.Format(
                                    LanguageMgr.GetTranslation(((GamePlayer)ad.Target).Client.Account.Language,
                                        "GameLiving.AttackData.YouStepInFront"), target.GetName(0, false)),
                                eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                    }
                    else
                    {
                        if (ad.Attacker is IGamePlayer)
                        {
                            string hitWeapon = "weapon";
                            if (weapon != null)
                                hitWeapon = GlobalConstants.NameToShortName(weapon.Name);
                            message = string.Format("{0} attacks {1} with {2} {3}!", ad.Attacker.GetName(0, true),
                                ad.Target.GetName(0, false), ad.Attacker.GetPronoun(1, false), hitWeapon);
                        }
                        else
                        {
                            message = string.Format("{0} attacks {1} and hits!", ad.Attacker.GetName(0, true),
                                ad.Target.GetName(0, false));
                        }
                    }

                    break;
                }
                default:
                broadcast = false;
                break;
            }

            SendAttackingCombatMessages(action, ad);

            #region Prevent Flight

            if (ad.Attacker is IGamePlayer)
            {
                if (ad.Attacker.HasAbilityType(typeof(AtlasOF_PreventFlight)) && Util.Chance(35))
                {
                    if (owner.IsObjectInFront(ad.Target, 120) && ad.Target.IsMoving)
                    {
                        bool preCheck = false;
                        float angle = ad.Target.GetAngle(ad.Attacker);
                        if (angle >= 150 && angle < 210) preCheck = true;

                        if (preCheck)
                        {
                            Spell spell = SkillBase.GetSpellByID(7083);
                            if (spell != null)
                            {
                                ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(owner, spell,
                                    SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
                                if (spellHandler != null)
                                {
                                    spellHandler.StartSpell(ad.Target);
                                }
                            }
                        }
                    }
                }
            }

            #endregion Prevent Flight

            #region controlled messages

            if (ad.Attacker is GameNPC npc && npc.Brain is IControlledBrain brain)
            {
                GamePlayer player = brain.GetPlayerOwner();

                if (player != null)
                {
                    excludes.Add(player);

                    switch (ad.AttackResult)
                    {
                        case eAttackResult.HitStyle:
                        case eAttackResult.HitUnstyled:
                        {
                            string modMessage;

                            if (ad.Modifier > 0)
                                modMessage = $" (+{ad.Modifier})";
                            else if (ad.Modifier < 0)
                                modMessage = $" ({ad.Modifier})";
                            else
                                modMessage = string.Empty;

                            string attackTypeMsg;

                            if (action.ActiveWeaponSlot is eActiveWeaponSlot.Distance)
                                attackTypeMsg = "shoots";
                            else
                                attackTypeMsg = "attacks";

                            player.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameLiving.AttackData.YourHits"),
                                ad.Attacker.Name, attackTypeMsg, ad.Target.GetName(0, false), ad.Damage, modMessage),
                                eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

                            if (ad.CriticalDamage > 0)
                            {
                                player.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameLiving.AttackData.YourCriticallyHits"),
                                    ad.Attacker.Name, ad.Target.GetName(0, false), ad.CriticalDamage) + $" ({ad.CriticalChance}%)",
                                    eChatType.CT_YouHit,eChatLoc.CL_SystemWindow);
                            }

                            break;
                        }
                        case eAttackResult.Missed:
                        {
                            player.Out.SendMessage($"{message} ({ad.MissChance:0.##}%)", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                            break;
                        }
                        default:
                        {
                            player.Out.SendMessage(message, eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    }
                }
            }

            if (ad.Target is GameNPC npcTarget && npcTarget.Brain is IControlledBrain targetBrain)
            {
                GamePlayer player = targetBrain.GetPlayerOwner();
                excludes.Add(player);

                if (player != null)
                {
                    switch (ad.AttackResult)
                    {
                        case eAttackResult.Blocked:
                        {
                            player.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameLiving.AttackData.Blocked"), ad.Attacker.GetName(0, true), ad.Target.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                            break;
                        }
                        case eAttackResult.Parried:
                        {
                            player.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameLiving.AttackData.Parried"), ad.Attacker.GetName(0, true), ad.Target.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                            break;
                        }
                        case eAttackResult.Evaded:
                        {
                            player.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameLiving.AttackData.Evaded"), ad.Attacker.GetName(0, true), ad.Target.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                            break;
                        }
                        case eAttackResult.Fumbled:
                        {
                            player.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameLiving.AttackData.Fumbled"), ad.Attacker.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                            break;
                        }
                        case eAttackResult.Missed:
                        {
                            if (ad.AttackType is AttackData.eAttackType.Spell)
                                break;

                            player.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameLiving.AttackData.Misses"), ad.Attacker.GetName(0, true), ad.Target.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                            break;
                        }
                        case eAttackResult.HitStyle:
                        case eAttackResult.HitUnstyled:
                        {
                            string modMessage;

                            if (ad.Modifier > 0)
                                modMessage = $" (+{ad.Modifier})";
                            else if (ad.Modifier < 0)
                                modMessage = $" ({ad.Modifier})";
                            else
                                modMessage = string.Empty;

                            player.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameLiving.AttackData.HitsForDamage"), ad.Attacker.GetName(0, true), ad.Target.Name, ad.Damage, modMessage), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);

                            if (ad.CriticalDamage > 0)
                                player.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameLiving.AttackData.CriticallyHitsForDamage"), ad.Attacker.GetName(0, true), ad.Target.Name, ad.CriticalDamage), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);

                            break;
                        }
                        default:
                            break;
                    }
                }
            }

            #endregion controlled messages

            if (broadcast)
                Message.SystemToArea(ad.Attacker, message, eChatType.CT_OthersCombat, (GameObject[]) excludes.ToArray(typeof(GameObject)));

            // Interrupt the target of the attack.
            ad.Target.StartInterruptTimer(interval, ad.AttackType, ad.Attacker);

            // If we're attacking via melee, start an interrupt timer on ourselves so we cannot swing + immediately cast.
            if (ad.IsMeleeAttack)
                owner.StartInterruptTimer(owner.SelfInterruptDurationOnMeleeAttack, ad.AttackType, ad.Attacker);

            owner.OnAttackEnemy(ad);
            return ad;
        }

        public double CalculateWeaponSkill(DbInventoryItem weapon, GameLiving target, out int spec, out (double, double) varianceRange, out double specModifier, out double baseWeaponSkill)
        {
            spec = CalculateSpec(weapon);
            specModifier = CalculateSpecModifier(target, spec, out varianceRange);
            return CalculateWeaponSkill(weapon, specModifier, out baseWeaponSkill);
        }

        public double CalculateWeaponSkill(DbInventoryItem weapon, double specModifier, out double baseWeaponSkill)
        {
            baseWeaponSkill = owner.GetWeaponSkill(weapon) + INHERENT_WEAPON_SKILL;
            double relicBonus = 1.0;

            if (owner is IGamePlayer)
                relicBonus += RelicMgr.GetRelicBonusModifier(owner.Realm, eRelicType.Strength);

            return baseWeaponSkill * relicBonus * specModifier;
        }

        public double CalculateDefensePenetration(DbInventoryItem weapon, int targetLevel)
        {
            int levelDifference = (owner is GamePlayer ? owner.WeaponSpecLevel(weapon) : owner.Level) - targetLevel;
            double specModifier = 1 + levelDifference * 0.01;
            return CalculateWeaponSkill(weapon, specModifier, out _) * 0.08 / 100;
        }

        public int CalculateSpec(DbInventoryItem weapon)
        {
            if (weapon == null)
                return 0;

            eObjectType objectType = (eObjectType) weapon.Object_Type;
            int slotPosition = weapon.SlotPosition;

            if (owner is GamePlayer && owner.Realm is eRealm.Albion && Properties.ENABLE_ALBION_ADVANCED_WEAPON_SPEC &&
                (GameServer.ServerRules.IsObjectTypesEqual((eObjectType) weapon.Object_Type, eObjectType.TwoHandedWeapon) ||
                GameServer.ServerRules.IsObjectTypesEqual((eObjectType) weapon.Object_Type, eObjectType.PolearmWeapon)))
            {
                // Albion dual spec penalty, which sets minimum damage to the base damage spec.
                if ((eDamageType) weapon.Type_Damage is eDamageType.Crush)
                    objectType = eObjectType.CrushingWeapon;
                else if ((eDamageType) weapon.Type_Damage is eDamageType.Slash)
                    objectType = eObjectType.SlashingWeapon;
                else
                    objectType = eObjectType.ThrustWeapon;
            }

            return owner.WeaponSpecLevel(objectType, slotPosition);
        }

        public (double, double) CalculateVarianceRange(GameLiving target, int spec)
        {
            (double lowerLimit, double upperLimit) varianceRange;

            if (owner is IGamePlayer playerOwner)
            {
                if (playerOwner.SpecLock > 0)
                    return (playerOwner.SpecLock, playerOwner.SpecLock);

                // Characters below level 5 get a bonus to their spec to help with the very wide variance at this level range.
                // Target level, lower bound at 2, lower bound at 1:
                // 0 | 1      | 0.25
                // 1 | 0.625  | 0.25
                // 2 | 0.5    | 0.25
                // 3 | 0.4375 | 0.25
                // 4 | 0.4    | 0.25
                // 5 | 0.375  | 0.25
                // Absolute minimum spec is set to 1 to prevent an issue where the lower bound (with staffs for example) would slightly rise with the target's level.
                // Also prevents negative values.
                spec = Math.Max(owner.Level < 5 ? 2 : 1, spec);
                double specVsTargetLevelMod = (spec - 1) / ((double) target.Level + 1);
                varianceRange = (Math.Min(0.75 * specVsTargetLevelMod + 0.25, 1.0), Math.Min(Math.Max(1.25 + (3.0 * specVsTargetLevelMod - 2) * 0.25, 1.25), 1.5));
            }
            else
                varianceRange = (0.9, 1.1);

            return varianceRange;
        }

        public double CalculateSpecModifier(GameLiving target, int spec, out (double lowerLimit, double upperLimit) varianceRange)
        {
            varianceRange = CalculateVarianceRange(target, spec);
            double difference = varianceRange.upperLimit - varianceRange.lowerLimit;
            return varianceRange.lowerLimit + Util.RandomDoubleIncl() * difference;
        }

        public static double CalculateTargetArmor(GameLiving target, eArmorSlot armorSlot, out double armorFactor, out double absorb)
        {
            armorFactor = target.GetArmorAF(armorSlot) + INHERENT_ARMOR_FACTOR;

            // Gives an extra 0.4~20 bonus AF to players. Ideally this should be done in `ArmorFactorCalculator`.
            if (target is IGamePlayer or GameTrainingDummy)
                armorFactor += target.Level * 20 / 50.0;

            absorb = target.GetArmorAbsorb(armorSlot);
            return absorb >= 1 ? double.MaxValue : armorFactor / (1 - absorb);
        }

        public static double CalculateTargetResistance(GameLiving target, eDamageType damageType, DbInventoryItem armor)
        {
            double damageModifier = 1.0;
            damageModifier *= 1.0 - (target.GetResist(damageType) + SkillBase.GetArmorResist(armor, damageType)) * 0.01;
            return damageModifier;
        }

        public static double CalculateTargetConversion(GameLiving target)
        {
            if (target is not IGamePlayer)
                return 1.0;

            double conversionMod = 1 - target.GetModified(eProperty.Conversion) / 100.0;
            return Math.Min(1.0, conversionMod);
        }

        public static void ApplyTargetConversionRegen(GameLiving target, int conversionAmount)
        {
            if (target is not IGamePlayer)
                return;

            GamePlayer playerTarget = target as GamePlayer;

            int powerConversion = conversionAmount;
            int enduranceConversion = conversionAmount;

            if (target.Mana + conversionAmount > target.MaxMana)
                powerConversion = target.MaxMana - target.Mana;

            if (target.Endurance + conversionAmount > target.MaxEndurance)
                enduranceConversion = target.MaxEndurance - target.Endurance;

            if (powerConversion > 0)
                playerTarget?.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(playerTarget.Client.Account.Language, "GameLiving.AttackData.GainPowerPoints"), powerConversion), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);

            if (enduranceConversion > 0)
                playerTarget?.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(playerTarget.Client.Account.Language, "GameLiving.AttackData.GainEndurancePoints"), enduranceConversion), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);

            target.Mana = Math.Min(target.MaxMana, target.Mana + powerConversion);
            target.Endurance = Math.Min(target.MaxEndurance, target.Endurance + enduranceConversion);
        }

        public virtual bool CheckBlock(AttackData ad)
        {
            double blockChance = owner.TryBlock(ad, out int shieldSize);
            ad.BlockChance = blockChance * 100;
            double blockRoll;

            if (blockChance > 0)
            {
                if (!Properties.OVERRIDE_DECK_RNG && owner is GamePlayer player)
                    blockRoll = player.RandomNumberDeck.GetPseudoDouble();
                else
                    blockRoll = Util.CryptoNextDouble();

                if (ad.Attacker is GamePlayer attacker && attacker.UseDetailedCombatLog)
                    attacker.Out.SendMessage($"target block%: {blockChance * 100:0.##} rand: {blockRoll * 100:0.##}", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);

                if (ad.Target is GamePlayer defender && defender.UseDetailedCombatLog)
                    defender.Out.SendMessage($"your block%: {blockChance * 100:0.##} rand: {blockRoll * 100:0.##}", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);

                // The order here matters a lot. Either we consume attempts (by calling `Consume` first`) or blocks (by checking the roll first; the current implementation).
                // If we consume attempts, the effective block rate changes according to this formula: "if (shieldSize < attackerCount) blockChance *= (shieldSize / attackerCount)".
                // If we consume blocks, then the reduction is lower the lower the base block chance, and identical with a theoretical 100% block chance.
                if (blockChance > blockRoll && _blockRoundHandler.Consume(shieldSize, ad))
                    return true;
            }

            if (ad.AttackType is AttackData.eAttackType.Ranged or AttackData.eAttackType.Spell)
            {
                // Nature's shield, 100% block chance, 120° frontal angle.
                if (owner.IsObjectInFront(ad.Attacker, 120) && (owner.styleComponent.NextCombatStyle?.ID == 394 || owner.styleComponent.NextCombatBackupStyle?.ID == 394))
                {
                    ad.BlockChance = 100;
                    return true;
                }
            }

            return false;
        }

        public bool CheckGuard(AttackData ad, bool stealthStyle)
        {
            foreach (GuardECSGameEffect guard in owner.effectListComponent.GetAbilityEffects().Where(e => e.EffectType is eEffect.Guard))
            {
                if (guard.Target != owner)
                    continue;

                GameLiving source = guard.Source;

                if (source == null ||
                    source.IsIncapacitated ||
                    source.ActiveWeaponSlot is eActiveWeaponSlot.Distance ||
                    source.IsSitting ||
                    stealthStyle ||
                    !guard.Source.IsObjectInFront(ad.Attacker, 180) ||
                    !guard.Source.IsWithinRadius(guard.Target, GuardAbilityHandler.GUARD_DISTANCE))
                    continue;

                DbInventoryItem rightHand = source.ActiveWeapon;
                DbInventoryItem leftHand = source.ActiveLeftWeapon;

                if (((rightHand != null && rightHand.Hand == 1) || leftHand == null || (eObjectType) leftHand.Object_Type is not eObjectType.Shield) && ( source is not GameNPC || source is MimicNPC))
                    continue;
                
                double guardChance;

                if (source is GameNPC && source is not MimicNPC)
                    guardChance = source.GetModified(eProperty.BlockChance);
                else
                    guardChance = source.GetModified(eProperty.BlockChance) * (leftHand.Quality * 0.01) * (leftHand.Condition / (double) leftHand.MaxCondition);

                guardChance *= 0.001;
                guardChance += source.GetAbilityLevel(Abilities.Guard) * 0.05; // 5% additional chance to guard with each Guard level.
                guardChance *= 1 - ad.DefensePenetration;

                if (guardChance > Properties.BLOCK_CAP && ad.Attacker is GamePlayer && ad.Target is GamePlayer)
                    guardChance = Properties.BLOCK_CAP;

                int shieldSize = 1; // Guard isn't affected by shield size or attacker count.

                if (leftHand != null)
                    shieldSize = Math.Max(leftHand.Type_Damage, 1);

                // Possibly intended to be applied in RvR only.
                if (shieldSize == 1 && guardChance > 0.8)
                    guardChance = 0.8;
                else if (shieldSize == 2 && guardChance > 0.9)
                    guardChance = 0.9;
                else if (shieldSize == 3 && guardChance > 0.99)
                    guardChance = 0.99;

                if (ad.AttackType is AttackData.eAttackType.MeleeDualWield)
                    guardChance *= ad.Attacker.DualWieldDefensePenetrationFactor;

                if (guardChance > 0)
                {
                    double guardRoll;

            if (!Properties.OVERRIDE_DECK_RNG && owner is IGamePlayer player)
                        guardRoll = player.RandomNumberDeck.GetPseudoDouble();
                    else
                        guardRoll = Util.CryptoNextDouble();

                    if (source is GamePlayer blockAttk && blockAttk.UseDetailedCombatLog)
                        blockAttk.Out.SendMessage($"chance to guard: {guardChance * 100:0.##} rand: {guardRoll * 100:0.##}", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);

                    if (guard.Target is GamePlayer blockTarg && blockTarg.UseDetailedCombatLog)
                        blockTarg.Out.SendMessage($"chance to be guarded: {guardChance * 100:0.##} rand: {guardRoll * 100:0.##}", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);

                    if (guardChance > guardRoll)
                    {
                        ad.Target = source;
                        ad.BlockChance = guardChance * 100;
                        return true;
                    }
                }
            }

            return false;
        }

        public bool CheckDashingDefense(AttackData ad, bool stealthStyle, out eAttackResult result)
        {
            // Not implemented.
            // Very outdated and needs to be rewritten.
            result = eAttackResult.Any;
            return false;
            DashingDefenseEffect dashing = null;

            if (dashing == null ||
                dashing.GuardSource.ObjectState != eObjectState.Active ||
                dashing.GuardSource.IsStunned != false ||
                dashing.GuardSource.IsMezzed != false ||
                dashing.GuardSource.ActiveWeaponSlot is eActiveWeaponSlot.Distance ||
                !dashing.GuardSource.IsAlive ||
                stealthStyle)
                return false;

            if (!dashing.GuardSource.IsWithinRadius(dashing.GuardTarget, DashingDefenseEffect.GUARD_DISTANCE))
                return false;

            DbInventoryItem rightHand = dashing.GuardSource.ActiveWeapon;
            DbInventoryItem leftHand = dashing.GuardSource.ActiveLeftWeapon;

            if ((rightHand == null || rightHand.Hand != 1) && leftHand != null && leftHand.Object_Type == (int)eObjectType.Shield)
            {
                int guardLevel = dashing.GuardSource.GetAbilityLevel(Abilities.Guard);
                double guardchance = dashing.GuardSource.GetModified(eProperty.BlockChance) * leftHand.Quality * 0.00001;
                guardchance *= guardLevel * 0.25 + 0.05;

                if (guardchance > 0.99)
                    guardchance = 0.99;

                int shieldSize = 0;

                if (leftHand != null)
                    shieldSize = leftHand.Type_Damage;

                if (Attackers.Count > shieldSize)
                    guardchance *= shieldSize / (double)Attackers.Count;

                if (ad.AttackType == AttackData.eAttackType.MeleeDualWield)
                    guardchance /= 2;

                double parrychance = dashing.GuardSource.GetModified(eProperty.ParryChance);

                if (parrychance != double.MinValue)
                {
                    parrychance *= 0.001;

                    if (parrychance > 0.99)
                        parrychance = 0.99;

                    if (Attackers.Count > 1)
                        parrychance /= Attackers.Count / 2;
                }

                if (Util.ChanceDouble(guardchance))
                {
                    ad.Target = dashing.GuardSource;
                    result = eAttackResult.Blocked;
                    return true;
                }
                else if (Util.ChanceDouble(parrychance))
                {
                    ad.Target = dashing.GuardSource;
                    result = eAttackResult.Parried;
                    return true;
                }
            }
            else
            {
                double parrychance = dashing.GuardSource.GetModified(eProperty.ParryChance);

                if (parrychance != double.MinValue)
                {
                    parrychance *= 0.001;

                    if (parrychance > 0.99)
                        parrychance = 0.99;

                    if (Attackers.Count > 1)
                        parrychance /= Attackers.Count / 2;
                }

                if (Util.ChanceDouble(parrychance))
                {
                    ad.Target = dashing.GuardSource;
                    result = eAttackResult.Parried;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the result of an enemy attack
        /// </summary>
        public virtual eAttackResult CalculateEnemyAttackResult(WeaponAction action, AttackData ad, DbInventoryItem attackerWeapon, ref double effectiveness)
        {
            if (owner.EffectList.CountOfType<NecromancerShadeEffect>() > 0)
                return eAttackResult.NoValidTarget;

            //1.To-Hit modifiers on styles do not any effect on whether your opponent successfully Evades, Blocks, or Parries.  Grab Bag 2/27/03
            //2.The correct Order of Resolution in combat is Intercept, Evade, Parry, Block (Shield), Guard, Hit/Miss, and then Bladeturn.  Grab Bag 2/27/03, Grab Bag 4/4/03
            //3.For every person attacking a monster, a small bonus is applied to each player's chance to hit the enemy. Allowances are made for those who don't technically hit things when they are participating in the raid  for example, a healer gets credit for attacking a monster when he heals someone who is attacking the monster, because that's what he does in a battle.  Grab Bag 6/6/03
            //4.Block, parry, and bolt attacks are affected by this code, as you know. We made a fix to how the code counts people as "in combat." Before this patch, everyone grouped and on the raid was counted as "in combat." The guy AFK getting Mountain Dew was in combat, the level five guy hovering in the back and hoovering up some exp was in combat  if they were grouped with SOMEONE fighting, they were in combat. This was a bad thing for block, parry, and bolt users, and so we fixed it.  Grab Bag 6/6/03
            //5.Positional degrees - Side Positional combat styles now will work an extra 15 degrees towards the rear of an opponent, and rear position styles work in a 60 degree arc rather than the original 90 degree standard. This change should even out the difficulty between side and rear positional combat styles, which have the same damage bonus. Please note that front positional styles are not affected by this change. 1.62
            //http://daoc.catacombs.com/forum.cfm?ThreadKey=511&DefMessage=681444&forum=DAOCMainForum#Defense

            InterceptECSGameEffect intercept = null;
            // ML effects
            GameSpellEffect phaseshift = null;
            GameSpellEffect grapple = null;
            GameSpellEffect brittleguard = null;

            AttackData lastAttackData = owner.attackComponent.attackAction.LastAttackData;
            bool defenseDisabled = ad.Target.IsMezzed | ad.Target.IsStunned | ad.Target.IsSitting;

            IGamePlayer playerOwner = owner as IGamePlayer;
            IGamePlayer playerAttacker = ad.Attacker as IGamePlayer;

            if (EffectListService.GetAbilityEffectOnTarget(owner, eEffect.Berserk) != null)
                defenseDisabled = true;

            // We check if interceptor can intercept.
            foreach (InterceptECSGameEffect inter in owner.effectListComponent.GetAbilityEffects().Where(e => e is InterceptECSGameEffect))
            {
                if (inter.Target == owner && !inter.Source.IsIncapacitated && !inter.Source.IsSitting && owner.IsWithinRadius(inter.Source, InterceptAbilityHandler.INTERCEPT_DISTANCE))
                {
                    double interceptRoll;

                    if (!Properties.OVERRIDE_DECK_RNG && playerOwner != null)
                        interceptRoll = playerOwner.RandomNumberDeck.GetPseudoDouble();
                    else
                        interceptRoll = Util.CryptoNextDouble();

                    interceptRoll *= 100;

                    if (inter.InterceptChance > interceptRoll)
                    {
                        intercept = inter;
                        break;
                    }
                }
            }

            bool stealthStyle = false;

            if (ad.Style != null && ad.Style.StealthRequirement && playerAttacker != null && StyleProcessor.CanUseStyle(lastAttackData, (GameLiving)playerAttacker, ad.Style, attackerWeapon))
            {
                stealthStyle = true;
                defenseDisabled = true;
                intercept = null;
                brittleguard = null;
            }

            if (playerOwner != null && playerOwner is GamePlayer gamePlayer)
            {
                GameLiving attacker = ad.Attacker;
                GamePlayer tempPlayerAttacker = gamePlayer ?? ((attacker as GameNPC)?.Brain as IControlledBrain)?.GetPlayerOwner();

                if (tempPlayerAttacker != null && action.ActiveWeaponSlot != eActiveWeaponSlot.Distance)
                {
                    GamePlayer bodyguard = gamePlayer.Bodyguard;

                    if (bodyguard != null)
                    {
                        gamePlayer.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(playerOwner.Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.YouWereProtected"), bodyguard.Name, attacker.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                        bodyguard.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(bodyguard.Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.YouHaveProtected"), gamePlayer.Name, attacker.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);

                        if (attacker == tempPlayerAttacker)
                            tempPlayerAttacker.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(tempPlayerAttacker.Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.YouAttempt"), gamePlayer.Name, gamePlayer.Name, bodyguard.Name), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        else
                            tempPlayerAttacker.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(tempPlayerAttacker.Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.YourPetAttempts"), gamePlayer.Name, gamePlayer.Name, bodyguard.Name), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

                        return eAttackResult.Bodyguarded;
                    }
                }
            }

            if (phaseshift != null)
                return eAttackResult.Missed;

            if (grapple != null)
                return eAttackResult.Grappled;

            if (brittleguard != null)
            {
                playerOwner?.Out.SendMessage(LanguageMgr.GetTranslation(playerOwner.Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.BlowIntercepted"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                playerAttacker?.Out.SendMessage(LanguageMgr.GetTranslation(playerAttacker.Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.StrikeIntercepted"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                brittleguard.Cancel(false);
                return eAttackResult.Missed;
            }

            if (intercept != null && !stealthStyle)
            {
                if (intercept.Source is not IGamePlayer || EffectService.RequestImmediateCancelEffect(intercept))
                {
                    ad.Target = intercept.Source;
                    return eAttackResult.HitUnstyled;
                }

                intercept = null;
            }

            ad.DefensePenetration = ad.Attacker.attackComponent.CalculateDefensePenetration(ad.Weapon, ad.Target.Level);

            if (!defenseDisabled)
            {
                if (lastAttackData != null && lastAttackData.AttackResult is not eAttackResult.HitStyle)
                    lastAttackData = null;

                double evadeChance = owner.TryEvade(ad, lastAttackData, Attackers.Count);
                ad.EvadeChance = evadeChance * 100;
                double evadeRoll;

                if (!Properties.OVERRIDE_DECK_RNG && playerOwner != null)
                    evadeRoll = playerOwner.RandomNumberDeck.GetPseudoDouble();
                else
                    evadeRoll = Util.CryptoNextDouble();

                if (evadeChance > 0)
                {
                    if (ad.Attacker is GamePlayer evadeAtk && evadeAtk.UseDetailedCombatLog)
                        evadeAtk.Out.SendMessage($"target evade%: {evadeChance * 100:0.##} rand: {evadeRoll * 100:0.##}", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);

                    if (ad.Target is GamePlayer evadeTarg && evadeTarg.UseDetailedCombatLog)
                        evadeTarg.Out.SendMessage($"your evade%: {evadeChance * 100:0.##} rand: {evadeRoll * 100:0.##}", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);

                    if (evadeChance > evadeRoll)
                        return eAttackResult.Evaded;
                }

                if (ad.IsMeleeAttack)
                {
                    double parryChance = owner.TryParry(ad, lastAttackData, Attackers.Count);
                    ad.ParryChance = parryChance * 100;
                    double parryRoll;

                    if (!Properties.OVERRIDE_DECK_RNG && playerOwner != null)
                        parryRoll = playerOwner.RandomNumberDeck.GetPseudoDouble();
                    else
                        parryRoll = Util.CryptoNextDouble();

                    if (parryChance > 0)
                    {
                        if (ad.Attacker is GamePlayer parryAtk && parryAtk.UseDetailedCombatLog)
                            parryAtk.Out.SendMessage($"target parry%: {parryChance * 100:0.##} rand: {parryRoll * 100:0.##}", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);

                        if (ad.Target is GamePlayer parryTarg && parryTarg.UseDetailedCombatLog)
                            parryTarg.Out.SendMessage($"your parry%: {parryChance * 100:0.##} rand: {parryRoll * 100:0.##}", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);

                        if (parryChance > parryRoll)
                            return eAttackResult.Parried;
                    }
                }

                if (CheckBlock(ad))
                    return eAttackResult.Blocked;
            }

            if (CheckGuard(ad, stealthStyle))
                return eAttackResult.Blocked;

            // Not implemented.
            // if (CheckDashingDefense(ad, stealthStyle, out eAttackResult result)
            //     return result;

            double missChance = Math.Min(1, GetMissChance(action, ad, lastAttackData, attackerWeapon) * 0.01);
            double fumbleChance = ad.IsMeleeAttack ? Math.Min(1, ad.Attacker.GetModified(eProperty.FumbleChance) * 0.001) : 0;

            // At some point during Atlas' development it was decided to make fumbles a subset of misses (can't fumble without a miss), since otherwise the miss + fumble rate at low level is way too high.
            // However, this prevented fumble debuffs from working properly when fumble chance became higher than the miss chance.
            // To solve this, an extra early fumble check was added when the attacker is affected by Dirty Tricks, but this effectively made fumble chance be checked twice and made Dirty Tricks way stronger than it should be.
            // But we want to keep fumbles as a subset of misses. The solution is then to ensure miss chance can't be lower than fumble chance.
            // This however means that when miss chance is equal to fumble chance, the attacker can no longer technically miss, and can only fumble.
            // It also means that a level 50 player will always have at least 0.1% chance to fumble even against a very low level target.
            if (missChance < fumbleChance)
                missChance = fumbleChance;

            ad.MissChance = missChance * 100;

            if (missChance > 0)
            {
                double missRoll;

                if (!Properties.OVERRIDE_DECK_RNG && playerAttacker != null)
                    missRoll = playerAttacker.RandomNumberDeck.GetPseudoDouble();
                else
                    missRoll = Util.CryptoNextDouble();

                if (playerAttacker is GamePlayer {UseDetailedCombatLog:true})
                {
                    playerAttacker.Out.SendMessage($"miss rate: {missChance * 100:0.##}% rand: {missRoll * 100:0.##}", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);

                    if (fumbleChance > 0)
                        playerAttacker.Out.SendMessage($"chance to fumble: {fumbleChance * 100:0.##}% rand: {missRoll * 100:0.##}", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);
                }

                if (ad.Target is GamePlayer playerTarget && playerTarget.UseDetailedCombatLog)
                    playerTarget.Out.SendMessage($"chance to be missed: {missChance * 100:0.##}% rand: {missRoll * 100:0.##}", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);

                if (missChance > missRoll)
                    return fumbleChance > missRoll ? eAttackResult.Fumbled : eAttackResult.Missed;
            }

            // Bladeturn
            // TODO: high level mob attackers penetrate bt, players are tested and do not penetrate (lv30 vs lv20)
            /*
             * http://www.camelotherald.com/more/31.shtml
             * - Bladeturns can now be penetrated by attacks from higher level monsters and
             * players. The chance of the bladeturn deflecting a higher level attack is
             * approximately the caster's level / the attacker's level.
             * Please be aware that everything in the game is
             * level/chance based - nothing works 100% of the time in all cases.
             * It was a bug that caused it to work 100% of the time - now it takes the
             * levels of the players involved into account.
             */

            if (EffectListService.GetSpellEffectOnTarget(owner, eEffect.Bladeturn) is BladeturnECSGameEffect bladeturn)
            {
                bool penetrate = false;

                if (stealthStyle)
                    return eAttackResult.HitUnstyled; // Exit early for stealth to prevent breaking bubble but still register a hit.

                if (ad.Attacker.Level > bladeturn.SpellHandler.Caster.Level && !Util.ChanceDouble(bladeturn.SpellHandler.Caster.Level / (double) ad.Attacker.Level))
                    penetrate = true;
                else if (ad.AttackType is AttackData.eAttackType.Ranged)
                {
                    double effectivenessAgainstBladeturn = CheckEffectivenessAgainstBladeturn(bladeturn);

                    if (effectivenessAgainstBladeturn > 0)
                        penetrate = true;

                    effectiveness *= effectivenessAgainstBladeturn;
                }

                if (EffectService.RequestImmediateCancelEffect(bladeturn))
                {
                    if (penetrate)
                        playerOwner?.Out.SendMessage(LanguageMgr.GetTranslation(playerOwner.Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.BlowPenetrated"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    else
                    {
                        playerAttacker?.Out.SendMessage(LanguageMgr.GetTranslation(playerAttacker.Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.StrikeAbsorbed"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);

                        if (playerOwner != null)
                        {
                        playerOwner?.Out.SendMessage(LanguageMgr.GetTranslation(playerOwner.Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.BlowAbsorbed"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        playerOwner?.Stealth(false);
                        }

                        return eAttackResult.Missed;
                    }
                }
            }

            //if (playerOwner.IsOnHorse)
            //    playerOwner.IsOnHorse = false;

            return eAttackResult.HitUnstyled;

            double CheckEffectivenessAgainstBladeturn(ECSGameSpellEffect bladeturn)
            {
                // 1.62: Longshot and Volley always penetrate.
                if (action.RangedAttackType is eRangedAttackType.Long or eRangedAttackType.Volley)
                    return 1.0;

                // 1.62: Penetrating Arrow penetrates only if the caster != target.
                if (owner == bladeturn.SpellHandler.Caster)
                    return 0.0;

                return 0.25 + ad.Attacker.GetAbilityLevel(Abilities.PenetratingArrow) * 0.25;
            }
        }

        private int GetBonusCapForLevel(int level)
        {
            int bonusCap = 0;
            if (level < 15) bonusCap = 0;
            else if (level < 20) bonusCap = 5;
            else if (level < 25) bonusCap = 10;
            else if (level < 30) bonusCap = 15;
            else if (level < 35) bonusCap = 20;
            else if (level < 40) bonusCap = 25;
            else if (level < 45) bonusCap = 30;
            else bonusCap = 35;

            return bonusCap;
        }

        /// <summary>
        /// Send the messages to the GamePlayer
        /// </summary>
        public void SendAttackingCombatMessages(WeaponAction action, AttackData ad)
        {
            // Used to prevent combat log spam when the target is out of range, dead, not visible, etc.
            // A null attackAction means it was cleared up before we had a chance to send combat messages.
            // This typically happens when a ranged weapon is shot once without auto reloading.
            // In this case, we simply assume the last round should show a combat message.
            if (ad.AttackResult is not eAttackResult.Missed
                and not eAttackResult.HitUnstyled
                and not eAttackResult.HitStyle
                and not eAttackResult.Evaded
                and not eAttackResult.Blocked
                and not eAttackResult.Parried)
            {
                if (GameLoop.GameLoopTime - attackAction.RoundWithNoAttackTime <= 1500)
                    return;

                attackAction.RoundWithNoAttackTime = 0;
            }

            if (owner is GamePlayer)
            {
                var p = owner as GamePlayer;

                GameObject target = ad.Target;
                DbInventoryItem weapon = ad.Weapon;
                if (ad.Target is GameNPC)
                {
                    switch (ad.AttackResult)
                    {
                        case eAttackResult.TargetNotVisible:
                        p.Out.SendMessage(LanguageMgr.GetTranslation(p.Client.Account.Language,
                                "GamePlayer.Attack.NotInView",
                                ad.Target.GetName(0, true, p.Client.Account.Language, (ad.Target as GameNPC))),
                            eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        break;

                        case eAttackResult.OutOfRange:
                        p.Out.SendMessage(LanguageMgr.GetTranslation(p.Client.Account.Language,
                                "GamePlayer.Attack.TooFarAway",
                                ad.Target.GetName(0, true, p.Client.Account.Language, (ad.Target as GameNPC))),
                            eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        break;

                        case eAttackResult.TargetDead:
                        p.Out.SendMessage(LanguageMgr.GetTranslation(p.Client.Account.Language,
                                "GamePlayer.Attack.AlreadyDead",
                                ad.Target.GetName(0, true, p.Client.Account.Language, (ad.Target as GameNPC))),
                            eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        break;

                        case eAttackResult.Blocked:
                        p.Out.SendMessage(LanguageMgr.GetTranslation(p.Client.Account.Language,
                                "GamePlayer.Attack.Blocked",
                                ad.Target.GetName(0, true, p.Client.Account.Language, (ad.Target as GameNPC))),
                            eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        break;

                        case eAttackResult.Parried:
                        p.Out.SendMessage(LanguageMgr.GetTranslation(p.Client.Account.Language,
                                "GamePlayer.Attack.Parried",
                                ad.Target.GetName(0, true, p.Client.Account.Language, (ad.Target as GameNPC))),
                            eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        break;

                        case eAttackResult.Evaded:
                        p.Out.SendMessage(LanguageMgr.GetTranslation(p.Client.Account.Language,
                                "GamePlayer.Attack.Evaded",
                                ad.Target.GetName(0, true, p.Client.Account.Language, (ad.Target as GameNPC))),
                            eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        break;

                        case eAttackResult.NoTarget:
                        p.Out.SendMessage(
                            LanguageMgr.GetTranslation(p.Client.Account.Language, "GamePlayer.Attack.NeedTarget"),
                            eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        break;

                        case eAttackResult.NoValidTarget:
                        p.Out.SendMessage(
                            LanguageMgr.GetTranslation(p.Client.Account.Language,
                                "GamePlayer.Attack.CantBeAttacked"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        break;

                        case eAttackResult.Missed:
                            string message;
                            if (ad.MissChance > 0)
                                message = LanguageMgr.GetTranslation(p.Client.Account.Language, "GamePlayer.Attack.Miss") + $" ({ad.MissChance:0.##}%)";
                            else
                                message = LanguageMgr.GetTranslation(p.Client.Account.Language, "GamePlayer.Attack.StrafMiss");
                            p.Out.SendMessage(message, eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                            break;
                        case eAttackResult.Fumbled:
                        p.Out.SendMessage(
                            LanguageMgr.GetTranslation(p.Client.Account.Language, "GamePlayer.Attack.Fumble"),
                            eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        break;

                        case eAttackResult.HitStyle:
                        case eAttackResult.HitUnstyled:
                        string modMessage;

                        if (ad.Modifier > 0)
                            modMessage = $" (+{ad.Modifier})";
                        else if (ad.Modifier < 0)
                            modMessage = $" ({ad.Modifier})";
                        else
                            modMessage = string.Empty;

                        string hitWeapon;

                            if (weapon != null)
                            {
                                switch (p.Client.Account.Language)
                                {
                                    case "DE":
                                    {
                                        hitWeapon = $" {LanguageMgr.GetTranslation(p.Client.Account.Language, "GamePlayer.Attack.WithYour")} {weapon.Name}";
                                        break;
                                    }
                                    default:
                                    {
                                        hitWeapon = $" {LanguageMgr.GetTranslation(p.Client.Account.Language, "GamePlayer.Attack.WithYour")} {GlobalConstants.NameToShortName(weapon.Name)}";
                                        break;
                                    }
                                }
                            }
                            else
                                hitWeapon = string.Empty;

                        string attackTypeMsg;

                            if (action.ActiveWeaponSlot is eActiveWeaponSlot.Distance)
                            attackTypeMsg = LanguageMgr.GetTranslation(p.Client.Account.Language, "GamePlayer.Attack.YouShot");
                        else
                            attackTypeMsg = LanguageMgr.GetTranslation(p.Client.Account.Language, "GamePlayer.Attack.YouAttack");

                        // intercept messages
                        if (target != null && target != ad.Target)
                        {
                            p.Out.SendMessage(
                                LanguageMgr.GetTranslation(p.Client.Account.Language,
                                    "GamePlayer.Attack.Intercepted", ad.Target.GetName(0, true),
                                    target.GetName(0, false)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                            p.Out.SendMessage(
                                LanguageMgr.GetTranslation(p.Client.Account.Language,
                                    "GamePlayer.Attack.InterceptedHit", attackTypeMsg, target.GetName(0, false),
                                    hitWeapon, ad.Target.GetName(0, false), ad.Damage, modMessage),
                                eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        }
                        else
                            p.Out.SendMessage(LanguageMgr.GetTranslation(p.Client.Account.Language,
                                "GamePlayer.Attack.InterceptHit", attackTypeMsg,
                                ad.Target.GetName(0, false, p.Client.Account.Language, (ad.Target as GameNPC)),
                                hitWeapon, ad.Damage, modMessage), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

                        // critical hit
                        if (ad.CriticalDamage > 0)
                            p.Out.SendMessage(LanguageMgr.GetTranslation(p.Client.Account.Language,
                                    "GamePlayer.Attack.Critical",
                                    ad.Target.GetName(0, false, p.Client.Account.Language, (ad.Target as GameNPC)),
                                        ad.CriticalDamage) + $" ({ad.CriticalChance}%)",
                                eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        break;
                    }
                }
                else
                {
                    switch (ad.AttackResult)
                    {
                        case eAttackResult.TargetNotVisible:
                        p.Out.SendMessage(
                            LanguageMgr.GetTranslation(p.Client.Account.Language, "GamePlayer.Attack.NotInView",
                                ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        break;

                        case eAttackResult.OutOfRange:
                        p.Out.SendMessage(
                            LanguageMgr.GetTranslation(p.Client.Account.Language, "GamePlayer.Attack.TooFarAway",
                                ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        break;

                        case eAttackResult.TargetDead:
                        p.Out.SendMessage(
                            LanguageMgr.GetTranslation(p.Client.Account.Language, "GamePlayer.Attack.AlreadyDead",
                                ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        break;

                        case eAttackResult.Blocked:
                        p.Out.SendMessage(
                            LanguageMgr.GetTranslation(p.Client.Account.Language, "GamePlayer.Attack.Blocked",
                                ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        break;

                        case eAttackResult.Parried:
                        p.Out.SendMessage(
                            LanguageMgr.GetTranslation(p.Client.Account.Language, "GamePlayer.Attack.Parried",
                                ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        break;

                        case eAttackResult.Evaded:
                        p.Out.SendMessage(
                            LanguageMgr.GetTranslation(p.Client.Account.Language, "GamePlayer.Attack.Evaded",
                                ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        break;

                        case eAttackResult.NoTarget:
                        p.Out.SendMessage(
                            LanguageMgr.GetTranslation(p.Client.Account.Language, "GamePlayer.Attack.NeedTarget"),
                            eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        break;

                        case eAttackResult.NoValidTarget:
                        p.Out.SendMessage(
                            LanguageMgr.GetTranslation(p.Client.Account.Language,
                                "GamePlayer.Attack.CantBeAttacked"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        break;

                        case eAttackResult.Missed:
                            string message;
                            if (ad.MissChance > 0)
                                message = LanguageMgr.GetTranslation(p.Client.Account.Language, "GamePlayer.Attack.Miss") + $" ({ad.MissChance:0.##}%)";
                            else
                                message = LanguageMgr.GetTranslation(p.Client.Account.Language, "GamePlayer.Attack.StrafMiss");
                            p.Out.SendMessage(message, eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                            break;
                        case eAttackResult.Fumbled:
                        p.Out.SendMessage(
                            LanguageMgr.GetTranslation(p.Client.Account.Language, "GamePlayer.Attack.Fumble"),
                            eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        break;

                        case eAttackResult.HitStyle:
                        case eAttackResult.HitUnstyled:
                        string modMessage;

                        if (ad.Modifier > 0)
                            modMessage = $" (+{ad.Modifier})";
                        else if (ad.Modifier < 0)
                            modMessage = $" ({ad.Modifier})";
                        else
                            modMessage = string.Empty;

                        string hitWeapon;

                            if (weapon != null)
                            {
                                switch (p.Client.Account.Language)
                                {
                                    case "DE":
                                    {
                                        hitWeapon = $" {LanguageMgr.GetTranslation(p.Client.Account.Language, "GamePlayer.Attack.WithYour")} {weapon.Name}";
                                        break;
                                    }
                                    default:
                                    {
                                        hitWeapon = $" {LanguageMgr.GetTranslation(p.Client.Account.Language, "GamePlayer.Attack.WithYour")} {GlobalConstants.NameToShortName(weapon.Name)}";
                                        break;
                                    }
                                }
                            }
                            else
                                hitWeapon = string.Empty;

                            string attackTypeMsg;

                            if (action.ActiveWeaponSlot is eActiveWeaponSlot.Distance)
                                attackTypeMsg = LanguageMgr.GetTranslation(p.Client.Account.Language, "GamePlayer.Attack.YouShot");
                            else
                                attackTypeMsg = LanguageMgr.GetTranslation(p.Client.Account.Language, "GamePlayer.Attack.YouAttack");

                        // intercept messages
                        if (target != null && target != ad.Target)
                        {
                            p.Out.SendMessage(
                                LanguageMgr.GetTranslation(p.Client.Account.Language,
                                    "GamePlayer.Attack.Intercepted", ad.Target.GetName(0, true),
                                    target.GetName(0, false)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                            p.Out.SendMessage(
                                LanguageMgr.GetTranslation(p.Client.Account.Language,
                                    "GamePlayer.Attack.InterceptedHit", attackTypeMsg, target.GetName(0, false),
                                    hitWeapon, ad.Target.GetName(0, false), ad.Damage, modMessage),
                                eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        }
                        else
                            p.Out.SendMessage(
                                LanguageMgr.GetTranslation(p.Client.Account.Language,
                                    "GamePlayer.Attack.InterceptHit", attackTypeMsg, ad.Target.GetName(0, false),
                                    hitWeapon, ad.Damage, modMessage), eChatType.CT_YouHit,
                                eChatLoc.CL_SystemWindow);

                        // critical hit
                        if (ad.CriticalDamage > 0)
                            p.Out.SendMessage(
                                LanguageMgr.GetTranslation(p.Client.Account.Language,
                                    "GamePlayer.Attack.Critical", ad.Target.GetName(0, false),
                                        ad.CriticalDamage) + $" ({ad.CriticalChance}%)", eChatType.CT_YouHit,
                                eChatLoc.CL_SystemWindow);
                        break;
                    }
                }
            }
        }

        public int CalculateCriticalDamage(AttackData ad)
        {
            if (!Util.Chance(ad.CriticalChance))
                return 0;

            if (owner is IGamePlayer)
            {
                // Triple wield prevents critical hits (1.62).
                if (EffectListService.GetAbilityEffectOnTarget(ad.Target, eEffect.TripleWield) != null)
                    return 0;

                int critMin;
                int critMax;
                ECSGameEffect berserk = EffectListService.GetEffectOnTarget(owner, eEffect.Berserk);

                if (berserk != null)
                {
                    int level = owner.GetAbilityLevel(Abilities.Berserk);
                    // https://web.archive.org/web/20061017095337/http://daoc.catacombs.com/forum.cfm?ThreadKey=10833&DefMessage=922046&forum=37
                    // 1% min is weird. Raised to 10%.
                    // Berserk 1 = 10-25%
                    // Berserk 2 = 10-50%
                    // Berserk 3 = 10-75%
                    // Berserk 4 = 10-99%
                    critMin = (int) (ad.Damage * 0.1);
                    critMax = (int)(Math.Min(0.99, level * 0.25) * ad.Damage);
                }
                else
                {
                    // Min crit damage is 10%.
                    critMin = (int) (ad.Damage * 0.1);

                    // Max crit damage to players is 50%.
                    if (ad.Target is IGamePlayer)
                        critMax = ad.Damage / 2;
                    else
                        critMax = ad.Damage;
                }

                critMin = Math.Max(critMin, 0);
                critMax = Math.Max(critMin, critMax);
                return Util.Random(critMin, critMax);
            }
            else
            {
                int maxCriticalDamage = ad.Target is IGamePlayer ? ad.Damage / 2 : ad.Damage;
                int minCriticalDamage = (int)(ad.Damage * MinMeleeCriticalDamage);

                if (minCriticalDamage > maxCriticalDamage)
                    minCriticalDamage = maxCriticalDamage;

                return Util.Random(minCriticalDamage, maxCriticalDamage);
            }
        }

        public double GetMissChance(WeaponAction action, AttackData ad, AttackData lastAD, DbInventoryItem weapon)
        {
            // No miss if the target is sitting or for Volley attacks.
            if ((owner is IGamePlayer player && player.IsSitting) || action.RangedAttackType is eRangedAttackType.Volley)
                return 0;

            // In 1.117C, every weapon was given the intrinsic 5% flat bonus special weapons (such as artifacts) had, lowering the base miss rate to 13%.
            double missChance = 18;
            missChance -= ad.Attacker.GetModified(eProperty.ToHitBonus);

            if (owner is not IGamePlayer || ad.Attacker is not IGamePlayer)
            {
                // 1.33 per level difference.
                missChance -= (ad.Attacker.EffectiveLevel - owner.EffectiveLevel) * (1 + 1 / 3.0);
                missChance -= Math.Max(0, Attackers.Count - 1) * Properties.MISSRATE_REDUCTION_PER_ATTACKERS;
            }

            // Weapon and armor bonuses.
            int armorBonus = 0;

            if (ad.Target is IGamePlayer playerTarget)
            {
                ad.ArmorHitLocation = playerTarget.CalculateArmorHitLocation(ad);

                if (ad.Target.Inventory != null)
                {
                    DbInventoryItem armor = ad.Target.Inventory.GetItem((eInventorySlot)ad.ArmorHitLocation);

                    if (armor != null)
                        armorBonus = armor.Bonus;
                }

                int bonusCap = GetBonusCapForLevel(playerTarget.Level);

                if (armorBonus > bonusCap)
                    armorBonus = bonusCap;
            }

            if (weapon != null)
            {
                int bonusCap = GetBonusCapForLevel(ad.Attacker.Level);
                int weaponBonus = weapon.Bonus;

                if (weaponBonus > bonusCap)
                    weaponBonus = bonusCap;

                armorBonus -= weaponBonus;
            }

            if (ad.Target is IGamePlayer && ad.Attacker is IGamePlayer)
                missChance += armorBonus;
            else
                missChance += missChance * armorBonus / 100;

            // Style bonuses.
            if (ad.Style != null)
                missChance -= ad.Style.BonusToHit;

            if (lastAD != null && lastAD.AttackResult is eAttackResult.HitStyle && lastAD.Style != null)
                missChance += lastAD.Style.BonusToDefense;

            if (action.ActiveWeaponSlot is eActiveWeaponSlot.Distance)
            {
                DbInventoryItem ammo = GetAttackAmmo();

                if (ammo != null)
                {
                    switch ((ammo.SPD_ABS >> 4) & 0x3)
                    {
                        // http://rothwellhome.org/guides/archery.htm
                        case 0:
                            missChance += missChance * 0.15;
                        break; // Rough
                        //case 1:
                        //  break;
                        case 2:
                            missChance -= missChance * 0.15;
                            break; // Doesn't exist (?)
                        case 3:
                            missChance -= missChance * 0.25;
                        break; // Footed
                    }
                }
            }

            return missChance;
        }

        /// <summary>
        /// Minimum melee critical damage as a percentage of the
        /// raw damage.
        /// </summary>
        protected float MinMeleeCriticalDamage => 0.1f;

        public static double CalculateSlowWeaponDamageModifier(DbInventoryItem weapon)
        {
            // Slow weapon bonus as found here: https://www2.uthgard.net/tracker/issue/2753/@/Bow_damage_variance_issue_(taking_item_/_spec_???)
            return 1 + (weapon.SPD_ABS - 20) * 0.003;
        }

        public double CalculateTwoHandedDamageModifier(DbInventoryItem weapon)
        {
            return 1.1 + owner.WeaponSpecLevel(weapon) * 0.005;
        }

        /// <summary>
        /// Checks whether Living has ability to use lefthanded weapons
        /// </summary>
        public bool CanUseLefthandedWeapon
        {
            get
            {
                if (owner is IGamePlayer playerOwner)
                    return playerOwner.CharacterClass.CanUseLefthandedWeapon;
                else if (owner is GameNPC)
                    return true;

                return false;
            }
        }

        public double CalculateDwCdLeftHandSwingChance()
        {
            int specLevel = owner.GetModifiedSpecLevel(Specs.Dual_Wield);
            specLevel = Math.Max(specLevel, owner.GetModifiedSpecLevel(Specs.Celtic_Dual));
            specLevel = Math.Max(specLevel, owner.GetModifiedSpecLevel(Specs.Fist_Wraps));

            if (specLevel > 0)
            {
                int bonus = owner.GetModified(eProperty.OffhandChance) + owner.GetModified(eProperty.OffhandDamageAndChance);
                return 25 + specLevel * 68 * 0.01 + bonus;
            }

            return 0;
        }

        public (double, double, double) CalculateHthSwingChances(DbInventoryItem leftWeapon)
        {
            int specLevel = owner.GetModifiedSpecLevel(Specs.HandToHand);

            if (specLevel <= 0 || (eObjectType) leftWeapon.Object_Type is not eObjectType.HandToHand)
                return (0, 0, 0);

            double doubleSwingChance = specLevel * 0.5; // specLevel >> 1
            double tripleSwingChance = specLevel >= 25 ? doubleSwingChance * 0.5 : 0; // specLevel >> 2
            double quadSwingChance = specLevel >= 40 ? tripleSwingChance * 0.25 : 0; // specLevel >> 4
            int bonus = owner.GetModified(eProperty.OffhandChance) + owner.GetModified(eProperty.OffhandDamageAndChance);
            doubleSwingChance += bonus; // It's apparently supposed to only affect double swing chance around 1.65, which puts it more in line with DW / CD.
            return (doubleSwingChance, tripleSwingChance, quadSwingChance);
        }

        /// <summary>
        /// Calculates how many times left hand swings
        /// </summary>
        public int CalculateLeftHandSwingCount(DbInventoryItem mainWeapon, DbInventoryItem leftWeapon)
        {
            // Let's make NPCs require an actual weapon too. It looks silly otherwise.
            if (!CanUseLefthandedWeapon || leftWeapon == null || (eObjectType) leftWeapon.Object_Type is eObjectType.Shield)
                return 0;

            if (owner is GameNPC npcOwner)
            {
                double random = Util.RandomDouble() * 100;
                return random < npcOwner.LeftHandSwingChance ? 1 : 0;
            }

            if (owner is not GamePlayer playerOwner || (eObjectType) leftWeapon.Object_Type is eObjectType.Shield || mainWeapon == null)
                return 0;

            if (owner.GetBaseSpecLevel(Specs.Left_Axe) > 0)
            {
                if (playerOwner is {UseDetailedCombatLog: true})
                {
                    // This shouldn't be done here.
                    double effectiveness = CalculateLeftAxeModifier();
                    playerOwner.Out.SendMessage($"{Math.Round(effectiveness * 100, 2)}% dmg (after LA penalty)\n", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);
                }

                return 1; // Always use left axe.
            }

            double leftHandSwingChance = CalculateDwCdLeftHandSwingChance();

            if (leftHandSwingChance > 0)
            {
                double random = Util.RandomDouble() * 100;

                if (playerOwner != null && playerOwner.UseDetailedCombatLog)
                    playerOwner.Out.SendMessage($"OH swing%: {leftHandSwingChance:0.##}\n", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);

                return random < leftHandSwingChance ? 1 : 0;
            }

            (double doubleSwingChance, double tripleSwingChance, double quadSwingChance) hthSwingChances = CalculateHthSwingChances(leftWeapon);

            if (hthSwingChances.doubleSwingChance > 0)
            {
                double random = Util.RandomDouble() * 100;

                if (playerOwner != null && playerOwner.UseDetailedCombatLog)
                    playerOwner.Out.SendMessage( $"Chance for 2 swings: {hthSwingChances.doubleSwingChance:0.##}% | 3 swings: {hthSwingChances.tripleSwingChance:0.##}% | 4 swings: {hthSwingChances.quadSwingChance:0.##}% \n", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);

                if (random < hthSwingChances.doubleSwingChance)
                    return 1;

                hthSwingChances.tripleSwingChance += hthSwingChances.doubleSwingChance;

                if (random < hthSwingChances.tripleSwingChance)
                    return 2;

                hthSwingChances.quadSwingChance += hthSwingChances.tripleSwingChance;

                if (random < hthSwingChances.quadSwingChance)
                    return 3;
            }

            return 0;
        }

        public double CalculateLeftAxeModifier()
        {
            int LeftAxeSpec = owner.GetModifiedSpecLevel(Specs.Left_Axe);

            if (LeftAxeSpec == 0)
                return 1.0;

            double modifier = 0.625 + 0.0034 * LeftAxeSpec;

            if (owner.GetModified(eProperty.OffhandDamageAndChance) > 0)
                return modifier + owner.GetModified(eProperty.OffhandDamageAndChance) * 0.01;

            return modifier;
        }

        public class BlockRoundHandler
        {
            private GameObject _owner;
            private int _usedBlockRoundCount;

            public BlockRoundHandler(GameObject owner)
            {
                _owner = owner;
            }

            public bool Consume(int shieldSize, AttackData attackData)
            {
                // Block rounds work from the point of view of the attacker and use their attack speed, similar to how interrupts work.
                // However, according to grab bags, it's supposed to be based on the defender's swing speed. But this sounds very wrong, since it implies haste buffs should make blocking more effective.

                if (attackData.Target is not GamePlayer)
                    return true;

                // There is no need to make dual wield even more effective against shields.
                // Returning true allow the off-hand of dual wield attacks to be blocked without consuming a block.
                if (attackData.AttackType is AttackData.eAttackType.MeleeDualWield && attackData.IsOffHand)
                    return true;

                // Many threads can enter this block simultaneously, so we increment the count first, then decrement if we've overshot the shield size.
                if (Interlocked.Increment(ref _usedBlockRoundCount) > shieldSize)
                {
                    Relinquish();
                    return false;
                }

                // Decrement the count after a duration equal to the attack interval.
                // We need to make sure it ticks before the attacker's next attack. We can't use `AttackData.Interval` only because `AttackAction.NextTick` is adjusted by `ServiceUtil.ShouldTickAdjust`.
                new BlockRoundCountDecrementTimer(_owner, Relinquish).Start((int) (attackData.Attacker.attackComponent.attackAction.NextTick - GameLoop.GameLoopTime + attackData.Interval));
                return true;
            }

            private void Relinquish()
            {
                Interlocked.Decrement(ref _usedBlockRoundCount);
            }

            class BlockRoundCountDecrementTimer : ECSGameTimerWrapperBase
            {
                private Action _decrementBlockRoundCount;

                public BlockRoundCountDecrementTimer(GameObject owner, Action decrementBlockRoundCount) : base(owner)
                {
                    _decrementBlockRoundCount = decrementBlockRoundCount;
                }

                protected override int OnTick(ECSGameTimer timer)
                {
                    _decrementBlockRoundCount();
                    return 0;
                }
            }
        }

        public class StandardAttackersCheckTimer : AttackersCheckTimer
        {
            public StandardAttackersCheckTimer(GameObject owner) : base(owner) { }

            protected override int OnTick(ECSGameTimer timer)
            {
                foreach (var pair in _owner.attackComponent.Attackers)
                    TryRemoveAttacker(pair);

                return base.OnTick(timer);
            }
        }

        public class EpicNpcAttackersCheckTimer : AttackersCheckTimer
        {
            private IGameEpicNpc _epicNpc;

            public EpicNpcAttackersCheckTimer(GameObject owner) : base(owner)
            {
                _epicNpc = owner as IGameEpicNpc;
            }

            protected override int OnTick(ECSGameTimer timer)
            {
                // Update `ArmorFactorScalingFactor`.
                double armorFactorScalingFactor = _epicNpc.DefaultArmorFactorScalingFactor;
                int petCount = 0;

                foreach (var pair in _owner.attackComponent.Attackers)
                {
                    if (TryRemoveAttacker(pair))
                        continue;

                    if (pair.Key is IGamePlayer)
                        armorFactorScalingFactor -= 0.04;
                    else if (pair.Key is GameSummonedPet && petCount <= _epicNpc.ArmorFactorScalingFactorPetCap)
                    {
                        armorFactorScalingFactor -= 0.01;
                        petCount++;
                    }

                    if (armorFactorScalingFactor < 0.4)
                    {
                        armorFactorScalingFactor = 0.4;
                        break;
                    }
                }

                _epicNpc.ArmorFactorScalingFactor = armorFactorScalingFactor;
                return base.OnTick(timer);
            }
        }

        public abstract class AttackersCheckTimer : ECSGameTimerWrapperBase
        {
            public readonly Lock Lock = new();
            protected GameLiving _owner;

            public AttackersCheckTimer(GameObject owner) : base(owner)
            {
                _owner = owner as GameLiving;
            }

            public static AttackersCheckTimer Create(GameLiving owner)
            {
                return owner is IGameEpicNpc ? new EpicNpcAttackersCheckTimer(owner) : new StandardAttackersCheckTimer(owner);
            }

            protected override int OnTick(ECSGameTimer timer)
            {
                return _owner.attackComponent.Attackers.IsEmpty ? 0 : CHECK_ATTACKERS_INTERVAL;
            }

            protected bool TryRemoveAttacker(in KeyValuePair<GameLiving, long> pair)
            {
                return pair.Value < GameLoop.GameLoopTime && _owner.attackComponent.Attackers.TryRemove(pair);
            }
        }
    }
}