using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.PropertyCalc;
using DOL.GS.Utils;
using System.Collections.Generic;
using static DOL.GS.GamePlayer;

namespace DOL.GS.Scripts
{
    public interface IGamePlayer
    {
        public IPacketLib Out { get; }
        public GameClient Client { get; }

        public int GetModified(eProperty property);

        public int ChangeHealth(GameObject changeSource, eHealthChangeType healthChangeType, int changeAmount);
        public int CalculateMaxHealth(int level, int constitution);
        public int CalculateMaxMana(int level, int manaStat);
      
        public SpellLine GetSpellLine(string keyname);     
        public bool HasAbility(string keyName);
        public T GetAbility<T>() where T : Ability;
        public int GetAbilityLevel(string keyName);
        public bool HasSpecialization(string keyName);        
        public int GetBaseSpecLevel(string keyName);
        public int GetModifiedSpecLevel(string keyName);
        public void DisableSkill(Skill skill, int duration);

        public IPropertyIndexer AbilityBonus { get; }

        public eArmorSlot CalculateArmorHitLocation(AttackData ad);
        public double WeaponDamageWithoutQualityAndCondition(DbInventoryItem weapon);
        public double ApplyWeaponQualityAndConditionToDamage(DbInventoryItem weapon, double damage);

        public void Stealth(bool goStealth);

        public void OnDuelStart(GameDuel duel);
        public void OnDuelStop();
        public GameLiving DuelPartner { get; }
        public bool IsDuelPartner(GameLiving living);

        public void AddXPGainer(GameObject xpGainer, float damageAmount);
        public void GainRealmPoints(long amount, bool modify);
        public int GetDistanceTo(IPoint3D point);
        public bool IsWithinRadius(GameObject obj, int radius);
        public void CommandNpcRelease();

        public RangeAttackComponent RangeAttackComponent { get; }
        public AttackComponent AttackComponent { get; }
        public StyleComponent StyleComponent { get; }
        public EffectListComponent EffectListComponent { get; }
        public GameEffectList EffectList { get; }
        public PropertyCollection TempProperties { get; }
        public IPropertyIndexer ItemBonus { get; }
        public IPropertyIndexer BaseBuffBonusCategory { get; }
        public IPropertyIndexer SpecBuffBonusCategory { get; }
        public IPropertyIndexer DebuffCategory { get; }
        public IPropertyIndexer BuffBonusCategory4 { get; }

        public GameObject TargetObject { get; set; }

        public Group Group { get; }
        public Guild Guild { get; set; }

        public IGameInventory Inventory { get; set; }
        public DbInventoryItem ActiveWeapon { get; }
        public eActiveWeaponSlot ActiveWeaponSlot { get; }

        public IControlledBrain ControlledBrain { get; set; }
       
        public PlayerDeck RandomNumberDeck { get; set; }
        public List<int> SelfBuffChargeIDs { get; }
        public int TotalConstitutionLostAtDeath { get; set; }

        public ICharacterClass CharacterClass { get; }
        public  short Race { get; set; }
        public string Name { get; set; }
        public byte Level { get; set; }
        public byte MaxLevel { get; }
        public  int RealmLevel { get; set; }
        public int MLLevel { get; set; }
        public eRealm Realm { get; set; }

        public double Effectiveness { get; set; }
        public double SpecLock { get; set; }

        public bool IsAlive { get; }
        public bool InCombat { get; }
        public bool IsAttacking { get; }
        public bool IsCasting { get; }
        public bool IsStealthed { get; }
        public int Encumberance { get; }
        public int MaxEncumberance { get; }
        public bool IsOverencumbered { get; set; }
        public bool IsStunned { get; set; }
        public bool IsMezzed { get; set; }
        public bool IsMoving { get; }
        public bool IsSprinting { get; }
        public bool IsSitting { get; set; }
        public bool IsStrafing { get; set; }

        public ControlledHorse ActiveHorse { get; }
        public bool IsOnHorse { get; set; }

        public int MaxHealth { get; }
        public int Mana { get; set; }
        public int MaxMana { get; }
        
        public int Endurance { get; set; }
        public  short MaxSpeedBase { get; set; }

        public int Strength { get; }
        public int Dexterity { get; }
        public int Quickness { get; }
        public int Intelligence { get; }

        public Region CurrentRegion { get; set; }
        public int Z { get; }
    }
}