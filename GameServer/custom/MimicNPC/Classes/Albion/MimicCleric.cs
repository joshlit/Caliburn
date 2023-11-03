using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicCleric : MimicNPC
    {
        public MimicCleric(byte level) : base(new ClassCleric(), level)
        {
            MimicSpec = new ClericSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);

            int shieldSize = 1;

            if (Level >= 10)
                shieldSize = 2;

            MimicEquipment.SetShield(this, shieldSize);

            eObjectType objectType = eObjectType.Leather;

            if (level >= 20)
                objectType = eObjectType.Chain;
            else if (level >= 10)
                objectType = eObjectType.Studded;

            MimicEquipment.SetArmor(this, objectType);
            MimicEquipment.SetJewelryROG(this, Realm, (eCharacterClass)CharacterClass.ID, Level, eObjectType.Magical);
            RefreshItemBonuses();
            SwitchWeapon(eActiveWeaponSlot.Standard);
            RefreshSpecDependantSkills(false);
            SetSpells();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class ClericSpec : MimicSpec
    {
        public ClericSpec()
        {
            SpecName = "ClericSpec";

            WeaponTypeOne = "Crush";

            int randVariance = Util.Random(7);

            switch (randVariance)
            {
                case 0:
                case 1:
                Add("Rejuvenation", 33, 0.5f);
                Add("Enhancement", 42, 0.8f);
                Add("Smite", 7, 0.0f);
                break;

                case 2:
                case 3:
                Add("Rejuvenation", 36, 0.5f);
                Add("Enhancement", 40, 0.8f);
                Add("Smite", 4, 0.0f);
                break;

                case 4:
                Add("Rejuvenation", 46, 0.8f);
                Add("Enhancement", 28, 0.5f);
                Add("Smite", 4, 0.0f);
                break;

                case 5:
                Add("Rejuvenation", 50, 0.8f);
                Add("Enhancement", 20, 0.5f);
                Add("Smite", 4, 0.0f);
                break;

                case 6:
                Add("Rejuvenation", 6, 0.0f);
                Add("Enhancement", 29, 0.5f);
                Add("Smite", 45, 0.8f);
                break;

                case 7:
                Add("Rejuvenation", 4, 0.0f);
                Add("Enhancement", 36, 0.5f);
                Add("Smite", 40, 0.8f);
                break;
            }
        }
    }
}