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
            SwitchWeapon(eActiveWeaponSlot.TwoHanded);
            RefreshSpecDependantSkills(false);
            SetCasterSpells();
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class SunEldritch : MimicSpec
    {
        public SunEldritch()
        {
            SpecName = "SunEldritch";

            WeaponTypeOne = eObjectType.Staff;

            int randVariance = Util.Random(3);

            switch (randVariance)
            {
                case 0:
                Add(Specs.Light, 45, 1.0f);
                Add(Specs.Void, 29, 0.1f);
                Add(Specs.Mana, 6, 0.0f);
                break;

                case 1:
                Add(Specs.Light, 40, 1.0f);
                Add(Specs.Void, 35, 0.1f);
                Add(Specs.Mana, 6, 0.0f);
                break;

                case 2:
                Add(Specs.Light, 47, 1.0f);
                Add(Specs.Void, 26, 0.1f);
                break;

                case 3:
                Add(Specs.Light, 45, 1.0f);
                Add(Specs.Mana, 29, 0.1f);
                break;
            }
        }
    }

    public class ManaEldritch : MimicSpec
    {
        public ManaEldritch()
        {
            SpecName = "ManaEldritch";

            WeaponTypeOne = eObjectType.Staff;

            int randVariance = Util.Random(3);

            switch (randVariance)
            {
                case 0:
                Add(Specs.Mana, 50, 1.0f);
                Add(Specs.Light, 20, 0.1f);
                break;

                case 1:
                Add(Specs.Mana, 50, 1.0f);
                Add(Specs.Void, 20, 0.1f);
                break;

                case 2:
                Add(Specs.Mana, 48, 1.0f);
                Add(Specs.Light, 24, 0.1f);
                break;

                case 3:
                Add(Specs.Mana, 48, 1.0f);
                Add(Specs.Void, 24, 0.1f);
                break;
            }
        }
    }

    public class VoidEldritch : MimicSpec
    {
        public VoidEldritch()
        {
            SpecName = "VoidEldritch";

            WeaponTypeOne = eObjectType.Staff;

            int randVariance = Util.Random(3);

            switch (randVariance)
            {
                case 0:
                Add(Specs.Void, 49, 1.0f);
                Add(Specs.Light, 19, 0.1f);
                Add(Specs.Mana, 12, 0.0f);
                break;

                case 1:
                Add(Specs.Void, 48, 1.0f);
                Add(Specs.Light, 24, 0.1f);
                Add(Specs.Mana, 6, 0.0f);
                break;

                case 2:
                Add(Specs.Void, 46, 1.0f);
                Add(Specs.Light, 28, 0.1f);
                break;

                case 3:
                Add(Specs.Void, 46, 1.0f);
                Add(Specs.Mana, 28, 0.1f);
                break;
            }
        }
    }

    public class HybridEldritch : MimicSpec
    {
        public HybridEldritch()
        {
            SpecName = "HybridEldritch";

            WeaponTypeOne = eObjectType.Staff;

            int randVariance = Util.Random(3);

            switch (randVariance)
            {
                case 0:
                Add(Specs.Void, 27, 1.0f);
                Add(Specs.Light, 24, 0.1f);
                Add(Specs.Mana, 39, 0.0f);
                break;

                case 1:
                Add(Specs.Void, 48, 1.0f);
                Add(Specs.Light, 24, 0.1f);
                Add(Specs.Mana, 6, 0.0f);
                break;

                case 2:
                Add(Specs.Void, 46, 1.0f);
                Add(Specs.Light, 28, 0.1f);
                break;

                case 3:
                Add(Specs.Void, 46, 1.0f);
                Add(Specs.Mana, 28, 0.1f);
                break;
            }
        }
    }
}