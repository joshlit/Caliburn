using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicShaman : MimicNPC
    {
        public MimicShaman(byte level) : base(new ClassShaman(), level)
        {
            MimicSpec = new ShamanSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);        
            SwitchWeapon(eActiveWeaponSlot.Standard);
            RefreshSpecDependantSkills(false);
            SetSpells();
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class ShamanSpec : MimicSpec
    {
        public ShamanSpec()
        {
            SpecName = "ShamanSpec";

            WeaponTypeOne = eObjectType.Hammer;

            int randVariance = Util.Random(7);
            
            switch (randVariance)
            {
                case 0:
                case 1:
                Add(Specs.Mending, 8, 0.2f);
                Add(Specs.Augmentation, 46, 0.8f);
                Add(Specs.Subterranean, 27, 0.5f);
                break;

                case 2:
                Add(Specs.Mending, 26, 0.5f);
                Add(Specs.Augmentation, 47, 0.8f);
                Add(Specs.Subterranean, 5, 0.0f);
                break;

                case 3:
                Add(Specs.Mending, 33, 0.5f);
                Add(Specs.Augmentation, 42, 0.8f);
                Add(Specs.Subterranean, 7, 0.0f);
                break;

                case 4:
                Add(Specs.Mending, 21, 0.5f);
                Add(Specs.Augmentation, 48, 0.8f);
                Add(Specs.Subterranean, 12, 0.0f);
                break;

                case 5:
                Add(Specs.Mending, 4, 0.2f);
                Add(Specs.Augmentation, 28, 0.5f);
                Add(Specs.Subterranean, 46, 0.8f);
                break;

                case 6:
                Add(Specs.Mending, 27, 0.5f);
                Add(Specs.Augmentation, 8, 0.1f);
                Add(Specs.Subterranean, 46, 0.8f);
                break;

                case 7:
                Add(Specs.Mending, 43, 0.8f);
                Add(Specs.Augmentation, 32, 0.5f);
                Add(Specs.Subterranean, 6, 0.0f);
                break;
            }
        }
    }
}