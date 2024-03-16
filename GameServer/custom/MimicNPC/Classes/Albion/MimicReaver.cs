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

            SwitchWeapon(eActiveWeaponSlot.Standard);
            RefreshSpecDependantSkills(false);
            SetSpells();
            RefreshItemBonuses();
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
                case 0: WeaponTypeOne = eObjectType.SlashingWeapon; break;
                case 1: WeaponTypeOne = eObjectType.ThrustWeapon; break;
                case 2: WeaponTypeOne = eObjectType.CrushingWeapon; break;
                case 3:
                case 4: WeaponTypeOne = eObjectType.Flexible; break;
            }

            int randVariance = Util.Random(2);

            switch (randVariance)
            {
                case 0:
                case 1:
                Add(ObjToSpec(WeaponTypeOne), 50, 0.8f);
                Add(Specs.Soulrending, 41, 1.0f);
                Add(Specs.Shields, 42, 0.5f);
                Add(Specs.Parry, 13, 0.1f);
                break;

                case 2:
                Add(ObjToSpec(WeaponTypeOne), 50, 0.8f);
                Add(Specs.Soulrending, 50, 1.0f);
                Add(Specs.Shields, 29, 0.4f);
                Add(Specs.Parry, 16, 0.1f);
                break;
            }
        }
    }
}