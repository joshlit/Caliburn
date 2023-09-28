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
	public class MimicDruid : MimicNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MimicDruid(byte level) : base(new ClassDruid(), level)
		{
            MimicSpec = new DruidSpec();

			DistributeSkillPoints();
			MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne);
			MimicEquipment.SetShield(this, 1);
			//SetRangedWeapon(eObjectType.Fired);
			MimicEquipment.SetArmor(this, eObjectType.Scale);
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

	public class DruidSpec : MimicSpec
	{
		public DruidSpec() 
		{
			SpecName = "DruidSpec";
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
                Add("Nurture", 42, 0.9f);
                Add("Nature", 7, 0.0f);
                Add("Regrowth", 33, 0.7f);
                break;

                case 1:
                Add("Nurture", 40, 0.9f);
                Add("Nature", 9, 0.0f);
                Add("Regrowth", 35, 0.7f);
                break;

                case 2:
                Add("Nurture", 14, 0.1f);
                Add("Nature", 39, 0.9f);
                Add("Regrowth", 34, 0.7f);
                break;

                case 3:
                Add("Nurture", 35, 0.7f);
                Add("Nature", 3, 0.0f);
                Add("Regrowth", 41, 0.8f);
                break;

            }
        }
	}
}