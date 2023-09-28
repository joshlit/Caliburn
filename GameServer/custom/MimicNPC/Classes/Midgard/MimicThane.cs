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
	public class MimicThane : MimicNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MimicThane(byte level) : base(new ClassThane(), level)
		{
			MimicSpec = new ThaneSpec();

			DistributeSkillPoints();
			MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, false, 0, eHand.oneHand);
			MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, false, 0, eHand.twoHand);
			MimicEquipment.SetShield(this, 2);
			//SetRangedWeapon(eObjectType.Fired);
			MimicEquipment.SetArmor(this, eObjectType.Chain);
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
			SetSpells();
		}
	}

	public class ThaneSpec : MimicSpec
	{
		public ThaneSpec()
		{
            SpecName = "ThaneSpec";

            int randBaseWeap = Util.Random(2);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Sword"; break;
                case 1: WeaponTypeOne = "Axe"; break;
                case 2: WeaponTypeOne = "Hammer"; break;
            }

            int randVariance = Util.Random(3);

			switch (randVariance)
			{
				case 0:
				case 1:
                Add(WeaponTypeOne, 39, 0.8f);
                Add("Stormcalling", 50, 1.0f);
                Add("Shields", 42, 0.5f);
                Add("Parry", 6, 0.0f);
				break;

                case 2:
                Add(WeaponTypeOne, 44, 0.8f);
                Add("Stormcalling", 48, 1.0f);
                Add("Shields", 35, 0.5f);
                Add("Parry", 18, 0.0f);
                break;

                case 3:
                Add(WeaponTypeOne, 50, 0.8f);
                Add("Stormcalling", 50, 1.0f);
                Add("Parry", 28, 0.1f);
                break;
            }
        }
	}
}