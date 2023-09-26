using DOL.GS.PlayerClass;
using log4net;
using System.Reflection;

namespace DOL.GS.Scripts
{
    public class MimicPaladin : MimicNPC
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MimicPaladin(GameLiving owner, byte level = 0, Point3D position = null) : base(owner, new ClassPaladin(), level, position)
        {
            MimicSpec = new PaladinSpec();

            DistributeSkillPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeTwo, false, MimicSpec.DamageType);
            MimicEquipment.SetArmor(this, eObjectType.Plate);
            MimicEquipment.SetShield(this, 3);
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

            if (!MimicSpec.is2H)
                SwitchWeapon(eActiveWeaponSlot.Standard);
            else
                SwitchWeapon(eActiveWeaponSlot.TwoHanded);

            RefreshSpecDependantSkills(false);
            SetSpells();
        }
    }

    public class PaladinSpec : MimicSpec
    {
        public PaladinSpec()
        {
            SpecName = "PaladinSpec";

            int randBaseWeap = Util.Random(2);

            switch (randBaseWeap)
            {
                case 0: 
                WeaponTypeOne = "Slash";
                DamageType = eWeaponDamageType.Slash;
                break;

                case 1: 
                WeaponTypeOne = "Thrust";
                DamageType = eWeaponDamageType.Thrust;
                break;

                case 2:
                WeaponTypeOne = "Crush";
                DamageType = eWeaponDamageType.Crush;
                break;
            }

            WeaponTypeTwo = "Two Handed";

            int randVariance = Util.Random(7);

            switch (randVariance)
            {
                case 0:
                is2H = true;
                Add(WeaponTypeOne, 39, 0.6f);
                Add(WeaponTypeTwo, 44, 0.8f);
                Add("Chants", 48, 0.9f);
                Add("Parry", 19, 0.2f);
                break;

                case 1:
                is2H = true;
                Add(WeaponTypeOne, 34, 0.6f);
                Add(WeaponTypeTwo, 50, 0.8f);
                Add("Chants", 48, 0.9f);
                Add("Parry", 13, 0.2f);
                break;

                case 2:
                case 3:
                Add(WeaponTypeOne, 39, 0.6f);
                Add("Chants", 50, 1.0f);
                Add("Shields", 42, 0.7f);
                Add("Parry", 18, 0.1f);
                break;

                case 4:
                Add(WeaponTypeOne, 29, 0.6f);
                Add("Chants", 46, 1.0f);
                Add("Shields", 50, 0.7f);
                Add("Parry", 25, 0.1f);
                break;

                case 5:
                Add(WeaponTypeOne, 29, 0.6f);
                Add("Chants", 46, 1.0f);
                Add("Shields", 42, 0.7f);
                Add("Parry", 25, 0.1f);
                break;

                case 6:
                case 7:
                Add(WeaponTypeOne, 39, 0.6f);
                Add("Chants", 48, 1.0f);
                Add("Shields", 42, 0.7f);
                Add("Parry", 23, 0.1f);
                break;
            }
        }
    }
}