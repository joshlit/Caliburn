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
using DOL.GS.Realm;
using System.Collections.Generic;

namespace DOL.GS.PlayerClass
{
	[CharacterClass((int)eCharacterClass.Paladin, "Paladin", "Fighter")]
	public class ClassPaladin : ClassFighter
	{
		private static readonly string[] AutotrainableSkills = new[] { Specs.Slash, Specs.Chants };

		public ClassPaladin()
			: base()
		{
			m_profession = "PlayerClass.Profession.ChurchofAlbion";
			m_specializationMultiplier = 25; //atlas increased from 20
			m_primaryStat = eStat.CON;
			m_secondaryStat = eStat.PIE;
			m_tertiaryStat = eStat.STR;
			m_manaStat = eStat.PIE;
			m_wsbase = 380;
			m_baseHP = 760;
		}

		public override eClassType ClassType
		{
			get { return eClassType.Hybrid; }
		}

		public override IList<string> GetAutotrainableSkills()
		{
			return AutotrainableSkills;
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}

		public override ushort MaxPulsingSpells
		{
			get { return 2; } //atlas reduced to 1
		}

		public override List<PlayerRace> EligibleRaces => new List<PlayerRace>()
		{
			 PlayerRace.Avalonian, PlayerRace.Briton, PlayerRace.Highlander, PlayerRace.Saracen,
		};
	}
}
