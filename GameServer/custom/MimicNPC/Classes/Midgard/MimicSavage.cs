using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicSavage : MimicNPC
    {
        public MimicSavage(byte level) : base(new ClassSavage(), level)
        {
            MimicSpec = new SavageSpec();

            SpendSpecPoints();

            if (MimicSpec.is2H)
            {
                MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.twoHand);
                MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
                SwitchWeapon(eActiveWeaponSlot.TwoHanded);
            }                
            else
            {
                MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
                MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.leftHand);
                SwitchWeapon(eActiveWeaponSlot.Standard);
            }

            RefreshSpecDependantSkills(false);
            GetTauntStyles();
            SetSpells();
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class SavageSpec : MimicSpec
    {
        public SavageSpec()
        {
            SpecName = "SavageSpec";

            int randBaseWeap = Util.Random(4);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = eObjectType.Sword; break;
                case 1: WeaponTypeOne = eObjectType.Axe; break;
                case 2: WeaponTypeOne = eObjectType.Hammer; break;
                case 3:
                case 4: WeaponTypeOne = eObjectType.HandToHand; break;
            }

            if (WeaponTypeOne != eObjectType.HandToHand)
                is2H = true;
            
            int randVariance = Util.Random(3);

            switch (randVariance)
            {
                case 0:
                Add(ObjToSpec(WeaponTypeOne), 44, 0.7f);
                Add(Specs.Savagery, 49, 0.9f);
                Add(Specs.Parry, 4, 0.0f);
                break;

                case 1:
                Add(ObjToSpec(WeaponTypeOne), 39, 0.7f);
                Add(Specs.Savagery, 49, 0.9f);
                Add(Specs.Parry, 20, 0.1f);
                break;

                case 2:
                Add(ObjToSpec(WeaponTypeOne), 44, 0.7f);
                Add(Specs.Savagery, 48, 0.9f);
                Add(Specs.Parry, 10, 0.1f);
                break;

                case 3:
                Add(ObjToSpec(WeaponTypeOne), 50, 0.7f);
                Add(Specs.Savagery, 42, 0.9f);
                Add(Specs.Parry, 9, 0.1f);
                break;
            }
        }
    }
}