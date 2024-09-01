/* Project inspired by code by RDSandersJR
 * contributing code from BluRaven from his MLNPC
 * Perfected By Bones
 * 
 *to create in game type: /mob create DOL.GS.Scripts.DreadedSealCollector 
 *or you can download the dreaded seal collector.sql which changes the proper Database mobs to use this script
*/

using System;
using log4net;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using System.Reflection;

namespace DOL.GS.Scripts
{


    public class DreadedSealCollector : GameNPC
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected int m_count; // count of items, for stack
        private long amount = 0;

        #region Constructor
        /*
        public override bool AddToWorld()
        {

            Name = "Bones";
            GuildName = "Dreaded Seal Collector";
            Level = 51;
            Flags = eFlags.PEACE;
            Realm = eRealm.None;
            Model = 2124;
            Size = 65;
            GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
            template.AddNPCEquipment(eInventorySlot.TorsoArmor, 4031, 0, 0, 5); //model, color, effect, extension
            template.AddNPCEquipment(eInventorySlot.ArmsArmor, 4033, 0, 0, 0);
            template.AddNPCEquipment(eInventorySlot.LegsArmor, 4032, 0, 0);
            template.AddNPCEquipment(eInventorySlot.HandsArmor, 4034, 0, 0, 5);
            //template.AddNPCEquipment(eInventorySlot.HeadArmor, 4067, 0, 0, 0); 
            template.AddNPCEquipment(eInventorySlot.Cloak, 3801, 0, 0, 0);
            template.AddNPCEquipment(eInventorySlot.FeetArmor, 4035, 0, 0, 5);
            Inventory = template.CloseTemplate();
            return base.AddToWorld();
            }
        }
        */
        #endregion Constructor
        

        #region Add Seals to Database
        // Add seals to Database if they don't exist
        [ScriptLoadedEvent]
        public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            #region Add Seals to Item Templates
                DbItemTemplate item;
                item = GameServer.Database.FindObjectByKey<DbItemTemplate>("glowing_dreaded_seal");
                if (item == null)
                {
                    item = new DbItemTemplate();
                    item.AllowAdd = true;
                    item.AllowUpdate = true;
                    item.Id_nb = "glowing_dreaded_seal";
                    item.Name = "Glowing Dreaded Seal";
                    item.Level = 30;
                    item.Item_Type = 14;
                    item.Model = 483;
                    item.CanDropAsLoot = true;
                    item.IsTradable = true;
                    item.IsIndestructible = false;
                    item.Object_Type = 0;
                    item.IsDropable = false;
                    item.Quality = 70;
                    item.Weight = 0;
                    item.MaxCondition = 100;
                    item.MaxDurability = 100;
                    item.Condition = 100;
                    item.Durability = 100;
                    item.MaxCount = 10;
                    item.IsDropable = false;
                    item.Description = "To show appreciation for service fighting these enemies -\n" +
                        "the lords of the land will award Realm points and Realm abilities to those who defeat them.\n" +
                        "The people who accept these seals are in the 3 major cities:\n" +
                        "Relena in Tir Na Nog\n" +
                        "Lady Nina in Camelot\n" +
                        "and Fiana in Jordheim.";
                    item.Price = 3000; // Realm Point Value
                    GameServer.Database.AddObject(item);

                    if (log.IsDebugEnabled)
                        log.Debug("Added " + item.Id_nb);
                }

                item = GameServer.Database.FindObjectByKey<DbItemTemplate>("sanguine_dreaded_seal");
                if (item == null)
                {
                    item = new DbItemTemplate();
                    item.AllowAdd = true;
                    item.AllowUpdate = true;
                    item.Id_nb = "sanguine_dreaded_seal";
                    item.Name = "Sanguine Dreaded Seal";
                    item.Level = 30;
                    item.Item_Type = 14;
                    item.Model = 484;
                    item.CanDropAsLoot = true;
                    item.IsTradable = true;
                    item.IsIndestructible = false;
                    item.Object_Type = 0;
                    item.Quality = 70;
                    item.Weight = 0;
                    item.MaxCondition = 100;
                    item.MaxDurability = 100;
                    item.Condition = 100;
                    item.Durability = 100;
                    item.MaxCount = 5;
                    item.IsDropable = false;
                    item.Description = "To show appreciation for service fighting these enemies - \n" +
                        "the lords of the land will award Realm points and Realm abilities to those who defeat them.\n" +
                        "The people who accept these seals are in the 3 major cities:\n" +
                        "Relena in Tir Na Nog\n" +
                        "Lady Nina in Camelot\n" +
                        "and Fiana in Jordheim.";
                    item.Price = 3000;
                    GameServer.Database.AddObject(item);

                    if (log.IsDebugEnabled)
                        log.Debug("Added " + item.Id_nb);
                }

