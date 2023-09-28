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
    public class MimicFriar : MimicNPC
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MimicFriar(byte level) : base(new ClassFriar(), level)
        {
            MimicSpec = new FriarSpec();

            DistributeSkillPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne);
            MimicEquipment.SetArmor(this, eObjectType.Leather);
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
            SetSpells();
        }
    }

    public class FriarSpec : MimicSpec
    {
        public FriarSpec()
        {
            SpecName = "FriarSpec";

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(4);

            switch (randVariance)
            {
                case 0:
                case 1:
                Add("Rejuvenation", 18, 0.5f);
                Add("Enhancement", 46, 0.8f);
                Add("Staff", 50, 0.9f);
                Add("Parry", 16, 0.1f);
                break;

                case 2:
                Add("Rejuvenation", 10, 0.1f);
                Add("Enhancement", 50, 0.8f);
                Add("Staff", 50, 0.9f);
                Add("Parry", 10, 0.1f);
                break;

                case 3:
                Add("Rejuvenation", 44, 0.8f);
                Add("Enhancement", 45, 0.9f);
                Add("Staff", 34, 0.6f);
                Add("Parry", 8, 0.1f);
                break;

                case 4:
                Add("Rejuvenation", 15, 0.1f);
                Add("Enhancement", 50, 0.9f);
                Add("Staff", 44, 0.8f);
                Add("Parry", 23, 0.2f);
                break;
            }
        }
    }
}