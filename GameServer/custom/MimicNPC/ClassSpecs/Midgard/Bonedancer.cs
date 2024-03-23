using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    //public class MimicBonedancer : MimicNPC
    //{
    //    public MimicBonedancer(byte level) : base(new ClassBonedancer(), level)
    //    { }
    //}

    public class BonedancerSpec : MimicSpec
    {
        public BonedancerSpec()
        {
            SpecName = "BonedancerSpec";

            WeaponOneType = eObjectType.Staff;
            Is2H = true;

            int randVariance = Util.Random(5);
            
            switch (randVariance)
            {
                case 0:
                SpecType = eSpecType.SuppBone;
                Add(Specs.Darkness, 26, 0.1f);
                Add(Specs.Suppression, 47, 1.0f);
                Add(Specs.BoneArmy, 5, 0.0f);
                break;

                case 1:
                SpecType = eSpecType.SuppBone;
                Add(Specs.Darkness, 24, 0.1f);
                Add(Specs.Suppression, 48, 1.0f);
                Add(Specs.BoneArmy, 6, 0.0f);
                break;

                case 2:
                SpecType = eSpecType.SuppBone;
                Add(Specs.Darkness, 5, 0.0f);
                Add(Specs.Suppression, 47, 1.0f);
                Add(Specs.BoneArmy, 26, 0.1f);
                break;

                case 3:
                SpecType = eSpecType.DarkBone;
                Add(Specs.Darkness, 39, 0.5f);
                Add(Specs.Suppression, 37, 0.8f);
                Add(Specs.BoneArmy, 4, 0.0f);
                break;

                case 4:
                SpecType = eSpecType.DarkBone;
                Add(Specs.Darkness, 50, 1.0f);
                Add(Specs.Suppression, 20, 0.1f);
                Add(Specs.BoneArmy, 4, 0.0f);
                break;

                case 5:
                SpecType = eSpecType.ArmyBone;
                Add(Specs.Darkness, 6, 0.0f);
                Add(Specs.Suppression, 24, 0.1f);
                Add(Specs.BoneArmy, 48, 1.0f);
                break;
            }
        }
    }
}