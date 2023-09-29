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
	public class MimicTheurgist : MimicNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MimicTheurgist(byte level) : base(new ClassTheurgist(), level)
		{
			MimicSpec = new TheurgistSpec();

			DistributeSkillPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.twoHand);
            MimicEquipment.SetArmor(this, eObjectType.Cloth);
            MimicEquipment.SetJewelry(this);
            RefreshItemBonuses();
            SwitchWeapon(eActiveWeaponSlot.TwoHanded);
			RefreshSpecDependantSkills(false);
			SetCasterSpells();
            IsCloakHoodUp = Util.RandomBool();
        }
	}

    public class TheurgistSpec : MimicSpec
    {
        public TheurgistSpec()
        {
            SpecName = "TheurgistSpec";

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(2);

            switch (randVariance)
            {
                case 0:
                Add("Earth Magic", 28, 0.1f);
                Add("Cold Magic", 20, 0.0f);
				Add("Wind Magic", 45, 1.0f);
                break;

                case 1:
                Add("Earth Magic", 4, 0.0f);
                Add("Cold Magic", 50, 1.0f);
                Add("Wind Magic", 20, 0.1f);
                break;

                case 2:
                Add("Earth Magic", 50, 1.0f);
                Add("Cold Magic", 4, 0.0f);
                Add("Wind Magic", 20, 0.1f);
                break;
            }
        }
    }
}