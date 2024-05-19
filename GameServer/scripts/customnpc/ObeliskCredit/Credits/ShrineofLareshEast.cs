using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Shrine of Laresh (East).
	/// </summary>
	public class ShrineofLareshEast : BaseObeliskCredit
    {
		public ShrineofLareshEast(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public ShrineofLareshEast(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Shrine of Laresh (East)"; }
		}
	}
}
