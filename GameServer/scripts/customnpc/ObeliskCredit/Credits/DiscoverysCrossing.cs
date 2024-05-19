using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Discovery's Crossing.
	/// </summary>
	public class DiscoverysCrossing : BaseObeliskCredit
    {
		public DiscoverysCrossing(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public DiscoverysCrossing(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Discovery's Crossing"; }
		}
	}
}
