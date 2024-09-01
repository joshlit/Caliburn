using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Ygrasia's Crossing.
	/// </summary>
	public class YgrasiasCrossing : BaseObeliskCredit
    {
		public YgrasiasCrossing(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public YgrasiasCrossing(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Ygrasia's Crossing"; }
		}
	}
}
