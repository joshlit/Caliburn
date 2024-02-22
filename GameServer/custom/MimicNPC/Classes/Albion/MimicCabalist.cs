using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicCabalist : MimicNPC
    {
        public MimicCabalist(byte level) : base(new ClassCabalist(), level)
        {
            MimicSpec = MimicManager.Random(this);

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.twoHand);

            SwitchWeapon(eActiveWeaponSlot.TwoHanded);
            RefreshSpecDependantSkills(false);
            SetCasterSpells();
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class MatterCabalist : MimicSpec
    {
        public MatterCabalist()
        {
            SpecName = "MatterCabalist";

            WeaponTypeOne = eObjectType.Staff;

            int randVariance = Util.Random(3);

            switch (randVariance)
            {
                case 0:
                {
                    Add(Specs.Matter_Magic, 50, 1.0f);
                    Add(Specs.Spirit_Magic, 20, 0.1f);
                    break;
                }

                case 1:
                {
                    Add(Specs.Matter_Magic, 49, 1.0f);
                    Add(Specs.Spirit_Magic, 22, 0.1f);
                    break;
                }

                case 2:
                {
                    Add(Specs.Matter_Magic, 46, 1.0f);
                    Add(Specs.Spirit_Magic, 28, 0.1f);
                    break;
                }

                case 3:
                {
                    Add(Specs.Matter_Magic, 46, 1.0f);
                    Add(Specs.Body_Magic, 28, 0.1f);
                    Add(Specs.Spirit_Magic, 4, 0.0f);
                    break;
                }
            }
        }
    }

    public class BodyCabalist : MimicSpec
    {
        public BodyCabalist()
        {
            SpecName = "BodyCabalist";

            WeaponTypeOne = eObjectType.Staff;

            int randVariance = Util.Random(6);

            switch (randVariance)
            {
                case 0:
                {
                    Add(Specs.Body_Magic, 50, 1.0f);
                    Add(Specs.Spirit_Magic, 20, 0.1f);
                    break;
                }

                case 1:
                {
                    Add(Specs.Body_Magic, 47, 1.0f);
                    Add(Specs.Spirit_Magic, 26, 0.1f);
                    break;
                }

                case 2:
                {
                    Add(Specs.Body_Magic, 46, 1.0f);
                    Add(Specs.Spirit_Magic, 28, 0.2f);
                    break;
                }

                case 3:
                {
                    Add(Specs.Body_Magic, 45, 1.0f);
                    Add(Specs.Spirit_Magic, 29, 0.2f);
                    break;
                }

                case 4:
                {
                    Add(Specs.Body_Magic, 47, 1.0f);
                    Add(Specs.Matter_Magic, 25, 0.2f);
                    Add(Specs.Spirit_Magic, 8, 0.1f);
                    break;
                }

                case 5:
                {
                    Add(Specs.Body_Magic, 46, 1.0f);
                    Add(Specs.Matter_Magic, 27, 0.2f);
                    Add(Specs.Spirit_Magic, 8, 0.1f);
                    break;
                }

                case 6:
                {
                    Add(Specs.Body_Magic, 45, 1.0f);
                    Add(Specs.Matter_Magic, 29, 0.2f);
                    Add(Specs.Spirit_Magic, 4, 0.1f);
                    break;
                }
            }
        }
    }

    public class SpiritCabalist : MimicSpec
    {
        public SpiritCabalist()
        {
            SpecName = "SpiritCabalist";

            WeaponTypeOne = eObjectType.Staff;

            int randVariance = Util.Random(6);

            switch (randVariance)
            {
                case 0:
                {
                    Add(Specs.Spirit_Magic, 33, 0.9f);
                    Add(Specs.Body_Magic, 28, 0.3f);
                    Add(Specs.Matter_Magic, 32, 0.1f);
                    break;
                }

                case 1:
                {
                    Add(Specs.Spirit_Magic, 38, 0.9f);
                    Add(Specs.Matter_Magic, 38, 0.1f);
                    break;
                }

                case 2:
                {
                    Add(Specs.Spirit_Magic, 38, 0.9f);
                    Add(Specs.Body_Magic, 38, 0.1f);
                    break;
                }

                case 3:
                {
                    Add(Specs.Spirit_Magic, 46, 1.0f);
                    Add(Specs.Body_Magic, 28, 0.1f);
                    break;
                }

                case 4:
                {
                    Add(Specs.Spirit_Magic, 47, 1.0f);
                    Add(Specs.Body_Magic, 26, 0.1f);
                    break;
                }

                case 5:
                {
                    Add(Specs.Spirit_Magic, 46, 1.0f);
                    Add(Specs.Body_Magic, 28, 0.1f);
                    break;
                }

                case 6:
                {
                    Add(Specs.Spirit_Magic, 50, 1.0f);
                    Add(Specs.Body_Magic, 20, 0.1f);
                    break;
                }
            }
        }
    }
}