                item = GameServer.Database.FindObjectByKey<DbItemTemplate>("lambent_dreaded_seal");
                if (item == null)
                {
                    item = new DbItemTemplate();
                    item.AllowAdd = true;
                    item.AllowUpdate = true;
                    item.Id_nb = "lambent_dreaded_seal";
                    item.Name = "Lambent Dreaded Seal";
                    item.Level = 30;
                    item.Item_Type = 14;
                    item.Model = 485;
                    item.CanDropAsLoot = true;
                    item.IsTradable = true;
                    item.IsIndestructible = false;
                    item.Object_Type = 0;
                    item.Quality = 70;
                    item.Weight = 0;
                    item.MaxCondition = 100;
                    item.MaxDurability = 100;
                    item.Condition = 100;
                    item.Durability = 100;
                    item.MaxCount = 5;
                    item.IsDropable = false;
                    item.Description = "To show appreciation for service fighting these enemies - \n" +
                        "the lords of the land will award Realm points and Realm abilities to those who defeat them.\n" +
                        "The people who accept these seals are in the 3 major cities:\n" +
                        "Relena in Tir Na Nog\n" +
                        "Lady Nina in Camelot\n" +
                        "and Fiana in Jordheim.\n" +
                        "This seal is worth 10 times the Glowing variety.";
                    item.Price = 30000;
                    GameServer.Database.AddObject(item);

                    if (log.IsDebugEnabled)
                        log.Debug("Added " + item.Id_nb);
                }

                item = GameServer.Database.FindObjectByKey<DbItemTemplate>("lambent_dreaded_seal2");
                if (item == null)
                {
                    item = new DbItemTemplate();
                    item.AllowAdd = true;
                    item.AllowUpdate = true;
                    item.Id_nb = "lambent_dreaded_seal2";
                    item.Name = "Lambent Dreaded Seal";
                    item.Level = 30;
                    item.Item_Type = 14;
                    item.Model = 485;
                    item.CanDropAsLoot = true;
                    item.IsTradable = true;
                    item.IsIndestructible = false;
                    item.Object_Type = 0;
                    item.Quality = 70;
                    item.Weight = 0;
                    item.MaxCondition = 100;
                    item.MaxDurability = 100;
                    item.Condition = 100;
                    item.Durability = 100;
                    item.MaxCount = 5;
                    item.IsDropable = false;
                    item.Description = "To show appreciation for service fighting these enemies - \n" +
                        "the lords of the land will award Realm points and Realm abilities to those who defeat them.\n" +
                        "The people who accept these seals are in the 3 major cities:\n" +
                        "Relena in Tir Na Nog \n" +
                        "Lady Nina in Camelot \n" +
                        "and Fiana in Jordheim. \n" +
                        "This seal is worth 10 times the Glowing variety.";
                    item.Price = 30000;
                    GameServer.Database.AddObject(item);

                    if (log.IsDebugEnabled)
                        log.Debug("Added " + item.Id_nb);
                }

