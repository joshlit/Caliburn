using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicShadowblade : MimicNPC
    {
        public MimicShadowblade(byte level) : base(new ClassShadowblade(), level)
        {
            MimicSpec = new ShadowbladeSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeTwo, eHand.leftHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.twoHand);
            //MimicEquipment.SetRangedWeapon(this, eObjectType.Thrown);
            SwitchWeapon(eActiveWeaponSlot.Standard);

            if (GetSpecializationByName(Specs.Left_Axe).Level == 1 && Level != 1)
                SwitchWeapon(eActiveWeaponSlot.TwoHanded);

            RefreshSpecDependantSkills(false);
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class ShadowbladeSpec : MimicSpec
    {
        public ShadowbladeSpec()
        {
            SpecName = "ShadowbladeSpec";

            int randBaseWeap = Util.Random(1);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = eObjectType.Sword; break;
                case 1: WeaponTypeOne = eObjectType.Axe; break;
            }

            WeaponTypeTwo = eObjectType.Axe;

            int randVariance = Util.Random(4);

            switch (randVariance)
            {
                case 0:
                case 1:
                Add(ObjToSpec(WeaponTypeOne), 34, 0.6f);
                Add(Specs.Left_Axe, 39, 0.8f);
                Add(Specs.Critical_Strike, 34, 0.9f);
                Add(Specs.Stealth, 35, 0.3f);
                Add(Specs.Envenom, 35, 0.5f);
                break;

                case 2:
                case 3:
                Add(ObjToSpec(WeaponTypeOne), 34, 0.6f);
                Add(Specs.Left_Axe, 50, 0.8f);
                Add(Specs.Critical_Strike, 10, 0.4f);
                Add(Specs.Stealth, 36, 0.3f);
                Add(Specs.Envenom, 36, 0.5f);
                break;

                case 4:
                Add(ObjToSpec(WeaponTypeOne), 39, 0.8f);
                Add(Specs.Critical_Strike, 44, 0.9f);
                Add(Specs.Stealth, 38, 0.3f);
                Add(Specs.Envenom, 38, 0.5f);
                break;
            }
        }
    }
}