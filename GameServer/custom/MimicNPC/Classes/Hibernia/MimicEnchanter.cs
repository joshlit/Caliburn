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
    public class MimicEnchanter : MimicNPC
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MimicEnchanter(GameLiving owner, byte level = 0, Point3D position = null) : base(owner, new ClassEnchanter(), level, position)
        {
            MimicSpec = MimicManager.Random(this);

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

    public class ManaEnchanter : MimicSpec
    {
        public ManaEnchanter()
        {
            SpecName = "ManaEnchanter";

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(1);

            switch (randVariance)
            {
                case 0:
                Add("Mana", 50, 1.0f);
                Add("Light", 20, 0.1f);
                Add("Enchantments", 4, 0.0f);
                break;

                case 1:
                Add("Mana", 49, 1.0f);
                Add("Light", 22, 0.1f);
                Add("Enchantments", 5, 0.0f);
                break;
            }
        }
    }

    public class LightEnchanter : MimicSpec 
    { 
        public LightEnchanter() 
        {
            SpecName = "LightEnchanter";

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(1);

            switch (randVariance)
            {
                case 0:
                Add("Mana", 27, 0.2f);
                Add("Light", 45, 1.0f);
                Add("Enchantments", 12, 0.1f);
                break;

                case 1:
                Add("Mana", 24, 0.2f);
                Add("Light", 45, 1.0f);
                Add("Enchantments", 17, 0.1f);
                break;
            }
        } 
    }
}