                item = GameServer.Database.FindObjectByKey<DbItemTemplate>("fulgent_dreaded_seal");
                if (item == null)
                {
                    item = new DbItemTemplate();
                    item.AllowAdd = true;
                    item.AllowUpdate = true;
                    item.Id_nb = "fulgent_dreaded_seal";
                    item.Name = "Fulgent Dreaded Seal";
                    item.Level = 30;
                    item.Item_Type = 14;
                    item.Model = 486;
                    item.CanDropAsLoot = true;
                    item.IsTradable = true;
                    item.IsIndestructible = false;
                    item.Object_Type = 0;
                    item.Quality = 70;
                    item.Weight = 0;
                    item.MaxCondition = 100;
                    item.MaxDurability = 100;
                    item.Condition = 100;
                    item.Durability = 100;
                    item.MaxCount = 1;
                    item.IsDropable = false;
                    item.Description = "To show appreciation for service fighting these enemies - \n" +
                        "the lords of the land will award Realm points and Realm abilities to those who defeat them.\n" +
                        "The people who accept these seals are in the 3 major cities:\n" +
                        "Relena in Tir Na Nog\n" +
                        "Lady Nina in Camelot \n" +
                        "and Fiana in Jordheim.\n" +
                        "This seal is worth 50 times the Glowing variety.";
                    item.Price = 150000;
                    GameServer.Database.AddObject(item);

                    if (log.IsDebugEnabled)
                        log.Debug("Added " + item.Id_nb);
                }

                item = GameServer.Database.FindObjectByKey<DbItemTemplate>("effulgent_dreaded_seal");
                if (item == null)
                {
                    item = new DbItemTemplate();
                    item.AllowAdd = true;
                    item.AllowUpdate = true;
                    item.Id_nb = "effulgent_dreaded_seal";
                    item.Name = "Effulgent Dreaded Seal";
                    item.Level = 30;
                    item.Item_Type = 14;
                    item.Model = 487;
                    item.CanDropAsLoot = true;
                    item.IsTradable = true;
                    item.IsIndestructible = false;
                    item.Object_Type = 0;
                    item.Quality = 70;
                    item.Weight = 0;
                    item.MaxCondition = 100;
                    item.MaxDurability = 100;
                    item.Condition = 100;
                    item.Durability = 100;
                    item.MaxCount = 1;
                    item.IsDropable = false;
                    item.Description = "To show appreciation for service fighting these enemies - \n" +
                        "the lords of the land will award Realm points and Realm abilities to those who defeat them.\n" +
                        "The people who accept these seals are in the 3 major cities:\n" +
                        "Relena in Tir Na Nog\n" +
                        "Lady Nina in Camelot \n" +
                        "and Fiana in Jordheim.\n" +
                        "This seal is worth 250 times the Glowing variety.";
                    item.Price = 750000;
                    GameServer.Database.AddObject(item);

                    if (log.IsDebugEnabled)
                        log.Debug("Added " + item.Id_nb);
                }

                #endregion Add Seals to Item Templates
            #region Alb Crafting
                // add to Crafted Item Table
                DbCraftedItem seal;

                seal = GameServer.Database.FindObjectByKey<DbCraftedItem>("lambent_dreaded_seal");
                if (seal == null)
                {
                    seal = new DbCraftedItem();
                    seal.AllowAdd = true;
                    seal.Id_nb = "lambent_dreaded_seal";
                    seal.CraftedItemID = "4894"; // Hib Crafting
                    seal.CraftingLevel = 1;
                    seal.CraftingSkillType = 15;
                    seal.MakeTemplated = true;
                    GameServer.Database.AddObject(seal);

                    if (log.IsDebugEnabled)
                        log.Debug("Added " + seal.Id_nb);
                }

                seal = GameServer.Database.FindObjectByKey<DbCraftedItem>("lambent_dreaded_seal2");
                if (seal == null)
                {
                    seal = new DbCraftedItem();
                    seal.AllowAdd = true;
                    seal.Id_nb = "lambent_dreaded_seal2";
                    seal.CraftedItemID = "4895"; // Hib Crafting
                    seal.CraftingLevel = 1;
                    seal.CraftingSkillType = 15;
                    seal.MakeTemplated = true;
                    GameServer.Database.AddObject(seal);

                    if (log.IsDebugEnabled)
                        log.Debug("Added " + seal.Id_nb);
                }

                seal = GameServer.Database.FindObjectByKey<DbCraftedItem>("fulgent_dreaded_seal");
                if (seal == null)
                {
                    seal = new DbCraftedItem();
                    seal.AllowAdd = true;
                    seal.Id_nb = "fulgent_dreaded_seal";
                    seal.CraftedItemID = "4896"; // Hib Crafting
                    seal.CraftingLevel = 1;
                    seal.CraftingSkillType = 15;
                    seal.MakeTemplated = true;
                    GameServer.Database.AddObject(seal);

                    if (log.IsDebugEnabled)
                        log.Debug("Added " + seal.Id_nb);
                }

