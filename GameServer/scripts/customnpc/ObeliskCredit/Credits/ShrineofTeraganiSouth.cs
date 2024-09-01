using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Shrine of Teragani (South).
	/// </summary>
	public class ShrineofTeraganiSouth : BaseObeliskCredit
    {
		public ShrineofTeraganiSouth(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public ShrineofTeraganiSouth(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Shrine of Teragani (South)"; }
		}
	}
}
