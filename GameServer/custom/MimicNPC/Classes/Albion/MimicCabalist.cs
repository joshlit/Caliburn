using System;
using System.Reflection;
using DOL.GS;
using DOL.GS.Scripts;
using DOL.Database;
using log4net;
using DOL.GS.Realm;
using System.Collections.Generic;
using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
	public class MimicCabalist : MimicNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MimicCabalist(GameLiving owner, byte level = 0, Point3D position = null) : base(owner, new ClassCabalist(), level, position)
		{
            MimicSpec = MimicManager.Random(this);

            DistributeSkillPoints();
            SetMeleeWeapon(MimicSpec.WeaponTypeOne);
            SetArmor(eObjectType.Cloth);
			SetJewelry();

			//foreach (InventoryItem item in Inventory.EquippedItems)
			//{
			//	if (item == null)
			//		return;

			//	if (item.Quality < 90)
			//	{
			//		item.Quality = Util.Random(90, 100);
			//	}

			//	log.Debug("Name: " + item.Name);
			//	log.Debug("Slot: " + Enum.GetName(typeof(eInventorySlot), item.SlotPosition));
			//	log.Debug("DPS_AF: " + item.DPS_AF);
			//	log.Debug("SPD_ABS: " + item.SPD_ABS);
			//}

			SwitchWeapon(eActiveWeaponSlot.TwoHanded);

			RefreshSpecDependantSkills(false);
			SetCasterSpells();
		}
	}

	public class MatterCabalist : MimicSpec
	{
		public MatterCabalist()
		{
			SpecName = "MatterCabalist";

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(3);

			switch (randVariance)
			{
				case 0:
				{
                    Add("Matter Magic", 50, 1.0f);
                    Add("Spirit Magic", 20, 0.1f);
                    break;
                }

				case 1:
				{
					Add("Matter Magic", 49, 1.0f);
					Add("Spirit Magic", 22, 0.1f);
					break;
				}

				case 2:
				{
                    Add("Matter Magic", 46, 1.0f);
                    Add("Spirit Magic", 28, 0.1f);
                    break;
                }

                case 3:
                {
                    Add("Matter Magic", 46, 1.0f);         
                    Add("Body Magic", 28, 0.1f);
                    Add("Spirit Magic", 4, 0.0f);
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

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(6);

			switch (randVariance)
			{
				case 0:
				{
                    Add("Body Magic", 50, 1.0f);
                    Add("Spirit Magic", 20, 0.1f);
					break;
                }

                case 1:
                {
                    Add("Body Magic", 47, 1.0f);
                    Add("Spirit Magic", 26, 0.1f);
                    break;
                }

                case 2:
                {
                    Add("Body Magic", 46, 1.0f);
                    Add("Spirit Magic", 28, 0.2f);
                    break;
                }

                case 3:
                {
                    Add("Body Magic", 45, 1.0f);
                    Add("Spirit Magic", 29, 0.2f);
                    break;
                }

                case 4:
                {
                    Add("Body Magic", 47, 1.0f);
                    Add("Matter Magic", 25, 0.2f);
                    Add("Spirit Magic", 8, 0.1f);
                    break;
                }

                case 5:
                {
                    Add("Body Magic", 46, 1.0f);
                    Add("Matter Magic", 27, 0.2f);
                    Add("Spirit Magic", 8, 0.1f);
                    break;
                }

                case 6:
                {
                    Add("Body Magic", 45, 1.0f);
                    Add("Matter Magic", 29, 0.2f);
                    Add("Spirit Magic", 4, 0.1f);
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

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(6);

            switch (randVariance)
            {
                case 0:
                {
                    Add("Spirit Magic", 33, 0.9f);
                    Add("Body Magic", 28, 0.3f);
                    Add("Matter Magic", 32, 0.1f);
                    break;
                }

                case 1:
                {
                    Add("Spirit Magic", 38, 0.9f);
                    Add("Matter Magic", 38, 0.1f);
                    break;
                }

                case 2:
                {
                    Add("Spirit Magic", 38, 0.9f);
                    Add("Body Magic", 38, 0.1f);
                    break;
                }

                case 3:
                {
                    Add("Spirit Magic", 46, 1.0f);
                    Add("Body Magic", 28, 0.1f);
                    break;
                }

                case 4:
                {
                    Add("Spirit Magic", 47, 1.0f);
                    Add("Body Magic", 26, 0.1f);
                    break;
                }

                case 5:
                {
                    Add("Spirit Magic", 46, 1.0f);
                    Add("Body Magic", 28, 0.1f);
                    break;
                }

                case 6:
                {
                    Add("Spirit Magic", 50, 1.0f);
                    Add("Body Magic", 20, 0.1f);
                    break;
                }
            }
        }
	}
}