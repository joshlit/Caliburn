using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicBerserker : MimicNPC
    {
        public MimicBerserker(byte level) : base(new ClassBerserker(), level)
        {
            MimicSpec = new BerserkerSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.twoHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeTwo, eHand.leftHand);
            SwitchWeapon(eActiveWeaponSlot.Standard);
            RefreshSpecDependantSkills(false);
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class BerserkerSpec : MimicSpec
    {
        public BerserkerSpec()
        {
            SpecName = "BerserkerSpec";

            int randBaseWeap = Util.Random(2);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = eObjectType.Sword; break;
                case 1: WeaponTypeOne = eObjectType.Axe; break;
                case 2: WeaponTypeOne = eObjectType.Hammer; break;
            }

            WeaponTypeTwo = eObjectType.Axe;

            int randVariance = Util.Random(2);

            switch (randVariance)
            {
                case 0:
                case 1:
                Add(ObjToSpec(WeaponTypeOne), 50, 0.8f);
                Add(Specs.Left_Axe, 50, 1.0f);
                Add(Specs.Parry, 28, 0.2f);
                break;

                case 2:
                Add(ObjToSpec(WeaponTypeOne), 44, 0.8f);
                Add(Specs.Left_Axe, 50, 1.0f);
                Add(Specs.Parry, 37, 0.2f);
                break;
            }
        }
    }
}