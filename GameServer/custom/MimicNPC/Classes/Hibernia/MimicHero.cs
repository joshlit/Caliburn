using DOL.GS.PlayerClass;
using log4net;
using System.Reflection;

namespace DOL.GS.Scripts
{
    public class MimicHero : MimicNPC
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MimicHero(GameLiving owner, byte level = 0, Point3D position = null) : base(owner, new ClassHero(), level, position)
        {
            MimicSpec = MimicManager.Random(this);
            DistributeSkillPoints();

            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne);

            if (MimicSpec.SpecName == "HybridHero")
            {
                MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeTwo);
            }

            MimicEquipment.SetShield(this, 3);

            //SetRangedWeapon(eObjectType.Fired);

            if (Level >= 15)
                MimicEquipment.SetArmor(this, eObjectType.Scale);
            else
                MimicEquipment.SetArmor(this, eObjectType.Reinforced);

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

    public class ShieldHero : MimicSpec
    {
        public ShieldHero()
        {
            SpecName = "ShieldHero";
            is2H = false;

            string weaponType = string.Empty;

            int randBaseWeap = Util.Random(2);

            switch (randBaseWeap)
            {
                case 0: weaponType = "Blades"; break;
                case 1: weaponType = "Piercing"; break;
                case 2: weaponType = "Blunt"; break;
            }

            WeaponTypeOne = weaponType;
            DamageType = 0;

            int randVariance = Util.Random(2);

            switch (randVariance)
            {
                case 0:
                Add(weaponType, 50, 1.0f);
                Add("Shields", 50, 0.9f);
                Add("Parry", 28, 0.1f);
                break;

                case 1:
                Add(weaponType, 39, 0.9f);
                Add("Shields", 42, 1.0f);
                Add("Parry", 50, 0.2f);
                break;

                case 2:
                Add(weaponType, 44, 1.0f);
                Add("Shields", 50, 0.9f);
                Add("Parry", 37, 0.2f);
                break;
            }
        }
    }

    public class HybridHero : MimicSpec
    {
        public HybridHero()
        {
            SpecName = "HybridHero";
            is2H = true;

            int randBaseWeap = Util.Random(2);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Blades"; break;
                case 1: WeaponTypeOne = "Piercing"; break;
                case 2: WeaponTypeOne = "Blunt"; break;
            }

            Add(WeaponTypeOne, 39, 0.8f);

            int randVariance = Util.Random(1);

            if (randVariance == 0)
                WeaponTypeTwo = "Celtic Spear";
            else
                WeaponTypeTwo = "Large Weapons";

            int randVariance2 = Util.Random(1);

            switch (randVariance2)
            {
                case 0:
                Add(WeaponTypeTwo, 50, 1.0f);
                Add("Shields", 42, 0.5f);
                Add("Parry", 6, 0.1f);
                break;

                case 1:
                Add(WeaponTypeTwo, 44, 1.0f);
                Add("Shields", 42, 0.5f);
                Add("Parry", 24, 0.1f);
                break;
            }
        }
    }
}