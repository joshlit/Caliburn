using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicScout : MimicNPC
    {
        public MimicScout(byte level) : base(new ClassScout(), level)
        {
            MimicSpec = new ScoutSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            MimicEquipment.SetRangedWeapon(this, eObjectType.Longbow);
            SwitchWeapon(eActiveWeaponSlot.Distance);
            RefreshSpecDependantSkills(false);
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class ScoutSpec : MimicSpec
    {
        public ScoutSpec()
        {
            SpecName = "ScoutSpec";
            is2H = false;

            int randBaseWeap = Util.Random(1);
            
            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = eObjectType.SlashingWeapon; break;
                case 1: WeaponTypeOne = eObjectType.ThrustWeapon; break;
            }

            int randVariance = Util.Random(1);
            
            switch (randVariance)
            {
                case 0:
                Add(ObjToSpec(WeaponTypeOne), 29, 0.6f);
                Add(Specs.Longbow, 44, 0.8f);
                Add(Specs.Shields, 42, 0.7f);
                Add(Specs.Stealth, 35, 0.1f);
                break;

                case 1:
                Add(ObjToSpec(WeaponTypeOne), 29, 0.7f);
                Add(Specs.Longbow, 50, 0.8f);
                Add(Specs.Shields, 35, 0.6f);
                Add(Specs.Stealth, 35, 0.1f);
                break;
            }
        }
    }
}