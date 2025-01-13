using System;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Scripts;

namespace DOL.GS.Spells
{
	[SpellHandler(eSpellType.DirectDamage)]
	public class DirectDamageSpellHandler : SpellHandler
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private bool m_castFailed = false;

		/// <summary>
		/// Execute direct damage spell
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
			if (!m_castFailed)
			{
				m_caster.Mana -= PowerCost(target);
			}

			base.FinishSpellCast(target);
		}

		/// <summary>
		/// Calculates the base 100% spell damage which is then modified by damage variance factors
		/// </summary>
		/// <returns></returns>
		public override double CalculateDamageBase(GameLiving target)
		{
            IGamePlayer player = Caster as IGamePlayer;

			// % damage procs
			if (Spell.Damage < 0)
			{
				double spellDamage = 0;

				if (player != null)
				{
					// This equation is used to simulate live values - Tolakram
					spellDamage = (target.MaxHealth * -Spell.Damage * .01) / 2.5;
				}

				if (spellDamage < 0)
					spellDamage = 0;

				return spellDamage;
			}

			return base.CalculateDamageBase(target);
		}


		public override double DamageCap(double effectiveness)
		{
			if (Spell.Damage < 0)
			{
				return (Target.MaxHealth * -Spell.Damage * .01) * 3.0 * effectiveness;
			}

			return base.DamageCap(effectiveness);
		}

		public override void OnDirectEffect(GameLiving target)
		{
			if (target == null)
				return;

			// 1.65 compliance. No LoS check on PBAoE or AoE spells.
			if (Spell.Target is eSpellTarget.CONE)
			{
				GamePlayer checkPlayer = null;

				if (target is GamePlayer)
					checkPlayer = target as GamePlayer;
				else
				{
					if (Caster is GamePlayer)
						checkPlayer = Caster as GamePlayer;
					else if (Caster is GameNPC npcCaster && npcCaster.Brain is IControlledBrain npcCasterBrain)
						checkPlayer = npcCasterBrain.GetPlayerOwner();
				}

				if (checkPlayer != null)
					checkPlayer.Out.SendCheckLos(Caster, target, new CheckLosResponse(DealDamageCheckLos));
				else
					DealDamage(target);
			}
			else
				DealDamage(target);
		}

		protected virtual void DealDamageCheckLos(GamePlayer player, eLosCheckResponse response, ushort sourceOID, ushort targetOID)
		{
			if (response is eLosCheckResponse.TRUE)
			{
				try
				{
					GameLiving target = Caster.CurrentRegion.GetObject(targetOID) as GameLiving;
					if (target != null)
					{
						DealDamage(target);
						// Due to LOS check delay the actual cast happens after FinishSpellCast does a notify, so we notify again
						GameEventMgr.Notify(GameLivingEvent.CastFinished, m_caster, new CastingEventArgs(this, target, m_lastAttackData));
					}
				}
				catch (Exception e)
				{
					m_castFailed = true;

					if (log.IsErrorEnabled)
						log.Error(string.Format("targetOID:{0} caster:{1} exception:{2}", targetOID, Caster, e));
				}
			}
			else
			{
				if (Spell.Target == eSpellTarget.ENEMY && Spell.Radius == 0 && Spell.Range != 0)
				{
					m_castFailed = true;
					MessageToCaster("You can't see your target!", eChatType.CT_SpellResisted);
				}
			}
		}

		protected virtual void DealDamage(GameLiving target)
		{
			if (!target.IsAlive || target.ObjectState is not GameObject.eObjectState.Active)
				return;

			AttackData ad = CalculateDamageToTarget(target);
			SendDamageMessages(ad);
			DamageTarget(ad, true);
			target.StartInterruptTimer(target.SpellInterruptDuration, ad.AttackType, Caster);
		}

		/*
		 * We need to send resist spell los check packets because spell resist is calculated first, and
		 * so you could be inside keep and resist the spell and be interrupted when not in view
		 */
		protected override void OnSpellResisted(GameLiving target)
		{
			// 1.65 compliance. No LoS check on PBAoE or AoE spells.
			if (Spell.Target is eSpellTarget.CONE)
			{
				GamePlayer checkPlayer = null;

				if (target is GamePlayer)
					checkPlayer = target as GamePlayer;
				else
				{
					if (Caster is GamePlayer)
						checkPlayer = Caster as GamePlayer;
					else if (Caster is GameNPC npcCaster && npcCaster.Brain is IControlledBrain npcCasterBrain)
						checkPlayer = npcCasterBrain.GetPlayerOwner();
				}

				if (checkPlayer != null)
					checkPlayer.Out.SendCheckLos(Caster, target, new CheckLosResponse(ResistSpellCheckLos));
				else
					base.OnSpellResisted(target);
			}
			else
				base.OnSpellResisted(target);
		}

		private void ResistSpellCheckLos(GamePlayer player, eLosCheckResponse response, ushort sourceOID, ushort targetOID)
		{
			if (response is eLosCheckResponse.TRUE)
			{
				try
				{
					GameLiving target = Caster.CurrentRegion.GetObject(targetOID) as GameLiving;
					if (target != null)
						base.OnSpellResisted(target);
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error(string.Format("targetOID:{0} caster:{1} exception:{2}", targetOID, Caster, e));
				}
			}
		}

		// constructor
		public DirectDamageSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
