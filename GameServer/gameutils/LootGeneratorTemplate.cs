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
 */

using System;
using System.Collections.Generic;
using System.Linq;
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
    /// <summary>
    /// TemplateLootGenerator
    /// This implementation uses LootTemplates to relate loots to a specific mob type.
    /// Used DB Tables:
    ///				MobxLootTemplate  (Relation between Mob and loottemplate
    ///				LootTemplate	(loottemplate containing possible loot items)
    /// </summary>
    public class LootGeneratorTemplate : LootGeneratorBase
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Map holding a list of ItemTemplateIDs for each TemplateName
        /// 1:n mapping between loottemplateName and loottemplate entries
        /// </summary>
        protected static Dictionary<string, Dictionary<string, LootTemplate>> m_lootTemplates = null;

        /// <summary>
        /// Map holding the corresponding LootTemplateName for each MobName
        /// 1:n Mapping between Mob and LootTemplate
        /// </summary>
        protected static Dictionary<string, List<MobXLootTemplate>> m_mobXLootTemplates = null;

        /// <summary>
        /// Construct a new templategenerate and load its values from database.
        /// </summary>
        public LootGeneratorTemplate()
        {
            PreloadLootTemplates();
        }

        public static void ReloadLootTemplates()
        {
            m_lootTemplates = null;
            m_mobXLootTemplates = null;
            PreloadLootTemplates();
        }

        /// <summary>
        /// Loads the loottemplates
        /// </summary>
        /// <returns></returns>
        protected static bool PreloadLootTemplates()
        {
            if (m_lootTemplates == null)
            {
                m_lootTemplates = new Dictionary<string, Dictionary<string, LootTemplate>>();
                Dictionary<string, ItemTemplate> itemtemplates = new Dictionary<string, ItemTemplate>();

                lock (m_lootTemplates)
                {
                    IList<LootTemplate> dbLootTemplates = null;

                    try
                    {
                        // TemplateName (typically the mob name), ItemTemplateID, Chance
                        dbLootTemplates = GameServer.Database.SelectAllObjects<LootTemplate>();
                        itemtemplates = GameServer.Database.SelectAllObjects<ItemTemplate>()
                            .ToDictionary(k => k.Id_nb.ToLower());
                    }
                    catch (Exception e)
                    {
                        if (log.IsErrorEnabled)
                        {
                            log.Error("LootGeneratorTemplate: LootTemplates could not be loaded:", e);
                        }

                        return false;
                    }

                    if (dbLootTemplates != null)
                    {
                        Dictionary<string, LootTemplate> loot = null;

                        foreach (LootTemplate dbTemplate in dbLootTemplates)
                        {
                            if (!m_lootTemplates.TryGetValue(dbTemplate.TemplateName.ToLower(), out loot))
                            {
                                loot = new Dictionary<string, LootTemplate>();
                                m_lootTemplates[dbTemplate.TemplateName.ToLower()] = loot;
                            }

                            ItemTemplate drop = null;

                            if (itemtemplates.ContainsKey(dbTemplate.ItemTemplateID.ToLower()))
                                drop = itemtemplates[dbTemplate.ItemTemplateID.ToLower()];

                            if (drop == null)
                            {
                                if (log.IsErrorEnabled)
                                    log.Error("ItemTemplate: " + dbTemplate.ItemTemplateID +
                                              " is not found, it is referenced from loottemplate: " +
                                              dbTemplate.TemplateName);
                            }
                            else
                            {
                                if (!loot.ContainsKey(dbTemplate.ItemTemplateID.ToLower()))
                                    loot.Add(dbTemplate.ItemTemplateID.ToLower(), dbTemplate);
                            }
                        }
                    }
                }

                log.Info("LootTemplates pre-loaded.");
            }

            if (m_mobXLootTemplates == null)
            {
                m_mobXLootTemplates = new Dictionary<string, List<MobXLootTemplate>>();

                lock (m_mobXLootTemplates)
                {
                    IList<MobXLootTemplate> dbMobXLootTemplates = null;

                    try
                    {
                        // MobName, LootTemplateName, DropCount
                        dbMobXLootTemplates = GameServer.Database.SelectAllObjects<MobXLootTemplate>();
                    }
                    catch (Exception e)
                    {
                        if (log.IsErrorEnabled)
                        {
                            log.Error("LootGeneratorTemplate: MobXLootTemplates could not be loaded", e);
                        }

                        return false;
                    }

                    if (dbMobXLootTemplates != null)
                    {
                        foreach (MobXLootTemplate dbMobXTemplate in dbMobXLootTemplates)
                        {
                            // There can be multiple MobXLootTemplates for a mob, each pointing to a different loot template
                            List<MobXLootTemplate> mobxLootTemplates;
                            if (!m_mobXLootTemplates.TryGetValue(dbMobXTemplate.MobName.ToLower(),
                                    out mobxLootTemplates))
                            {
                                mobxLootTemplates = new List<MobXLootTemplate>();
                                m_mobXLootTemplates[dbMobXTemplate.MobName.ToLower()] = mobxLootTemplates;
                            }

                            mobxLootTemplates.Add(dbMobXTemplate);
                        }
                    }
                }

                log.Info("MobXLootTemplates pre-loaded.");
            }

            return true;
        }

        /// <summary>
        /// Reload the loot templates for this mob
        /// </summary>
        /// <param name="mob"></param>
        public override void Refresh(GameNPC mob)
        {
            if (mob == null)
                return;

            bool isDefaultLootTemplateRefreshed = false;

            // First see if there are any MobXLootTemplates associated with this mob

            var mxlts = DOLDB<MobXLootTemplate>.SelectObjects(DB.Column("MobName").IsEqualTo(mob.Name.ToLower()));

            if (mxlts != null)
            {
                lock (m_mobXLootTemplates)
                {
                    foreach (MobXLootTemplate mxlt in mxlts)
                    {
                        List<MobXLootTemplate> mobxLootTemplates;
                        if (!m_mobXLootTemplates.TryGetValue(mxlt.MobName.ToLower(), out mobxLootTemplates))
                        {
                            mobxLootTemplates = new List<MobXLootTemplate>();
                            m_mobXLootTemplates[mxlt.MobName.ToLower()] = mobxLootTemplates;
                        }

                        mobxLootTemplates.Add(mxlt);

                        RefreshLootTemplate(mxlt.LootTemplateName);


                        if (mxlt.LootTemplateName.ToLower() == mob.Name.ToLower())
                            isDefaultLootTemplateRefreshed = true;
                    }
                }
            }

            // now force a refresh of the mobs default loot template

            if (isDefaultLootTemplateRefreshed == false)
                RefreshLootTemplate(mob.Name);
        }

        protected void RefreshLootTemplate(string templateName)
        {
            lock (m_lootTemplates)
            {
                if (m_lootTemplates.ContainsKey(templateName.ToLower()))
                {
                    m_lootTemplates.Remove(templateName.ToLower());
                }
            }

            var lootTemplates =
                DOLDB<LootTemplate>.SelectObjects(DB.Column("TemplateName").IsEqualTo(templateName.ToLower()));

            if (lootTemplates != null)
            {
                lock (m_lootTemplates)
                {
                    if (m_lootTemplates.ContainsKey(templateName.ToLower()))
                    {
                        m_lootTemplates.Remove(templateName.ToLower());
                    }

                    Dictionary<string, LootTemplate> lootList = new Dictionary<string, LootTemplate>();

                    foreach (LootTemplate lt in lootTemplates)
                    {
                        if (lootList.ContainsKey(lt.ItemTemplateID.ToLower()) == false)
                        {
                            lootList.Add(lt.ItemTemplateID.ToLower(), lt);
                        }
                    }

                    m_lootTemplates.Add(templateName.ToLower(), lootList);
                }
            }
        }

        public override LootList GenerateLoot(GameNPC mob, GameObject killer)
        {
            LootList loot = base.GenerateLoot(mob, killer);

            string XPItemKey = "XP_Item";
            string XPItemDroppersKey = "XP_Item_Droppers";

            try
            {
                GamePlayer player = null;

                if (killer is GamePlayer)
                {
                    player = killer as GamePlayer;
                }
                else if (killer is GameNPC && (killer as GameNPC).Brain is IControlledBrain)
                {
                    player = ((killer as GameNPC).Brain as ControlledNpcBrain).GetPlayerOwner();
                }

                // allow the leader to decide the loot realm
                if (player != null && player.Group != null)
                {
                    player = player.Group.Leader;
                }

                if (player != null)
                {
                    List<MobXLootTemplate> killedMobXLootTemplates = null;

                    // Graveen: we first privilegiate the loottemplate named 'templateid' if it exists	
                    if (mob.NPCTemplate != null &&
                        m_mobXLootTemplates.ContainsKey(mob.NPCTemplate.TemplateId.ToString().ToLower()))
                    {
                        killedMobXLootTemplates = m_mobXLootTemplates[mob.NPCTemplate.TemplateId.ToString().ToLower()];
                    }
                    // else we are choosing the loottemplate named 'mob name'
                    // this is easily allowing us to affect different level choosen loots to different level choosen mobs
                    // with identical names
                    else if (m_mobXLootTemplates.ContainsKey(mob.Name.ToLower()))
                    {
                        killedMobXLootTemplates = m_mobXLootTemplates[mob.Name.ToLower()];
                    }

                    // MobXLootTemplate contains a loot template name and the max number of drops allowed for that template.
                    // We don't need an entry in MobXLootTemplate in order to drop loot, only to control the max number of drops.

                    // LootTemplate contains a template name and an itemtemplateid (id_nb).
                    // TemplateName usually equals Mob name, so if you want to know what drops for a mob:
                    // select * from LootTemplate where templatename = 'mob name';
                    // It is possible to have TemplateName != MobName but this works only if there is an entry in MobXLootTemplate for the MobName.

                    if (killedMobXLootTemplates == null)
                    {
                        // If there is no MobXLootTemplate entry then every item in this mobs LootTemplate can drop.
                        // In addition, we can use LootTemplate.Count to determine how many of a fixed (100% chance) item can drop
                        if (m_lootTemplates.ContainsKey(mob.Name.ToLower()))
                        {
                            Dictionary<string, LootTemplate> lootTemplatesToDrop = m_lootTemplates[mob.Name.ToLower()];
                            List<LootTemplate> timedDrops = new List<LootTemplate>();

                            if (lootTemplatesToDrop != null)
                            {
                                long dropChan = 0;
                                long tmp = 0;
                                foreach (LootTemplate lootTemplate in lootTemplatesToDrop.Values)
                                {
                                    ItemTemplate drop =
                                        GameServer.Database.FindObjectByKey<ItemTemplate>(lootTemplate.ItemTemplateID);

                                    if (drop != null && (drop.Realm == (int) player.Realm || drop.Realm == 0 ||
                                                         player.CanUseCrossRealmItems))
                                    {
                                        if (lootTemplate.Chance < 0)
                                        {
                                            timedDrops.Add(lootTemplate);
                                        }
                                        else if (lootTemplate.Chance == 100)
                                        {
                                            loot.AddFixed(drop, lootTemplate.Count);
                                        }
                                        else
                                        {
                                            loot.AddRandom(lootTemplate.Chance, drop, 1);
                                        }
                                    }
                                }

                                if (timedDrops.Count > 0)
                                {
                                    LootTemplate
                                        lootTemplate =
                                            timedDrops[
                                                Util.Random(timedDrops.Count - 1)]; //randomly pick one available drop

                                    lock (player._xpGainersLock)
                                    {
                                        ItemTemplate drop =
                                            GameServer.Database.FindObjectByKey<ItemTemplate>(lootTemplate
                                                .ItemTemplateID);
                                        int dropCooldown =
                                            lootTemplate.Chance * -1 * 60 * 1000; //chance time in minutes
                                        long tempProp =
                                            player.TempProperties.GetProperty<long>(XPItemKey,
                                                0); //check if our loot has dropped for player
                                        List<string> itemsDropped =
                                            player.TempProperties
                                                .GetProperty<List<string>>(
                                                    XPItemDroppersKey); //check our list of dropped monsters
                                        if (itemsDropped == null) itemsDropped = new List<string>();
                                        GamePlayer GroupedTimerToUse = null;

                                        if (player.Group != null)
                                            GroupedTimerToUse =
                                                CheckGroupForValidXpTimer(XPItemKey, dropCooldown, player);

                                        //if we've never dropped an item, or our cooldown is up, drop an item
                                        if (tempProp == 0 ||
                                            tempProp + dropCooldown < GameLoop.GameLoopTime)
                                        {
                                            long nextDropTime = GameLoop.GameLoopTime;

                                            /*
                                            AccountXRealmLoyalty realmLoyalty =
                                                DOLDB<AccountXRealmLoyalty>.SelectObject(DB.Column("AccountID")
                                                    .IsEqualTo(player.Client.Account.ObjectId)
                                                    .And(DB.Column("Realm").IsEqualTo(player.Realm)));*/
                                            var realmLoyalty = LoyaltyManager.GetPlayerRealmLoyalty(player);
                                            if (realmLoyalty != null && realmLoyalty.Days > 0)
                                            {
                                                int tmpLoyal = realmLoyalty.Days > 30
                                                    ? 30
                                                    : realmLoyalty.Days;
                                                nextDropTime -=
                                                    tmpLoyal *
                                                    1000; //reduce cooldown by 1s per loyalty day up to 30s cap
                                            }

                                            var numRelics = RelicMgr.GetRelicCount(player.Realm);
                                            if (numRelics > 0) nextDropTime -= 10000 * numRelics;

                                            loot.AddFixed(drop, lootTemplate.Count);
                                            player.TempProperties.SetProperty(XPItemKey, nextDropTime);

                                            itemsDropped.Clear();
                                            player.TempProperties.SetProperty(XPItemDroppersKey, itemsDropped);
                                        }
                                        else if (GroupedTimerToUse != null)
                                        {
                                            long nextDropTime = GameLoop.GameLoopTime;
                                            /*
                                            AccountXRealmLoyalty realmLoyalty =
                                                DOLDB<AccountXRealmLoyalty>.SelectObject(DB.Column("AccountID")
                                                    .IsEqualTo(GroupedTimerToUse.Client.Account.ObjectId)
                                                    .And(DB.Column("Realm").IsEqualTo(player.Realm)));*/
                                            var realmLoyalty = LoyaltyManager.GetPlayerRealmLoyalty(GroupedTimerToUse);
                                            if (realmLoyalty != null && realmLoyalty.Days > 0)
                                            {
                                                int tmpLoyal = realmLoyalty.Days > 30
                                                    ? 30
                                                    : realmLoyalty.Days;
                                                nextDropTime -=
                                                    tmpLoyal *
                                                    1000; //reduce cooldown by 1s per loyalty day up to 30s cap
                                            }

                                            loot.AddFixed(drop, lootTemplate.Count);
                                            GroupedTimerToUse.TempProperties.SetProperty(XPItemKey, nextDropTime);

                                            itemsDropped.Clear();
                                            GroupedTimerToUse.TempProperties.SetProperty(XPItemDroppersKey,
                                                itemsDropped);
                                        }
                                        //else if this drop cycle has not seen this item, reduce global cooldown
                                        else if (!itemsDropped.Contains(drop.Name))
                                        {
                                            itemsDropped.Add(drop.Name);
                                            tempProp -= 20 * 1000; //take 20 seconds off cooldown
                                            player.TempProperties.SetProperty(XPItemKey, tempProp);
                                            player.TempProperties.SetProperty(XPItemDroppersKey, itemsDropped);
                                            tmp = tempProp;
                                            dropChan = dropCooldown;
                                        }
                                    }
                                }

                                if (tmp > 0 && dropChan > 0)
                                {
                                    long timeDifference = GameLoop.GameLoopTime - (tmp + dropChan);
                                    timeDifference *= -1;
                                    //"PvE Time Remaining: " + TimeSpan.FromMilliseconds(pve).Hours + "h " + TimeSpan.FromMilliseconds(pve).Minutes + "m " + TimeSpan.FromMilliseconds(pve).Seconds + "s");
                                    if (timeDifference > 0)
                                        player.Out.SendMessage(
                                            TimeSpan.FromMilliseconds(timeDifference).Hours + "h " +
                                            TimeSpan.FromMilliseconds(timeDifference).Minutes + "m " +
                                            TimeSpan.FromMilliseconds(timeDifference).Seconds + "s until next XP item",
                                            eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    else
                                        player.Out.SendMessage("XP item will drop after your next kill!",
                                            eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                }
                            }
                        }
                    }
                    else
                    {
                        // MobXLootTemplate exists and tells us the max number of items that can drop.
                        // Because we are restricting the max number of items to drop we need to traverse the list
                        // and add every 100% chance items to the loots Fixed list and add the rest to the Random list
                        // due to the fact that 100% items always drop regardless of the drop limit

                        List<LootTemplate> lootTemplatesToDrop = new List<LootTemplate>();

                        long dropChan = 0;
                        long tmp = 0;
                        foreach (MobXLootTemplate mobXLootTemplate in killedMobXLootTemplates)
                        {
                            loot = GenerateLootFromMobXLootTemplates(mobXLootTemplate, lootTemplatesToDrop, loot,
                                player);

                            if (lootTemplatesToDrop != null)
                            {
                                List<LootTemplate> timedDrops = new List<LootTemplate>();
                                foreach (LootTemplate lootTemplate in lootTemplatesToDrop)
                                {
                                    ItemTemplate drop =
                                        GameServer.Database.FindObjectByKey<ItemTemplate>(lootTemplate.ItemTemplateID);

                                    if (lootTemplate.Chance < 0)
                                    {
                                        timedDrops.Add(lootTemplate);
                                    }
                                    else if (drop != null && (drop.Realm == (int) player.Realm || drop.Realm == 0 ||
                                                              player.CanUseCrossRealmItems))
                                    {
                                        loot.AddRandom(lootTemplate.Chance, drop, 1);
                                    }
                                }

                                if (timedDrops.Count > 0)
                                {
                                    LootTemplate
                                        lootTemplate =
                                            timedDrops[
                                                Util.Random(timedDrops.Count - 1)]; //randomly pick one available drop

                                    lock (player._xpGainersLock)
                                    {
                                        ItemTemplate drop =
                                            GameServer.Database.FindObjectByKey<ItemTemplate>(lootTemplate
                                                .ItemTemplateID);
                                        int dropCooldown =
                                            lootTemplate.Chance * -1 * 60 * 1000; //chance time in minutes
                                        long tempProp =
                                            player.TempProperties.GetProperty<long>(XPItemKey,
                                                0); //check if our loot has dropped for player
                                        List<string> itemsDropped =
                                            player.TempProperties
                                                .GetProperty<List<string>>(
                                                    XPItemDroppersKey); //check our list of dropped monsters
                                        GamePlayer GroupedTimerToUse = null;
                                        if (itemsDropped == null) itemsDropped = new List<string>();

                                        if (player.Group != null)
                                            GroupedTimerToUse =
                                                CheckGroupForValidXpTimer(XPItemKey, dropCooldown, player);

                                        //if we've never dropped an item, or our cooldown is up, drop an item
                                        if (tempProp == 0 ||
                                            tempProp + dropCooldown < GameLoop.GameLoopTime)
                                        {
                                            long nextDropTime = GameLoop.GameLoopTime;
                                            /*
                                            AccountXRealmLoyalty realmLoyalty =
                                                DOLDB<AccountXRealmLoyalty>.SelectObject(DB.Column("AccountID")
                                                    .IsEqualTo(player.Client.Account.ObjectId)
                                                    .And(DB.Column("Realm").IsEqualTo(player.Realm)));*/
                                            var realmLoyalty = LoyaltyManager.GetPlayerRealmLoyalty(player);
                                            if (realmLoyalty != null && realmLoyalty.Days > 0)
                                            {
                                                int tmpLoyal = realmLoyalty.Days > 30
                                                    ? 30
                                                    : realmLoyalty.Days;
                                                nextDropTime -=
                                                    tmpLoyal *
                                                    1000; //reduce cooldown by 1s per loyalty day up to 30s cap
                                            }
                                            
                                            var numRelics = RelicMgr.GetRelicCount(player.Realm);
                                            if (numRelics > 0) nextDropTime -= 10000 * numRelics;

                                            loot.AddFixed(drop, lootTemplate.Count);
                                            player.TempProperties.SetProperty(XPItemKey, nextDropTime);

                                            itemsDropped.Clear();
                                            player.TempProperties.SetProperty(XPItemDroppersKey, itemsDropped);
                                        }
                                        else if (GroupedTimerToUse != null)
                                        {
                                            long nextDropTime = GameLoop.GameLoopTime;
                                            /*AccountXRealmLoyalty realmLoyalty =
                                                DOLDB<AccountXRealmLoyalty>.SelectObject(DB.Column("AccountID")
                                                    .IsEqualTo(GroupedTimerToUse.Client.Account.ObjectId)
                                                    .And(DB.Column("Realm").IsEqualTo(player.Realm)));*/
                                            var realmLoyalty = LoyaltyManager.GetPlayerRealmLoyalty(GroupedTimerToUse);
                                            if (realmLoyalty != null && realmLoyalty.Days > 0)
                                            {
                                                int tmpLoyal = realmLoyalty.Days > 30
                                                    ? 30
                                                    : realmLoyalty.Days;
                                                nextDropTime -=
                                                    tmpLoyal *
                                                    1000; //reduce cooldown by 1s per loyalty day up to 30s cap
                                            }

                                            loot.AddFixed(drop, lootTemplate.Count);
                                            GroupedTimerToUse.TempProperties.SetProperty(XPItemKey, nextDropTime);

                                            itemsDropped.Clear();
                                            GroupedTimerToUse.TempProperties.SetProperty(XPItemDroppersKey,
                                                itemsDropped);
                                        }
                                        //else if this drop cycle has not seen this item, reduce global cooldown
                                        else if (itemsDropped == null || !itemsDropped.Contains(drop.Name))
                                        {
                                            itemsDropped.Add(drop.Name);
                                            tempProp -= 20 * 1000; //take 20 seconds off cooldown
                                            player.TempProperties.SetProperty(XPItemKey, tempProp);
                                            player.TempProperties.SetProperty(XPItemDroppersKey, itemsDropped);
                                        }

                                        tmp = tempProp;
                                        dropChan = dropCooldown;
                                    }
                                }
                            }
                        }

                        if (tmp > 0 && dropChan > 0)
                        {
                            long timeDifference = GameLoop.GameLoopTime - (tmp + dropChan);
                            timeDifference *= -1;
                            //"PvE Time Remaining: " + TimeSpan.FromMilliseconds(pve).Hours + "h " + TimeSpan.FromMilliseconds(pve).Minutes + "m " + TimeSpan.FromMilliseconds(pve).Seconds + "s");
                            if (timeDifference > 0)
                                player.Out.SendMessage(
                                    TimeSpan.FromMilliseconds(timeDifference).Hours + "h " +
                                    TimeSpan.FromMilliseconds(timeDifference).Minutes + "m " +
                                    TimeSpan.FromMilliseconds(timeDifference).Seconds + "s until next XP item",
                                    eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            else
                                player.Out.SendMessage("XP item will drop after your next kill!", eChatType.CT_System,
                                    eChatLoc.CL_SystemWindow);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in LootGeneratorTemplate for mob {0}.  Exception: {1} {2}", mob.Name, ex.Message,
                    ex.StackTrace);
            }

            return loot;
        }

        private GamePlayer CheckGroupForValidXpTimer(String xpItemKey, int dropCooldown, GamePlayer player)
        {
            //check if any group member has a valid timer to use
            foreach (GamePlayer groupMember in player.Group.GetNearbyPlayersInTheGroup(player))
            {
                if ((player.CurrentZone != groupMember.CurrentZone) ||
                    player.CurrentRegion != groupMember.CurrentRegion) continue;
                if (player.GetDistance(groupMember) > WorldMgr.MAX_EXPFORKILL_DISTANCE) continue;
                long tempProp = groupMember.TempProperties.GetProperty<long>(xpItemKey, 0);
                if (tempProp == 0 || tempProp + dropCooldown < GameLoop.GameLoopTime)
                    return groupMember;
            }

            return null;
        }

        /// <summary>
        /// Add all loot templates specified in MobXLootTemplate for an entry in LootTemplates
        /// If the item has a 100% drop chance add it as a fixed drop to the loot list.
        /// </summary>
        /// <param name="mobXLootTemplate">Entry in MobXLootTemplate.</param>
        /// <param name="lootTemplates">List of all itemtemplates this mob can drop and the chance to drop</param>
        /// <param name="lootList">List to hold loot.</param>
        /// <param name="player">Player used to determine realm</param>
        /// <returns>lootList (for readability)</returns>
        private LootList GenerateLootFromMobXLootTemplates(MobXLootTemplate mobXLootTemplates,
            List<LootTemplate> lootTemplates, LootList lootList, GamePlayer player)
        {
            if (mobXLootTemplates == null || lootTemplates == null || player == null)
                return lootList;

            // Using Name + Realm (if ALLOW_CROSS_REALM_ITEMS) for the key to try and prevent duplicate drops

            Dictionary<string, LootTemplate> templateList = null;

            if (m_lootTemplates.ContainsKey(mobXLootTemplates.MobName.ToLower()))
            {
                templateList = m_lootTemplates[mobXLootTemplates.MobName.ToLower()];
            }

            if (templateList != null)
            {
                foreach (LootTemplate lootTemplate in templateList.Values)
                {
                    ItemTemplate drop = GameServer.Database.FindObjectByKey<ItemTemplate>(lootTemplate.ItemTemplateID);

                    if (drop != null && (drop.Realm == (int) player.Realm || drop.Realm == 0 ||
                                         player.CanUseCrossRealmItems))
                    {
                        if (lootTemplate.Chance == 100)
                        {
                            // Added support for specifying drop count in LootTemplate rather than relying on MobXLootTemplate DropCount
                            if (lootTemplate.Count > 0)
                                lootList.AddFixed(drop, lootTemplate.Count);
                            else
                                lootList.AddFixed(drop, mobXLootTemplates.DropCount);
                        }
                        else
                        {
                            lootTemplates.Add(lootTemplate);
                            lootList.DropCount = Math.Max(lootList.DropCount, mobXLootTemplates.DropCount);
                        }
                    }
                }
            }

            return lootList;
        }
    }
}
