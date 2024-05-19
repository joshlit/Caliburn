using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Ergaleio's Path.
	/// </summary>
	public class ErgaleiosPath : BaseObeliskCredit
    {
		public ErgaleiosPath(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public ErgaleiosPath(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Ergaleio's Path"; }
		}
	}
}
