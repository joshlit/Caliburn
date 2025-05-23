using System;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Styles;
using DOL.Language;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Base for all Realm Abilities
	/// </summary>
	public class RealmAbility : Ability
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public RealmAbility(DbAbility ability, int level) : base(ability, level) { }

		public virtual int CostForUpgrade(int currentLevel)
		{
			return 1000;
		}

		/// <summary>
		/// true if player can immediately use that ability
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public virtual bool CheckRequirement(GamePlayer player)
		{
			return true;
		}

		/// <summary>
		/// max level this RA can reach
		/// </summary>
		public virtual int MaxLevel
		{
			get { return 0; }
		}

		/// <summary>
		/// Delve for this RA
		/// </summary>
		public virtual void AddDelve(ref MiniDelveWriter w)
		{
			w.AddKeyValuePair("Name", m_name);
			w.AddKeyValuePair("description_string", string.Join('\n', DelveInfo));

			if (Icon > 0)
				w.AddKeyValuePair("icon", Icon);

			for (int i = 0; i <= MaxLevel - 1; i++)
			{
				if (CostForUpgrade(i) > 0)
					w.AddKeyValuePair(string.Format("TrainingCost_{0}", i + 1), CostForUpgrade(i));
			}
		}

		public override string Name
		{
			get
			{
				//Lifeflight: Right after a RA is trained the m_name already contains the roman numerals
				//So check to see if it ends with the correct RomanLevel, and if so just return m_name
				if (m_name.EndsWith(getRomanLevel()))
					return m_name;
				else
					return (Level <= 1) ? base.Name : m_name + " " + getRomanLevel();
			}
		}


		public override eSkillPage SkillType
		{
			get
			{
				return eSkillPage.RealmAbilities;
			}
		}
	}

	public class TimedRealmAbility : RealmAbility
	{

		public TimedRealmAbility(DbAbility ability, int level) : base(ability, level) { }

		public override int CostForUpgrade(int level)
		{
			if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
			{
				switch(level)
				{
					case 0: return 5;
					case 1: return 5;
					case 2: return 5;
					case 3: return 7;
					case 4: return 8;
					default: return 1000;
				}
			}
			else
			{
				return (level + 1) * 5;
			}

			
		}

		public override bool CheckRequirement(GamePlayer player)
		{
			return true;
		}

		public virtual int GetReUseDelay(int level)
		{
			return 0;
		}

		protected string FormatTimespan(int seconds)
		{
			if (seconds >= 60)
			{
				return String.Format("{0:00}:{1:00} min", seconds / 60, seconds % 60);
			}
			else
			{
				return seconds + " sec";
			}
		}

		public virtual void AddReUseDelayInfo(IList<string> list)
		{
			for (int i = 1; i <= MaxLevel; i++)
			{
				int reUseTime = GetReUseDelay(i);
				list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "RealmAbility.AddReUseDelayInfo.Every", i, ((reUseTime == 0) ? "always" : FormatTimespan(reUseTime))));
			}
		}

		public virtual void AddEffectsInfo(IList<string> list)
		{
		}

		public override int MaxLevel
		{
			get
			{
				if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
				{
					return 5;
				}
				else
				{
					return 3;
				}
			}
		}

		public override void AddDelve(ref MiniDelveWriter w)
		{
			base.AddDelve(ref w);

			for (int i = 1; i <= MaxLevel; i++)
			{
				w.AddKeyValuePair(string.Format("ReuseTimer_{0}", i), GetReUseDelay(i));
			}
		}

		public virtual void DisableSkill(GameLiving living)
		{
			living.DisableSkill(this, GetReUseDelay(Level) * 1000);
		}

		public override IList<string> DelveInfo
		{
			get
			{
				IList<string> list = base.DelveInfo;
				int size = list.Count;
				AddEffectsInfo(list);

				if (list.Count > size)
				{ // something was added
					list.Insert(size, "");	// add empty line
				}

				size = list.Count;
				AddReUseDelayInfo(list);
				if (list.Count > size)
				{ // something was added
					list.Insert(size, "");	// add empty line
				}

				return list;
			}
		}


		/// <summary>
		/// Send spell effect animation on caster and send messages
		/// </summary>
		/// <param name="caster"></param>
		/// <param name="spellEffect"></param>
		/// <param name="success"></param>
		public virtual void SendCasterSpellEffectAndCastMessage(GameLiving caster, ushort spellEffect, bool success)
		{
			foreach (GamePlayer player in caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendSpellEffectAnimation(caster, caster, spellEffect, 0, false, success ? (byte)1 : (byte)0);

				if ( caster.IsWithinRadius( player, WorldMgr.INFO_DISTANCE ) )
				{
					if (player == caster)
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "RealmAbility.SendCasterSpellEffectAndCastMessage.You", m_name), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
					}
					else
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "RealmAbility.SendCasterSpellEffectAndCastMessage.Caster", caster.Name), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
					}
				}
			}
		}
		
		public virtual void SendCasterSpellEffect(GameLiving caster, ushort spellEffect, bool success)
		{
			foreach (GamePlayer player in caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendSpellEffectAnimation(caster, caster, spellEffect, 0, false, success ? (byte)1 : (byte)0);
			}
		}


		/// <summary>
		/// Sends cast message to environment
		/// </summary>
		protected virtual void SendCastMessage(GameLiving caster)
		{
			foreach (GamePlayer player in caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				if ( caster.IsWithinRadius( player, WorldMgr.INFO_DISTANCE ) )
				{
					if (player == caster)
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "RealmAbility.SendCastMessage.YouCast", m_name), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
					}
					else
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "RealmAbility.SendCastMessage.PlayerCasts", player.Name), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
					}
				}
			}
		}

		/// <summary>
		/// Checks for any of the given conditions and returns true if there was any
		/// prints messages
		/// </summary>
		/// <param name="living"></param>
		/// <param name="bitmask"></param>
		/// <returns></returns>
		public virtual bool CheckPreconditions(GameLiving living, long bitmask)
		{
			GamePlayer player = living as GamePlayer;
			if ((bitmask & DEAD) != 0 && !living.IsAlive)
			{
				if (player != null)
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "RealmAbility..CheckPreconditions.Dead"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				return true;
			}
			if ((bitmask & MEZZED) != 0 && living.IsMezzed)
			{
				if (player != null)
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "RealmAbility..CheckPreconditions.Mesmerized"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				return true;
			}
			if ((bitmask & STUNNED) != 0 && living.IsStunned)
			{
				if (player != null)
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "RealmAbility..CheckPreconditions.Stunned"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				return true;
			}
			if ((bitmask & SITTING) != 0 && living.IsSitting)
			{
				if (player != null)
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "RealmAbility..CheckPreconditions.Sitting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				return true;
			}
			if ((bitmask & INCOMBAT) != 0 && living.InCombat)
			{
				if (player != null)
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "RealmAbility..CheckPreconditions.Combat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				return true;
			}
			if ((bitmask & NOTINCOMBAT) != 0 && !living.InCombat)
			{
				if (player != null)
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "RealmAbility..CheckPreconditions.BeInCombat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				return true;
			}
			if ((bitmask & STEALTHED) != 0 && living.IsStealthed)
			{
				if (player != null)
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "RealmAbility..CheckPreconditions.Stealthed"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				return true;
			}
			if (player != null && (bitmask & NOTINGROUP) != 0 && player.Group == null)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "RealmAbility..CheckPreconditions.BeInGroup"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return true;
			}
			return false;
		}

		/*
		 * Stored in hex, different values in binary
		 * e.g.
		 * 16|8|4|2|1
		 * ----------
		 * 1
		 * 0|0|0|0|1 stored as 0x00000001
		 * 2
		 * 0|0|0|1|0 stored as 0x00000002
		 * 4
		 * 0|0|1|0|0 stored as 0x00000004
		 * 8
		 * 0|1|0|0|0 stored as 0x00000008
		 * 16
		 * 1|0|0|0|0 stored as 0x00000010
		 */
		public const long DEAD = 0x00000001;
		public const long SITTING = 0x00000002;
		public const long MEZZED = 0x00000004;
		public const long STUNNED = 0x00000008;
		public const long INCOMBAT = 0x00000010;
		public const long NOTINCOMBAT = 0x00000020;
		public const long NOTINGROUP = 0x00000040;
		public const long STEALTHED = 0x000000080;
		public const long TARGET = 0x000000100;
	}

	public abstract class StyleRealmAbility : TimedRealmAbility
	{
		public Style StyleToUse;

		public StyleRealmAbility(DbAbility ability, int level) : base(ability, level)
		{
			StyleToUse = CreateStyle();
		}

		public override int MaxLevel
		{
			get { return 1; }
		}

		public override int CostForUpgrade(int currentLevel)
		{
			return 10;
		}
		
		public override int GetReUseDelay(int level) { return 600; } // 10 mins

		public override void DisableSkill(GameLiving living)
		{
			StyleComponent styleComponent = living.styleComponent;

			// Remove RA styles from the backup slot so that it doesn't fire twice.
			if (styleComponent.NextCombatBackupStyle == StyleToUse)
				styleComponent.NextCombatBackupStyle = null;

			base.DisableSkill(living);
		}

		public override void Execute(GameLiving living)
		{
			StyleProcessor.TryToUseStyle(living, StyleToUse);
			base.Execute(living);
		}

		protected abstract Style CreateStyle();
	}

	public class L5RealmAbility : RealmAbility
	{
		public L5RealmAbility(DbAbility ability, int level) : base(ability, level) { }

		public override int CostForUpgrade(int level)
		{
			if (ServerProperties.Properties.USE_NEW_PASSIVES_RAS_SCALING)
			{
				switch (level)
				{
						case 0: return 1;
						case 1: return 1;
						case 2: return 2;
						case 3: return 3;
						case 4: return 3;
						case 5: return 5;
						case 6: return 5;
						case 7: return 7;
						case 8: return 7;
						default: return 1000;
				}
			}
			else
			{
				switch (level)
				{
						case 0: return 1;
						case 1: return 3;
						case 2: return 6;
						case 3: return 10;
						case 4: return 14;
						default: return 1000;
				}
			}
		}

		public override bool CheckRequirement(GamePlayer player)
		{
			if (ServerProperties.Properties.USE_NEW_PASSIVES_RAS_SCALING)
			{
				return Level <= 9;
			}
			else
			{
				return Level <= 5;
			}
		}

		public override int MaxLevel
		{
			get
			{
				if (ServerProperties.Properties.USE_NEW_PASSIVES_RAS_SCALING)
				{
					return 9;
				}
				else
				{
					return 5;
				}
			}
		}
	}

	public class RR5RealmAbility : TimedRealmAbility
	{
		public RR5RealmAbility(DbAbility ability, int level) : base(ability, level) { }

		public override int MaxLevel
		{
			get
			{
				return 1;
			}
		}

		public override bool CheckRequirement(GamePlayer player)
		{
			return player.RealmLevel >= 40;
		}

		public override int CostForUpgrade(int level)
		{
			return 0;
		}

	}

	/*
	public class L1RealmAbility : TimedRealmAbility
	{
		public L1RealmAbility(DBAbility ability, int level) : base(ability, level) { }

		public override int MaxLevel
		{
			get
			{
				return 1;
			}
		}

		public override bool CheckRequirement(GamePlayer player)
		{
			return player.RealmLevel >= 40;
		}

		public override int CostForUpgrade(int level)
		{
			return 14;
		}

	}
	*/
}