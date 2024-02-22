using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicTheurgist : MimicNPC
    {
        public MimicTheurgist(byte level) : base(new ClassTheurgist(), level)
        {
            MimicSpec = new TheurgistSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.twoHand);
            SwitchWeapon(eActiveWeaponSlot.TwoHanded);
            RefreshSpecDependantSkills(false);
            SetCasterSpells();
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class TheurgistSpec : MimicSpec
    {
        public TheurgistSpec()
        {
            SpecName = "TheurgistSpec";

            WeaponTypeOne = eObjectType.Staff;

            int randVariance = Util.Random(2);
            
            switch (randVariance)
            {
                case 0:
                Add(Specs.Earth_Magic, 28, 0.1f);
                Add(Specs.Cold_Magic, 20, 0.0f);
                Add(Specs.Wind_Magic, 45, 1.0f);
                break;

                case 1:
                Add(Specs.Earth_Magic, 4, 0.0f);
                Add(Specs.Cold_Magic, 50, 1.0f);
                Add(Specs.Wind_Magic, 20, 0.1f);
                break;

                case 2:
                Add(Specs.Earth_Magic, 50, 1.0f);
                Add(Specs.Cold_Magic, 4, 0.0f);
                Add(Specs.Wind_Magic, 20, 0.1f);
                break;
            }
        }
    }
}