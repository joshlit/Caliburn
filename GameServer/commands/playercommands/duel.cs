using DOL.GS.PacketHandler;
using DOL.GS.Scripts;
using DOL.Language;

namespace DOL.GS.Commands
{
    [CmdAttribute(
         "&duel",
         ePrivLevel.Player,
         "Duel another player",
         "/duel")]
    public class DuelCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        private const string DUEL_STARTER_WEAK = "DuelStarter";
        private const string CHALLENGE_TARGET_WEAK = "DuelTarget";

        public void OnCommand(GameClient client, string[] args)
        {
            if (IsSpammingCommand(client.Player, "duel"))
                return;

            switch (client.Player.CurrentRegionID)
            {
                case 10:
                case 101:
                case 201:
                {
                    DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Duel.SafeZone"), new object[] { });
                    return;
                }
            }

            WeakRef weak = null;
            GameLiving duelStarter = null;
            GameLiving duelTarget = null;

            if (args.Length > 1)
            {
                switch (args[1].ToLower())
                {
                    case "challenge":
                    {
                        GamePlayer targetPlayer = client.Player.TargetObject as GamePlayer;
                        MimicNPC targetMimic = client.Player.TargetObject as MimicNPC;

                        if ((targetPlayer == null || targetPlayer == client.Player) && (targetMimic == null))
                        {
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Duel.NeedTarget"), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                            return;
                        }

                        GameLiving targetLiving = client.Player.TargetObject as GameLiving;

                        if (!CheckDuelStart(client.Player, targetLiving))
                            return;

                        lock (client.Player.TempProperties)
                        {
                            weak = client.Player.TempProperties.GetProperty<WeakRef>(CHALLENGE_TARGET_WEAK, null);
                            if (weak != null && (duelTarget = weak.Target as GameLiving) != null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Duel.YouAlreadyChallenging", duelTarget.Name), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            weak = client.Player.TempProperties.GetProperty<WeakRef>(DUEL_STARTER_WEAK, null);
                            if (weak != null && (duelStarter = weak.Target as GameLiving) != null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Duel.YouAlreadyConsidering", duelStarter.Name), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                                return;
                            }
                        }

                        lock (targetLiving.TempProperties)
                        {
                            if (targetLiving.TempProperties.GetProperty<WeakRef>(DUEL_STARTER_WEAK, null) != null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Duel.TargetAlreadyConsidering", targetLiving.Name), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (targetLiving.TempProperties.GetProperty<WeakRef>(CHALLENGE_TARGET_WEAK, null) != null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Duel.TargetAlreadyChallenging", targetLiving.Name), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            targetLiving.TempProperties.SetProperty(DUEL_STARTER_WEAK, new WeakRef(client.Player));
                        }

                        lock (client.Player.TempProperties)
                        {
                            client.Player.TempProperties.SetProperty(CHALLENGE_TARGET_WEAK, new WeakRef(targetLiving));
                        }

                        client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Duel.YouChallenge", targetLiving.Name), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                        targetPlayer?.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Duel.ChallengesYou", client.Player.Name), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);

                        if (targetMimic != null)
                        {
                            if (Util.Chance(95))
                            {
                                targetMimic.TargetObject = client.Player;

                                lock (targetMimic.TempProperties)
                                {
                                    weak = targetMimic.TempProperties.GetProperty<WeakRef>(DUEL_STARTER_WEAK, null);
                                }

                                // Considering. Probably not needed as mimics immediately accept or decline.
                                if (weak == null || (duelStarter = weak.Target as GameLiving) == null)
                                    return;

                                if (!CheckDuelStart(client.Player, duelStarter))
                                    return;

                                GameDuel duel = new(client.Player, targetMimic);
                                duel.Start();

                                client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Duel.TargetAccept", client.Player.Name), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);

                                lock (targetMimic.TempProperties)
                                {
                                    targetMimic.TempProperties.RemoveProperty(DUEL_STARTER_WEAK);
                                }

                                lock (duelStarter.TempProperties)
                                {
                                    (duelStarter).TempProperties.RemoveProperty(CHALLENGE_TARGET_WEAK);
                                }
                            }
                            else
                            {
                                lock (client.Player.TempProperties)
                                {
                                    weak = client.Player.TempProperties.GetProperty<WeakRef>(DUEL_STARTER_WEAK, null);
                                    client.Player.TempProperties.RemoveProperty(DUEL_STARTER_WEAK);
                                }

                                if (weak == null || (duelStarter = weak.Target as GameLiving) == null)
                                {
                                    client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Duel.NotInDuel"), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                                    return;
                                }

                                lock (duelStarter.TempProperties)
                                {
                                    duelStarter.TempProperties.RemoveProperty(CHALLENGE_TARGET_WEAK);
                                }

                                if (duelStarter is GamePlayer)
                                    ((GamePlayer)duelStarter).Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Duel.TargetDeclines", client.Player.Name), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);

                                client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Duel.YouDecline", duelStarter.Name), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                            }
                        }

                        return;
                    }

                    case "accept":
                    {
                        lock (client.Player.TempProperties)
                        {
                            weak = client.Player.TempProperties.GetProperty<WeakRef>(DUEL_STARTER_WEAK, null);
                        }

                        if (weak == null || (duelStarter = weak.Target as GameLiving) == null)
                        {
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Duel.ConsideringDuel"), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                            return;
                        }

                        if (!CheckDuelStart(client.Player, duelStarter))
                            return;

                        GameDuel duel = new(duelStarter, client.Player);
                        duel.Start();

                        if (duelStarter is GamePlayer)
                            ((GamePlayer)duelStarter).Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Duel.TargetAccept", client.Player.Name), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);

                        client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Duel.YouAccept"), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);

                        lock (client.Player.TempProperties)
                        {
                            client.Player.TempProperties.RemoveProperty(DUEL_STARTER_WEAK);
                        }
                        lock (duelStarter.TempProperties)
                        {
                            duelStarter.TempProperties.RemoveProperty(CHALLENGE_TARGET_WEAK);
                        }

                        return;
                    }

                    case "decline":
                    {
                        lock (client.Player.TempProperties)
                        {
                            weak = client.Player.TempProperties.GetProperty<WeakRef>(DUEL_STARTER_WEAK, null);
                            client.Player.TempProperties.RemoveProperty(DUEL_STARTER_WEAK);
                        }

                        if (weak == null || (duelStarter = weak.Target as GameLiving) == null)
                        {
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Duel.NotInDuel"), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                            return;
                        }

                        lock (duelStarter.TempProperties)
                        {
                            duelStarter.TempProperties.RemoveProperty(CHALLENGE_TARGET_WEAK);
                        }

                        if (duelStarter is GamePlayer)
                            ((GamePlayer)duelStarter).Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Duel.TargetDeclines", client.Player.Name), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);

                        client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Duel.YouDecline", duelStarter.Name), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                        return;
                    }

                    case "cancel":
                    {
                        lock (client.Player.TempProperties)
                        {
                            weak = client.Player.TempProperties.GetProperty<WeakRef>(CHALLENGE_TARGET_WEAK, null);
                            client.Player.TempProperties.RemoveProperty(CHALLENGE_TARGET_WEAK);
                        }

                        if (weak == null || (duelTarget = weak.Target as GameLiving) == null)
                        {
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Duel.YouHaventChallenged"), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                            return;
                        }

                        lock (duelTarget.TempProperties)
                        {
                            duelTarget.TempProperties.RemoveProperty(DUEL_STARTER_WEAK);
                        }

                        if (duelStarter is GamePlayer)
                            ((GamePlayer)duelStarter).Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Duel.TargetCancel", client.Player.Name), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);

                        client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Duel.YouCancel"), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                        return;
                    }

                    case "surrender":
                    {
                        GameLiving target = client.Player.DuelPartner;

						if (target == null)
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Duel.NotInDuel"), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
							return;
						}

                        client.Player.Duel.Stop();

                        client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Duel.YouSurrender", target.Name), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);

                        if (target is GamePlayer)
                            ((GamePlayer)target).Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Duel.TargetSurrender", client.Player.Name), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);

                        Message.SystemToArea(client.Player, LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Duel.PlayerVsPlayer", client.Player.Name, target.Name), eChatType.CT_Emote, client.Player, target);

                        return;
                    }
                }
            }

            client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Duel.DuelOptions"), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
        }

