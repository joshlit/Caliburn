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
using System.Collections.Specialized;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("Resurrect")]
	public class ResurrectSpellHandler : SpellHandler
	{
		private const string RESURRECT_CASTER_PROPERTY = "RESURRECT_CASTER";
		protected readonly ListDictionary m_resTimersByLiving = new ListDictionary();

		// constructor
		public ResurrectSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}

		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= PowerCost(target);
			base.FinishSpellCast(target);
		}

		/// <summary>
		/// execute non duration spell effect on target
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			base.OnDirectEffect(target, effectiveness);
			if(target == null || target.IsAlive) return;

			SendEffectAnimation(target, 0, false, 1);
			GamePlayer targetPlayer = target as GamePlayer;
			if (targetPlayer == null)
			{
				//not a player
				ResurrectLiving(target);
			}
			else
			{
				targetPlayer.TempProperties.SetProperty(RESURRECT_CASTER_PROPERTY, m_caster);
				ECSGameTimer resurrectExpiredTimer = new ECSGameTimer(targetPlayer);
				resurrectExpiredTimer.Callback = new ECSGameTimer.ECSTimerCallback(ResurrectExpiredCallback);
				resurrectExpiredTimer.Properties.SetProperty("targetPlayer", targetPlayer);
				resurrectExpiredTimer.Start(15000);
				lock (m_resTimersByLiving.SyncRoot)
				{
					m_resTimersByLiving.Add(target, resurrectExpiredTimer);
				}

				//send resurrect dialog
				targetPlayer.Out.SendCustomDialog("Do you allow " + m_caster.GetName(0, true) + " to resurrected you\n with " + m_spell.ResurrectHealth + " percent hits/power?", new CustomDialogResponse(ResurrectResponceHandler));
			}
		}

		/// <summary>
		/// Calculates the power to cast the spell
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public override int PowerCost(GameLiving target)
		{
			if (IsPerfectRecovery())
				return 0;

			float factor = Math.Max (0.1f, 0.5f + (target.Level - m_caster.Level) / (float)m_caster.Level);

			//DOLConsole.WriteLine("res power needed: " + (int) (m_caster.MaxMana * factor) + "; factor="+factor);
			return (int) (m_caster.MaxMana * factor);
		}

		/// <summary>
		/// Resurrects target if it accepts
		/// </summary>
		/// <param name="player"></param>
		/// <param name="response"></param>
		protected virtual void ResurrectResponceHandler(GamePlayer player, byte response)
		{
			//DOLConsole.WriteLine("resurrect responce: " + response);
			ECSGameTimer resurrectExpiredTimer = null;
			lock (m_resTimersByLiving.SyncRoot)
			{
				resurrectExpiredTimer = (ECSGameTimer)m_resTimersByLiving[player];
				m_resTimersByLiving.Remove(player);
			}
			if (resurrectExpiredTimer != null)
			{
				resurrectExpiredTimer.Stop();
			}

			GameLiving rezzer = player.TempProperties.GetProperty<GameLiving>(RESURRECT_CASTER_PROPERTY, null);
			if (!player.IsAlive)
			{
				if (rezzer == null)
				{
					player.Out.SendMessage("No one is currently trying to resurrect you.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					if (response == 1)
					{
						ResurrectLiving(player); //accepted
					}
					else
					{
						player.Out.SendMessage("You decline to be resurrected.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						//refund mana
						m_caster.Mana += PowerCost(player);

						// Reset PR cooldown if it wasnt accepted.
						if (IsPerfectRecovery() && Ability != null)
                        {
							AtlasOF_PerfectRecovery PRAbility = Ability as AtlasOF_PerfectRecovery;
							PRAbility.OnRezDeclined(Caster as GamePlayer);
						}
					}
				}
			}
			player.TempProperties.RemoveProperty(RESURRECT_CASTER_PROPERTY);
		}

		/// <summary>
		/// Resurrects living
		/// </summary>
		/// <param name="living"></param>
		protected virtual void ResurrectLiving(GameLiving living)
		{
			if (m_caster.ObjectState != GameObject.eObjectState.Active) return;
			if (m_caster.CurrentRegionID != living.CurrentRegionID) return;

            GamePlayer player = living as GamePlayer;
			if (player != null)
            {
				// TempProperty is used to either halve (for high level spec rez) or remove (PR) rez sick.
				// That's done either in GamePlayer.OnRevive() or in the rez sick spell handler if Effectiveness > 0;
				double rezSickEffectiveness = 1;

				// Must check PR first since PR also has ResurrectHealth == 100 but should not apply rez sick.
				if (IsPerfectRecovery())
                {
                    rezSickEffectiveness = 0;
                }
                else if (Spell.ResurrectHealth == 100)
                {
					//Patch 1.56: Resurrection sickness now goes from 100 % to 50 % when doing a "full rez" on another player.
					rezSickEffectiveness = 0.5;
				}

                player.TempProperties.SetProperty(GamePlayer.RESURRECT_REZ_SICK_EFFECTIVENESS, rezSickEffectiveness);

                player.Notify(GamePlayerEvent.Revive, player, new RevivedEventArgs(Caster, Spell));
            }

			living.Health = living.MaxHealth * m_spell.ResurrectHealth / 100;
			double tempManaEnd = m_spell.ResurrectMana / 100.0;
			living.Mana = (int)(living.MaxMana * tempManaEnd);

            //The spec rez spells are the only ones that have endurance
            if (!SpellLine.IsBaseLine)
				living.Endurance = (int)(living.MaxEndurance * tempManaEnd);
			else
				living.Endurance = 0;

			living.MoveTo(m_caster.CurrentRegionID, m_caster.X, m_caster.Y, m_caster.Z, m_caster.Heading);

			ECSGameTimer resurrectExpiredTimer = null;
			lock (m_resTimersByLiving.SyncRoot)
			{
				resurrectExpiredTimer = (ECSGameTimer)m_resTimersByLiving[living];
				m_resTimersByLiving.Remove(living);
			}
			if (resurrectExpiredTimer != null)
			{
				resurrectExpiredTimer.Stop();
			}
		
			if (player != null)
			{
				player.StopReleaseTimer();
				player.Out.SendPlayerRevive(player);
				player.UpdatePlayerStatus();
				player.Out.SendMessage("You have been resurrected by " + m_caster.GetName(0, false) + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				//player.Notify(GamePlayerEvent.Revive, player, new RevivedEventArgs(Caster, Spell));

				//Lifeflight add this should make it so players who have been ressurected don't take damage for 5 seconds
				RezDmgImmunityEffect rezImmune = new RezDmgImmunityEffect();
                rezImmune.Start(player);

				IList<GameObject> attackers;
				lock (player.attackComponent.Attackers) { attackers = new List<GameObject>(player.attackComponent.Attackers); }

				foreach (GameObject attacker in attackers)
				{
					if (attacker is GameLiving && attacker != living.TargetObject)
						attacker.Notify(
							GameLivingEvent.EnemyHealed,
							attacker,
							new EnemyHealedEventArgs(living, m_caster, eHealthChangeType.Spell, living.Health));
				}

				GamePlayer casterPlayer = Caster as GamePlayer;
				if (casterPlayer != null)
				{
					long rezRps = player.LastDeathRealmPoints * (Spell.ResurrectHealth + 50) / 1000;
					if (rezRps > 0)
					{
						casterPlayer.GainRealmPoints(rezRps);
					}
					else
					{
						casterPlayer.Out.SendMessage("The player you resurrected was not worth realm points on death.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						casterPlayer.Out.SendMessage("You thus get no realm points for the resurrect.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					}
				}
			}
		}

		/// <summary>
		/// Cancels resurrection after some time
		/// </summary>
		/// <param name="callingTimer"></param>
		/// <returns></returns>
		protected virtual int ResurrectExpiredCallback(ECSGameTimer callingTimer)
		{
			GamePlayer player = callingTimer.Properties.GetProperty<GamePlayer>("targetPlayer", null);
			if (player == null) return 0;
			player.TempProperties.RemoveProperty(RESURRECT_CASTER_PROPERTY);
			player.Out.SendMessage("Your resurrection spell has expired.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return 0;
		}

		/// <summary>
		/// All checks before any casting begins
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public override bool CheckBeginCast(GameLiving target)
		{
			if (!base.CheckBeginCast(target))
				return false;

            //Lifeflight, the base call to Checkbegincast uses its own power check, which is bad for rez spells
            //so I added another check here.
            if (m_caster.Mana < PowerCost(target))
            {
                MessageToCaster("You don't have enough power to cast that!", eChatType.CT_SpellResisted);
				return false;
            }

			GameLiving resurrectionCaster = target.TempProperties.GetProperty<GameLiving>(RESURRECT_CASTER_PROPERTY, null);
			if (resurrectionCaster != null)
			{
				//already considering resurrection - do nothing
				MessageToCaster("Your target is already considering a resurrection!", eChatType.CT_SpellResisted);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Checks after casting before spell is executed
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public override bool CheckEndCast(GameLiving target)
		{
			GameLiving resurrectionCaster = target.TempProperties.GetProperty<GameLiving>(RESURRECT_CASTER_PROPERTY, null);
			if (resurrectionCaster != null)
			{
				//already considering resurrection - do nothing
				MessageToCaster("Your target is already considering a resurrection!", eChatType.CT_SpellResisted);
				return false;
			}
			return base.CheckEndCast(target);
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList<string> DelveInfo 
		{
			get 
			{
				/*
				<Begin Info: Revive>
				Function: raise dead
 
				Brings target back to life, restores a portion of their health 
				and power and eliminates the experience penalty and con loss they 
				would have suffered were they to have /release.
 
				Health restored: 10
				Target: Dead
				Range: 1500
				Casting time: 4.0 sec
 
				<End Info>
				*/

				var list = new List<string>();

				list.Add("Function: " + (Spell.SpellType.ToString() == "" ? "(not implemented)" : Spell.SpellType.ToString()));
				list.Add(" "); //empty line
				list.Add(Spell.Description);
				list.Add(" "); //empty line
				list.Add("Health restored: " + Spell.ResurrectHealth);
				if(Spell.ResurrectMana != 0) list.Add("Power restored: " + Spell.ResurrectMana);
				list.Add("Target: " + Spell.Target);
				if (Spell.Range != 0) list.Add("Range: " + Spell.Range);
				list.Add("Casting time: " + (Spell.CastTime*0.001).ToString("0.0## sec;-0.0## sec;'instant'"));
			
				return list;
			}
		}

		private bool IsPerfectRecovery() { return SpellLine.Name == "RealmAbilities"; }
	}
}
