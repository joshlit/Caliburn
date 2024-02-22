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
            SwitchWeapon(eActiveWeaponSlot.Standard);
            RefreshSpecDependantSkills(false);
            SetSpells();
            RefreshItemBonuses();
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
                case 0: WeaponTypeOne = eObjectType.Blades; break;
                case 1: WeaponTypeOne = eObjectType.Blunt; break;
            }

            int randVariance = Util.Random(3);

            switch (randVariance)
            {
                case 0:
                Add(Specs.Nurture, 42, 0.9f);
                Add(Specs.Nature, 7, 0.0f);
                Add(Specs.Regrowth, 33, 0.7f);
                break;

                case 1:
                Add(Specs.Nurture, 40, 0.9f);
                Add(Specs.Nature, 9, 0.0f);
                Add(Specs.Regrowth, 35, 0.7f);
                break;

                case 2:
                Add(Specs.Nurture, 14, 0.1f);
                Add(Specs.Nature, 39, 0.9f);
                Add(Specs.Regrowth, 34, 0.7f);
                break;

                case 3:
                Add(Specs.Nurture, 35, 0.7f);
                Add(Specs.Nature, 3, 0.0f);
                Add(Specs.Regrowth, 41, 0.8f);
                break;
            }
        }
    }
}