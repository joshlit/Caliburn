using DOL.GS.PlayerClass;
using log4net;
using System.Reflection;

namespace DOL.GS.Scripts
{
    public class MimicRanger : MimicNPC
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MimicRanger(GameLiving owner, byte level = 0, Point3D position = null) : base(owner, new ClassRanger(), level, position)
        {
            MimicSpec = new RangerSpec();

            DistributeSkillPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, true);
            MimicEquipment.SetRangedWeapon(this, eObjectType.RecurvedBow);
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
            SetSpells();
        }
    }

    public class RangerSpec : MimicSpec
    {
        public RangerSpec()
        {
            SpecName = "RangerSpec";
            is2H = false;

            int randBaseWeap = Util.Random(1);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Blades"; break;
                case 1: WeaponTypeOne = "Piercing"; break;
            }

            int randVariance = Util.Random(7);

            switch (randVariance)
            {
                case 0:
                case 1:
                Add(WeaponTypeOne, 32, 0.4f);
                Add("Archery", 35, 0.9f);
                //Add("Pathfinding", 40, 0.5f);
                Add("Celtic Dual", 29, 0.3f);
                Add("Stealth", 35, 0.2f);
                break;

                case 2:
                case 3:
                Add(WeaponTypeOne, 35, 0.4f);
                Add("Archery", 35, 0.9f);
                //Add("Pathfinding", 36, 0.5f);
                Add("Celtic Dual", 31, 0.3f);
                Add("Stealth", 35, 0.2f);
                break;

                case 4:
                case 5:
                Add(WeaponTypeOne, 27, 0.4f);
                Add("Archery", 45, 0.9f);
                //Add("Pathfinding", 40, 0.5f);
                Add("Celtic Dual", 19, 0.3f);
                Add("Stealth", 35, 0.2f);
                break;

                case 6:
                Add(WeaponTypeOne, 35, 0.6f);
                Add("Pathfinding", 42, 0.5f);
                Add("Celtic Dual", 40, 1.0f);
                Add("Stealth", 35, 0.2f);
                break;

                case 7:
                Add(WeaponTypeOne, 25, 0.6f);
                Add("Pathfinding", 40, 0.5f);
                Add("Celtic Dual", 50, 1.0f);
                Add("Stealth", 33, 0.2f);
                break;
            }
        }
    }
}