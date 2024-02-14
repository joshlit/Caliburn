using System.Collections.Generic;
using DOL.AI.Brain;
using DOL.GS.PacketHandler;
using DOL.GS.Scripts;
using DOL.GS.Spells;
using DOL.Language;

namespace DOL.GS
{
    public class GameDuel
    {
        private const string DUEL_PREVIOUS_LASTATTACKTICKPVP = "DUEL_PREVIOUS_LASTATTACKTICKPVP";
        private const string DUEL_PREVIOUS_LASTATTACKEDBYENEMYTICKPVP = "DUEL_PREVIOUS_LASTATTACKEDBYENEMYTICKPVP";

        public GameLiving Starter { get; private set; }
        public GameLiving Target { get; private set; }

        public GameDuel(GameLiving starter, GameLiving target)
        {
            Starter = starter;
            Target = target;
        }

        public GameLiving GetPartnerOf(GameLiving living)
        {
            if (living is GameNPC npc && npc.Brain is ControlledNpcBrain brain)
                living = brain.GetLivingOwner();

            return living == Starter ? Target : Starter;
        }

        public void Start()
        {
            HandleLiving(Starter, this);
            HandleLiving(Target, this);

            static void HandleLiving(GameLiving participant, GameDuel duel)
            {
                GamePlayer player = participant as GamePlayer;
                MimicNPC mimic = participant as MimicNPC;

                if (player != null)
                {
                    player.OnDuelStart(duel);
                    player.TempProperties.SetProperty(DUEL_PREVIOUS_LASTATTACKTICKPVP, player.LastAttackTickPvP);
                    player.TempProperties.SetProperty(DUEL_PREVIOUS_LASTATTACKEDBYENEMYTICKPVP, player.LastAttackedByEnemyTickPvP);
                }
                else if (mimic != null)
                {
                    mimic.OnDuelStart(duel);
                    mimic.TempProperties.SetProperty(DUEL_PREVIOUS_LASTATTACKTICKPVP, mimic.LastAttackTickPvP);
                    mimic.TempProperties.SetProperty(DUEL_PREVIOUS_LASTATTACKEDBYENEMYTICKPVP, mimic.LastAttackedByEnemyTickPvP);
                }
            }
        }

        public void Stop()
        {
            HandleLiving(Starter, Target);
            HandleLiving(Target, Starter);

            static void HandleLiving(GameLiving participant, GameLiving partner)
            {
                GamePlayer player = participant as GamePlayer;
                MimicNPC mimic = participant as MimicNPC;

                if (player != null)
                {
                    player.OnDuelStop();
                    player.LastAttackTickPvP = player.TempProperties.GetProperty<long>(DUEL_PREVIOUS_LASTATTACKTICKPVP);
                    player.LastAttackedByEnemyTickPvP = player.TempProperties.GetProperty<long>(DUEL_PREVIOUS_LASTATTACKEDBYENEMYTICKPVP);

                    lock (player.XPGainers.SyncRoot)
                    {
                        player.XPGainers.Clear();
                    }

                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GamePlayer.DuelStop.DuelEnds"), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                }
                else if (mimic != null)
                {
                    mimic.OnDuelStop();
                    mimic.LastAttackTickPvP = mimic.TempProperties.GetProperty<long>(DUEL_PREVIOUS_LASTATTACKTICKPVP);
                    mimic.LastAttackedByEnemyTickPvP = mimic.TempProperties.GetProperty<long>(DUEL_PREVIOUS_LASTATTACKEDBYENEMYTICKPVP);

                    lock (mimic.XPGainers.SyncRoot)
                    {
                        mimic.XPGainers.Clear();
                    }
                }

                StopEffects(participant, partner);
            }

            static void StopEffects(GameLiving living, GameLiving caster)
            {
                Loop(living.effectListComponent.GetAllEffects(), caster);

                IControlledBrain controlledBrain = living.ControlledBrain;

                if (controlledBrain != null)
                    Loop(controlledBrain.Body.effectListComponent.GetAllEffects(), caster);

                static void Loop(List<ECSGameEffect> effects, GameLiving caster)
                {
                    GameNPC petCaster = caster.ControlledBrain?.Body;

                    foreach (ECSGameEffect effect in effects)
                    {
                        if (effect.HasPositiveEffect)
                            continue;

                        ISpellHandler spellHandler = effect.SpellHandler;

                        if (spellHandler == null)
                            continue;

                        if (spellHandler.Caster == caster || (spellHandler.Caster != null && spellHandler.Caster == petCaster))
                        {
                            effect.TriggersImmunity = false;
                            EffectService.RequestCancelEffect(effect);
                        }
                    }
                }
            }
        }
    }
}