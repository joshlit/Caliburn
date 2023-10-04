using DOL.AI.Brain;
using DOL.GS.API;
using DOL.GS.Behaviour.Actions;
using DOL.GS.Commands;
using DOL.GS.PacketHandler;
using log4net;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DOL.GS.Scripts
{
    [CmdAttribute(
    "&mimic",
    ePrivLevel.Player,
    "/mimic - To summon a realm mate")]
    public class SummonCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length > 0)
            {
                byte level;

                if (args.Length > 2)
                {
                    level = byte.Parse(args[2]);

                    if (level < 1 || level > 50)
                        level = 1;
                }
                else
                    level = client.Player.Level;

                Point3D position = new Point3D(client.Player.X, client.Player.Y, client.Player.Z);

                if (client.Player.GroundTarget != null)
                {
                    Point2D playerPos = new Point2D(client.Player.X, client.Player.Y);

                    if (client.Player.GroundTarget.GetDistance(playerPos) < 5000)
                        position = new Point3D(client.Player.GroundTarget);
                }

                if (position != null)
                {
                    string capitalize = char.ToUpper(args[1][0]) + args[1].Substring(1);
                    eMimicClasses mclass = (eMimicClasses)Enum.Parse(typeof(eMimicClasses), capitalize);

                    MimicNPC mimic = MimicManager.GetMimic(mclass, level);
                    MimicManager.AddMimicToWorld(mimic, position, client.Player.CurrentRegionID);
                }
            }
        }
    }

    [CmdAttribute(
       "&mimicgroup",
       ePrivLevel.Player,
       "/mimicgroup - To summon a group of mimics from a realm. Args: realm, amount, level")]
    public class SummonMimicGroupCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length >= 2)
            {
                args[1] = args[1].ToLower();

                byte groupSize = 8;
                if (args.Length >= 3)
                {
                    groupSize = byte.Parse(args[2]);

                    if (groupSize < 1 || groupSize > 8)
                        groupSize = 8;
                }

                byte level;
                if (args.Length >= 4)
                {
                    level = byte.Parse(args[3]);

                    if (level < 1 || level > 50)
                        level = 1;
                }
                else
                    level = client.Player.Level;

                Point3D position = new Point3D(client.Player.X, client.Player.Y, client.Player.Z);

                if (client.Player.GroundTarget != null)
                {
                    Point2D playerPos = new Point2D(client.Player.X, client.Player.Y);

                    if (client.Player.GroundTarget.GetDistance(playerPos) < 5000)
                        position = new Point3D(client.Player.GroundTarget);
                }

                if (position != null)
                {
                    List<GameLiving> groupMembers = new List<GameLiving>();
                    MimicNPC mimic;

                    switch (args[1])
                    {
                        case "albion":
                        {
                            for (int i = 0; i < groupSize; i++)
                            {
                                int randomX = Util.Random(-100, 100);
                                int randomY = Util.Random(-100, 100);

                                position.X += randomX;
                                position.Y += randomY;

                                mimic = MimicManager.GetMimic(eMimicClasses.Random, level, eRealm.Albion);
                                MimicManager.AddMimicToWorld(mimic, position, client.Player.CurrentRegionID);

                                if (mimic != null)
                                    groupMembers.Add(mimic);
                            }

                            break;
                        }

                        case "hibernia":
                        {
                            for (int i = 0; i < groupSize; i++)
                            {
                                int randomX = Util.Random(-100, 100);
                                int randomY = Util.Random(-100, 100);

                                position.X += randomX;
                                position.Y += randomY;

                                mimic = MimicManager.GetMimic(eMimicClasses.Random, level, eRealm.Hibernia);
                                MimicManager.AddMimicToWorld(mimic, position, client.Player.CurrentRegionID);

                                if (mimic != null)
                                    groupMembers.Add(mimic);
                            }

                            break;
                        }

                        case "midgard":
                        {
                            for (int i = 0; i < groupSize; i++)
                            {
                                int randomX = Util.Random(-100, 100);
                                int randomY = Util.Random(-100, 100);

                                position.X += randomX;
                                position.Y += randomY;

                                mimic = MimicManager.GetMimic(eMimicClasses.Random, level, eRealm.Midgard);
                                MimicManager.AddMimicToWorld(mimic, position, client.Player.CurrentRegionID);

                                if (mimic != null)
                                    groupMembers.Add(mimic);
                            }

                            break;
                        }

                        default: break;
                    }

                    if (groupMembers.Count > 0)
                    {
                        if (groupMembers[0].Group == null)
                        {
                            groupMembers[0].Group = new Group(groupMembers[0]);
                            groupMembers[0].Group.AddMember(groupMembers[0]);
                        }

                        foreach (GameLiving living in groupMembers)
                        {
                            if (living.Group == null)
                            {
                                groupMembers[0].Group.AddMember(living);

                                MimicBrain brain = ((MimicNPC)living).Brain as MimicBrain;
                                brain.PvPMode = true;
                                brain.Roam = true;
                                brain.FSM.SetCurrentState(eFSMStateType.WAKING_UP);
                            }
                        }
                    }
                }
            }
        }
    }

    [CmdAttribute(
       "&mc",
       ePrivLevel.Player,
       "/mc - Set universal combat prevention on/off")]
    public class MimicCombatPreventCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length > 0)
            {
                string toLower = args[1].ToLower();

                switch (args[1])
                {
                    case "on":
                    MimicManager.SetPreventCombat(true);
                    break;

                    case "off":
                    MimicManager.SetPreventCombat(false);
                    break;
                }
            }
        }
    }

    [CmdAttribute(
      "&mimicbattle",
      ePrivLevel.Player,
      "/mimicbattle - Call mimics to Thid. You must be in Thid atm for the command.")]
    public class MimicBattleCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            if (!MimicBattlegrounds.Running)
                MimicBattlegrounds.Start(client.Player);
        }
    }

    [CmdAttribute(
      "&mbstats",
      ePrivLevel.Player,
      "/mbstats - Get stats on Thid.")]
    public class MimicBattleStatsCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            if (MimicBattlegrounds.Running)
                MimicBattlegrounds.MimicBattlegroundStats(client.Player);
        }
    }

    [CmdAttribute(
        "&pull",
        ePrivLevel.Admin,
        "/mpull - Pull all players to you.")]
    public class PullCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void OnCommand(GameClient client, string[] args)
        {
            foreach (GamePlayer player in ClientService.GetPlayers())
            {
                player.MoveTo(client.Player.CurrentRegionID, client.Player.X, client.Player.Y, client.Player.Z, client.Player.Heading);
            }
        }
    }

    [CmdAttribute(
        "&mlfg",
        ePrivLevel.Player,
        "/mlfg - Get a list of Mimics that are looking for a group.")]
    public class LocCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void OnCommand(GameClient client, string[] args)
        {
            GamePlayer player = client.Player;

            if (player == null)
                return;

            var entries = MimicLFGManager.GetLFG(player.Realm, player.Level);
            string message = string.Empty;

            if (entries.Any())
            {
                if (args.Length < 2)
                {
                    int index = 1;
                    foreach (var entry in entries)
                    {
                        message += index++.ToString() + " " + entry.Mimic.Name + " " + entry.Mimic.CharacterClass.Name + " " + entry.Mimic.Level + "\n";
                    }
                }
                else
                {
                    int index = int.Parse(args[1]) - 1;

                    if (index < 0 || index > entries.Count - 1)
                        message = "Invalid input.";
                    else
                    {
                        MimicNPC mimic = entries[index].Mimic;

                        MimicManager.AddMimicToWorld(mimic, new Point3D(player.X, player.Y, player.Z), player.CurrentRegionID);

                        if (player.Group == null)
                        {
                            player.Group = new Group(player);
                            player.Group.AddMember(player);
                        }

                        player.Group.AddMember(mimic);

                        MimicLFGManager.Remove(player.Realm, mimic);
                    }
                }
            }
            else
                message = "No mimics available.";

            player.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_PopupWindow);

        }
    }
}
