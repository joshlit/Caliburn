using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicFriar : MimicNPC
    {
        public MimicFriar(byte level) : base(new ClassFriar(), level)
        {
            MimicSpec = new FriarSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.twoHand);

            SwitchWeapon(eActiveWeaponSlot.TwoHanded);
            RefreshSpecDependantSkills(false);
            SetSpells();
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class FriarSpec : MimicSpec
    {
        public FriarSpec()
        {
            SpecName = "FriarSpec";

            WeaponTypeOne = eObjectType.Staff;

            int randVariance = Util.Random(6);

            switch (randVariance)
            {
                case 0:
                Add(Specs.Rejuvenation, 15, 0.2f);
                Add(Specs.Enhancement, 35, 0.8f);
                Add(Specs.Staff, 50, 0.9f);
                Add(Specs.Parry, 19, 0.1f);
                break;

                case 1:
                Add(Specs.Rejuvenation, 2, 0.0f);
                Add(Specs.Enhancement, 42, 0.8f);
                Add(Specs.Staff, 50, 0.9f);
                Add(Specs.Parry, 9, 0.1f);
                break;

                case 2:
                Add(Specs.Rejuvenation, 7, 0.2f);
                Add(Specs.Enhancement, 45, 0.8f);
                Add(Specs.Staff, 44, 0.9f);
                Add(Specs.Parry, 18, 0.2f);
                break;

                case 3:
                Add(Specs.Rejuvenation, 24, 0.1f);
                Add(Specs.Enhancement, 45, 0.9f);
                Add(Specs.Staff, 39, 0.8f);
                Add(Specs.Parry, 14, 0.2f);
                break;

                case 4:
                Add(Specs.Rejuvenation, 15, 0.1f);
                Add(Specs.Enhancement, 45, 0.9f);
                Add(Specs.Staff, 39, 0.8f);
                Add(Specs.Parry, 23, 0.2f);
                break;

                case 5:
                Add(Specs.Rejuvenation, 44, 0.9f);
                Add(Specs.Enhancement, 37, 0.8f);
                Add(Specs.Staff, 29, 0.2f);
                Add(Specs.Parry, 13, 0.1f);
                break;

                case 6:
                Add(Specs.Rejuvenation, 34, 0.8f);
                Add(Specs.Enhancement, 37, 0.5f);
                Add(Specs.Staff, 39, 0.3f);
                Add(Specs.Parry, 16, 0.1f);
                break;
            }
        }
    }
}