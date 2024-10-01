/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 * -BluRaven 3/30/09
 * Okay so I know it's totally hard-coded and inflexible.
 * Someone should re-write this into an obelisk manager,
 * but that is above my present skill level in c#.
 * But hey, this is better then not having obelisks at all.
 * 
 * TODO:
 * add in the quest's/activating.
 * 
 * notes:
 * Obelisk in mob DB should be type DOL.GS.Obelisk
 * this script should come with a mob.sql and a teleport.sql,
 * if it did not, I put them in comments at the bottom as well.
 * 
 * -Blu
 */
using System;
using log4net;
using System.Reflection;
using DOL.Events;
using DOL.Database;
using DOL.GS.Quests.Catacombs.Obelisks;

namespace DOL.GS
{
    /// <summary>
    /// Obelisk (Catacombs teleporter stone).
    /// </summary>
    /// <author>BluRaven, based on the work of Aredhel</author>
    public class Obelisk : GameTeleporter
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Creates a new obelisk.
        /// </summary>
        public Obelisk()
            : base()
        {
            Realm = eRealm.None;
            Flags += (int)eFlags.DONTSHOWNAME;
            Flags += (int)eFlags.PEACE;
        }

        /// <summary>
        /// Teleporter type, needed to pick the right TeleportID.
        /// </summary>
        protected override String Type
        {
            get { return "Obelisk"; }
        }

        public override void TurnTo(ushort heading, int duration)
        {
            //do nothing.  Obelisks don't turn!
        }

        /// <summary>
        /// Player right-clicked the Obelisk.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                return false;

            //TODO: Check if player has completed the quest of this obelisk, if he has, ignore and continue, if he has not, give him the quest
            //complete the quest, and play spell effect of activating the obelisk, then break and wait for next interact.
            
            //check if player has our quest and if not, give it and complete it.
            //switch (this.CurrentRegionID)
            //{
            //    case 229: //Mid-Burial Grounds Obelisk
            //        {
            //            break;
            //        }
            //    case 243: //Mid-Kobold Undercity Obelisk
            //        {
            //            KoboldUndercity KoboldCityQuest = new KoboldUndercity(player);
            //            if (player.HasFinishedQuest(KoboldCityQuest.GetType()) == 0)
            //            {
            //                KoboldCityQuest.FinishQuest();
            //                player.Out.SendSpellEffectAnimation(this, this, 9024, 0, false, 1);
            //                SayTo(player, "You have activated this Obelisk.");
            //                return true;
            //            }
            //            break;
            //        }
            //    default:
            //        {
            //            break;
            //        }
            //}

