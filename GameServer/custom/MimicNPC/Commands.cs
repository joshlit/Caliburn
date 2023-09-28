using DOL.AI.Brain;
using DOL.GS.API;
using DOL.GS.Commands;
using DOL.GS.PacketHandler;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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

                    if (level < 1 || level > 50)
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
                    eMimicClasses mclass = (eMimicClasses)Enum.Parse(typeof(eMimicClasses), args[1]);

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
                log.Info("args[1]: " + args[1]);

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
                                eMimicClasses mimicClass = (eMimicClasses)Util.Random(12, 22);

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
                                eMimicClasses mimicClass = (eMimicClasses)Util.Random(23, 33);

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

    [CmdAttribute(
       "&equip",
       ePrivLevel.Player,
       "/equip - Get a set of armor and weapons. WeaponSpec, ArmorType")]
    public class EquipCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length > 0)
            {
                string weapon = args[1];
                string armor = args[2];

                MimicEquipment.SetMeleeWeapon(client.Player, weapon);
                MimicEquipment.SetArmor(client.Player, MimicEquipment.GetObjectType(armor));
                MimicEquipment.SetJewelry(client.Player);
            }
        }
    }

    [CmdAttribute(
       "&effects",
       ePrivLevel.Player,
       "/effects - Get a list of current effects.")]
    public class EffectsCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void OnCommand(GameClient client, string[] args)
        {
            //client.Player.Out.SendMessage(msg, eChatType.CT_System, eChatLoc.CL_PopupWindow);
        }
    }

    [CmdAttribute(
      "&mimicbattle",
      ePrivLevel.Player,
      "/mimicbattle - Call mimics to Thid")]
    public class MimicBattleCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void OnCommand(GameClient client, string[] args)
        {
            MimicBattlegrounds.Start(client.Player);
        }
    }

    [CmdAttribute(
      "&mbstats",
      ePrivLevel.Player,
      "/mbstats - Get stats on Thid.")]
    public class MimicBattleStatsCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void OnCommand(GameClient client, string[] args)
        {
            if (MimicBattlegrounds.Running)
            {
                MimicBattlegrounds.MimicBattlegroundStats(client.Player);
            }
        }
    }
}
