/*
-------------------------------------------------------
Author  : Fulmine         
-------------------------------------------------------
*/
using DOL.Events;
using DOL.GS;
using DOL.GS.Quests;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Myrddin.Labyrinth.NPC;

namespace Myrddin.Labyrinth
{
    public class LabyrinthObeliskHelper
    {
        #region Declarations

        private static List<ObeliskStruct> obeliskList = new List<ObeliskStruct>();

        #region struct ObeliskStruct

        private struct ObeliskStruct
        {
            /// <summary>
            /// internalName for intern use
            /// </summary>
            public string internalName;
            /// <summary>
            /// Language English
            /// </summary>
            public string NameEN;
            /// <summary>
            /// Language Deutch
            /// </summary>
            public string NameDE;
            /// <summary>
            /// Language French
            /// </summary>
            public string NameFR;
            /// <summary>
            /// Validation quest credit
            /// </summary>
            public Type ObeliskCredit;
            /// <summary>
            /// GameLoc: X
            /// </summary>
            public int X;
            /// <summary>
            /// GameLoc: Y
            /// </summary>
            public int Y;
            /// <summary>
            /// GameLoc: Z
            /// </summary>
            public int Z;
            /// <summary>
            /// GameLoc: Heading
            /// </summary>
            public ushort H;
            /// <summary>
            /// Gameloc: RegionID
            /// </summary>
            public ushort RegionID;
        }


        #endregion struct ObeliskStruct

        #endregion Declarations

        #region OnScriptLoaded // OnScriptUnloaded

        [ScriptLoadedEvent]
        public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            PopulateobeliskList();

            foreach (ObeliskStruct obelisk in obeliskList)
            {
                GameObelisk newGameObelisk = new GameObelisk();
                newGameObelisk.ObeliskInternalName = obelisk.internalName;
                newGameObelisk.X = obelisk.X;
                newGameObelisk.Y = obelisk.Y;
                newGameObelisk.Z = obelisk.Z;
                newGameObelisk.Heading = obelisk.H;
                newGameObelisk.CurrentRegionID = obelisk.RegionID;

                if (obelisk.internalName == "Nurizanes_Crossroads")
                    newGameObelisk.isNurizaneCrossroadsObelisk = true;
                else
                    newGameObelisk.isNurizaneCrossroadsObelisk = false;

                newGameObelisk.ObeliskCredit = obelisk.ObeliskCredit;

                newGameObelisk.AddToWorld();
            }
        }

        [ScriptUnloadedEvent]
        public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
        {

        }

        #endregion OnScriptLoaded // OnScriptUnloaded

        #region Public methods

        public static AbstractQuest GetAbstractQuestForPlayer(String obeliskName, GamePlayer player)
        {
            ObeliskStruct? obelisk = obeliskList.Where(o => o.internalName == obeliskName).FirstOrDefault();
            if (obelisk.HasValue)
            {
                AbstractQuest newQuest = (AbstractQuest)Activator.CreateInstance(obelisk.Value.ObeliskCredit, new object[] { player });
                return newQuest;
            }
            return null;
        }

        public static Boolean PlayerHasObelisk(String obeliskName, GamePlayer player)
        {
            ObeliskStruct? obelisk = obeliskList.Where(o => o.internalName == obeliskName).FirstOrDefault();
            if (obelisk.HasValue)
                return (player.HasFinishedQuest(obelisk.Value.ObeliskCredit) > 0);
            return false;
        }

        public static String GetObeliskRealName(String obeliskName, GamePlayer player)
        {
            ObeliskStruct? obelisk = obeliskList.Where(o => o.internalName == obeliskName).FirstOrDefault();
            if (obelisk.HasValue)
            {
                switch (player.Client.Account.Language)
                {
                    case "EN":
                        return obelisk.Value.NameEN;
                    case "FR":
                        return obelisk.Value.NameFR;
                    case "DE":
                        return obelisk.Value.NameDE;
                    default:
                        return obelisk.Value.NameEN;
                }
            }
            return String.Empty;
        }

        public static String GetObeliskInternalName(String obeliskName)
        {
            ObeliskStruct? obeliskEN = obeliskList.Where(o => o.NameEN == obeliskName).FirstOrDefault();

            if (obeliskEN.HasValue && obeliskEN.Value.NameEN == obeliskName)
                return obeliskEN.Value.internalName;

            return String.Empty;
        }

