using DOL.GS.Scripts;
using DOL.GS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Net;
using DOL.Events;
using log4net;
using System.Reflection;
using DOL.AI.Brain;
using System.Security.Policy;
using DOL.Database;
using log4net.Core;
using System.Collections;
using System.Drawing.Text;
using System.Reflection.Emit;
using DOL.GS.API;

namespace DOL.GS.Scripts
{
    #region Battlegrounds

    public static class MimicBattlegrounds
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        //public static BattleGroundTimer battleTimer;

        public static List<MimicNPC> albMimics = new List<MimicNPC>();
        public static List<MimicNPC> hibMimics = new List<MimicNPC>();
        public static List<MimicNPC> midMimics = new List<MimicNPC>();

        public static Point3D albThidSpawnPoint = new Point3D(37200, 51200, 3950);
        public static Point3D hibThidSpawnPoint = new Point3D(19820, 19305, 4050);
        public static Point3D midThidSpawnPoint = new Point3D(53300, 26100, 4270);
     
        public static bool Running;

        private static readonly List<BattleStats> battleStats = new List<BattleStats>();

        private static ushort thidRegion = 252;
        private static int maxMimics = 256;
        private static int totalMimics = 0;

        public static void Start(GamePlayer player)
        {
            if (player.CurrentRegion.ID == thidRegion)
            {
                Running = true;

                for (int i = 0; i < maxMimics; i++)
                {
                    byte level = (byte)Util.Random(20, 24);
                    eRealm randomRealm = (eRealm)Util.Random(1, 3);

                    if (randomRealm == eRealm.Albion)
                    {
                        MimicNPC mimic = MimicManager.GetMimic(eMimicClasses.Random, level, eRealm.Albion);
                        MimicManager.AddMimicToWorld(mimic, albThidSpawnPoint, thidRegion);

                        albMimics.Add(mimic);
                    }
                    else if (randomRealm == eRealm.Hibernia)
                    {
                        MimicNPC mimic = MimicManager.GetMimic(eMimicClasses.Random, level, eRealm.Hibernia);
                        MimicManager.AddMimicToWorld(mimic, hibThidSpawnPoint, thidRegion);

                        hibMimics.Add(mimic);
                    }
                    else if (randomRealm == eRealm.Midgard)
                    {
                        MimicNPC mimic = MimicManager.GetMimic(eMimicClasses.Random, level, eRealm.Midgard);
                        MimicManager.AddMimicToWorld(mimic, midThidSpawnPoint, thidRegion);

                        midMimics.Add(mimic);
                    }  
                }

                totalMimics = albMimics.Count + hibMimics.Count + midMimics.Count;
                log.Info("Alb: " + albMimics.Count);
                log.Info("Hib: " + hibMimics.Count);
                log.Info("Mid: " + midMimics.Count);
                log.Info("Total Mimics: " + totalMimics);
            }
        }

        public static void UpdateBattleStats(MimicNPC mimic)
        {
            battleStats.Add(new BattleStats(mimic.RaceName, mimic.CharacterClass.Name, mimic.Kills, mimic.KillStreak, true));
        }

        public static void MimicBattlegroundStats(GamePlayer player)
        {
            List<MimicNPC> currentMimics = GetMasterList();
            List<BattleStats> currentStats = new List<BattleStats>();

            if (currentMimics.Count > 0)
            {
                foreach (MimicNPC mimic in currentMimics)
                    currentStats.Add(new BattleStats(mimic.RaceName, mimic.CharacterClass.Name, mimic.Kills, mimic.KillStreak, false));
            }

            List<BattleStats> masterStatList = new List<BattleStats>();
            masterStatList.AddRange(currentStats);

            lock (battleStats)
            {
                masterStatList.AddRange(battleStats);
            }

            List<BattleStats> sortedList = masterStatList.OrderByDescending(obj => obj.TotalKills)
                                                         .ThenByDescending(obj => obj.KillStreak)
                                                         .ToList();

            string message = "----------------------------------------\n\n";
            int index = Math.Min(25, sortedList.Count);

            if (sortedList.Any())
            {
                for (int i = 0; i < index; i++)
                {
                    string stats = string.Format("{0}. {1} - {2} - Kills: {3} - KillStreak: {4}",
                        i + 1,
                        sortedList[i].Race,
                        sortedList[i].ClassName,
                        sortedList[i].TotalKills,
                        sortedList[i].KillStreak);

                    if (sortedList[i].IsDead)
                        stats += " - DEAD";

                    stats += "\n\n";

                    message += stats;
                }
            }

            player.Out.SendMessage(message, PacketHandler.eChatType.CT_System, PacketHandler.eChatLoc.CL_PopupWindow);
        }

