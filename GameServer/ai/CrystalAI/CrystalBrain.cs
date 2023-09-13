/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Crystal;
using DOL.AI;
using DOL.AI.Brain;
using DOL.GS;
using DOL.GS.RealmAbilities;
using DOL.GS.SkillHandler;
using DOL.GS.Spells;
using log4net;


public class CrystalBrain : ABrain, IControlledBrain
{
    public GameLoopDecisionMaker DecisionMaker;
    public bool checkAbility;
    protected static SpellLine m_mobSpellLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells);
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public CrystalBrain(GameLiving attachedEntity) : base()
    {
        DecisionMaker = GameLoopDecisionMaker.CreateWarrior(attachedEntity);
        DecisionMaker.Start();
    }


    public override void Think()
    {
        if (Body is Companion {RootOwner: null} companion)
        {
            companion.Die(companion);
            return;
        }
        
        // Load abilities on first Think() cycle.
        if (!checkAbility)
        {
            CheckAbilities();
            Body.SortSpells();
            checkAbility = true;
        }

        DecisionMaker?.Think();
    }

    public override void KillFSM()
    {
    }

    public eWalkState WalkState { get; }
    public eAggressionState AggressionState { get; set; }
    protected GameLiving m_owner;

    public GameLiving Owner
    {
        get { return m_owner; }
        set { m_owner = value; }
    }

    public GameLiving AttachedEntity;

    public void Attack(GameObject target)
    {
    }

    public void Disengage()
    {
    }

    public void Follow(GameObject target)
    {
    }

    public void FollowOwner()
    {
    }

    public void Stay()
    {
    }

    public void ComeHere()
    {
    }

    public void Goto(GameObject target)
    {
    }

    public void UpdatePetWindow()
    {
    }

    public GamePlayer GetPlayerOwner()
    {
        return Owner as GamePlayer;
    }

    public GameNPC GetNPCOwner()
    {
        return Owner as GameNPC;
    }

    public GameLiving GetLivingOwner()
    {
        return Owner;
    }

    public void SetAggressionState(eAggressionState state)
    {
    }

    public bool IsMainPet { get; set; }

    protected bool CanCastDefensiveSpell(Spell spell)
    {
        if (spell == null || spell.IsHarmful)
            return false;

        // Make sure we're currently able to cast the spell.
        if (spell.CastTime > 0 && Body.IsBeingInterrupted && !spell.Uninterruptible)
            return false;

        // Make sure the spell isn't disabled.
        return !spell.HasRecastDelay || Body.GetSkillDisabledDuration(spell) <= 0;
    }

    public bool CheckSpells(StandardMobBrain.eCheckSpellType type)
    {
        if (Body == null || Body.Spells == null || Body.Spells.Count < 1)
            return false;

        bool casted = false;
        if (type == StandardMobBrain.eCheckSpellType.Defensive)
        {
            // Check instant spells, but only cast one of each type to prevent spamming
            if (Body.CanCastInstantHealSpells)
            {
                foreach (Spell spell in Body.InstantHealSpells)
                {
                    if (CheckDefensiveSpells(spell))
                        break;
                }
            }

            if (Body.CanCastInstantMiscSpells)
            {
                foreach (Spell spell in Body.InstantMiscSpells)
                {
                    if (CheckDefensiveSpells(spell))
                        break;
                }
            }

            // Check spell lists, prioritizing healing
            if (Body.CanCastHealSpells)
            {
                foreach (Spell spell in Body.HealSpells)
                {
                    if (CheckDefensiveSpells(spell))
                    {
                        casted = true;
                        break;
                    }
                }
            }

            if (!casted && Body.CanCastMiscSpells)
            {
                foreach (Spell spell in Body.MiscSpells)
                {
                    if (CheckDefensiveSpells(spell))
                    {
                        casted = true;
                        break;
                    }
                }
            }
        }
        else if (Body.TargetObject is GameLiving living && living.IsAlive)
        {
            // Check instant spells, but only cast one to prevent spamming
            if (Body.CanCastInstantHarmfulSpells)
            {
                foreach (Spell spell in Body.InstantHarmfulSpells)
                {
                    if (CheckOffensiveSpells(spell))
                        break;
                }
            }

            if (Body.CanCastHarmfulSpells)
            {
                foreach (Spell spell in Body.HarmfulSpells)
                {
                    if (CheckOffensiveSpells(spell))
                    {
                        casted = true;
                        break;
                    }
                }
            }
        }

        return casted || Body.IsCasting;
    }

    protected bool CheckDefensiveSpells(Spell spell)
    {
        if (!CanCastDefensiveSpell(spell))
            return false;

        bool casted = false;
        Body.TargetObject = null;
        GamePlayer player;
        GameLiving owner;

        switch (spell.SpellType)
        {
            #region Buffs

            case eSpellType.AcuityBuff:
            case eSpellType.AFHitsBuff:
            case eSpellType.AllMagicResistBuff:
            case eSpellType.ArmorAbsorptionBuff:
            case eSpellType.ArmorFactorBuff:
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
            case eSpellType.PaladinArmorFactorBuff:
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
            {
                string target = spell.Target.ToUpper();

                // Buff self
                if (!LivingHasEffect(Body, spell))
                {
                    Body.TargetObject = Body;
                    break;
                }

                if (target is "REALM" or "GROUP")
                {
                    owner = (this as IControlledBrain).Owner;

                    // Buff owner
                    if (!LivingHasEffect(owner, spell) && Body.IsWithinRadius(owner, spell.Range))
                    {
                        Body.TargetObject = owner;
                        break;
                    }

                    if (owner is GameNPC npc)
                    {
                        //Buff other minions
                        foreach (IControlledBrain icb in npc.ControlledNpcList)
                        {
                            if (icb != null && icb.Body != null && !LivingHasEffect(icb.Body, spell)
                                && Body.IsWithinRadius(icb.Body, spell.Range))
                            {
                                Body.TargetObject = icb.Body;
                                break;
                            }
                        }
                    }

                    player = GetPlayerOwner();

                    // Buff player
                    if (player != null)
                    {
                        if (!LivingHasEffect(player, spell))
                        {
                            Body.TargetObject = player;
                            break;
                        }

                        if (player.Group != null)
                        {
                            foreach (GamePlayer member in player.Group.GetPlayersInTheGroup())
                            {
                                if (!LivingHasEffect(member, spell) && Body.IsWithinRadius(member, spell.Range))
                                {
                                    Body.TargetObject = member;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
                break;

            #endregion Buffs

            #region Disease Cure/Poison Cure/Summon

            case eSpellType.CureDisease:
                //Cure owner
                owner = (this as IControlledBrain).Owner;
                if (owner.IsDiseased)
                {
                    Body.TargetObject = owner;
                    break;
                }

                //Cure self
                if (Body.IsDiseased)
                {
                    Body.TargetObject = Body;
                    break;
                }

                // Cure group members

                player = GetPlayerOwner();

                if (player.Group != null)
                {
                    foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
                    {
                        if (p.IsDiseased && Body.IsWithinRadius(p, spell.Range))
                        {
                            Body.TargetObject = p;
                            break;
                        }
                    }
                }

                break;
            case eSpellType.CurePoison:
                //Cure owner
                owner = (this as IControlledBrain).Owner;
                if (LivingIsPoisoned(owner))
                {
                    Body.TargetObject = owner;
                    break;
                }

                //Cure self
                if (LivingIsPoisoned(Body))
                {
                    Body.TargetObject = Body;
                    break;
                }

                // Cure group members

                player = GetPlayerOwner();

                if (player.Group != null)
                {
                    foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
                    {
                        if (LivingIsPoisoned(p) && Body.IsWithinRadius(p, spell.Range))
                        {
                            Body.TargetObject = p;
                            break;
                        }
                    }
                }

                break;
            case eSpellType.Summon:
                Body.TargetObject = Body;
                break;

            #endregion

            #region Heals

            case eSpellType.CombatHeal:
            case eSpellType.Heal:
            case eSpellType.HealOverTime:
            case eSpellType.MercHeal:
            case eSpellType.OmniHeal:
            case eSpellType.PBAoEHeal:
            case eSpellType.SpreadHeal:
                String spellTarget = spell.Target.ToUpper();
                int bodyPercent = Body.HealthPercent;
                //underhill ally heals at half the normal threshold 'will heal seriously injured groupmates'
                int healThreshold = this.Body.Name.Contains("underhill") ? DOL.GS.ServerProperties.Properties.NPC_HEAL_THRESHOLD / 2 : DOL.GS.ServerProperties.Properties.NPC_HEAL_THRESHOLD;

                if (Body.Name.Contains("empyrean"))
                {
                    healThreshold = this.Body.Name.Contains("empyrean") ? DOL.GS.ServerProperties.Properties.CHARMED_NPC_HEAL_THRESHOLD : DOL.GS.ServerProperties.Properties.NPC_HEAL_THRESHOLD;
                }

                if (spellTarget == "SELF")
                {
                    if (bodyPercent < healThreshold && !LivingHasEffect(Body, spell))
                        Body.TargetObject = Body;

                    break;
                }

                // Heal seriously injured targets first
                int emergencyThreshold = healThreshold / 2;

                //Heal owner
                owner = (this as IControlledBrain).Owner;
                int ownerPercent = owner.HealthPercent;
                if (ownerPercent < emergencyThreshold && !LivingHasEffect(owner, spell) && Body.IsWithinRadius(owner, spell.Range))
                {
                    Body.TargetObject = owner;
                    break;
                }

                //Heal self
                if (bodyPercent < emergencyThreshold
                    && !LivingHasEffect(Body, spell))
                {
                    Body.TargetObject = Body;
                    break;
                }

                // Heal group
                player = GetPlayerOwner();
                ICollection<GamePlayer> playerGroup = null;
                if (player.Group != null && (spellTarget == "REALM" || spellTarget == "GROUP"))
                {
                    playerGroup = player.Group.GetPlayersInTheGroup();

                    foreach (GamePlayer p in playerGroup)
                    {
                        if (p.HealthPercent < emergencyThreshold && !LivingHasEffect(p, spell)
                                                                 && Body.IsWithinRadius(p, spell.Range))
                        {
                            Body.TargetObject = p;
                            break;
                        }
                    }
                }

                // Now check for targets which aren't seriously injured

                if (spellTarget == "SELF")
                {
                    // if we have a self heal and health is less than 75% then heal, otherwise return false to try another spell or do nothing
                    if (bodyPercent < healThreshold
                        && !LivingHasEffect(Body, spell))
                    {
                        Body.TargetObject = Body;
                    }

                    break;
                }

                //Heal owner
                owner = (this as IControlledBrain).Owner;
                if (ownerPercent < healThreshold
                    && !LivingHasEffect(owner, spell) && Body.IsWithinRadius(owner, spell.Range))
                {
                    Body.TargetObject = owner;
                    break;
                }

                //Heal self
                if (bodyPercent < healThreshold
                    && !LivingHasEffect(Body, spell))
                {
                    Body.TargetObject = Body;
                    break;
                }

                // Heal group
                if (playerGroup != null)
                {
                    foreach (GamePlayer p in playerGroup)
                    {
                        if (p.HealthPercent < healThreshold
                            && !LivingHasEffect(p, spell) && Body.IsWithinRadius(p, spell.Range))
                        {
                            Body.TargetObject = p;
                            break;
                        }
                    }
                }

                break;

            #endregion

            default:
                log.Warn($"CheckDefensiveSpells() encountered an unknown spell type [{spell.SpellType}] for CrystalBrain {Body?.Name}");
                break;
        }

        if (Body?.TargetObject != null)
            casted = Body.CastSpell(spell, m_mobSpellLine, true);

        return casted;
    }

    // Temporary until StandardMobBrain is updated
    protected bool CheckOffensiveSpells(Spell spell)
    {
        if (spell == null || spell.IsHelpful || !(Body.TargetObject is GameLiving living) || !living.IsAlive)
            return false;

        // Make sure we're currently able to cast the spell
        if (spell.CastTime > 0 && Body.IsBeingInterrupted && !spell.Uninterruptible)
            return false;

        // Make sure the spell isn't disabled
        if (spell.HasRecastDelay && Body.GetSkillDisabledDuration(spell) > 0)
            return false;

        if (!Body.IsWithinRadius(Body.TargetObject, spell.Range))
            return false;

        if (!spell.Target.Equals("enemy", StringComparison.OrdinalIgnoreCase) &&
            !spell.Target.Equals("area", StringComparison.OrdinalIgnoreCase) &&
            !spell.Target.Equals("cone", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        bool casted = false;

        if (Body.TargetObject is GameLiving && (spell.Duration == 0 || !LivingHasEffect(living, spell) || spell.SpellType == eSpellType.DirectDamageWithDebuff || spell.SpellType == eSpellType.DamageSpeedDecrease))
        {
            if (Body.TargetObject != Body)
                Body.TurnTo(Body.TargetObject);

            casted = Body.CastSpell(spell, m_mobSpellLine);

            if (casted)
            {
                if (spell.CastTime > 0)
                    Body.StopFollowing();
                else if (Body.FollowTarget != Body.TargetObject)
                    Body.Follow(Body.TargetObject, GameNPC.STICK_MINIMUM_RANGE, GameNPC.STICK_MAXIMUM_RANGE);
            }
        }

        return casted;
    }

    public bool LivingHasEffect(GameLiving target, Spell spell)
    {
        if (target == null)
            return true;

        /* all my homies hate vampires
        if (target is GamePlayer && (target as GamePlayer).CharacterClass.ID == (int)eCharacterClass.Vampiir)
        {
            switch (spell.SpellType)
            {
                case eSpellType.StrengthConstitutionBuff:
                case eSpellType.DexterityQuicknessBuff:
                case eSpellType.StrengthBuff:
                case eSpellType.DexterityBuff:
                case eSpellType.ConstitutionBuff:
                case eSpellType.AcuityBuff:
                    return true;
            }
        }*/

        ISpellHandler spellHandler = Body.castingComponent.SpellHandler;

        // If we're currently casting 'spell' on 'target', assume it already has the effect.
        // This allows spell queuing while preventing casting on the same target more than once.
        if (spellHandler != null && spellHandler.Spell.ID == spell.ID && spellHandler.Target == target)
            return true;

        // May not be the right place for that, but without that check NPCs with more than one offensive or defensive proc will only buff themselves once.
        if (spell.SpellType is eSpellType.OffensiveProc or eSpellType.DefensiveProc)
        {
            if (target.effectListComponent.Effects.TryGetValue(EffectService.GetEffectFromSpell(spell, m_mobSpellLine.IsBaseLine), out List<ECSGameEffect> existingEffects))
            {
                if (existingEffects.FirstOrDefault(e => e.SpellHandler.Spell.ID == spell.ID || (spell.EffectGroup > 0 && e.SpellHandler.Spell.EffectGroup == spell.EffectGroup)) != null)
                    return true;
            }

            return false;
        }

        eEffect spellEffect = EffectService.GetEffectFromSpell(spell, m_mobSpellLine.IsBaseLine);
        ECSGameEffect effect = EffectListService.GetEffectOnTarget(target, spellEffect);

        if (effect != null)
            return true;

        eEffect immunityToCheck = eEffect.Unknown;

        switch (spellEffect)
        {
            case eEffect.Stun:
            {
                immunityToCheck = eEffect.StunImmunity;
                break;
            }
            case eEffect.Mez:
            {
                immunityToCheck = eEffect.MezImmunity;
                break;
            }
            case eEffect.Snare:
            case eEffect.MeleeSnare:
            {
                immunityToCheck = eEffect.SnareImmunity;
                break;
            }
            case eEffect.Nearsight:
            {
                immunityToCheck = eEffect.NearsightImmunity;
                break;
            }
        }

        return immunityToCheck != eEffect.Unknown && EffectListService.GetEffectOnTarget(target, immunityToCheck) != null;
    }

    protected static bool LivingIsPoisoned(GameLiving target)
    {
        return target.effectListComponent.ContainsEffectForEffectType(eEffect.DamageOverTime);
    }
    
    public void CheckAbilities()
    {
        if (Body.Abilities == null || Body.Abilities.Count <= 0)
            return;

        foreach (Ability ab in Body.Abilities.Values)
        {
            switch (ab.KeyName)
            {
                case Abilities.Intercept:
                {
                    // The pet should intercept even if a player is still intercepting for the owner.
                    GamePlayer playerOwner = GetPlayerOwner();

                    if (playerOwner != null)
                        new InterceptECSGameEffect(new ECSGameEffectInitParams(Body, 0, 1), Body, playerOwner);

                    break;
                }
                case Abilities.Guard:
                {
                    GamePlayer playerOwner = GetPlayerOwner();

                    if (playerOwner != null)
                    {
                        GuardAbilityHandler.CheckExistingEffectsOnTarget(Body, playerOwner, false, out bool foundOurEffect, out GuardECSGameEffect existingEffectFromAnotherSource);

                        if (foundOurEffect)
                            break;

                        if (existingEffectFromAnotherSource == null)
                            GuardAbilityHandler.CancelOurEffectThenAddOnTarget(Body, playerOwner);
                    }

                    break;
                }
                case Abilities.Protect:
                {
                    GamePlayer playerOwner = GetPlayerOwner();

                    if (playerOwner != null)
                        new ProtectECSGameEffect(new ECSGameEffectInitParams(playerOwner, 0, 1), null, playerOwner);

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