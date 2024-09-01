using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Efeyresi's Junction.
	/// </summary>
	public class EfeyresisJunction : BaseObeliskCredit
    {
		public EfeyresisJunction(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public EfeyresisJunction(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Efeyresi's Junction"; }
		}
	}
}
