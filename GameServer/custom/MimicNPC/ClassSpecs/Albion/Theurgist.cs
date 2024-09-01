using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    //public class MimicTheurgist : MimicNPC
    //{
    //    public MimicTheurgist(byte level) : base(new ClassTheurgist(), level)
    //    { }
    //}

    public class TheurgistSpec : MimicSpec
    {
        public TheurgistSpec()
        {
            SpecName = "TheurgistSpec";

            WeaponOneType = eObjectType.Staff;
            Is2H = true;

            int randVariance = Util.Random(2);
            
            switch (randVariance)
            {
                case 0:
                SpecType = eSpecType.AirTheur;
                Add(Specs.Earth_Magic, 28, 0.1f);
                Add(Specs.Cold_Magic, 20, 0.0f);
                Add(Specs.Wind_Magic, 45, 1.0f);
                break;

                case 1:
                SpecType = eSpecType.IceTheur;
                Add(Specs.Earth_Magic, 4, 0.0f);
                Add(Specs.Cold_Magic, 50, 1.0f);
                Add(Specs.Wind_Magic, 20, 0.1f);
                break;

                case 2:
                SpecType = eSpecType.EarthTheur;
                Add(Specs.Earth_Magic, 50, 1.0f);
                Add(Specs.Cold_Magic, 4, 0.0f);
                Add(Specs.Wind_Magic, 20, 0.1f);
                break;
            }
        }
    }
}