            //Give the player the proper list of choices depending on which obelisk we are.
            String portlocations = "";
            switch (this.CurrentRegionID)
            {
                case 148: //Mid - Frontlines
                case 58:  //Mid - Underground Forest
                case 226: //Mid - Abandoned Mines
                case 229: //Mid - burial grounds
                    {
                        portlocations += "Would you like to be teleported to the [Kobold Undercity]?\n";
                        break;
                    }

                    
                //Mid One-way obelisks
                case 162: //Mid - Deadlands of Annwn
                case 149: //Mid - Nyttheim
                case 195: //Mid - Otherworld
                case 189: //Mid - Glashtin Forges
                    {
                        portlocations += "Would you like to be teleported to the [Kobold Undercity]? This is a one-way Obelisk.  You will not be able to teleport to this Obelisk from the Kobold Undercity.";
                        break;
                    }


                case 63:  //Alb - Roman Aqueducts
                case 66:  //Alb - Albion's Underground Forest
                case 227: //Alb - Abandoned Mines
                case 67:  //Alb - Albion's Deadlands of Annwn
                    {
                        portlocations += "Would you like to be teleported to the [Inconnu Crypt]?\n";
                        break;
                    }

                    //ALB One-Way Obelesks
                case 68:  //Alb - Lower Crypt
                case 59:  //Alb - Albion's Glashtin Forges
                case 109: //Alb - Albion's Frontlines
                case 196: //Alb - Albion's Otherworld
                    {
                        portlocations += "Would you like to be teleported to the [Inconnu Crypt]? This is a one-way Obelisk.  You will not be able to teleport to this Obelisk from the Inconnu Crypt.";
                        break;
                    }


                case 92:  //Hib - Veil Rift
                case 96:  //Hib - Hibernia's Underground Forest
                case 197: //Hib - Hibernia's Otherworld
                case 228: //Hib - Hibernia's Abandoned Mines
                    {
                        portlocations += "Would you like to be teleported to the [Shar Labyrinth]?\n";
                        break;
                    }
    
                //Hib one-way Obelisks
                case 94:  //Hib - The Queen's Labyrinth
                case 99:  //Hib - Hibernia's Glashtin Forge
                case 97:  //Hib - Hibernia's Deadlands of Annwn
                case 95:  //Hib - Hibernia's Frontlines
                    {
                        portlocations += "Would you like to be teleported to the [Shar Labyrinth]? This is a one-way Obelisk.  You will not be able to teleport to this Obelisk from the Shar Labyrinth.";
                        break;
                    }




                case 243: //Mid - Kobold Undercity
                    {
                        //TODO: Check for quests here!
                        portlocations += "Would you like to be teleported to the [Burial Grounds]?\n";
                        portlocations += "Would you like to be teleported to the [Abandoned Mines]?\n";
                        portlocations += "Would you like to be teleported to the [Frontlines]?\n";
                        portlocations += "Would you like to be teleported to the [Underground Forest]?\n";
                        portlocations += "Would you like to be teleported to [Jordheim]?\n";
                        break;
                    }
                case 65: //Alb - Inconnu Crypt
                    {
                        //TODO: Check for quests here!
                        portlocations += "Would you like to be teleported to the [Roman Aqueducts]?\n";
                        portlocations += "Would you like to be teleported to the [Abandoned Mines]?\n";
                        portlocations += "Would you like to be teleported to the [Underground Forest]?\n";
                        portlocations += "Would you like to be teleported to the [Deadlands of Annwn]?\n";
                        portlocations += "Would you like to be teleported to [Camelot]?\n";
                        break;
                    }
                case 93: //Hib - Shar Labyrinth
                    {
                        //TODO: Check for quests here!
                        portlocations += "Would you like to be teleported to the [Veil Rift]?\n";
                        portlocations += "Would you like to be teleported to the [Abandoned Mines]?\n";
                        portlocations += "Would you like to be teleported to the [Underground Forest]?\n";
                        portlocations += "Would you like to be teleported to the [Otherworld]?\n";
                        portlocations += "Would you like to be teleported to [Tir na Nog]?\n";
                        break;
                    }

                default: //Error, we don't have a port location
                    {
                        portlocations += "An Error occurred, I don't have any destinations for the region I'm located in!  Sorry.\n";
                        break;
                    }

            }