                seal = GameServer.Database.FindObjectByKey<DbCraftedItem>("effulgent_dreaded_seal");
                if (seal == null)
                {
                    seal = new DbCraftedItem();
                    seal.AllowAdd = true;
                    seal.Id_nb = "effulgent_dreaded_seal";
                    seal.CraftedItemID = "4897"; // Hib Crafting
                    seal.CraftingLevel = 1;
                    seal.CraftingSkillType = 15;
                    seal.MakeTemplated = true;
                    GameServer.Database.AddObject(seal);

                    if (log.IsDebugEnabled)
                        log.Debug("Added " + seal.Id_nb);
                }
                #endregion Alb Crafting
            #region Mid Crafting
                // add to Crafted Item Table
                seal = GameServer.Database.FindObjectByKey<DbCraftedItem>("lambent_dreaded_seal");
                if (seal == null)
                {
                    seal = new DbCraftedItem();
                    seal.AllowAdd = true;
                    seal.Id_nb = "lambent_dreaded_seal";
                    seal.CraftedItemID = "11834"; // Hib Crafting
                    seal.CraftingLevel = 1;
                    seal.CraftingSkillType = 15;
                    seal.MakeTemplated = true;
                    GameServer.Database.AddObject(seal);

                    if (log.IsDebugEnabled)
                        log.Debug("Added " + seal.Id_nb);
                }

                seal = GameServer.Database.FindObjectByKey<DbCraftedItem>("lambent_dreaded_seal2");
                if (seal == null)
                {
                    seal = new DbCraftedItem();
                    seal.AllowAdd = true;
                    seal.Id_nb = "lambent_dreaded_seal2";
                    seal.CraftedItemID = "11835"; // Hib Crafting
                    seal.CraftingLevel = 1;
                    seal.CraftingSkillType = 15;
                    seal.MakeTemplated = true;
                    GameServer.Database.AddObject(seal);

                    if (log.IsDebugEnabled)
                        log.Debug("Added " + seal.Id_nb);
                }

                seal = GameServer.Database.FindObjectByKey<DbCraftedItem>("fulgent_dreaded_seal");
                if (seal == null)
                {
                    seal = new DbCraftedItem();
                    seal.AllowAdd = true;
                    seal.Id_nb = "fulgent_dreaded_seal";
                    seal.CraftedItemID = "11836"; // Hib Crafting
                    seal.CraftingLevel = 1;
                    seal.CraftingSkillType = 15;
                    seal.MakeTemplated = true;
                    GameServer.Database.AddObject(seal);

                    if (log.IsDebugEnabled)
                        log.Debug("Added " + seal.Id_nb);
                }

                seal = GameServer.Database.FindObjectByKey<DbCraftedItem>("effulgent_dreaded_seal");
                if (seal == null)
                {
                    seal = new DbCraftedItem();
                    seal.AllowAdd = true;
                    seal.Id_nb = "effulgent_dreaded_seal";
                    seal.CraftedItemID = "11837"; // Hib Crafting
                    seal.CraftingLevel = 1;
                    seal.CraftingSkillType = 15;
                    seal.MakeTemplated = true;
                    GameServer.Database.AddObject(seal);

                    if (log.IsDebugEnabled)
                        log.Debug("Added " + seal.Id_nb);
                }
                #endregion Mid Crafting
            #region Hib Crafting 
                // add to Crafted Item Table
                seal = GameServer.Database.FindObjectByKey<DbCraftedItem>("lambent_dreaded_seal");
                if (seal == null)
                {
                    seal = new DbCraftedItem();
                    seal.AllowAdd = true;
                    seal.Id_nb = "lambent_dreaded_seal";
                    seal.CraftedItemID = "16564"; // Hib Crafting
                    seal.CraftingLevel = 1;
                    seal.CraftingSkillType = 15;
                    seal.MakeTemplated = true;
                    GameServer.Database.AddObject(seal);

                    if (log.IsDebugEnabled)
                        log.Debug("Added " + seal.Id_nb);
                }

