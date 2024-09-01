using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Path of Zoi.
	/// </summary>
	public class PathofZoi : BaseObeliskCredit
    {
		public PathofZoi(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public PathofZoi(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Path of Zoi"; }
		}
	}
}
