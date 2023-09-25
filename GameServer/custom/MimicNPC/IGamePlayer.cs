///*
///*
// * DAWN OF LIGHT - The first free open source DAoC server emulator
// *
// * This program is free software; you can redistribute it and/or
// * modify it under the terms of the GNU General Public License
// * as published by the Free Software Foundation; either version 2
// * of the License, or (at your option) any later version.
// *
// * This program is distributed in the hope that it will be useful,
// * but WITHOUT ANY WARRANTY; without even the implied warranty of
// * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// * GNU General Public License for more details.
// *
// * You should have received a copy of the GNU General Public License
// * along with this program; if not, write to the Free Software
// * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
// *
// */

//using DOL.AI.Brain;
//using DOL.Database;
//using DOL.Events;
//using DOL.GS.Housing;
//using DOL.GS.PacketHandler;
//using DOL.GS.PlayerTitles;
//using DOL.GS.Quests;
//using DOL.GS.RealmAbilities;
//using DOL.GS.Utils;
//using System;
//using System.Collections;
//using System.Collections.Concurrent;
//using System.Collections.Generic;

//namespace DOL.GS
//{
//    public interface IGamePlayer
//    {
//        string AccountName { get; }
//        int ActiveBuffCharges { get; set; }
//        GamePlayer.ControlledHorse ActiveHorse { get; }
//        IGameInventoryObject ActiveInventoryObject { get; set; }
//        byte ActiveSaddleBags { get; set; }
//        bool Advisor { get; set; }
//        long AreaUpdateTick { get; set; }
//        bool Autoloot { get; set; }
//        bool AutoSplitLoot { get; set; }
//        ConcurrentQueue<byte> AvailableQuestIndexes { get; }
//        byte BaseLevel { get; }
//        int BestArmorLevel { get; }
//        int BindAllowInterval { get; }
//        int BindHeading { get; set; }
//        int BindHouseHeading { get; set; }
//        int BindHouseRegion { get; set; }
//        int BindHouseXpos { get; set; }
//        int BindHouseYpos { get; set; }
//        int BindHouseZpos { get; set; }
//        int BindRegion { get; set; }
//        int BindXpos { get; set; }
//        int BindYpos { get; set; }
//        int BindZpos { get; set; }
//        GamePlayer Bodyguard { get; }
//        bool Boosted { get; set; }
//        long BountyPoints { get; set; }
//        int BountyPointsValue { get; }
//        bool CanBreathUnderWater { get; set; }
//        bool CanBuyRespec { get; }
//        bool CanGenerateNews { get; set; }
//        bool CanTradeAnyItem { get; }
//        bool CanUseCrossRealmItems { get; }
//        bool CanUseSlashLevel { get; }
//        int CapturedKeeps { get; set; }
//        int CapturedRelics { get; set; }
//        int CapturedTowers { get; set; }
//        bool Champion { get; set; }
//        long ChampionExperience { get; set; }
//        long ChampionExperienceForCurrentLevel { get; }
//        long ChampionExperienceForNextLevel { get; }
//        int ChampionLevel { get; set; }
//        ushort ChampionLevelPermill { get; }
//        int ChampionMaxLevel { get; }
//        int ChampionSpecialtyPoints { get; }
//        ICharacterClass CharacterClass { get; }
//        int Charisma { get; }
//        bool ClassNameFlag { get; set; }
//        GameClient Client { get; }
//        IPlayerTitle CLTitle { get; }
//        double CombatRegen { get; set; }
//        int Concentration { get; }
//        int Constitution { get; }
//        int Copper { get; }
//        CraftAction CraftAction { get; set; }
//        eCraftingSkill CraftingPrimarySkill { get; set; }
//        int CraftingSkillBonus { get; }
//        Dictionary<eCraftingSkill, int> CraftingSkills { get; }
//        double CraftingSpeed { get; }
//        ECSGameTimer CraftTimer { get; set; }
//        IPlayerTitle CraftTitle { get; }
//        DateTime CreationDate { get; }
//        ushort CreationModel { get; }
//        IList<IArea> CurrentAreas { get; set; }
//        Region CurrentRegion { set; }
//        short CurrentSpeed { set; }
//        IPlayerTitle CurrentTitle { get; set; }
//        CustomDialogResponse CustomDialogCallback { get; set; }
//        byte CustomisationStep { get; set; }
//        long DamageRvRMemory { get; set; }
//        int DBMaxEndurance { get; set; }
//        byte DeathCount { get; set; }
//        int DeathsPvP { get; set; }
//        long DeathTime { get; set; }
//        eDeathType DeathType { get; set; }
//        int Dexterity { get; }
//        long DisabledCastingTimeout { get; set; }
//        int DisplayedWeaponSkill { get; }
//        ConcurrentDictionary<GameDoorBase, long> DoorUpdateCache { get; }
//        GamePlayer DuelTarget { get; }
//        double Effectiveness { get; set; }
//        int EffectiveOverallAF { get; }
//        int Empathy { get; }
//        int Encumberance { get; }
//        int Endchant { get; set; }
//        int EnduDebuff { get; set; }
//        int Endurance { get; set; }
//        ECSGameTimer EnduRegenTimer { get; }
//        bool EnteredGame { get; set; }
//        long Experience { get; set; }
//        long ExperienceForCurrentLevel { get; }
//        long ExperienceForCurrentLevelSecondStage { get; }
//        long ExperienceForNextLevel { get; }
//        long ExperienceValue { get; }
//        byte FreeLevelState { get; }
//        bool GainRP { get; set; }
//        bool GainXP { get; set; }
//        eGameObjectType GameObjectType { get; }
//        eGender Gender { get; set; }
//        bool GMStealthed { get; set; }
//        int Gold { get; }
//        bool GroundTargetInView { get; set; }
//        Guild Guild { get; set; }
//        GuildBanner GuildBanner { get; set; }
//        string GuildID { get; set; }
//        string GuildName { get; set; }
//        string GuildNote { get; set; }
//        DBRank GuildRank { get; set; }
//        bool HasHorse { get; }
//        bool HCCompleted { get; set; }
//        bool HCFlag { get; set; }
//        ushort Heading { set; }
//        int Health { get; set; }
//        byte HealthPercentGroupWindow { get; }
//        bool HideSpecializationAPI { get; set; }
//        ConcurrentDictionary<House, long> HouseUpdateCache { get; }
//        ArrayList IgnoreList { get; set; }
//        bool IgnoreStatistics { get; set; }
//        bool InCombat { get; }
//        int Intelligence { get; }
//        bool IsAlive { get; }
//        bool IsAllowedToFly { get; set; }
//        bool IsAnonymous { get; set; }
//        bool IsAttackable { get; }
//        bool IsCastingRealmAbility { get; }
//        bool IsClimbing { get; set; }
//        bool IsCloakHoodUp { get; set; }
//        bool IsCloakInvisible { get; set; }
//        bool IsCrafting { get; }
//        bool IsDiving { get; set; }
//        bool IsEligibleToGiveMeritPoints { get; set; }
//        bool IsHelmInvisible { get; set; }
//        bool isInBG { get; set; }
//        bool IsInvulnerableToAttack { get; }
//        bool IsJumping { get; set; }
//        bool IsLevelRespecUsed { get; set; }
//        bool IsLevelSecondStage { get; set; }
//        bool IsMoving { get; }
//        bool IsOnHorse { get; set; }
//        bool IsOverencumbered { get; set; }
//        bool IsPraying { get; }
//        bool IsRiding { get; }
//        bool IsSalvagingOrRepairing { get; }
//        bool IsShade { get; }
//        bool IsSitting { get; set; }
//        bool IsSprinting { get; }
//        bool IsStealthed { get; }
//        bool IsStrafing { get; set; }
//        bool IsSummoningMount { get; }
//        bool IsSwimming { get; set; }
//        bool IsTorchLighted { get; set; }
//        bool IsWireframe { get; set; }
//        ConcurrentDictionary<GameStaticItem, long> ItemUpdateCache { get; }
//        int KillsAlbionDeathBlows { get; set; }
//        int KillsAlbionPlayers { get; set; }
//        int KillsAlbionSolo { get; set; }
//        int KillsDragon { get; set; }
//        int KillsEpicBoss { get; set; }
//        int KillsHiberniaDeathBlows { get; set; }
//        int KillsHiberniaPlayers { get; set; }
//        int KillsHiberniaSolo { get; set; }
//        int KillsLegion { get; set; }
//        int KillsMidgardDeathBlows { get; set; }
//        int KillsMidgardPlayers { get; set; }
//        int KillsMidgardSolo { get; set; }
//        int KillStreak { get; set; }
//        long LastAttackedByEnemyTickPvE { set; }
//        long LastAttackedByEnemyTickPvP { set; }
//        long LastAttackTickPvE { set; }
//        long LastAttackTickPvP { set; }
//        bool LastDeathPvP { get; set; }
//        long LastDeathRealmPoints { get; set; }
//        long LastEnduTick { get; set; }
//        int LastFreeLevel { get; set; }
//        DateTime LastFreeLeveled { get; set; }
//        DateTime LastLevelUp { get; set; }
//        string LastName { get; set; }
//        DateTime LastPlayed { get; }
//        Point3DFloat LastPositionUpdatePoint { get; set; }
//        long LastPositionUpdateTick { get; set; }
//        Zone LastPositionUpdateZone { get; set; }
//        GameLocation[] LastUniqueLocations { get; }
//        long LastWorldUpdate { get; set; }
//        byte Level { get; set; }
//        ushort LevelPermill { get; }
//        bool LookingForGroup { get; set; }
//        int Mana { get; set; }
//        int MaxConcentration { get; }
//        int MaxEncumberance { get; }
//        int MaxEndurance { get; set; }
//        int MaxLastZ { get; set; }
//        byte MaxLevel { get; }
//        int MaxMana { get; }
//        short MaxSpeedBase { get; set; }
//        MinotaurRelic MinotaurRelic { get; set; }
//        AbstractMission Mission { get; set; }
//        int Mithril { get; }
//        long MLExperience { get; set; }
//        bool MLGranted { get; set; }
//        int MLLevel { get; set; }
//        byte MLLine { get; set; }
//        IPlayerTitle MLTitle { get; }
//        ushort Model { get; set; }
//        long MoneyValue { get; }
//        string Name { get; set; }
//        bool NoHelp { get; set; }
//        double NonCombatNonSprintRegen { get; set; }
//        byte NotDisplayedInHerald { get; set; }
//        ConcurrentDictionary<GameNPC, ClientService.CachedNpcValues> NpcUpdateCache { get; }
//        string ObjectId { get; set; }
//        IPacketLib Out { get; }
//        int OutOfClassROGPercent { get; set; }
//        int Piety { get; }
//        int Platinum { get; }
//        long PlayedTime { get; }
//        long PlayedTimeSinceLevel { get; }
//        ECSGameTimer PredatorTimeoutTimer { get; set; }
//        DateTime PreviousLoginDate { get; set; }
//        ConcurrentDictionary<AbstractQuest, byte> QuestList { get; }
//        string QuestPlayerID { get; }
//        int Quickness { get; }
//        int QuitTime { get; set; }
//        short Race { get; set; }
//        string RaceName { get; }
//        PlayerDeck RandomNumberDeck { get; set; }
//        eRealm Realm { get; set; }
//        ECSGameTimer RealmAbilityCastTimer { get; set; }
//        int RealmLevel { get; set; }
//        long RealmPoints { get; set; }
//        int RealmPointsValue { get; }
//        int RealmSpecialtyPoints { get; }
//        string RealmTitle { get; }
//        bool ReceiveROG { get; set; }
//        int Regen { get; set; }
//        double RegenAfterTireless { get; set; }
//        double RegenBuff { get; set; }
//        int RegenRateAtChange { get; set; }
//        eReleaseType ReleaseType { get; }
//        int RespecAmountAllSkill { get; set; }
//        int RespecAmountChampionSkill { get; set; }
//        int RespecAmountDOL { get; set; }
//        int RespecAmountRealmSkill { get; set; }
//        int RespecAmountSingleSkill { get; set; }
//        int RespecBought { get; set; }
//        long RespecCost { get; }
//        bool RPFlag { get; set; }
//        bool SafetyFlag { get; set; }
//        List<int> SelfBuffChargeIDs { get; }
//        string[] SerializedFriendsList { get; set; }
//        string[] SerializedIgnoreList { get; set; }
//        ShadeECSGameEffect ShadeEffect { get; set; }
//        ushort ShadeModel { get; }
//        bool ShowGuildLogins { get; set; }
//        bool ShowXFireInfo { get; set; }
//        GameSiegeWeapon SiegeWeapon { get; set; }
//        int Silver { get; }
//        eSize Size { get; set; }
//        int SkillSpecialtyPoints { get; }
//        double SpecLock { get; set; }
//        bool SpellQueue { get; set; }
//        IPlayerStatistics Statistics { get; }
//        bool StatsAnonFlag { get; set; }
//        GameNPC Steed { get; set; }
//        int Strength { get; }
//        bool Stuck { get; set; }
//        bool TargetInView { get; set; }
//        int TargetInViewAlwaysTrueMinRange { get; }
//        AbstractTask Task { get; set; }
//        ISet<IPlayerTitle> Titles { get; }
//        int TotalConstitutionLostAtDeath { get; set; }
//        ITradeWindow TradeWindow { get; set; }
//        bool UseDetailedCombatLog { get; set; }
//        bool UsedLevelCommand { get; set; }
//        InventoryItem UseItem { get; set; }
//        byte WarMapPage { get; set; }
//        bool WasMovedByCorpseSummoner { get; set; }
//        int X { set; }
//        eXPLogState XPLogState { get; set; }
//        int Y { set; }
//        int Z { set; }

