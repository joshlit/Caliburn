using DOL.Database;
using DOL.GS;

namespace Myrddin.Labyrinth.Quest.ObeliskCredit
{
	/// <summary>
	/// Discovery credit for Construct Assembly Room.
	/// </summary>
	public class ConstructAssemblyRoom : BaseObeliskCredit
    {
		public ConstructAssemblyRoom(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public ConstructAssemblyRoom(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Name of the discovery quest.
		/// </summary>
		public override string Name
		{
			get { return "Construct Assembly Room"; }
		}
	}
}
