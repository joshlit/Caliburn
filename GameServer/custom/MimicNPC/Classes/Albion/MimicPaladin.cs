using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicPaladin : MimicNPC
    {
        public MimicPaladin(byte level) : base(new ClassPaladin(), level)
        {
            MimicSpec = new PaladinSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeTwo, eHand.twoHand, MimicSpec.DamageType);

            if (!MimicSpec.is2H)
                SwitchWeapon(eActiveWeaponSlot.Standard);
            else
                SwitchWeapon(eActiveWeaponSlot.TwoHanded);

            RefreshSpecDependantSkills(false);
            SetSpells();
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class PaladinSpec : MimicSpec
    {
        public PaladinSpec()
        {
            SpecName = "PaladinSpec";

            int randBaseWeap = Util.Random(2);

            switch (randBaseWeap)
            {
                case 0:
                WeaponTypeOne = eObjectType.SlashingWeapon;
                DamageType = eWeaponDamageType.Slash;
                break;

                case 1:
                WeaponTypeOne = eObjectType.ThrustWeapon;
                DamageType = eWeaponDamageType.Thrust;
                break;

                case 2:
                WeaponTypeOne = eObjectType.CrushingWeapon;
                DamageType = eWeaponDamageType.Crush;
                break;
            }

            WeaponTypeTwo = eObjectType.TwoHandedWeapon;

            int randVariance = Util.Random(7);

            switch (randVariance)
            {
                case 0:
                is2H = true;
                Add(ObjToSpec(WeaponTypeOne), 39, 0.6f);
                Add(ObjToSpec(WeaponTypeTwo), 44, 0.8f);
                Add(Specs.Chants, 48, 0.9f);
                Add(Specs.Parry, 19, 0.2f);
                break;

                case 1:
                is2H = true;
                Add(ObjToSpec(WeaponTypeOne), 34, 0.6f);
                Add(ObjToSpec(WeaponTypeTwo), 50, 0.8f);
                Add(Specs.Chants, 48, 0.9f);
                Add(Specs.Parry, 13, 0.2f);
                break;

                case 2:
                case 3:
                Add(ObjToSpec(WeaponTypeOne), 39, 0.6f);
                Add(Specs.Chants, 50, 1.0f);
                Add(Specs.Shields, 42, 0.7f);
                Add(Specs.Parry, 18, 0.1f);
                break;

                case 4:
                Add(ObjToSpec(WeaponTypeOne), 29, 0.6f);
                Add(Specs.Chants, 46, 1.0f);
                Add(Specs.Shields, 50, 0.7f);
                Add(Specs.Parry, 25, 0.1f);
                break;

                case 5:
                Add(ObjToSpec(WeaponTypeOne), 29, 0.6f);
                Add(Specs.Chants, 46, 1.0f);
                Add(Specs.Shields, 42, 0.7f);
                Add(Specs.Parry, 25, 0.1f);
                break;

                case 6:
                case 7:
                Add(ObjToSpec(WeaponTypeOne), 39, 0.6f);
                Add(Specs.Chants, 48, 1.0f);
                Add(Specs.Shields, 42, 0.7f);
                Add(Specs.Parry, 23, 0.1f);
                break;
            }
        }
    }
}