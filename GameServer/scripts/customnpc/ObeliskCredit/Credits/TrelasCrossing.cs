using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Trela's Crossing.
	/// </summary>
	public class TrelasCrossing : BaseObeliskCredit
    {
		public TrelasCrossing(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public TrelasCrossing(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Trela's Crossing"; }
		}
	}
}
