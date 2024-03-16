using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicWarrior : MimicNPC
    {
        public MimicWarrior(byte level) : base(new ClassWarrior(), level)
        {
            MimicSpec = new WarriorSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.twoHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            //MimicEquipment.SetRangedWeapon(this, eObjectType.Thrown);      
            RefreshItemBonuses();
            SwitchWeapon(eActiveWeaponSlot.Standard);
            RefreshSpecDependantSkills(false);
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class WarriorSpec : MimicSpec
    {
        public WarriorSpec()
        {
            SpecName = "WarriorSpec";

            int randBaseWeap = Util.Random(2);
            
            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = eObjectType.Sword; break;
                case 1: WeaponTypeOne = eObjectType.Axe; break;
                case 2: WeaponTypeOne = eObjectType.Hammer; break;
            }

            int randVariance = Util.Random(1);

            switch (randVariance)
            {
                case 0:
                Add(ObjToSpec(WeaponTypeOne), 50, 0.8f);
                Add(Specs.Shields, 42, 0.5f);
                Add(Specs.Parry, 39, 0.2f);
                break;

                case 1:
                Add(ObjToSpec(WeaponTypeOne), 50, 0.8f);
                Add(Specs.Shields, 50, 0.5f);
                Add(Specs.Parry, 28, 0.2f);
                break;
            }
        }
    }
}