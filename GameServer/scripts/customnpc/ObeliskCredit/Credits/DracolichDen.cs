using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Dracolich Den.
	/// </summary>
	public class DracolichDen : BaseObeliskCredit
    {
		public DracolichDen(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public DracolichDen(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Dracolich Den"; }
		}
	}
}
