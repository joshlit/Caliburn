using System;
using System.Reflection;
using DOL.Database;
using DOL.GS.PacketHandler;
using log4net;
using DOL.GS.Scripts;

namespace DOL.GS
{
	/// <summary>
	/// This class represents an inventory item
	/// </summary>
	public class GuildBannerItem : GameInventoryItem
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public enum eStatus : byte
		{
			Active = 1,
			Dropped = 2,
			Recovered = 3
		}


		private Guild m_ownerGuild = null;
		private GamePlayer m_summonPlayer = null;
		private eStatus m_status = eStatus.Active;

		public GuildBannerItem()
			: base()
		{
		}

		public GuildBannerItem(DbItemTemplate template)
			: base(template)
		{
		}

		/// <summary>
		/// What guild owns this banner
		/// </summary>
		public Guild OwnerGuild
		{
			get { return m_ownerGuild; }
			set { m_ownerGuild = value; }
		}

		public GamePlayer SummonPlayer
		{
			get { return m_summonPlayer; }
			set { m_summonPlayer = value; }
		}

		public eStatus Status
		{
			get { return m_status; }
		}


		/// <summary>
		/// Player receives this item (added to players inventory)
		/// </summary>
		/// <param name="player"></param>
		public override void OnReceive(GamePlayer player)
		{
			// for guild banners we don't actually add it to inventory but instead register
			// if it is rescued by a friendly player or taken by the enemy

			player.Inventory.RemoveItem(this);

			int trophyModel = 0;
			eRealm realm = eRealm.None;

			switch (Model)
			{
				case 3223:
					trophyModel = 3359;
					realm = eRealm.Albion;
					break;
				case 3224:
					trophyModel = 3361;
					realm = eRealm.Midgard;
					break;
				case 3225:
					trophyModel = 3360;
					realm = eRealm.Hibernia;
					break;
			}

			// if picked up by an enemy then turn this into a trophy
			if (realm != player.Realm)
			{
				DbItemUnique template = new DbItemUnique(Template);
				template.ClassType = string.Empty;
				template.Model = trophyModel;
				template.IsDropable = true;
				template.IsIndestructible = false;

				GameInventoryItem trophy = new GameInventoryItem(template);
                player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, trophy);
				OwnerGuild.SendMessageToGuildMembers(player.Name + " of " + GlobalConstants.RealmToName(player.Realm) + " has captured your guild banner!", eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
				OwnerGuild.GuildBannerLostTime = DateTime.Now;
			}
			else
			{
				m_status = eStatus.Recovered;

				// A friendly player has picked up the banner.
				if (OwnerGuild != null)
				{
					OwnerGuild.SendMessageToGuildMembers(player.Name + " has recovered your guild banner!", eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
				}

				if (SummonPlayer != null)
				{
					SummonPlayer.GuildBanner = null;
				}
			}
		}

		/// <summary>
		/// Player has dropped, traded, or otherwise lost this item
		/// </summary>
		/// <param name="player"></param>
		public override void OnLose(GamePlayer player)
		{
			if (player.GuildBanner != null)
			{
				player.GuildBanner.Stop();
				m_status = eStatus.Dropped;
			}
		}



		/// <summary>
		/// Drop this item on the ground
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override WorldInventoryItem Drop(GamePlayer player)
		{
			return null;
		}


		public override void OnRemoveFromWorld()
		{
			if (Status == eStatus.Dropped)
			{
				if (SummonPlayer != null)
				{
					SummonPlayer.GuildBanner = null;
					SummonPlayer = null;
				}

				if (OwnerGuild != null)
				{
					// banner was dropped and not picked up, must be re-purchased
					OwnerGuild.GuildBanner = false;
					OwnerGuild.SendMessageToGuildMembers("Your guild banner has been lost!", eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
					OwnerGuild = null;
				}
			}

			base.OnRemoveFromWorld();
		}


		/// <summary>
		/// Is this a valid item for this player?
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool CheckValid(IGamePlayer player)
		{
			return false;
		}
	}
}
