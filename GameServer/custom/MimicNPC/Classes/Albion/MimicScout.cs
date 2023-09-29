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
	public class MimicScout : MimicNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MimicScout(byte level) : base(new ClassScout(), level)
		{
			MimicSpec = new ScoutSpec();

			DistributeSkillPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            MimicEquipment.SetRangedWeapon(this, eObjectType.Longbow);
            MimicEquipment.SetShield(this, 1);
            MimicEquipment.SetArmor(this, eObjectType.Studded);
            MimicEquipment.SetJewelry(this);
            RefreshItemBonuses();
            SwitchWeapon(eActiveWeaponSlot.Distance);
			RefreshSpecDependantSkills(false);
            IsCloakHoodUp = Util.RandomBool();
        }
	}

    public class ScoutSpec : MimicSpec
    {
        public ScoutSpec()
        {
            SpecName = "ScoutSpec";
            is2H = false;

            int randBaseWeap = Util.Random(1);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Slash"; break;
                case 1: WeaponTypeOne = "Thrust"; break;
            }

            int randVariance = Util.Random(1);

            switch (randVariance)
            {
                case 0:
                Add(WeaponTypeOne, 29, 0.6f);
                Add("Longbows", 44, 0.8f);
                Add("Shields", 42, 0.7f);
                Add("Stealth", 35, 0.1f);
                break;

                case 1:
                Add(WeaponTypeOne, 29, 0.7f);
                Add("Longbows", 50, 0.8f);
                Add("Shields", 35, 0.6f);
                Add("Stealth", 35, 0.1f);
                break;
            }
        }
    }
}