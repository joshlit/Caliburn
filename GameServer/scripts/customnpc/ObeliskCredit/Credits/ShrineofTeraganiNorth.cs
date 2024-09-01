using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Shrine of Teragani (North).
	/// </summary>
	public class ShrineofTeraganiNorth : BaseObeliskCredit
    {
		public ShrineofTeraganiNorth(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public ShrineofTeraganiNorth(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Shrine of Teragani (North)"; }
		}
	}
}