                seal = GameServer.Database.FindObjectByKey<DbCraftedItem>("lambent_dreaded_seal2");
                if (seal == null)
                {
                    seal = new DbCraftedItem();
                    seal.AllowAdd = true;
                    seal.Id_nb = "lambent_dreaded_seal2";
                    seal.CraftedItemID = "16565"; // Hib Crafting
                    seal.CraftingLevel = 1;
                    seal.CraftingSkillType = 15;
                    seal.MakeTemplated = true;
                    GameServer.Database.AddObject(seal);

                    if (log.IsDebugEnabled)
                        log.Debug("Added " + seal.Id_nb);
                }

                seal = GameServer.Database.FindObjectByKey<DbCraftedItem>("fulgent_dreaded_seal");
                if (seal == null)
                {
                    seal = new DbCraftedItem();
                    seal.AllowAdd = true;
                    seal.Id_nb = "fulgent_dreaded_seal";
                    seal.CraftedItemID = "16566"; // Hib Crafting
                    seal.CraftingLevel = 1;
                    seal.CraftingSkillType = 15;
                    seal.MakeTemplated = true;
                    GameServer.Database.AddObject(seal);

                    if (log.IsDebugEnabled)
                        log.Debug("Added " + seal.Id_nb);
                }

                seal = GameServer.Database.FindObjectByKey<DbCraftedItem>("effulgent_dreaded_seal");
                if (seal == null)
                {
                    seal = new DbCraftedItem();
                    seal.AllowAdd = true;
                    seal.Id_nb = "effulgent_dreaded_seal";
                    seal.CraftedItemID = "16567"; // Hib Crafting
                    seal.CraftingLevel = 1;
                    seal.CraftingSkillType = 15;
                    seal.MakeTemplated = true;
                    GameServer.Database.AddObject(seal);

                    if (log.IsDebugEnabled)
                        log.Debug("Added " + seal.Id_nb);
                }
                #endregion Hib Crafting
            #region All Realm Crafted X
                // add to Crafted X Item Table
                DbCraftedXItem sealx;
                sealx = GameServer.Database.FindObjectByKey<DbCraftedXItem>("lambent_dreaded_seal");
                if (sealx == null)
                {
                    sealx = new DbCraftedXItem();
                    sealx.AllowAdd = true;
                    sealx.CraftedItemId_nb = "lambent_dreaded_seal";
                    sealx.IngredientId_nb = "glowing_dreaded_seal";
                    sealx.Count = 10;

                    GameServer.Database.AddObject(sealx);

                    if (log.IsDebugEnabled)
                        log.Debug("Added " + sealx.CraftedItemId_nb);
                }

                sealx = GameServer.Database.FindObjectByKey<DbCraftedXItem>("lambent_dreaded_seal2");
                if (sealx == null)
                {
                    sealx = new DbCraftedXItem();
                    sealx.AllowAdd = true;
                    sealx.CraftedItemId_nb = "lambent_dreaded_seal2";
                    sealx.IngredientId_nb = "sanguine_dreaded_seal";
                    sealx.Count = 10;

                    GameServer.Database.AddObject(sealx);

                    if (log.IsDebugEnabled)
                        log.Debug("Added " + sealx.CraftedItemId_nb);
                }

                sealx = GameServer.Database.FindObjectByKey<DbCraftedXItem>("fulgent_dreaded_seal");
                if (sealx == null)
                {
                    sealx = new DbCraftedXItem();
                    sealx.AllowAdd = true;
                    sealx.CraftedItemId_nb = "fulgent_dreaded_seal";
                    sealx.IngredientId_nb = "lambent_dreaded_seal";
                    sealx.Count = 5;

                    GameServer.Database.AddObject(sealx);

                    if (log.IsDebugEnabled)
                        log.Debug("Added " + sealx.CraftedItemId_nb);
                }

