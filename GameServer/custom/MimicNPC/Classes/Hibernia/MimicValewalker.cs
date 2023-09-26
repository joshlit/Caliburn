using DOL.GS.PlayerClass;
using log4net;
using System.Reflection;

namespace DOL.GS.Scripts
{
    public class MimicValewalker : MimicNPC
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MimicValewalker(GameLiving owner, byte level = 0, Point3D position = null) : base(owner, new ClassValewalker(), level, position)
        {
            MimicSpec = new ValewalkerSpec();

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
            SetSpells();
        }
    }

    public class ValewalkerSpec : MimicSpec
    {
        public ValewalkerSpec()
        {
            SpecName = "ValewalkerSpec";

            WeaponTypeOne = "Scythe";

            int randVariance = Util.Random(5);

            switch (randVariance)
            {
                case 0:
                Add("Arboreal Path", 43, 0.8f);
                Add("Parry", 23, 0.3f);
                Add("Scythe", 44, 0.9f);
                break;

                case 1:
                Add("Arboreal Path", 43, 0.8f);
                Add("Parry", 2, 0.1f);
                Add("Scythe", 50, 0.9f);
                break;

                case 2:
                Add("Arboreal Path", 34, 0.8f);
                Add("Parry", 26, 0.1f);
                Add("Scythe", 50, 0.9f);
                break;

                case 3:
                Add("Arboreal Path", 50, 0.9f);
                Add("Parry", 18, 0.2f);
                Add("Scythe", 39, 0.8f);
                break;

                case 4:
                Add("Arboreal Path", 43, 0.8f);
                Add("Parry", 2, 0.1f);
                Add("Scythe", 50, 0.9f);
                break;

                case 5:
                Add("Arboreal Path", 48, 0.8f);
                Add("Parry", 10, 0.1f);
                Add("Scythe", 44, 0.9f);
                break;
            }
        }
    }
}