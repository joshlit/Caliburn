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
	public class MimicWarrior : MimicNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MimicWarrior(GameLiving owner, byte level = 0, Point3D position = null) : base(owner, new ClassWarrior(), level, position)
		{
			MimicSpec = new WarriorSpec();

			DistributeSkillPoints();
			MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, false, 0, eHand.oneHand);
			MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, false, 0, eHand.twoHand);
			MimicEquipment.SetShield(this, 3);
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
		}
	}

	public class WarriorSpec : MimicSpec
	{
		public WarriorSpec()
		{
            SpecName = "WarriorSpec";

            int randBaseWeap = Util.Random(2);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Sword"; break;
                case 1: WeaponTypeOne = "Axe"; break;
                case 2: WeaponTypeOne = "Hammer"; break;
            }

            int randVariance = Util.Random(1);

			switch (randVariance)
			{
				case 0:
                Add(WeaponTypeOne, 50, 0.8f);
                Add("Shields", 42, 0.5f);
                Add("Parry", 39, 0.2f);
				break;

                case 1:
                Add(WeaponTypeOne, 50, 0.8f);
                Add("Shields", 50, 0.5f);
                Add("Parry", 28, 0.2f);
                break;
            }
        }
	}
}