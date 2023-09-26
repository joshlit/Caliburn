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

namespace DOL.GS.Scripts
{
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

    public static class MimicManager
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static List<MimicNPC> MimicNPCs = new List<MimicNPC>();

        public static Faction alb = new Faction();
        public static Faction hib = new Faction();
        public static Faction mid = new Faction();

        #region Spec

        // Albion
        static Type[] cabalistSpecs = { typeof(MatterCabalist), typeof(BodyCabalist), typeof(SpiritCabalist) };

        // Hibernia
        static Type[] heroSpecs = { typeof(ShieldHero), typeof(HybridHero) };
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
                case MimicHero: return Activator.CreateInstance(heroSpecs[Util.Random(heroSpecs.Length - 1)]) as MimicSpec;
                case MimicEldritch: return Activator.CreateInstance(eldritchSpecs[Util.Random(eldritchSpecs.Length - 1)]) as MimicSpec;
                case MimicEnchanter: return Activator.CreateInstance(enchanterSpecs[Util.Random(enchanterSpecs.Length - 1)]) as MimicSpec;
                case MimicMentalist: return Activator.CreateInstance(mentalistSpecs[Util.Random(mentalistSpecs.Length - 1)]) as MimicSpec;

                // Midgard
                case MimicHealer: return Activator.CreateInstance(healerSpecs[Util.Random(healerSpecs.Length - 1)]) as MimicSpec;

