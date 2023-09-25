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
                //if (preventCombat)
                //{
                //    MimicBrain mimicBrain = mimic.Brain as MimicBrain;

                //    if (mimicBrain != null)
                //        mimicBrain.PreventCombat = true;
                //}

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
