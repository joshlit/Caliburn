using DOL.GS.PlayerClass;
using log4net;
using System;
using System.Reflection;

namespace DOL.GS.Scripts
{
    public class MimicHero : MimicNPC
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MimicHero(byte level) : base(new ClassHero(), level)
        {
            MimicSpec = new HeroSpec();

            DistributeSkillPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeTwo, eHand.twoHand);
            MimicEquipment.SetShield(this, 3);

            eObjectType objectType;

            if (level < 15)
                objectType = eObjectType.Reinforced;
            else
                objectType = eObjectType.Scale;

            MimicEquipment.SetArmor(this, objectType);
            MimicEquipment.SetJewelry(this);
            RefreshItemBonuses();

            if (MimicSpec.is2H)
                SwitchWeapon(eActiveWeaponSlot.TwoHanded);
            else
                SwitchWeapon(eActiveWeaponSlot.Standard);

            RefreshSpecDependantSkills(false);
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class HeroSpec : MimicSpec
    {
        public HeroSpec()
        {
            SpecName = "HeroSpec";
            is2H = false;

            string weaponType = string.Empty;

            int randBaseWeap = Util.Random(2);

            switch (randBaseWeap)
            {
                case 0: weaponType = "Blades"; break;
                case 1: weaponType = "Piercing"; break;
                case 2: weaponType = "Blunt"; break;
            }

            WeaponTypeOne = weaponType;
            DamageType = 0;

            if (Util.RandomBool())
                WeaponTypeTwo = "Celtic Spear";
            else
                WeaponTypeTwo = "Large Weapons";

            int randVariance = Util.Random(4);

            switch (randVariance)
            {
                case 0:
                Add(WeaponTypeOne, 50, 1.0f);
                Add("Shields", 50, 0.9f);
                Add("Parry", 28, 0.1f);
                break;

                case 1:
                case 2:
                is2H = true;
                Add(WeaponTypeOne, 39, 0.8f);
                Add(WeaponTypeTwo, 50, 1.0f);
                Add("Shields", 42, 0.5f);
                Add("Parry", 6, 0.1f);
                break;

                case 3:
                case 4:
                is2H = true;
                Add(WeaponTypeOne, 39, 0.8f);
                Add(WeaponTypeTwo, 44, 1.0f);
                Add("Shields", 42, 0.5f);
                Add("Parry", 24, 0.1f);
                break;
            }
        }
    }
}