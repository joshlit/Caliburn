using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicHunter : MimicNPC
    {
        public MimicHunter(byte level) : base(new ClassHunter(), level)
        {
            MimicSpec = new HunterSpec();

            SpendSpecPoints();
            MimicEquipment.SetRangedWeapon(this, eObjectType.CompositeBow);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.twoHand);

            if (MimicSpec.WeaponTypeOne == eObjectType.Sword)
                MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
                     
            SwitchWeapon(eActiveWeaponSlot.Distance);
            RefreshSpecDependantSkills(false);
            SetSpells();
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class HunterSpec : MimicSpec
    {
        public HunterSpec()
        {
            SpecName = "HunterSpec";

            int randBaseWeap = Util.Random(2);

            switch (randBaseWeap)
            {
                case 0:
                case 1: WeaponTypeOne = eObjectType.Spear; break;
                case 2: WeaponTypeOne = eObjectType.Sword; break;
            }

            int randVariance = Util.Random(4);
            
            switch (randVariance)
            {
                case 0:
                case 1:
                Add(ObjToSpec(WeaponTypeOne), 39, 0.8f);
                Add(Specs.CompositeBow, 35, 0.9f);
                Add(Specs.Beastcraft, 40, 0.6f);
                Add(Specs.Stealth, 38, 0.3f);
                break;

                case 2:
                case 3:
                Add(ObjToSpec(WeaponTypeOne), 39, 0.8f);
                Add(Specs.CompositeBow, 45, 0.9f);
                Add(Specs.Beastcraft, 32, 0.6f);
                Add(Specs.Stealth, 38, 0.3f);
                break;

                case 4:
                Add(ObjToSpec(WeaponTypeOne), 44, 0.9f);
                Add(Specs.Beastcraft, 50, 0.8f);
                Add(Specs.Stealth, 37, 0.5f);
                break;
            }
        }
    }
}