using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicEnchanter : MimicNPC
    {
        public MimicEnchanter(byte level) : base(new ClassEnchanter(), level)
        {
            MimicSpec = MimicManager.Random(this);

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

    public class ManaEnchanter : MimicSpec
    {
        public ManaEnchanter()
        {
            SpecName = "ManaEnchanter";

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(1);

            switch (randVariance)
            {
                case 0:
                Add("Mana", 50, 1.0f);
                Add("Light", 20, 0.1f);
                Add("Enchantments", 4, 0.0f);
                break;

                case 1:
                Add("Mana", 49, 1.0f);
                Add("Light", 22, 0.1f);
                Add("Enchantments", 5, 0.0f);
                break;
            }
        }
    }

    public class LightEnchanter : MimicSpec
    {
        public LightEnchanter()
        {
            SpecName = "LightEnchanter";

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(1);

            switch (randVariance)
            {
                case 0:
                Add("Mana", 27, 0.2f);
                Add("Light", 45, 1.0f);
                Add("Enchantments", 12, 0.1f);
                break;

                case 1:
                Add("Mana", 24, 0.2f);
                Add("Light", 45, 1.0f);
                Add("Enchantments", 17, 0.1f);
                break;
            }
        }
    }
}