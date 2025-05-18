﻿using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS.API;
using DOL.GS.Realm;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DOL.GS.Scripts
{
    #region Battlegrounds

    public static class MimicBattlegrounds
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static MimicBattleground ThidBattleground;

        public static void Initialize()
        {
            ThidBattleground = new MimicBattleground(252,
                                                    new Point3D(37200, 51200, 3950),
                                                    new Point3D(19820, 19305, 4050),
                                                    new Point3D(53300, 26100, 4270),
                                                    300,
                                                    1500,
                                                    20,
                                                    24);
        }

        public class MimicBattleground
        {
            public MimicBattleground(ushort region, Point3D albSpawn, Point3D hibSpawn, Point3D midSpawn, int minMimics, int maxMimics, byte minLevel, byte maxLevel)
            {
                m_region = region;
                m_albSpawnPoint = albSpawn;
                m_hibSpawnPoint = hibSpawn;
                m_midSpawnPoint = midSpawn;
                m_minTotalMimics = minMimics;
                m_maxTotalMimics = maxMimics;
                m_minLevel = minLevel;
                m_maxLevel = maxLevel;
            }

            private ECSGameTimer m_masterTimer;

            private MimicSpawner m_albSpawner;
            private MimicSpawner m_hibSpawner;
            private MimicSpawner m_midSpawner;

            private int m_timerInterval = 600000; // 10 minutes
            private int m_dormantInterval;
            private long m_resetMaxTime = 0;

            private readonly List<BattleStats> m_battleStats = new List<BattleStats>();

            private Point3D m_albSpawnPoint;
            private Point3D m_hibSpawnPoint;
            private Point3D m_midSpawnPoint;

            private ushort m_region;

            private byte m_minLevel;
            private byte m_maxLevel;

            private int m_minTotalMimics;
            private int m_maxTotalMimics;

            private int m_currentMinTotalMimics;
            private int m_currentMaxTotalMimics;

            private int m_currentMaxAlb;
            private int m_currentMaxHib;
            private int m_currentMaxMid;

            private int m_groupChance = 50;

            public void Start()
            {
                if (m_masterTimer != null)
                {
                    if (!m_masterTimer.IsAlive)
                        m_masterTimer.Start();

                    m_albSpawner.Start();
                    m_hibSpawner.Start();
                    m_midSpawner.Start();
                }
                else
                {
                    ResetMaxMimics();

                    m_masterTimer = new ECSGameTimer(null, new ECSGameTimer.ECSTimerCallback(MasterTimerCallback), m_timerInterval);

                    m_albSpawner = new MimicSpawner(eRealm.Albion, m_minLevel, m_maxLevel, m_currentMaxAlb, m_albSpawnPoint, m_region, 0, true);
                    m_hibSpawner = new MimicSpawner(eRealm.Hibernia, m_minLevel, m_maxLevel, m_currentMaxHib, m_hibSpawnPoint, m_region, 0, true);
                    m_midSpawner = new MimicSpawner(eRealm.Midgard, m_minLevel, m_maxLevel, m_currentMaxMid, m_midSpawnPoint, m_region, 0, true);
                }
            }

            public void Stop()
            {
                m_masterTimer?.Stop();
                m_albSpawner?.Stop();
                m_hibSpawner?.Stop();
                m_midSpawner?.Stop();
            }

            public void Clear()
            {
                Stop();

                m_masterTimer = null;

                if (m_albSpawner != null)
                {
                    foreach (MimicNPC mimic in m_albSpawner.Mimics)
                        mimic.Delete();

                    m_albSpawner.Delete();
                    m_albSpawner = null;
                }

                if (m_hibSpawner != null)
                {
                    foreach (MimicNPC mimic in m_hibSpawner.Mimics)
                        mimic.Delete();

                    m_hibSpawner.Delete();
                    m_hibSpawner = null;
                }

                if (m_midSpawner != null)
                {
                    foreach (MimicNPC mimic in m_midSpawner.Mimics)
                        mimic.Delete();

                    m_midSpawner.Delete();
                    m_midSpawner = null;
                }
            }

            private int MasterTimerCallback(ECSGameTimer timer)
            {
                if (GameLoop.GameLoopTime > m_resetMaxTime &&
                    !m_albSpawner.IsRunning &&
                    !m_hibSpawner.IsRunning &&
                    !m_midSpawner.IsRunning)
                {
                    ResetMaxMimics();
                }

                int totalMimics = m_albSpawner.Mimics.Count + m_hibSpawner.Mimics.Count + m_midSpawner.Mimics.Count;
                log.Info("Alb: " + m_albSpawner.Mimics.Count + "/" + m_currentMaxAlb);
                log.Info("Hib: " + m_hibSpawner.Mimics.Count + "/" + m_currentMaxHib);
                log.Info("Mid: " + m_midSpawner.Mimics.Count + "/" + m_currentMaxMid);
                log.Info("Total Mimics: " + totalMimics + "/" + m_currentMaxTotalMimics);

                return m_timerInterval + Util.Random(-300000, 300000); // 10 minutes + or - 5 minutes
            }

            /// <summary>
            /// Gets a new total maximum and minimum of mimics for each realm randomly.
            /// </summary>
            private void ResetMaxMimics()
            {
                m_currentMaxTotalMimics = Util.Random(m_minTotalMimics, m_maxTotalMimics);
                m_currentMaxAlb = 0;
                m_currentMaxHib = 0;
                m_currentMaxMid = 0;

                for (int i = 0; i < m_currentMaxTotalMimics; i++)
                {
                    int randomRealm = Util.Random(2);

                    if (randomRealm == 0)
                        m_currentMaxAlb++;
                    else if (randomRealm == 1)
                        m_currentMaxHib++;
                    else
                        m_currentMaxMid++;
                }
            }

            private void SetGroupMembers(List<MimicNPC> list)
            {
                if (list.Count > 1)
                {
                    int groupChance = m_groupChance;
                    int groupLeaderIndex = -1;

                    for (int i = 0; i < list.Count; i++)
                    {
                        if (i + 1 < list.Count)
                        {
                            if (Util.Chance(groupChance) && !(list[i].Group?.GetMembersInTheGroup().Count > 7))
                            {
                                if (groupLeaderIndex == -1)
                                {
                                    list[i].Group = new Group(list[i]);
                                    list[i].Group.AddMember(list[i]);
                                    groupLeaderIndex = i;
                                }

                                list[groupLeaderIndex].Group.AddMember(list[i + 1]);
                                groupChance -= 5;
                            }
                            else
                            {
                                groupLeaderIndex = -1;
                                groupChance = m_groupChance;
                            }
                        }
                    }
                }
            }

            public void UpdateBattleStats(MimicNPC mimic)
            {
                m_battleStats.Add(new BattleStats(mimic.Name, mimic.RaceName, mimic.CharacterClass.Name, mimic.Kills, true));
            }

            public void BattlegroundStats(GamePlayer player)
            {
                List<MimicNPC> currentMimics = GetMasterList();
                List<BattleStats> currentStats = new List<BattleStats>();

                if (currentMimics.Count != 0)
                {
                    foreach (MimicNPC mimic in currentMimics)
                        currentStats.Add(new BattleStats(mimic.Name, mimic.RaceName, mimic.CharacterClass.Name, mimic.Kills, false));
                }

                List<BattleStats> masterStatList = new List<BattleStats>();
                masterStatList.AddRange(currentStats);
                masterStatList.AddRange(m_battleStats);

                List<BattleStats> sortedList = masterStatList.OrderByDescending(obj => obj.TotalKills).ToList();

                string message = "----------------------------------------\n\n";
                int index = Math.Min(25, sortedList.Count);

                if (sortedList.Count != 0)
                {
                    for (int i = 0; i < index; i++)
                    {
                        string stats = string.Format("{0}. {1} - {2} - {3} - Kills: {4}",
                            i + 1,
                            sortedList[i].Name,
                            sortedList[i].Race,
                            sortedList[i].ClassName,
                            sortedList[i].TotalKills);

                        if (sortedList[i].IsDead)
                            stats += " - DEAD";

                        stats += "\n\n";

                        message += stats;
                    }
                }

                switch (player.Realm)
                {
                    case eRealm.Albion: message += "Alb count: " + m_albSpawner.SpawnCount; break;
                    case eRealm.Hibernia: message += "Hib count: " + m_hibSpawner.SpawnCount; break;
                    case eRealm.Midgard: message += "Mid count: " + m_midSpawner.SpawnCount; break;
                }

                player.Out.SendMessage(message, PacketHandler.eChatType.CT_System, PacketHandler.eChatLoc.CL_PopupWindow);
            }

            public List<MimicNPC> GetMasterList()
            {
                List<MimicNPC> masterList = new List<MimicNPC>();

                foreach (MimicNPC mimic in m_albSpawner.Mimics)
                {
                    if (mimic != null && mimic.ObjectState == GameObject.eObjectState.Active && mimic.ObjectState != GameObject.eObjectState.Deleted)
                        masterList.Add(mimic);
                }

                foreach (MimicNPC mimic in m_hibSpawner.Mimics)
                {
                    if (mimic != null && mimic.ObjectState == GameObject.eObjectState.Active && mimic.ObjectState != GameObject.eObjectState.Deleted)
                        masterList.Add(mimic);
                }

                foreach (MimicNPC mimic in m_midSpawner.Mimics)
                {
                    if (mimic != null && mimic.ObjectState == GameObject.eObjectState.Active && mimic.ObjectState != GameObject.eObjectState.Deleted)
                        masterList.Add(mimic);
                }

                return masterList;
            }
        }

        private struct BattleStats
        {
            public string Name;
            public string Race;
            public string ClassName;
            public int TotalKills;
            public bool IsDead;

            public BattleStats(string name, string race, string className, int totalKills, bool dead)
            {
                Name = name;
                Race = race;
                ClassName = className;
                TotalKills = totalKills;
                IsDead = dead;
            }
        }
    }

    #endregion Battlegrounds

    #region Spawning

    public static class MimicSpawning
    {
        public static List<MimicSpawner> MimicSpawners
        {
            get
            {
                return _mimicSpawners ?? (_mimicSpawners = new List<MimicSpawner>());
            }
        }

        private static List<MimicSpawner> _mimicSpawners;


        public static List<MimicSpawnerPersistent> MimicSpawnersPersistent
        {
            get
            {
                return _mimicSpawnersPersistent ?? (_mimicSpawnersPersistent = new List<MimicSpawnerPersistent>());
            }
        }

        private static List<MimicSpawnerPersistent> _mimicSpawnersPersistent;
    }

    #endregion Spawning

    public static class MimicManager
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static List<MimicNPC> MimicNPCs = new List<MimicNPC>();

        public static bool Initialize()
        {
            log.Info("MimicManager Initializing...");

            MimicBattlegrounds.Initialize();

            return true;
        }

        public static bool AddMimicToWorld(MimicNPC mimic, Point3D position, ushort region)
        {
            if (mimic != null)
            {
                mimic.X = position.X;
                mimic.Y = position.Y;
                mimic.Z = position.Z;

                mimic.CurrentRegionID = region;

                if (mimic.AddToWorld())
                    return true;
            }

            return false;
        }

        public static MimicNPC GetMimic(eMimicClass charClass, byte level, string name = "", eGender gender = eGender.Neutral, bool preventCombat = false)
        {
            if (charClass == eMimicClass.None)
                return null;

            MimicNPC mimic = new MimicNPC(charClass, level);

            if (mimic != null)
            {
                if (name != "")
                    mimic.Name = name;

                if (gender != eGender.Neutral)
                {
                    mimic.Gender = gender;

                    foreach (PlayerRace race in PlayerRace.AllRaces)
                    {
                        if (race.ID == (eRace)mimic.Race)
                        {
                            mimic.Model = (ushort)race.GetModel(gender);
                            break;
                        }
                    }
                }

                if (preventCombat)
                {
                    MimicBrain mimicBrain = mimic.Brain as MimicBrain;

                    if (mimicBrain != null)
                        mimicBrain.PreventCombat = preventCombat;
                }

                mimic.movementComponent.MaxSpeedBase = GamePlayer.PLAYER_BASE_SPEED;

                return mimic;
            }

            return null;
        }

        public static eMimicClass GetRandomMimicClass(eRealm realm = eRealm.None)
        {
            Array mimicClasses = Enum.GetValues(typeof(eMimicClass));

            if (realm == eRealm.None)
            {
                int randomIndex = Util.Random(1, mimicClasses.Length - 1);
                return (eMimicClass)mimicClasses.GetValue(randomIndex);
            }

            List<eMimicClass> classes = new List<eMimicClass>();

            foreach (eMimicClass mimicClass in mimicClasses)
            {
                if (GlobalConstants.STARTING_CLASSES_DICT[realm].Contains((eCharacterClass)mimicClass))
                    classes.Add(mimicClass);
            }

            return classes[Util.Random(classes.Count - 1)];
        }

        public static eMimicClass GetRandomMeleeClass(eRealm realm = eRealm.None)
        {
            List<eMimicClass> meleeClasses = new List<eMimicClass>();

            foreach (eMimicClass mimicClass in Enum.GetValues(typeof(eMimicClass)))
            {
                switch (mimicClass)
                {
                    case eMimicClass.None:
                    case eMimicClass.Cabalist:
                    case eMimicClass.Sorcerer:
                    case eMimicClass.Theurgist:
                    case eMimicClass.Wizard:
                    case eMimicClass.Eldritch:
                    case eMimicClass.Enchanter:
                    case eMimicClass.Mentalist:
                    case eMimicClass.Bonedancer:
                    case eMimicClass.Runemaster:
                    case eMimicClass.Spiritmaster:
                    continue;

                    default:
                    if (realm != eRealm.None)
                            if (!GlobalConstants.STARTING_CLASSES_DICT[realm].Contains((eCharacterClass)mimicClass))
                                continue;

                    meleeClasses.Add(mimicClass);

                    break;
                }
            }

            return meleeClasses[Util.Random(meleeClasses.Count - 1)];
        }

        public static eMimicClass GetRandomCasterClass(eRealm realm = eRealm.None)
        {
            List<eMimicClass> casterClasses = new List<eMimicClass>();

            foreach (eMimicClass mimicClass in Enum.GetValues(typeof(eMimicClass)))
            {
                switch (mimicClass)
                {
                    case eMimicClass.Cabalist:
                    case eMimicClass.Sorcerer:
                    case eMimicClass.Theurgist:
                    case eMimicClass.Wizard:
                    case eMimicClass.Eldritch:
                    case eMimicClass.Enchanter:
                    case eMimicClass.Mentalist:
                    case eMimicClass.Bonedancer:
                    case eMimicClass.Runemaster:
                    case eMimicClass.Spiritmaster:

                    if (realm != eRealm.None)
                        if (!GlobalConstants.STARTING_CLASSES_DICT[realm].Contains((eCharacterClass)mimicClass))
                            continue;

                    casterClasses.Add(mimicClass);
                    break;

                    default:
                    continue;
                }
            }

            return casterClasses[Util.Random(casterClasses.Count - 1)];
        }

        public static eRealm GetRealmFromClass(eMimicClass mimicClass)
        {
            eRealm realm;

            if ((int)mimicClass >= 1 && (int)mimicClass <= 19)
                realm = eRealm.Albion;
            else if ((int)mimicClass >= 21 && (int)mimicClass <= 32)
                realm = eRealm.Midgard;
            else
                realm = eRealm.Hibernia;

            return realm;
        }
    }

    #region Equipment

    public static class MimicEquipment
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void SetWeaponROG(GameLiving living, eRealm realm, eCharacterClass charClass, byte level, eObjectType objectType, eInventorySlot slot, eDamageType damageType)
        {
            DbItemTemplate itemToCreate = new GeneratedUniqueItem(false, realm, charClass, level, objectType, slot, damageType);

            GameInventoryItem item = GameInventoryItem.Create(itemToCreate);
            living.Inventory.AddItem(slot, item);
        }

        public static void SetArmorROG(GameLiving living, eRealm realm, eCharacterClass charClass, byte level, eObjectType objectType)
        {
            for (int i = Slot.HELM; i <= Slot.ARMS; i++)
            {
                if (i == Slot.JEWELRY || i == Slot.CLOAK)
                    continue;

                eInventorySlot slot = (eInventorySlot)i;
                DbItemTemplate itemToCreate = new GeneratedUniqueItem(false, realm, charClass, level, objectType, slot);

                GameInventoryItem item = GameInventoryItem.Create(itemToCreate);

                living.Inventory.AddItem(slot, item);
            }
        }

        public static void SetJewelryROG(GameLiving living, eRealm realm, eCharacterClass charClass, byte level, eObjectType objectType)
        {
            for (int i = Slot.JEWELRY; i <= Slot.RIGHTRING; i++)
            {
                if (i is Slot.TORSO or Slot.LEGS or Slot.ARMS or Slot.FOREARMS or Slot.SHIELD)
                    continue;

                eInventorySlot slot = (eInventorySlot)i;
                DbItemTemplate itemToCreate = new GeneratedUniqueItem(false, realm, charClass, level, objectType, slot);

                GameInventoryItem item = GameInventoryItem.Create(itemToCreate);

                if (i == Slot.RIGHTRING || i == Slot.LEFTRING)
                    living.Inventory.AddItem(living.Inventory.FindFirstEmptySlot(eInventorySlot.LeftRing, eInventorySlot.RightRing), item);
                else if (i == Slot.LEFTWRIST || i == Slot.RIGHTWRIST)
                    living.Inventory.AddItem(living.Inventory.FindFirstEmptySlot(eInventorySlot.LeftBracer, eInventorySlot.RightBracer), item);
                else
                    living.Inventory.AddItem(slot, item);
            }
        }

        public static void SetInstrumentROG(GameLiving living, eRealm realm, eCharacterClass charClass, byte level, eObjectType objectType, eInventorySlot slot, eInstrumentType instrumentType)
        {
            DbItemTemplate itemToCreate = new GeneratedUniqueItem(false, realm, charClass, level, objectType, slot, instrumentType);

            GameInventoryItem item = GameInventoryItem.Create(itemToCreate);
            living.Inventory.AddItem(slot, item);
        }

        public static void SetMeleeWeapon(IGamePlayer player, eObjectType weapType, eHand hand, eWeaponDamageType damageType = 0)
        {
            int min = Math.Max(1, player.Level - 6);
            int max = Math.Min(51, player.Level + 4);

            IList<DbItemTemplate> itemList;

            itemList = GameServer.Database.SelectObjects<DbItemTemplate>(DB.Column("Level").IsGreaterOrEqualTo(min).And(
                                                                       DB.Column("Level").IsLessOrEqualTo(max).And(
                                                                       DB.Column("Object_Type").IsEqualTo((int)weapType).And(
                                                                       DB.Column("Realm").IsEqualTo((int)player.Realm)).And(
                                                                       DB.Column("IsPickable").IsEqualTo(1)))));
            if (itemList.Count != 0)
            {
                List<DbItemTemplate> itemsToKeep = new List<DbItemTemplate>();

                foreach (DbItemTemplate item in itemList)
                {
                    bool shouldAddItem = false;

                    switch (hand)
                    {
                        case eHand.oneHand:
                        shouldAddItem = item.Item_Type == Slot.RIGHTHAND || item.Item_Type == Slot.LEFTHAND;
                        break;

                        case eHand.leftHand:
                        shouldAddItem = item.Item_Type == Slot.LEFTHAND;
                        break;

                        case eHand.twoHand:
                        shouldAddItem = item.Item_Type == Slot.TWOHAND && (damageType == 0 || item.Type_Damage == (int)damageType);
                        break;

                        default:
                        break;
                    }

                    if (shouldAddItem)
                        itemsToKeep.Add(item);
                }

                if (itemsToKeep.Count != 0)
                {
                    DbItemTemplate itemTemplate = itemsToKeep[Util.Random(itemsToKeep.Count - 1)];
                    AddItem(player, itemTemplate, hand);
                }
            }
            else
                log.Info("No melee weapon found for " + player.Name);
        }

        public static void SetRangedWeapon(IGamePlayer player, eObjectType weapType)
        {
            int min = Math.Max(1, player.Level - 6);
            int max = Math.Min(51, player.Level + 3);

            IList<DbItemTemplate> itemList;
            itemList = GameServer.Database.SelectObjects<DbItemTemplate>(DB.Column("Level").IsGreaterOrEqualTo(min).And(
                                                                       DB.Column("Level").IsLessOrEqualTo(max).And(
                                                                       DB.Column("Object_Type").IsEqualTo((int)weapType).And(
                                                                       DB.Column("Item_Type").IsEqualTo(13).And(
                                                                       DB.Column("Realm").IsEqualTo((int)player.Realm)).And(
                                                                       DB.Column("IsPickable").IsEqualTo(1))))));

            if (itemList.Count != 0)
            {
                DbItemTemplate itemTemplate = itemList[Util.Random(itemList.Count - 1)];
                AddItem(player, itemTemplate);

                return;
            }
            else
                log.Info("No Ranged weapon found for " + player.Name);
        }

        public static void SetShield(IGamePlayer player, int shieldSize)
        {
            if (shieldSize < 1)
                return;

            int min = Math.Max(1, player.Level - 6);
            int max = Math.Min(51, player.Level + 3);

            IList<DbItemTemplate> itemList;

            itemList = GameServer.Database.SelectObjects<DbItemTemplate>(DB.Column("Level").IsGreaterOrEqualTo(min).And(
                                                                       DB.Column("Level").IsLessOrEqualTo(max).And(
                                                                       DB.Column("Object_Type").IsEqualTo((int)eObjectType.Shield).And(
                                                                       DB.Column("Realm").IsEqualTo((int)player.Realm)).And(
                                                                       DB.Column("Type_Damage").IsEqualTo(shieldSize).And(
                                                                       DB.Column("IsPickable").IsEqualTo(1))))));

            if (itemList.Count != 0)
            {
                DbItemTemplate itemTemplate = itemList[Util.Random(itemList.Count - 1)];
                AddItem(player, itemTemplate);

                return;
            }
            else
                log.Info("No Shield found for " + player.Name);
        }

        public static void SetArmor(IGamePlayer player, eObjectType armorType)
        {
            int min = Math.Max(1, player.Level - 6);
            int max = Math.Min(51, player.Level + 3);

            IList<DbItemTemplate> itemList;

            itemList = GameServer.Database.SelectObjects<DbItemTemplate>(DB.Column("Level").IsGreaterOrEqualTo(min).And(
                                                                       DB.Column("Level").IsLessOrEqualTo(max).And(
                                                                       DB.Column("Object_Type").IsEqualTo((int)armorType).And(
                                                                       DB.Column("Realm").IsEqualTo((int)player.Realm)).And(
                                                                       DB.Column("IsPickable").IsEqualTo(1)))));

            if (itemList.Count != 0)
            {
                Dictionary<int, List<DbItemTemplate>> armorSlots = new Dictionary<int, List<DbItemTemplate>>();

                foreach (DbItemTemplate template in itemList)
                {
                    if (!armorSlots.TryGetValue(template.Item_Type, out List<DbItemTemplate> slotList))
                    {
                        slotList = new List<DbItemTemplate>();
                        armorSlots[template.Item_Type] = slotList;
                    }

                    slotList.Add(template);
                }

                foreach (var pair in armorSlots)
                {
                    if (pair.Value.Count != 0)
                    {
                        DbItemTemplate itemTemplate = pair.Value[Util.Random(pair.Value.Count - 1)];
                        AddItem(player, itemTemplate);
                    }
                }
            }
            else
                log.Info("No armor found for " + player.Name);
        }

        public static void SetInstrument(IGamePlayer player, eObjectType weapType, eInventorySlot slot, eInstrumentType instrumentType)
        {
            int min = Math.Max(1, player.Level - 6);
            int max = Math.Min(51, player.Level + 3);

            IList<DbItemTemplate> itemList;
            itemList = GameServer.Database.SelectObjects<DbItemTemplate>(DB.Column("Level").IsGreaterOrEqualTo(min).And(
                                                                       DB.Column("Level").IsLessOrEqualTo(max).And(
                                                                       DB.Column("Object_Type").IsEqualTo((int)weapType).And(
                                                                       DB.Column("DPS_AF").IsEqualTo((int)instrumentType).And(
                                                                       DB.Column("Realm").IsEqualTo((int)player.Realm)).And(
                                                                       DB.Column("IsPickable").IsEqualTo(1))))));

            if (itemList.Count != 0)
            {
                DbItemTemplate itemTemplate = itemList[Util.Random(itemList.Count - 1)];
                DbInventoryItem item = GameInventoryItem.Create(itemTemplate);
                player.Inventory.AddItem(slot, item);

                return;
            }
            else
                log.Info("No instrument found for " + player.Name);
        }

        public static void SetJewelry(IGamePlayer player)
        {
            int min = Math.Max(1, player.Level - 30);
            int max = Math.Min(51, player.Level + 3);

            IList<DbItemTemplate> itemList;
            List<DbItemTemplate> cloakList = new List<DbItemTemplate>();
            List<DbItemTemplate> jewelryList = new List<DbItemTemplate>();
            List<DbItemTemplate> ringList = new List<DbItemTemplate>();
            List<DbItemTemplate> wristList = new List<DbItemTemplate>();
            List<DbItemTemplate> neckList = new List<DbItemTemplate>();
            List<DbItemTemplate> waistList = new List<DbItemTemplate>();

            itemList = GameServer.Database.SelectObjects<DbItemTemplate>(DB.Column("Level").IsGreaterOrEqualTo(min).And(
                                                                       DB.Column("Level").IsLessOrEqualTo(max).And(
                                                                       DB.Column("Object_Type").IsEqualTo((int)eObjectType.Magical).And(
                                                                       DB.Column("Realm").IsEqualTo((int)player.Realm)).And(
                                                                       DB.Column("IsPickable").IsEqualTo(1)))));
            if (itemList.Count != 0)
            {
                foreach (DbItemTemplate template in itemList)
                {
                    if (template.Item_Type == Slot.CLOAK)
                    {
                        template.Color = Util.Random((Enum.GetValues(typeof(eColor)).Length));
                        cloakList.Add(template);
                    }
                    else if (template.Item_Type == Slot.JEWELRY)
                        jewelryList.Add(template);
                    else if (template.Item_Type == Slot.LEFTRING || template.Item_Type == Slot.RIGHTRING)
                        ringList.Add(template);
                    else if (template.Item_Type == Slot.LEFTWRIST || template.Item_Type == Slot.RIGHTWRIST)
                        wristList.Add(template);
                    else if (template.Item_Type == Slot.NECK)
                        neckList.Add(template);
                    else if (template.Item_Type == Slot.WAIST)
                        waistList.Add(template);
                }

                List<List<DbItemTemplate>> masterList = new List<List<DbItemTemplate>>
                {
                cloakList,
                jewelryList,
                neckList,
                waistList
                };

                foreach (List<DbItemTemplate> list in masterList)
                {
                    if (list.Count != 0)
                    {
                        DbItemTemplate itemTemplate = list[Util.Random(list.Count - 1)];
                        AddItem(player, itemTemplate);
                    }
                }

                // Add two rings and bracelets
                for (int i = 0; i < 2; i++)
                {
                    if (ringList.Count != 0)
                    {
                        DbItemTemplate itemTemplate = ringList[Util.Random(ringList.Count - 1)];
                        AddItem(player, itemTemplate);
                    }

                    if (wristList.Count != 0)
                    {
                        DbItemTemplate itemTemplate = wristList[Util.Random(wristList.Count - 1)];
                        AddItem(player, itemTemplate);
                    }
                }

                // Not sure this is needed what were you thinking past self?
                if (player.Inventory.GetItem(eInventorySlot.Cloak) == null)
                {
                    DbItemTemplate cloak = GameServer.Database.FindObjectByKey<DbItemTemplate>("cloak");
                    cloak.Color = Util.Random((Enum.GetValues(typeof(eColor)).Length));
                    AddItem(player, cloak);
                }
            }
            else
                log.Info("No jewelry of any kind found for " + player.Name);
        }

        private static void AddItem(IGamePlayer player, DbItemTemplate itemTemplate, eHand hand = eHand.None)
        {
            if (itemTemplate == null)
                log.Info("itemTemplate in AddItem is null");

            DbInventoryItem item = GameInventoryItem.Create(itemTemplate);

            if (item != null)
            {
                if (item.Item_Type == Slot.LEFTRING || item.Item_Type == Slot.RIGHTRING)
                {
                    player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.LeftRing, eInventorySlot.RightRing), item);
                    return;
                }
                else if (item.Item_Type == Slot.LEFTWRIST || item.Item_Type == Slot.RIGHTWRIST)
                {
                    player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.LeftBracer, eInventorySlot.RightBracer), item);
                    return;
                }
                else if (item.Item_Type == Slot.LEFTHAND && item.Object_Type != (int)eObjectType.Shield && hand == eHand.oneHand)
                {
                    player.Inventory.AddItem(eInventorySlot.RightHandWeapon, item);
                    return;
                }
                else
                {
                    if (item.Object_Type == (int)eObjectType.Shield &&
                        (player.CharacterClass.ID == (int)eCharacterClass.Infiltrator ||
                        player.CharacterClass.ID == (int)eCharacterClass.Mercenary ||
                        player.CharacterClass.ID == (int)eCharacterClass.Nightshade ||
                        player.CharacterClass.ID == (int)eCharacterClass.Ranger ||
                        player.CharacterClass.ID == (int)eCharacterClass.Blademaster ||
                        player.CharacterClass.ID == (int)eCharacterClass.Shadowblade ||
                        player.CharacterClass.ID == (int)eCharacterClass.Berserker ||
                        (player.CharacterClass.ID == (int)eCharacterClass.Savage)))
                    {
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstEmptyBackpack, eInventorySlot.LastEmptyBackpack), item);
                    }
                    else
                        player.Inventory.AddItem((eInventorySlot)item.Item_Type, item);
                }
            }
            else
                log.Info("Item failed to be created for " + player.Name);
        }
    }

    #endregion Equipment

    #region Spec

    public class MimicSpec
    {
        public static string SpecName;
        public eObjectType WeaponOneType;
        public eObjectType WeaponTwoType;
        public eWeaponDamageType DamageType = 0;
        public eSpecType SpecType;

        public bool Is2H;

        public List<SpecLine> SpecLines = new List<SpecLine>();

        public MimicSpec()
        { }

        protected void Add(string spec, uint cap, float ratio)
        {
            SpecLines.Add(new SpecLine(spec, cap, ratio));
        }

        protected string ObjToSpec(eObjectType obj)
        {
            string spec = SkillBase.ObjectTypeToSpec(obj);

            return spec;
        }

        public static MimicSpec GetSpec(eMimicClass charClass)
        {
            switch (charClass)
            {
                case eMimicClass.Armsman: return new ArmsmanSpec();
                case eMimicClass.Cabalist: return new CabalistSpec();
                case eMimicClass.Cleric: return new ClericSpec();
                case eMimicClass.Friar: return new FriarSpec();
                case eMimicClass.Infiltrator: return new InfiltratorSpec();
                case eMimicClass.Mercenary: return new MercenarySpec();
                case eMimicClass.Minstrel: return new MinstrelSpec();
                case eMimicClass.Paladin: return new PaladinSpec();
                case eMimicClass.Reaver: return new ReaverSpec();
                case eMimicClass.Scout: return new ScoutSpec();
                case eMimicClass.Sorcerer: return new SorcererSpec();
                case eMimicClass.Theurgist: return new TheurgistSpec();
                case eMimicClass.Wizard: return new WizardSpec();

                case eMimicClass.Bard: return new BardSpec();
                case eMimicClass.Blademaster: return new BlademasterSpec();
                case eMimicClass.Champion: return new ChampionSpec();
                case eMimicClass.Druid: return new DruidSpec();
                case eMimicClass.Eldritch: return new EldritchSpec();
                case eMimicClass.Enchanter: return new EnchanterSpec();
                case eMimicClass.Hero: return new HeroSpec();
                case eMimicClass.Mentalist: return new MentalistSpec();
                case eMimicClass.Nightshade: return new NightshadeSpec();
                case eMimicClass.Ranger: return new RangerSpec();
                case eMimicClass.Valewalker: return new ValewalkerSpec();
                case eMimicClass.Warden: return new WardenSpec();

                case eMimicClass.Berserker: return new BerserkerSpec();
                case eMimicClass.Bonedancer: return new BonedancerSpec();
                case eMimicClass.Healer: return new HealerSpec();
                case eMimicClass.Hunter: return new HunterSpec();
                case eMimicClass.Runemaster: return new RunemasterSpec();
                case eMimicClass.Savage: return new SavageSpec();
                case eMimicClass.Shadowblade: return new ShadowbladeSpec();
                case eMimicClass.Shaman: return new ShamanSpec();
                case eMimicClass.Skald: return new SkaldSpec();
                case eMimicClass.Spiritmaster: return new SpiritmasterSpec();
                case eMimicClass.Thane: return new ThaneSpec();
                case eMimicClass.Warrior: return new WarriorSpec();
            }

            return null;
        }
    }

    public struct SpecLine
    {
        public string Spec;
        public uint SpecCap;
        public float levelRatio;

        public SpecLine(string spec, uint cap, float ratio)
        {
            Spec = spec;
            SpecCap = cap;
            levelRatio = ratio;
        }
    }

    #endregion Spec

    #region LFG

    public static class MimicLFGManager
    {
        private static List<MimicLFGEntry> _mimicEntries = new List<MimicLFGEntry>();

        private static long _respawnTime = 0;

        private static int _minRespawnTime = 60000;
        private static int _maxRespawnTime = 600000;

        private static int _minRemoveTime = 300000;
        private static int _maxRemoveTime = 3600000;

        private static int _maxMimics = 20;
        private static int _addMimicChance = 50;

        public static List<MimicLFGEntry> GetLFG(byte level)
        {
            if (_respawnTime == 0)
            {
                _respawnTime = GameLoop.GameLoopTime + Util.Random(_minRespawnTime, _maxRespawnTime);
                _mimicEntries = GenerateList(level);
            }

            lock (_mimicEntries)
            {
                _mimicEntries = ValidateList(_mimicEntries);

                if (GameLoop.GameLoopTime > _respawnTime)
                {
                    _mimicEntries = GenerateList(level);
                    _respawnTime = GameLoop.GameLoopTime + Util.Random(_minRespawnTime, _maxRespawnTime);
                }
            }

            return _mimicEntries;
        }

        public static void Remove(MimicLFGEntry entryToRemove)
        {
            if (_mimicEntries.Count != 0)
            {
                lock (_mimicEntries)
                {
                    foreach (MimicLFGEntry entry in _mimicEntries)
                    {
                        if (entry == entryToRemove)
                        {
                            entry.RemoveTime = GameLoop.GameLoopTime - 1;
                            break;
                        }
                    }
                }
            }
        }

        private static List<MimicLFGEntry> GenerateList(byte level)
        {
            if (_mimicEntries.Count < _maxMimics)
            {
                int mimicsToAdd = _maxMimics - _mimicEntries.Count;

                for (int i = 0; i < mimicsToAdd; i++)
                {
                    if (Util.Chance(_addMimicChance))
                    {
                        int levelMin = Math.Max(1, level - 3);
                        int levelMax = Math.Min(50, level + 3);
                        int levelRand = Util.Random(levelMin, levelMax);
                        long removeTime = GameLoop.GameLoopTime + Util.Random(_minRemoveTime, _maxRemoveTime);
                        eMimicClass mimicClass = MimicManager.GetRandomMimicClass();
                        eRealm realm = MimicManager.GetRealmFromClass(mimicClass);
 
                        MimicLFGEntry entry = new MimicLFGEntry(mimicClass, realm, (byte)levelRand, removeTime);

                        _mimicEntries.Add(entry);
                    }
                }
            }

            List<MimicLFGEntry> generateList = new List<MimicLFGEntry>();
            generateList.AddRange(_mimicEntries);

            return generateList;
        }

        private static List<MimicLFGEntry> ValidateList(List<MimicLFGEntry> entries)
        {
            List<MimicLFGEntry> validList = new List<MimicLFGEntry>();

            if (entries.Count != 0)
            {
                foreach (MimicLFGEntry entry in entries)
                {
                    if (GameLoop.GameLoopTime < entry.RemoveTime)
                        validList.Add(entry);
                }
            }

            return validList;
        }

        public class MimicLFGEntry
        {
            public string Name;
            public eGender Gender;
            public eMimicClass MimicClass;
            public byte Level;
            public long RemoveTime;
            public bool RefusedGroup;

            public MimicLFGEntry(eMimicClass mimicClass, eRealm realm, byte level, long removeTime)
            {
                Gender = Util.RandomBool() ? eGender.Male : eGender.Female;
                Name = MimicNames.GetName(Gender, realm);
                MimicClass = mimicClass;
                Level = level;
                RemoveTime = removeTime;
            }
        }
    }

    #endregion LFG

    public class SetupMimicsEvent
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [ScriptLoadedEvent]
        public static void OnScriptsCompiled(DOLEvent e, object sender, EventArgs args)
        {
            if (MimicManager.Initialize())
                log.Info("MimicNPCs Initialized.");
            else
                log.Error("MimicNPCs Failed to Initialize.");
        }
    }

    // Just a quick way to get names...
    public static class MimicNames
    {
        private const string albMaleNames = "Gareth,Lancelot,Cedric,Tristan,Percival,Gawain,Arthur,Merlin,Galahad,Ector,Uther,Mordred,Bors,Lionel,Agravain,Bedivere,Kay,Lamorak,Erec,Gaheris,Pellinore,Loholt,Leodegrance,Aglovale,Tor,Ywain,Uri,Cador,Elayne,Tristram,Cei,Gavain,Kei,Launcelot,Meleri,Isolde,Dindrane,Ragnelle,Lunete,Morgause,Yseult,Bellicent,Brangaine,Blanchefleur,Enid,Vivian,Laudine,Selivant,Lisanor,Ganelon,Cundrie,Guinevere,Norgal,Vivienne,Clarissant,Ettard,Morgaine,Serene,Serien,Selwod,Siraldus,Corbenic,Gurnemanz,Terreban,Malory,Dodinel,Serien,Gurnemanz,Manessen,Herzeleide,Taulat,Serien,Bohort,Ysabele,Karados,Dodinel,Peronell,Dinadan,Segwarides,Lucan,Lamorat,Enide,Parzival,Aelfric,Geraint,Rivalin,Blanchefleur,Gurnemanz,Terreban,Launceor,Clarissant,Herzeleide,Taulat,Zerbino,Serien,Bohort,Ysabele,Dodinel,Peronell,Serenadine,Dinadan,Caradoc,Segwarides,Lucan,Lamorat,Enide,Parzival,Aelfric,Geraint,Rivalin,Blanchefleur,Kaherdin,Gurnemanz,Terreban,Launceor,Clarissant,Patrise,Navarre,Taulat,Iseut,Guivret,Madouc,Ygraine,Tristran,Perceval,Lanzarote,Lamorat,Ysolt,Evaine,Guenever,Elisena,Rowena,Deirdre,Maelis,Clarissant,Palamedes,Yseult,Iseult,Palomides,Brangaine,Laudine,Herlews,Tristram,Alundyne,Blasine,Dinas";
        private const string albFemaleNames = "Guinevere,Isolde,Morgana,Elaine,Vivienne,Nimue,Lynette,Rhiannon,Enid,Iseult,Bellicent,Brangaine,Blanchefleur,Laudine,Selivant,Lisanor,Elidor,Brisen,Linet,Serene,Serien,Selwod,Ysabele,Karados,Peronell,Serenadine,Dinadan,Clarissant,Igraine,Aelfric,Herzeleide,Taulat,Zerbino,Iseut,Guivret,Madouc,Ygraine,Elisena,Rowena,Deirdre,Maelis,Herlews,Alundyne,Blasine,Dinas,Evalach,Rohais,Soredamors,Orguelleuse,Egletine,Fenice,Amide,Lionesse,Eliduc,Silvayne,Amadas,Amadis,Iaonice,Emerause,Ysabeau,Idonia,Alardin,Lessele,Evelake,Herzeleide,Carahes,Elyabel,Igrayne,Laudine,Guenloie,Isolt,Urgan,Yglais,Nimiane,Arabele,Amabel,Clarissant,Patrise,Navarre,Iseut,Guivret,Madouc,Ygraine,Elisena,Rowena,Deirdre,Maelis,Herlews,Alundyne,Blasine,Dinas,Evalach,Rohais,Soredamors,Orguelleuse,Egletine,Fenice,Amide,Lionesse,Eliduc,Silvayne,Amadas,Amadis,Iaonice,Emerause,Ysabeau,Idonia,Alardin,Lessele,Evelake,Herzeleide,Carahes,Elyabel,Igrayne,Laudine,Guenloie,Isolt,Urgan,Yglais,Nimiane,Arabele,Amabel";

        private const string hibMaleNames = "Aonghus,Breandan,Cian,Dallan,Eogan,Fearghal,Greagoir,Iomhar,Lorcan,Mairtin,Neachtan,Odhran,Paraic,Ruairi,Seosamh,Toireasa,aed,Beircheart,Colm,Domhnall,eanna,Fergus,Goll,Irial,Liam,MacCon,Naoimhin,odhran,Padraig,Ronan,Seanan,Tadhgan,Uilliam,Ailill,Bran,Cairbre,Daithi,Eoghan,Faolan,Gorm,Iollan,Lughaidh,Manannan,Niall,Oisin,Padraig,Ronan,Seadna,Tadhg,Ultan,Alastar,Bairre,Caoilte,Daire,enna,Fiachra,Gairm,Imleach,Jarlath,Kian,Laoiseach,Malachy,Naoise,Odhran,Paidin,Roibeard,Seamus,Turlough,Uilleag,Alastriona,Bairrfhionn,Caoimhe,Dymphna,eabha,Fionnuala,Grainne,Isolt,Laoise,Maire,Niamh,Oonagh,Padraigin,Roisin,Saoirse,Teagan,Una,Aoife,Brid,Caitriona,Deirdre,eibhlin,Fia,Gormlaith,Iseult,Jennifer,Kerstin,Lean,Maighread,Noirin,orlaith,Plurabelle,Rioghnach,Siobhan,Treasa,Ursula,Aodh,Baird,Caoimhin,Daire,eamon,Fearghas,Gartlach,iomhar,Jozsef,Lochlainn,Manus,Naois,oisin,Paidin,Roibeard,Seaan,Tomas,Uilliam,Ailbhe,Bairrionn,Caoilinn,Dairine,Eabhnat,Gormfhlaith,Ite,Juliana,Kaitlin,Laochlann,Nollaig,ornait,Pala,Roise,Seaghdha,Tomaltach,Uinseann,Ailbin,Bairrionn,Caoimhin,Dairine,Eabhnat,Fearchara,Gormfhlaith,Ite,Juliana,Kaitlin,Laochlann,Nollaig,ornait,Pala,Roise,Seaghdha,Tomaltach,Uinseann";
        private const string hibFemaleNames = "Aibhlinn,Brighid,Caoilfhionn,Deirdre,eabha,Fionnuala,Grainne,Iseult,Jennifer,Kerstin,Lean,Maire,Niamh,Oonagh,Padraigin,Roisin,Saoirse,Teagan,Una,Aoife,Aisling,Blathnat,Cliodhna,Dymphna,eidin,Fineachan,Gormfhlaith,iomhar,Juliana,Kaitlin,Laoise,Maighread,Noirin,orlaith,Plurabelle,Rioghnach,Siobhan,Treasa,Ursula,Ailbhe,Bairrfhionn,Caoilinn,Dairine,eabhnat,Fearchara,Gormlaith,Ite,Laochlann,Mairtin,Nollaig,ornait,Pala,Roise,Seaghdha,Tomaltach,Uinseann,Ailbin,Ailis,Blath,Dairin,eadaoin,Fionn,Gra,Iseabal,Jacinta,Kait,Laoiseach,Nuala,orfhlaith,Poilin,Saibh,Teadgh";

        private const string midMaleNames = "Agnar,Bjorn,Dagur,Eirik,Fjolnir,Geir,Haldor,Ivar,Jarl,Kjartan,Leif,Magnus,Njall,Orvar,Ragnald,Sigbjorn,Thrain,Ulf,Vifil,Arni,Bardi,Dain,Einar,Faldan,Grettir,Hogni,Ingvar,Jokul,Koll,Leiknir,Mord,Nikul,Ornolf,Ragnvald,Sigmund,Thorfinn,Ulfar,Vali,Yngvar,Asgeir,Bolli,Darri,Egill,Flosi,Gisli,Hjortur,Ingolf,Jokull,Kolbeinn,Leikur,Mordur,Nils,Orri,Ragnaldur,Sigurdur,Thormundur,Ulfur,Valur,Yngvi,Arnstein,Bardur,David,Egill,Flosi,Gisli,Hjortur,Ingolf,Jokull,Kolbeinn,Leikur,Mordur,Nils,Orri,Ragnaldur,Sigurdur,Thormundur,Ulfur,Valur,Yngvi,Arnstein,Bardur,David,Eik,Fridgeir,Grimur,Hafthor,Ivar,Jorundur,Kari,Ljotur,Mord,Nokkvi,Oddur,Rafn,Steinar,Thorir,Valgard,Yngve,Askur,Baldur,Dagr,Eirikur,Fridleif";
        private const string midFemaleNames = "Aesa,Bjorg,Dalla,Edda,Fjola,Gerd,Halla,Inga,Jora,Kari,Lina,Marna,Njola,Orna,Ragna,Sif,Thora,Ulfhild,Vika,Alva,Bodil,Dagny,Eira,Frida,Gisla,Hildur,Ingibjorg,Jofrid,Kolfinna,Leidr,Mina,Olina,Ragnheid,Sigrid,Thordis,Una,Yrsa,Asgerd,Bergthora,Eilif,Flosa,Gudrid,Hjordis,Ingimund,Jolninna,Lidgerd,Mjoll,Oddny,Ranveig,Sigrun,Thorhalla,Valdis,Alfhild,Bardis,Davida,Eilika,Fridleif,Gudrun,Hjortur,Jokulina,Kolfinna,Leiknir,Mordur,Njall,Orvar,Ragnald,Sigbjorn,Thrain,Ulf,Vifil,Arnstein,Bardur,David,Egill,Fridgeir,Grimur,Hafthor,Ivar,Jorundur,Kari,Ljotur,Mord,Nokkvi,Oddur,Rafn,Steinar,Thorir,Valgard,Yngve,Askur,Baldur,Dagr,Eirikur,Fridleif,Grimur,Halfdan,Ivarr,Kjell,Ljung,Nikul,Ornolf,Ragnvald,Sigurdur,Thormundur,Ulfur,Valur,Yngvi";

        public static string GetName(eGender gender, eRealm realm)
        {
            string[] names = new string[0];

            switch (realm)
            {
                case eRealm.Albion:
                if (gender == eGender.Male)
                    names = albMaleNames.Split(',');
                else
                    names = albFemaleNames.Split(',');
                break;

                case eRealm.Hibernia:
                if (gender == eGender.Male)
                    names = hibMaleNames.Split(',');
                else
                    names = hibFemaleNames.Split(",");
                break;

                case eRealm.Midgard:
                if (gender == eGender.Male)
                    names = midMaleNames.Split(',');
                else
                    names = midFemaleNames.Split(",");
                break;
            }

            int randomIndex = Util.Random(names.Length - 1);

            return names[randomIndex];
        }
    }
}