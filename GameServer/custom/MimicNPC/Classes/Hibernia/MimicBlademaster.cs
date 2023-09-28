using System;
using System.Reflection;
using DOL.GS;
using DOL.GS.Scripts;
using DOL.Database;
using log4net;
using DOL.GS.Realm;
using System.Collections.Generic;
using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
	public class MimicBlademaster : MimicNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MimicBlademaster(byte level) : base(new ClassBlademaster(), level)
		{
			MimicSpec = new BlademasterSpec();

			DistributeSkillPoints();
			MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, true);
			//SetRangedWeapon(eObjectType.Fired);
			MimicEquipment.SetArmor(this, eObjectType.Reinforced);
			MimicEquipment.SetJewelry(this);

			//foreach (InventoryItem item in Inventory.EquippedItems)
			//{
			//	if (item == null)
			//		return;

			//	if (item.Quality < 90)
			//	{
			//		item.Quality = Util.Random(85, 100);
			//	}

			//	log.Debug("Name: " + item.Name);
			//	log.Debug("Slot: " + Enum.GetName(typeof(eInventorySlot), item.SlotPosition));
			//	log.Debug("DPS_AF: " + item.DPS_AF);
			//	log.Debug("SPD_ABS: " + item.SPD_ABS);
			//}

			SwitchWeapon(eActiveWeaponSlot.Standard);

			RefreshSpecDependantSkills(false);
		}
	}

	public class BlademasterSpec : MimicSpec
	{
		public BlademasterSpec()
		{
            SpecName = "BlademasterSpec";
            is2H = false;

            int randBaseWeap = Util.Random(2);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Blades"; break;
                case 1: WeaponTypeOne = "Piercing"; break;
                case 2: WeaponTypeOne = "Blunt"; break;
            }

            int randVariance = Util.Random(0);

			switch (randVariance)
			{
				case 0:
                Add(WeaponTypeOne, 50, 0.8f);
                Add("Celtic Dual", 50, 1.0f);
                Add("Parry", 28, 0.2f);
				break;
            }
        }
	}
}