                default: return null;
            }
        }

        #endregion

        public static MimicNPC AddMimicToWorld(eMimicClasses mimicClass, GamePlayer player, byte level, Point3D position, bool preventCombat = true)
        {
            if (mimicClass == eMimicClasses.None)
                return null;

            MimicNPC mimic = null;

            switch (mimicClass)
            {
                case eMimicClasses.Armsman: mimic = new MimicArmsman(player, level, position); break;
                case eMimicClasses.Mercenary: mimic = new MimicMercenary(player, level, position); break;
                case eMimicClasses.Reaver: mimic = new MimicReaver(player, level, position); break;
                case eMimicClasses.Paladin: mimic = new MimicPaladin(player, level, position); break;
                case eMimicClasses.Friar: mimic = new MimicFriar(player, level, position); break;
                case eMimicClasses.Cleric: mimic = new MimicCleric(player, level, position); break;
                case eMimicClasses.Minstrel: mimic = new MimicMinstrel(player, level, position); break;
                case eMimicClasses.Infiltrator: mimic = new MimicInfiltrator(player, level, position); break;
                //case eMimicClasses.Scout: mimic = new MimicScout(player, level, position); break;
                case eMimicClasses.Wizard: mimic = new MimicWizard(player, level, position); break;
                case eMimicClasses.Theurgist: mimic = new MimicTheurgist(player, level, position); break;
                case eMimicClasses.Sorcerer: mimic = new MimicSorcerer(player, level, position); break;
                case eMimicClasses.Cabalist: mimic = new MimicCabalist(player, level, position); break;

                case eMimicClasses.Bard: mimic = new MimicBard(player, level, position); break;
                case eMimicClasses.Blademaster: mimic = new MimicBlademaster(player, level, position); break;
                case eMimicClasses.Champion: mimic = new MimicChampion(player, level, position); break;
                case eMimicClasses.Druid: mimic = new MimicDruid(player, level, position); break;
                case eMimicClasses.Eldritch: mimic = new MimicEldritch(player, level, position); break;
                case eMimicClasses.Enchanter: mimic = new MimicEnchanter(player, level, position); break;
                case eMimicClasses.Hero: mimic = new MimicHero(player, level, position); break;
                case eMimicClasses.Mentalist: mimic = new MimicMentalist(player, level, position); break;
                case eMimicClasses.Nightshade: mimic = new MimicNightshade(player, level, position); break;
                //case eMimicClasses.Ranger: mimic = new MimicRanger(player, level, position); break;
                case eMimicClasses.Valewalker: mimic = new MimicValewalker(player, level, position); break;
                case eMimicClasses.Warden: mimic = new MimicWarden(player, level, position); break;

                case eMimicClasses.Berserker: mimic = new MimicBerserker(player, level, position); break;
                case eMimicClasses.Bonedancer: mimic = new MimicBonedancer(player, level, position); break;
                case eMimicClasses.Healer: mimic = new MimicHealer(player, level, position); break;
                //case eMimicClasses.Hunter: mimic = new MimicHunter(player, level, position); break;
                case eMimicClasses.Runemaster: mimic = new MimicRunemaster(player, level, position); break;
                case eMimicClasses.Savage: mimic = new MimicSavage(player, level, position); break;
                case eMimicClasses.Shadowblade: mimic = new MimicShadowblade(player, level, position); break;
                case eMimicClasses.Shaman: mimic = new MimicShaman(player, level, position); break;
                case eMimicClasses.Skald: mimic = new MimicSkald(player, level, position); break;
                case eMimicClasses.Spiritmaster: mimic = new MimicSpiritmaster(player, level, position); break;
                case eMimicClasses.Thane: mimic = new MimicThane(player, level, position); break;
                case eMimicClasses.Warrior: mimic = new MimicWarrior(player, level, position); break;
            }

            if (mimic != null)
            {
                if (preventCombat)
                {
                    MimicBrain mimicBrain = mimic.Brain as MimicBrain;

                    if (mimicBrain != null)
                        mimicBrain.PreventCombat = true;
                }

                mimic.AddToWorld();   

                MimicNPCs.Add(mimic);
            }

            return mimic;
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

    public static class MimicEquipment
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static bool SetMeleeWeapon(GameLiving living, string weapType, bool dualWield = false, eWeaponDamageType damageType = 0, eHand hand = eHand.None)
        {
            eObjectType objectType = GetObjectType(weapType);

            int min = Math.Max(0, living.Level - 6);
            int max = Math.Min(50, living.Level + 4);

            IList<DbItemTemplate> itemList;

            itemList = GameServer.Database.SelectObjects<DbItemTemplate>(DB.Column("Level").IsGreaterOrEqualTo(min).And(
                                                                       DB.Column("Level").IsLessOrEqualTo(max).And(
                                                                       DB.Column("Object_Type").IsEqualTo((int)objectType).And(
                                                                       DB.Column("Realm").IsEqualTo((int)living.Realm)).And(
                                                                       DB.Column("IsPickable").IsEqualTo(1)))));

            if (dualWield)
            {
                List<DbItemTemplate> leftHandItems = new List<DbItemTemplate>();

                foreach (DbItemTemplate item in itemList)
                {
                    if (item.Item_Type == Slot.LEFTHAND)
                        leftHandItems.Add(item);
                }

                if (leftHandItems.Count > 0)
                    AddItem(living, leftHandItems[Util.Random(leftHandItems.Count - 1)]);
            }

            if (hand != eHand.None)
            {
                List<DbItemTemplate> itemsToKeep = new List<DbItemTemplate>();

                foreach (DbItemTemplate item in itemList)
                {
                    if (item.Hand == (int)hand)
                        itemsToKeep.Add(item);
                }

                if (itemsToKeep.Count > 0)
                {
                    AddItem(living, itemsToKeep[Util.Random(itemsToKeep.Count - 1)]);

                    return true;
                }
                else
                    return false;
            }

            if (objectType != eObjectType.TwoHandedWeapon && objectType != eObjectType.PolearmWeapon && objectType != eObjectType.Staff)
            {
                foreach (DbItemTemplate template in itemList)
                {
                    if (template.Item_Type == Slot.LEFTHAND)
                        template.Item_Type = Slot.RIGHTHAND;
                }
            }

            if ((int)damageType != 0)
            {
                List<DbItemTemplate> itemsToKeep = new List<DbItemTemplate>();

                foreach (DbItemTemplate item in itemList)
                {
                    if (item.Type_Damage == (int)damageType)
                    {
                        itemsToKeep.Add(item);
                    }
                }

                if (itemsToKeep.Count > 0)
                {
                    DbItemTemplate template = itemsToKeep[Util.Random(itemsToKeep.Count - 1)];
                    return AddItem(living, template);
                }

                return false;
            }
            else if (itemList.Count > 0)
            {
                DbItemTemplate template = itemList[Util.Random(itemList.Count - 1)];

                return AddItem(living, template);
            }
            else
            {
                log.Debug("Could not find any fucking items for this peice of shit.");
                return false;
            }
        }

        public static bool SetRangedWeapon(GameLiving living, eObjectType weapType)
        {
            int min = Math.Max(1, living.Level - 6);
            int max = Math.Min(51, living.Level + 3);

            IList<DbItemTemplate> itemList;
            itemList = GameServer.Database.SelectObjects<DbItemTemplate>(DB.Column("Level").IsGreaterOrEqualTo(min).And(
                                                                       DB.Column("Level").IsLessOrEqualTo(max).And(
                                                                       DB.Column("Object_Type").IsEqualTo((int)weapType).And(
                                                                       DB.Column("Realm").IsEqualTo((int)living.Realm)).And(
                                                                       DB.Column("IsPickable").IsEqualTo(1)))));

            if (itemList.Count > 0)
            {
                AddItem(living, itemList[Util.Random(itemList.Count - 1)]);

                return true;
            }
            else
            {
                log.Debug("No Ranged weapon.");
                return false;
            }
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

            DbItemTemplate item = itemList[Util.Random(itemList.Count - 1)];

            AddItem(living, item);
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

            AddItem(living, armsList[Util.Random(armsList.Count - 1)]);
            AddItem(living, handsList[Util.Random(handsList.Count - 1)]);
            AddItem(living, legsList[Util.Random(legsList.Count - 1)]);
            AddItem(living, feetList[Util.Random(feetList.Count - 1)]);
            AddItem(living, torsoList[Util.Random(torsoList.Count - 1)]);
            AddItem(living, helmList[Util.Random(helmList.Count - 1)]);
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
                if (list.Count > 0)
                {
                    AddItem(living, list[Util.Random(list.Count - 1)]);
                }
            }

            for (int i = 0; i < 2; i++)
            {
                if (ringList.Count > 0)
                {
                    AddItem(living, ringList[Util.Random(ringList.Count - 1)]);
                }

                if (wristList.Count > 0)
                {
                    AddItem(living, wristList[Util.Random(wristList.Count - 1)]);
                }
            }

            if (living.Inventory.GetItem(eInventorySlot.Cloak) == null)
            {
                DbItemTemplate cloak = GameServer.Database.FindObjectByKey<DbItemTemplate>("cloak");
                int color = Util.Random(500);
                log.Debug("Color: " + color);
                cloak.Color = color;
                AddItem(living, cloak);
            }
        }

        private static bool AddItem(GameLiving living, DbItemTemplate itemTemplate)
        {
            if (itemTemplate == null)
            {
                log.Debug("itemTemplate in AddItem is null");
                return false;
            }

            DbInventoryItem item = GameInventoryItem.Create(itemTemplate);

            if (item != null)
            {
                if (itemTemplate.Item_Type == Slot.LEFTRING || itemTemplate.Item_Type == Slot.RIGHTRING)
                {
                    return living.Inventory.AddItem(living.Inventory.FindFirstEmptySlot(eInventorySlot.LeftRing, eInventorySlot.RightRing), item);
                }
                else if (itemTemplate.Item_Type == Slot.LEFTWRIST || itemTemplate.Item_Type == Slot.RIGHTWRIST)
                {
                    return living.Inventory.AddItem(living.Inventory.FindFirstEmptySlot(eInventorySlot.LeftBracer, eInventorySlot.RightBracer), item);
                }
                else
                    return living.Inventory.AddItem((eInventorySlot)item.Item_Type, item);
            }
            else
            {
                log.Debug("Item failed to be created.");
                return false;
            }
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
            }

            return objectType;
        }
    }

    //public static list<>

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

            //if (log.IsInfoEnabled)
            //    if (log.IsInfoEnabled)
            //        log.Info("MimicNPCS initialized: " + good);
        }

        [ScriptUnloadedEvent]
        public static void OnScriptUnload(DOLEvent e, object sender, EventArgs args)
        {
            //if (gameMimicNPC != null)
            //    gameMimicNPC.Delete();
        }
    }
}
