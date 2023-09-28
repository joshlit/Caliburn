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
	public class MimicBonedancer : MimicNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MimicBonedancer(byte level) : base(new ClassBonedancer(), level)
		{
			MimicSpec = new BonedancerSpec();

			DistributeSkillPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne);
            MimicEquipment.SetArmor(this, eObjectType.Cloth);
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
			SetCasterSpells();
		}
	}

	public class BonedancerSpec : MimicSpec
	{
		public BonedancerSpec()
		{
            SpecName = "BonedancerSpec";

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(5);

			switch (randVariance)
			{
				case 0:
                Add("Darkness", 26, 0.1f);
                Add("Suppression", 47, 1.0f);
                Add("Bone Army", 5, 0.0f);
				break;

                case 1:
                Add("Darkness", 24, 0.1f);
                Add("Suppression", 48, 1.0f);
                Add("Bone Army", 6, 0.0f);
                break;

                case 2:
                Add("Darkness", 5, 0.0f);
                Add("Suppression", 47, 1.0f);
                Add("Bone Army", 26, 0.1f);
                break;

                case 3:
                Add("Darkness", 39, 0.5f);
                Add("Suppression", 37, 0.8f);
                Add("Bone Army", 4, 0.0f);
                break;

                case 4:
                Add("Darkness", 50, 1.0f);
                Add("Suppression", 20, 0.1f);
                Add("Bone Army", 4, 0.0f);
                break;

                case 5:
                Add("Darkness", 6, 0.0f);
                Add("Suppression", 24, 0.1f);
                Add("Bone Army", 48, 1.0f);
                break;
            }
        }
	}
}