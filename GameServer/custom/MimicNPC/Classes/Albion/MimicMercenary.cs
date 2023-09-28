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
	public class MimicMercenary : MimicNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MimicMercenary(byte level) : base(new ClassMercenary(), level)
		{
			MimicSpec = new MercenarySpec();

			DistributeSkillPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, true);
            MimicEquipment.SetArmor(this, eObjectType.Chain);
            //SetRangedWeapon(eObjectType.Fired);
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

	public class MercenarySpec : MimicSpec
	{
		public MercenarySpec()
		{
			SpecName = "MercenarySpec";

            int randBaseWeap = Util.Random(2);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Slash"; break;
                case 1: WeaponTypeOne = "Thrust"; break;
                case 2: WeaponTypeOne = "Crush"; break;
            }

            int randVariance = Util.Random(3);

            switch (randVariance)
            {
                case 0:
				case 1:
				case 2:
                Add(WeaponTypeOne, 50, 0.8f);
                Add("Dual Wield", 50, 1.0f);
                Add("Parry", 33, 0.2f);
                break;

				case 3:
                Add(WeaponTypeOne, 39, 0.8f);
                Add("Dual Wield", 50, 0.9f);
				Add("Shields", 42, 0.5f);
                Add("Parry", 18, 0.1f);
                break;
            }
        }
	}
}