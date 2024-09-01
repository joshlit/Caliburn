using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Shrine of Tegashirg.
	/// </summary>
	public class ShrineofTegashirg : BaseObeliskCredit
    {
		public ShrineofTegashirg(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public ShrineofTegashirg(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Shrine of Tegashirg"; }
		}
	}
}
