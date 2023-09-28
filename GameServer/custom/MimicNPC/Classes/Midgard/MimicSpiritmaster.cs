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
	public class MimicSpiritmaster : MimicNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MimicSpiritmaster(byte level) : base(new ClassSpiritmaster(), level)
		{
			MimicSpec = new SpiritmasterSpec();

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

	public class SpiritmasterSpec : MimicSpec
	{
		public SpiritmasterSpec()
		{
            SpecName = "SpiritmasterSpec";

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(9);

			switch (randVariance)
			{
				case 0:
				case 1:
                Add("Darkness", 47, 1.0f);
                Add("Suppression", 5, 0.0f);
                Add("Summoning", 26, 0.1f);
				break;

                case 2:
				case 3:
                Add("Darkness", 47, 1.0f);
                Add("Suppression", 26, 0.1f);
                Add("Summoning", 6, 0.0f);
                break;

                case 4:
				case 5:
                Add("Darkness", 5, 0.0f);
                Add("Suppression", 49, 0.0f);
                Add("Summoning", 22, 0.1f);
                break;

				case 6:
                Add("Darkness", 35, 0.1f);
                Add("Suppression", 41, 1.0f);
                Add("Summoning", 3, 0.0f);
                break;

                case 7:
                Add("Darkness", 24, 0.1f);
                Add("Suppression", 6, 0.0f);
                Add("Summoning", 48, 1.0f);
                break;

                case 8:
                Add("Darkness", 28, 0.1f);
                Add("Suppression", 10, 0.0f);
                Add("Summoning", 45, 1.0f);
                break;

                case 9:
                Add("Darkness", 12, 0.0f);
                Add("Suppression", 43, 0.1f);
                Add("Summoning", 30, 1.0f);
                break;
            }
        }
	}
}