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
	public class MimicSavage : MimicNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MimicSavage(GameLiving owner, byte level = 0, Point3D position = null) : base(owner, new ClassSavage(), level, position)
		{
            MimicSpec = new SavageSpec();

			DistributeSkillPoints();

			if (MimicSpec.WeaponTypeOne == "Hand to Hand")
                MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, true);
			else
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

            if (MimicSpec.WeaponTypeOne == "Hand to Hand")
                SwitchWeapon(eActiveWeaponSlot.Standard);
            else
                SwitchWeapon(eActiveWeaponSlot.TwoHanded);

			RefreshSpecDependantSkills(false);
            SetSpells();
		}
	}

	public class SavageSpec : MimicSpec
	{
		public SavageSpec()
		{
            SpecName = "SavageSpec";

            int randBaseWeap = Util.Random(4);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Sword"; break;
                case 1: WeaponTypeOne = "Axe"; break;
                case 2: WeaponTypeOne = "Hammer"; break;
                case 3:
				case 4: WeaponTypeOne = "Hand to Hand"; break;
            }

            int randVariance = Util.Random(3);

			switch (randVariance)
			{
				case 0:
                Add(WeaponTypeOne, 44, 0.7f);
                Add("Savagery", 49, 0.9f);
                Add("Parry", 4, 0.0f);
				break;

                case 1:
                Add(WeaponTypeOne, 39, 0.7f);
                Add("Savagery", 49, 0.9f);
                Add("Parry", 20, 0.1f);
                break;

                case 2:
                Add(WeaponTypeOne, 44, 0.7f);
                Add("Savagery", 48, 0.9f);
                Add("Parry", 10, 0.1f);
                break;

                case 3:
                Add(WeaponTypeOne, 50, 0.7f);
                Add("Savagery", 42, 0.9f);
                Add("Parry", 9, 0.1f);
                break;
            }
        }
	}
}