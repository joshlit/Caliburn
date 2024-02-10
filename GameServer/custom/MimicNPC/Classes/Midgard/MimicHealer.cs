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
            MimicEquipment.SetShield(this, 1);

            eObjectType objectType = eObjectType.Leather;

            if (level >= 20)
                objectType = eObjectType.Chain;
            else if (level >= 10)
                objectType = eObjectType.Studded;

            MimicEquipment.SetArmor(this, objectType);
            MimicEquipment.SetJewelryROG(this, Realm, (eCharacterClass)CharacterClass.ID, Level, eObjectType.Magical);
            RefreshItemBonuses();
            SwitchWeapon(eActiveWeaponSlot.Standard);
            RefreshSpecDependantSkills(false);
            SetSpells();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class PacHealer : MimicSpec
    {
        public PacHealer()
        {
            SpecName = "PacHealer";

            WeaponTypeOne = "Hammer";

            int randVariance = Util.Random(3);

            switch (randVariance)
            {
                case 0:
                case 1:
                Add("Mending", 31, 0.5f);
                Add("Augmentation", 4, 0.1f);
                Add("Pacification", 44, 0.8f);
                break;

                case 2:
                Add("Mending", 40, 0.5f);
                Add("Augmentation", 4, 0.1f);
                Add("Pacification", 36, 0.8f);
                break;

                case 3:
                Add("Mending", 33, 0.5f);
                Add("Augmentation", 19, 0.1f);
                Add("Pacification", 38, 0.8f);
                break;
            }
        }
    }

    public class AugHealer : MimicSpec
    {
        public AugHealer()
        {
            SpecName = "AugHealer";

            WeaponTypeOne = "Hammer";

            int randVariance = Util.Random(5);

            switch (randVariance)
            {
                case 0:
                case 1:
                Add("Mending", 31, 0.5f);
                Add("Augmentation", 44, 0.8f);
                Add("Pacification", 4, 0.2f);
                break;

                case 2:
                Add("Mending", 39, 0.5f);
                Add("Augmentation", 37, 0.8f);
                Add("Pacification", 4, 0.1f);
                break;

                case 3:
                Add("Mending", 40, 0.5f);
                Add("Augmentation", 36, 0.8f);
                Add("Pacification", 4, 0.1f);
                break;

                case 4:
                Add("Mending", 20, 0.5f);
                Add("Augmentation", 50, 0.8f);
                Add("Pacification", 4, 0.1f);
                break;

                case 5:
                Add("Mending", 42, 0.8f);
                Add("Augmentation", 33, 0.5f);
                Add("Pacification", 7, 0.2f);
                break;
            }
        }
    }
}