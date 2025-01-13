using System.Collections.Generic;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.Scripts;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Pve Resurrection Illness
	/// </summary>
	[SpellHandler(eSpellType.PveResurrectionIllness)]
	public class PveResurrectionIllness : AbstractIllnessSpellHandler
	{
		public override ECSGameSpellEffect CreateECSEffect(ECSGameEffectInitParams initParams)
		{
			IGamePlayer targetPlayer = Target as IGamePlayer;

			if (targetPlayer != null)
            {
                // Higher level rez spells reduce duration of rez sick.
                if (targetPlayer.TempProperties.GetAllProperties().Contains(GamePlayer.RESURRECT_REZ_SICK_EFFECTIVENESS))
                {
					double rezSickEffectiveness = targetPlayer.TempProperties.GetProperty<double>(GamePlayer.RESURRECT_REZ_SICK_EFFECTIVENESS);
                    targetPlayer.TempProperties.RemoveProperty(GamePlayer.RESURRECT_REZ_SICK_EFFECTIVENESS);
                    initParams.Duration = (int)(initParams.Duration * rezSickEffectiveness);
                }
                
                if (targetPlayer.GetModified(eProperty.ResIllnessReduction) > 0)
                {
	                initParams.Duration = initParams.Duration * (100-targetPlayer.GetModified(eProperty.ResIllnessReduction))/100;
                }
            }

			return new ResurrectionIllnessECSGameEffect(initParams);
		}

		/// <summary>
		/// When an applied effect starts
		/// duration spells only
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectStart(GameSpellEffect effect)
		{
			//GamePlayer player = effect.Owner as GamePlayer;
			//if (player != null)
			//{
			//	player.Effectiveness -= Spell.Value * 0.01;
			//	player.Out.SendUpdateWeaponAndArmorStats();
			//	player.Out.SendStatusUpdate();
			//}
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
			//GamePlayer player = effect.Owner as GamePlayer;
			//if (player != null)
			//{
			//	player.Effectiveness += Spell.Value * 0.01;
			//	player.Out.SendUpdateWeaponAndArmorStats();
			//	player.Out.SendStatusUpdate();
			//}
			return 0;
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList<string> DelveInfo 
		{
			get 
			{
				/*
				<Begin Info: Rusurrection Illness>
 
				The player's effectiveness is greatly reduced due to being recently resurrected.
 
				- Effectiveness penality: 50%
				- 4:56 remaining time
 
				<End Info>
				*/
				var list = new List<string>();

				list.Add(" "); //empty line
				list.Add(Spell.Description);
				list.Add(" "); //empty line
				list.Add("- Effectiveness penality: "+Spell.Value+"%");
				return list;
			}
		}

        /// <summary>
        /// Saves the effect when player quits
        /// </summary>        
        public override DbPlayerXEffect GetSavedEffect(GameSpellEffect e)
        {
            DbPlayerXEffect eff = new DbPlayerXEffect();
            eff.Var1 = Spell.ID;
            eff.Duration = e.RemainingTime;
            eff.IsHandler = true;
            eff.SpellLine = SpellLine.KeyName;
            return eff;
        }

        /// <summary>
        /// Restart the effects of resurrection illness
        /// </summary>        
        public override void OnEffectRestored(GameSpellEffect effect, int[] vars)
		{
			OnEffectStart(effect);
		}

        /// <summary>
        /// Remove the effects of resurrection illness 
        /// </summary>        
		public override int OnRestoredEffectExpires(GameSpellEffect effect, int[] vars, bool noMessages)
		{
			return OnEffectExpires(effect, false);
		}		

		public PveResurrectionIllness(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}	
	}

	/// <summary>
	/// Contains all common code for illness spell handlers (and negative spell effects without animation) 
	/// </summary>
	public class AbstractIllnessSpellHandler : SpellHandler
	{
		public override bool HasPositiveEffect 
		{
			get 
			{ 
				return false;
			}
		}

		public override double CalculateSpellResistChance(GameLiving target)
		{
			return 0;
		}

		protected override int CalculateEffectDuration(GameLiving target)
		{
			double modifier = 1.0;
			RealmAbilities.VeilRecoveryAbility ab = target.GetAbility<RealmAbilities.VeilRecoveryAbility>();
			if (ab != null)
				modifier -= ((double)ab.Amount / 100);

			return (int)((double)Spell.Duration * modifier); 
		}

		public AbstractIllnessSpellHandler(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}
	}
}
