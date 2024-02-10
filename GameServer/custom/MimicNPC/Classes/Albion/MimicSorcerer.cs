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
            MimicEquipment.SetArmor(this, eObjectType.Cloth);
            MimicEquipment.SetJewelryROG(this, Realm, (eCharacterClass)CharacterClass.ID, Level, eObjectType.Magical);
            RefreshItemBonuses();
            SwitchWeapon(eActiveWeaponSlot.TwoHanded);
            RefreshSpecDependantSkills(false);
            SetCasterSpells();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class SorcererSpec : MimicSpec
    {
        public SorcererSpec()
        {
            SpecName = "SorcererSpec";

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(4);

            switch (randVariance)
            {
                case 0:
                case 1:
                Add("Matter Magic", 8, 0.0f);
                Add("Body Magic", 30, 0.6f);
                Add("Mind Magic", 44, 0.8f);
                break;

                case 2:
                Add("Matter Magic", 26, 0.5f);
                Add("Body Magic", 5, 0.0f);
                Add("Mind Magic", 47, 0.8f);
                break;

                case 3:
                Add("Matter Magic", 5, 0.0f);
                Add("Body Magic", 22, 0.6f);
                Add("Mind Magic", 49, 0.8f);
                break;

                case 4:
                Add("Matter Magic", 8, 0.0f);
                Add("Body Magic", 40, 0.6f);
                Add("Mind Magic", 36, 0.8f);
                break;

                case 5:
                Add("Matter Magic", 24, 0.6f);
                Add("Body Magic", 48, 0.8f);
                Add("Mind Magic", 6, 0.1f);
                break;

                case 6:
                Add("Matter Magic", 6, 0.0f);
                Add("Body Magic", 45, 0.8f);
                Add("Mind Magic", 29, 0.6f);
                break;
            }
        }
    }
}