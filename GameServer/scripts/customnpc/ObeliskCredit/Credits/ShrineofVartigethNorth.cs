using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Shrine of Vartigeth (North).
	/// </summary>
	public class ShrineofVartigethNorth : BaseObeliskCredit
    {
		public ShrineofVartigethNorth(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public ShrineofVartigethNorth(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Shrine of Vartigeth (North)"; }
		}
	}
}
