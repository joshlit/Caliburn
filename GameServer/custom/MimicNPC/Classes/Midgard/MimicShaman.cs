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
	public class MimicShaman : MimicNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MimicShaman(GameLiving owner, byte level = 0, Point3D position = null) : base(owner, new ClassShaman(), level, position)
		{
            MimicSpec = new ShamanSpec();

			DistributeSkillPoints();
			SetMeleeWeapon(MimicSpec.WeaponTypeOne, false, 0, eHand.oneHand);
			SetShield(1);
			//SetRangedWeapon(eObjectType.Fired);
			SetArmor(eObjectType.Chain);
			SetJewelry();

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

	public class ShamanSpec : MimicSpec
	{
		public ShamanSpec()
		{
            SpecName = "ShamanSpec";

			WeaponTypeOne = "Hammer";

            int randVariance = Util.Random(7);

			switch (randVariance)
			{
				case 0:
                case 1:
                Add("Mending", 8, 0.2f);
                Add("Augmentation", 46, 0.8f);
                Add("Subterranean", 27, 0.5f);
                break;

                case 2:
                Add("Mending", 26, 0.5f);
                Add("Augmentation", 47, 0.8f);
                Add("Subterranean", 5, 0.0f);
                break;

                case 3:
                Add("Mending", 33, 0.5f);
                Add("Augmentation", 42, 0.8f);
                Add("Subterranean", 7, 0.0f);
                break;

                case 4:
                Add("Mending", 21, 0.5f);
                Add("Augmentation", 48, 0.8f);
                Add("Subterranean", 12, 0.0f);
                break;

                case 5:
                Add("Mending", 4, 0.2f);
                Add("Augmentation", 28, 0.5f);
                Add("Subterranean", 46, 0.8f);
                break;

                case 6:
                Add("Mending", 27, 0.5f);
                Add("Augmentation", 8, 0.1f);
                Add("Subterranean", 46, 0.8f);
                break;

                case 7:
                Add("Mending", 43, 0.8f);
                Add("Augmentation", 32, 0.5f);
                Add("Subterranean", 6, 0.0f);
                break;
            }
        }
	}
}