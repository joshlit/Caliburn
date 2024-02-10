using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicBonedancer : MimicNPC
    {
        public MimicBonedancer(byte level) : base(new ClassBonedancer(), level)
        {
            MimicSpec = new BonedancerSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.twoHand);
            MimicEquipment.SetArmor(this, eObjectType.Cloth);
            MimicEquipment.SetJewelryROG(this, Realm, (eCharacterClass)CharacterClass.ID, Level, eObjectType.Magical);
            RefreshItemBonuses();
            SwitchWeapon(eActiveWeaponSlot.Standard);
            RefreshSpecDependantSkills(false);
            SetCasterSpells();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class BonedancerSpec : MimicSpec
    {
        public BonedancerSpec()
        {
            SpecName = "BonedancerSpec";

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(5);

            switch (randVariance)
            {
                case 0:
                Add("Darkness", 26, 0.1f);
                Add("Suppression", 47, 1.0f);
                Add("Bone Army", 5, 0.0f);
                break;

                case 1:
                Add("Darkness", 24, 0.1f);
                Add("Suppression", 48, 1.0f);
                Add("Bone Army", 6, 0.0f);
                break;

                case 2:
                Add("Darkness", 5, 0.0f);
                Add("Suppression", 47, 1.0f);
                Add("Bone Army", 26, 0.1f);
                break;

                case 3:
                Add("Darkness", 39, 0.5f);
                Add("Suppression", 37, 0.8f);
                Add("Bone Army", 4, 0.0f);
                break;

                case 4:
                Add("Darkness", 50, 1.0f);
                Add("Suppression", 20, 0.1f);
                Add("Bone Army", 4, 0.0f);
                break;

                case 5:
                Add("Darkness", 6, 0.0f);
                Add("Suppression", 24, 0.1f);
                Add("Bone Army", 48, 1.0f);
                break;
            }
        }
    }
}