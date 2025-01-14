using System;
using DOL.GS.Keeps;
using DOL.GS.Scripts;

namespace DOL.GS.PropertyCalc
{
    /// <summary>
    /// The Armor Factor calculator
    ///
    /// BuffBonusCategory1 is used for base buffs directly in player.GetArmorAF because it must be capped by item AF cap
    /// BuffBonusCategory2 is used for spec buffs, level*1.875 cap for players
    /// BuffBonusCategory3 is used for debuff, uncapped
    /// BuffBonusCategory4 is used for buffs, uncapped
    /// BuffBonusMultCategory1 unused
    /// ItemBonus is used for players TOA bonuse, living.Level cap
    /// </summary>
    [PropertyCalculator(eProperty.ArmorFactor)]
    public class ArmorFactorCalculator : PropertyCalculator
    {
        public override int CalcValue(GameLiving living, eProperty property)
        {
            switch (living)
            {
                case IGamePlayer:
                case GameTrainingDummy:
                    return CalculatePlayerArmorFactor(living, property);
                case GameKeepDoor:
                case GameKeepComponent:
                    return CalculateKeepComponentArmorFactor(living);
                case IGameEpicNpc:
                    return CalculateLivingArmorFactor(living, property, 12.0 * (living as IGameEpicNpc).ArmorFactorScalingFactor, 50.0, false);
                case NecromancerPet:
                    return CalculateLivingArmorFactor(living, property, 12.0, 121.0, true);
                case GameSummonedPet:
                    return CalculateLivingArmorFactor(living, property, 12.0, 175.0, true);
                case GuardLord:
                    return CalculateLivingArmorFactor(living, property, 12.0, 134.0, false);
                default:
                    return CalculateLivingArmorFactor(living, property, 12.0, 200.0, false);
            }

            static int CalculatePlayerArmorFactor(GameLiving living, eProperty property)
            {
                // Base AF buffs are calculated in the item's armor calc since they have the same cap.
                int armorFactor = Math.Min((int) (living.Level * 1.875), living.SpecBuffBonusCategory[(int) property]);
                armorFactor -= Math.Abs(living.DebuffCategory[(int) property]);
                armorFactor += Math.Min(living.Level, living.ItemBonus[(int) property]);
                armorFactor += living.OtherBonus[(int) property];
                return armorFactor;
            }

            static int CalculateLivingArmorFactor(GameLiving living, eProperty property, double factor, double divisor, bool useBaseBuff)
            {
                int armorFactor = (int) ((1 + living.Level / divisor) * (living.Level * factor));

                if (useBaseBuff)
                    armorFactor += living.BaseBuffBonusCategory[(int) property];

                armorFactor += living.SpecBuffBonusCategory[(int) property];
                armorFactor -= Math.Abs(living.DebuffCategory[(int) property]);
                armorFactor += living.OtherBonus[(int) property];
                return armorFactor;
            }

            static int CalculateKeepComponentArmorFactor(GameLiving living)
            {
                GameKeepComponent component = null;

                if (living is GameKeepDoor keepDoor)
                    component = keepDoor.Component;
                else if (living is GameKeepComponent)
                    component = living as GameKeepComponent;

                if (component == null)
                    return 1;

                double keepLevelMod = 1 + component.Keep.Level * 0.1;
                int typeMod = component.Keep is GameKeep ? 24 : 12;
                return (int) (component.Keep.BaseLevel * keepLevelMod * typeMod);
            }
            }
        }
    }
