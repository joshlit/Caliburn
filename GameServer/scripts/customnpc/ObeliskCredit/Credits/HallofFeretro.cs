using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Hall of Feretro.
	/// </summary>
	public class HallofFeretro : BaseObeliskCredit
    {
		public HallofFeretro(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public HallofFeretro(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Hall of Feretro"; }
		}
	}
}
