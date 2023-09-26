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
	public class MimicBerserker : MimicNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MimicBerserker(GameLiving owner, byte level = 0, Point3D position = null) : base(owner, new ClassBerserker(), level, position)
		{
			MimicSpec = new BerserkerSpec();

			DistributeSkillPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, false, 0, eHand.oneHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeTwo, false, 0, eHand.leftHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, false, 0, eHand.twoHand);

            //SetRangedWeapon(eObjectType.Fired);
            MimicEquipment.SetArmor(this, eObjectType.Studded);
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

	public class BerserkerSpec : MimicSpec
	{
		public BerserkerSpec()
		{
            SpecName = "BerserkerSpec";

            int randBaseWeap = Util.Random(2);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Sword"; break;
                case 1: WeaponTypeOne = "Axe"; break;
                case 2: WeaponTypeOne = "Hammer"; break;
            }

			WeaponTypeTwo = "Axe";

            int randVariance = Util.Random(2);

			switch (randVariance)
			{
				case 0:
				case 1:
                Add(WeaponTypeOne, 50, 0.8f);
                Add("Left Axe", 50, 1.0f);
                Add("Parry", 28, 0.2f);
				break;

                case 2:
                Add(WeaponTypeOne, 44, 0.8f);
                Add("Left Axe", 50, 1.0f);
                Add("Parry", 37, 0.2f);
                break;
            }
        }
	}
}