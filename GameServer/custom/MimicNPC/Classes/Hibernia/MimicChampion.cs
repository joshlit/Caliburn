using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicChampion : MimicNPC
    {
        public MimicChampion(byte level) : base(new ClassChampion(), level)
        {
            MimicSpec = new ChampionSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeTwo, eHand.twoHand);

            int shieldSize = 1;

            if (level >= 5)
                shieldSize = 2;

            MimicEquipment.SetShield(this, shieldSize);

            eObjectType objectType = eObjectType.Reinforced;

            if (level >= 20)
                objectType = eObjectType.Scale;

            MimicEquipment.SetArmor(this, objectType);
            MimicEquipment.SetJewelryROG(this, Realm, (eCharacterClass)CharacterClass.ID, Level, eObjectType.Magical);
            RefreshItemBonuses();

            if (MimicSpec.is2H)
                SwitchWeapon(eActiveWeaponSlot.TwoHanded);
            else
                SwitchWeapon(eActiveWeaponSlot.Standard);

            RefreshSpecDependantSkills(false);
            GetTauntStyles();
            SetSpells();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class ChampionSpec : MimicSpec
    {
        public ChampionSpec()
        {
            SpecName = "ChampionSpec";

            int randBaseWeap = Util.Random(2);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Blades"; break;
                case 1: WeaponTypeOne = "Piercing"; break;
                case 2: WeaponTypeOne = "Blunt"; break;
            }

            WeaponTypeTwo = "Large Weapons";

            int randVariance = Util.Random(3);

            switch (randVariance)
            {
                case 0:
                Add(WeaponTypeOne, 39, 0.8f);
                Add("Valor", 50, 1.0f);
                Add("Shields", 42, 0.5f);
                Add("Parry", 6, 0.1f);
                break;

                case 1:
                Add(WeaponTypeOne, 50, 0.8f);
                Add("Valor", 50, 1.0f);
                Add("Shields", 28, 0.5f);
                Add("Parry", 6, 0.1f);
                break;

                case 2:
                case 3:
                is2H = true;
                Add(WeaponTypeTwo, 50, 0.9f);
                Add("Valor", 50, 1.0f);
                Add("Parry", 28, 0.1f);
                break;
            }
        }
    }
}