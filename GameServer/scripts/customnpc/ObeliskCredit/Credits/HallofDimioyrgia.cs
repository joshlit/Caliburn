using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Hall of Dimioyrgia.
	/// </summary>
	public class HallofDimioyrgia : BaseObeliskCredit
    {
		public HallofDimioyrgia(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public HallofDimioyrgia(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Hall of Dimioyrgia"; }
		}
	}
}
