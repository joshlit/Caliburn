using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicWarrior : MimicNPC
    {
        public MimicWarrior(byte level) : base(new ClassWarrior(), level)
        {
            MimicSpec = new WarriorSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.twoHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            //MimicEquipment.SetRangedWeapon(this, eObjectType.Thrown);

            int shieldSize = 1;

            if (level >= 10)
                shieldSize = 3;
            else if (level >= 5)
                shieldSize = 2;

            MimicEquipment.SetShield(this, shieldSize);

            eObjectType objectType = eObjectType.Studded;

            if (level >= 10)
                objectType = eObjectType.Chain;

            MimicEquipment.SetArmor(this, objectType);
            MimicEquipment.SetJewelryROG(this, Realm, (eCharacterClass)CharacterClass.ID, Level, eObjectType.Magical);
            RefreshItemBonuses();
            SwitchWeapon(eActiveWeaponSlot.Standard);
            RefreshSpecDependantSkills(false);
            GetTauntStyles();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class WarriorSpec : MimicSpec
    {
        public WarriorSpec()
        {
            SpecName = "WarriorSpec";

            int randBaseWeap = Util.Random(2);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Sword"; break;
                case 1: WeaponTypeOne = "Axe"; break;
                case 2: WeaponTypeOne = "Hammer"; break;
            }

            int randVariance = Util.Random(1);

            switch (randVariance)
            {
                case 0:
                Add(WeaponTypeOne, 50, 0.8f);
                Add("Shields", 42, 0.5f);
                Add("Parry", 39, 0.2f);
                break;

                case 1:
                Add(WeaponTypeOne, 50, 0.8f);
                Add("Shields", 50, 0.5f);
                Add("Parry", 28, 0.2f);
                break;
            }
        }
    }
}