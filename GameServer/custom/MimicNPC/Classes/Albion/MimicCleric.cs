using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicCleric : MimicNPC
    {
        public MimicCleric(byte level) : base(new ClassCleric(), level)
        {
            MimicSpec = new ClericSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);

            SwitchWeapon(eActiveWeaponSlot.Standard);
            RefreshSpecDependantSkills(false);
            SetSpells();
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class ClericSpec : MimicSpec
    {
        public ClericSpec()
        {
            SpecName = "ClericSpec";

            WeaponTypeOne = eObjectType.CrushingWeapon;

            int randVariance = Util.Random(7);

            switch (randVariance)
            {
                case 0:
                case 1:
                Add(Specs.Rejuvenation, 33, 0.5f);
                Add(Specs.Enhancement, 42, 0.8f);
                Add(Specs.Smite, 7, 0.0f);
                break;

                case 2:
                case 3:
                Add(Specs.Rejuvenation, 36, 0.5f);
                Add(Specs.Enhancement, 40, 0.8f);
                Add(Specs.Smite, 4, 0.0f);
                break;

                case 4:
                Add(Specs.Rejuvenation, 46, 0.8f);
                Add(Specs.Enhancement, 28, 0.5f);
                Add(Specs.Smite, 4, 0.0f);
                break;

                case 5:
                Add(Specs.Rejuvenation, 50, 0.8f);
                Add(Specs.Enhancement, 20, 0.5f);
                Add(Specs.Smite, 4, 0.0f);
                break;

                case 6:
                Add(Specs.Rejuvenation, 6, 0.0f);
                Add(Specs.Enhancement, 29, 0.5f);
                Add(Specs.Smite, 45, 0.8f);
                break;

                case 7:
                Add(Specs.Rejuvenation, 4, 0.0f);
                Add(Specs.Enhancement, 36, 0.5f);
                Add(Specs.Smite, 40, 0.8f);
                break;
            }
        }
    }
}