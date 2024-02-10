using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicDruid : MimicNPC
    {
        public MimicDruid(byte level) : base(new ClassDruid(), level)
        {
            MimicSpec = new DruidSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            MimicEquipment.SetShield(this, 1);

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
            SetSpells();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class DruidSpec : MimicSpec
    {
        public DruidSpec()
        {
            SpecName = "DruidSpec";
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
                Add("Nurture", 42, 0.9f);
                Add("Nature", 7, 0.0f);
                Add("Regrowth", 33, 0.7f);
                break;

                case 1:
                Add("Nurture", 40, 0.9f);
                Add("Nature", 9, 0.0f);
                Add("Regrowth", 35, 0.7f);
                break;

                case 2:
                Add("Nurture", 14, 0.1f);
                Add("Nature", 39, 0.9f);
                Add("Regrowth", 34, 0.7f);
                break;

                case 3:
                Add("Nurture", 35, 0.7f);
                Add("Nature", 3, 0.0f);
                Add("Regrowth", 41, 0.8f);
                break;
            }
        }
    }
}