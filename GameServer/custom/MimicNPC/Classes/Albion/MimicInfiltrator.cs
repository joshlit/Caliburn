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
	public class MimicInfiltrator : MimicNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MimicInfiltrator(byte level) : base(new ClassInfiltrator(), level)
		{
			MimicSpec = new InfiltratorSpec();

			DistributeSkillPoints();
			MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.leftHand);
            MimicEquipment.SetArmor(this, eObjectType.Leather);
			//MimicEquipment.SetRangedWeapon(this, eObjectType.Crossbow);
			MimicEquipment.SetJewelry(this);
            RefreshItemBonuses();
            SwitchWeapon(eActiveWeaponSlot.Standard);
			RefreshSpecDependantSkills(false);
            IsCloakHoodUp = Util.RandomBool();
        }
	}

    public class InfiltratorSpec : MimicSpec
    {
        public InfiltratorSpec()
        {
            SpecName = "InfiltratorSpec";
            is2H = false;

            int randBaseWeap = Util.Random(1);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Slash"; break;
                case 1: WeaponTypeOne = "Thrust"; break;
            }

            int randVariance = Util.Random(3);

            switch (randVariance)
            {
                case 0:
                Add(WeaponTypeOne, 50, 0.8f);
                Add("Dual Wield", 14, 0.2f);
                Add("Critical Strike", 44, 0.9f);
                Add("Stealth", 37, 0.5f);
                Add("Envenom", 37, 0.6f);
                break;

                case 1:
                Add(WeaponTypeOne, 39, 0.8f);
                Add("Dual Wield", 25, 0.2f);
                Add("Critical Strike", 50, 0.9f);
                Add("Stealth", 37, 0.5f);
                Add("Envenom", 37, 0.6f);
                break;

                case 2:
                Add(WeaponTypeOne, 39, 0.8f);
                Add("Dual Wield", 50, 0.9f);
                Add("Critical Strike", 21, 0.3f);
                Add("Stealth", 38, 0.5f);
                Add("Envenom", 38, 0.6f);
                break;

                case 3:
                Add(WeaponTypeOne, 39, 0.8f);
                Add("Dual Wield", 38, 0.9f);
                Add("Critical Strike", 44, 0.3f);
                Add("Stealth", 35, 0.5f);
                Add("Envenom", 35, 0.6f);
                break;
            }
        }
    }
}