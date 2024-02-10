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
            MimicEquipment.SetArmor(this, eObjectType.Leather);
            MimicEquipment.SetJewelryROG(this, Realm, (eCharacterClass)CharacterClass.ID, Level, eObjectType.Magical);
            RefreshItemBonuses();
            SwitchWeapon(eActiveWeaponSlot.TwoHanded);
            RefreshSpecDependantSkills(false);
            GetTauntStyles();
            SetSpells();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class FriarSpec : MimicSpec
    {
        public FriarSpec()
        {
            SpecName = "FriarSpec";

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(4);

            switch (randVariance)
            {
                case 0:
                case 1:
                Add("Rejuvenation", 18, 0.5f);
                Add("Enhancement", 46, 0.8f);
                Add("Staff", 50, 0.9f);
                Add("Parry", 16, 0.1f);
                break;

                case 2:
                Add("Rejuvenation", 10, 0.1f);
                Add("Enhancement", 50, 0.8f);
                Add("Staff", 50, 0.9f);
                Add("Parry", 10, 0.1f);
                break;

                case 3:
                Add("Rejuvenation", 44, 0.8f);
                Add("Enhancement", 45, 0.9f);
                Add("Staff", 34, 0.6f);
                Add("Parry", 8, 0.1f);
                break;

                case 4:
                Add("Rejuvenation", 15, 0.1f);
                Add("Enhancement", 50, 0.9f);
                Add("Staff", 44, 0.8f);
                Add("Parry", 23, 0.2f);
                break;
            }
        }
    }
}