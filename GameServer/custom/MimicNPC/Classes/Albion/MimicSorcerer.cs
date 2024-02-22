using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicSorcerer : MimicNPC
    {
        public MimicSorcerer(byte level) : base(new ClassSorcerer(), level)
        {
            MimicSpec = new SorcererSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.twoHand);
            SwitchWeapon(eActiveWeaponSlot.TwoHanded);
            RefreshSpecDependantSkills(false);
            SetCasterSpells();
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class SorcererSpec : MimicSpec
    {
        public SorcererSpec()
        {
            SpecName = "SorcererSpec";

            WeaponTypeOne = eObjectType.Staff;

            int randVariance = Util.Random(4);
                    
            switch (randVariance)
            {
                case 0:
                case 1:
                Add(Specs.Matter_Magic, 8, 0.0f);
                Add(Specs.Body_Magic, 30, 0.6f);
                Add(Specs.Mind_Magic, 44, 0.8f);
                break;

                case 2:
                Add(Specs.Matter_Magic, 26, 0.5f);
                Add(Specs.Body_Magic, 5, 0.0f);
                Add(Specs.Mind_Magic, 47, 0.8f);
                break;

                case 3:
                Add(Specs.Matter_Magic, 5, 0.0f);
                Add(Specs.Body_Magic, 22, 0.6f);
                Add(Specs.Mind_Magic, 49, 0.8f);
                break;

                case 4:
                Add(Specs.Matter_Magic, 8, 0.0f);
                Add(Specs.Body_Magic, 40, 0.6f);
                Add(Specs.Mind_Magic, 36, 0.8f);
                break;

                case 5:
                Add(Specs.Matter_Magic, 24, 0.6f);
                Add(Specs.Body_Magic, 48, 0.8f);
                Add(Specs.Mind_Magic, 6, 0.1f);
                break;

                case 6:
                Add(Specs.Matter_Magic, 6, 0.0f);
                Add(Specs.Body_Magic, 45, 0.8f);
                Add(Specs.Mind_Magic, 29, 0.6f);
                break;
            }
        }
    }
}