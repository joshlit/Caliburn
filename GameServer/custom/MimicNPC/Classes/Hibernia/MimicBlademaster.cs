using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicBlademaster : MimicNPC
    {
        public MimicBlademaster(byte level) : base(new ClassBlademaster(), level)
        {
            MimicSpec = new BlademasterSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.leftHand);
            MimicEquipment.SetRangedWeapon(this, eObjectType.Fired);
            SwitchWeapon(eActiveWeaponSlot.Standard);
            RefreshSpecDependantSkills(false);
            GetTauntStyles();
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class BlademasterSpec : MimicSpec
    {
        public BlademasterSpec()
        {
            SpecName = "BlademasterSpec";
            is2H = false;

            int randBaseWeap = Util.Random(2);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = eObjectType.Blades; break;
                case 1: WeaponTypeOne = eObjectType.Piercing; break;
                case 2: WeaponTypeOne = eObjectType.Blunt; break;
            }

            int randVariance = Util.Random(2);

            switch (randVariance)
            {
                case 0:
                case 1:
                Add(ObjToSpec(WeaponTypeOne), 50, 0.8f);
                Add(Specs.Celtic_Dual, 50, 1.0f);
                Add(Specs.Parry, 28, 0.2f);
                break;

                case 2:
                Add(ObjToSpec(WeaponTypeOne), 39, 0.8f);
                Add(Specs.Celtic_Dual, 50, 1.0f);
                Add(Specs.Shields, 42, 0.5f);
                Add(Specs.Parry, 6, 0.1f);
                break;
            }
        }
    }
}