        /// <summary>
        /// Checks if a duel can be started between 2 players at this moment
        /// </summary>
        /// <param name="actionSource">The duel starter</param>
        /// <param name="actionTarget">The duel target</param>
        /// <returns>true if players can start a duel</returns>
        private static bool CheckDuelStart(GameLiving actionSource, GameLiving actionTarget)
        {
            GamePlayer playerActionSource = actionSource as GamePlayer;
            GamePlayer playerActionTarget = actionTarget as GamePlayer;
            MimicNPC mimicActionSource = actionSource as MimicNPC;
            MimicNPC mimicActionTarget = actionTarget as MimicNPC;

            if (!GameServer.ServerRules.IsSameRealm(actionSource, actionTarget, true))
            {
                playerActionSource?.Out.SendMessage(LanguageMgr.GetTranslation(playerActionSource.Client, "Scripts.Players.Duel.EnemyRealm"), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (playerActionSource?.DuelPartner != null)
            {
                playerActionSource?.Out.SendMessage(LanguageMgr.GetTranslation(playerActionSource.Client, "Scripts.Players.Duel.YouInDuel"), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                return false;
            }
            if (playerActionTarget?.DuelPartner != null)
            {
                playerActionSource?.Out.SendMessage(LanguageMgr.GetTranslation(playerActionSource.Client, "Scripts.Players.Duel.TargetInDuel", actionTarget.Name), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (mimicActionSource?.DuelPartner != null || mimicActionTarget?.DuelPartner != null)
                return false;

            if (actionTarget.InCombat)
            {
                playerActionSource?.Out.SendMessage(LanguageMgr.GetTranslation(playerActionSource.Client, "Scripts.Players.Duel.TargetInCombat", actionTarget.Name), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                return false;
            }
            if (actionSource.InCombat)
            {
                playerActionSource?.Out.SendMessage(LanguageMgr.GetTranslation(playerActionSource.Client, "Scripts.Players.Duel.YouInCombat"), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                return false;
            }
            if (actionTarget.Group != null)
            {
                playerActionSource?.Out.SendMessage(LanguageMgr.GetTranslation(playerActionSource.Client, "Scripts.Players.Duel.TargetInGroup", actionTarget.Name), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                return false;
            }
            if (actionSource.Group != null)
            {
                playerActionSource?.Out.SendMessage(LanguageMgr.GetTranslation(playerActionSource.Client, "Scripts.Players.Duel.YouInGroup"), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                return false;
            }
            if (actionSource.Health < actionSource.MaxHealth)
            {
                playerActionSource?.Out.SendMessage(LanguageMgr.GetTranslation(playerActionSource.Client, "Scripts.Players.Duel.YouHealth"), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                return false;
            }
            if (actionTarget.Health < actionTarget.MaxHealth)
            {
                playerActionSource?.Out.SendMessage(LanguageMgr.GetTranslation(playerActionSource.Client, "Scripts.Players.Duel.TargetHealth"), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                return false;
            }

            return true;
        }
    }
}