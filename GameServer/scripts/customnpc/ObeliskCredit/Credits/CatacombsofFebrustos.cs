using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Catacombs of Februstos.
	/// </summary>
	public class CatacombsofFebrustos : BaseObeliskCredit
    {
		public CatacombsofFebrustos(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public CatacombsofFebrustos(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Catacombs of Februstos"; }
		}
	}
}
