using System;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.Scripts;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Abstract CC spell handler
    /// </summary>
    public abstract class AbstractCCSpellHandler : ImmunityEffectSpellHandler
    {
        public override void ApplyEffectOnTarget(GameLiving target)
        {
            if (target.HasAbility(Abilities.CCImmunity))
            {
                MessageToCaster(target.Name + " is immune to this effect!", eChatType.CT_SpellResisted);
                return;
            }

            if (target.EffectList.GetOfType<ChargeEffect>() != null || target.TempProperties.GetProperty<bool>("Charging"))
            {
                MessageToCaster(target.Name + " is moving too fast for this spell to have any effect!", eChatType.CT_SpellResisted);
                return;
            }

            base.ApplyEffectOnTarget(target);
        }

        /// <summary>
        /// When an applied effect expires.
        /// Duration spells only.
        /// </summary>
        /// <param name="effect">The expired effect</param>
        /// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
        /// <returns>immunity duration in milliseconds</returns>
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            if (effect.Owner == null)
                return 0;

            base.OnEffectExpires(effect, noMessages);

            if (effect.Owner is GamePlayer player)
            {
                player.Client.Out.SendUpdateMaxSpeed();

                if (player.Group != null)
                    player.Group.UpdateMember(player, false, false);
            }

            effect.Owner.Notify(GameLivingEvent.CrowdControlExpired, effect.Owner);
            return (effect.Name == "Pet Stun") ? 0 : 60000;
        }

        protected override int CalculateEffectDuration(GameLiving target)
        {
            double duration = base.CalculateEffectDuration(target);

            // [Atlas - Takii] Disabling MOC effectiveness scaling in OF.
            // double mocFactor = 1.0;
            // MasteryofConcentrationEffect moc = Caster.EffectList.GetOfType<MasteryofConcentrationEffect>();
            // if (moc != null)
            // {
            //     AtlasOF_MasteryofConcentration ra = Caster.GetAbility<AtlasOF_MasteryofConcentration>();
            //     if (ra != null)
            //         mocFactor = System.Math.Round((double)ra.GetAmountForLevel(ra.Level) / 100, 2);
            //     duration = (double)Math.Round(duration * mocFactor);
            // }

            if (Spell.SpellType != eSpellType.StyleStun)
            {
                // capping duration adjustment to 100%, live cap unknown - Tolakram
                double hitChance = Math.Min(200, CalculateToHitChance(target));

                if (hitChance <= 0)
                {
                    duration = 0;
                }
                else if (hitChance < 55)
                {
                    duration -= duration * (55 - hitChance) * 0.01;
                }
                else if (hitChance > 100)
                {
                    duration += duration * (hitChance - 100) * 0.01;
                }
            }

            return (int)duration;
        }

        public override double CalculateSpellResistChance(GameLiving target)
        {
            double resistChance;

            /*
            GameSpellEffect fury = SpellHandler.FindEffectOnTarget(target, "Fury");
            if (fury != null)
            {
                resist += (int)fury.Spell.Value;
            }*/

            // Bonedancer RR5.
            if (target.EffectList.GetOfType<AllureofDeathEffect>() != null)
                return AllureofDeathEffect.ccchance;

            if (m_spellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect)
                return 0;
            if (HasPositiveEffect)
                return 0;

            double hitChance = CalculateToHitChance(target);

            // Calculate the resist chance.
            resistChance = 100 - hitChance;

            if (resistChance > 100)
                resistChance = 100;

            // Use ResurrectHealth = 1 if the CC should not be resisted.
            if (Spell.ResurrectHealth == 1)
                resistChance = 0;

            return resistChance;
        }

        public AbstractCCSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    /// <summary>
    /// Mezz
    /// </summary>
    [SpellHandler(eSpellType.Mesmerize)]
    public class MesmerizeSpellHandler : AbstractCCSpellHandler
    {
        public override ECSGameSpellEffect CreateECSEffect(ECSGameEffectInitParams initParams)
        {
            return new MezECSGameEffect(initParams);
        }

        public override void OnEffectPulse(GameSpellEffect effect)
        {
            SendEffectAnimation(effect.Owner, 0, false, 1);
            base.OnEffectPulse(effect);
        }

        protected override bool CheckSpellResist(GameLiving target)
        {
            bool earlyResist = false;

            if (target.effectListComponent.ContainsEffectForEffectType(eEffect.Mez))
            {
                MessageToCaster("Your target is already mezzed!", eChatType.CT_SpellResisted);
                earlyResist = true;
            }

            if (target.effectListComponent.Effects.ContainsKey(eEffect.MezImmunity) || target.HasAbility(Abilities.MezzImmunity))
            {
                MessageToCaster($"{target.Name} is immune to this effect!", eChatType.CT_SpellResisted);
                earlyResist = true;
            }

            if (FindStaticEffectOnTarget(target, typeof(MezzRootImmunityEffect)) != null)
            {
                MessageToCaster("Your target is immune!", eChatType.CT_System);
                earlyResist = true;
            }

            if (target is GameNPC && target is not MimicNPC && target.HealthPercent < 75)
            {
                MessageToCaster("Your target is enraged and resists the spell!", eChatType.CT_System);
                earlyResist = true;
            }

            GameSpellEffect mezblock = FindEffectOnTarget(target, "CeremonialBracerMezz");

            if (mezblock != null)
            {
                mezblock.Cancel(false);
                (target as GamePlayer)?.Out.SendMessage("Your item effect intercepts the mesmerization spell and fades!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                MessageToCaster("Ceremonial Bracer intercept your mez!", eChatType.CT_SpellResisted);
                earlyResist = true;
            }

            if (earlyResist)
            {
                SendSpellResistAnimation(target);
                SendSpellResistNotification(target);
                StartSpellResistInterruptTimer(target);
                StartSpellResistLastAttackTimer(target);
                return true;
            }

            return base.CheckSpellResist(target);
        }

        protected override int CalculateEffectDuration(GameLiving target)
        {
            double duration = base.CalculateEffectDuration(target);
            duration *= target.GetModified(eProperty.MesmerizeDurationReduction) * 0.01;
            NPCECSMezImmunityEffect npcImmune = (NPCECSMezImmunityEffect)EffectListService.GetEffectOnTarget(target, eEffect.NPCMezImmunity);

            if (npcImmune != null)
                duration = npcImmune.CalculateMezDuration((long)duration);

            if (duration < 1)
                duration = 1;
            else if (duration > (Spell.Duration * 4))
                duration = Spell.Duration * 4;

            return (int)duration;
        }

        public MesmerizeSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    /// <summary>
    /// Stun
    /// </summary>
    [SpellHandler(eSpellType.Stun)]
    public class StunSpellHandler : AbstractCCSpellHandler
    {
        public override ECSGameSpellEffect CreateECSEffect(ECSGameEffectInitParams initParams)
        {
            return new StunECSGameEffect(initParams);
        }

        protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
        {
            // Use ResurrectMana=1 if the Stun should not have immunity.

            if (Spell.ResurrectMana == 1)
            {
                int freq = Spell != null ? Spell.Frequency : 0;
                return new GameSpellEffect(this, CalculateEffectDuration(target), freq, effectiveness);
            }

            else
                return new GameSpellAndImmunityEffect(this, CalculateEffectDuration(target), 0, effectiveness);
        }

        /// <summary>
        /// When an applied effect expires.
        /// Duration spells only.
        /// </summary>
        /// <param name="effect">The expired effect</param>
        /// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
        /// <returns>immunity duration in milliseconds</returns>
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            effect.Owner.IsStunned=false;
            effect.Owner.DisableTurning(false);

            // Use ResurrectHealth > 0 to calculate stun immunity timer (such pet stun spells), actually (1.90) pet stun immunity is 5x the stun duration.
            if (Spell.ResurrectHealth > 0)
            {
                base.OnEffectExpires(effect, noMessages);
                return Spell.Duration * Spell.ResurrectHealth;
            }

            return base.OnEffectExpires(effect, noMessages);
        }

        public override void ApplyEffectOnTarget(GameLiving target)
        {
            if ((target.effectListComponent.Effects.ContainsKey(eEffect.StunImmunity) && this is not UnresistableStunSpellHandler) || (EffectListService.GetEffectOnTarget(target, eEffect.Stun) != null && !(Caster is GameSummonedPet)))//target.HasAbility(Abilities.StunImmunity))
            {
                MessageToCaster(target.Name + " is immune to this effect!", eChatType.CT_SpellResisted);
                target.StartInterruptTimer(target.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
                base.OnSpellResisted(target);
                return;
            }

            // Ceremonial bracer doesn't intercept physical stun.
            if(Spell.SpellType != eSpellType.StyleStun)
            {
                /*
                GameSpellEffect stunblock = SpellHandler.FindEffectOnTarget(target, "CeremonialBracerStun");
                if (stunblock != null)
                {
                    stunblock.Cancel(false);
                    if (target is GamePlayer)
                        (target as GamePlayer).Out.SendMessage("Your item effect intercepts the stun spell and fades!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    base.OnSpellResisted(target);
                    return;
                }*/
            }

            base.ApplyEffectOnTarget(target);
        }

        protected override int CalculateEffectDuration(GameLiving target)
        {
            double duration = base.CalculateEffectDuration(target);
            duration *= target.GetModified(eProperty.StunDurationReduction) * 0.01;
            NPCECSStunImmunityEffect npcImmune = (NPCECSStunImmunityEffect)EffectListService.GetEffectOnTarget(target, eEffect.NPCStunImmunity);

            if (npcImmune != null)
                duration = npcImmune.CalculateStunDuration((long)duration); //target.GetModified(eProperty.StunDurationReduction) * 0.01;

            if (duration < 1)
                duration = 1;
            else if (duration > (Spell.Duration * 4))
                duration = Spell.Duration * 4;

            return (int)duration;
        }

        /// <summary>
        /// Determines wether this spell is compatible with given spell
        /// and therefore overwritable by better versions
        /// spells that are overwritable cannot stack
        /// </summary>
        public override bool IsOverwritable(ECSGameSpellEffect compare)
        {
            if (Spell.EffectGroup != 0 || compare.SpellHandler.Spell.EffectGroup != 0)
                return Spell.EffectGroup == compare.SpellHandler.Spell.EffectGroup;
            if (compare.SpellHandler.Spell.SpellType == eSpellType.StyleStun) return true;
            return base.IsOverwritable(compare);
        }

        public StunSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}
