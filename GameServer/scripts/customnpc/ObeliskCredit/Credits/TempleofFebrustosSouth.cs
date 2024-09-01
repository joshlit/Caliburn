using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Temple of Februstos (South).
	/// </summary>
	public class TempleofFebrustosSouth : BaseObeliskCredit
    {
		public TempleofFebrustosSouth(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public TempleofFebrustosSouth(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Temple of Februstos (South)"; }
		}
	}
}
