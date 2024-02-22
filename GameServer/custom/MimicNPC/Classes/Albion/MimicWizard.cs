using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicWizard : MimicNPC
    {
        public MimicWizard(byte level) : base(new ClassWizard(), level)
        {
            MimicSpec = new WizardSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.twoHand);
            SwitchWeapon(eActiveWeaponSlot.TwoHanded);
            RefreshSpecDependantSkills(false);
            SetCasterSpells();
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class WizardSpec : MimicSpec
    {
        public WizardSpec()
        {
            SpecName = "WizardSpec";

            WeaponTypeOne = eObjectType.Staff;

            int randVariance = Util.Random(2);
            
            switch (randVariance)
            {
                case 0:
                Add(Specs.Earth_Magic, 50, 1.0f);
                Add(Specs.Cold_Magic, 20, 0.1f);
                break;

                case 1:
                Add(Specs.Earth_Magic, 24, 0.1f);
                Add(Specs.Cold_Magic, 48, 1.0f);
                break;

                case 2:
                Add(Specs.Cold_Magic, 20, 0.1f);
                Add(Specs.Fire_Magic, 50, 1.0f);
                break;
            }
        }
    }
}