using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    //public class MimicShadowblade : MimicNPC
    //{
    //    public MimicShadowblade(byte level) : base(new ClassShadowblade(), level)
    //    { }
    //}

    public class ShadowbladeSpec : MimicSpec
    {
        public ShadowbladeSpec()
        {
            SpecName = "ShadowbladeSpec";

            int randBaseWeap = Util.Random(1);

            switch (randBaseWeap)
            {
                case 0: WeaponOneType = eObjectType.Sword; break;
                case 1: WeaponOneType = eObjectType.Axe; break;
            }

            WeaponTwoType = eObjectType.Axe;

            int randVariance = Util.Random(4);

            switch (randVariance)
            {
                case 0:
                case 1:
                SpecType = eSpecType.LeftAxe;
                Add(ObjToSpec(WeaponOneType), 34, 0.6f);
                Add(Specs.Left_Axe, 39, 0.8f);
                Add(Specs.Critical_Strike, 34, 0.9f);
                Add(Specs.Stealth, 35, 0.3f);
                Add(Specs.Envenom, 35, 0.5f);
                break;

                case 2:
                case 3:
                SpecType = eSpecType.LeftAxe;
                Add(ObjToSpec(WeaponOneType), 34, 0.6f);
                Add(Specs.Left_Axe, 50, 0.8f);
                Add(Specs.Critical_Strike, 10, 0.4f);
                Add(Specs.Stealth, 36, 0.3f);
                Add(Specs.Envenom, 36, 0.5f);
                break;

                case 4:
                Is2H = true;
                SpecType = eSpecType.Mid;
                Add(ObjToSpec(WeaponOneType), 39, 0.8f);
                Add(Specs.Critical_Strike, 44, 0.9f);
                Add(Specs.Stealth, 38, 0.3f);
                Add(Specs.Envenom, 38, 0.5f);
                break;
            }
        }
    }
}