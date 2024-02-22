using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicRunemaster : MimicNPC
    {
        public MimicRunemaster(byte level) : base(new ClassRunemaster(), level)
        {
            MimicSpec = new RunemasterSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.twoHand);
            SwitchWeapon(eActiveWeaponSlot.TwoHanded);
            RefreshSpecDependantSkills(false);
            SetCasterSpells();
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class RunemasterSpec : MimicSpec
    {
        public RunemasterSpec()
        {
            SpecName = "RunemasterSpec";

            WeaponTypeOne = eObjectType.Staff;

            int randVariance = Util.Random(7);
            
            switch (randVariance)
            {
                case 0:
                case 1:
                Add(Specs.Darkness, 47, 1.0f);
                Add(Specs.Suppression, 26, 0.1f);
                Add(Specs.Runecarving, 5, 0.0f);
                break;

                case 2:
                case 3:
                Add(Specs.Darkness, 24, 0.5f);
                Add(Specs.Suppression, 6, 0.1f);
                Add(Specs.Runecarving, 48, 1.0f);
                break;

                case 4:
                Add(Specs.Darkness, 5, 0.0f);
                Add(Specs.Suppression, 26, 0.1f);
                Add(Specs.Runecarving, 47, 1.0f);
                break;

                case 5:
                case 6:
                Add(Specs.Darkness, 20, 0.1f);
                Add(Specs.Suppression, 50, 1.0f);
                Add(Specs.Runecarving, 4, 0.0f);
                break;

                case 7:
                Add(Specs.Darkness, 31, 0.1f);
                Add(Specs.Suppression, 44, 1.0f);
                Add(Specs.Runecarving, 4, 0.0f);
                break;
            }
        }
    }
}