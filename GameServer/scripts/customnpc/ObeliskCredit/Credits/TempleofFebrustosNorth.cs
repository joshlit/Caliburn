using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Temple of Februstos (North).
	/// </summary>
	public class TempleofFebrustosNorth : BaseObeliskCredit
    {
		public TempleofFebrustosNorth(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public TempleofFebrustosNorth(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Temple of Februstos (North)"; }
		}
	}
}
