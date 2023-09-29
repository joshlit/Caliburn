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
	public class MimicCleric : MimicNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MimicCleric(byte level) : base(new ClassCleric(), level)
		{
			MimicSpec = new ClericSpec();

			DistributeSkillPoints();
			MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            MimicEquipment.SetShield(this, 2);
            MimicEquipment.SetArmor(this, eObjectType.Chain);
			MimicEquipment.SetJewelry(this);
            RefreshItemBonuses();
            SwitchWeapon(eActiveWeaponSlot.Standard);
			RefreshSpecDependantSkills(false);
			SetSpells();
            IsCloakHoodUp = Util.RandomBool();
		}
	}

	public class ClericSpec : MimicSpec
	{
		public ClericSpec()
		{
			SpecName = "ClericSpec";

			WeaponTypeOne = "Crush";

            int randVariance = Util.Random(7);

            switch (randVariance)
            {
                case 0:
				case 1:
				Add("Rejuvenation", 33, 0.5f);
				Add("Enhancement", 42, 0.8f);
                Add("Smite", 7, 0.0f);
				break;

				case 2:
				case 3:
                Add("Rejuvenation", 36, 0.5f);
                Add("Enhancement", 40, 0.8f);
                Add("Smite", 4, 0.0f);
                break;

                case 4:
                Add("Rejuvenation", 46, 0.8f);
                Add("Enhancement", 28, 0.5f);
                Add("Smite", 4, 0.0f);
                break;

				case 5:
                Add("Rejuvenation", 50, 0.8f);
                Add("Enhancement", 20, 0.5f);
                Add("Smite", 4, 0.0f);
				break;

                case 6:
                Add("Rejuvenation", 6, 0.0f);
                Add("Enhancement", 29, 0.5f);
                Add("Smite", 45, 0.8f);
                break;

                case 7:
                Add("Rejuvenation", 4, 0.0f);
                Add("Enhancement", 36, 0.5f);
                Add("Smite", 40, 0.8f);
                break;
            }
        }
	}
}