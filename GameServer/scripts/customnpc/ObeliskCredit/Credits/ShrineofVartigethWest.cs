using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Shrine of Vartigeth (West).
	/// </summary>
	public class ShrineofVartigethWest : BaseObeliskCredit
    {
		public ShrineofVartigethWest(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public ShrineofVartigethWest(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Shrine of Vartigeth (West)"; }
		}
	}
}
