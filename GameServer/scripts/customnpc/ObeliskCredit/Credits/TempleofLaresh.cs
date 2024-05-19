using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Temple of Laresh.
	/// </summary>
	public class TempleofLaresh : BaseObeliskCredit
    {
		public TempleofLaresh(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public TempleofLaresh(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Temple of Laresh"; }
		}
	}
}
