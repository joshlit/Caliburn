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
	public class MimicWarrior : MimicNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MimicWarrior(byte level) : base(new ClassWarrior(), level)
		{
			MimicSpec = new WarriorSpec();

			DistributeSkillPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.twoHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
			MimicEquipment.SetShield(this, 3);
            //MimicEquipment.SetRangedWeapon(this, eObjectType.Thrown);

            eObjectType objectType;

            if (level < 10)
                objectType = eObjectType.Studded;
            else
                objectType = eObjectType.Chain;

            MimicEquipment.SetArmor(this, objectType);
            MimicEquipment.SetJewelry(this);
            RefreshItemBonuses();
            SwitchWeapon(eActiveWeaponSlot.Standard);
			RefreshSpecDependantSkills(false);
            IsCloakHoodUp = Util.RandomBool();
        }
	}

	public class WarriorSpec : MimicSpec
	{
		public WarriorSpec()
		{
            SpecName = "WarriorSpec";

            int randBaseWeap = Util.Random(2);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Sword"; break;
                case 1: WeaponTypeOne = "Axe"; break;
                case 2: WeaponTypeOne = "Hammer"; break;
            }

            int randVariance = Util.Random(1);

			switch (randVariance)
			{
				case 0:
                Add(WeaponTypeOne, 50, 0.8f);
                Add("Shields", 42, 0.5f);
                Add("Parry", 39, 0.2f);
				break;

                case 1:
                Add(WeaponTypeOne, 50, 0.8f);
                Add("Shields", 50, 0.5f);
                Add("Parry", 28, 0.2f);
                break;
            }
        }
	}
}