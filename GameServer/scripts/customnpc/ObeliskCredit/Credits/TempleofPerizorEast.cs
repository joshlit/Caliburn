using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Temple of Perizor (East).
	/// </summary>
	public class TempleofPerizorEast : BaseObeliskCredit
    {
		public TempleofPerizorEast(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public TempleofPerizorEast(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Temple of Perizor (East)"; }
		}
	}
}
