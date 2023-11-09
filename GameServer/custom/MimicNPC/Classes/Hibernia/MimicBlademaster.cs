using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicBlademaster : MimicNPC
    {
        public MimicBlademaster(byte level) : base(new ClassBlademaster(), level)
        {
            MimicSpec = new BlademasterSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.leftHand);
            MimicEquipment.SetRangedWeapon(this, eObjectType.Fired);
            MimicEquipment.SetArmor(this, eObjectType.Reinforced);
            MimicEquipment.SetJewelryROG(this, Realm, (eCharacterClass)CharacterClass.ID, Level, eObjectType.Magical);
            RefreshItemBonuses();
            SwitchWeapon(eActiveWeaponSlot.Standard);
            RefreshSpecDependantSkills(false);
            GetTauntStyles();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class BlademasterSpec : MimicSpec
    {
        public BlademasterSpec()
        {
            SpecName = "BlademasterSpec";
            is2H = false;

            int randBaseWeap = Util.Random(2);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Blades"; break;
                case 1: WeaponTypeOne = "Piercing"; break;
                case 2: WeaponTypeOne = "Blunt"; break;
            }

            int randVariance = Util.Random(2);

            switch (randVariance)
            {
                case 0:
                case 1:
                Add(WeaponTypeOne, 50, 0.8f);
                Add("Celtic Dual", 50, 1.0f);
                Add("Parry", 28, 0.2f);
                break;

                case 2:
                Add(WeaponTypeOne, 39, 0.8f);
                Add("Celtic Dual", 50, 1.0f);
                Add("Shields", 42, 0.5f);
                Add("Parry", 6, 0.1f);
                break;
            }
        }
    }
}