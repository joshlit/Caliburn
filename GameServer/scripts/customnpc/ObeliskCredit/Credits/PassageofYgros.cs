using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Passage of Ygros.
	/// </summary>
	public class PassageofYgros : BaseObeliskCredit
    {
		public PassageofYgros(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public PassageofYgros(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Passage of Ygros"; }
		}
	}
}
