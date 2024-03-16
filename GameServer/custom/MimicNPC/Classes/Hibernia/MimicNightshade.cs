using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicNightshade : MimicNPC
    {
        public MimicNightshade(byte level) : base(new ClassNightshade(), level)
        {
            MimicSpec = new NightshadeSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.leftHand);        
            SwitchWeapon(eActiveWeaponSlot.Standard);
            RefreshSpecDependantSkills(false);
            SetSpells();
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class NightshadeSpec : MimicSpec
    {
        public NightshadeSpec()
        {
            SpecName = "NightshadeSpec";
            is2H = false;

            int randBaseWeap = Util.Random(1);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = eObjectType.Blades; break;
                case 1: WeaponTypeOne = eObjectType.Piercing; break;
            }

            int randVariance = Util.Random(2);
            
            switch (randVariance)
            {
                case 0:
                case 1:
                Add(ObjToSpec(WeaponTypeOne), 39, 0.8f);
                Add(Specs.Celtic_Dual, 15, 0.1f);
                Add(Specs.Critical_Strike, 44, 0.9f);
                Add(Specs.Stealth, 37, 0.5f);
                Add(Specs.Envenom, 37, 0.6f);
                break;

                case 2:
                Add(ObjToSpec(WeaponTypeOne), 39, 0.9f);
                Add(Specs.Celtic_Dual, 50, 1.0f);
                Add(Specs.Stealth, 34, 0.5f);
                Add(Specs.Envenom, 34, 0.6f);
                break;
            }
        }
    }
}