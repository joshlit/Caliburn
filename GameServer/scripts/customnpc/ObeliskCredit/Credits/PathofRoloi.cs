using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Path of Roloi.
	/// </summary>
	public class PathofRoloi : BaseObeliskCredit
    {
		public PathofRoloi(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public PathofRoloi(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Path of Roloi"; }
		}
	}
}
