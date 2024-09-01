using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Shrine of Nethuni (North).
	/// </summary>
	public class ShrineofNethuniNorth : BaseObeliskCredit
    {
		public ShrineofNethuniNorth(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public ShrineofNethuniNorth(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Shrine of Nethuni (North)"; }
		}
	}
}
