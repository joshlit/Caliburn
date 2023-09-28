using DOL.GS.PlayerClass;
using log4net;
using System.Reflection;

namespace DOL.GS.Scripts
{
    public class MimicMentalist : MimicNPC
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MimicMentalist(byte level) : base(new ClassMentalist(), level)
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

    public class LightMentalist : MimicSpec
    {
        public LightMentalist()
        {
            SpecName = "LightMentalist";

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(5);

            switch (randVariance)
            {
                case 0:
                Add("Light", 45, 1.0f);
                Add("Mentalism", 28, 0.1f);
                Add("Mana", 10, 0.0f);
                break;

                case 1:
                Add("Light", 45, 1.0f);
                Add("Mentalism", 17, 0.0f);
                Add("Mana", 24, 0.1f);
                break;

                case 2:
                Add("Light", 45, 1.0f);
                Add("Mentalism", 6, 0.0f);
                Add("Mana", 29, 0.1f);
                break;

                case 3:
                Add("Light", 42, 1.0f);
                Add("Mentalism", 33, 0.1f);
                Add("Mana", 7, 0.0f);
                break;

                case 4:
                Add("Light", 42, 1.0f);
                Add("Mentalism", 23, 0.2f);
                Add("Mana", 24, 0.1f);
                break;
            }
        }
    }

    public class ManaMentalist : MimicSpec
    {
        public ManaMentalist()
        {
            SpecName = "ManaMentalist";

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(5);

            switch (randVariance)
            {
                case 0:
                Add("Mana", 48, 1.0f);
                Add("Mentalism", 24, 0.1f);
                Add("light", 6, 0.0f);
                break;

                case 1:
                Add("Mana", 48, 1.0f);
                Add("Mentalism", 6, 0.0f);
                Add("light", 24, 0.1f);
                break;

                case 2:
                Add("Mana", 46, 1.0f);
                Add("Mentalism", 28, 0.1f);
                Add("light", 4, 0.0f);
                break;

                case 3:
                Add("Mana", 44, 1.0f);
                Add("Mentalism", 31, 0.1f);
                Add("light", 4, 0.0f);
                break;

                case 4:
                Add("Mana", 44, 1.0f);
                Add("Mentalism", 4, 0.0f);
                Add("light", 31, 0.1f);
                break;
            }
        }
    }

    public class MentalismMentalist : MimicSpec
    {
        public MentalismMentalist()
        {
            SpecName = "MentalismMentalist";

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(4);

            switch (randVariance)
            {
                case 0:
                Add("Mentalism", 50, 1.0f);
                Add("Mana", 4, 0.0f);
                Add("light", 20, 0.1f);
                break;

                case 1:
                Add("Mentalism", 50, 1.0f);
                Add("Mana", 14, 0.1f);
                Add("light", 14, 0.0f);
                break;

                case 2:
                Add("Mentalism", 42, 1.0f);
                Add("Mana", 7, 0.0f);
                Add("light", 33, 0.1f);
                break;

                case 3:
                Add("Mentalism", 41, 0.9f);
                Add("Mana", 34, 0.1f);
                Add("light", 8, 0.0f);
                break;
            }
        }
    }
}