using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Anapaysi's Crossing.
	/// </summary>
	public class AnapaysisCrossing : BaseObeliskCredit
    {
		public AnapaysisCrossing(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public AnapaysisCrossing(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Anapaysi's Crossing"; }
		}
	}
}
