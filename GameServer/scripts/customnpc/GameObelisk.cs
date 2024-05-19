/*
-------------------------------------------------------
Author: Fulmine                            
Dialog: The dialogues are taken from official server.
-------------------------------------------------------
*/
using System;
using DOL.GS.PacketHandler;
using DOL.GS.Quests;
using DOL.GS;

namespace Myrddin.Labyrinth.NPC
{
    public class GameObelisk : GameNPC
    {
        public string ObeliskName = "Obelisk of Nurizane";

        public String ObeliskInternalName { get; set; }

        public Type ObeliskCredit { get; set; }

        public Boolean isNurizaneCrossroadsObelisk { get; set; }

        public override bool AddToWorld()
        {
            this.Model = 2256;
            this.Flags |= eFlags.DONTSHOWNAME;
            this.Name = ObeliskName;
            this.Size = 51;
            return base.AddToWorld();
        }
        public override void SaveIntoDatabase()
        {

        }

        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player) || player == null)
                return false;
            
            if (isNurizaneCrossroadsObelisk)
            {
                player.Out.SendMessage("The Obelisk of Nurizane will allow you to traverse the nexus to any other Obelisk in the Labyrinth by activating the runes in the correct order.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                player.Out.SendMessage("Choose a section of the Labyrinth: [Collapsed], [Flooded], or [Clockwork]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                if (player.Realm == eRealm.Albion)
                    player.Out.SendMessage("You may also leave the Labyrinth entirely for [Castle Sauvage], the [Tower of Korazh] on Agramon, or the [Decayed Lands] on Agramon.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                if (player.Realm == eRealm.Midgard)
                    player.Out.SendMessage("You may also leave the Labyrinth entirely for [Svasud Faste], the [Tower of Deifrang] on Agramon, or the [Decayed Lands] on Agramon.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                if (player.Realm == eRealm.Hibernia)
                    player.Out.SendMessage("You may also leave the Labyrinth entirely for [Druim Ligen], the [Tower of Graoch] on Agramon, or the [Decayed Lands] on Agramon.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
            }
            else
            {
                if (this.ObeliskCredit == null)
                {
                    player.Out.SendMessage("Obelisk of Nurizane - " + LabyrinthObeliskHelper.GetObeliskRealName(this.ObeliskInternalName, player), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    player.Out.SendMessage("The Obelisk of Nurizane will allow you to traverse the nexus to any other Obelisk in the Labyrinth by activating the runes in the correct order.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    player.Out.SendMessage("The runes for [Nurizane's Crossroads] and [Nethuni's Sanctuary] are written at the top. If you know the runes for other destinations, you may chosse a section of the Labyrinth: [Collapsed], [Flooded], or [Clockwork]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    player.Out.SendMessage("Choose a section of the Labyrinth: [Collapsed], [Flooded], or [Clockwork]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    if (player.Realm == eRealm.Albion)
                        player.Out.SendMessage("You may also leave the Labyrinth entirely for [Castle Sauvage], the [Tower of Korazh] on Agramon, or the [Decayed Lands] on Agramon.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    if (player.Realm == eRealm.Midgard)
                        player.Out.SendMessage("You may also leave the Labyrinth entirely for [Svasud Faste], the [Tower of Deifrang] on Agramon, or the [Decayed Lands] on Agramon.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    if (player.Realm == eRealm.Hibernia)
                        player.Out.SendMessage("You may also leave the Labyrinth entirely for [Druim Ligen], the [Tower of Graoch] on Agramon, or the [Decayed Lands] on Agramon.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                }
                else
                {
                    if (LabyrinthObeliskHelper.PlayerHasObelisk(this.ObeliskInternalName, player))
                        player.Out.SendMessage("Obelisk of Nurizane - " + LabyrinthObeliskHelper.GetObeliskRealName(this.ObeliskInternalName, player) + "\n\nWould you like to Return to [Nurizane's Crossroads]?", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    else
                    {
                        AbstractQuest obeliskQuest = LabyrinthObeliskHelper.GetAbstractQuestForPlayer(this.ObeliskInternalName, player);
                        if (obeliskQuest != null)
                        {
                            obeliskQuest.FinishQuest();
                            player.AddFinishedQuest(obeliskQuest);
                        }
                        player.Out.SendMessage("You have completed the Lab: Obelisk - " + LabyrinthObeliskHelper.GetObeliskRealName(this.ObeliskInternalName, player) + " quest!", eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);
                        player.Out.SendSpellEffectAnimation(this, this, 1657, 0, false, 1);
                        player.Out.SendMessage("Obelisk of Nurizane - " + LabyrinthObeliskHelper.GetObeliskRealName(this.ObeliskInternalName, player) + "\n\nYou take note of the series of runes at the top of the Obelisk You may now use these runes to port this obelisk for Nurizane's Crossroads\n\nWould you like to Return to [Nurizane's Crossroads]?", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }
                }
            }
            return false;
        }

        public override bool WhisperReceive(GameLiving source, string text)
        {
            if (!base.WhisperReceive(source, text) || !(source is GamePlayer))
                return false;
            GamePlayer player = source as GamePlayer;

            if (text == "Tower of Korazh")
                TeleportByRealm(player, "Agramon");
            else if (text == "Tower of Deifrang")
                TeleportByRealm(player, "Agramon");
            else if (text == "Tower of Graoch")
                TeleportByRealm(player, "Agramon");
            else if (text == "Nethuni's Sanctuary")
                TeleportByRealm(player, "SafeZone");
            else if (text == "Castle Sauvage")
                TeleportByRealm(player, "BorderKeep");
            else if (text == "Svasud Faste")
                TeleportByRealm(player, "BorderKeep");
            else if (text == "Druim Ligen")
                TeleportByRealm(player, "BorderKeep");
            else if (text == "Decayed Lands")
                player.MoveTo(163, 524006, 464201, 9424, 3101);
            else if (text == "Collapsed")
                player.Out.SendMessage("Choose a destination: [Agramon's Lair], [Discovery's Crossing], the [Forge of Pyrkagia], the [Shrine of Laresh (West)], the [Shrine of Teragani (North)], or the [Temple of Februstos (North)]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
            else if (text == "Flooded")
                player.Out.SendMessage("Choose a destination: [Anapaysi's Crossing], the [Catacombs of Februstos], [Diabasi's Junction], the [Dracolich Den], [Dynami's Crossing], the [Hall of Feretro], the [Hall of Thanatoy], The [Passage of Ygros], the [Path of Zoi], [Plimmyra's Landing], the [Shrine of Teragani (South)], the [Shrine of Vartigeth (North)], the [Shrine of Vartiget (South)], the [Shrine of Vartiget (West)], the [Shrine of Nethuni (North)], the [Temple of Laresh], the [Temple of Perizor (West)], or [Ygrasia's Corssing]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
            else if (text == "Clockwork")
                player.Out.SendMessage("Choose a destination: the [Construct Assembly Room], [Efeyresi's Junction], [Ergaleio's Path], the [Great Forge of Thivek], the [Hall ofd Allagi], the [Hall of Dimioyrgia], [Kainotomia's Crossing], the [Path of Roloi], the [Shrine of Laresh (East)], the [Shrine of Nethuni (South)], the [Shrine of Tegashirg], the [Temple of Februstos (South)], the [Temple of Perizor (East)], or [Trela's Corssing]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
            else if (text == "Nurizane's Crossroads" && !this.isNurizaneCrossroadsObelisk )
            {
                player.MoveTo(LabyrinthObeliskHelper.GetRandomCrossroadsLocation());
            }
            else
            {
                if (LabyrinthObeliskHelper.PlayerHasObelisk(LabyrinthObeliskHelper.GetObeliskInternalName(text), player))
                {
                    GameLocation worldPosition = LabyrinthObeliskHelper.GetObeliskGameLocation(LabyrinthObeliskHelper.GetObeliskInternalName(text));
                    if (worldPosition != null)
                        player.MoveTo(worldPosition);
                }
                else
                    player.Out.SendMessage("You do not yet know the rune activation order requierd to access that portion of Nurziane's nexus. You will need to find the Obelisk there first and memoriez its sequance of runes", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
            }
            return true;
        }
        public void TeleportByRealm(GamePlayer player, string category)
        {
            if (category == "Agramon") // Teleport to Tower on Agramon
            {
                if (player.Realm == eRealm.Albion)
                    player.MoveTo(163, 546770, 480746, 9270, 3358);
                else if (player.Realm == eRealm.Midgard)
                    player.MoveTo(163, 525501, 439641, 9254, 1556);
                else if (player.Realm == eRealm.Hibernia)
                    player.MoveTo(163, 505855, 578187, 9145, 3982);
            }
            else if (category == "BorderKeep") // Teleport to Border Keep for zone 163 (New Frontier)
            {
                if (player.Realm == eRealm.Albion)
                    player.MoveTo(163, 654908, 617239, 9560, 1929);
                else if (player.Realm == eRealm.Midgard)
                    player.MoveTo(163, 651645, 312820, 9432, 1043);
                else if (player.Realm == eRealm.Hibernia)
                    player.MoveTo(163, 397429, 618251, 9856, 1912);
            }
            else if (category == "SafeZone") // Teleport to Safe Zone in Labyrinth
            {
                if (player.Realm == eRealm.Albion)
                    player.MoveTo(245, 70497, 70674, 339, 2039);
                else if (player.Realm == eRealm.Midgard)
                    player.MoveTo(245, 11594, 70568, 339, 1023);
                else if (player.Realm == eRealm.Hibernia)
                    player.MoveTo(245, 11679, 11681, 339, 1021);
            }
        }
    }

}
