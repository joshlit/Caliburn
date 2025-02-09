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

namespace DOL.GS.Relics
{
	/// <summary>
	/// Class representing a relic pillar.
	/// </summary>
	/// <author>Aredhel</author>
	public class RelicPillar : GameDoorBase
	{
		/// <summary>
		/// Creates a new relic pillar.
		/// </summary>
		public RelicPillar() : base()
		{
			Realm = 0;
			Close();
		}

		/// <summary>
		/// Object used for thread synchronization.
		/// </summary>
		private object m_syncPillar = new object();

		private int m_pillarID;

		/// <summary>
		/// ID for this pillar.
		/// </summary>
		public override int DoorID
		{
			get { return m_pillarID; }
			set { m_pillarID = value; }
		}

		/// <summary>
		/// Pillars behave like regular doors.
		/// </summary>
		public override uint Flag
		{
			get { return 0; }
			set { }
		}

		private eDoorState m_pillarState;

		/// <summary>
		/// State of this pillar (up == closed, down == open).
		/// </summary>
		public override eDoorState State
		{
			get { return m_pillarState; }
			set
			{
				if (m_pillarState != value)
				{
					lock (m_syncPillar)
					{
						m_pillarState = value;

						foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
							PlayerService.UpdateObjectForPlayer(player, this);
					}
				}
			}
		}

		/// <summary>
		/// Make the pillar start moving down.
		/// </summary>
		public override void Open(GameLiving opener = null)
		{
			State = eDoorState.Open;
		}

		/// <summary>
		/// Reset pillar.
		/// </summary>
		public override void Close(GameLiving closer = null)
		{
			State = eDoorState.Closed;
		}

		/// <summary>
		/// NPCs cannot make pillars move.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="open"></param>
		public override void NPCManipulateDoorRequest(GameNPC npc, bool open)
		{
		}
	}
}
