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

            int shieldSize = 1;

            if (level >= 10)
                shieldSize = 3;
            else if (level >= 5)
                shieldSize = 2;

            MimicEquipment.SetShield(this, shieldSize);

            eObjectType objectType = eObjectType.Studded;

            if (level >= 20)
                objectType = eObjectType.Plate;
            else if (level >= 10)
                objectType = eObjectType.Chain;

            MimicEquipment.SetArmor(this, objectType);
            MimicEquipment.SetJewelryROG(this, Realm, (eCharacterClass)CharacterClass.ID, Level, eObjectType.Magical);
            RefreshItemBonuses();

            if (!MimicSpec.is2H)
                SwitchWeapon(eActiveWeaponSlot.Standard);
            else
                SwitchWeapon(eActiveWeaponSlot.TwoHanded);

            RefreshSpecDependantSkills(false);
            GetTauntStyles();
            SetSpells();
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
                WeaponTypeOne = "Slash";
                DamageType = eWeaponDamageType.Slash;
                break;

                case 1:
                WeaponTypeOne = "Thrust";
                DamageType = eWeaponDamageType.Thrust;
                break;

                case 2:
                WeaponTypeOne = "Crush";
                DamageType = eWeaponDamageType.Crush;
                break;
            }

            WeaponTypeTwo = "Two Handed";

            int randVariance = Util.Random(7);

            switch (randVariance)
            {
                case 0:
                is2H = true;
                Add(WeaponTypeOne, 39, 0.6f);
                Add(WeaponTypeTwo, 44, 0.8f);
                Add("Chants", 48, 0.9f);
                Add("Parry", 19, 0.2f);
                break;

                case 1:
                is2H = true;
                Add(WeaponTypeOne, 34, 0.6f);
                Add(WeaponTypeTwo, 50, 0.8f);
                Add("Chants", 48, 0.9f);
                Add("Parry", 13, 0.2f);
                break;

                case 2:
                case 3:
                Add(WeaponTypeOne, 39, 0.6f);
                Add("Chants", 50, 1.0f);
                Add("Shields", 42, 0.7f);
                Add("Parry", 18, 0.1f);
                break;

                case 4:
                Add(WeaponTypeOne, 29, 0.6f);
                Add("Chants", 46, 1.0f);
                Add("Shields", 50, 0.7f);
                Add("Parry", 25, 0.1f);
                break;

                case 5:
                Add(WeaponTypeOne, 29, 0.6f);
                Add("Chants", 46, 1.0f);
                Add("Shields", 42, 0.7f);
                Add("Parry", 25, 0.1f);
                break;

                case 6:
                case 7:
                Add(WeaponTypeOne, 39, 0.6f);
                Add("Chants", 48, 1.0f);
                Add("Shields", 42, 0.7f);
                Add("Parry", 23, 0.1f);
                break;
            }
        }
    }
}