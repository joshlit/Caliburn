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

    public class ShamanSpec : MimicSpec
    {
        public ShamanSpec()
        {
            SpecName = "ShamanSpec";

            WeaponTypeOne = "Hammer";

            int randVariance = Util.Random(7);

            switch (randVariance)
            {
                case 0:
                case 1:
                Add("Mending", 8, 0.2f);
                Add("Augmentation", 46, 0.8f);
                Add("Subterranean", 27, 0.5f);
                break;

                case 2:
                Add("Mending", 26, 0.5f);
                Add("Augmentation", 47, 0.8f);
                Add("Subterranean", 5, 0.0f);
                break;

                case 3:
                Add("Mending", 33, 0.5f);
                Add("Augmentation", 42, 0.8f);
                Add("Subterranean", 7, 0.0f);
                break;

                case 4:
                Add("Mending", 21, 0.5f);
                Add("Augmentation", 48, 0.8f);
                Add("Subterranean", 12, 0.0f);
                break;

                case 5:
                Add("Mending", 4, 0.2f);
                Add("Augmentation", 28, 0.5f);
                Add("Subterranean", 46, 0.8f);
                break;

                case 6:
                Add("Mending", 27, 0.5f);
                Add("Augmentation", 8, 0.1f);
                Add("Subterranean", 46, 0.8f);
                break;

                case 7:
                Add("Mending", 43, 0.8f);
                Add("Augmentation", 32, 0.5f);
                Add("Subterranean", 6, 0.0f);
                break;
            }
        }
    }
}