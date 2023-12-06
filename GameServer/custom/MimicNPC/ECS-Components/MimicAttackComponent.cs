using DOL.AI.Brain;
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
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using static DOL.GS.GameLiving;
using static DOL.GS.GameObject;

namespace DOL.GS
{
    public class MimicAttackComponent : AttackComponent, IManagedEntity
    {
        public MimicAttackComponent(GameLiving owner) : base(owner)
        { }

        /// <summary>
        /// The chance for a critical hit
        /// </summary>
        /// <param name="weapon">attack weapon</param>
        public override int AttackCriticalChance(WeaponAction action, DbInventoryItem weapon)
        {
            if (owner is GamePlayer || owner is MimicNPC)
            {
                if (weapon != null)
                {
                    if (weapon.Item_Type != Slot.RANGED)
                        return owner.GetModified(eProperty.CriticalMeleeHitChance);
                    else
                    {
                        if (action.RangedAttackType == eRangedAttackType.Critical)
                            return 0;
                        else
                            return owner.GetModified(eProperty.CriticalArcheryHitChance);
                    }
                }

                // Base of 10% critical chance.
                return 10;
            }

            /// [Atlas - Takii] Wild Minion Implementation. We don't want any non-pet NPCs to crit.
            /// We cannot reliably check melee vs ranged here since archer pets don't necessarily have a proper weapon with the correct slot type assigned.
            /// Since Wild Minion is the only way for pets to crit and we (currently) want it to affect melee/ranged/spells, we can just rely on the Melee crit chance even for archery attacks
            /// and as a result we don't actually need to detect melee vs ranged to end up with the correct behavior since all attack types will have the same % chance to crit in the end.
            if (owner is GameNPC npc)
            {
                // Player-Summoned pet.
                if (npc is GameSummonedPet summonedPet && summonedPet.Owner is GamePlayer)
                    return npc.GetModified(eProperty.CriticalMeleeHitChance);

                // Charmed Pet.
                if (npc.Brain is IControlledBrain charmedPetBrain && charmedPetBrain.GetPlayerOwner() != null)
                    return npc.GetModified(eProperty.CriticalMeleeHitChance);
            }

            return 0;
        }

        /// <summary>
        /// Returns the damage type of the current attack
        /// </summary>
        /// <param name="weapon">attack weapon</param>
        public override eDamageType AttackDamageType(DbInventoryItem weapon)
        {
            if (owner is GamePlayer || owner is MimicNPC || owner is CommanderPet)
            {
                var p = owner as GamePlayer;
                var m = owner as MimicNPC;

                if (weapon == null)
                    return eDamageType.Natural;

                switch ((eObjectType)weapon.Object_Type)
                {
                    case eObjectType.Crossbow:
                    case eObjectType.Longbow:
                    case eObjectType.CompositeBow:
                    case eObjectType.RecurvedBow:
                    case eObjectType.Fired:
                    DbInventoryItem ammo = p == null ? m.rangeAttackComponent.Ammo : p.rangeAttackComponent.Ammo;

                    if (ammo == null)
                        return (eDamageType)weapon.Type_Damage;

                    return (eDamageType)ammo.Type_Damage;

                    case eObjectType.Shield:
                    return eDamageType.Crush; // TODO: shields do crush damage (!) best is if Type_Damage is used properly
                    default:
                    return (eDamageType)weapon.Type_Damage;
                }
            }
            else if (owner is GameNPC)
                return (owner as GameNPC).MeleeDamageType;
            else
                return eDamageType.Natural;
        }

