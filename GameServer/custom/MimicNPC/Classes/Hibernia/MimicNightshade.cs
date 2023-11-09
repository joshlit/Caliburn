using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicNightshade : MimicNPC
    {
        public MimicNightshade(byte level) : base(new ClassNightshade(), level)
        {
            MimicSpec = new NightshadeSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.leftHand);
            MimicEquipment.SetArmor(this, eObjectType.Leather);
            MimicEquipment.SetJewelryROG(this, Realm, (eCharacterClass)CharacterClass.ID, Level, eObjectType.Magical);
            RefreshItemBonuses();
            SwitchWeapon(eActiveWeaponSlot.Standard);
            RefreshSpecDependantSkills(false);
            GetTauntStyles();
            SetSpells();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class NightshadeSpec : MimicSpec
    {
        public NightshadeSpec()
        {
            SpecName = "NightshadeSpec";
            is2H = false;

            int randBaseWeap = Util.Random(1);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Blades"; break;
                case 1: WeaponTypeOne = "Piercing"; break;
            }

            int randVariance = Util.Random(2);

            switch (randVariance)
            {
                case 0:
                case 1:
                Add(WeaponTypeOne, 39, 0.8f);
                Add("Celtic Dual", 15, 0.1f);
                Add("Critical Strike", 44, 0.9f);
                Add("Stealth", 37, 0.5f);
                Add("Envenom", 37, 0.6f);
                break;

                case 2:
                Add(WeaponTypeOne, 39, 0.9f);
                Add("Celtic Dual", 50, 1.0f);
                Add("Stealth", 34, 0.5f);
                Add("Envenom", 34, 0.6f);
                break;
            }
        }
    }
}