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
	public class MimicSorcerer : MimicNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MimicSorcerer(GameLiving owner, byte level = 0, Point3D position = null) : base(owner, new ClassSorcerer(), level, position)
		{
			MimicSpec = new SorcererSpec();
			
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
			//		item.Quality = Util.Random(90, 100);
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
	
	public class SorcererSpec : MimicSpec
	{
		public SorcererSpec()
		{
			SpecName = "SorcererSpec";

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(4);

			switch (randVariance)
			{
				case 0:
				case 1:
                Add("Matter Magic", 8, 0.0f);
                Add("Body Magic", 30, 0.6f);
				Add("Mind Magic", 44, 0.8f);
                break;

				case 2:
                Add("Matter Magic", 26, 0.5f);
                Add("Body Magic", 5, 0.0f);
				Add("Mind Magic", 47, 0.8f);
                break;

				case 3:
                Add("Matter Magic", 5, 0.0f);
                Add("Body Magic", 22, 0.6f);
				Add("Mind Magic", 49, 0.8f);
                break;

                case 4:
                Add("Matter Magic", 8, 0.0f);
                Add("Body Magic", 40, 0.6f);
				Add("Mind Magic", 36, 0.8f);
                break;

                case 5:
                Add("Matter Magic", 24, 0.6f);
                Add("Body Magic", 48, 0.8f);
                Add("Mind Magic", 6, 0.1f);
                break;

                case 6:
                Add("Matter Magic", 6, 0.0f);
                Add("Body Magic", 45, 0.8f);
                Add("Mind Magic", 29, 0.6f);
                break;
            }
		}
	}
}