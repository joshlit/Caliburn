using DOL.GS.PlayerClass;
using log4net;
using System.Reflection;

namespace DOL.GS.Scripts
{
    public class MimicShadowblade : MimicNPC
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MimicShadowblade(GameLiving owner, byte level = 0, Point3D position = null) : base(owner, new ClassShadowblade(), level, position)
        {
            MimicSpec = new ShadowbladeSpec();

            DistributeSkillPoints();
            SetMeleeWeapon(MimicSpec.WeaponTypeOne, false, 0, eHand.oneHand);
            SetMeleeWeapon(MimicSpec.WeaponTypeTwo, false, 0, eHand.leftHand);
            SetMeleeWeapon(MimicSpec.WeaponTypeOne, false, 0, eHand.twoHand);
            //SetRangedWeapon(eObjectType.CompositeBow);
            SetArmor(eObjectType.Leather);
            SetJewelry();

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

            if (GetSpecializationByName("Left Axe").Level == 1)
                SwitchWeapon(eActiveWeaponSlot.TwoHanded);

            RefreshSpecDependantSkills(false);
        }
    }

    public class ShadowbladeSpec : MimicSpec
    {
        public ShadowbladeSpec()
        {
            SpecName = "ShadowbladeSpec";

            int randBaseWeap = Util.Random(1);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Sword"; break;
                case 1: WeaponTypeOne = "Axe"; break;
            }

            WeaponTypeTwo = "Axe";

            int randVariance = Util.Random(4);

            switch (randVariance)
            {
                case 0:
                case 1:
                Add(WeaponTypeOne, 34, 0.6f);
                Add("Left Axe", 39, 0.8f);
                Add("Critical Strike", 34, 0.9f);
                Add("Stealth", 35, 0.3f);
                Add("Envenom", 35, 0.5f);
                break;

                case 2:
                case 3:
                Add(WeaponTypeOne, 34, 0.6f);
                Add("Left Axe", 50, 0.8f);
                Add("Critical Strike", 10, 0.4f);
                Add("Stealth", 36, 0.3f);
                Add("Envenom", 36, 0.5f);
                break;

                case 4:
                Add(WeaponTypeOne, 39, 0.8f);
                Add("Critical Strike", 44, 0.9f);
                Add("Stealth", 38, 0.3f);
                Add("Envenom", 38, 0.5f);
                break;
            }
        }
    }
}