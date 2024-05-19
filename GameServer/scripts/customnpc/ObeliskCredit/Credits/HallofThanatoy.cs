using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Hall of Thanatoy.
	/// </summary>
	public class HallofThanatoy : BaseObeliskCredit
    {
		public HallofThanatoy(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public HallofThanatoy(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Hall of Thanatoy"; }
		}
	}
}
