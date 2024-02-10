using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicEldritch : MimicNPC
    {
        public MimicEldritch(byte level) : base(new ClassEldritch(), level)
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

    public class SunEldritch : MimicSpec
    {
        public SunEldritch()
        {
            SpecName = "SunEldritch";

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(3);

            switch (randVariance)
            {
                case 0:
                Add("Light", 45, 1.0f);
                Add("Void", 29, 0.1f);
                Add("Mana", 6, 0.0f);
                break;

                case 1:
                Add("Light", 40, 1.0f);
                Add("Void", 35, 0.1f);
                Add("Mana", 6, 0.0f);
                break;

                case 2:
                Add("Light", 47, 1.0f);
                Add("Void", 26, 0.1f);
                break;

                case 3:
                Add("Light", 45, 1.0f);
                Add("Mana", 29, 0.1f);
                break;
            }
        }
    }

    public class ManaEldritch : MimicSpec
    {
        public ManaEldritch()
        {
            SpecName = "ManaEldritch";

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(3);

            switch (randVariance)
            {
                case 0:
                Add("Mana", 50, 1.0f);
                Add("Light", 20, 0.1f);
                break;

                case 1:
                Add("Mana", 50, 1.0f);
                Add("Void", 20, 0.1f);
                break;

                case 2:
                Add("Mana", 48, 1.0f);
                Add("Light", 24, 0.1f);
                break;

                case 3:
                Add("Mana", 48, 1.0f);
                Add("Void", 24, 0.1f);
                break;
            }
        }
    }

    public class VoidEldritch : MimicSpec
    {
        public VoidEldritch()
        {
            SpecName = "VoidEldritch";

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(3);

            switch (randVariance)
            {
                case 0:
                Add("Void", 49, 1.0f);
                Add("Light", 19, 0.1f);
                Add("Mana", 12, 0.0f);
                break;

                case 1:
                Add("Void", 48, 1.0f);
                Add("Light", 24, 0.1f);
                Add("Mana", 6, 0.0f);
                break;

                case 2:
                Add("Void", 46, 1.0f);
                Add("Light", 28, 0.1f);
                break;

                case 3:
                Add("Void", 46, 1.0f);
                Add("Mana", 28, 0.1f);
                break;
            }
        }
    }

    public class HybridEldritch : MimicSpec
    {
        public HybridEldritch()
        {
            SpecName = "HybridEldritch";

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(3);

            switch (randVariance)
            {
                case 0:
                Add("Void", 27, 1.0f);
                Add("Light", 24, 0.1f);
                Add("Mana", 39, 0.0f);
                break;

                case 1:
                Add("Void", 48, 1.0f);
                Add("Light", 24, 0.1f);
                Add("Mana", 6, 0.0f);
                break;

                case 2:
                Add("Void", 46, 1.0f);
                Add("Light", 28, 0.1f);
                break;

                case 3:
                Add("Void", 46, 1.0f);
                Add("Mana", 28, 0.1f);
                break;
            }
        }
    }
}