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
	public class MimicScout : MimicNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MimicScout(GameLiving owner, byte level = 0, Point3D position = null) : base(owner, new ClassScout(), level, position)
		{
			MimicSpec = new ScoutSpec();

			DistributeSkillPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne);
            MimicEquipment.SetRangedWeapon(this, eObjectType.Longbow);
            MimicEquipment.SetShield(this, 1);
            MimicEquipment.SetArmor(this, eObjectType.Studded);
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

			SwitchWeapon(eActiveWeaponSlot.Distance);

			RefreshSpecDependantSkills(false);
		}
	}

    public class ScoutSpec : MimicSpec
    {
        public ScoutSpec()
        {
            SpecName = "ScoutSpec";
            is2H = false;

            int randBaseWeap = Util.Random(1);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Slash"; break;
                case 1: WeaponTypeOne = "Thrust"; break;
            }

            int randVariance = Util.Random(1);

            switch (randVariance)
            {
                case 0:
                Add(WeaponTypeOne, 29, 0.6f);
                Add("Archery", 44, 0.8f);
                Add("Shields", 42, 0.7f);
                Add("Stealth", 35, 0.1f);
                break;

                case 1:
                Add(WeaponTypeOne, 29, 0.7f);
                Add("Archery", 50, 0.8f);
                Add("Shields", 35, 0.6f);
                Add("Stealth", 35, 0.1f);
                break;
            }
        }
    }
}