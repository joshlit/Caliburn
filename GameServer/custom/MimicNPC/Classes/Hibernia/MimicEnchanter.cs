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
            SwitchWeapon(eActiveWeaponSlot.TwoHanded);
            RefreshSpecDependantSkills(false);
            SetCasterSpells();
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class ManaEnchanter : MimicSpec
    {
        public ManaEnchanter()
        {
            SpecName = "ManaEnchanter";

            WeaponTypeOne = eObjectType.Staff;

            int randVariance = Util.Random(1);

            switch (randVariance)
            {
                case 0:
                Add(Specs.Mana, 50, 1.0f);
                Add(Specs.Light, 20, 0.1f);
                Add(Specs.Enchantments, 4, 0.0f);
                break;

                case 1:
                Add(Specs.Mana, 49, 1.0f);
                Add(Specs.Light, 22, 0.1f);
                Add(Specs.Enchantments, 5, 0.0f);
                break;
            }
        }
    }

    public class LightEnchanter : MimicSpec
    {
        public LightEnchanter()
        {
            SpecName = "LightEnchanter";

            WeaponTypeOne = eObjectType.Staff;

            int randVariance = Util.Random(1);

            switch (randVariance)
            {
                case 0:
                Add(Specs.Mana, 27, 0.2f);
                Add(Specs.Light, 45, 1.0f);
                Add(Specs.Enchantments, 12, 0.1f);
                break;

                case 1:
                Add(Specs.Mana, 24, 0.2f);
                Add(Specs.Light, 45, 1.0f);
                Add(Specs.Enchantments, 17, 0.1f);
                break;
            }
        }
    }
}