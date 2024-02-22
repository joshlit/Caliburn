using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicMentalist : MimicNPC
    {
        public MimicMentalist(byte level) : base(new ClassMentalist(), level)
        {
            MimicSpec = MimicManager.Random(this);

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.twoHand);
            SwitchWeapon(eActiveWeaponSlot.TwoHanded);
            RefreshSpecDependantSkills(false);
            SetCasterSpells();
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class LightMentalist : MimicSpec
    {
        public LightMentalist()
        {
            SpecName = "LightMentalist";

            WeaponTypeOne = eObjectType.Staff;

            int randVariance = Util.Random(4);
            
            switch (randVariance)
            {
                case 0:
                Add(Specs.Light, 45, 1.0f);
                Add(Specs.Mentalism, 28, 0.1f);
                Add(Specs.Mana, 10, 0.0f);
                break;

                case 1:
                Add(Specs.Light, 45, 1.0f);
                Add(Specs.Mentalism, 17, 0.0f);
                Add(Specs.Mana, 24, 0.1f);
                break;

                case 2:
                Add(Specs.Light, 45, 1.0f);
                Add(Specs.Mentalism, 6, 0.0f);
                Add(Specs.Mana, 29, 0.1f);
                break;

                case 3:
                Add(Specs.Light, 42, 1.0f);
                Add(Specs.Mentalism, 33, 0.1f);
                Add(Specs.Mana, 7, 0.0f);
                break;

                case 4:
                Add(Specs.Light, 42, 1.0f);
                Add(Specs.Mentalism, 23, 0.2f);
                Add(Specs.Mana, 24, 0.1f);
                break;
            }
        }
    }

    public class ManaMentalist : MimicSpec
    {
        public ManaMentalist()
        {
            SpecName = "ManaMentalist";

            WeaponTypeOne = eObjectType.Staff;

            int randVariance = Util.Random(4);

            switch (randVariance)
            {
                case 0:
                Add(Specs.Mana, 48, 1.0f);
                Add(Specs.Mentalism, 24, 0.1f);
                Add(Specs.Light, 6, 0.0f);
                break;

                case 1:
                Add(Specs.Mana, 48, 1.0f);
                Add(Specs.Mentalism, 6, 0.0f);
                Add(Specs.Light, 24, 0.1f);
                break;

                case 2:
                Add(Specs.Mana, 46, 1.0f);
                Add(Specs.Mentalism, 28, 0.1f);
                Add(Specs.Light, 4, 0.0f);
                break;

                case 3:
                Add(Specs.Mana, 44, 1.0f);
                Add(Specs.Mentalism, 31, 0.1f);
                Add(Specs.Light, 4, 0.0f);
                break;

                case 4:
                Add(Specs.Mana, 44, 1.0f);
                Add(Specs.Mentalism, 4, 0.0f);
                Add(Specs.Light, 31, 0.1f);
                break;
            }
        }
    }

    public class MentalismMentalist : MimicSpec
    {
        public MentalismMentalist()
        {
            SpecName = "MentalismMentalist";

            WeaponTypeOne = eObjectType.Staff;

            int randVariance = Util.Random(3);

            switch (randVariance)
            {
                case 0:
                Add(Specs.Mentalism, 50, 1.0f);
                Add(Specs.Mana, 4, 0.0f);
                Add(Specs.Light, 20, 0.1f);
                break;

                case 1:
                Add(Specs.Mentalism, 50, 1.0f);
                Add(Specs.Mana, 14, 0.1f);
                Add(Specs.Light, 14, 0.0f);
                break;

                case 2:
                Add(Specs.Mentalism, 42, 1.0f);
                Add(Specs.Mana, 7, 0.0f);
                Add(Specs.Light, 33, 0.1f);
                break;

                case 3:
                Add(Specs.Mentalism, 41, 0.9f);
                Add(Specs.Mana, 34, 0.1f);
                Add(Specs.Light, 8, 0.0f);
                break;
            }
        }
    }
}