using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Temple of Perizor (West).
	/// </summary>
	public class TempleofPerizorWest : BaseObeliskCredit
    {
		public TempleofPerizorWest(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public TempleofPerizorWest(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Temple of Perizor (West)"; }
		}
	}
}
