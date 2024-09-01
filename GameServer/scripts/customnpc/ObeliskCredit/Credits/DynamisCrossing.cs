using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Dynami's Crossing.
	/// </summary>
	public class DynamisCrossing : BaseObeliskCredit
    {
		public DynamisCrossing(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public DynamisCrossing(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Dynami's Crossing"; }
		}
	}
}
