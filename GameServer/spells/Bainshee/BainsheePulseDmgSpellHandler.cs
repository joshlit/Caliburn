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
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("BainsheePulseDmg")]
	public class BainsheePulseDmgSpellHandler : SpellHandler
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public const string FOCUS_WEAK = "FocusSpellHandler.Online";
		/// <summary>
		/// Execute direct damage spell
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
            if (Spell.Pulse != 0)
            {
                GameEventMgr.AddHandler(Caster, GamePlayerEvent.Moving, new DOLEventHandler(EventAction));
                GameEventMgr.AddHandler(Caster, GamePlayerEvent.Dying, new DOLEventHandler(EventAction));
            }
			m_caster.Mana -= PowerCost(target);
			base.FinishSpellCast(target);
		}
        public override bool CancelPulsingSpell(GameLiving living, eSpellType spellType)
        {
            lock (living.effectListComponent.ConcentrationEffectsLock)
            {
                for (int i = 0; i < living.effectListComponent.ConcentrationEffects.Count; i++)
                {
					PulsingSpellEffect effect = null; //living.ConcentrationEffects[i] as PulsingSpellEffect;
                    if (effect == null)
                        continue;
                    if (effect.SpellHandler.Spell.SpellType == spellType)
                    {
                        effect.Cancel(false);
                        GameEventMgr.RemoveHandler(Caster, GamePlayerEvent.Moving, new DOLEventHandler(EventAction));
                        GameEventMgr.RemoveHandler(Caster, GamePlayerEvent.Dying, new DOLEventHandler(EventAction));
                        return true;
                    }
                }
            }
            return false;
        }
        public void EventAction(DOLEvent e, object sender, EventArgs args)
        {
            GameLiving player = sender as GameLiving;

            if (player == null) return;
            if (Spell.Pulse != 0 && CancelPulsingSpell(Caster, Spell.SpellType))
            {
                MessageToCaster("You cancel your effect.", eChatType.CT_Spell);
                return;
            }
        }

		#region LOS on Keeps

		private const string LOSEFFECTIVENESS = "LOS Effectivness";

		/// <summary>
		/// execute direct effect
		/// </summary>
		/// <param name="target">target that gets the damage</param>
		/// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			if (target == null)
				return;

			if (Spell.Target.ToLower() == "cone" || (Spell.Target == "Enemy" && Spell.IsPBAoE))
			{
				GamePlayer player = null;
				if (target is GamePlayer)
					player = target as GamePlayer;
				else
				{
					if (Caster is GamePlayer)
						player = Caster as GamePlayer;
					else if (Caster is GameNPC && (Caster as GameNPC).Brain is IControlledBrain)
					{
						IControlledBrain brain = (Caster as GameNPC).Brain as IControlledBrain;
						player = brain.GetPlayerOwner();
					}
				}
				if (player != null)
				{
					player.TempProperties.SetProperty(LOSEFFECTIVENESS, effectiveness);
					player.Out.SendCheckLOS(Caster, target, new CheckLOSResponse(DealDamageCheckLOS));
				}
				else
					DealDamage(target, effectiveness);
			}
			else DealDamage(target, effectiveness);
		}

		private void DealDamageCheckLOS(GamePlayer player, ushort response, ushort targetOID)
		{
			if (player == null) // Hmm
				return;
			if ((response & 0x100) == 0x100)
			{
				try
				{
					GameLiving target = Caster.CurrentRegion.GetObject(targetOID) as GameLiving;
					if (target != null)
					{
						double effectiveness = player.TempProperties.GetProperty(LOSEFFECTIVENESS, 0.0);
						DealDamage(target, effectiveness);
					}
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error(string.Format("targetOID:{0} caster:{1} exception:{2}", targetOID, Caster, e));
				}
			}
		}

		private void DealDamage(GameLiving target, double effectiveness)
		{
			if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active) return;

			// calc damage
			AttackData ad = CalculateDamageToTarget(target, effectiveness);
			DamageTarget(ad, true);
			SendDamageMessages(ad);
			target.StartInterruptTimer(target.SpellInterruptDuration, ad.AttackType, Caster);
		}
		/*
		 * We need to send resist spell los check packets because spell resist is calculated first, and
		 * so you could be inside keep and resist the spell and be interupted when not in view
		 */
		protected override void OnSpellResisted(GameLiving target)
		{
			if (target is GamePlayer && Caster.TempProperties.GetProperty("player_in_keep_property", false))
			{
				GamePlayer player = target as GamePlayer;
				player.Out.SendCheckLOS(Caster, player, new CheckLOSResponse(ResistSpellCheckLOS));
			}
			else SpellResisted(target);
		}

		private void ResistSpellCheckLOS(GamePlayer player, ushort response, ushort targetOID)
		{
			if ((response & 0x100) == 0x100)
			{
				try
				{
					GameLiving target = Caster.CurrentRegion.GetObject(targetOID) as GameLiving;
					if (target != null)
						SpellResisted(target);
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error(string.Format("targetOID:{0} caster:{1} exception:{2}", targetOID, Caster, e));
				}
			}
		}

		private void SpellResisted(GameLiving target)
		{
			base.OnSpellResisted(target);
		}
		#endregion

		// constructor
        public BainsheePulseDmgSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
}
