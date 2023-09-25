using DOL.GS.Commands;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DOL.GS.Scripts
{
    //[CmdAttribute(
    //    "&pull",
    //    ePrivLevel.Admin,
    //    "/mpull - Pull a player to you.")]
    //public class PullCommandHandler : AbstractCommandHandler, ICommandHandler
    //{
    //    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    //    public void OnCommand(GameClient client, string[] args)
    //    {
    //        foreach (GameClient playerClient in WorldMgr.GetAllPlayingClients())
    //        {
    //            GamePlayer player = client.Player;

    //            playerClient.Player.MoveTo(player.CurrentRegionID, player.X, player.Y, player.Z, player.Heading);
    //        }
    //    }
    //}

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
                byte level = 1;

                if (args.Length > 2)
                {
                    level = byte.Parse(args[2]);

                    if (level < 1 || level > 255)
                        level = 1;

                    log.Info("Level: " + level);
                }
                else
                {
                    level = client.Player.Level;
                }

                Point3D position = null;

                if (client.Player.GroundTarget != null)
                {
                    Point2D playerPos = new Point2D(client.Player.X, client.Player.Y);

                    if (client.Player.GroundTarget.GetDistance(playerPos) < 3000)
                        position = new Point3D(client.Player.GroundTarget);
                }

                if (position != null)
                {
                    eMimicClasses mclass = (eMimicClasses)Enum.Parse(typeof(eMimicClasses), args[1]);

                    MimicManager.AddMimicToWorld(mclass, client.Player, level, position);
                }
            }
        }
    }

    [CmdAttribute(
       "&mimicgroup",
       ePrivLevel.Player,
       "/mimicgroup - To summon a group of mimics from a realm")]
    public class SummonMimicGroupCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length > 0)
            {
                args[1] = args[1].ToLower();

                byte level = 1;

                if (args.Length > 2)
                {
                    level = byte.Parse(args[2]);

                    if (level < 1 || level > 255)
                        level = 1;
                }
                else
                {
                    level = client.Player.Level;
                }

                Point3D position = null;

                if (client.Player.GroundTarget != null)
                {
                    Point2D playerPos = new Point2D(client.Player.X, client.Player.Y);

                    if (client.Player.GroundTarget.GetDistance(playerPos) < 3000)
                        position = new Point3D(client.Player.GroundTarget);
                }

                if (position != null)
                {
                    List<GameLiving> groupMembers = new List<GameLiving>();
                    MimicNPC mimic = null;

                    switch (args[1])
                    {
                        case "albion":
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                eMimicClasses mimicClass = (eMimicClasses)Util.Random(11);

                                int randomX = Util.Random(-100, 100);
                                int randomY = Util.Random(-100, 100);

                                position.X += randomX;
                                position.Y += randomY;

                                mimic = MimicManager.AddMimicToWorld(mimicClass, client.Player, level, position, true);

                                if (mimic != null)
                                    groupMembers.Add(mimic);
                            }

                            break;
                        }

                        case "hibernia":
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                eMimicClasses mimicClass = (eMimicClasses)Util.Random(12, 22);

                                int randomX = Util.Random(-100, 100);
                                int randomY = Util.Random(-100, 100);

                                position.X += randomX;
                                position.Y += randomY;

                                mimic = MimicManager.AddMimicToWorld(mimicClass, client.Player, level, position, true);

                                if (mimic != null)
                                    groupMembers.Add(mimic);
                            }

                            break;
                        }

                        case "midgard":
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                eMimicClasses mimicClass = (eMimicClasses)Util.Random(23, 33);

                                int randomX = Util.Random(-100, 100);
                                int randomY = Util.Random(-100, 100);

                                position.X += randomX;
                                position.Y += randomY;

                                mimic = MimicManager.AddMimicToWorld(mimicClass, client.Player, level, position, true);

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
                                groupMembers[0].Group.AddMember(living);
                        }
                    }
                }
            }
        }
    }

    [CmdAttribute(
       "&mimiccombat",
       ePrivLevel.Player,
       "/mimiccombat - Set universal combat prevention on/off")]
    public class MimicCombatPreventCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length > 0)
            {
                string toLower = args[1].ToLower();

                switch(args[1])
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
}