        public static List<MimicNPC> GetMasterList()
        {
            List<MimicNPC> masterList = new List<MimicNPC>();

            lock (albMimics)
            {
                foreach (MimicNPC mimic in albMimics)
                {
                    if (mimic != null && mimic.ObjectState == GameObject.eObjectState.Active && mimic.ObjectState != GameObject.eObjectState.Deleted)
                        masterList.Add(mimic);
                }
            }

            lock (hibMimics)
            {
                foreach (MimicNPC mimic in hibMimics)
                {
                    if (mimic != null && mimic.ObjectState == GameObject.eObjectState.Active && mimic.ObjectState != GameObject.eObjectState.Deleted)
                        masterList.Add(mimic);
                }
            }

            lock (midMimics)
            {
                foreach (MimicNPC mimic in midMimics)
                {
                    if (mimic != null && mimic.ObjectState == GameObject.eObjectState.Active && mimic.ObjectState != GameObject.eObjectState.Deleted)
                        masterList.Add(mimic);
                }
            }

            return masterList;
        }

        private struct BattleStats
        {
            public string Race;
            public string ClassName;
            public int TotalKills;
            public int KillStreak;
            public bool IsDead;

            public BattleStats(string race, string className, int totalKills, int killStreak, bool dead)
            {
                Race = race;
                ClassName = className;
                TotalKills = totalKills;
                KillStreak = killStreak;
                IsDead = dead;
            }
        }
    }

    #endregion
    public static class MimicManager
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static List<MimicNPC> MimicNPCs = new List<MimicNPC>();

        public static Faction alb = new Faction();
        public static Faction hib = new Faction();
        public static Faction mid = new Faction();

        #region Spec

        // TODO: Will likley need to be able to tell caster specs apart for AI purposes since they operate so differently. Will bring them into here, or use some sort of enum.

        // Albion
        static Type[] cabalistSpecs = { typeof(MatterCabalist), typeof(BodyCabalist), typeof(SpiritCabalist) };

        // Hibernia
        static Type[] eldritchSpecs = { typeof(SunEldritch), typeof(ManaEldritch), typeof(VoidEldritch) };
        static Type[] enchanterSpecs = { typeof(ManaEnchanter), typeof(LightEnchanter) };
        static Type[] mentalistSpecs = { typeof(LightMentalist), typeof(ManaMentalist), typeof(MentalismMentalist) };

        // Midgard

        static Type[] healerSpecs = { typeof(PacHealer), typeof(AugHealer) };

        public static MimicSpec Random(MimicNPC mimicNPC)
        {
            switch (mimicNPC)
            {
                // Albion
                case MimicArmsman: return Activator.CreateInstance(typeof(ArmsmanSpec)) as MimicSpec;
                case MimicCabalist: return Activator.CreateInstance(cabalistSpecs[Util.Random(cabalistSpecs.Length - 1)]) as MimicSpec;
                case MimicMercenary: return Activator.CreateInstance(typeof(MercenarySpec)) as MimicSpec;

                // Hibernia
                case MimicEldritch: return Activator.CreateInstance(eldritchSpecs[Util.Random(eldritchSpecs.Length - 1)]) as MimicSpec;
                case MimicEnchanter: return Activator.CreateInstance(enchanterSpecs[Util.Random(enchanterSpecs.Length - 1)]) as MimicSpec;
                case MimicMentalist: return Activator.CreateInstance(mentalistSpecs[Util.Random(mentalistSpecs.Length - 1)]) as MimicSpec;

                // Midgard
                case MimicHealer: return Activator.CreateInstance(healerSpecs[Util.Random(healerSpecs.Length - 1)]) as MimicSpec;

                default: return null;
            }
        }

        #endregion

        public static bool AddMimicToWorld(MimicNPC mimic, Point3D position, ushort region)
        {
            if (mimic != null)
            {
                mimic.X = position.X;
                mimic.Y = position.Y;
                mimic.Z = position.Z;

                mimic.CurrentRegionID = region;

                if (mimic.AddToWorld())
                {
                    MimicNPCs.Add(mimic);
                    return true;
                }
            }

            return false;
        }

        public static MimicNPC GetMimic(eMimicClasses mimicClass, byte level, eRealm realm = 0, bool preventCombat = false)
        {
            if (mimicClass == eMimicClasses.None)
                return null;

            MimicNPC mimic = null;

            if (mimicClass == eMimicClasses.Random && realm != eRealm.None)
            {
                int randomIndex = 0;

                if (realm == eRealm.Albion)
                    randomIndex = Util.Random(12);
                else if (realm == eRealm.Hibernia)
                    randomIndex = Util.Random(13, 24);
                else if (realm == eRealm.Midgard)
                    randomIndex = Util.Random(25, 36);

                mimicClass = (eMimicClasses)randomIndex;
            }
            else if (mimicClass == eMimicClasses.Random && realm == eRealm.None)
            {
                int randomIndex = Util.Random(36);
                mimicClass = (eMimicClasses)randomIndex;
            }

            switch (mimicClass)
            {
                case eMimicClasses.Armsman: mimic = new MimicArmsman(level); break;
                case eMimicClasses.Cabalist: mimic = new MimicCabalist(level); break;
                case eMimicClasses.Cleric: mimic = new MimicCleric(level); break;
                case eMimicClasses.Friar: mimic = new MimicFriar(level); break;
                case eMimicClasses.Infiltrator: mimic = new MimicInfiltrator(level); break;
                case eMimicClasses.Mercenary: mimic = new MimicMercenary(level); break;
                case eMimicClasses.Minstrel: mimic = new MimicMinstrel(level); break;
                case eMimicClasses.Paladin: mimic = new MimicPaladin(level); break;
                case eMimicClasses.Reaver: mimic = new MimicReaver(level); break;
                case eMimicClasses.Scout: mimic = new MimicScout(level); break;
                case eMimicClasses.Sorcerer: mimic = new MimicSorcerer(level); break;
                case eMimicClasses.Theurgist: mimic = new MimicTheurgist(level); break;
                case eMimicClasses.Wizard: mimic = new MimicWizard(level); break;

                case eMimicClasses.Bard: mimic = new MimicBard(level); break;
                case eMimicClasses.Blademaster: mimic = new MimicBlademaster(level); break;
                case eMimicClasses.Champion: mimic = new MimicChampion(level); break;
                case eMimicClasses.Druid: mimic = new MimicDruid(level); break;
                case eMimicClasses.Eldritch: mimic = new MimicEldritch(level); break;
                case eMimicClasses.Enchanter: mimic = new MimicEnchanter(level); break;
                case eMimicClasses.Hero: mimic = new MimicHero(level); break;
                case eMimicClasses.Mentalist: mimic = new MimicMentalist(level); break;
                case eMimicClasses.Nightshade: mimic = new MimicNightshade(level); break;
                case eMimicClasses.Ranger: mimic = new MimicRanger(level); break;
                case eMimicClasses.Valewalker: mimic = new MimicValewalker(level); break;
                case eMimicClasses.Warden: mimic = new MimicWarden(level); break;

                case eMimicClasses.Berserker: mimic = new MimicBerserker(level); break;
                case eMimicClasses.Bonedancer: mimic = new MimicBonedancer(level); break;
                case eMimicClasses.Healer: mimic = new MimicHealer(level); break;
                case eMimicClasses.Hunter: mimic = new MimicHunter(level); break;
                case eMimicClasses.Runemaster: mimic = new MimicRunemaster(level); break;
                case eMimicClasses.Savage: mimic = new MimicSavage(level); break;
                case eMimicClasses.Shadowblade: mimic = new MimicShadowblade(level); break;
                case eMimicClasses.Shaman: mimic = new MimicShaman(level); break;
                case eMimicClasses.Skald: mimic = new MimicSkald(level); break;
                case eMimicClasses.Spiritmaster: mimic = new MimicSpiritmaster(level); break;
                case eMimicClasses.Thane: mimic = new MimicThane(level); break;
                case eMimicClasses.Warrior: mimic = new MimicWarrior(level); break;
            }

            if (mimic != null)
            {
                if (preventCombat)
                {
                    MimicBrain mimicBrain = mimic.Brain as MimicBrain;

                    if (mimicBrain != null)
                        mimicBrain.PreventCombat = preventCombat;
                }

                return mimic;
            }

            return null;
        }

        public static void SetPreventCombat(bool preventCombat)
        {
            MimicNPCs = ValidateList();

            if (MimicNPCs.Count > 0)
            {
                foreach (MimicNPC mimic in MimicNPCs)
                {
                    MimicBrain mimicBrain = mimic.Brain as MimicBrain;

                    if (mimicBrain != null)
                        mimicBrain.PreventCombat = preventCombat;
                }
            }
        }

        private static List<MimicNPC> ValidateList()
        {
            lock (MimicNPCs)
            {
                if (MimicNPCs.Any())
                {
                    foreach (MimicNPC mimic in MimicNPCs)
                    {
                        if (mimic == null || mimic.ObjectState != GameObject.eObjectState.Active || mimic.ObjectState == GameObject.eObjectState.Deleted)
                            MimicNPCs.Remove(mimic);

                        return new List<MimicNPC>(MimicNPCs);
                    }
                }
            }          

            return null;
        }

        public static bool Initialize()
        {
            // Factions
            alb.AddEnemyFaction(hib);
            alb.AddEnemyFaction(mid);

            hib.AddEnemyFaction(alb);
            hib.AddEnemyFaction(mid);

            mid.AddEnemyFaction(alb);
            mid.AddEnemyFaction(hib);

            return true;
        }
    }

    #region Equipment

    public static class MimicEquipment
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void SetMeleeWeapon(GameLiving living, string weapType, eHand hand, eWeaponDamageType damageType = 0)
        {
            eObjectType objectType = GetObjectType(weapType);

            int min = Math.Max(0, living.Level - 6);
            int max = Math.Min(51, living.Level + 4);

            IList<DbItemTemplate> itemList;

            itemList = GameServer.Database.SelectObjects<DbItemTemplate>(DB.Column("Level").IsGreaterOrEqualTo(min).And(
                                                                       DB.Column("Level").IsLessOrEqualTo(max).And(
                                                                       DB.Column("Object_Type").IsEqualTo((int)objectType).And(
                                                                       DB.Column("Realm").IsEqualTo((int)living.Realm)).And(
                                                                       DB.Column("IsPickable").IsEqualTo(1)))));
            if (itemList.Any())
            {
                // This allows for left handed weapons to be equipped to the right hand. Prevents Twohanded midgard weapons from being selected.
                if (hand == eHand.oneHand)
                {
                    List<DbItemTemplate> oneHandItemsToKeep = new List<DbItemTemplate>();

                    foreach (DbItemTemplate item in itemList)
                    {
                        if (item.Hand != (int)eHand.twoHand)
                            oneHandItemsToKeep.Add(item);
                    }

                    if (oneHandItemsToKeep.Any())
                    {
                        DbItemTemplate oneHandItemTemplate = oneHandItemsToKeep[Util.Random(oneHandItemsToKeep.Count - 1)];
                        AddItem(living, oneHandItemTemplate, hand);

                        return;
                    }
                }

                List<DbItemTemplate> itemsToKeep = new List<DbItemTemplate>();

                foreach (DbItemTemplate item in itemList)
                {
                    // Only used for Armsman and Paladin to ensure twohand weapon matches one handed spec.
                    if (damageType != 0)
                    {
                        if (item.Hand == (int)hand && item.Type_Damage == (int)damageType)
                            itemsToKeep.Add(item);
                    }
                    else if (item.Hand == (int)hand)
                        itemsToKeep.Add(item);

                    if (itemsToKeep.Any())
                    {
                        DbItemTemplate itemTemplate = itemsToKeep[Util.Random(itemsToKeep.Count - 1)];
                        AddItem(living, itemTemplate, hand);

                        return;
                    }
                }
            }
            else
                log.Info("No melee weapon found for " + living.Name);
        }

        public static void SetRangedWeapon(GameLiving living, eObjectType weapType)
        {
            int min = Math.Max(1, living.Level - 6);
            int max = Math.Min(51, living.Level + 3);

            IList<DbItemTemplate> itemList;
            itemList = GameServer.Database.SelectObjects<DbItemTemplate>(DB.Column("Level").IsGreaterOrEqualTo(min).And(
                                                                       DB.Column("Level").IsLessOrEqualTo(max).And(
                                                                       DB.Column("Object_Type").IsEqualTo((int)weapType).And(
                                                                       DB.Column("Realm").IsEqualTo((int)living.Realm)).And(
                                                                       DB.Column("IsPickable").IsEqualTo(1)))));

            if (itemList.Any())
            {
                DbItemTemplate itemTemplate = itemList[Util.Random(itemList.Count - 1)];
                AddItem(living, itemTemplate);
                
                return;
            }
            else
                log.Info("No Ranged weapon found for " + living.Name);
        }

        public static void SetShield(GameLiving living, int shieldSize)
        {
            int min = Math.Max(1, living.Level - 6);
            int max = Math.Min(51, living.Level + 3);

            IList<DbItemTemplate> itemList;

            itemList = GameServer.Database.SelectObjects<DbItemTemplate>(DB.Column("Level").IsGreaterOrEqualTo(min).And(
                                                                       DB.Column("Level").IsLessOrEqualTo(max).And(
                                                                       DB.Column("Object_Type").IsEqualTo((int)eObjectType.Shield).And(
                                                                       DB.Column("Realm").IsEqualTo((int)living.Realm)).And(
                                                                       DB.Column("Type_Damage").IsEqualTo(shieldSize).And(
                                                                       DB.Column("IsPickable").IsEqualTo(1))))));

            if (itemList.Any())
            {
                DbItemTemplate itemTemplate = itemList[Util.Random(itemList.Count - 1)];
                AddItem(living, itemTemplate);

                return;
            }
            else
                log.Info("No Shield found for " + living.Name);
        }

        public static void SetArmor(GameLiving living, eObjectType armorType)
        {
            int min = Math.Max(1, living.Level - 6);
            int max = Math.Min(51, living.Level + 3);

            IList<DbItemTemplate> itemList;

            List<DbItemTemplate> armsList = new List<DbItemTemplate>();
            List<DbItemTemplate> handsList = new List<DbItemTemplate>();
            List<DbItemTemplate> legsList = new List<DbItemTemplate>();
            List<DbItemTemplate> feetList = new List<DbItemTemplate>();
            List<DbItemTemplate> torsoList = new List<DbItemTemplate>();
            List<DbItemTemplate> helmList = new List<DbItemTemplate>();

            itemList = GameServer.Database.SelectObjects<DbItemTemplate>(DB.Column("Level").IsGreaterOrEqualTo(min).And(
                                                                       DB.Column("Level").IsLessOrEqualTo(max).And(
                                                                       DB.Column("Object_Type").IsEqualTo((int)armorType).And(
                                                                       DB.Column("Realm").IsEqualTo((int)living.Realm)).And(
                                                                       DB.Column("IsPickable").IsEqualTo(1)))));

            if (itemList.Any())
            {
                foreach (DbItemTemplate template in itemList)
                {
                    if (template.Item_Type == Slot.ARMS)
                        armsList.Add(template);
                    else if (template.Item_Type == Slot.HANDS)
                        handsList.Add(template);
                    else if (template.Item_Type == Slot.LEGS)
                        legsList.Add(template);
                    else if (template.Item_Type == Slot.FEET)
                        feetList.Add(template);
                    else if (template.Item_Type == Slot.TORSO)
                        torsoList.Add(template);
                    else if (template.Item_Type == Slot.HELM)
                        helmList.Add(template);
                }

                List<List<DbItemTemplate>> masterList = new List<List<DbItemTemplate>>
                {
                    armsList,
                    handsList,
                    legsList,
                    feetList,
                    torsoList,
                    helmList
                };

                foreach(List<DbItemTemplate> list in masterList)
                {
                    if (list.Any())
                    {
                        DbItemTemplate itemTemplate = list[Util.Random(list.Count - 1)];
                        AddItem(living, itemTemplate);
                    }
                }
            }
            else
                log.Info("No armor found for " + living.Name);
        }

        public static void SetJewelry(GameLiving living)
        {
            int min = Math.Max(1, living.Level - 30);
            int max = Math.Min(51, living.Level + 1);

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
                                                                       DB.Column("Realm").IsEqualTo((int)living.Realm)).And(
                                                                       DB.Column("IsPickable").IsEqualTo(1)))));
            if (itemList.Any())
            {
                foreach (DbItemTemplate template in itemList)
                {
                    if (template.Item_Type == Slot.CLOAK)
                        cloakList.Add(template);
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
                    if (list.Any())
                    {
                        DbItemTemplate itemTemplate = list[Util.Random(list.Count - 1)];
                        AddItem(living, itemTemplate);
                    }
                }

                // Add two rings and bracelets
                for (int i = 0; i < 2; i++)
                {
                    if (ringList.Any())
                    {
                        DbItemTemplate itemTemplate = ringList[Util.Random(ringList.Count - 1)];
                        AddItem(living, itemTemplate);
                    }

                    if (wristList.Any())
                    {
                        DbItemTemplate itemTemplate = wristList[Util.Random(wristList.Count - 1)];
                        AddItem(living, itemTemplate);
                    }
                }

                // Not sure this is needed what were you thinking past self?
                if (living.Inventory.GetItem(eInventorySlot.Cloak) == null)
                {
                    DbItemTemplate cloak = GameServer.Database.FindObjectByKey<DbItemTemplate>("cloak");
                    AddItem(living, cloak);
                }
            }
            else
                log.Info("No jewelry of any kind found for " + living.Name);
        }

        private static void AddItem(GameLiving living, DbItemTemplate itemTemplate, eHand hand = eHand.None)
        {
            if (itemTemplate == null)
                log.Info("itemTemplate in AddItem is null");

            DbInventoryItem item = GameInventoryItem.Create(itemTemplate);

            if (item != null)
            {
                log.Info("Item is called " + item.Name + " it is slot " + item.Item_Type);

                if (itemTemplate.Item_Type == Slot.LEFTRING || itemTemplate.Item_Type == Slot.RIGHTRING)
                {
                    living.Inventory.AddItem(living.Inventory.FindFirstEmptySlot(eInventorySlot.LeftRing, eInventorySlot.RightRing), item);
                    return;
                }
                else if (itemTemplate.Item_Type == Slot.LEFTWRIST || itemTemplate.Item_Type == Slot.RIGHTWRIST)
                {
                    living.Inventory.AddItem(living.Inventory.FindFirstEmptySlot(eInventorySlot.LeftBracer, eInventorySlot.RightBracer), item);
                    return;
                }
                else if (itemTemplate.Item_Type == Slot.LEFTHAND && itemTemplate.Object_Type != (int)eObjectType.Shield && hand == eHand.oneHand)
                {
                    living.Inventory.AddItem(eInventorySlot.RightHandWeapon, item);
                    return;
                }
                else
                    living.Inventory.AddItem((eInventorySlot)itemTemplate.Item_Type, item);
            }
            else
                log.Info("Item failed to be created for " + living.Name);
        }

        public static eObjectType GetObjectType(string obj)
        {
            eObjectType objectType = 0;

            switch (obj)
            {
                case "Staff": objectType = eObjectType.Staff; break;

                case "Slash": objectType = eObjectType.SlashingWeapon; break;
                case "Thrust": objectType = eObjectType.ThrustWeapon; break;
                case "Crush": objectType = eObjectType.CrushingWeapon; break;
                case "Flexible": objectType = eObjectType.Flexible; break;
                case "Polearm": objectType = eObjectType.PolearmWeapon; break;
                case "Two Handed": objectType = eObjectType.TwoHandedWeapon; break;

                case "Blades": objectType = eObjectType.Blades; break;
                case "Piercing": objectType = eObjectType.Piercing; break;
                case "Blunt": objectType = eObjectType.Blunt; break;
                case "Large Weapons": objectType = eObjectType.LargeWeapons; break;
                case "Celtic Spear": objectType = eObjectType.CelticSpear; break;
                case "Scythe": objectType = eObjectType.Scythe; break;

                case "Sword": objectType = eObjectType.Sword; break;
                case "Axe": objectType = eObjectType.Axe; break;
                case "Hammer": objectType = eObjectType.Hammer; break;
                case "Hand to Hand": objectType = eObjectType.HandToHand; break;

                case "Cloth": objectType = eObjectType.Cloth; break;
                case "Leather": objectType = eObjectType.Leather; break;
                case "Studded": objectType = eObjectType.Studded; break;
                case "Chain": objectType = eObjectType.Chain; break;
                case "Plate": objectType = eObjectType.Plate; break;

                case "Reinforced": objectType = eObjectType.Reinforced; break;
                case "Scale": objectType = eObjectType.Scale; break;
            }

            return objectType;
        }
    }

    #endregion

    #region Spec

    public class MimicSpec
    {
        public static string SpecName;
        public string WeaponTypeOne;
        public string WeaponTypeTwo;
        public eWeaponDamageType DamageType = 0;

        public bool is2H;

        public List<SpecLine> SpecLines = new List<SpecLine>();

        public MimicSpec()
        { }

        protected void Add(string name, uint cap, float ratio)
        {
            SpecLines.Add(new SpecLine(name, cap, ratio));
        }
    }

    public struct SpecLine
    {
        public string SpecName;
        public uint SpecCap;
        public float levelRatio;

        public SpecLine(string name, uint cap, float ratio)
        {
            SpecName = name;
            SpecCap = cap;
            levelRatio = ratio;
        }

        public void SetName(string name)
        {
            SpecName = name;
        }
    }

    #endregion

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

        //[ScriptUnloadedEvent]
        //public static void OnScriptUnload(DOLEvent e, object sender, EventArgs args)
        //{
        //if (gameMimicNPC != null)
        //    gameMimicNPC.Delete();
        //}
    }

    // Just a quick way to get names...
    public static class MimicNames
    {
        const string albMaleNames = "Gareth,Lancelot,Cedric,Tristan,Percival,Gawain,Arthur,Merlin,Galahad,Ector,Uther,Mordred,Bors,Lionel,Agravain,Bedivere,Kay,Lamorak,Erec,Gaheris,Caradoc,Pellinore,Loholt,Leodegrance,Aglovale,Tor,Ywain,Uri,Cador,Elayne,Tristram,Cei,Gavain,Kei,Launcelot,Meleri,Isolde,Dindrane,Ragnelle,Lunete,Morgause,Yseult,Bellicent,Brangaine,Blanchefleur,Enid,Vivian,Laudine,Selivant,Lisanor,Ganelon,Cundrie,Guinevere,Norgal,Vivienne,Clarissant,Ettard,Morgaine,Serene,Serien,Selwod,Siraldus,Corbenic,Gurnemanz,Terreban,Malory,Ettard,Dodinel,Serien,Gurnemanz,Manessen,Herzeleide,Taulat,Zerbino,Serien,Bohort,Ysabele,Karados,Dodinel,Peronell,Serenadine,Corbenic,Dinadan,Caradoc,Segwarides,Lucan,Lamorat,Enide,Parzival,Aelfric,Geraint,Lynette,Rivalin,Blanchefleur,Kaherdin,Gurnemanz,Terreban,Launceor,Clarissant,Herzeleide,Taulat,Zerbino,Serien,Bohort,Ysabele,Karados,Dodinel,Peronell,Serenadine,Corbenic,Dinadan,Caradoc,Segwarides,Lucan,Lamorat,Enide,Parzival,Aelfric,Geraint,Lynette,Rivalin,Blanchefleur,Kaherdin,Gurnemanz,Terreban,Launceor,Clarissant,Patrise,Navarre,Taulat,Iseut,Guivret,Madouc,Ygraine,Tristran,Perceval,Lanzarote,Lamorat,Ysolt,Evaine,Guenever,Elisena,Rowena,Deirdre,Maelis,Clarissant,Kaherdin,Ector,Palamedes,Yseult,Iseult,Tristan,Palomides,Brangaine,Elaine,Nimue,Laudine,Herlews,Tristram,Alundyne,Blasine,Dinas";
        const string albFemaleNames = "Guinevere,Isolde,Morgana,Elaine,Vivienne,Nimue,Lynette,Rhiannon,Enid,Iseult,Bellicent,Brangaine,Blanchefleur,Laudine,Selivant,Lisanor,Elidor,Brisen,Linet,Serene,Serien,Selwod,Ysabele,Karados,Peronell,Serenadine,Dinadan,Clarissant,Igraine,Aelfric,Lynette,Herzeleide,Taulat,Zerbino,Iseut,Guivret,Madouc,Ygraine,Elisena,Rowena,Deirdre,Maelis,Elaine,Nimue,Herlews,Alundyne,Blasine,Dinas,Evalach,Rohais,Soredamors,Orguelleuse,Egletine,Fenice,Amide,Lionesse,Eliduc,Silvayne,Amadas,Amadis,Iaonice,Emerause,Ysabeau,Idonia,Alardin,Lessele,Evelake,Herzeleide,Carahes,Elyabel,Igrayne,Laudine,Guenloie,Isolt,Urgan,Yglais,Nimiane,Arabele,Amabel,Clarissant,Patrise,Navarre,Iseut,Guivret,Madouc,Ygraine,Elisena,Rowena,Deirdre,Maelis,Elaine,Nimue,Herlews,Alundyne,Blasine,Dinas,Evalach,Rohais,Soredamors,Orguelleuse,Egletine,Fenice,Amide,Lionesse,Eliduc,Silvayne,Amadas,Amadis,Iaonice,Emerause,Ysabeau,Idonia,Alardin,Lessele,Evelake,Herzeleide,Carahes,Elyabel,Igrayne,Laudine,Guenloie,Isolt,Urgan,Yglais,Nimiane,Arabele,Amabel";

        const string hibMaleNames = "Ailill,Bran,Cairbre,Daithi,Eoghan,Faolan,Gorm,Iollan,Lughaidh,Manannan,Niall,Oisin,Pádraig,Rónán,Séadna,Tadhg,Ultán,Alastar,Bairre,Caoilte,Dáire,Énna,Fiachra,Gairm,Imleach,Jarlath,Kian,Laoiseach,Malachy,Naoise,Odhrán,Páidín,Roibéard,Seamus,Turlough,Uilleag,Alastriona,Bairrfhionn,Caoimhe,Dymphna,Éabha,Fionnuala,Gráinne,Isolt,Laoise,Máire,Niamh,Oonagh,Pádraigín,Róisín,Saoirse,Teagan,Úna,Aoife,Bríd,Caitríona,Deirdre,Éibhlin,Fia,Gormlaith,Iseult,Jennifer,Kerstin,Léan,Máighréad,Nóirín,Órlaith,Plurabelle,Ríoghnach,Siobhán,Treasa,Úrsula,Aodh,Baird,Caoimhín,Dáire,Éamon,Fearghas,Gartlach,Íomhar,József,Lochlainn,Mánus,Naois,Óisin,Páidín,Roibeárd,Seaán,Tomás,Uilliam,Ailbhe,Bairrionn,Caoilinn,Dairine,Eabhnat,Fearchara,Gormfhlaith,Ite,Juliana,Kaitlín,Laochlann,Máirtín,Nollaig,Órnait,Pála,Roise,Seaghdha,Tomaltach,Uinseann,Ailbín,Bairrionn,Caoimhín,Dairine,Eabhnat,Fearchara,Gormfhlaith,Ite,Juliana,Kaitlín,Laochlann,Máirtín,Nollaig,Órnait,Pála,Roise,Seaghdha,Tomaltach,Uinseann";
        const string hibFemaleNames = "Aibhlinn,Brighid,Caoilfhionn,Deirdre,Éabha,Fionnuala,Gráinne,Iseult,Jennifer,Kerstin,Léan,Máire,Niamh,Oonagh,Pádraigín,Róisín,Saoirse,Teagan,Úna,Aoife,Aisling,Bláthnat,Clíodhna,Dymphna,Éidín,Fíneachán,Gormfhlaith,Íomhar,Juliana,Kaitlín,Laoise,Máighréad,Nóirín,Órlaith,Plurabelle,Ríoghnach,Siobhán,Treasa,Úrsula,Ailbhe,Bairrfhionn,Caoilinn,Dairine,Éabhnat,Fearchara,Gormlaith,Ite,Laochlann,Máirtín,Nollaig,Órnait,Pála,Roise,Seaghdha,Tomaltach,Uinseann,Ailbín,Ailis,Bláth,Dairín,Éadaoin,Fionn,Grá,Iseabal,Jacinta,Káit,Laoiseach,Máire,Nuala,Órfhlaith,Póilín,Saibh,Téadgh";

        const string midMaleNames = "Agnar,Bjorn,Dagur,Eirik,Fjolnir,Geir,Haldor,Ivar,Jarl,Kjartan,Leif,Magnus,Njall,Orvar,Ragnald,Sigbjorn,Thrain,Ulf,Vifil,Arni,Bardi,Dain,Einar,Faldan,Grettir,Hogni,Ingvar,Jokul,Koll,Leiknir,Mord,Nikul,Ornolf,Ragnvald,Sigmund,Thorfinn,Ulfar,Vali,Yngvar,Asgeir,Bolli,Darri,Egill,Flosi,Gisli,Hjortur,Ingolf,Jokull,Kolbeinn,Leikur,Mordur,Nils,Orri,Ragnaldur,Sigurdur,Thormundur,Ulfur,Valur,Yngvi,Arnstein,Bardur,David,Egill,Flosi,Gisli,Hjortur,Ingolf,Jokull,Kolbeinn,Leikur,Mordur,Nils,Orri,Ragnaldur,Sigurdur,Thormundur,Ulfur,Valur,Yngvi,Arnstein,Bardur,David,Eik,Fridgeir,Grimur,Hafthor,Ivar,Jorundur,Kari,Ljotur,Mord,Nokkvi,Oddur,Rafn,Steinar,Thorir,Valgard,Yngve,Askur,Baldur,Dagr,Eirikur,Fridleif";
        const string midFemaleNames = "Aesa,Bjorg,Dalla,Edda,Fjola,Gerd,Halla,Inga,Jora,Kari,Lina,Marna,Njola,Orna,Ragna,Sif,Thora,Ulfhild,Vika,Alva,Bodil,Dagny,Eira,Frida,Gisla,Hildur,Ingibjorg,Jofrid,Kolfinna,Leidr,Mina,Olina,Ragnheid,Sigrid,Thordis,Una,Yrsa,Asgerd,Bergthora,Eilif,Flosa,Gudrid,Hjordis,Ingimund,Jolninna,Lidgerd,Mjoll,Oddny,Ranveig,Sigrun,Thorhalla,Valdis,Alfhild,Bardis,Davida,Eilika,Fridleif,Gudrun,Hjortur,Jokulina,Kolfinna,Leiknir,Mordur,Njall,Orvar,Ragnald,Sigbjorn,Thrain,Ulf,Vifil,Arnstein,Bardur,David,Egill,Fridgeir,Grimur,Hafthor,Ivar,Jorundur,Kari,Ljotur,Mord,Nokkvi,Oddur,Rafn,Steinar,Thorir,Valgard,Yngve,Askur,Baldur,Dagr,Eirikur,Fridleif,Grimur,Halfdan,Ivarr,Kjell,Ljung,Nikul,Ornolf,Ragnvald,Sigurdur,Thormundur,Ulfur,Valur,Yngvi";

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
                        names= hibFemaleNames.Split(",");
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
