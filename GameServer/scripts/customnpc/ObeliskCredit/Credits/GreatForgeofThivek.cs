using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Great Forge of Thivek.
	/// </summary>
	public class GreatForgeofThivek : BaseObeliskCredit
    {
		public GreatForgeofThivek(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public GreatForgeofThivek(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Great Forge of Thivek"; }
		}
	}
}
