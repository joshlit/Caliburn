using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicWarden : MimicNPC
    {
        public MimicWarden(byte level) : base(new ClassWarden(), level)
        {
            MimicSpec = new WardenSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);

            if (level >= 7)
                MimicEquipment.SetRangedWeapon(this, eObjectType.Fired);

            int shieldSize = 1;

            if (level >= 5)
                shieldSize = 2;

            MimicEquipment.SetShield(this, shieldSize);

            eObjectType objectType = eObjectType.Leather;

            if (level >= 20)
                objectType = eObjectType.Scale;
            else if (level >= 10)
                objectType = eObjectType.Reinforced;

            MimicEquipment.SetArmor(this, objectType);
            MimicEquipment.SetJewelryROG(this, Realm, (eCharacterClass)CharacterClass.ID, Level, eObjectType.Magical);
            RefreshItemBonuses();
            SwitchWeapon(eActiveWeaponSlot.Standard);
            RefreshSpecDependantSkills(false);
            GetTauntStyles();
            SetSpells();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class WardenSpec : MimicSpec
    {
        public WardenSpec()
        {
            SpecName = "WardenSpec";
            is2H = false;

            int randBaseWeap = Util.Random(1);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Blades"; break;
                case 1: WeaponTypeOne = "Blunt"; break;
            }

            int randVariance = Util.Random(3);

            switch (randVariance)
            {
                case 0:
                Add(WeaponTypeOne, 39, 0.6f);
                Add("Nurture", 45, 0.9f);
                Add("Regrowth", 26, 0.5f);
                Add("Parry", 10, 0.1f);
                break;

                case 1:
                Add(WeaponTypeOne, 39, 0.7f);
                Add("Nurture", 49, 0.9f);
                Add("Regrowth", 16, 0.3f);
                Add("Parry", 12, 0.1f);
                break;

                case 2:
                Add(WeaponTypeOne, 34, 0.3f);
                Add("Nurture", 49, 0.9f);
                Add("Regrowth", 26, 0.4f);
                Add("Parry", 10, 0.0f);
                break;

                case 3:
                Add("Nurture", 45, 0.9f);
                Add("Regrowth", 48, 0.7f);
                Add("Parry", 5, 0.0f);
                break;
            }
        }
    }
}