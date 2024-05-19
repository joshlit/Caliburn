using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Hall of Allagi.
	/// </summary>
	public class HallofAllagi : BaseObeliskCredit
    {
		public HallofAllagi(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public HallofAllagi(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Hall of Allagi"; }
		}
	}
}
