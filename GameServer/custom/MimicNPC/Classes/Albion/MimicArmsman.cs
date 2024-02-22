using DOL.Database;
using DOL.GS.PlayerClass;
using log4net;
using System.Reflection;

namespace DOL.GS.Scripts
{
    public class MimicArmsman : MimicNPC
    {
        public MimicArmsman(byte level) : base(new ClassArmsman(), level)
        {
            MimicSpec = new ArmsmanSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeTwo, eHand.twoHand, MimicSpec.DamageType);

            if (level >= 15)
                MimicEquipment.SetRangedWeapon(this, eObjectType.Crossbow);

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

    public class ArmsmanSpec : MimicSpec
    {
        public ArmsmanSpec()
        {
            SpecName = "ArmsmanSpec";

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

            bool rand2H = Util.RandomBool();

            if (rand2H)
                WeaponTypeTwo = eObjectType.PolearmWeapon;
            else
                WeaponTypeTwo = eObjectType.TwoHandedWeapon;

            int randVariance = Util.Random(5);

            switch (randVariance)
            {
                case 0:
                Add(ObjToSpec(WeaponTypeOne), 50, 1.0f);
                Add(Specs.Shields, 50, 1.0f);
                Add(Specs.Parry, 18, 0.2f);
                Add(Specs.Crossbow, 25, 0.1f);
                break;

                case 1:
                Add(ObjToSpec(WeaponTypeOne), 44, 1.0f);
                Add(Specs.Shields, 50, 1.0f);
                Add(Specs.Parry, 32, 0.2f);
                Add(Specs.Crossbow, 25, 0.1f);
                break;

                case 2:
                is2H = true;
                Add(ObjToSpec(WeaponTypeOne), 50, 0.9f);
                Add(ObjToSpec(WeaponTypeTwo), 39, 0.6f);
                Add(Specs.Shields, 42, 0.8f);
                Add(Specs.Parry, 18, 0.1f);
                break;

                case 3:
                case 4:
                is2H = true;
                Add(ObjToSpec(WeaponTypeOne), 39, 0.6f);
                Add(ObjToSpec(WeaponTypeTwo), 50, 1.0f);
                Add(Specs.Shields, 42, 0.5f);
                Add(Specs.Parry, 18, 0.1f);
                Add(Specs.Crossbow, 3, 0.0f);
                break;

                case 5:
                is2H = true;
                Add(ObjToSpec(WeaponTypeOne), 50, 1.0f);
                Add(ObjToSpec(WeaponTypeTwo), 50, 1.0f);
                Add(Specs.Parry, 22, 0.2f);
                Add(Specs.Crossbow, 25, 0.1f);
                break;
            }
        }
    }
}