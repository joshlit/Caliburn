using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicSkald : MimicNPC
    {
        public MimicSkald(byte level) : base(new ClassSkald(), level)
        {
            MimicSpec = new SkaldSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.twoHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            SwitchWeapon(eActiveWeaponSlot.TwoHanded);
            RefreshSpecDependantSkills(false);
            SetSpells();
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class SkaldSpec : MimicSpec
    {
        public SkaldSpec()
        {
            SpecName = "SkaldSpec";

            int randBaseWeap = Util.Random(2);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = eObjectType.Sword; break;
                case 1: WeaponTypeOne = eObjectType.Axe; break;
                case 2: WeaponTypeOne = eObjectType.Hammer; break;
            }

            int randVariance = Util.Random(3);

            switch (randVariance)
            {
                case 0:
                case 1:
                Add(ObjToSpec(WeaponTypeOne), 39, 0.7f);
                Add(Specs.Battlesongs, 50, 0.8f);
                Add(Specs.Parry, 18, 0.1f);
                break;

                case 2:
                Add(ObjToSpec(WeaponTypeOne), 44, 0.7f);
                Add(Specs.Battlesongs, 46, 0.8f);
                Add(Specs.Parry, 17, 0.1f);
                break;

                case 3:
                Add(ObjToSpec(WeaponTypeOne), 44, 0.7f);
                Add(Specs.Battlesongs, 49, 0.8f);
                Add(Specs.Parry, 4, 0.1f);
                break;
            }
        }
    }
}