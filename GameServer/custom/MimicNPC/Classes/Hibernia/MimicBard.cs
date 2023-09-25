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
	public class MimicBard : MimicNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MimicBard(GameLiving owner, byte level = 0, Point3D position = null) : base(owner, new ClassBard(), level, position)
		{
            MimicSpec = new BardSpec();

			DistributeSkillPoints();
			SetMeleeWeapon(MimicSpec.WeaponTypeOne);
			SetShield(1);
			//SetRangedWeapon(eObjectType.Fired);
			SetArmor(eObjectType.Reinforced);
			SetJewelry();

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

	public class BardSpec : MimicSpec
	{
		public BardSpec() 
		{
			SpecName = "BardSpec";
            is2H = false;

            int randBaseWeap = Util.Random(1);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Blades"; break;
                case 1: WeaponTypeOne = "Blunt"; break;
            }

            int randVariance = Util.Random(2);

            switch (randVariance)
            {
                case 0:
                Add("Music", 47, 0.8f);
                Add("Nurture", 43, 0.9f);
                Add("Regrowth", 16, 0.1f);
                break;

                case 1:
                Add("Music", 37, 0.4f);
                Add("Nurture", 43, 0.9f);
                Add("Regrowth", 33, 0.7f);
                break;

                case 2:
                Add(WeaponTypeOne, 29, 0.7f);
                Add("Music", 37, 0.4f);
                Add("Nurture", 43, 0.9f);
                Add("Regrowth", 16, 0.1f);
                break;
            }
        }
	}
}