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

            int shieldSize = 1;

            if (level >= 10)
                shieldSize = 3;
            else if (level >= 5)
                shieldSize = 2;

            MimicEquipment.SetShield(this, shieldSize);

            eObjectType objectType = eObjectType.Studded;

            if (level >= 15)
            {
                objectType = eObjectType.Plate;
                MimicEquipment.SetRangedWeapon(this, eObjectType.Crossbow);
            }
            else if (level >= 5)
                objectType = eObjectType.Chain;

            MimicEquipment.SetArmor(this, objectType);
            MimicEquipment.SetJewelryROG(this, Realm, (eCharacterClass)CharacterClass.ID, Level, eObjectType.Magical);

            RefreshItemBonuses();

            if (MimicSpec.is2H)
                SwitchWeapon(eActiveWeaponSlot.TwoHanded);
            else
                SwitchWeapon(eActiveWeaponSlot.Standard);

            RefreshSpecDependantSkills(false);
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class ArmsmanSpec : MimicSpec
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ArmsmanSpec()
        {
            SpecName = "ArmsmanSpec";

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

            bool rand2H = Util.RandomBool();

            if (rand2H)
                WeaponTypeTwo = "Polearm";
            else
                WeaponTypeTwo = "Two Handed";

            int randVariance = Util.Random(5);

            switch (randVariance)
            {
                case 0:
                Add(WeaponTypeOne, 50, 1.0f);
                Add("Shields", 50, 1.0f);
                Add("Parry", 18, 0.2f);
                Add("Crossbows", 25, 0.1f);
                break;

                case 1:
                Add(WeaponTypeOne, 44, 1.0f);
                Add("Shields", 50, 1.0f);
                Add("Parry", 32, 0.2f);
                Add("Crossbows", 25, 0.1f);
                break;

                case 2:
                is2H = true;
                Add(WeaponTypeOne, 50, 0.9f);
                Add(WeaponTypeTwo, 39, 0.6f);
                Add("Shields", 42, 0.8f);
                Add("Parry", 18, 0.1f);
                break;

                case 3:
                case 4:
                is2H = true;
                Add(WeaponTypeOne, 39, 0.6f);
                Add(WeaponTypeTwo, 50, 1.0f);
                Add("Shields", 42, 0.5f);
                Add("Parry", 18, 0.1f);
                Add("Crossbows", 3, 0.0f);
                break;

                case 5:
                is2H = true;
                Add(WeaponTypeOne, 50, 1.0f);
                Add(WeaponTypeTwo, 50, 1.0f);
                Add("Parry", 22, 0.2f);
                Add("Crossbows", 25, 0.1f);
                break;
            }
        }
    }
}