        public static GameLocation GetObeliskGameLocation(String obeliskName)
        {
            ObeliskStruct? obelisk = obeliskList.Where(o => o.internalName == obeliskName).FirstOrDefault();
            if (obelisk.HasValue)
                return new GameLocation(null, obelisk.Value.RegionID, obelisk.Value.X - 100, obelisk.Value.Y - 4, obelisk.Value.Z, obelisk.Value.H);
            return null;
        }
        public static GameLocation GetRandomCrossroadsLocation()
        {
            List<ObeliskStruct> obelisks = obeliskList.Where(o => o.internalName == "Nurizanes_Crossroads").ToList();
            if (obelisks.Count > 0)
            {
                ObeliskStruct? obelisk = obelisks[Util.Random(0, obelisks.Count - 1)];
                if (obelisk.HasValue)
                    return new GameLocation(null, obelisk.Value.RegionID, obelisk.Value.X - 100, obelisk.Value.Y - 4, obelisk.Value.Z, obelisk.Value.H);
            }
            return null;
        }


        #endregion Public methods

        #region Obelisk List

        /// <summary>
        /// Populate obelisk list
        /// </summary>
        private static void PopulateobeliskList()
        {
            #region Obelisk: Diabasi's Junction

            ObeliskStruct Obelisk1 = new ObeliskStruct();
            Obelisk1.internalName = "Diabasis_Junction";
            Obelisk1.NameEN = "Diabasi's Junction";
            Obelisk1.NameDE = "Diabasi's Junction";
            Obelisk1.NameFR = "Diabasi's Junction";
            Obelisk1.ObeliskCredit = typeof(Quest.ObeliskCredit.DiabasisJunction);
            Obelisk1.X = 52788;
            Obelisk1.Y = 21906;
            Obelisk1.Z = 25897;
            Obelisk1.H = 512;
            Obelisk1.RegionID = 245;
            obeliskList.Add(Obelisk1);

            #endregion Obelisk: Diabasi's Junction

            #region Obelisk: Dracolich Den

            ObeliskStruct Obelisk2 = new ObeliskStruct();
            Obelisk2.internalName = "Dracolich_Den";
            Obelisk2.NameEN = "Dracolich Den";
            Obelisk2.NameDE = "Dracolich Den";
            Obelisk2.NameFR = "Dracolich Den";
            Obelisk2.ObeliskCredit = typeof(Quest.ObeliskCredit.DracolichDen);
            Obelisk2.X = 46596;
            Obelisk2.Y = 62035;
            Obelisk2.Z = 22406;
            Obelisk2.H = 3072;
            Obelisk2.RegionID = 245;
            obeliskList.Add(Obelisk2);
            #endregion Obelisk: Dracolich Den

            #region Obelisk: Dynami's Crossing
            ObeliskStruct Obelisk3 = new ObeliskStruct();
            Obelisk3.ObeliskCredit = typeof(Quest.ObeliskCredit.DynamisCrossing);
            Obelisk3.internalName = "Dynamis_Crossing";
            Obelisk3.NameEN = "Dynami's Crossing";
            Obelisk3.NameDE = "Dynami's Crossing";
            Obelisk3.NameFR = "Dynami's Crossing";
            Obelisk3.X = 40987;
            Obelisk3.Y = 20201;
            Obelisk3.Z = 25897;
            Obelisk3.H = 0;
            Obelisk3.RegionID = 245;
            obeliskList.Add(Obelisk3);
            #endregion

            #region Obelisk: Shrine of Vartigeth (North)
            ObeliskStruct Obelisk4 = new ObeliskStruct();
            Obelisk4.ObeliskCredit = typeof(Quest.ObeliskCredit.ShrineofVartigethNorth);
            Obelisk4.internalName = "Shrine_of_Vartigeth_(North)";
            Obelisk4.NameEN = "Shrine of Vartigeth (North)";
            Obelisk4.NameDE = "Shrine of Vartigeth (North)";
            Obelisk4.NameFR = "Shrine of Vartigeth (North)";
            Obelisk4.X = 30592;
            Obelisk4.Y = 11639;
            Obelisk4.Z = 25897;
            Obelisk4.H = 1024;
            Obelisk4.RegionID = 245;
            obeliskList.Add(Obelisk4);
            #endregion

            #region Obelisk: Hall of Thanatoy
            ObeliskStruct Obelisk5 = new ObeliskStruct();
            Obelisk5.ObeliskCredit = typeof(Quest.ObeliskCredit.HallofThanatoy);
            Obelisk5.internalName = "Hall_of_Thanatoy";
            Obelisk5.NameEN = "Hall of Thanatoy";
            Obelisk5.NameDE = "Hall of Thanatoy";
            Obelisk5.NameFR = "Hall of Thanatoy";
            Obelisk5.X = 53406;
            Obelisk5.Y = 51510;
            Obelisk5.Z = 25897;
            Obelisk5.H = 2560;
            Obelisk5.RegionID = 245;
            obeliskList.Add(Obelisk5);
            #endregion

            #region Obelisk: Anapaysi's Crossing
            ObeliskStruct Obelisk6 = new ObeliskStruct();
            Obelisk6.ObeliskCredit = typeof(Quest.ObeliskCredit.AnapaysisCrossing);
            Obelisk6.internalName = "Anapaysis_Crossing";
            Obelisk6.NameEN = "Anapaysi's Crossing";
            Obelisk6.NameDE = "Anapaysi's Crossing";
            Obelisk6.NameFR = "Anapaysi's Crossing";
            Obelisk6.X = 43269;
            Obelisk6.Y = 55902;
            Obelisk6.Z = 25897;
            Obelisk6.H = 2510;
            Obelisk6.RegionID = 245;
            obeliskList.Add(Obelisk6);
            #endregion

            #region Obelisk: Temple of Februstos (North)
            ObeliskStruct Obelisk7 = new ObeliskStruct();
            Obelisk7.ObeliskCredit = typeof(Quest.ObeliskCredit.TempleofFebrustosNorth);
            Obelisk7.internalName = "Temple_of_Februstos_(North)";
            Obelisk7.NameEN = "Temple of Februstos (North)";
            Obelisk7.NameDE = "Temple of Februstos (North)";
            Obelisk7.NameFR = "Temple of Februstos (North)";
            Obelisk7.X = 53409;
            Obelisk7.Y = 12405;
            Obelisk7.Z = 28457;
            Obelisk7.H = 3072;
            Obelisk7.RegionID = 245;
            obeliskList.Add(Obelisk7);
            #endregion

            #region Obelisk: Shrine of Laresh (West)
            ObeliskStruct Obelisk8 = new ObeliskStruct();
            Obelisk8.ObeliskCredit = typeof(Quest.ObeliskCredit.ShrineofLareshWest);
            Obelisk8.internalName = "Shrine_of_Laresh_(West)";
            Obelisk8.NameEN = "Shrine of Laresh (West)";
            Obelisk8.NameDE = "Shrine of Laresh (West)";
            Obelisk8.NameFR = "Shrine of Laresh (West)";
            Obelisk8.X = 38028;
            Obelisk8.Y = 35292;
            Obelisk8.Z = 27433;
            Obelisk8.H = 0;
            Obelisk8.RegionID = 245;
            obeliskList.Add(Obelisk8);
            #endregion

            #region Obelisk: Shrine of Vartigeth (West)
            ObeliskStruct Obelisk9 = new ObeliskStruct();
            Obelisk9.ObeliskCredit = typeof(Quest.ObeliskCredit.ShrineofVartigethWest);
            Obelisk9.internalName = "Shrine_of_Vartigeth_(West)";
            Obelisk9.NameEN = "Shrine of Vartigeth (West)";
            Obelisk9.NameDE = "Shrine of Vartigeth (West)";
            Obelisk9.NameFR = "Shrine of Vartigeth (West)";
            Obelisk9.X = 30342;
            Obelisk9.Y = 44352;
            Obelisk9.Z = 24872;
            Obelisk9.H = 2560;
            Obelisk9.RegionID = 245;
            obeliskList.Add(Obelisk9);
            #endregion

            #region Obelisk: Passage of Ygros
            ObeliskStruct Obelisk10 = new ObeliskStruct();
            Obelisk10.ObeliskCredit = typeof(Quest.ObeliskCredit.PassageofYgros);
            Obelisk10.internalName = "Passage_of_Ygros";
            Obelisk10.NameEN = "Passage of Ygros";
            Obelisk10.NameDE = "Passage of Ygros";
            Obelisk10.NameFR = "Passage of Ygros";
            Obelisk10.X = 32513;
            Obelisk10.Y = 62455;
            Obelisk10.Z = 27432;
            Obelisk10.H = 3584;
            Obelisk10.RegionID = 245;
            obeliskList.Add(Obelisk10);
            #endregion

            #region Obelisk: Temple of Perizor (West)
            ObeliskStruct Obelisk11 = new ObeliskStruct();
            Obelisk11.ObeliskCredit = typeof(Quest.ObeliskCredit.TempleofPerizorWest);
            Obelisk11.internalName = "Temple_of_Perizor_(West)";
            Obelisk11.NameEN = "Temple of Perizor (West)";
            Obelisk11.NameDE = "Temple of Perizor (West)";
            Obelisk11.NameFR = "Temple of Perizor (West)";
            Obelisk11.X = 26788;
            Obelisk11.Y = 24915;
            Obelisk11.Z = 24368;
            Obelisk11.H = 992;
            Obelisk11.RegionID = 245;
            obeliskList.Add(Obelisk11);
            #endregion

            #region Obelisk: Hall of Dimioyrgia
            ObeliskStruct Obelisk13 = new ObeliskStruct();
            Obelisk13.ObeliskCredit = typeof(Quest.ObeliskCredit.HallofDimioyrgia);
            Obelisk13.internalName = "Hall_of_Dimioyrgia";
            Obelisk13.NameEN = "Hall of Dimioyrgia";
            Obelisk13.NameDE = "Hall of Dimioyrgia";
            Obelisk13.NameFR = "Hall of Dimioyrgia";
            Obelisk13.X = 31066;
            Obelisk13.Y = 63903;
            Obelisk13.Z = 29481;
            Obelisk13.H = 2560;
            Obelisk13.RegionID = 245;
            obeliskList.Add(Obelisk13);
            #endregion

            #region Obelisk: Kainotomia's Crossing
            ObeliskStruct Obelisk14 = new ObeliskStruct();
            Obelisk14.ObeliskCredit = typeof(Quest.ObeliskCredit.KainotomiasCrossing);
            Obelisk14.internalName = "Kainotomias_Crossing";
            Obelisk14.NameEN = "Kainotomia's Crossing";
            Obelisk14.NameDE = "Kainotomia's Crossing";
            Obelisk14.NameFR = "Kainotomia's Crossing";
            Obelisk14.X = 44091;
            Obelisk14.Y = 53765;
            Obelisk14.Z = 29481;
            Obelisk14.H = 1536;
            Obelisk14.RegionID = 245;
            obeliskList.Add(Obelisk14);
            #endregion

            #region Obelisk: Shrine of Teragani (North)
            ObeliskStruct Obelisk15 = new ObeliskStruct();
            Obelisk15.ObeliskCredit = typeof(Quest.ObeliskCredit.ShrineofTeraganiNorth);
            Obelisk15.internalName = "Shrine_of_Teragani_(North)";
            Obelisk15.NameEN = "Shrine of Teragani (North)";
            Obelisk15.NameDE = "Shrine of Teragani (North)";
            Obelisk15.NameFR = "Shrine of Teragani (North)";
            Obelisk15.X = 66427;
            Obelisk15.Y = 27654;
            Obelisk15.Z = 28480;
            Obelisk15.H = 3670;
            Obelisk15.RegionID = 245;
            obeliskList.Add(Obelisk15);
            #endregion

            #region Obelisk: Catacombs of Februstos
            ObeliskStruct Obelisk16 = new ObeliskStruct();
            Obelisk16.ObeliskCredit = typeof(Quest.ObeliskCredit.CatacombsofFebrustos);
            Obelisk16.internalName = "Catacombs_of_Februstos";
            Obelisk16.NameEN = "Catacombs of Februstos";
            Obelisk16.NameDE = "Catacombs of Februstos";
            Obelisk16.NameFR = "Catacombs of Februstos";
            Obelisk16.X = 54236;
            Obelisk16.Y = 20459;
            Obelisk16.Z = 23593;
            Obelisk16.H = 2560;
            Obelisk16.RegionID = 245;
            obeliskList.Add(Obelisk16);
            #endregion

            #region Obelisk: Shrine of Teragani (South)
            ObeliskStruct Obelisk17 = new ObeliskStruct();
            Obelisk17.ObeliskCredit = typeof(Quest.ObeliskCredit.ShrineofTeraganiSouth);
            Obelisk17.internalName = "Shrine_of_Teragani_(South)";
            Obelisk17.NameEN = "Shrine of Teragani (South)";
            Obelisk17.NameDE = "Shrine of Teragani (South)";
            Obelisk17.NameFR = "Shrine of Teragani (South)";
            Obelisk17.X = 65411;
            Obelisk17.Y = 47365;
            Obelisk17.Z = 24361;
            Obelisk17.H = 3543;
            Obelisk17.RegionID = 245;
            obeliskList.Add(Obelisk17);
            #endregion

            #region Obelisk: Shrine of Laresh (East)
            ObeliskStruct Obelisk18 = new ObeliskStruct();
            Obelisk18.ObeliskCredit = typeof(Quest.ObeliskCredit.ShrineofLareshEast);
            Obelisk18.internalName = "Shrine_of_Laresh_(East)";
            Obelisk18.NameEN = "Shrine of Laresh (East)";
            Obelisk18.NameDE = "Shrine of Laresh (East)";
            Obelisk18.NameFR = "Shrine of Laresh (East)";
            Obelisk18.X = 64015;
            Obelisk18.Y = 47609;
            Obelisk18.Z = 30504;
            Obelisk18.H = 3072;
            Obelisk18.RegionID = 245;
            obeliskList.Add(Obelisk18);
            #endregion

            #region Obelisk: Shrine of Vartigeth (South)
            ObeliskStruct Obelisk19 = new ObeliskStruct();
            Obelisk19.ObeliskCredit = typeof(Quest.ObeliskCredit.ShrineofVartigethSouth);
            Obelisk19.internalName = "Shrine_of_Vartigeth_(South)";
            Obelisk19.NameEN = "Shrine of Vartigeth (South)";
            Obelisk19.NameDE = "Shrine of Vartigeth (South)";
            Obelisk19.NameFR = "Shrine of Vartigeth (South)";
            Obelisk19.X = 26642;
            Obelisk19.Y = 65243;
            Obelisk19.Z = 27944;
            Obelisk19.H = 3584;
            Obelisk19.RegionID = 245;
            obeliskList.Add(Obelisk19);
            #endregion

            #region Obelisk: Path of Roloi
            ObeliskStruct Obelisk20 = new ObeliskStruct();
            Obelisk20.ObeliskCredit = typeof(Quest.ObeliskCredit.PathofRoloi);
            Obelisk20.internalName = "Path_of_Roloi";
            Obelisk20.NameEN = "Path of Roloi";
            Obelisk20.NameDE = "Path of Roloi";
            Obelisk20.NameFR = "Path of Roloi";
            Obelisk20.X = 54238;
            Obelisk20.Y = 43631;
            Obelisk20.Z = 30504;
            Obelisk20.H = 3584;
            Obelisk20.RegionID = 245;
            obeliskList.Add(Obelisk20);
            #endregion

            #region Obelisk: Ergaleio's Path
            ObeliskStruct Obelisk21 = new ObeliskStruct();
            Obelisk21.ObeliskCredit = typeof(Quest.ObeliskCredit.ErgaleiosPath);
            Obelisk21.internalName = "Ergaleios_Path";
            Obelisk21.NameEN = "Ergaleio's Path";
            Obelisk21.NameDE = "Ergaleio's Path";
            Obelisk21.NameFR = "Ergaleio's Path";
            Obelisk21.X = 64375;
            Obelisk21.Y = 53766;
            Obelisk21.Z = 30504;
            Obelisk21.H = 1536;
            Obelisk21.RegionID = 245;
            obeliskList.Add(Obelisk21);
            #endregion

            #region Obelisk: Great Forge of Thivek
            ObeliskStruct Obelisk22 = new ObeliskStruct();
            Obelisk22.ObeliskCredit = typeof(Quest.ObeliskCredit.GreatForgeofThivek);
            Obelisk22.internalName = "Great_Forge_of_Thivek";
            Obelisk22.NameEN = "Great Forge of Thivek";
            Obelisk22.NameDE = "Great Forge of Thivek";
            Obelisk22.NameFR = "Great Forge of Thivek";
            Obelisk22.X = 31064;
            Obelisk22.Y = 52317;
            Obelisk22.Z = 29481;
            Obelisk22.H = 512;
            Obelisk22.RegionID = 245;
            obeliskList.Add(Obelisk22);
            #endregion

            #region Obelisk: Discovery's Crossing
            ObeliskStruct Obelisk23 = new ObeliskStruct();
            Obelisk23.ObeliskCredit = typeof(Quest.ObeliskCredit.DiscoverysCrossing);
            Obelisk23.internalName = "Discoverys_Crossing";
            Obelisk23.NameEN = "Discovery's Crossing";
            Obelisk23.NameDE = "Discovery's Crossing";
            Obelisk23.NameFR = "Discovery's Crossing";
            Obelisk23.X = 48443;
            Obelisk23.Y = 32042;
            Obelisk23.Z = 26921;
            Obelisk23.H = 2560;
            Obelisk23.RegionID = 245;
            obeliskList.Add(Obelisk23);
            #endregion

            #region Obelisk: Plimmyra's Landing
            ObeliskStruct Obelisk24 = new ObeliskStruct();
            Obelisk24.ObeliskCredit = typeof(Quest.ObeliskCredit.PlimmyrasLanding);
            Obelisk24.internalName = "Plimmyras_Landing";
            Obelisk24.NameEN = "Plimmyra's Landing";
            Obelisk24.NameDE = "Plimmyra's Landing";
            Obelisk24.NameFR = "Plimmyra's Landing";
            Obelisk24.X = 49260;
            Obelisk24.Y = 45719;
            Obelisk24.Z = 24867;
            Obelisk24.H = 2560;
            Obelisk24.RegionID = 245;
            obeliskList.Add(Obelisk24);
            #endregion

            #region Obelisk: Construct Assembly Room
            ObeliskStruct Obelisk25 = new ObeliskStruct();
            Obelisk25.ObeliskCredit = typeof(Quest.ObeliskCredit.ConstructAssemblyRoom);
            Obelisk25.internalName = "Construct_Assembly_Room";
            Obelisk25.NameEN = "Construct Assembly Room";
            Obelisk25.NameDE = "Construct Assembly Room";
            Obelisk25.NameFR = "Construct Assembly Room";
            Obelisk25.X = 22946;
            Obelisk25.Y = 33292;
            Obelisk25.Z = 27433;
            Obelisk25.H = 3072;
            Obelisk25.RegionID = 245;
            obeliskList.Add(Obelisk25);
            #endregion

            #region Obelisk: Hall of Allagi
            ObeliskStruct Obelisk26 = new ObeliskStruct();
            Obelisk26.ObeliskCredit = typeof(Quest.ObeliskCredit.HallofAllagi);
            Obelisk26.internalName = "Hall_of_Allagi";
            Obelisk26.NameEN = "Hall of Allagi";
            Obelisk26.NameDE = "Hall of Allagi";
            Obelisk26.NameFR = "Hall of Allagi";
            Obelisk26.X = 22948;
            Obelisk26.Y = 39021;
            Obelisk26.Z = 29481;
            Obelisk26.H = 0;
            Obelisk26.RegionID = 245;
            obeliskList.Add(Obelisk26);
            #endregion

            #region Obelisk: Shrine of Nethuni (North)
            ObeliskStruct Obelisk27 = new ObeliskStruct();
            Obelisk27.ObeliskCredit = typeof(Quest.ObeliskCredit.ShrineofNethuniNorth);
            Obelisk27.internalName = "Shrine_of_Nethuni_(North)";
            Obelisk27.NameEN = "Shrine of Nethuni (North)";
            Obelisk27.NameDE = "Shrine of Nethuni (North)";
            Obelisk27.NameFR = "Shrine of Nethuni (North)";
            Obelisk27.X = 38306;
            Obelisk27.Y = 47974;
            Obelisk27.Z = 24872;
            Obelisk27.H = 3584;
            Obelisk27.RegionID = 245;
            obeliskList.Add(Obelisk27);
            #endregion

            #region Obelisk: Temple of Februstos (South)
            ObeliskStruct Obelisk28 = new ObeliskStruct();
            Obelisk28.ObeliskCredit = typeof(Quest.ObeliskCredit.TempleofFebrustosSouth);
            Obelisk28.internalName = "Temple_of_Februstos_(South)";
            Obelisk28.NameEN = "Temple of Februstos (South)";
            Obelisk28.NameDE = "Temple of Februstos (South)";
            Obelisk28.NameFR = "Temple of Februstos (South)";
            Obelisk28.X = 65834;
            Obelisk28.Y = 63891;
            Obelisk28.Z = 29992;
            Obelisk28.H = 3584;
            Obelisk28.RegionID = 245;
            obeliskList.Add(Obelisk28);
            #endregion

            #region Obelisk: Trela's Crossing
            ObeliskStruct Obelisk29 = new ObeliskStruct();
            Obelisk29.ObeliskCredit = typeof(Quest.ObeliskCredit.TrelasCrossing);
            Obelisk29.internalName = "Trelas_Crossing";
            Obelisk29.NameEN = "Trela's Crossing";
            Obelisk29.NameDE = "Trela's Crossing";
            Obelisk29.NameFR = "Trela's Crossing";
            Obelisk29.X = 25183;
            Obelisk29.Y = 30084;
            Obelisk29.Z = 29993;
            Obelisk29.H = 512;
            Obelisk29.RegionID = 245;
            obeliskList.Add(Obelisk29);
            #endregion

            #region Obelisk: Temple of Perizor (East)
            ObeliskStruct Obelisk30 = new ObeliskStruct();
            Obelisk30.ObeliskCredit = typeof(Quest.ObeliskCredit.TempleofPerizorEast);
            Obelisk30.internalName = "Temple_of_Perizor_(East)";
            Obelisk30.NameEN = "Temple of Perizor (East)";
            Obelisk30.NameDE = "Temple of Perizor (East)";
            Obelisk30.NameFR = "Temple of Perizor (East)";
            Obelisk30.X = 49891;
            Obelisk30.Y = 51231;
            Obelisk30.Z = 29481;
            Obelisk30.H = 3072;
            Obelisk30.RegionID = 245;
            obeliskList.Add(Obelisk30);
            #endregion

            #region Obelisk: Path of Zoi
            ObeliskStruct Obelisk32 = new ObeliskStruct();
            Obelisk32.ObeliskCredit = typeof(Quest.ObeliskCredit.PathofZoi);
            Obelisk32.internalName = "Path_of_Zoi";
            Obelisk32.NameEN = "Path of Zoi";
            Obelisk32.NameDE = "Path of Zoi";
            Obelisk32.NameFR = "Path of Zoi";
            Obelisk32.X = 29617;
            Obelisk32.Y = 36388;
            Obelisk32.Z = 24872;
            Obelisk32.H = 3584;
            Obelisk32.RegionID = 245;
            obeliskList.Add(Obelisk32);
            #endregion

            #region Obelisk: Temple of Laresh
            ObeliskStruct Obelisk33 = new ObeliskStruct();
            Obelisk33.ObeliskCredit = typeof(Quest.ObeliskCredit.TempleofLaresh);
            Obelisk33.internalName = "Temple_of_Laresh";
            Obelisk33.NameEN = "Temple of Laresh";
            Obelisk33.NameDE = "Temple of Laresh";
            Obelisk33.NameFR = "Temple of Laresh";
            Obelisk33.X = 16181;
            Obelisk33.Y = 52443;
            Obelisk33.Z = 26408;
            Obelisk33.H = 512;
            Obelisk33.RegionID = 245;
            obeliskList.Add(Obelisk33);
            #endregion

            #region Obelisk: Hall of Feretro
            ObeliskStruct Obelisk34 = new ObeliskStruct();
            Obelisk34.ObeliskCredit = typeof(Quest.ObeliskCredit.HallofFeretro);
            Obelisk34.internalName = "Hall_of_Feretro";
            Obelisk34.NameEN = "Hall of Feretro";
            Obelisk34.NameDE = "Hall of Feretro";
            Obelisk34.NameFR = "Hall of Feretro";
            Obelisk34.X = 38907;
            Obelisk34.Y = 33706;
            Obelisk34.Z = 24872;
            Obelisk34.H = 512;
            Obelisk34.RegionID = 245;
            obeliskList.Add(Obelisk34);
            #endregion

            #region Obelisk: Shrine of Nethuni (South)
            ObeliskStruct Obelisk36 = new ObeliskStruct();
            Obelisk36.ObeliskCredit = typeof(Quest.ObeliskCredit.ShrineofNethuniSouth);
            Obelisk36.internalName = "Shrine_of_Nethuni_(South)";
            Obelisk36.NameEN = "Shrine of Nethuni (South)";
            Obelisk36.NameDE = "Shrine of Nethuni (South)";
            Obelisk36.NameFR = "Shrine of Nethuni (South)";
            Obelisk36.X = 44099;
            Obelisk36.Y = 65355;
            Obelisk36.Z = 29481;
            Obelisk36.H = 512;
            Obelisk36.RegionID = 245;
            obeliskList.Add(Obelisk36);
            #endregion

            #region Obelisk: Efeyresi's Junction
            ObeliskStruct Obelisk37 = new ObeliskStruct();
            Obelisk37.ObeliskCredit = typeof(Quest.ObeliskCredit.EfeyresisJunction);
            Obelisk37.internalName = "Efeyresis_Junction";
            Obelisk37.NameEN = "Efeyresi's Junction";
            Obelisk37.NameDE = "Efeyresi's Junction";
            Obelisk37.NameFR = "Efeyresi's Junction";
            Obelisk37.X = 33962;
            Obelisk37.Y = 46525;
            Obelisk37.Z = 29481;
            Obelisk37.H = 3584;
            Obelisk37.RegionID = 245;
            obeliskList.Add(Obelisk37);
            #endregion

            #region Obelisk: Shrine of Tegashirg
            ObeliskStruct Obelisk38 = new ObeliskStruct();
            Obelisk38.ObeliskCredit = typeof(Quest.ObeliskCredit.ShrineofTegashirg);
            Obelisk38.internalName = "Shrine_of_Tegashirg";
            Obelisk38.NameEN = "Shrine of Tegashirg";
            Obelisk38.NameDE = "Shrine of Tegashirg";
            Obelisk38.NameFR = "Shrine of Tegashirg";
            Obelisk38.X = 41199;
            Obelisk38.Y = 39285;
            Obelisk38.Z = 29481;
            Obelisk38.H = 512;
            Obelisk38.RegionID = 245;
            obeliskList.Add(Obelisk38);
            #endregion

            #region Obelisk: Ygrasia's Crossing
            ObeliskStruct Obelisk39 = new ObeliskStruct();
            Obelisk39.ObeliskCredit = typeof(Quest.ObeliskCredit.YgrasiasCrossing);
            Obelisk39.internalName = "Ygrasias_Crossing";
            Obelisk39.NameEN = "Ygrasia's Crossing";
            Obelisk39.NameDE = "Ygrasia's Crossing";
            Obelisk39.NameFR = "Ygrasia's Crossing";
            Obelisk39.X = 23825;
            Obelisk39.Y = 42180;
            Obelisk39.Z = 26408;
            Obelisk39.H = 2560;
            Obelisk39.RegionID = 245;
            obeliskList.Add(Obelisk39);
            #endregion

            #region Obelisk: Agramon's Lair
            ObeliskStruct Obelisk40 = new ObeliskStruct();
            Obelisk40.ObeliskCredit = typeof(Quest.ObeliskCredit.AgramonsLair);
            Obelisk40.internalName = "Agramons_Lair";
            Obelisk40.NameEN = "Agramon's Lair";
            Obelisk40.NameDE = "Agramon's Lair";
            Obelisk40.NameFR = "Agramon's Lair";
            Obelisk40.X = 56463;
            Obelisk40.Y = 31374;
            Obelisk40.Z = 24976;
            Obelisk40.H = 3584;
            Obelisk40.RegionID = 245;
            obeliskList.Add(Obelisk40);
            #endregion

            #region Obelisk: Forge of Pyrkagia
            ObeliskStruct Obelisk43 = new ObeliskStruct();
            Obelisk43.ObeliskCredit = typeof(Quest.ObeliskCredit.ForgeofPyrkagia);
            Obelisk43.internalName = "Forge_of_Pyrkagia";
            Obelisk43.NameEN = "Forge of Pyrkagia";
            Obelisk43.NameDE = "Forge of Pyrkagia";
            Obelisk43.NameFR = "Forge of Pyrkagia";
            Obelisk43.X = 53388;
            Obelisk43.Y = 42461;
            Obelisk43.Z = 27433;
            Obelisk43.H = 1024;
            Obelisk43.RegionID = 245;
            obeliskList.Add(Obelisk43);
            #endregion

            #region Obelisk: Albion Entrance
            ObeliskStruct Obelisk44 = new ObeliskStruct();
            Obelisk44.X = 65461;
            Obelisk44.Y = 42363;
            Obelisk44.Z = 30292;
            Obelisk44.H = 568;
            Obelisk44.RegionID = 245;
            obeliskList.Add(Obelisk44);
            #endregion

            #region Obelisk: Hibernian Entrance
            ObeliskStruct Obelisk45 = new ObeliskStruct();
            Obelisk45.X = 29873;
            Obelisk45.Y = 20252;
            Obelisk45.Z = 30292;
            Obelisk45.H = 2582;
            Obelisk45.RegionID = 245;
            obeliskList.Add(Obelisk45);
            #endregion

            #region Obelisk: Midgardian Enrance
            ObeliskStruct Obelisk12 = new ObeliskStruct();
            Obelisk12.X = 65960;
            Obelisk12.Y = 13560;
            Obelisk12.Z = 30292;
            Obelisk12.H = 3538;
            Obelisk12.RegionID = 245;
            obeliskList.Add(Obelisk12);
            #endregion

            #region Obelisk: Nurizane's Crossroads

            #region North

            ObeliskStruct crossroadsObelisk1 = new ObeliskStruct();
            crossroadsObelisk1.internalName = "Nurizanes_Crossroads";
            crossroadsObelisk1.NameEN = "Nurizane's Crossroads";
            crossroadsObelisk1.NameDE = "Nurizane's Crossroads";
            crossroadsObelisk1.NameFR = "Nurizane's Crossroads";
            crossroadsObelisk1.X = 49784;
            crossroadsObelisk1.Y = 29145;
            crossroadsObelisk1.Z = 30068;
            crossroadsObelisk1.H = 1027;
            crossroadsObelisk1.RegionID = 245;
            obeliskList.Add(crossroadsObelisk1);

            #endregion North

            #region South

            ObeliskStruct crossroadsObelisk2 = new ObeliskStruct();
            crossroadsObelisk2.internalName = "Nurizanes_Crossroads";
            crossroadsObelisk2.NameEN = "Nurizane's Crossroads";
            crossroadsObelisk2.NameDE = "Nurizane's Crossroads";
            crossroadsObelisk2.NameFR = "Nurizane's Crossroads";
            crossroadsObelisk2.X = 52897;
            crossroadsObelisk2.Y = 29147;
            crossroadsObelisk2.Z = 30068;
            crossroadsObelisk2.H = 3012;
            crossroadsObelisk2.RegionID = 245;
            obeliskList.Add(crossroadsObelisk2);

            #endregion South

            #region West

            ObeliskStruct crossroadsObelisk3 = new ObeliskStruct();
            crossroadsObelisk3.internalName = "Nurizanes_Crossroads";
            crossroadsObelisk3.NameEN = "Nurizane's Crossroads";
            crossroadsObelisk3.NameDE = "Nurizane's Crossroads";
            crossroadsObelisk3.NameFR = "Nurizane's Crossroads";
            crossroadsObelisk3.X = 51339;
            crossroadsObelisk3.Y = 27586;
            crossroadsObelisk3.Z = 30068;
            crossroadsObelisk3.H = 2028;
            crossroadsObelisk3.RegionID = 245;
            obeliskList.Add(crossroadsObelisk3);

            #endregion West

            #region Est

            ObeliskStruct crossroadsObelisk4 = new ObeliskStruct();
            crossroadsObelisk4.internalName = "Nurizanes_Crossroads";
            crossroadsObelisk4.NameEN = "Nurizane's Crossroads";
            crossroadsObelisk4.NameDE = "Nurizane's Crossroads";
            crossroadsObelisk4.NameFR = "Nurizane's Crossroads";
            crossroadsObelisk4.X = 51340;
            crossroadsObelisk4.Y = 30706;
            crossroadsObelisk4.Z = 30068;
            crossroadsObelisk4.H = 4045;
            crossroadsObelisk4.RegionID = 245;
            obeliskList.Add(crossroadsObelisk4);

            #endregion Est

            #endregion Obelisk: Nurizane's Crossroads
        }

        #endregion Obelisk List
    }
}
