using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicHealer : MimicNPC
    {
        public MimicHealer(byte level) : base(new ClassHealer(), level)
        {
            MimicSpec = MimicManager.Random(this);

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);    
            SwitchWeapon(eActiveWeaponSlot.Standard);
            RefreshSpecDependantSkills(false);
            SetSpells();
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class PacHealer : MimicSpec
    {
        public PacHealer()
        {
            SpecName = "PacHealer";

            WeaponTypeOne = eObjectType.Hammer;

            int randVariance = Util.Random(3);
            
            switch (randVariance)
            {
                case 0:
                case 1:
                Add(Specs.Mending, 31, 0.5f);
                Add(Specs.Augmentation, 4, 0.1f);
                Add(Specs.Pacification, 44, 0.8f);
                break;

                case 2:
                Add(Specs.Mending, 40, 0.5f);
                Add(Specs.Augmentation, 4, 0.1f);
                Add(Specs.Pacification, 36, 0.8f);
                break;

                case 3:
                Add(Specs.Mending, 33, 0.5f);
                Add(Specs.Augmentation, 19, 0.1f);
                Add(Specs.Pacification, 38, 0.8f);
                break;
            }
        }
    }

    public class AugHealer : MimicSpec
    {
        public AugHealer()
        {
            SpecName = "AugHealer";

            WeaponTypeOne = eObjectType.Hammer;

            int randVariance = Util.Random(5);

            switch (randVariance)
            {
                case 0:
                case 1:
                Add(Specs.Mending, 31, 0.5f);
                Add(Specs.Augmentation, 44, 0.8f);
                Add(Specs.Pacification, 4, 0.2f);
                break;

                case 2:
                Add(Specs.Mending, 39, 0.5f);
                Add(Specs.Augmentation, 37, 0.8f);
                Add(Specs.Pacification, 4, 0.1f);
                break;

                case 3:
                Add(Specs.Mending, 40, 0.5f);
                Add(Specs.Augmentation, 36, 0.8f);
                Add(Specs.Pacification, 4, 0.1f);
                break;

                case 4:
                Add(Specs.Mending, 20, 0.5f);
                Add(Specs.Augmentation, 50, 0.8f);
                Add(Specs.Pacification, 4, 0.1f);
                break;

                case 5:
                Add(Specs.Mending, 42, 0.8f);
                Add(Specs.Augmentation, 33, 0.5f);
                Add(Specs.Pacification, 7, 0.2f);
                break;
            }
        }
    }
}