                sealx = GameServer.Database.FindObjectByKey<DbCraftedXItem>("effulgent_dreaded_seal");
                if (sealx == null)
                {
                    sealx = new DbCraftedXItem();
                    sealx.AllowAdd = true;
                    sealx.CraftedItemId_nb = "effulgent_dreaded_seal";
                    sealx.IngredientId_nb = "fulgent_dreaded_seal";
                    sealx.Count = 5;

                    GameServer.Database.AddObject(sealx);

                    if (log.IsDebugEnabled)
                        log.Debug("Added " + sealx.CraftedItemId_nb);
                }

            }
            #endregion All Realm Crafted X

        #endregion Add Seals to Database

        private void SendReply(GamePlayer target, string msg)
        {
            target.Out.SendMessage(msg, eChatType.CT_System, eChatLoc.CL_ChatWindow);
        }

        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                return false;
            player.Out.SendMessage("Hand me any Dreaded Seal and I'll give you the appropriate realm points!.", 
                eChatType.CT_Say, eChatLoc.CL_ChatWindow);
            return true;
        }

        public override bool ReceiveItem(GameLiving source, DbInventoryItem item)
        {
            GamePlayer player = source as GamePlayer;
            int Level = player.Level;
            long currentrps = player.RealmPoints;
            long maxrps = 66181501;

            if (GetDistanceTo(player) > WorldMgr.INTERACT_DISTANCE)
            {
                ((GamePlayer)source).Out.SendMessage("You are too far away to give anything to me " 
                    + player.Name + ". Come a little closer.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (player != null && item != null && currentrps < maxrps && item.Id_nb == "glowing_dreaded_seal" 
                || item.Id_nb == "sanguine_dreaded_seal"
                || item.Id_nb == "lambent_dreaded_seal" 
                || item.Id_nb == "lambent_dreaded_seal2"  
                || item.Id_nb == "fulgent_dreaded_seal" 
                || item.Id_nb == "effulgent_dreaded_seal")
            {
                m_count = item.Count;
                    if  (Level <= 20)
                    {
                        ((GamePlayer)source).Out.SendMessage("You are too young yet to make use of these items "
                            + player.Name + ". Come back in " + (21 - Level) + " levels.", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
                        return false;
                    }
                    else if (Level > 20 & Level < 26)
                    {
                        amount += (item.Price / 150) * m_count; // At level 21 to 25 you get 20 Realm Points per set of seals.
                        player.GainBountyPoints(1);             // Force a +1 BP gain
                    }
                    else if (Level > 25 & Level < 31)
                    {
                        amount += (item.Price / 100) * m_count; // At level 26 to 30 you get 30 Realm Points per set of seals.
                        player.GainBountyPoints(2);             // Force a +2 BP gain
                    }
                    else if (Level > 30 & Level < 36)
                    {
                        amount += (item.Price / 60) * m_count; // At level 31 to 35 you get 50 Realm Points per set of seals.
                        player.GainBountyPoints(3);            // Force a +3 BP gain
                    }
                    else if (Level > 35 & Level < 41)
                    {
                        amount += (item.Price / 10) * m_count; // At level 36 to 40 you get 300 Realm Points per set of seals.
                    }
                    else if (Level > 40 & Level < 46)
                    {
                        amount += (item.Price / 4) * m_count; // At level 41 to 45 you get 700 Realm Points per set of seals.
                    }
                    else if (Level > 45 & Level < 50)
                    {
                        amount += (item.Price / 2) * m_count; // At level 46 to 49 you get 1500 Realm Points per set of seals.
                    }
                    else if (Level > 49)
                    {
                        amount += item.Price * m_count; // At level 50 you get 3000 Realm Points per set of seals.
                    }
                
                if (amount + currentrps > maxrps) 
                {
                    amount = maxrps - currentrps; // only give enough realm points to reach max
                }
                
                player.GainRealmPoints(amount);
                if (Level > 35) {player.GainBountyPoints(amount / 55);} // Only BP+ those of 36+ to prevent double BP gains
                player.Inventory.RemoveItem(item);
                player.Out.SendUpdatePoints();
                amount = 0;
                m_count = 0;
                currentrps = 0;
                return base.ReceiveItem(source, item);
            }

            ((GamePlayer)source).Out.SendMessage("I am not interested in that item, come back with something useful "
                + player.Name + ".", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
            return false;
        }
    }
}