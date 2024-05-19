using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Shrine of Vartigeth (South).
	/// </summary>
	public class ShrineofVartigethSouth : BaseObeliskCredit
    {
		public ShrineofVartigethSouth(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public ShrineofVartigethSouth(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Shrine of Vartigeth (South)"; }
		}
	}
}
