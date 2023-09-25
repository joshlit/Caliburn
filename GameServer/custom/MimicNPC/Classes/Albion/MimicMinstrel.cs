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
	public class MimicMinstrel : MimicNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MimicMinstrel(GameLiving owner, byte level = 0, Point3D position = null) : base(owner, new ClassMinstrel(), level, position)
		{
			MimicSpec = new MinstrelSpec();

			DistributeSkillPoints();
			SetMeleeWeapon(MimicSpec.WeaponTypeOne);
            SetArmor(eObjectType.Chain);
			SetShield(1);

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

	public class MinstrelSpec : MimicSpec
	{
		public MinstrelSpec()
		{
			SpecName = "MinstrelSpec";

            int randBaseWeap = Util.Random(1);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Slash"; break;
                case 1: WeaponTypeOne = "Thrust"; break;
            }

            int randVariance = Util.Random(4);

            switch (randVariance)
            {
                case 0:
				case 1:
                Add(WeaponTypeOne, 39, 0.6f);
                Add("Instruments", 50, 1.0f);
                Add("Stealth", 21, 0.1f);
                break;

				case 2:
				case 3:
                Add(WeaponTypeOne, 44, 0.6f);
                Add("Instruments", 50, 1.0f);
                Add("Stealth", 8, 0.0f);
                break;

                case 4:
                Add(WeaponTypeOne, 50, 0.6f);
                Add("Instruments", 44, 1.0f);
                Add("Stealth", 8, 0.0f);
                break;
            }
        }
	}
}