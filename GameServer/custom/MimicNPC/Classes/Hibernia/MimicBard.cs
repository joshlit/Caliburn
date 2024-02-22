using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicBard : MimicNPC
    {
        public MimicBard(byte level) : base(new ClassBard(), level)
        {
            MimicSpec = new BardSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            MimicEquipment.SetInstrumentROG(this, Realm, (eCharacterClass)CharacterClass.ID, Level, eObjectType.Instrument, eInventorySlot.TwoHandWeapon, eInstrumentType.Lute);
            MimicEquipment.SetInstrumentROG(this, Realm, (eCharacterClass)CharacterClass.ID, Level, eObjectType.Instrument, eInventorySlot.DistanceWeapon, eInstrumentType.Drum);
            //MimicEquipment.SetInstrumentROG(this, Realm, (eCharacterClass)CharacterClass.ID, Level, eObjectType.Instrument, eInventorySlot.FirstEmptyBackpack, eInstrumentType.Flute);
            SwitchWeapon(eActiveWeaponSlot.Standard);
            RefreshSpecDependantSkills(false);
            SetSpells();
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class BardSpec : MimicSpec
    {
        public BardSpec()
        {
            SpecName = "BardSpec";
            is2H = false;

            int randBaseWeap = Util.Random(1);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = eObjectType.Blades; break;
                case 1: WeaponTypeOne = eObjectType.Blunt; break;
            }

            int randVariance = Util.Random(2);

            switch (randVariance)
            {
                case 0:
                Add(Specs.Music, 47, 0.8f);
                Add(Specs.Nurture, 43, 0.9f);
                Add(Specs.Regrowth, 16, 0.1f);
                break;

                case 1:
                Add(Specs.Music, 37, 0.4f);
                Add(Specs.Nurture, 43, 0.9f);
                Add(Specs.Regrowth, 33, 0.7f);
                break;

                case 2:
                Add(ObjToSpec(WeaponTypeOne), 29, 0.7f);
                Add(Specs.Music, 37, 0.4f);
                Add(Specs.Nurture, 43, 0.9f);
                Add(Specs.Regrowth, 16, 0.1f);
                break;
            }
        }
    }
}