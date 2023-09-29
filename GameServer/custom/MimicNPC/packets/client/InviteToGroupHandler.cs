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

//using DOL.GS.PacketHandler;
//using log4net;
//using System.Reflection;

//namespace DOL.GS.Scripts
//{
//    [PacketHandlerAttribute(PacketHandlerType.TCP, eClientPackets.InviteToGroup, "Handle Invite to Group Request.", eClientStatus.PlayerInGame)]
//    public class InviteToGroupHandler : IPacketHandler
//    {
//        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

//        public void HandlePacket(GameClient client, GSPacketIn packet)
//        {
//            log.Info("Using New Handler!");
//            log.Info("client.Player.Name: " + client.Player.Name);
//            new HandleGroupInviteAction(client.Player).Start(1);
//        }

//        /// <summary>
//        /// Handles group invlite actions
//        /// </summary>
//        protected class HandleGroupInviteAction : RegionAction
//        {
//            private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
//            /// <summary>
//            /// constructs a new HandleGroupInviteAction
//            /// </summary>
//            /// <param name="actionSource">The action source</param>
//            public HandleGroupInviteAction(GamePlayer actionSource) : base(actionSource)
//            {
//            }

//            /// <summary>
//            /// Called on every timer tick
//            /// </summary>
//            protected override void OnTick()
//            {
//                log.Info("OnTick");

//                var player = (GamePlayer)m_actionSource;

//                if (player.TargetObject == null || player.TargetObject == player)
//                {
//                    ChatUtil.SendSystemMessage(player, "You have not selected a valid player as your target.");
//                    return;
//                }

//                if (!(player.TargetObject is GamePlayer) && !(player.TargetObject is MimicNPC))
//                {
//                    ChatUtil.SendSystemMessage(player, "You have not selected a valid player as your target.");
//                    return;
//                }

//                if (player.Group != null && player.Group.Leader != player)
//                {
//                    ChatUtil.SendSystemMessage(player, "You are not the leader of your group.");
//                    return;
//                }

//                if (player.Group != null && player.Group.MemberCount >= ServerProperties.Properties.GROUP_MAX_MEMBER)
//                {
//                    ChatUtil.SendSystemMessage(player, "The group is full.");
//                    return;
//                }

//                var targetGamePlayer = (GamePlayer)player.TargetObject;

//                if (targetGamePlayer != null)
//                {
//                    if (!GameServer.ServerRules.IsAllowedToGroup(player, targetGamePlayer, false))
//                        return;
//                }

//                var targetGameLiving = (GameLiving)player.TargetObject;

//                if (targetGameLiving.Group != null)
//                {
//                    ChatUtil.SendSystemMessage(player, "The player is still in a group.");
//                    return;
//                }

//                ChatUtil.SendSystemMessage(player, "You have invited " + targetGameLiving.Name + " to join your group.");

//                player.Group.AddMember(targetGameLiving);
//            }
//        }
//    }
//}