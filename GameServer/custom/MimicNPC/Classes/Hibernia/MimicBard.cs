using System;
using System.Reflection;
using DOL.GS;
using DOL.GS.Scripts;
using DOL.Database;
using log4net;
using DOL.GS.Realm;
using System.Collections.Generic;
using System.Web;
using static DOL.GS.CommanderPet;
using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
	public class MimicBard : MimicNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MimicBard(byte level) : base(new ClassBard(), level)
		{
            MimicSpec = new BardSpec();

			DistributeSkillPoints();
			MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.oneHand);
			MimicEquipment.SetShield(this, 1);
			MimicEquipment.SetArmor(this, eObjectType.Reinforced);
			MimicEquipment.SetJewelry(this);
            RefreshItemBonuses();
            SwitchWeapon(eActiveWeaponSlot.Standard);
            RefreshSpecDependantSkills(false);
			SetSpells();
            IsCloakHoodUp = Util.RandomBool();
        }
	}

	public class BardSpec : MimicSpec
	{
		public BardSpec() 
		{
			SpecName = "BardSpec";
            is2H = false;

            int randBaseWeap = Util.Random(1);

            switch (randBaseWeap)
            {
                case 0: WeaponTypeOne = "Blades"; break;
                case 1: WeaponTypeOne = "Blunt"; break;
            }

            int randVariance = Util.Random(2);

            switch (randVariance)
            {
                case 0:
                Add("Music", 47, 0.8f);
                Add("Nurture", 43, 0.9f);
                Add("Regrowth", 16, 0.1f);
                break;

                case 1:
                Add("Music", 37, 0.4f);
                Add("Nurture", 43, 0.9f);
                Add("Regrowth", 33, 0.7f);
                break;

                case 2:
                Add(WeaponTypeOne, 29, 0.7f);
                Add("Music", 37, 0.4f);
                Add("Nurture", 43, 0.9f);
                Add("Regrowth", 16, 0.1f);
                break;
            }
        }
	}
}