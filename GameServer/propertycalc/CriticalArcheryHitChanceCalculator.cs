using System;
using DOL.AI.Brain;
using DOL.GS.Scripts;

namespace DOL.GS.PropertyCalc
{
    /// <summary>
    /// The critical hit chance calculator. Returns 0 .. 100 chance.
    /// 
    /// BuffBonusCategory1 unused
    /// BuffBonusCategory2 unused
    /// BuffBonusCategory3 unused
    /// BuffBonusCategory4 for uncapped realm ability bonus
    /// BuffBonusMultCategory1 unused
    /// </summary>
    [PropertyCalculator(eProperty.CriticalArcheryHitChance)]
    public class CriticalArcheryHitChanceCalculator : PropertyCalculator
    {
        public override int CalcValue(GameLiving living, eProperty property)
        {
            int chance = living.OtherBonus[(int) property] + living.AbilityBonus[(int) property];

            if (living is GamePlayer)
                chance += 10;
            else if (ServerProperties.Properties.EXPAND_WILD_MINION &&
                living is GameNPC npc &&
                npc.Brain is IControlledBrain brain &&
                brain.GetPlayerOwner() is IGamePlayer playerOwner)
            {
                if (npc is NecromancerPet)
                    chance += 10;

                if (playerOwner.GetAbility<RealmAbilities.AtlasOF_WildMinionAbility>() is { } wildMinionAbility)
                    chance += wildMinionAbility.Amount;
            }

            // 50% hardcap.
            return Math.Min(chance, 50);
        }
    }
}
