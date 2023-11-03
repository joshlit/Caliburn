using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicReaver : MimicNPC
    {
        public MimicReaver(byte level) : base(new ClassReaver(), level)
        {
            MimicSpec = new ReaverSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);

            int shieldSize = 1;

            if (level >= 17)
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
            SetSpells();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class ReaverSpec : MimicSpec
    {
        public ReaverSpec()
        {
            SpecName = "ReaverSpec";

            int randBaseWeap = Util.Random(4);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Slash"; break;
                case 1: WeaponTypeOne = "Thrust"; break;
                case 2: WeaponTypeOne = "Crush"; break;
                case 3:
                case 4: WeaponTypeOne = "Flexible"; break;
            }

            int randVariance = Util.Random(2);

            switch (randVariance)
            {
                case 0:
                case 1:
                Add(WeaponTypeOne, 50, 0.8f);
                Add("Soulrending", 41, 1.0f);
                Add("Shields", 42, 0.5f);
                Add("Parry", 13, 0.1f);
                break;

                case 2:
                Add(WeaponTypeOne, 50, 0.8f);
                Add("Soulrending", 50, 1.0f);
                Add("Shields", 29, 0.4f);
                Add("Parry", 16, 0.1f);
                break;
            }
        }
    }
}