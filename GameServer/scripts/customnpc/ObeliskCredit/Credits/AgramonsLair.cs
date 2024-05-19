using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Agramon's Lair.
	/// </summary>
	public class AgramonsLair : BaseObeliskCredit
    {
		public AgramonsLair(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public AgramonsLair(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Agramon's Lair"; }
		}
	}
}
