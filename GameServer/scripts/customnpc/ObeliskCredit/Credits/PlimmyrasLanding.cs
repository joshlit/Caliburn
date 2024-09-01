using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Plimmyra's Landing.
	/// </summary>
	public class PlimmyrasLanding : BaseObeliskCredit
    {
		public PlimmyrasLanding(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public PlimmyrasLanding(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Plimmyra's Landing"; }
		}
	}
}
