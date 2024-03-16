using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicMercenary : MimicNPC
    {
        public MimicMercenary(byte level) : base(new ClassMercenary(), level)
        {
            MimicSpec = new MercenarySpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.leftHand);
            MimicEquipment.SetRangedWeapon(this, eObjectType.Fired);

            SwitchWeapon(eActiveWeaponSlot.Standard);
            RefreshSpecDependantSkills(false);
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class MercenarySpec : MimicSpec
    {
        public MercenarySpec()
        {
            SpecName = "MercenarySpec";

            int randBaseWeap = Util.Random(2);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = eObjectType.SlashingWeapon; break;
                case 1: WeaponTypeOne = eObjectType.ThrustWeapon; break;
                case 2: WeaponTypeOne = eObjectType.CrushingWeapon; break;
            }

            int randVariance = Util.Random(3);

            switch (randVariance)
            {
                case 0:
                case 1:
                case 2:
                Add(ObjToSpec(WeaponTypeOne), 50, 0.8f);
                Add(Specs.Dual_Wield, 50, 1.0f);
                Add(Specs.Parry, 33, 0.2f);
                break;

                case 3:
                Add(ObjToSpec(WeaponTypeOne), 39, 0.8f);
                Add(Specs.Dual_Wield, 50, 0.9f);
                Add(Specs.Shields, 42, 0.5f);
                Add(Specs.Parry, 18, 0.1f);
                break;
            }
        }
    }
}