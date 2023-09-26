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
	public class MimicRunemaster : MimicNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MimicRunemaster(GameLiving owner, byte level = 0, Point3D position = null) : base(owner, new ClassRunemaster(), level, position)
		{
			MimicSpec = new RunemasterSpec();

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

			SwitchWeapon(eActiveWeaponSlot.TwoHanded);

			RefreshSpecDependantSkills(false);
			SetCasterSpells();
		}
	}

	public class RunemasterSpec : MimicSpec
	{
		public RunemasterSpec()
		{
            SpecName = "RunemasterSpec";

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(7);

			switch (randVariance)
			{
				case 0:
				case 1:
                Add("Darkness", 47, 1.0f);
                Add("Suppression", 26, 0.1f);
                Add("Runecarving", 5, 0.0f);
				break;

                case 2:
				case 3:
                Add("Darkness", 24, 0.5f);
                Add("Suppression", 6, 0.1f);
                Add("Runecarving", 48, 1.0f);
                break;

                case 4:
                Add("Darkness", 5, 0.0f);
                Add("Suppression", 26, 0.1f);
                Add("Runecarving", 47, 1.0f);
                break;

                case 5:
				case 6:
                Add("Darkness", 20, 0.1f);
                Add("Suppression", 50, 1.0f);
                Add("Runecarving", 4, 0.0f);
                break;

                case 7:
                Add("Darkness", 31, 0.1f);
                Add("Suppression", 44, 1.0f);
                Add("Runecarving", 4, 0.0f);
                break;
            }
        }
	}
}