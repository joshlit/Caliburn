using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicHero : MimicNPC
    {
        public MimicHero(byte level) : base(new ClassHero(), level)
        {
            MimicSpec = new HeroSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeTwo, eHand.twoHand);

            if (level >= 15)
                MimicEquipment.SetRangedWeapon(this, eObjectType.Fired);

            if (MimicSpec.is2H)
                SwitchWeapon(eActiveWeaponSlot.TwoHanded);
            else
                SwitchWeapon(eActiveWeaponSlot.Standard);

            RefreshSpecDependantSkills(false);
            GetTauntStyles();
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class HeroSpec : MimicSpec
    {
        public HeroSpec()
        {
            SpecName = "HeroSpec";
            is2H = false;

            int randBaseWeap = Util.Random(2);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = eObjectType.Blades; break;
                case 1: WeaponTypeOne = eObjectType.Piercing; break;
                case 2: WeaponTypeOne = eObjectType.Blunt; break;
            }

            DamageType = 0;

            if (Util.RandomBool())
                WeaponTypeTwo = eObjectType.CelticSpear;
            else
                WeaponTypeTwo = eObjectType.LargeWeapons;

            int randVariance = Util.Random(4);

            switch (randVariance)
            {
                case 0:
                Add(ObjToSpec(WeaponTypeOne), 50, 1.0f);
                Add(Specs.Shields, 50, 0.9f);
                Add(Specs.Parry, 28, 0.1f);
                break;

                case 1:
                case 2:
                is2H = true;
                Add(ObjToSpec(WeaponTypeOne), 39, 0.8f);
                Add(ObjToSpec(WeaponTypeTwo), 50, 1.0f);
                Add(Specs.Shields, 42, 0.5f);
                Add(Specs.Parry, 6, 0.1f);
                break;

                case 3:
                case 4:
                is2H = true;
                Add(ObjToSpec(WeaponTypeOne), 39, 0.8f);
                Add(ObjToSpec(WeaponTypeTwo), 44, 1.0f);
                Add(Specs.Shields, 42, 0.5f);
                Add(Specs.Parry, 24, 0.1f);
                break;
            }
        }
    }
}