using DOL.GS.PlayerClass;
using log4net;
using System.Reflection;

namespace DOL.GS.Scripts
{
    public class MimicHunter : MimicNPC
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MimicHunter(byte level) : base(new ClassHunter(), level)
        {
            MimicSpec = new HunterSpec();

            DistributeSkillPoints();
            MimicEquipment.SetRangedWeapon(this, eObjectType.CompositeBow);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, false, 0, eHand.twoHand);
            MimicEquipment.SetShield(this, 1);

            if (MimicSpec.WeaponTypeOne == "Sword")
                MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, false, 0, eHand.oneHand);
                    
            MimicEquipment.SetArmor(this, eObjectType.Studded);        
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

            SwitchWeapon(eActiveWeaponSlot.Distance);

            RefreshSpecDependantSkills(false);
            SetSpells();
        }
    }

    public class HunterSpec : MimicSpec
    {
        public HunterSpec()
        {
            SpecName = "HunterSpec";

            int randBaseWeap = Util.Random(2);

            switch (randBaseWeap)
            {
                case 0:
                case 1: WeaponTypeOne = "Spear"; break;
                case 2: WeaponTypeOne = "Sword"; break;
            }

            int randVariance = Util.Random(4);

            switch (randVariance)
            {
                case 0:
                case 1:
                Add(WeaponTypeOne, 39, 0.8f);
                Add("Composite Bow", 35, 0.9f);
                Add("Beastcraft", 40, 0.6f);
                Add("Stealth", 38, 0.3f);
                break;

                case 2:
                case 3:
                Add(WeaponTypeOne, 39, 0.8f);
                Add("Composite Bow", 45, 0.9f);
                Add("Beastcraft", 32, 0.6f);
                Add("Stealth", 38, 0.3f);
                break;

                case 4:
                Add(WeaponTypeOne, 44, 0.9f);
                Add("Beastcraft", 50, 0.8f);
                Add("Stealth", 37, 0.5f);
                break;
            }
        }
    }
}