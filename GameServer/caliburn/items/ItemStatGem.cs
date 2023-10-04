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
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using DOL.Language;
using DOL.GS.PacketHandler;
using DOL.Database;
using DOL.GS.Spells;
using log4net;
using System.Linq;

namespace DOL.GS
{
    /// <summary>
    /// This class represents a relic in a players inventory
    /// </summary>
    public class ItemStatGem : GameInventoryItem
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ItemStatGem()
            : base()
        {
        }
        public ItemStatGem(DbItemTemplate template)
            : base(template)
        {
        }

        public ItemStatGem(DbItemUnique template)
            : base(template)
        {
        }

        public ItemStatGem(DbInventoryItem item)
            : base(item)
        {
            OwnerID = item.OwnerID;
            ObjectId = item.ObjectId;
        }

        public override bool Combine(GamePlayer player, DbInventoryItem targetItem)
        {

            if (true)
            {
                List<(eProperty, int)> statGemProperties = new()
                {
                    ((eProperty)this.Bonus1Type, this.Bonus1),
                    ((eProperty)this.Bonus2Type, this.Bonus2),
                    ((eProperty)this.Bonus3Type, this.Bonus3),
                    ((eProperty)this.Bonus4Type, this.Bonus4),
                    ((eProperty)this.Bonus5Type, this.Bonus5),
                    ((eProperty)this.Bonus6Type, this.Bonus6),
                    ((eProperty)this.Bonus7Type, this.Bonus7),
                    ((eProperty)this.Bonus8Type, this.Bonus8),
                    ((eProperty)this.Bonus9Type, this.Bonus9),
                    ((eProperty)this.Bonus10Type, this.Bonus10),
                };


                List<(eProperty, int)> targetItemProperties = new()
                {
                    ((eProperty)targetItem.Bonus1Type, targetItem.Bonus1),
                    ((eProperty)targetItem.Bonus2Type, targetItem.Bonus2),
                    ((eProperty)targetItem.Bonus3Type, targetItem.Bonus3),
                    ((eProperty)targetItem.Bonus4Type, targetItem.Bonus4),
                    ((eProperty)targetItem.Bonus5Type, targetItem.Bonus5),
                    ((eProperty)targetItem.Bonus6Type, targetItem.Bonus6),
                    ((eProperty)targetItem.Bonus7Type, targetItem.Bonus7),
                    ((eProperty)targetItem.Bonus8Type, targetItem.Bonus8),
                    ((eProperty)targetItem.Bonus9Type, targetItem.Bonus9),
                    ((eProperty)targetItem.Bonus10Type, targetItem.Bonus10),
                };

                statGemProperties = statGemProperties.Where(A => A.Item2 > 0).ToList();
                var targetItemStatSockets = targetItemProperties.Where(A => A.Item1 == eProperty.Socket_Stat).ToList();
                if (targetItemStatSockets.Count == 0)
                {
                    player.Out.SendMessage($"{targetItem.Name} has no stat sockets!", eChatType.CT_Advise, eChatLoc.CL_ChatWindow);
                    return false;
                }
                if (targetItemStatSockets.Count < statGemProperties.Count)
                {
                    player.Out.SendMessage($"{targetItem.Name} has {targetItemStatSockets.Count} sockets but this gem has {statGemProperties.Count} stats!", eChatType.CT_Advise, eChatLoc.CL_ChatWindow);
                    return false;
                }

                DbItemUnique unique = new DbItemUnique(targetItem.Template);

                if (statGemProperties.Count == 0)
                {
                    player.Out.SendMessage($"{this.Name} has no stats!", eChatType.CT_Advise, eChatLoc.CL_ChatWindow);
                    return false;

                }

                foreach (var item in statGemProperties)
                {
                    if (unique.Bonus1Type == (int)eProperty.Socket_Stat)
                    {
                        unique.Bonus1Type = (int)item.Item1;
                        unique.Bonus1 = item.Item2;
                    }
                    else if (unique.Bonus2Type == (int)eProperty.Socket_Stat)
                    {
                        unique.Bonus2Type = (int)item.Item1;
                        unique.Bonus2 = item.Item2;
                    }
                    else if (unique.Bonus3Type == (int)eProperty.Socket_Stat)
                    {
                        unique.Bonus3Type = (int)item.Item1;
                        unique.Bonus3 = item.Item2;
                    }
                    else if (unique.Bonus4Type == (int)eProperty.Socket_Stat)
                    {
                        unique.Bonus4Type = (int)item.Item1;
                        unique.Bonus4 = item.Item2;
                    }
                    else if (unique.Bonus5Type == (int)eProperty.Socket_Stat)
                    {
                        unique.Bonus5Type = (int)item.Item1;
                        unique.Bonus5 = item.Item2;
                    }
                    else if (unique.Bonus6Type == (int)eProperty.Socket_Stat)
                    {
                        unique.Bonus6Type = (int)item.Item1;
                        unique.Bonus6 = item.Item2;
                    }
                    else if (unique.Bonus7Type == (int)eProperty.Socket_Stat)
                    {
                        unique.Bonus7Type = (int)item.Item1;
                        unique.Bonus7 = item.Item2;
                    }
                    else if (unique.Bonus8Type == (int)eProperty.Socket_Stat)
                    {
                        unique.Bonus8Type = (int)item.Item1;
                        unique.Bonus8 = item.Item2;
                    }
                    else if (unique.Bonus9Type == (int)eProperty.Socket_Stat)
                    {
                        unique.Bonus9Type = (int)item.Item1;
                        unique.Bonus9 = item.Item2;
                    }
                    else if (unique.Bonus10Type == (int)eProperty.Socket_Stat)
                    {
                        unique.Bonus10Type = (int)item.Item1;
                        unique.Bonus10 = item.Item2;
                    }
                }


                GameServer.Database.AddObject(unique);
                player.Inventory.RemoveItem(targetItem);
                player.Inventory.RemoveCountFromStack(this, 1);

                DbInventoryItem newInventoryItem = GameInventoryItem.Create(unique as DbItemTemplate);
                if (targetItem.IsCrafted)
                    newInventoryItem.IsCrafted = true;
                if (targetItem.Creator != "")
                    newInventoryItem.Creator = targetItem.Creator;


                newInventoryItem.Count = 1;

                player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, newInventoryItem);
                player.Out.SendInventoryItemsUpdate(new DbInventoryItem[] { newInventoryItem });

                player.SaveIntoDatabase();

                player.Out.SendMessage($"Your {targetItem.Name} has been upgraded!", eChatType.CT_Advise, eChatLoc.CL_ChatWindow);


                if (statGemProperties.Count == 1) 
                { 

                }



                return true;
            }
            return false;
        }
    }
}
