using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicShadowblade : MimicNPC
    {
        public MimicShadowblade(byte level) : base(new ClassShadowblade(), level)
        {
            MimicSpec = new ShadowbladeSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeTwo, eHand.leftHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.twoHand);
            //MimicEquipment.SetRangedWeapon(this, eObjectType.Thrown);
            MimicEquipment.SetArmor(this, eObjectType.Leather);
            MimicEquipment.SetJewelryROG(this, Realm, (eCharacterClass)CharacterClass.ID, Level, eObjectType.Magical);
            RefreshItemBonuses();
            SwitchWeapon(eActiveWeaponSlot.Standard);

            if (GetSpecializationByName("Left Axe").Level == 1)
                SwitchWeapon(eActiveWeaponSlot.TwoHanded);

            RefreshSpecDependantSkills(false);
            IsCloakHoodUp = Util.RandomBool();
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