using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicThane : MimicNPC
    {
        public MimicThane(byte level) : base(new ClassThane(), level)
        {
            MimicSpec = new ThaneSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.twoHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);          
            SwitchWeapon(eActiveWeaponSlot.Standard);
            RefreshSpecDependantSkills(false);
            GetTauntStyles();
            SetSpells();
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class ThaneSpec : MimicSpec
    {
        public ThaneSpec()
        {
            SpecName = "ThaneSpec";

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
                Add(ObjToSpec(WeaponTypeOne), 39, 0.8f);
                Add(Specs.Stormcalling, 50, 1.0f);
                Add(Specs.Shields, 42, 0.5f);
                Add(Specs.Parry, 6, 0.0f);
                break;

                case 2:
                Add(ObjToSpec(WeaponTypeOne), 44, 0.8f);
                Add(Specs.Stormcalling, 48, 1.0f);
                Add(Specs.Shields, 35, 0.5f);
                Add(Specs.Parry, 18, 0.0f);
                break;

                case 3:
                Add(ObjToSpec(WeaponTypeOne), 50, 0.8f);
                Add(Specs.Stormcalling, 50, 1.0f);
                Add(Specs.Parry, 28, 0.1f);
                break;
            }
        }
    }
}