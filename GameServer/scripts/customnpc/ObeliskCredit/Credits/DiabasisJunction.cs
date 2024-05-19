using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Diabasi's Junction.
	/// </summary>
	public class DiabasisJunction : BaseObeliskCredit
    {
		public DiabasisJunction(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public DiabasisJunction(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Diabasi's Junction"; }
		}
	}
}
