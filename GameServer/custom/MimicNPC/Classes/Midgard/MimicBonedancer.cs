using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicBonedancer : MimicNPC
    {
        public MimicBonedancer(byte level) : base(new ClassBonedancer(), level)
        {
            MimicSpec = new BonedancerSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.twoHand);
            SwitchWeapon(eActiveWeaponSlot.Standard);
            RefreshSpecDependantSkills(false);
            SetCasterSpells();
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class BonedancerSpec : MimicSpec
    {
        public BonedancerSpec()
        {
            SpecName = "BonedancerSpec";

            WeaponTypeOne = eObjectType.Staff;

            int randVariance = Util.Random(5);
            
            switch (randVariance)
            {
                case 0:
                Add(Specs.Darkness, 26, 0.1f);
                Add(Specs.Suppression, 47, 1.0f);
                Add(Specs.BoneArmy, 5, 0.0f);
                break;

                case 1:
                Add(Specs.Darkness, 24, 0.1f);
                Add(Specs.Suppression, 48, 1.0f);
                Add(Specs.BoneArmy, 6, 0.0f);
                break;

                case 2:
                Add(Specs.Darkness, 5, 0.0f);
                Add(Specs.Suppression, 47, 1.0f);
                Add(Specs.BoneArmy, 26, 0.1f);
                break;

                case 3:
                Add(Specs.Darkness, 39, 0.5f);
                Add(Specs.Suppression, 37, 0.8f);
                Add(Specs.BoneArmy, 4, 0.0f);
                break;

                case 4:
                Add(Specs.Darkness, 50, 1.0f);
                Add(Specs.Suppression, 20, 0.1f);
                Add(Specs.BoneArmy, 4, 0.0f);
                break;

                case 5:
                Add(Specs.Darkness, 6, 0.0f);
                Add(Specs.Suppression, 24, 0.1f);
                Add(Specs.BoneArmy, 48, 1.0f);
                break;
            }
        }
    }
}