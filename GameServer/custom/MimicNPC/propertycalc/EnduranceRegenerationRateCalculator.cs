/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */

using DOL.GS.PropertyCalc;
using DOL.GS.RealmAbilities;

namespace DOL.GS.Scripts
{
    /// <summary>
	/// The health regen rate calculator
	///
	/// BuffBonusCategory1 is used for all buffs
	/// BuffBonusCategory2 is used for all debuffs (positive values expected here)
	/// BuffBonusCategory3 unused
	/// BuffBonusCategory4 unused
	/// BuffBonusMultCategory1 unused
	/// </summary>
	[PropertyCalculator(eProperty.EnduranceRegenerationRate)]
    public class EnduranceRegenerationRateCalculator : PropertyCalculator
    {
        public EnduranceRegenerationRateCalculator()
        { }

        /// <summary>
        /// calculates the final property value
        /// </summary>
        /// <param name="living"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public override int CalcValue(GameLiving living, eProperty property)
        {
            // buffs allow to regenerate endurance even in combat and while moving
            double regen = living.BaseBuffBonusCategory[(int)property] + living.ItemBonus[(int)property];

            if (living is GamePlayer player)
            {
                if (player.IsSitting)
                    regen += 10;
                else if (!player.IsAttacking && !player.IsMoving && !player.IsSprinting)
                    regen += 2;
            }
            else if (living is MimicNPC mimic)
            {
                if (mimic.IsSitting)
                    regen += 10;
                else if(!mimic.IsAttacking && !mimic.IsMoving && !mimic.IsSprinting)
                    regen += 2;
            }

            var raTireless = living.GetAbility<AtlasOF_RAEndRegenEnhancer>();

            if (raTireless != null)
                regen++;

            int debuff = living.SpecBuffBonusCategory[(int)property];

            if (debuff < 0)
                debuff = -debuff;

            regen -= debuff;

            if (regen < 0)
                regen = 0;

            if (regen != 0 && ServerProperties.Properties.ENDURANCE_REGEN_RATE != 1)
                regen *= ServerProperties.Properties.ENDURANCE_REGEN_RATE;

            double decimals = regen - (int)regen;

            if (Util.ChanceDouble(decimals))
                regen += 1; // compensate int rounding error

            return (int)regen;
        }
    }
}