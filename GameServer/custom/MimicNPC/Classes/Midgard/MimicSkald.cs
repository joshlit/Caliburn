using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicSkald : MimicNPC
    {
        public MimicSkald(byte level) : base(new ClassSkald(), level)
        {
            MimicSpec = new SkaldSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.twoHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            MimicEquipment.SetShield(this, 1);

            eObjectType objectType = eObjectType.Studded;

            if (level >= 20)
                objectType = eObjectType.Chain;

            MimicEquipment.SetArmor(this, objectType);
            MimicEquipment.SetJewelryROG(this, Realm, (eCharacterClass)CharacterClass.ID, Level, eObjectType.Magical);
            RefreshItemBonuses();
            SwitchWeapon(eActiveWeaponSlot.TwoHanded);
            RefreshSpecDependantSkills(false);
            GetTauntStyles();
            SetSpells();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class SkaldSpec : MimicSpec
    {
        public SkaldSpec()
        {
            SpecName = "SkaldSpec";

            int randBaseWeap = Util.Random(2);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Sword"; break;
                case 1: WeaponTypeOne = "Axe"; break;
                case 2: WeaponTypeOne = "Hammer"; break;
            }

            int randVariance = Util.Random(2);

            switch (randVariance)
            {
                case 0:
                case 1:
                Add(WeaponTypeOne, 39, 0.7f);
                Add("Battlesongs", 50, 0.8f);
                Add("Parry", 28, 0.1f);
                break;

                case 2:
                Add(WeaponTypeOne, 44, 0.7f);
                Add("Battlesongs", 49, 0.8f);
                Add("Parry", 4, 0.1f);
                break;
            }
        }
    }
}