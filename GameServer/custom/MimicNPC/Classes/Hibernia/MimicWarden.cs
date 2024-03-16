using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicWarden : MimicNPC
    {
        public MimicWarden(byte level) : base(new ClassWarden(), level)
        {
            MimicSpec = new WardenSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);

            if (level >= 7)
                MimicEquipment.SetRangedWeapon(this, eObjectType.Fired);
                      
            SwitchWeapon(eActiveWeaponSlot.Standard);
            RefreshSpecDependantSkills(false);
            SetSpells();
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class WardenSpec : MimicSpec
    {
        public WardenSpec()
        {
            SpecName = "WardenSpec";
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
                Add(ObjToSpec(WeaponTypeOne), 39, 0.6f);
                Add(Specs.Nurture, 45, 0.9f);
                Add(Specs.Regrowth, 26, 0.5f);
                Add(Specs.Parry, 10, 0.1f);
                break;

                case 1:
                Add(ObjToSpec(WeaponTypeOne), 39, 0.7f);
                Add(Specs.Nurture, 49, 0.9f);
                Add(Specs.Regrowth, 16, 0.3f);
                Add(Specs.Parry, 12, 0.1f);
                break;

                case 2:
                Add(ObjToSpec(WeaponTypeOne), 34, 0.3f);
                Add(Specs.Nurture, 49, 0.9f);
                Add(Specs.Regrowth, 26, 0.4f);
                Add(Specs.Parry, 10, 0.0f);
                break;

                case 3:
                Add(Specs.Nurture, 45, 0.9f);
                Add(Specs.Regrowth, 48, 0.7f);
                Add(Specs.Parry, 5, 0.0f);
                break;
            }
        }
    }
}