        /// <summary>
        /// Returns this attack's range
        /// </summary>
        public override int AttackRange
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
                if (owner is GamePlayer || owner is MimicNPC)
                {
                    DbInventoryItem weapon = owner.ActiveWeapon;

                    if (weapon == null)
                        return 0;

                    var player = owner as GamePlayer;
                    var mimic = owner as MimicNPC;

                    GameLiving target;

                    if (player != null)
                        target = player.TargetObject as GameLiving;
                    else
                        target = mimic.TargetObject as GameLiving;

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

                        range = Math.Max(32, range * owner.GetModified(eProperty.ArcheryRange) * 0.01);

                        DbInventoryItem ammo = mimic is null ? player.rangeAttackComponent.Ammo : mimic.rangeAttackComponent.Ammo;

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

                    // int meleeRange = 128;
                    int meleeRange = 150; // Increase default melee range to 150 to help with higher latency players.

                    if (target is GameKeepComponent)
                        meleeRange += 150;
                    else
                    {
                        if (target != null && target.IsMoving)
                            meleeRange += 32;
                        if (owner.IsMoving)
                            meleeRange += 32;
                    }

                    return meleeRange;
                }
                else
                {
                    if (owner.ActiveWeaponSlot == eActiveWeaponSlot.Distance)
                        return Math.Max(32, (int)(2000.0 * owner.GetModified(eProperty.ArcheryRange) * 0.01));

                    return 200;
                }
            }
        }

        /// <summary>
        /// Gets the current attackspeed of this living in milliseconds
        /// </summary>
        /// <returns>effective speed of the attack. average if more than one weapon.</returns>
        public override int AttackSpeed(DbInventoryItem mainWeapon, DbInventoryItem leftWeapon = null)
        {
            if (owner is GamePlayer || owner is MimicNPC)
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

                int qui = 0;

                if (owner is GamePlayer)
                    qui = Math.Min(250, ((GamePlayer)owner).Quickness); //250 soft cap on quickness
                else
                    qui = Math.Min(250, (int)((MimicNPC)owner).Quickness);

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

                        if (owner.rangeAttackComponent.RangedAttackType == eRangedAttackType.Critical)
                            speed = speed * 2 - (owner.GetAbilityLevel(Abilities.Critical_Shot) - 1) * speed / 10;
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

                // apply speed cap
                if (speed < 15)
                {
                    speed = 15;
                }

                return (int)(speed * 100);
            }
            else
            {
                double speed = NpcWeaponSpeed() * 100 * (1.0 - (owner.GetModified(eProperty.Quickness) - 60) / 500.0);
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
                    if (owner.ActiveWeaponSlot == eActiveWeaponSlot.Distance)
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

                return (int)Math.Max(500.0, speed);
            }
        }

        public override double AttackDamage(DbInventoryItem weapon, out double damageCap)
        {
            double effectiveness = 1;
            damageCap = 0;

            if (owner is GamePlayer player)
            {
                if (weapon == null)
                    return 0;

                damageCap = player.WeaponDamageWithoutQualityAndCondition(weapon) * weapon.SPD_ABS * 0.1 * CalculateSlowWeaponDamageModifier(weapon);

                if (weapon.Item_Type == Slot.RANGED)
                {
                    damageCap *= CalculateTwoHandedDamageModifier(weapon);
                    DbInventoryItem ammo = player.rangeAttackComponent.Ammo;

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

                    if (weapon.Object_Type is ((int)eObjectType.Longbow) or ((int)eObjectType.RecurvedBow) or ((int)eObjectType.CompositeBow))
                    {
                        if (Properties.ALLOW_OLD_ARCHERY)
                            effectiveness += player.GetModified(eProperty.RangedDamage) * 0.01;
                        else
                        {
                            effectiveness += owner.GetModified(eProperty.RangedDamage) * 0.01;
                            effectiveness += owner.GetModified(eProperty.SpellDamage) * 0.01;
                        }
                    }
                    else
                        effectiveness += player.GetModified(eProperty.RangedDamage) * 0.01;
                }
                else if (weapon.Item_Type is Slot.RIGHTHAND or Slot.LEFTHAND or Slot.TWOHAND)
                {
                    effectiveness += player.GetModified(eProperty.MeleeDamage) * 0.01;

                    if (weapon.Item_Type == Slot.TWOHAND)
                        damageCap *= CalculateTwoHandedDamageModifier(weapon);
                    else if (player.Inventory?.GetItem(eInventorySlot.LeftHandWeapon) != null)
                        damageCap *= CalculateLeftAxeModifier();
                }

                damageCap *= effectiveness;
                double damage = player.ApplyWeaponQualityAndConditionToDamage(weapon, damageCap);
                damageCap *= 3;
                return damage *= effectiveness;
            }
            else if (owner is MimicNPC mimic)
            {
                if (weapon == null)
                    return 0;

                damageCap = mimic.WeaponDamageWithoutQualityAndCondition(weapon) * weapon.SPD_ABS * 0.1 * CalculateSlowWeaponDamageModifier(weapon);

                if (weapon.Item_Type == Slot.RANGED)
                {
                    damageCap *= CalculateTwoHandedDamageModifier(weapon);
                    DbInventoryItem ammo = mimic.rangeAttackComponent.Ammo;

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

                    if (weapon.Object_Type is ((int)eObjectType.Longbow) or ((int)eObjectType.RecurvedBow) or ((int)eObjectType.CompositeBow))
                    {
                        if (Properties.ALLOW_OLD_ARCHERY)
                            effectiveness += mimic.GetModified(eProperty.RangedDamage) * 0.01;
                        else
                        {
                            effectiveness += owner.GetModified(eProperty.RangedDamage) * 0.01;
                            effectiveness += owner.GetModified(eProperty.SpellDamage) * 0.01;
                        }
                    }
                    else
                        effectiveness += mimic.GetModified(eProperty.RangedDamage) * 0.01;
                }
                else if (weapon.Item_Type is Slot.RIGHTHAND or Slot.LEFTHAND or Slot.TWOHAND)
                {
                    effectiveness += mimic.GetModified(eProperty.MeleeDamage) * 0.01;

                    if (weapon.Item_Type == Slot.TWOHAND)
                        damageCap *= CalculateTwoHandedDamageModifier(weapon);
                    else if (mimic.Inventory?.GetItem(eInventorySlot.LeftHandWeapon) != null)
                        damageCap *= CalculateLeftAxeModifier();
                }

                damageCap *= effectiveness;
                double damage = mimic.ApplyWeaponQualityAndConditionToDamage(weapon, damageCap);
                damageCap *= 3;
                return damage *= effectiveness;
            }
            else
            {
                double damage = (1.0 + owner.Level / Properties.PVE_MOB_DAMAGE_F1 + owner.Level * owner.Level / Properties.PVE_MOB_DAMAGE_F2) * NpcWeaponSpeed() * 0.1;

                if (weapon == null ||
                    weapon.SlotPosition == Slot.RIGHTHAND ||
                    weapon.SlotPosition == Slot.LEFTHAND ||
                    weapon.SlotPosition == Slot.TWOHAND)
                {
                    effectiveness += owner.GetModified(eProperty.MeleeDamage) * 0.01;
                }
                else if (weapon.SlotPosition == Slot.RANGED)
                {
                    if (weapon.Object_Type is ((int)eObjectType.Longbow) or ((int)eObjectType.RecurvedBow) or ((int)eObjectType.CompositeBow))
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

                if (owner is GameEpicBoss epicBoss)
                    damageCap = damage + epicBoss.Empathy / 100.0 * Properties.SET_EPIC_ENCOUNTER_WEAPON_DAMAGE_CAP;
                else
                    damageCap = damage * 3;

                return damage;
            }
        }

        /// <summary>
        /// Called whenever a single attack strike is made
        /// </summary>
        public override AttackData MakeAttack(WeaponAction action, GameObject target, DbInventoryItem weapon, Style style, double effectiveness, int interruptDuration, bool dualWield)
        {
            if (owner is GamePlayer playerOwner)
            {
                if (playerOwner.IsCrafting)
                {
                    playerOwner.Out.SendMessage(LanguageMgr.GetTranslation(playerOwner.Client.Account.Language, "GamePlayer.Attack.InterruptedCrafting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    playerOwner.craftComponent.StopCraft();
                    playerOwner.CraftTimer = null;
                    playerOwner.Out.SendCloseTimerWindow();
                }

                if (playerOwner.IsSalvagingOrRepairing)
                {
                    playerOwner.Out.SendMessage(LanguageMgr.GetTranslation(playerOwner.Client.Account.Language, "GamePlayer.Attack.InterruptedCrafting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    playerOwner.CraftTimer.Stop();
                    playerOwner.CraftTimer = null;
                    playerOwner.Out.SendCloseTimerWindow();
                }

                AttackData ad = LivingMakeAttack(action, target, weapon, style, effectiveness * playerOwner.Effectiveness, interruptDuration, dualWield);

                switch (ad.AttackResult)
                {
                    case eAttackResult.HitStyle:
                    case eAttackResult.HitUnstyled:
                    {
                        // Keep component.
                        if ((ad.Target is GameKeepComponent || ad.Target is GameKeepDoor || ad.Target is GameSiegeWeapon) &&
                           ad.Attacker is GamePlayer && ad.Attacker.GetModified(eProperty.KeepDamage) > 0)
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
                            weaponItem.OnStrikeTarget(playerOwner, target);

                        // Camouflage will be disabled only when attacking a GamePlayer or ControlledNPC of a GamePlayer.
                        if ((target is GamePlayer || target is MimicNPC && playerOwner.HasAbility(Abilities.Camouflage)) ||
                            (target is GameNPC targetNpc && targetNpc.Brain is IControlledBrain targetNpcBrain && targetNpcBrain.GetPlayerOwner() != null))
                        {
                            CamouflageECSGameEffect camouflage = (CamouflageECSGameEffect)EffectListService.GetAbilityEffectOnTarget(playerOwner, eEffect.Camouflage);

                            if (camouflage != null)
                                EffectService.RequestImmediateCancelEffect(camouflage, false);

                            playerOwner.DisableSkill(SkillBase.GetAbility(Abilities.Camouflage), CamouflageSpecHandler.DISABLE_DURATION);
                        }

                        // Multiple Hit check.
                        if (ad.AttackResult == eAttackResult.HitStyle)
                        {
                            List<GameObject> extraTargets = new();
                            List<GameObject> listAvailableTargets = new();
                            DbInventoryItem attackWeapon = owner.ActiveWeapon;
                            DbInventoryItem leftWeapon = playerOwner.Inventory?.GetItem(eInventorySlot.LeftHandWeapon);

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
                                    listAvailableTargets.RemoveAt(index);
                                    extraTargets.Add(availableTarget);
                                }
                            }

                            foreach (GameObject extraTarget in extraTargets)
                            {
                                if (extraTarget is GamePlayer player && player.IsSitting)
                                    effectiveness *= 2;

                                // TODO: Figure out why Shield Swipe is handled differently here.
                                if (IsNotShieldSwipe)
                                {
                                    weaponAction = new MimicWeaponAction(playerOwner, extraTarget, attackWeapon, leftWeapon, effectiveness, AttackSpeed(attackWeapon), null);
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
            else if (owner is MimicNPC mimicOwner)
            {
                AttackData ad = LivingMakeAttack(action, target, weapon, style, effectiveness * mimicOwner.Effectiveness, interruptDuration, dualWield);

                switch (ad.AttackResult)
                {
                    case eAttackResult.HitStyle:
                    case eAttackResult.HitUnstyled:
                    {
                        // Keep component.
                        if ((ad.Target is GameKeepComponent || ad.Target is GameKeepDoor || ad.Target is GameSiegeWeapon) &&
                            (ad.Attacker is MimicNPC) && ad.Attacker.GetModified(eProperty.KeepDamage) > 0)
                        {
                            int keepdamage = (int)Math.Floor(ad.Damage * ((double)ad.Attacker.GetModified(eProperty.KeepDamage) / 100));
                            int keepstyle = (int)Math.Floor(ad.StyleDamage * ((double)ad.Attacker.GetModified(eProperty.KeepDamage) / 100));
                            ad.Damage += keepdamage;
                            ad.StyleDamage += keepstyle;
                        }

                        // Vampiir.
                        if (mimicOwner.CharacterClass is PlayerClass.ClassVampiir &&
                            target is not GameKeepComponent and not GameKeepDoor and not GameSiegeWeapon)
                        {
                            int perc = Convert.ToInt32((double)(ad.Damage + ad.CriticalDamage) / 100 * (55 - mimicOwner.Level));
                            perc = (perc < 1) ? 1 : ((perc > 15) ? 15 : perc);
                            mimicOwner.Mana += Convert.ToInt32(Math.Ceiling((decimal)(perc * mimicOwner.MaxMana) / 100));
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
                            weaponItem.OnStrikeTarget(mimicOwner, target);

                        // Camouflage will be disabled only when attacking a GamePlayer or ControlledNPC of a GamePlayer.
                        if ((target is GamePlayer || target is MimicNPC && mimicOwner.HasAbility(Abilities.Camouflage)) ||
                            (target is GameNPC targetNpc && targetNpc.Brain is IControlledBrain targetNpcBrain && targetNpcBrain.GetPlayerOwner() != null))
                        {
                            CamouflageECSGameEffect camouflage = (CamouflageECSGameEffect)EffectListService.GetAbilityEffectOnTarget(mimicOwner, eEffect.Camouflage);

                            if (camouflage != null)
                                EffectService.RequestImmediateCancelEffect(camouflage, false);

                            mimicOwner.DisableSkill(SkillBase.GetAbility(Abilities.Camouflage), CamouflageSpecHandler.DISABLE_DURATION);
                        }

                        // Multiple Hit check.
                        if (ad.AttackResult == eAttackResult.HitStyle)
                        {
                            List<GameObject> extraTargets = new();
                            List<GameObject> listAvailableTargets = new();
                            DbInventoryItem attackWeapon = owner.ActiveWeapon;
                            DbInventoryItem leftWeapon = mimicOwner.Inventory?.GetItem(eInventorySlot.LeftHandWeapon);

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
                                    listAvailableTargets.RemoveAt(index);
                                    extraTargets.Add(availableTarget);
                                }
                            }

                            foreach (GameObject extraTarget in extraTargets)
                            {
                                if (extraTarget is GamePlayer player && player.IsSitting ||
                                    extraTarget is MimicNPC mimic && mimic.IsSitting)
                                    effectiveness *= 2;

                                // TODO: Figure out why Shield Swipe is handled differently here.
                                if (IsNotShieldSwipe)
                                {
                                    weaponAction = new MimicWeaponAction(mimicOwner, extraTarget, attackWeapon, leftWeapon, effectiveness, AttackSpeed(attackWeapon), null);
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

                return LivingMakeAttack(action, target, weapon, style, effectiveness, interruptDuration, dualWield);
            }
        }

        /// <summary>
        /// This method is called to make an attack, it is called from the
        /// attacktimer and should not be called manually
        /// </summary>
        /// <returns>the object where we collect and modifiy all parameters about the attack</returns>
        public override AttackData LivingMakeAttack(WeaponAction action, GameObject target, DbInventoryItem weapon, Style style, double effectiveness, int interruptDuration, bool dualWield, bool ignoreLOS = false)
        {
            AttackData ad = new()
            {
                Attacker = owner,
                Target = target as GameLiving,
                Damage = 0,
                CriticalDamage = 0,
                Style = style,
                WeaponSpeed = AttackSpeed(weapon) / 100,
                DamageType = AttackDamageType(weapon),
                ArmorHitLocation = eArmorSlot.NOTSET,
                Weapon = weapon,
                IsOffHand = weapon != null && weapon.SlotPosition == Slot.LEFTHAND
            };

            // Asp style range add.
            IEnumerable<(Spell, int, int)> rangeProc = style?.Procs.Where(x => x.Item1.SpellType == eSpellType.StyleRange);
            int addRange = rangeProc?.Any() == true ? (int)(rangeProc.First().Item1.Value - AttackRange) : 0;

            if (dualWield && (ad.Attacker is GamePlayer gPlayer) && gPlayer.CharacterClass.ID != (int)eCharacterClass.Savage ||
                             (ad.Attacker is MimicNPC mimic) && mimic.CharacterClass.ID != (int)eCharacterClass.Savage)
                ad.AttackType = AttackData.eAttackType.MeleeDualWield;
            else if (weapon == null)
                ad.AttackType = AttackData.eAttackType.MeleeOneHand;
            else
            {
                ad.AttackType = weapon.SlotPosition switch
                {
                    Slot.TWOHAND => AttackData.eAttackType.MeleeTwoHand,
                    Slot.RANGED => AttackData.eAttackType.Ranged,
                    _ => AttackData.eAttackType.MeleeOneHand,
                };
            }

            // No target.
            if (ad.Target == null)
            {
                ad.AttackResult = (target == null) ? eAttackResult.NoTarget : eAttackResult.NoValidTarget;
                SendAttackingCombatMessages(action, ad);
                return ad;
            }

            // Region / state check.
            if (ad.Target.CurrentRegionID != owner.CurrentRegionID || ad.Target.ObjectState != eObjectState.Active)
            {
                ad.AttackResult = eAttackResult.NoValidTarget;
                SendAttackingCombatMessages(action, ad);
                return ad;
            }

            // LoS / in front check.
            if (!ignoreLOS && ad.AttackType != AttackData.eAttackType.Ranged && owner is GamePlayer &&
                !(ad.Target is GameKeepComponent) &&
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
            if (ad.AttackType != AttackData.eAttackType.Ranged)
            {
                if (!owner.IsWithinRadius(ad.Target, AttackRange + addRange))
                {
                    ad.AttackResult = eAttackResult.OutOfRange;
                    SendAttackingCombatMessages(action, ad);
                    return ad;
                }
            }

            if (!GameServer.ServerRules.IsAllowedToAttack(ad.Attacker, ad.Target, attackAction != null && GameLoop.GameLoopTime - attackAction.RoundWithNoAttackTime <= 1500))
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
            ad.AttackResult = ad.Target.attackComponent.CalculateEnemyAttackResult(action, ad, weapon);

            GamePlayer playerOwner = owner as GamePlayer;
            MimicNPC mimicOwner = owner as MimicNPC;

            // Strafing miss.
            if (playerOwner != null && playerOwner.IsStrafing && ad.Target is GamePlayer && Util.Chance(30))
            {
                // Used to tell the difference between a normal miss and a strafing miss.
                // Ugly, but we shouldn't add a new field to 'AttackData' just for that purpose.
                ad.MissRate = 0;
                ad.AttackResult = eAttackResult.Missed;
            }

            switch (ad.AttackResult)
            {
                // Calculate damage only if we hit the target.
                case eAttackResult.HitUnstyled:
                case eAttackResult.HitStyle:
                {
                    double damage = AttackDamage(weapon, out double damageCap) * effectiveness;
                    DbInventoryItem armor = null;

                    if (ad.Target.Inventory != null)
                        armor = ad.Target.Inventory.GetItem((eInventorySlot)ad.ArmorHitLocation);

                    DbInventoryItem weaponForSpecModifier = null;

                    if (weapon != null)
                    {
                        weaponForSpecModifier = new DbInventoryItem();
                        weaponForSpecModifier.Object_Type = weapon.Object_Type;
                        weaponForSpecModifier.SlotPosition = weapon.SlotPosition;

                        if (owner is GamePlayer || owner is MimicNPC && owner.Realm == eRealm.Albion && Properties.ENABLE_ALBION_ADVANCED_WEAPON_SPEC &&
                            (GameServer.ServerRules.IsObjectTypesEqual((eObjectType)weapon.Object_Type, eObjectType.TwoHandedWeapon) ||
                            GameServer.ServerRules.IsObjectTypesEqual((eObjectType)weapon.Object_Type, eObjectType.PolearmWeapon)))
                        {
                            // Albion dual spec penalty, which sets minimum damage to the base damage spec.
                            if (weapon.Type_Damage == (int)eDamageType.Crush)
                                weaponForSpecModifier.Object_Type = (int)eObjectType.CrushingWeapon;
                            else if (weapon.Type_Damage == (int)eDamageType.Slash)
                                weaponForSpecModifier.Object_Type = (int)eObjectType.SlashingWeapon;
                            else
                                weaponForSpecModifier.Object_Type = (int)eObjectType.ThrustWeapon;
                        }
                    }

                    double specModifier = CalculateSpecModifier(ad.Target, weaponForSpecModifier);
                    double weaponSkill = CalculateWeaponSkill(ad.Target, weapon, specModifier, out double baseWeaponSkill);
                    double armorMod = CalculateTargetArmor(ad.Target, ad.ArmorHitLocation, out double bonusArmorFactor, out double armorFactor, out double absorb);
                    double damageMod = weaponSkill / armorMod;

                    if (action.RangedAttackType == eRangedAttackType.Critical)
                        damageCap *= 2; // This may be incorrect. Critical shot doesn't double damage on >yellow targets.

                    if (playerOwner != null)
                    {
                        if (playerOwner.UseDetailedCombatLog)
                            PrintDetailedCombatLog(playerOwner, armorFactor, absorb, armorMod, baseWeaponSkill, specModifier, weaponSkill, damageMod, damageCap);

                        // Badge Of Valor Calculation 1+ absorb or 1- absorb
                        // if (ad.Attacker.EffectList.GetOfType<BadgeOfValorEffect>() != null)
                        //     damage *= 1.0 + Math.Min(0.85, ad.Target.GetArmorAbsorb(ad.ArmorHitLocation));
                        // else
                        //     damage *= 1.0 - Math.Min(0.85, ad.Target.GetArmorAbsorb(ad.ArmorHitLocation));
                    }
                    else
                    {
                        if (owner is GameEpicBoss boss)
                            damageMod += boss.Strength / 200;

                        // Badge Of Valor Calculation 1+ absorb or 1- absorb
                        // if (ad.Attacker.EffectList.GetOfType<BadgeOfValorEffect>() != null)
                        //     damage *= 1.0 + Math.Min(0.85, ad.Target.GetArmorAbsorb(ad.ArmorHitLocation));
                        // else
                        //     damage *= 1.0 - Math.Min(0.85, ad.Target.GetArmorAbsorb(ad.ArmorHitLocation));
                    }

                    if (target is GamePlayer targetPlayer && targetPlayer.UseDetailedCombatLog)
                        PrintDetailedCombatLog(targetPlayer, armorFactor, absorb, armorMod, baseWeaponSkill, specModifier, weaponSkill, damageMod, damageCap);

                    if (ad.IsOffHand)
                        damage *= 1 + owner.GetModified(eProperty.OffhandDamage) * 0.01;

                    // If the target is another player's pet, shouldn't 'PVP_MELEE_DAMAGE' be used?
                    if ((owner is GamePlayer || owner is MimicNPC) || (owner is GameNPC npcOwner && npcOwner.Brain is IControlledBrain && owner.Realm != 0))
                    {
                        if (target is GamePlayer || target is MimicNPC)
                            damage = (int)(damage * Properties.PVP_MELEE_DAMAGE);
                        else if (target is GameNPC)
                            damage = (int)(damage * Properties.PVE_MELEE_DAMAGE);
                    }

                    damage *= damageMod;
                    double preResistDamage = damage;
                    double primarySecondaryResistMod = CalculateTargetResistance(ad.Target, ad.DamageType, armor);
                    double preConversionDamage = preResistDamage * primarySecondaryResistMod;
                    double conversionMod = CalculateTargetConversion(ad.Target, damage * primarySecondaryResistMod);

                    if (MimicStyleProcessor.ExecuteStyle(owner, ad.Target, ad.Style, weapon, preResistDamage, damageCap, ad.ArmorHitLocation, ad.StyleEffects, out int styleDamage, out int animationId))
                    {
                        double preResistStyleDamage = styleDamage;
                        double preConversionStyleDamage = preResistStyleDamage * primarySecondaryResistMod;
                        ad.StyleDamage = (int)(preConversionStyleDamage * conversionMod);

                        preResistDamage += preResistStyleDamage;
                        preConversionDamage += preConversionStyleDamage;
                        damage += ad.StyleDamage;

                        ad.AnimationId = animationId;
                        ad.AttackResult = eAttackResult.HitStyle;
                    }

                    damage = preConversionDamage * conversionMod;
                    ad.Modifier = (int)(damage - preResistDamage);
                    damage = (int)Math.Min(damage, damageCap);

                    if (conversionMod < 1)
                    {
                        double conversionAmount = conversionMod > 0 ? damage / conversionMod - damage : damage;
                        ApplyTargetConversionRegen(ad.Target, (int)conversionAmount);
                    }

                    ad.Damage = (int)Math.Min(damage, damageCap);
                    ad.CriticalDamage = CalculateMeleeCriticalDamage(ad, action, weapon);
                    break;
                }
                case eAttackResult.Blocked:
                case eAttackResult.Evaded:
                case eAttackResult.Parried:
                case eAttackResult.Missed:
                {
                    // Reduce endurance by half the style's cost if we missed.
                    if (ad.Style != null && playerOwner != null && weapon != null)
                        playerOwner.Endurance -= MimicStyleProcessor.CalculateEnduranceCost(playerOwner, ad.Style, weapon.SPD_ABS) / 2;
                    else if (ad.Style != null && mimicOwner != null && weapon != null)
                        mimicOwner.Endurance -= MimicStyleProcessor.CalculateEnduranceCost(mimicOwner, ad.Style, weapon.SPD_ABS) / 2;

                    break;
                }

                static void PrintDetailedCombatLog(GamePlayer player, double armorFactor, double absorb, double armorMod, double baseWeaponSkill, double specModifier, double weaponSkill, double damageMod, double damageCap)
                {
                    StringBuilder stringBuilder = new();
                    stringBuilder.Append($"BaseWS: {baseWeaponSkill:0.00} | SpecMod: {specModifier:0.00} | WS: {weaponSkill:0.00}\n");
                    stringBuilder.Append($"AF: {armorFactor:0.00} | ABS: {absorb * 100:0.00}% | AF/ABS: {armorMod:0.00}\n");
                    stringBuilder.Append($"DamageMod: {damageMod:0.00} | DamageCap: {damageCap:0.00}");
                    player.Out.SendMessage(stringBuilder.ToString(), eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);
                }
            }

            // Attacked living may modify the attack data. Primarily used for keep doors and components.
            ad.Target.ModifyAttack(ad);

            string message = "";
            bool broadcast = true;

            ArrayList excludes = new()
            {
                ad.Attacker,
                ad.Target
            };

            switch (ad.AttackResult)
            {
                case eAttackResult.Parried:
                message = string.Format("{0} attacks {1} and is parried!", ad.Attacker.GetName(0, true), ad.Target.GetName(0, false));
                break;

                case eAttackResult.Evaded:
                message = string.Format("{0} attacks {1} and is evaded!", ad.Attacker.GetName(0, true), ad.Target.GetName(0, false));
                break;

                case eAttackResult.Fumbled:
                message = string.Format("{0} fumbled!", ad.Attacker.GetName(0, true), ad.Target.GetName(0, false));
                break;

                case eAttackResult.Missed:
                message = string.Format("{0} attacks {1} and misses!", ad.Attacker.GetName(0, true), ad.Target.GetName(0, false));
                break;

                case eAttackResult.Blocked:
                {
                    message = string.Format("{0} attacks {1} and is blocked!", ad.Attacker.GetName(0, true),
                        ad.Target.GetName(0, false));
                    // guard messages
                    if (target != null && target != ad.Target)
                    {
                        excludes.Add(target);

                        // another player blocked for real target
                        if (target is GamePlayer)
                            ((GamePlayer)target).Out.SendMessage(
                                string.Format(
                                    LanguageMgr.GetTranslation(((GamePlayer)target).Client.Account.Language,
                                        "GameLiving.AttackData.BlocksYou"), ad.Target.GetName(0, true),
                                    ad.Attacker.GetName(0, false)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);

                        // blocked for another player
                        if (ad.Target is GamePlayer || ad.Target is MimicNPC)
                        {
                            if (ad.Target is GamePlayer)
                            {
                                ((GamePlayer)ad.Target).Out.SendMessage(
                                    string.Format(
                                        LanguageMgr.GetTranslation(((GamePlayer)ad.Target).Client.Account.Language,
                                            "GameLiving.AttackData.YouBlock") +
                                            $" ({ad.BlockChance:0.0}%)", ad.Attacker.GetName(0, false),
                                        target.GetName(0, false)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);

                                ((GamePlayer)ad.Target).Stealth(false);
                            }
                            else
                                ((MimicNPC)ad.Target).Stealth(false);
                        }
                    }
                    else if (ad.Target is GamePlayer)
                    {
                        ((GamePlayer)ad.Target).Out.SendMessage(
                            string.Format(
                                LanguageMgr.GetTranslation(((GamePlayer)ad.Target).Client.Account.Language,
                                    "GameLiving.AttackData.AttacksYou") +
                                    $" ({ad.BlockChance:0.0}%)", ad.Attacker.GetName(0, true)),
                            eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                    }

                    break;
                }
                case eAttackResult.HitUnstyled:
                case eAttackResult.HitStyle:
                {
                    if (ad.AttackResult == eAttackResult.HitStyle)
                    {
                        if (owner is GamePlayer)
                        {
                            GamePlayer player = owner as GamePlayer;

                            string damageAmount = (ad.StyleDamage > 0)
                                ? " (+" + ad.StyleDamage + ", GR: " + ad.Style.GrowthRate + ")"
                                : "";
                            player.Out.SendMessage(
                                LanguageMgr.GetTranslation(player.Client.Account.Language,
                                    "StyleProcessor.ExecuteStyle.PerformPerfectly", ad.Style.Name, damageAmount),
                                eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        }
                        else if (owner is GameNPC)
                        {
                            ControlledNpcBrain brain = ((GameNPC)owner).Brain as ControlledNpcBrain;

                            if (brain != null)
                            {
                                GamePlayer player = brain.GetPlayerOwner();
                                if (player != null)
                                {
                                    string damageAmount = (ad.StyleDamage > 0)
                                        ? " (+" + ad.StyleDamage + ", GR: " + ad.Style.GrowthRate + ")"
                                        : "";
                                    player.Out.SendMessage(
                                        LanguageMgr.GetTranslation(player.Client.Account.Language,
                                            "StyleProcessor.ExecuteStyle.PerformsPerfectly", owner.Name, ad.Style.Name,
                                            damageAmount), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                                }
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
                        if (ad.Attacker is GamePlayer)
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

            if (ad.Attacker is GamePlayer)
            {
                //GamePlayer attacker = ad.Attacker as GamePlayer;
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

            if (ad.Attacker is GameNPC)
            {
                IControlledBrain brain = ((GameNPC)ad.Attacker).Brain as IControlledBrain;

                if (brain != null)
                {
                    GamePlayer owner = brain.GetPlayerOwner();

                    if (owner != null)
                    {
                        excludes.Add(owner);

                        switch (ad.AttackResult)
                        {
                            case eAttackResult.HitStyle:
                            case eAttackResult.HitUnstyled:
                            {
                                string modmessage = "";

                                if (ad.Modifier > 0)
                                    modmessage = $" (+{ad.Modifier})";
                                else if (ad.Modifier < 0)
                                    modmessage = $" ({ad.Modifier})";

                                string attackTypeMsg;

                                if (action.ActiveWeaponSlot == eActiveWeaponSlot.Distance)
                                    attackTypeMsg = "shoots";
                                else
                                    attackTypeMsg = "attacks";

                                owner.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(owner.Client.Account.Language, "GameLiving.AttackData.YourHits"),
                                    ad.Attacker.Name, attackTypeMsg, ad.Target.GetName(0, false), ad.Damage, modmessage),
                                    eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

                                if (ad.CriticalDamage > 0)
                                {
                                    owner.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(owner.Client.Account.Language, "GameLiving.AttackData.YourCriticallyHits"),
                                        ad.Attacker.Name, ad.Target.GetName(0, false), ad.CriticalDamage) + $" ({AttackCriticalChance(action, ad.Weapon)}%)",
                                        eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                                }

                                break;
                            }
                            case eAttackResult.Missed:
                            {
                                owner.Out.SendMessage(message + $" ({ad.MissRate}%)", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                                break;
                            }
                            default:
                            owner.Out.SendMessage(message, eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    }
                }
            }

            if (ad.Target is GameNPC)
            {
                IControlledBrain brain = ((GameNPC)ad.Target).Brain as IControlledBrain;
                if (brain != null)
                {
                    GameLiving owner_living = brain.GetLivingOwner();
                    excludes.Add(owner_living);
                    if (owner_living != null && owner_living is GamePlayer && owner_living.ControlledBrain != null &&
                        ad.Target == owner_living.ControlledBrain.Body)
                    {
                        GamePlayer owner = owner_living as GamePlayer;
                        switch (ad.AttackResult)
                        {
                            case eAttackResult.Blocked:
                            owner.Out.SendMessage(
                                string.Format(
                                    LanguageMgr.GetTranslation(owner.Client.Account.Language,
                                        "GameLiving.AttackData.Blocked"), ad.Attacker.GetName(0, true),
                                    ad.Target.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                            break;

                            case eAttackResult.Parried:
                            owner.Out.SendMessage(
                                string.Format(
                                    LanguageMgr.GetTranslation(owner.Client.Account.Language,
                                        "GameLiving.AttackData.Parried"), ad.Attacker.GetName(0, true),
                                    ad.Target.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                            break;

                            case eAttackResult.Evaded:
                            owner.Out.SendMessage(
                                string.Format(
                                    LanguageMgr.GetTranslation(owner.Client.Account.Language,
                                        "GameLiving.AttackData.Evaded"), ad.Attacker.GetName(0, true),
                                    ad.Target.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                            break;

                            case eAttackResult.Fumbled:
                            owner.Out.SendMessage(
                                string.Format(
                                    LanguageMgr.GetTranslation(owner.Client.Account.Language,
                                        "GameLiving.AttackData.Fumbled"), ad.Attacker.GetName(0, true)),
                                eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                            break;

                            case eAttackResult.Missed:
                            if (ad.AttackType != AttackData.eAttackType.Spell)
                                owner.Out.SendMessage(
                                    string.Format(
                                        LanguageMgr.GetTranslation(owner.Client.Account.Language,
                                            "GameLiving.AttackData.Misses"), ad.Attacker.GetName(0, true),
                                        ad.Target.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                            break;

                            case eAttackResult.HitStyle:
                            case eAttackResult.HitUnstyled:
                            {
                                string modmessage = "";
                                if (ad.Modifier > 0) modmessage = " (+" + ad.Modifier + ")";
                                if (ad.Modifier < 0) modmessage = " (" + ad.Modifier + ")";
                                owner.Out.SendMessage(
                                    string.Format(
                                        LanguageMgr.GetTranslation(owner.Client.Account.Language,
                                            "GameLiving.AttackData.HitsForDamage"), ad.Attacker.GetName(0, true),
                                        ad.Target.Name, ad.Damage, modmessage), eChatType.CT_Damaged,
                                    eChatLoc.CL_SystemWindow);
                                if (ad.CriticalDamage > 0)
                                {
                                    owner.Out.SendMessage(
                                        string.Format(
                                            LanguageMgr.GetTranslation(owner.Client.Account.Language,
                                                "GameLiving.AttackData.CriticallyHitsForDamage"),
                                            ad.Attacker.GetName(0, true), ad.Target.Name, ad.CriticalDamage),
                                        eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
                                }

                                break;
                            }
                            default: break;
                        }
                    }
                }
            }

            #endregion controlled messages

            // broadcast messages
            if (broadcast)
            {
                Message.SystemToArea(ad.Attacker, message, eChatType.CT_OthersCombat,
                    (GameObject[])excludes.ToArray(typeof(GameObject)));
            }

            // Interrupt the target of the attack
            ad.Target.StartInterruptTimer(interruptDuration, ad.AttackType, ad.Attacker);

            // If we're attacking via melee, start an interrupt timer on ourselves so we cannot swing + immediately cast.
            if (ad.AttackType != AttackData.eAttackType.Spell && ad.AttackType != AttackData.eAttackType.Ranged && owner.StartInterruptTimerOnItselfOnMeleeAttack())
                owner.StartInterruptTimer(owner.SpellInterruptDuration, ad.AttackType, ad.Attacker);

            owner.OnAttackEnemy(ad);

            //Return the result
            return ad;
        }

        public override double CalculateWeaponSkill(GameLiving target, double baseWeaponSkill, double relicBonus, double specModifier)
        {
            if (owner is GamePlayer || owner is MimicNPC)
                return baseWeaponSkill * relicBonus * specModifier;

            baseWeaponSkill += target.Level * 65 / 50.0;

            if (owner.Level < 10)
                baseWeaponSkill *= 1 - 0.05 * (10 - owner.Level);

            return baseWeaponSkill;
        }

        public override double CalculateSpecModifier(GameLiving target, DbInventoryItem weapon)
        {
            double specModifier = 0;

            if (owner is GamePlayer || owner is MimicNPC)
            {
                int spec = owner.WeaponSpecLevel(weapon);

                if (owner.Level < 5 && spec < 2)
                    spec = 2;

                double lowerLimit = Math.Min(0.75 * (spec - 1) / (target.EffectiveLevel + 1) + 0.25, 1.0);

                if (lowerLimit < 0.01)
                    lowerLimit = 0.01;

                double upperLimit = Math.Min(Math.Max(1.25 + (3.0 * (spec - 1) / (target.EffectiveLevel + 1) - 2) * 0.25, 1.25), 1.50);
                int varianceRange = (int)(upperLimit * 100 - lowerLimit * 100);

                if (owner is GamePlayer playerOwner)
                    specModifier = playerOwner.SpecLock > 0 ? playerOwner.SpecLock : lowerLimit + Util.Random(varianceRange) * 0.01;
                else if (owner is MimicNPC mimicOwner)
                    specModifier = mimicOwner.SpecLock > 0 ? mimicOwner.SpecLock : lowerLimit + Util.Random(varianceRange) * 0.01;
            }
            else
            {
                int minimum;
                int maximum;

                if (owner is GameEpicBoss)
                {
                    minimum = 95;
                    maximum = 105;
                }
                else
                {
                    minimum = 75;
                    maximum = 125;
                }

                specModifier = (Util.Random(maximum - minimum) + minimum) * 0.01;
            }

            return specModifier;
        }

        private const int ARMOR_FACTOR_LEVEL_SCALAR = 25;

        public override double CalculateTargetArmor(GameLiving target, eArmorSlot armorSlot, out double bonusArmorFactor, out double armorFactor, out double absorb)
        {
            if ((owner is GamePlayer || owner is MimicNPC) && (target is not GamePlayer && target is not MimicNPC))
                bonusArmorFactor = 2;
            else
                bonusArmorFactor = target.Level * ARMOR_FACTOR_LEVEL_SCALAR / 50.0;

            armorFactor = bonusArmorFactor + target.GetArmorAF(armorSlot);
            absorb = target.GetArmorAbsorb(armorSlot);
            return absorb >= 1 ? double.MaxValue : armorFactor / (1 - absorb);
        }

        public static new double CalculateTargetResistance(GameLiving target, eDamageType damageType, DbInventoryItem armor)
        {
            double damageModifier = 1.0;

            damageModifier *= 1.0 - (target.GetResist(damageType) + SkillBase.GetArmorResist(armor, damageType)) * 0.01;

            return damageModifier;
        }

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static new double CalculateTargetConversion(GameLiving target, double damage)
        {
            if (target is not GamePlayer && target is not MimicNPC)
                return 1.0;

            double conversionMod = 1 - target.GetModified(eProperty.Conversion) / 100.0;

            if (conversionMod > 1.0)
                return 1.0;

            return conversionMod;
        }

        public static new void ApplyTargetConversionRegen(GameLiving target, int conversionAmount)
        {
            if (target is not GamePlayer && target is not MimicNPC)
                return;

            GamePlayer playerTarget = target as GamePlayer;

            int powerConversion = conversionAmount;
            int enduranceConversion = conversionAmount;

            if (target.Mana + conversionAmount > target.MaxMana)
                powerConversion = target.MaxMana - target.Mana;

            if (target.Endurance + conversionAmount > target.MaxEndurance)
                enduranceConversion = target.MaxEndurance - target.Endurance;

            if (powerConversion > 0 && playerTarget != null)
                playerTarget.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(playerTarget.Client.Account.Language, "GameLiving.AttackData.GainPowerPoints"), powerConversion), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);

            if (enduranceConversion > 0 && playerTarget != null)
                playerTarget.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(playerTarget.Client.Account.Language, "GameLiving.AttackData.GainEndurancePoints"), enduranceConversion), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);

            target.Mana = Math.Min(target.MaxMana, target.Mana + powerConversion);
            target.Endurance = Math.Min(target.MaxEndurance, target.Endurance + enduranceConversion);
        }

        public override bool CheckBlock(AttackData ad, double attackerConLevel)
        {
            double blockChance = owner.TryBlock(ad, attackerConLevel, Attackers.Count);
            ad.BlockChance = blockChance;
            double ranBlockNum = Util.CryptoNextDouble() * 10000;
            ranBlockNum = Math.Floor(ranBlockNum);
            ranBlockNum /= 100;
            blockChance *= 100;

            if (blockChance > 0)
            {
                double? blockDouble = null;

                if (owner is GamePlayer)
                    blockDouble = (owner as GamePlayer)?.RandomNumberDeck.GetPseudoDouble();
                else if (owner is MimicNPC)
                    blockDouble = (owner as MimicNPC)?.RandomNumberDeck.GetPseudoDouble();

                double? blockOutput = (blockDouble != null) ? Math.Round((double)(blockDouble * 100), 2) : ranBlockNum;

                if (ad.Attacker is GamePlayer blockAttk && blockAttk.UseDetailedCombatLog)
                    blockAttk.Out.SendMessage($"target block%: {Math.Round(blockChance, 2)} rand: {blockOutput}", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);

                if (ad.Target is GamePlayer blockTarg && blockTarg.UseDetailedCombatLog)
                    blockTarg.Out.SendMessage($"your block%: {Math.Round(blockChance, 2)} rand: {blockOutput}", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);

                if (blockDouble == null || Properties.OVERRIDE_DECK_RNG)
                {
                    if (blockChance > ranBlockNum)
                        return true;
                }
                else
                {
                    blockDouble *= 100;

                    if (blockChance > blockDouble)
                        return true;
                }
            }

            if (ad.AttackType is AttackData.eAttackType.Ranged or AttackData.eAttackType.Spell)
            {
                // Nature's shield, 100% block chance, 120° frontal angle.
                if (owner.IsObjectInFront(ad.Attacker, 120) && (owner.styleComponent.NextCombatStyle?.ID == 394 || owner.styleComponent.NextCombatBackupStyle?.ID == 394))
                {
                    ad.BlockChance = 1;
                    return true;
                }
            }

            return false;
        }

        public override bool CheckGuard(AttackData ad, bool stealthStyle, double attackerConLevel)
        {
            GuardECSGameEffect guard = EffectListService.GetAbilityEffectOnTarget(owner, eEffect.Guard) as GuardECSGameEffect;

            if (guard?.GuardTarget != owner)
                return false;

            GameLiving guardSource = guard.GuardSource;

            if (guardSource == null ||
                guardSource.ObjectState != eObjectState.Active ||
                guardSource.IsStunned != false ||
                guardSource.IsMezzed != false ||
                guardSource.ActiveWeaponSlot == eActiveWeaponSlot.Distance ||
                !guardSource.IsAlive ||
                guardSource.IsSitting ||
                stealthStyle ||
                !guard.GuardSource.IsWithinRadius(guard.GuardTarget, GuardAbilityHandler.GUARD_DISTANCE))
                return false;

            DbInventoryItem leftHand = guard.GuardSource.Inventory.GetItem(eInventorySlot.LeftHandWeapon);
            DbInventoryItem rightHand = guard.GuardSource.ActiveWeapon;

            if (((rightHand != null && rightHand.Hand == 1) || leftHand == null || leftHand.Object_Type != (int)eObjectType.Shield) && guard.GuardSource is not GameNPC)
                return false;

            // TODO: Insert actual formula for guarding here, this is just a guessed one based on block.
            int guardLevel = guard.GuardSource.GetAbilityLevel(Abilities.Guard);

            double guardChance;

            if (guard.GuardSource is GameNPC && guard.GuardSource is not MimicNPC)
                guardChance = guard.GuardSource.GetModified(eProperty.BlockChance);
            else
                guardChance = guard.GuardSource.GetModified(eProperty.BlockChance) * (leftHand.Quality * 0.01) * (leftHand.Condition / (double)leftHand.MaxCondition);

            guardChance *= 0.001;
            guardChance += guardLevel * 5 * 0.01; // 5% additional chance to guard with each Guard level.
            guardChance += attackerConLevel * 0.05;
            int shieldSize = 1;

            if (leftHand != null)
            {
                shieldSize = Math.Max(leftHand.Type_Damage, 1);

                if (guardSource is GamePlayer || guardSource is MimicNPC)
                    guardChance += (double)(leftHand.Level - 1) / 50 * 0.15; // Up to 15% extra block chance based on shield level.
            }

            if (Attackers.Count > shieldSize)
                guardChance *= shieldSize / (double)Attackers.Count;

            // Reduce chance by attacker's defense penetration.
            guardChance *= 1 - ad.Attacker.GetAttackerDefensePenetration(ad.Attacker, ad.Weapon) / 100;

            if (guardChance < 0.01)
                guardChance = 0.01;
            else if (guardChance > Properties.BLOCK_CAP && ad.Attacker is GamePlayer && ad.Target is GamePlayer)
                guardChance = Properties.BLOCK_CAP;

            /// Possibly intended to be applied in RvR only.
            if (shieldSize == 1 && guardChance > 0.8)
                guardChance = 0.8;
            else if (shieldSize == 2 && guardChance > 0.9)
                guardChance = 0.9;
            else if (shieldSize == 3 && guardChance > 0.99)
                guardChance = 0.99;

            if (ad.AttackType == AttackData.eAttackType.MeleeDualWield)
                guardChance *= 0.5;

            double ranBlockNum = Util.CryptoNextDouble() * 10000;
            ranBlockNum = Math.Floor(ranBlockNum);
            ranBlockNum /= 100;
            guardChance *= 100;

            double? blockDouble = null;

            if (owner is GamePlayer)
                blockDouble = (owner as GamePlayer)?.RandomNumberDeck.GetPseudoDouble();
            else if (owner is MimicNPC)
                blockDouble = (owner as MimicNPC)?.RandomNumberDeck.GetPseudoDouble();

            double? blockOutput = (blockDouble != null) ? blockDouble * 100 : ranBlockNum;

            if (guard.GuardSource is GamePlayer blockAttk && blockAttk.UseDetailedCombatLog)
                blockAttk.Out.SendMessage($"Chance to guard: {guardChance} rand: {blockOutput} GuardSuccess? {guardChance > blockOutput}", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);

            if (guard.GuardTarget is GamePlayer blockTarg && blockTarg.UseDetailedCombatLog)
                blockTarg.Out.SendMessage($"Chance to be guarded: {guardChance} rand: {blockOutput} GuardSuccess? {guardChance > blockOutput}", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);

            if (blockDouble == null || Properties.OVERRIDE_DECK_RNG)
            {
                if (guardChance > ranBlockNum)
                {
                    ad.Target = guard.GuardSource;
                    return true;
                }
            }
            else
            {
                if (guardChance > blockOutput)
                {
                    ad.Target = guard.GuardSource;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the result of an enemy attack
        /// </summary>
        public override eAttackResult CalculateEnemyAttackResult(WeaponAction action, AttackData ad, DbInventoryItem attackerWeapon)
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
            ECSGameSpellEffect bladeturn = null;
            // ML effects
            GameSpellEffect phaseshift = null;
            GameSpellEffect grapple = null;
            GameSpellEffect brittleguard = null;

            AttackData lastAttackData = owner.TempProperties.GetProperty<AttackData>(LAST_ATTACK_DATA, null);
            bool defenseDisabled = ad.Target.IsMezzed | ad.Target.IsStunned | ad.Target.IsSitting;

            GamePlayer playerOwner = owner as GamePlayer;
            GamePlayer playerAttacker = ad.Attacker as GamePlayer;

            // If berserk is on, no defensive skills may be used: evade, parry, ...
            // unfortunately this as to be check for every action itself to kepp oder of actions the same.
            // Intercept and guard can still be used on berserked
            // BerserkEffect berserk = null;

            if (EffectListService.GetAbilityEffectOnTarget(owner, eEffect.Berserk) != null)
                defenseDisabled = true;

            if (EffectListService.GetSpellEffectOnTarget(owner, eEffect.Bladeturn) is ECSGameSpellEffect bladeturnEffect)
            {
                if (bladeturn == null)
                    bladeturn = bladeturnEffect;
            }

            // We check if interceptor can intercept.
            if (EffectListService.GetAbilityEffectOnTarget(owner, eEffect.Intercept) is InterceptECSGameEffect inter)
            {
                if (intercept == null && inter != null && inter.InterceptTarget == owner && !inter.InterceptSource.IsStunned && !inter.InterceptSource.IsMezzed
                    && !inter.InterceptSource.IsSitting && inter.InterceptSource.ObjectState == eObjectState.Active && inter.InterceptSource.IsAlive
                    && owner.IsWithinRadius(inter.InterceptSource, InterceptAbilityHandler.INTERCEPT_DISTANCE)) // && Util.Chance(inter.InterceptChance))
                {
                    int interceptRoll;

                    if (!Properties.OVERRIDE_DECK_RNG && playerOwner != null)
                        interceptRoll = playerOwner.RandomNumberDeck.GetInt();
                    else if (!Properties.OVERRIDE_DECK_RNG && owner as MimicNPC != null)
                        interceptRoll = ((MimicNPC)owner).RandomNumberDeck.GetInt();
                    else
                        interceptRoll = Util.Random(100);

                    if (inter.InterceptChance > interceptRoll)
                        intercept = inter;
                }
            }

            bool stealthStyle = false;

            if (ad.Style != null && ad.Style.StealthRequirement && (ad.Attacker is GamePlayer || ad.Attacker is MimicNPC) && StyleProcessor.CanUseStyle(lastAttackData, ad.Attacker, ad.Style, attackerWeapon))
            {
                stealthStyle = true;
                defenseDisabled = true;
                intercept = null;
                brittleguard = null;
            }

            if (playerOwner != null)
            {
                GameLiving attacker = ad.Attacker;
                GamePlayer tempPlayerAttacker = playerAttacker ?? ((attacker as GameNPC)?.Brain as IControlledBrain)?.GetPlayerOwner();

                if (tempPlayerAttacker != null && action.ActiveWeaponSlot != eActiveWeaponSlot.Distance)
                {
                    GamePlayer bodyguard = playerOwner.Bodyguard;

                    if (bodyguard != null)
                    {
                        playerOwner.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(playerOwner.Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.YouWereProtected"), bodyguard.Name, attacker.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                        bodyguard.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(bodyguard.Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.YouHaveProtected"), playerOwner.Name, attacker.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);

                        if (attacker == tempPlayerAttacker)
                            tempPlayerAttacker.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(tempPlayerAttacker.Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.YouAttempt"), playerOwner.Name, playerOwner.Name, bodyguard.Name), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        else
                            tempPlayerAttacker.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(tempPlayerAttacker.Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.YourPetAttempts"), playerOwner.Name, playerOwner.Name, bodyguard.Name), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

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
                ad.Target = intercept.InterceptSource;

                if (intercept.InterceptSource is GamePlayer || intercept.InterceptSource is MimicNPC)
                    intercept.Cancel(false);

                return eAttackResult.HitUnstyled;
            }

            double attackerConLevel = -owner.GetConLevel(ad.Attacker);

            if (!defenseDisabled)
            {
                if (lastAttackData != null && lastAttackData.AttackResult != eAttackResult.HitStyle)
                    lastAttackData = null;

                double evadeChance = owner.TryEvade(ad, lastAttackData, attackerConLevel, Attackers.Count);
                ad.EvadeChance = evadeChance;
                double randomEvadeNum = Util.CryptoNextDouble() * 10000;
                randomEvadeNum = Math.Floor(randomEvadeNum);
                randomEvadeNum /= 100;
                evadeChance *= 100;

                if (evadeChance > 0)
                {
                    double? evadeDouble = null;

                    if (owner is GamePlayer)
                        evadeDouble = (owner as GamePlayer)?.RandomNumberDeck.GetPseudoDouble();
                    else if (owner is MimicNPC)
                        evadeDouble = (owner as MimicNPC)?.RandomNumberDeck.GetPseudoDouble();

                    double? evadeOutput = (evadeDouble != null) ? Math.Round((double)(evadeDouble * 100), 2) : randomEvadeNum;

                    if (ad.Attacker is GamePlayer evadeAtk && evadeAtk.UseDetailedCombatLog)
                        evadeAtk.Out.SendMessage($"target evade%: {Math.Round(evadeChance, 2)} rand: {evadeOutput}", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);

                    if (ad.Target is GamePlayer evadeTarg && evadeTarg.UseDetailedCombatLog)
                        evadeTarg.Out.SendMessage($"your evade%: {Math.Round(evadeChance, 2)} rand: {evadeOutput}", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);

                    if (evadeDouble == null || Properties.OVERRIDE_DECK_RNG)
                    {
                        if (evadeChance > randomEvadeNum)
                            return eAttackResult.Evaded;
                    }
                    else
                    {
                        evadeDouble *= 100;

                        if (evadeChance > evadeDouble)
                            return eAttackResult.Evaded;
                    }
                }

                if (ad.IsMeleeAttack)
                {
                    double parryChance = owner.TryParry(ad, lastAttackData, attackerConLevel, Attackers.Count);
                    ad.ParryChance = parryChance;
                    double ranParryNum = Util.CryptoNextDouble() * 10000;
                    ranParryNum = Math.Floor(ranParryNum);
                    ranParryNum /= 100;
                    parryChance *= 100;

                    if (parryChance > 0)
                    {
                        double? parryDouble = null;

                        if (owner is GamePlayer)
                            parryDouble = (owner as GamePlayer)?.RandomNumberDeck.GetPseudoDouble();
                        else if (owner is MimicNPC)
                            parryDouble = (owner as MimicNPC)?.RandomNumberDeck.GetPseudoDouble();

                        double? parryOutput = (parryDouble != null) ? Math.Round((double)(parryDouble * 100.0), 2) : ranParryNum;

                        if (ad.Attacker is GamePlayer parryAtk && parryAtk.UseDetailedCombatLog)
                            parryAtk.Out.SendMessage($"target parry%: {Math.Round(parryChance, 2)} rand: {parryOutput}", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);

                        if (ad.Target is GamePlayer parryTarg && parryTarg.UseDetailedCombatLog)
                            parryTarg.Out.SendMessage($"your parry%: {Math.Round(parryChance, 2)} rand: {parryOutput}", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);

                        if (parryDouble == null || Properties.OVERRIDE_DECK_RNG)
                        {
                            if (parryChance > ranParryNum)
                                return eAttackResult.Parried;
                        }
                        else
                        {
                            parryDouble *= 100;

                            if (parryChance > parryDouble)
                                return eAttackResult.Parried;
                        }
                    }
                }

                if (CheckBlock(ad, attackerConLevel))
                    return eAttackResult.Blocked;
            }

            if (CheckGuard(ad, stealthStyle, attackerConLevel))
                return eAttackResult.Blocked;

            // Not implemented.
            // if (CheckDashingDefense(ad, stealthStyle, attackerConLevel, out eAttackResult result)
            //     return result;

            // Miss chance.
            int missChance = GetMissChance(action, ad, lastAttackData, attackerWeapon);

            // Check for dirty trick fumbles before misses.
            DirtyTricksDetrimentalECSGameEffect dt = (DirtyTricksDetrimentalECSGameEffect)EffectListService.GetAbilityEffectOnTarget(ad.Attacker, eEffect.DirtyTricksDetrimental);

            if (dt != null && ad.IsRandomFumble)
                return eAttackResult.Fumbled;

            ad.MissRate = missChance;

            if (missChance > 0)
            {
                double? rand = null;

                if (owner is GamePlayer)
                    rand = (owner as GamePlayer)?.RandomNumberDeck.GetPseudoDouble();
                else if (owner is MimicNPC)
                    rand = (owner as MimicNPC)?.RandomNumberDeck.GetPseudoDouble();

                if (rand == null)
                    rand = Util.CryptoNextDouble();

                if (ad.Attacker is GamePlayer misser && misser.UseDetailedCombatLog)
                {
                    misser.Out.SendMessage($"miss rate on target: {missChance}% rand: {rand * 100:0.##}", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);
                    misser.Out.SendMessage($"Your chance to fumble: {100 * ad.Attacker.ChanceToFumble:0.##}% rand: {100 * rand:0.##}", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);
                }

                if (ad.Target is GamePlayer missee && missee.UseDetailedCombatLog)
                    missee.Out.SendMessage($"chance to be missed: {missChance}% rand: {rand * 100:0.##}", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);

                // Check for normal fumbles.
                // NOTE: fumbles are a subset of misses, and a player can only fumble if the attack would have been a miss anyways.
                if (missChance > rand * 100)
                {
                    if (ad.Attacker.ChanceToFumble > rand)
                        return eAttackResult.Fumbled;

                    return eAttackResult.Missed;
                }
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
            if (bladeturn != null)
            {
                bool penetrate = false;

                if (stealthStyle)
                    return eAttackResult.HitUnstyled; // Exit early for stealth to prevent breaking bubble but still register a hit.

                if (action.RangedAttackType == eRangedAttackType.Long ||
                    (ad.AttackType == AttackData.eAttackType.Ranged && ad.Target != bladeturn.SpellHandler.Caster && playerAttacker?.HasAbility(Abilities.PenetratingArrow) == true))
                    penetrate = true;

                if (ad.IsMeleeAttack && !Util.ChanceDouble(bladeturn.SpellHandler.Caster.Level / ad.Attacker.Level))
                    penetrate = true;

                if (penetrate)
                {
                    if (playerOwner != null || owner is MimicNPC)
                    {
                        if (playerOwner != null)
                            playerOwner.Out.SendMessage(LanguageMgr.GetTranslation(playerOwner.Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.BlowPenetrated"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        EffectService.RequestImmediateCancelEffect(bladeturn);
                    }
                }
                else
                {
                    if (playerOwner != null)
                    {
                        playerOwner.Out.SendMessage(LanguageMgr.GetTranslation(playerOwner.Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.BlowAbsorbed"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        playerOwner.Stealth(false);
                    }
                    else if (owner is MimicNPC)
                    {
                        ((MimicNPC)owner).Stealth(false);
                    }

                    playerAttacker?.Out.SendMessage(LanguageMgr.GetTranslation(playerAttacker.Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.StrikeAbsorbed"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    EffectService.RequestImmediateCancelEffect(bladeturn);
                    return eAttackResult.Missed;
                }
            }

            if (playerOwner?.IsOnHorse == true)
                playerOwner.IsOnHorse = false;

            return eAttackResult.HitUnstyled;
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

        public override int CalculateMeleeCriticalDamage(AttackData ad, WeaponAction action, DbInventoryItem weapon)
        {
            if (!Util.Chance(AttackCriticalChance(action, weapon)))
                return 0;

            if (owner is GamePlayer || owner is MimicNPC)
            {
                // triple wield prevents critical hits
                if (EffectListService.GetAbilityEffectOnTarget(ad.Target, eEffect.TripleWield) != null)
                    return 0;

                int critMin;
                int critMax;
                ECSGameEffect berserk = EffectListService.GetEffectOnTarget(owner, eEffect.Berserk);

                if (berserk != null)
                {
                    int level = owner.GetAbilityLevel(Abilities.Berserk);
                    // According to : http://daoc.catacombs.com/forum.cfm?ThreadKey=10833&DefMessage=922046&forum=37
                    // Zerk 1 = 1-25%
                    // Zerk 2 = 1-50%
                    // Zerk 3 = 1-75%
                    // Zerk 4 = 1-99%
                    critMin = (int)(0.01 * ad.Damage);
                    critMax = (int)(Math.Min(0.99, level * 0.25) * ad.Damage);
                }
                else
                {
                    // Min crit damage is 10%.
                    critMin = ad.Damage / 10;
                    // Max crit damage to players is 50%. Berzerkers go up to 99% in Berserk mode.

                    if (ad.Target is GamePlayer || ad.Target is MimicNPC)
                        critMax = ad.Damage / 2;
                    else
                        critMax = ad.Damage;
                }

                critMin = Math.Max(critMin, 0);
                critMax = Math.Max(critMin, critMax);
                return ad.CriticalDamage = Util.Random(critMin, critMax);
            }
            else
            {
                int maxCriticalDamage = 0;

                if (ad.Target is GamePlayer || ad.Target is MimicNPC)
                    maxCriticalDamage = ad.Damage / 2;
                else
                    maxCriticalDamage = ad.Damage;

                int minCriticalDamage = (int)(ad.Damage * MinMeleeCriticalDamage);

                if (minCriticalDamage > maxCriticalDamage)
                    minCriticalDamage = maxCriticalDamage;

                return Util.Random(minCriticalDamage, maxCriticalDamage);
            }
        }

        public override int GetMissChance(WeaponAction action, AttackData ad, AttackData lastAD, DbInventoryItem weapon)
        {
            // No miss if the target is sitting or for Volley attacks.
            if (owner is GamePlayer player && player.IsSitting || (owner is MimicNPC mimic && mimic.IsSitting) || action.RangedAttackType == eRangedAttackType.Volley)
                return 0;

            int missChance = ad.Attacker is GamePlayer or GameSummonedPet or MimicNPC ? 18 : 25;
            missChance -= ad.Attacker.GetModified(eProperty.ToHitBonus);

            // PVE group miss rate.
            //if (owner is GameNPC && owner is not MimicNPC && ad.Attacker is GamePlayer or MimicNPC && ad.Attacker.Group != null && (int)(0.90 * ad.Attacker.Group.Leader.Level) >= ad.Attacker.Level && ad.Attacker.IsWithinRadius(ad.Attacker.Group.Leader, 3000))
            //    missChance -= (int)(5 * ad.Attacker.Group.Leader.GetConLevel(owner));
            //else if (owner is GameNPC || ad.Attacker is GameNPC && ad.Attacker is not MimicNPC && owner is not MimicNPC)
            //{
            //    GameLiving misscheck = ad.Attacker;

            //    if (ad.Attacker is GameSummonedPet petAttacker && petAttacker.Level < petAttacker.Owner.Level)
            //        misscheck = petAttacker.Owner;

            //    missChance += (int)(5 * misscheck.GetConLevel(owner));
            //}

            // Experimental miss rate adjustment for number of attackers.
            if ((owner is GamePlayer && ad.Attacker is GamePlayer && ad.Attacker is MimicNPC && owner is MimicNPC) == false)
                missChance -= Math.Max(0, Attackers.Count - 1) * Properties.MISSRATE_REDUCTION_PER_ATTACKERS;

            // Weapon and armor bonuses.
            int armorBonus = 0;

            if (ad.Target is GamePlayer p)
            {
                ad.ArmorHitLocation = ((GamePlayer)ad.Target).CalculateArmorHitLocation(ad);

                if (ad.Target.Inventory != null)
                {
                    DbInventoryItem armor = ad.Target.Inventory.GetItem((eInventorySlot)ad.ArmorHitLocation);

                    if (armor != null)
                        armorBonus = armor.Bonus;
                }

                int bonusCap = GetBonusCapForLevel(p.Level);

                if (armorBonus > bonusCap)
                    armorBonus = bonusCap;
            }
            else if (ad.Target is MimicNPC m)
            {
                ad.ArmorHitLocation = ((MimicNPC)ad.Target).CalculateArmorHitLocation(ad);

                if (ad.Target.Inventory != null)
                {
                    DbInventoryItem armor = ad.Target.Inventory.GetItem((eInventorySlot)ad.ArmorHitLocation);

                    if (armor != null)
                        armorBonus = armor.Bonus;
                }

                int bonusCap = GetBonusCapForLevel(m.Level);

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

            if ((ad.Target is GamePlayer || ad.Target is MimicNPC) && (ad.Attacker is GamePlayer || ad.Attacker is MimicNPC))
                missChance += armorBonus;
            else
                missChance += missChance * armorBonus / 100;

            // Style bonuses.
            if (ad.Style != null)
                missChance -= ad.Style.BonusToHit;

            if (lastAD != null && lastAD.AttackResult == eAttackResult.HitStyle && lastAD.Style != null)
                missChance += lastAD.Style.BonusToDefense;

            if ((owner is GamePlayer || owner is MimicNPC) && (ad.Attacker is GamePlayer || ad.Attacker is MimicNPC) && weapon != null)
                missChance -= (int)((ad.Attacker.WeaponSpecLevel(weapon) - 1) * 0.1);

            if (action.ActiveWeaponSlot == eActiveWeaponSlot.Distance)
            {
                DbInventoryItem ammo = ad.Attacker.rangeAttackComponent.Ammo;

                if (ammo != null)
                {
                    switch ((ammo.SPD_ABS >> 4) & 0x3)
                    {
                        // http://rothwellhome.org/guides/archery.htm
                        case 0:
                        missChance += (int)Math.Round(missChance * 0.15);
                        break; // Rough
                        //case 1:
                        //  missrate -= 0;
                        //  break;
                        case 2:
                        missChance -= (int)Math.Round(missChance * 0.15);
                        break; // doesn't exist (?)
                        case 3:
                        missChance -= (int)Math.Round(missChance * 0.25);
                        break; // Footed
                    }
                }
            }

            return missChance;
        }

        /// <summary>
        /// Checks whether Living has ability to use lefthanded weapons
        /// </summary>
        public override bool CanUseLefthandedWeapon
        {
            get
            {
                if (owner is GamePlayer)
                    return (owner as GamePlayer).CharacterClass.CanUseLefthandedWeapon;
                else if (owner is MimicNPC)
                    return (owner as MimicNPC).CharacterClass.CanUseLefthandedWeapon;
                else
                    return false;
            }
        }
    }
}