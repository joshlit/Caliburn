using DOL.GS.PlayerClass;
using log4net;
using System.Reflection;

namespace DOL.GS.Scripts
{
    public class MimicChampion : MimicNPC
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MimicChampion(GameLiving owner, byte level = 0, Point3D position = null) : base(owner, new ClassChampion(), level, position)
        {
            MimicSpec = new ChampionSpec();

            DistributeSkillPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne);
            MimicEquipment.SetArmor(this, eObjectType.Scale);

            if (!MimicSpec.is2H)
                MimicEquipment.SetShield(this, 2);

            MimicEquipment.SetJewelry(this);

            //foreach (InventoryItem item in Inventory.EquippedItems)
            //{
            //	if (item == null)
            //		return;

            //	if (item.Quality < 90)
            //	{
            //		item.Quality = Util.Random(85, 100);
            //	}

            //	log.Debug("Name: " + item.Name);
            //	log.Debug("Slot: " + Enum.GetName(typeof(eInventorySlot), item.SlotPosition));
            //	log.Debug("DPS_AF: " + item.DPS_AF);
            //	log.Debug("SPD_ABS: " + item.SPD_ABS);
            //}

            if (MimicSpec.is2H)
                SwitchWeapon(eActiveWeaponSlot.TwoHanded);
            else
                SwitchWeapon(eActiveWeaponSlot.Standard);

            RefreshSpecDependantSkills(false);
            SetSpells();
        }
    }

    public class ChampionSpec : MimicSpec
    {
        public ChampionSpec()
        {
            SpecName = "ChampionSpec";

            int randVariance = Util.Random(1);

            switch (randVariance)
            {
                case 0:
                {
                    int randBaseWeap = Util.Random(2);

                    switch (randBaseWeap)
                    {
                        case 0: WeaponTypeOne = "Blades"; break;
                        case 1: WeaponTypeOne = "Piercing"; break;
                        case 2: WeaponTypeOne = "Blunt"; break;
                    }

                    Add(WeaponTypeOne, 50, 0.8f);
                    Add("Valor", 50, 1.0f);
                    Add("Shields", 42, 0.5f);
                    Add("Parry", 6, 0.1f);
                    break;
                }

                case 1:
                {
                    WeaponTypeOne = "Large Weapons";
                    is2H = true;

                    Add("Valor", 50, 1.0f);
                    Add(WeaponTypeOne, 50, 0.9f);
                    Add("Parry", 28, 0.1f);
                    break;
                }
            }
        }
    }
}