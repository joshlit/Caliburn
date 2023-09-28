using DOL.GS.PlayerClass;
using log4net;
using System.Reflection;

namespace DOL.GS.Scripts
{
    public class MimicEldritch : MimicNPC
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MimicEldritch(byte level) : base(new ClassEldritch(), level)
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

    public class SunEldritch : MimicSpec
    {
        public SunEldritch()
        {
            SpecName = "SunEldritch";

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(3);

            switch (randVariance)
            {
                case 0:
                Add("Light", 45, 1.0f);
                Add("Void", 29, 0.1f);
                Add("Mana", 6, 0.0f);
                break;

                case 1:
                Add("Light", 40, 1.0f);
                Add("Void", 35, 0.1f);
                Add("Mana", 6, 0.0f);
                break;

                case 2:
                Add("Light", 47, 1.0f);
                Add("Void", 26, 0.1f);
                break;

                case 3:
                Add("Light", 45, 1.0f);
                Add("Mana", 29, 0.1f);
                break;
            }
        }
    }

    public class ManaEldritch : MimicSpec
    {
        public ManaEldritch()
        {
            SpecName = "ManaEldritch";

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(3);

            switch (randVariance)
            {
                case 0:
                Add("Mana", 50, 1.0f);
                Add("Light", 20, 0.1f);
                break;

                case 1:
                Add("Mana", 50, 1.0f);
                Add("Void", 20, 0.1f);
                break;

                case 2:
                Add("Mana", 48, 1.0f);
                Add("Light", 24, 0.1f);
                break;

                case 3:
                Add("Mana", 48, 1.0f);
                Add("Void", 24, 0.1f);
                break;
            }
        }
    }

    public class VoidEldritch : MimicSpec
    {
        public VoidEldritch()
        {
            SpecName = "VoidEldritch";

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(3);

            switch (randVariance)
            {
                case 0:
                Add("Void", 49, 1.0f);
                Add("Light", 19, 0.1f);
                Add("Mana", 12, 0.0f);
                break;

                case 1:
                Add("Void", 48, 1.0f);
                Add("Light", 24, 0.1f);
                Add("Mana", 6, 0.0f);
                break;

                case 2:
                Add("Void", 46, 1.0f);
                Add("Light", 28, 0.1f);
                break;

                case 3:
                Add("Void", 46, 1.0f);
                Add("Mana", 28, 0.1f);
                break;
            }
        }
    }

    public class HybridEldritch : MimicSpec
    {
        public HybridEldritch()
        {
            SpecName = "HybridEldritch";

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(3);

            switch (randVariance)
            {
                case 0:
                Add("Void", 27, 1.0f);
                Add("Light", 24, 0.1f);
                Add("Mana", 39, 0.0f);
                break;

                case 1:
                Add("Void", 48, 1.0f);
                Add("Light", 24, 0.1f);
                Add("Mana", 6, 0.0f);
                break;

                case 2:
                Add("Void", 46, 1.0f);
                Add("Light", 28, 0.1f);
                break;

                case 3:
                Add("Void", 46, 1.0f);
                Add("Mana", 28, 0.1f);
                break;
            }
        }
    }
}