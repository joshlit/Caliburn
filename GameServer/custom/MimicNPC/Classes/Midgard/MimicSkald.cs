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
	public class MimicSkald : MimicNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MimicSkald(byte level) : base(new ClassSkald(), level)
		{
			MimicSpec = new SkaldSpec();

			DistributeSkillPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.twoHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
			MimicEquipment.SetShield(this, 1);
			MimicEquipment.SetArmor(this, eObjectType.Chain);
			MimicEquipment.SetJewelry(this);
            RefreshItemBonuses();
            SwitchWeapon(eActiveWeaponSlot.TwoHanded);
			RefreshSpecDependantSkills(false);
			SetSpells();
            IsCloakHoodUp = Util.RandomBool();
        }
	}

	public class SkaldSpec : MimicSpec
	{
		public SkaldSpec()
		{
            SpecName = "SkaldSpec";

            int randBaseWeap = Util.Random(2);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Sword"; break;
                case 1: WeaponTypeOne = "Axe"; break;
                case 2: WeaponTypeOne = "Hammer"; break;
            }

            int randVariance = Util.Random(2);

			switch (randVariance)
			{
				case 0:
				case 1:
                Add(WeaponTypeOne, 39, 0.7f);
                Add("Battlesongs", 50, 0.8f);
                Add("Parry", 28, 0.1f);
				break;

                case 2:
                Add(WeaponTypeOne, 44, 0.7f);
                Add("Battlesongs", 49, 0.8f);
                Add("Parry", 4, 0.1f);
                break;
            }
        }
	}
}