using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Shrine of Nethuni (South).
	/// </summary>
	public class ShrineofNethuniSouth : BaseObeliskCredit
    {
		public ShrineofNethuniSouth(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public ShrineofNethuniSouth(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Shrine of Nethuni (South)"; }
		}
	}
}
