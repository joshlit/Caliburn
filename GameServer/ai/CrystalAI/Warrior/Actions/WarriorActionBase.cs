using System;
using Crystal;
using DOL.GS;

public class WarriorActionBase : ActionBase<WarriorContext>
{
    protected GameLiving GetNearestLiving(CompanionContextBase context)
    {
        int dist = -1;
        GameLiving nearest = null;
        foreach (var player in context.Body.GetPlayersInRadius((ushort)context.DISTANCE_TO_CHECK))
        {
            if (player == context.Body) continue;
            var distTo = context.Body.GetDistanceTo(player);
            if (dist == -1 || distTo < dist)
            {
                dist = distTo;
                nearest = player;
            }
        }
            
        foreach (var gameNpc in context.Body.GetNPCsInRadius((ushort)context.DISTANCE_TO_CHECK))
        {
            if (gameNpc == context.Body) continue;
            var distTo = context.Body.GetDistanceTo(gameNpc);
            if (dist == -1 || distTo < dist)
            {
                dist = distTo;
                nearest = gameNpc;
            }
        }

        return nearest;
    }

    protected GameLiving DeterminePriorityDefensiveTarget(WarriorContext warrior)
    {
        return warrior.PlayerOwner;
    }

    protected bool CanSwitchDefensiveTarget(WarriorContext warrior)
    {
        if (warrior.LastTargetChangeTick + (warrior.MINIMUM_TARGET_PROTECTION_TIME * 1000) > GameLoop.GameLoopTime) return false;
        return true;
    }

    public WarriorActionBase() {
    }

    WarriorActionBase(WarriorActionBase other) : base(other) {
    }

    public WarriorActionBase(string name, IActionCollection collection) : base(name, collection) {
    }
}