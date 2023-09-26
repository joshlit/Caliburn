using DOL.GS.PlayerClass;
using log4net;
using System.Reflection;

namespace DOL.GS.Scripts
{
    public class MimicNightshade : MimicNPC
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MimicNightshade(GameLiving owner, byte level = 0, Point3D position = null) : base(owner, new ClassNightshade(), level, position)
        {
            MimicSpec = new NightshadeSpec();

            DistributeSkillPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, true);

            //SetRangedWeapon(eObjectType.Fired);
            MimicEquipment.SetArmor(this, eObjectType.Leather);
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

    public class NightshadeSpec : MimicSpec
    {
        public NightshadeSpec()
        {
            SpecName = "NightshadeSpec";
            is2H = false;

            int randBaseWeap = Util.Random(1);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Blades"; break;
                case 1: WeaponTypeOne = "Piercing"; break;
            }

            int randVariance = Util.Random(2);

            switch (randVariance)
            {
                case 0:
                case 1:
                Add(WeaponTypeOne, 39, 0.8f);
                Add("Celtic Dual", 15, 0.1f);
                Add("Critical Strike", 44, 0.9f);
                Add("Stealth", 37, 0.5f);
                Add("Envenom", 37, 0.6f);
                break;

                case 2:
                Add(WeaponTypeOne, 39, 0.9f);
                Add("Celtic Dual", 50, 1.0f);
                Add("Stealth", 34, 0.5f);
                Add("Envenom", 34, 0.6f);
                break;
            }
        }
    }
}