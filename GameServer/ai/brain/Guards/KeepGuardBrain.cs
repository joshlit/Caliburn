using DOL.GS;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;

namespace DOL.AI.Brain
{
	/// <summary>
	/// Brain Class for Area Capture Guards
	/// </summary>
	public class KeepGuardBrain : StandardMobBrain
	{
		protected GameKeepGuard _keepGuardBody;

		public override GameNPC Body
		{
			get => _keepGuardBody;
			set => _keepGuardBody = value is GameKeepGuard gameKeepGuard ? gameKeepGuard : new GameKeepGuard(); // Dummy object to avoid errors caused by bad DB entries
		}

		public override int ThinkInterval => 500;

		/// <summary>
		/// Constructor for the Brain setting default values
		/// </summary>
		public KeepGuardBrain() : base()
		{
			AggroLevel = 90;
			AggroRange = 1000;
		}

		public void SetAggression(int aggroLevel, int aggroRange)
		{
			AggroLevel = aggroLevel;
			AggroRange = aggroRange;
		}

		public override bool CheckProximityAggro()
		{
			if (Body is GuardArcher or GuardStaticArcher or GuardLord)
			{
				GameObject target = Body.TargetObject;

				// Ranged guards check LoS constantly
				if (target != null)
				{
					GamePlayer losChecker = null;

					if (target is GameNPC && (target as GameNPC).Brain is IControlledBrain)
						losChecker = ((target as GameNPC).Brain as IControlledBrain).GetPlayerOwner();
					else if (target is GamePlayer)
						losChecker = target as GamePlayer;

					if (losChecker != null)
						losChecker.Out.SendCheckLOS(Body, target, new CheckLOSResponse(LosCheckInCombatCallback));
				}

				// Drop aggro and disengage if the target is out of range
				if (Body.IsAttacking && !Body.IsWithinRadius(target, AggroRange, false))
				{
					FSM.SetCurrentState(eFSMStateType.RETURN_TO_SPAWN);

					if (target is GameLiving livingTarget && livingTarget != null)
						RemoveFromAggroList(livingTarget);
				}

				if (Body.attackComponent.AttackState && _keepGuardBody.CanUseRanged)
					Body.SwitchToRanged(target);
			}

			return base.CheckProximityAggro();
		}

		public override bool IsBeyondTetherRange()
		{
			// Eden - Portal Keeps Guards max distance
			if (Body.Level > 200 && !Body.IsWithinRadius(Body.SpawnPoint, 2000))
				return true;
			else if (!Body.InCombat && !Body.IsWithinRadius(Body.SpawnPoint, 6000))
				return true;

			return false;
		}

		private void LosCheckInCombatCallback(GamePlayer player, ushort response, ushort targetOID)
		{
			if (targetOID == 0)
				return;

			if ((response & 0x100) != 0x100)
			{
				GameObject gameObject = Body.CurrentRegion.GetObject(targetOID);

				if (gameObject is GameLiving gameLiving)
				{
					FSM.SetCurrentState(eFSMStateType.RETURN_TO_SPAWN);
					RemoveFromAggroList(gameLiving);
				}
			}
		}
	}
}
