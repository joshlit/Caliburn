using DOL.GS.PlayerClass;

namespace DOL.GS.Scripts
{
    public class MimicWizard : MimicNPC
    {
        public MimicWizard(byte level) : base(new ClassWizard(), level)
        {
            MimicSpec = new WizardSpec();

            SpendSpecPoints();
            MimicEquipment.SetMeleeWeapon(this, MimicSpec.WeaponTypeOne, eHand.twoHand);
            MimicEquipment.SetArmor(this, eObjectType.Cloth);
            MimicEquipment.SetJewelryROG(this, Realm, (eCharacterClass)CharacterClass.ID, Level, eObjectType.Magical);
            RefreshItemBonuses();
            SwitchWeapon(eActiveWeaponSlot.TwoHanded);
            RefreshSpecDependantSkills(false);
            SetCasterSpells();
            IsCloakHoodUp = Util.RandomBool();
        }
    }

    public class WizardSpec : MimicSpec
    {
        public WizardSpec()
        {
            SpecName = "WizardSpec";

            WeaponTypeOne = "Staff";

            int randVariance = Util.Random(2);

            switch (randVariance)
            {
                case 0:
                Add("Earth Magic", 50, 1.0f);
                Add("Cold Magic", 20, 0.1f);
                break;

                case 1:
                Add("Earth Magic", 24, 0.1f);
                Add("Cold Magic", 48, 1.0f);
                break;

                case 2:
                Add("Cold Magic", 20, 0.1f);
                Add("Fire Magic", 50, 1.0f);
                break;
            }
        }
    }
}