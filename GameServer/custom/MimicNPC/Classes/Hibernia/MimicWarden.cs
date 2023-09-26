using System;
using System.Reflection;
using DOL.GS;
using DOL.GS.Scripts;
using DOL.Database;
using log4net;
using DOL.GS.Realm;
using System.Collections.Generic;
using System.Web;
using static DOL.GS.CommanderPet;
using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
	public class MimicWarden : MimicNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MimicWarden(GameLiving owner, byte level = 0, Point3D position = null) : base(owner, new ClassWarden(), level, position)
		{
            MimicSpec = new WardenSpec();

			DistributeSkillPoints();
			MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne);
			MimicEquipment.SetShield(this, 2);
			//SetRangedWeapon(eObjectType.Fired);
			MimicEquipment.SetArmor(this, eObjectType.Reinforced);
			MimicEquipment.SetJewelry(this);

            //foreach (InventoryItem item in Inventory.EquippedItems)
            //{
            //    if (item == null)
            //        return;

            //    if (item.Quality < 90)
            //    {
            //        item.Quality = Util.Random(85, 100);
            //    }

            //    log.Debug("Name: " + item.Name);
            //    log.Debug("Slot: " + Enum.GetName(typeof(eInventorySlot), item.SlotPosition));
            //    log.Debug("DPS_AF: " + item.DPS_AF);
            //    log.Debug("SPD_ABS: " + item.SPD_ABS);
            //}

            SwitchWeapon(eActiveWeaponSlot.Standard);

            RefreshSpecDependantSkills(false);
			SetSpells();
		}
	}

	public class WardenSpec : MimicSpec
	{
		public WardenSpec() 
		{
			SpecName = "WardenSpec";
            is2H = false;

            int randBaseWeap = Util.Random(1);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Blades"; break;
                case 1: WeaponTypeOne = "Blunt"; break;
            }

            int randVariance = Util.Random(3);

            switch (randVariance)
            {
                case 0:
                Add(WeaponTypeOne, 39, 0.6f);
                Add("Nurture", 45, 0.9f);
                Add("Regrowth", 26, 0.5f);
                Add("Parry", 10, 0.1f);
                break;

                case 1:
                Add(WeaponTypeOne, 39, 0.7f);
                Add("Nurture", 49, 0.9f);
                Add("Regrowth", 16, 0.3f);
                Add("Parry", 12, 0.1f);
                break;

                case 2:
                Add(WeaponTypeOne, 34, 0.3f);
                Add("Nurture", 49, 0.9f);
                Add("Regrowth", 26, 0.4f);
                Add("Parry", 10, 0.0f);
                break;

                case 3:
                Add("Nurture", 45, 0.9f);
                Add("Regrowth", 48, 0.7f);
                Add("Parry", 5, 0.0f);
                break;
            }
        }
	}
}