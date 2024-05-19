using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Forge of Pyrkagia.
	/// </summary>
	public class ForgeofPyrkagia : BaseObeliskCredit
    {
		public ForgeofPyrkagia(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public ForgeofPyrkagia(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Forge of Pyrkagia"; }
		}
	}
}
