using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicChampion : MimicNPC
    {
        public MimicChampion(byte level) : base(new ClassChampion(), level)
        {
            MimicSpec = new ChampionSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeTwo, eHand.twoHand);

            if (MimicSpec.is2H)
                SwitchWeapon(eActiveWeaponSlot.TwoHanded);
            else
                SwitchWeapon(eActiveWeaponSlot.Standard);

            RefreshSpecDependantSkills(false);
            SetSpells();
            RefreshItemBonuses();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class ChampionSpec : MimicSpec
    {
        public ChampionSpec()
        {
            SpecName = "ChampionSpec";

            int randBaseWeap = Util.Random(2);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = eObjectType.Blades; break;
                case 1: WeaponTypeOne = eObjectType.Piercing; break;
                case 2: WeaponTypeOne = eObjectType.Blunt; break;
            }

            WeaponTypeTwo = eObjectType.LargeWeapons;

            int randVariance = Util.Random(3);

            switch (randVariance)
            {
                case 0:
                Add(ObjToSpec(WeaponTypeOne), 39, 0.8f);
                Add(Specs.Valor, 50, 1.0f);
                Add(Specs.Shields, 42, 0.5f);
                Add(Specs.Parry, 6, 0.1f);
                break;

                case 1:
                Add(ObjToSpec(WeaponTypeOne), 50, 0.8f);
                Add(Specs.Valor, 50, 1.0f);
                Add(Specs.Shields, 28, 0.5f);
                Add(Specs.Parry, 6, 0.1f);
                break;

                case 2:
                case 3:
                is2H = true;
                Add(ObjToSpec(WeaponTypeTwo), 50, 0.9f);
                Add(Specs.Valor, 50, 1.0f);
                Add(Specs.Parry, 28, 0.1f);
                break;
            }
        }
    }
}