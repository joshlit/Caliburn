using DOL.GS.PlayerClass;
using log4net;
using System.Reflection;

namespace DOL.GS.Scripts
{
    public class MimicChampion : MimicNPC
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MimicChampion(byte level) : base(new ClassChampion(), level)
        {
            MimicSpec = new ChampionSpec();

            DistributeSkillPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeTwo, eHand.twoHand);
            MimicEquipment.SetShield(this, 2);
            MimicEquipment.SetArmor(this, eObjectType.Scale);
            MimicEquipment.SetJewelry(this);
            RefreshItemBonuses();

            if (MimicSpec.is2H)
                SwitchWeapon(eActiveWeaponSlot.TwoHanded);
            else
                SwitchWeapon(eActiveWeaponSlot.Standard);

            RefreshSpecDependantSkills(false);
            SetSpells();
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
                case 0: WeaponTypeOne = "Blades"; break;
                case 1: WeaponTypeOne = "Piercing"; break;
                case 2: WeaponTypeOne = "Blunt"; break;
            }

            WeaponTypeTwo = "Large Weapons";

            int randVariance = Util.Random(3);

            switch (randVariance)
            {
                case 0:
                Add(WeaponTypeOne, 39, 0.8f);
                Add("Valor", 50, 1.0f);
                Add("Shields", 42, 0.5f);
                Add("Parry", 6, 0.1f);
                break;

                case 1:
                Add(WeaponTypeOne, 50, 0.8f);
                Add("Valor", 50, 1.0f);
                Add("Shields", 28, 0.5f);
                Add("Parry", 6, 0.1f);
                break;

                case 2:
                case 3:
                is2H = true;
                Add(WeaponTypeTwo, 50, 0.9f);
                Add("Valor", 50, 1.0f);
                Add("Parry", 28, 0.1f);
                break;
            }
        }
    }
}