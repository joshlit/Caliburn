using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Kainotomia's Crossing.
	/// </summary>
	public class KainotomiasCrossing : BaseObeliskCredit
    {
		public KainotomiasCrossing(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public KainotomiasCrossing(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Kainotomia's Crossing"; }
		}
	}
}