//        event GamePlayer.DismountSteedHandler OnDismountSteed;
//        event GamePlayer.MountSteedHandler OnMountSteed;
//        event GamePlayer.SendHandler OnSend;
//        event GamePlayer.SendReceiveHandler OnSendReceive;

//        void Achieve(string achievementName, int count = 1);
//        void AddAbility(Ability ability, bool sendUpdates);
//        bool AddCraftingSkill(eCraftingSkill skill, int startValue);
//        void AddFinishedQuest(AbstractQuest quest);
//        void AddMoney(long money);
//        void AddMoney(long money, string messageFormat);
//        void AddMoney(long money, string messageFormat, eChatType ct, eChatLoc cl);
//        bool AddQuest(AbstractQuest quest);
//        void AddRealmAbility(RealmAbility ability, bool sendUpdates);
//        void AddSpecialization(Specialization skill);
//        void AddSpellLine(SpellLine line);
//        void AddSpellLine(SpellLine line, bool notify);
//        bool AddTitle(IPlayerTitle title);
//        bool AddToWorld();
//        bool ApplyPoison(InventoryItem poisonPotion, InventoryItem toItem);
//        double ApplyWeaponQualityAndConditionToDamage(InventoryItem weapon, double damage);
//        void Bind(bool forced);
//        double CalcFallDamage(int fallDamagePercent);
//        eArmorSlot CalculateArmorHitLocation(AttackData ad);
//        int CalculateCastingTime(SpellLine line, Spell spell);
//        int CalculateMaxHealth(int level, int constitution);
//        int CalculateMaxMana(int level, int manaStat);
//        bool CanCastWhileAttacking();
//        bool CanDetect(GamePlayer enemy);
//        bool CanSeeObject(GameObject obj);
//        bool CanUseHorseInventorySlot(int slot);
//        bool CastSpell(ISpellCastingAbilityHandler ab);
//        void ChampionLevelUp();
//        void ChangeBaseStat(eStat stat, short val);
//        void ClearDismountSteedHandlers();
//        void ClearMountSteedHandlers();
//        void ClearOnSend();
//        void ClearOnSendReceive();
//        void ClearSpellQueue();
//        void CommandNpcAgressive();
//        void CommandNpcAttack();
//        void CommandNpcComeHere();
//        void CommandNpcDefensive();
//        void CommandNpcFollow();
//        void CommandNpcGoTarget();
//        void CommandNpcPassive();
//        void CommandNpcRelease();
//        void CommandNpcStay();
//        void CraftItem(ushort itemID);
//        WorldInventoryItem CreateItemOnTheGround(InventoryItem item);
//        void CreateStatistics();
//        void Delete();
//        bool DelveItem<T>(T item, List<string> delveInfo);
//        bool DelveSpell(IList<string> output, Spell spell, SpellLine spellLine);
//        void DisableSkill(Skill skill, int duration);
//        void DisableSkills(ICollection<Tuple<Skill, int>> skills);
//        bool DismountSteed(bool forced);
//        bool DropItem(eInventorySlot slot_pos);
//        bool DropItem(eInventorySlot slot_pos, out WorldInventoryItem droppedItem);
//        void DuelStart(GamePlayer duelTarget);
//        void DuelStop();
//        void EnemyKilled(GameLiving enemy);
//        void ForceGainExperience(long expTotal);
//        IList<string> FormatStatistics();
//        void GainBountyPoints(long amount);
//        void GainBountyPoints(long amount, bool modify);
//        void GainBountyPoints(long amount, bool modify, bool sendMessage);
//        void GainBountyPoints(long amount, bool modify, bool sendMessage, bool notify);
//        void GainChampionExperience(long experience);
//        void GainChampionExperience(long experience, eXPSource source);
//        bool GainCraftingSkill(eCraftingSkill skill, int count);
//        void GainExperience(eXPSource xpSource, long expTotal, long expCampBonus, long expGroupBonus, long atlasBonus, long expOutpostBonus, bool sendMessage);
//        void GainExperience(eXPSource xpSource, long expTotal, long expCampBonus, long expGroupBonus, long atlasBonus, long expOutpostBonus, bool sendMessage, bool allowMultiply);
//        void GainExperience(eXPSource xpSource, long expTotal, long expCampBonus, long expGroupBonus, long expOutpostBonus, long atlasBonus, bool sendMessage, bool allowMultiply, bool notify);
//        void GainRealmPoints(long amount);
//        void GainRealmPoints(long amount, bool modify);
//        void GainRealmPoints(long amount, bool modify, bool sendMessage);
//        void GainRealmPoints(long amount, bool modify, bool sendMessage, bool notify);
//        int GetAchievementProgress(string achievementName);
//        List<Tuple<SpellLine, List<Skill>>> GetAllUsableListSpells(bool update = false);
//        List<Tuple<Skill, Skill>> GetAllUsableSkills(bool update = false);
//        double GetArmorAbsorb(eArmorSlot slot);
//        double GetArmorAF(eArmorSlot slot);
//        int GetAutoTrainPoints(Specialization spec, int Mode);
//        int GetBaseSpecLevel(string keyName);
//        double GetBlockChance();
//        ICollection<string> GetBonuses();
//        long GetChampionExperienceForLevel(int level);
//        byte GetCountMLStepsCompleted(byte ml);
//        int GetCraftingSkillValue(eCraftingSkill skill);
//        long GetCurrentMoney();
//        int GetDamageResist(eProperty property);
//        byte GetDisplayLevel(GamePlayer player);
//        double GetEvadeChance();
//        IList GetExamineMessages(GamePlayer player);
//        long GetExperienceNeededForLevel(int level);
//        byte GetFaceAttribute(eCharFacePart part);
//        List<AbstractQuest> GetFinishedQuests();
//        long GetMLExperienceForLevel(int level);
//        string GetMLStepDescription(byte ml, int step);
//        int GetModifiedSpecLevel(string keyName);
//        string GetName(GamePlayer target);
//        IList GetNonTrainableSkillList();
//        double GetParryChance();
//        string GetPronoun(GameClient Client, int form, bool capitalize);
//        string GetPronoun(int form, bool firstLetterUppercase);
//        List<RealmAbility> GetRealmAbilities();
//        Specialization GetSpecializationByName(string name, bool caseSensitive = false);
//        IList<Specialization> GetSpecList();
//        SpellLine GetSpellLine(string keyname);
//        List<SpellLine> GetSpellLines();
//        byte GetStepCountForML(byte ml);
//        double GetWeaponSkill(InventoryItem weapon);
//        string GetWeaponSpec(InventoryItem weapon);
//        int GetWeaponStat(InventoryItem weapon);
//        long GetXPForML(byte ml);
//        bool HasFinishedMLStep(int mlLevel, int step);
//        bool HasFinishedQuest(AbstractQuest quest);
//        int HasFinishedQuest(Type questType);
//        bool HasSpecialization(string keyName);
//        AbstractQuest IsDoingQuest(AbstractQuest quest);
//        AbstractQuest IsDoingQuest(Type questType);
//        bool IsIgnoring(GameLiving source);
//        string ItemBonusName(int BonusType);
//        void LeaveHouse();
//        void LoadClassSpecializations(bool sendMessages);
//        void LoadFromDatabase(DataObject obj);
//        void LoadQuests();
//        void MessageFromArea(GameObject source, string message, eChatType chatType, eChatLoc chatLocation);
//        void MessageToSelf(string message, eChatType chatType);
//        void ModifyIgnoreList(string Name, bool remove);
//        bool MountSteed(GameNPC steed, bool forced);
//        bool MoveTo(ushort regionID, int x, int y, int z, ushort heading);
//        bool MoveToBind();
//        void Notify(DOLEvent e);
//        void Notify(DOLEvent e, EventArgs args);
//        void Notify(DOLEvent e, object sender);
//        void Notify(DOLEvent e, object sender, EventArgs args);
//        void OnAttackedByEnemy(AttackData ad);
//        void OnLevelSecondStage();
//        void OnLevelUp(int previouslevel);
//        void OnLinkdeath();
//        void OnPlayerMove();
//        void OnRevive(DOLEvent e, object sender, EventArgs args);
//        void OnSkillTrained(Specialization skill);
//        bool OpenSelfCraft(InventoryItem item);
//        bool OpenTrade(GamePlayer tradePartner);
//        void PickupObject(GameObject floorObject, bool checkRange);
//        void Pray();
//        void PrayTimerStop();
//        bool PrivateMessageReceive(GamePlayer source, string str);
//        void ProcessDeath(GameObject killer);
//        bool Quit(bool forced);
//        void RaiseRealmLoyaltyFloor(int amount);
//        string RealmRankTitle(string language);
//        bool ReceiveItem(GameLiving source, InventoryItem item);
//        bool ReceiveTradeItem(GamePlayer source, InventoryItem item);
//        bool ReceiveTradeMoney(GamePlayer source, long money);
//        void RefreshItemBonuses();
//        void RefreshSpecDependantSkills(bool sendMessages);
//        void RefreshWorld();
//        void Release(eReleaseType releaseCommand, bool forced);
//        void RemoveAllAbilities();
//        void RemoveAllSpecs();
//        void RemoveAllSpellLines();
//        bool RemoveBountyPoints(long amount);
//        bool RemoveBountyPoints(long amount, string str);
//        bool RemoveBountyPoints(long amount, string str, eChatType type, eChatLoc loc);
//        void RemoveChampionLevels();
//        bool RemoveEncounterCredit(Type questType);
//        void RemoveFinishedQuest(AbstractQuest quest);
//        void RemoveFinishedQuests(Predicate<AbstractQuest> match);
//        bool RemoveFromWorld();
//        bool RemoveMoney(long money);
//        bool RemoveMoney(long money, string messageFormat);
//        bool RemoveMoney(long money, string messageFormat, eChatType ct, eChatLoc cl);
//        bool RemoveSpecialization(string specKeyName);
//        bool RemoveSpellLine(string lineKeyName);
//        bool RemoveTitle(IPlayerTitle title);
//        void RepairItem(InventoryItem item);
//        void Reset();
//        bool RespecAll();
//        void RespecChampionSkills();
//        bool RespecDOL();
//        bool RespecRealm(bool useRespecPoint = true);
//        int RespecSingle(Specialization specLine);
//        void SalvageItem(InventoryItem item);
//        void SalvageItemList(IList<InventoryItem> itemList);
//        void SalvageSiegeWeapon(GameSiegeWeapon siegeWeapon);
//        void SaveIntoDatabase();
//        bool Say(string str);
//        bool SayReceive(GameLiving source, string str);
//        bool SendPrivateMessage(GamePlayer target, string str);
//        void SendTrainerWindow();
//        void SetAchievementTo(string achievementName, int value);
//        bool SetCharacterClass(int id);
//        void SetControlledBrain(IControlledBrain controlledBrain);
//        void SetFinishedMLStep(int mlLevel, int step, bool setFinished = true);
//        void SetGroundTarget(int groundX, int groundY, int groundZ);
//        void Shade(bool state);
//        void Sit(bool sit);
//        bool Sprint(bool state);
//        void StartEnduranceRegeneration();
//        void StartHealthRegeneration();
//        bool StartInvulnerabilityTimer(int duration, GamePlayer.InvulnerabilityExpiredCallback callback);
//        void StartPowerRegeneration();
//        void StartStealthUncoverAction();
//        void Stealth(bool goStealth);
//        void StopEnduranceRegeneration();
//        void StopHealthRegeneration();
//        void StopPowerRegeneration();
//        void StopReleaseTimer();
//        void StopStealthUncoverAction();
//        void SwitchQuiver(eActiveQuiverSlot slot, bool forced);
//        void SwitchSeat(int slot);
//        void SwitchWeapon(eActiveWeaponSlot slot);
//        void TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount);
//        string ToString();
//        void UncoverLOSHandler(GamePlayer player, ushort response, ushort targetOID);
//        void UpdateCurrentTitle();
//        void UpdateEncumberance();
//        void UpdateEquipmentAppearance();
//        void UpdateHealthManaEndu();
//        void UpdatePlayerStatus();
//        void UpdateWaterBreathState(eWaterBreath state);
//        void UseSlot(eInventorySlot slot, eUseType type);
//        void UseSlot(int slot, int type);
//        int VerifySpecPoints();
//        int WeaponBaseSpecLevel(InventoryItem weapon);
//        double WeaponDamage(InventoryItem weapon);
//        double WeaponDamageWithoutQualityAndCondition(InventoryItem weapon);
//        int WeaponSpecLevel(InventoryItem weapon);
//        bool Whisper(GameObject target, string str);
//        bool WhisperReceive(GameLiving source, string str);
//        bool Yell(string str);
//        bool YellReceive(GameLiving source, string str);
//    }
//}