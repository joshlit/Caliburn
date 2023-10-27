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
	public class MimicBerserker : MimicNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MimicBerserker(byte level) : base(new ClassBerserker(), level)
		{
			MimicSpec = new BerserkerSpec();

			DistributeSkillPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.twoHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeTwo, eHand.leftHand);
            MimicEquipment.SetArmor(this, eObjectType.Studded);
			MimicEquipment.SetJewelry(this);
            RefreshItemBonuses();
            SwitchWeapon(eActiveWeaponSlot.Standard);
			RefreshSpecDependantSkills(false);
            IsCloakHoodUp = Util.RandomBool();
        }
	}

	public class BerserkerSpec : MimicSpec
	{
		public BerserkerSpec()
		{
            SpecName = "BerserkerSpec";

            int randBaseWeap = Util.Random(2);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Sword"; break;
                case 1: WeaponTypeOne = "Axe"; break;
                case 2: WeaponTypeOne = "Hammer"; break;
            }

			WeaponTypeTwo = "Axe";

            int randVariance = Util.Random(2);

			switch (randVariance)
			{
				case 0:
				case 1:
                Add(WeaponTypeOne, 50, 0.8f);
                Add("Left Axe", 50, 1.0f);
                Add("Parry", 28, 0.2f);
				break;

                case 2:
                Add(WeaponTypeOne, 44, 0.8f);
                Add("Left Axe", 50, 1.0f);
                Add("Parry", 37, 0.2f);
                break;
            }
        }
	}
}