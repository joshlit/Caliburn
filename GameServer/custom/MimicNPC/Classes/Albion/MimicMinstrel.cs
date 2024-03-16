using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicMinstrel : MimicNPC
    {
        public MimicMinstrel(byte level) : base(new ClassMinstrel(), level)
        {
            MimicSpec = new MinstrelSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            MimicEquipment.SetInstrumentROG(this, Realm, (eCharacterClass)CharacterClass.ID, Level, eObjectType.Instrument, eInventorySlot.TwoHandWeapon, eInstrumentType.Flute);
            MimicEquipment.SetInstrumentROG(this, Realm, (eCharacterClass)CharacterClass.ID, Level, eObjectType.Instrument, eInventorySlot.DistanceWeapon, eInstrumentType.Drum);
            //MimicEquipment.SetInstrumentROG(this, Realm, (eCharacterClass)CharacterClass.ID, Level, eObjectType.Instrument, eInventorySlot.FirstEmptyBackpack, eInstrumentType.Lute);

            SwitchWeapon(eActiveWeaponSlot.Standard);
            RefreshSpecDependantSkills(false);
            SetSpells();
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class MinstrelSpec : MimicSpec
    {
        public MinstrelSpec()
        {
            SpecName = "MinstrelSpec";

            int randBaseWeap = Util.Random(1);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = eObjectType.SlashingWeapon; break;
                case 1: WeaponTypeOne = eObjectType.ThrustWeapon; break;
            }

            int randVariance = Util.Random(4);

            switch (randVariance)
            {
                case 0:
                case 1:
                Add(ObjToSpec(WeaponTypeOne), 39, 0.6f);
                Add(Specs.Instruments, 50, 1.0f);
                Add(Specs.Stealth, 21, 0.1f);
                break;

                case 2:
                case 3:
                Add(ObjToSpec(WeaponTypeOne), 44, 0.6f);
                Add(Specs.Instruments, 50, 1.0f);
                Add(Specs.Stealth, 8, 0.0f);
                break;

                case 4:
                Add(ObjToSpec(WeaponTypeOne), 50, 0.6f);
                Add(Specs.Instruments, 44, 1.0f);
                Add(Specs.Stealth, 8, 0.0f);
                break;
            }
        }
    }
}