            SayTo(player, portlocations);
            return true;
        }

        /// <summary>
        /// Talk to the obelisk.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public override bool WhisperReceive(GameLiving source, string text)
        {
            if (!base.WhisperReceive(source, text))
                return false;

            GamePlayer player = source as GamePlayer;
            if (player == null)
                return false;

            //Test that the player whispered a valid destination
            bool proceed = false;
            switch (text)
            {
                case "Kobold Undercity":
                case "Inconnu Crypt":
                case "Shar Labyrinth":
                case "Burial Grounds":
                case "Abandoned Mines":
                case "Frontlines":
                case "Underground Forest":
                case "Jordheim":
                case "Roman Aqueducts":
                case "Deadlands of Annwn":
                case "Camelot":
                case "Veil Rift":
                case "Otherworld":
                case "Tir na Nog":
                    {
                        proceed = true;
                        break;
                    }
                default:
                    {
                        proceed = false;
                        break;
                    }

            }
            //Only if the player whispered a valid keyword, look for the port destination in the database.
            if (proceed)
            {
                //Find the teleport location in the database.

                var port = WorldMgr.GetTeleportLocation(source.Realm, String.Format("{0}:{1}", Type, text));
                if (port != null)
                {
                    //TODO: Check for quests
                    switch (text)
                    {
                        case "Kobold Undercity":
                            {
                                //no checks for region on the hub destination.
                                    OnDestinationPicked(player, port);
                                break;
                            }
                        case "Inconnu Crypt":
                            {
                                //no checks for region on the hub destination.
                                    OnDestinationPicked(player, port);
                                
                                break;
                            }
                        case "Shar Labyrinth":
                            {
                                //no checks for region on the hub destination.
                                    OnDestinationPicked(player, port);
                                
                                break;
                            }
                        case "Burial Grounds":
                            {
                                if (this.CurrentRegionID == 243) //Kobold Undercity Obelisk
                                {
                                    OnDestinationPicked(player, port);
                                }
                                break;
                            }
                        case "Abandoned Mines":
                            {
                                if (this.CurrentRegionID == 243|| this.CurrentRegionID == 65 || this.CurrentRegionID ==  93) //Is it a valid destination for this obelisk?
                                {
                                    OnDestinationPicked(player, port);
                                }
                                break;
                            }
                        case "Frontlines":
                            {
                                if (this.CurrentRegionID == 243 || this.CurrentRegionID == 65 || this.CurrentRegionID == 93) //Is it a valid destination for this obelisk?
                                {
                                    OnDestinationPicked(player, port);
                                }
                                break;
                            }
                        case "Underground Forest":
                            {
                                if (this.CurrentRegionID == 243 || this.CurrentRegionID == 65 || this.CurrentRegionID == 93) //Is it a valid destination for this obelisk?
                                {
                                    OnDestinationPicked(player, port);
                                }
                                break;
                            }
                        case "Jordheim":
                            {
                                if (this.CurrentRegionID == 243) //Is it a valid destination for this obelisk?
                                {
                                    OnDestinationPicked(player, port);
                                }
                                break;
                            }
                        case "Roman Aqueducts":
                            {
                                if (this.CurrentRegionID == 65) //Is it a valid destination for this obelisk?
                                {
                                    OnDestinationPicked(player, port);
                                }
                                break;
                            }
                        case "Deadlands of Annwn":
                            {
                                if (this.CurrentRegionID == 243 || this.CurrentRegionID == 65 || this.CurrentRegionID == 93) //Is it a valid destination for this obelisk?
                                {
                                    OnDestinationPicked(player, port);
                                }
                                break;
                            }
                        case "Camelot":
                            {
                                if (this.CurrentRegionID == 65) //Is it a valid destination for this obelisk?
                                {
                                    OnDestinationPicked(player, port);
                                }
                                break;
                            }
                        case "Veil Rift":
                            {
                                if (this.CurrentRegionID == 93) //Is it a valid destination for this obelisk?
                                {
                                    OnDestinationPicked(player, port);
                                }
                                break;
                            }
                        case "Otherworld":
                            {
                                if (this.CurrentRegionID == 93) //Is it a valid destination for this obelisk?
                                {
                                    OnDestinationPicked(player, port);
                                }
                                break;
                            }
                        case "Tir na Nog":
                            {
                                if (this.CurrentRegionID == 93) //Is it a valid destination for this obelisk?
                                {
                                    OnDestinationPicked(player, port);
                                }
                                break;
                            }
                        default:
                            {
                                break;
                            }

                    }
                    return false;
                }
            }

            return true;
        }
        
    }
}
/*
 * Save this as Teleport.SQL and import it into your DB (if the file didn't already come with this script)


INSERT INTO `teleport` VALUES ('00b4dbee-20eb-44a9-ad7e-4b2aed2c0228', 'Roman Aqueducts', '1', '63', '32252', '32541', '16084', '27', 'Obelisk');
INSERT INTO `teleport` VALUES ('8828fc72-e3a1-48ea-8cfb-3ae62f4f2408', 'Frontlines', '1', '109', '28812', '21939', '15864', '3713', 'Obelisk');
INSERT INTO `teleport` VALUES ('94f8ed4b-d251-4041-8942-6b544e57b6f9', 'Inconnu Crypt', '1', '65', '30995', '36151', '16149', '3041', 'Obelisk');
INSERT INTO `teleport` VALUES ('26bcb303-45c3-4817-b8ed-694b4673aadd', 'Underground Forest', '1', '66', '18786', '21529', '16187', '851', 'Obelisk');
INSERT INTO `teleport` VALUES ('c49d3479-e9dd-4b54-8a91-21bbc63c4f45', 'Otherworld', '1', '196', '36424', '38303', '15887', '3427', 'Obelisk');
INSERT INTO `teleport` VALUES ('a9f307b2-7add-469f-9da3-538bab9abe11', 'Abandoned Mines', '1', '227', '31476', '31955', '16025', '3556', 'Obelisk');
INSERT INTO `teleport` VALUES ('e2cd258b-2983-41e6-8604-4dc622dd56d1', 'Deadlands of Annwn', '1', '67', '20670', '37086', '15863', '1005', 'Obelisk');
INSERT INTO `teleport` VALUES ('06e7876c-2c3c-488b-bbaf-4910445dab3a', 'Kobold Undercity', '2', '243', '30498', '30212', '16239', '371', 'Obelisk');
INSERT INTO `teleport` VALUES ('0d87e2a6-0ae7-4cd5-8c0e-5124da3c0317', 'Frontlines', '2', '148', '35257', '28248', '16309', '2673', 'Obelisk');
INSERT INTO `teleport` VALUES ('a666fdb9-1f0e-4cd4-a2d1-01c3709b181b', 'Deadlands of Annwn', '2', '162', '28829', '12268', '15884', '14', 'Obelisk');
INSERT INTO `teleport` VALUES ('3661bfe1-fe6c-4f82-94c5-ba83db3c34b0', 'Underground Forest', '2', '58', '24340', '22099', '16157', '70', 'Obelisk');
INSERT INTO `teleport` VALUES ('d72dbefc-a5c4-4ffe-8bc5-ef5fd63d6eba', 'Otherworld', '2', '195', '33899', '17508', '16079', '1061', 'Obelisk');
INSERT INTO `teleport` VALUES ('35fa4590-87a0-46b1-8ffa-43faf2dc2fc5', 'Abandoned Mines', '2', '226', '31525', '32426', '16019', '1834', 'Obelisk');
INSERT INTO `teleport` VALUES ('b37e2032-5a2e-41c3-a3fc-c6e40ccfb2ea', 'Burial Grounds', '2', '229', '36422', '40630', '16779', '1358', 'Obelisk');
INSERT INTO `teleport` VALUES ('10b84deb-4e33-42ef-91d8-aac04c58a953', 'Shar Labyrinth', '3', '93', '24274', '27755', '17549', '3829', 'Obelisk');
INSERT INTO `teleport` VALUES ('9ff6f0c6-6f4b-4eb3-8e47-9e503dfbb8d8', 'Veil Rift', '3', '92', '33864', '29929', '11787', '1679', 'Obelisk');
INSERT INTO `teleport` VALUES ('652f691b-1c06-4acd-9c13-c0f5e77f5083', 'Frontlines', '3', '95', '30240', '41029', '15867', '2363', 'Obelisk');
INSERT INTO `teleport` VALUES ('f117ecae-f47f-45ba-9488-f9a522a44eb2', 'Underground Forest', '3', '96', '19054', '20762', '16176', '1624', 'Obelisk');
INSERT INTO `teleport` VALUES ('21e80f87-8b3c-46c5-8e65-781df4364afd', 'Deadlands of Annwn', '3', '97', '29114', '12525', '15887', '540', 'Obelisk');
INSERT INTO `teleport` VALUES ('66cbaa62-6fc9-4ced-b67f-4ccbbd0dc513', 'Otherworld', '3', '197', '28197', '38806', '16548', '1951', 'Obelisk');
INSERT INTO `teleport` VALUES ('61ed9ce9-3db0-4748-b120-625c65d2cf4d', 'Abandoned Mines', '3', '228', '32356', '32395', '16023', '2419', 'Obelisk');



 * Save this one as mob.SQL and import it into your DB also (if the file didn't already come with this script)


INSERT INTO `mob` VALUES ('00b4dbee-20eb-44a9-ad7e-4b2aed2c0228', 'DOL.GS.Obelisk', 'Obelisk', '', '32256', '32647', '16084', '0', '1752', '63', '1878', '49', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', null, null, '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);
INSERT INTO `mob` VALUES ('06e7876c-2c3c-488b-bbaf-4910445dab3a', 'DOL.GS.Obelisk', 'Obelisk', '', '30453', '30285', '16254', '0', '284', '243', '1823', '49', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', '', '', '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);
INSERT INTO `mob` VALUES ('0d87e2a6-0ae7-4cd5-8c0e-5124da3c0317', 'DOL.GS.Obelisk', 'Obelisk', '', '35391', '28147', '16372', '0', '603', '148', '1823', '49', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', null, null, '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);
INSERT INTO `mob` VALUES ('10b84deb-4e33-42ef-91d8-aac04c58a953', 'DOL.GS.Obelisk', 'Obelisk', '', '24314', '27846', '17549', '0', '2810', '93', '1863', '49', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', null, null, '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);
INSERT INTO `mob` VALUES ('21e80f87-8b3c-46c5-8e65-781df4364afd', 'DOL.GS.Obelisk', 'Obelisk', '', '29039', '12593', '15892', '0', '2252', '97', '1863', '50', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', null, null, '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);
INSERT INTO `mob` VALUES ('26bcb303-45c3-4817-b8ed-694b4673aadd', 'DOL.GS.Obelisk', 'Obelisk', '', '18677', '21565', '16188', '0', '2889', '66', '1878', '52', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', null, null, '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);
INSERT INTO `mob` VALUES ('35fa4590-87a0-46b1-8ffa-43faf2dc2fc5', 'DOL.GS.Obelisk', 'Obelisk', '', '31497', '32344', '16011', '0', '3857', '226', '1823', '49', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', null, null, '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);
INSERT INTO `mob` VALUES ('3661bfe1-fe6c-4f82-94c5-ba83db3c34b0', 'DOL.GS.Obelisk', 'Obelisk', '', '24330', '22200', '16152', '0', '2161', '58', '1823', '50', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', null, null, '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);
INSERT INTO `mob` VALUES ('61ed9ce9-3db0-4748-b120-625c65d2cf4d', 'DOL.GS.Obelisk', 'Obelisk', '', '32424', '32295', '16019', '0', '1410', '228', '1863', '48', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', null, null, '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);
INSERT INTO `mob` VALUES ('652f691b-1c06-4acd-9c13-c0f5e77f5083', 'DOL.GS.Obelisk', 'Obelisk', '', '30296', '40918', '15864', '0', '1285', '95', '1863', '48', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', null, null, '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);
INSERT INTO `mob` VALUES ('66cbaa62-6fc9-4ced-b67f-4ccbbd0dc513', 'DOL.GS.Obelisk', 'Obelisk', '', '28181', '38697', '16550', '0', '944', '197', '1863', '49', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', null, null, '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);
INSERT INTO `mob` VALUES ('6f663214-2402-430e-a6e7-b4683635a059', 'DOL.GS.Obelisk', 'Obelisk', '', '33863', '33849', '16000', '0', '1536', '59', '1878', '49', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', null, null, '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);
INSERT INTO `mob` VALUES ('8828fc72-e3a1-48ea-8cfb-3ae62f4f2408', 'DOL.GS.Obelisk', 'Obelisk', '', '28871', '22016', '15864', '0', '2264', '109', '1878', '50', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', null, null, '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);
INSERT INTO `mob` VALUES ('94f8ed4b-d251-4041-8942-6b544e57b6f9', 'DOL.GS.Obelisk', 'Obelisk', '', '31118', '36141', '16149', '0', '978', '65', '1878', '52', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', null, null, '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);
INSERT INTO `mob` VALUES ('990a7080-413b-4460-b875-e94124b0879d', 'DOL.GS.Obelisk', 'Obelisk', '', '32388', '31019', '16464', '0', '45', '68', '1878', '51', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', null, null, '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);
INSERT INTO `mob` VALUES ('9ff6f0c6-6f4b-4eb3-8e47-9e503dfbb8d8', 'DOL.GS.Obelisk', 'Obelisk', '', '33795', '29824', '11793', '0', '2002', '92', '1863', '49', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', null, null, '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);
INSERT INTO `mob` VALUES ('a61a315b-56e0-4198-a818-7cf229789bbf', 'DOL.GS.Obelisk', 'Obelisk', '', '30076', '33802', '16000', '0', '2628', '99', '1863', '51', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', null, null, '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);
INSERT INTO `mob` VALUES ('a666fdb9-1f0e-4cd4-a2d1-01c3709b181b', 'DOL.GS.Obelisk', 'Obelisk', '', '28834', '12362', '15884', '0', '3948', '162', '1823', '50', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', null, null, '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);
INSERT INTO `mob` VALUES ('a9f307b2-7add-469f-9da3-538bab9abe11', 'DOL.GS.Obelisk', 'Obelisk', '', '31575', '32041', '16028', '0', '1490', '227', '1878', '52', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', null, null, '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);
INSERT INTO `mob` VALUES ('b37e2032-5a2e-41c3-a3fc-c6e40ccfb2ea', 'DOL.GS.Obelisk', 'Obelisk', '', '36353', '40595', '16770', '0', '1285', '229', '1823', '50', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', null, null, '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);
INSERT INTO `mob` VALUES ('bcf37c9f-3914-4b2c-869f-e758214d17d5', 'DOL.GS.Obelisk', 'Obelisk', '', '38773', '28435', '20940', '0', '2764', '94', '1863', '48', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', null, null, '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);
INSERT INTO `mob` VALUES ('c49d3479-e9dd-4b54-8a91-21bbc63c4f45', 'DOL.GS.Obelisk', 'Obelisk', '', '36566', '38389', '15889', '0', '1378', '196', '1878', '52', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', null, null, '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);
INSERT INTO `mob` VALUES ('d4b312be-d60f-464f-afd0-3f0aa3617892', 'DOL.GS.Obelisk', 'Obelisk', '', '33907', '33869', '16000', '0', '1695', '189', '1823', '52', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', null, null, '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);
INSERT INTO `mob` VALUES ('d7a9a0d6-372b-4163-a42d-0e16d8068615', 'DOL.GS.Obelisk', 'Obelisk', '', '38561', '31350', '15996', '0', '2252', '149', '1823', '52', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', null, null, '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);
INSERT INTO `mob` VALUES ('e2cd258b-2983-41e6-8604-4dc622dd56d1', 'DOL.GS.Obelisk', 'Obelisk', '', '20536', '37086', '15856', '0', '1035', '67', '1878', '50', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', null, null, '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);
INSERT INTO `mob` VALUES ('f117ecae-f47f-45ba-9488-f9a522a44eb2', 'DOL.GS.Obelisk', 'Obelisk', '', '18970', '20653', '16179', '0', '3310', '96', '1863', '52', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', null, null, '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);
INSERT INTO `mob` VALUES ('d72dbefc-a5c4-4ffe-8bc5-ef5fd63d6eba', 'DOL.GS.Obelisk', 'Obelisk', '', '33750', '17502', '16075', '0', '3097', '195', '1823', '49', '30', '30', '30', '30', '30', '30', '30', '30', '0', '0', null, null, '-1', '20', '0', '0', '2', '0', '0', '0', '-1', null, null, '0', '', '0', null, null);


*/