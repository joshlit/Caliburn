//using DOL.AI;
//using DOL.AI.Brain;
//using DOL.Database;
//using DOL.Events;
//using DOL.GS.Commands;
//using DOL.GS.Effects;
//using DOL.GS.Keeps;
//using DOL.GS.PacketHandler;
//using DOL.GS.Realm;
//using DOL.GS.RealmAbilities;
//using DOL.GS.SkillHandler;
//using DOL.GS.Spells;
//using DOL.GS.Styles;
//using DOL.GS.Utils;
//using DOL.Language;
//using log4net;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Numerics;
//using System.Reflection;
//using System.Security.Cryptography.X509Certificates;
//using System.Text;

//namespace DOL.GS.Scripts
//{
//    public class MimicNPC : GameNPC
//    {
//        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

//        public MimicSpec MimicSpec = new MimicSpec();

//        public MimicNPC(GameLiving owner, CharacterClass cClass, byte level = 0, Point3D position = null) : base()
//        {
//            if (position == null)
//            {
//                X = owner.X;
//                Y = owner.Y;
//                Z = owner.Z;
//                Heading = owner.Heading;
//            }
//            else
//            {
//                X = position.X;
//                Y = position.Y;
//                Z = position.Z;
//                Heading = 0x0;
//            }

//            CurrentRegionID = owner.CurrentRegionID;

//            Gender = Util.RandomBool() ? Gender = eGender.Female : Gender = eGender.Male;

//            MaxSpeedBase = 191;
//            m_followMinDist = 100;
//            m_followMaxDist = 3000;
//            Effectiveness = 1.0;

//            Inventory = new MimicNPCInventory(this);

//            CharacterClass = cClass;

//            SetRaceAndName(CharacterClass, owner);

//            if (level == 0)
//                level = owner.Level;

//            Level = 1;
//            Experience = 0;

//            SetLevel(level);

//            ChangeBaseStat(CharacterClass.PrimaryStat, 10);
//            ChangeBaseStat(CharacterClass.SecondaryStat, 10);
//            ChangeBaseStat(CharacterClass.TertiaryStat, 10);

//            log.Info("PrimaryStat: " + CharacterClass.PrimaryStat);
//            log.Info("SecondaryStat: " + CharacterClass.SecondaryStat);
//            log.Info("TertiaryStat: " + CharacterClass.TertiaryStat);

//            SetOwnBrain(new MimicBrain());

//            lock (m_respawnTimerLock)
//            {
//                if (m_respawnTimer != null)
//                {
//                    m_respawnTimer.Stop();
//                    m_respawnTimer = null;
//                }
//            }
//        }

//        public override bool Interact(GamePlayer player)
//        {
//            if (!base.Interact(player))
//                return false;

//            player.Out.SendMessage("[Group], [Spec], [Stats], [Hood], [Weapon], [Helm], [Torso], [Legs], [Arms], [Hands], [Boots], [Delete]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
//            return true;
//        }

//        public override bool WhisperReceive(GameLiving source, string str)
//        {
//            if (!base.WhisperReceive(source, str))
//                return false;

//            GamePlayer player = source as GamePlayer;

//            if (player == null)
//                return false;

//            switch (str)
//            {
//                case "Save":

//                case "Group":

//                if (player.Group == null)
//                {
//                    player.Group = new Group(player);
//                    player.Group.AddMember(player);
//                }
//                else
//                {
//                    if (player.Group.GetMembersInTheGroup().Contains(this))
//                        break;

//                    //((MimicBrain)Brain).GroupMembers = player.Group.GetMembersInTheGroup();
//                }

//                player.Group.AddMember(this);

//                //if (Brain is not MimicControlledBrain)
//                //    SetOwnBrain(new MimicControlledBrain(player));

//                break;

//                case "Stats":
//                SendReply(player, string.Format("Level: {0} Str: {1} Con: {2} Dex: {3} Qui: {4} Int: {5} Pie: {6} Emp: {7} Cha: {8} HP: {9} AF: {10}",
//                    Level,
//                    Strength,
//                    Constitution,
//                    Dexterity,
//                    Quickness,
//                    Intelligence,
//                    Piety,
//                    Empathy,
//                    Charisma,
//                    Health,
//                    EffectiveOverallAF)); break;

//                case "Spec":
//                {
//                    string message = string.Empty;

//                    var specs = GetSpecList();

//                    foreach (Specialization spec in specs)
//                    {
//                        message += spec.Name + ": " + spec.Level + " \n";
//                    }

//                    SendReply(player, message + DisplayedWeaponSkill);

//                    break;
//                }

//                case "Hood":
//                IsCloakHoodUp = !IsCloakHoodUp; BroadcastLivingEquipmentUpdate(); break;

//                case "Helm":
//                {
//                    InventoryItem item = Inventory.GetItem(eInventorySlot.HeadArmor);

//                    if (item != null)
//                    {
//                        Inventory.RemoveItem(item);

//                        if (!(player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item)))
//                        {
//                            player.SendPrivateMessage(player, "No room in your backpack you twat.");
//                            Inventory.AddItem((eInventorySlot)item.SlotPosition, item);
//                        }

//                        if (Inventory.AllItems.Contains(item))
//                            log.Debug("Still has that shit.");
//                    }

//                    BroadcastLivingEquipmentUpdate();
//                    break;
//                }

//                case "Torso":
//                {
//                    InventoryItem item = Inventory.GetItem(eInventorySlot.TorsoArmor);

//                    if (item != null)
//                    {
//                        Inventory.RemoveItem(item);

//                        if (!(player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item)))
//                        {
//                            player.SendPrivateMessage(player, "No room in your backpack you twat.");
//                            Inventory.AddItem((eInventorySlot)item.SlotPosition, item);
//                        }
//                    }

//                    BroadcastLivingEquipmentUpdate();
//                    break;
//                }

//                case "Legs":
//                {
//                    InventoryItem item = Inventory.GetItem(eInventorySlot.LegsArmor);

//                    if (item != null)
//                    {
//                        Inventory.RemoveItem(item);

//                        if (!(player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item)))
//                        {
//                            player.SendPrivateMessage(player, "No room in your backpack you twat.");
//                            Inventory.AddItem((eInventorySlot)item.SlotPosition, item);
//                        }
//                    }

//                    BroadcastLivingEquipmentUpdate();
//                    break;
//                }

//                case "Arms":
//                {
//                    InventoryItem item = Inventory.GetItem(eInventorySlot.ArmsArmor);

//                    if (item != null)
//                    {
//                        Inventory.RemoveItem(item);

//                        if (!(player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item)))
//                        {
//                            player.SendPrivateMessage(player, "No room in your backpack you twat.");
//                            Inventory.AddItem((eInventorySlot)item.SlotPosition, item);
//                        }
//                    }

//                    BroadcastLivingEquipmentUpdate();
//                    break;
//                }

//                case "Hands":
//                {
//                    InventoryItem item = Inventory.GetItem(eInventorySlot.HandsArmor);

//                    if (item != null)
//                    {
//                        Inventory.RemoveItem(item);

//                        if (!(player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item)))
//                        {
//                            player.SendPrivateMessage(player, "No room in your backpack you twat.");
//                            Inventory.AddItem((eInventorySlot)item.SlotPosition, item);
//                        }
//                    }

//                    BroadcastLivingEquipmentUpdate();
//                    break;
//                }

//                case "Boots":
//                {
//                    InventoryItem item = Inventory.GetItem(eInventorySlot.FeetArmor);

//                    if (item != null)
//                    {
//                        Inventory.RemoveItem(item);

//                        if (!(player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item)))
//                        {
//                            player.SendPrivateMessage(player, "No room in your backpack you twat.");
//                            Inventory.AddItem((eInventorySlot)item.SlotPosition, item);
//                        }
//                    }

//                    BroadcastLivingEquipmentUpdate();
//                    break;
//                }

//                case "Weapon":
//                {
//                    InventoryItem item = Inventory.GetItem(Inventory.FindFirstFullSlot(eInventorySlot.DistanceWeapon, eInventorySlot.RightHandWeapon));

//                    if (item != null)
//                    {
//                        Inventory.RemoveItem(item);

//                        if (!(player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item)))
//                        {
//                            player.SendPrivateMessage(player, "No room in your backpack you twat.");
//                            Inventory.AddItem((eInventorySlot)item.SlotPosition, item);
//                        }
//                    }

//                    BroadcastLivingEquipmentUpdate();
//                    break;
//                }

//                case "Delete":
//                {
//                    if (ControlledBody != null)
//                    {
//                        ControlledBody.Delete();
//                    }

//                    Delete();

//                    break;
//                }

//                default: break;
//            }

//            return true;
//        }

//        public void SetLevel(byte level)
//        {
//            Level = level;
//            GainExperience(eXPSource.Other, GetExperienceValueForLevel(level));

//            RespecAllLines();
//        }

//        public void SetRaceAndName(CharacterClass cClass, GameLiving owner)
//        {
//            List<PlayerRace> raceList = new List<PlayerRace>(cClass.EligibleRaces);

//            int removeIndex = -1;
//            for (int i = 0; i < raceList.Count; i++)
//            {
//                if (raceList[i] == PlayerRace.Korazh || raceList[i] == PlayerRace.Deifrang || raceList[i] == PlayerRace.Graoch)
//                {
//                    removeIndex = i;
//                    break;
//                }
//            }

//            if (removeIndex > -1)
//                raceList.RemoveAt(removeIndex);

//            PlayerRace playerRace = raceList[Util.Random(raceList.Count - 1)];

//            Race = (short)playerRace.ID;
//            Model = (ushort)playerRace.GetModel(Gender);
//            Size = (byte)Util.Random(45, 60);

//            Name = string.Format("{0} {1}", ((eRace)Race).ToString("F"), cClass.GetSalutation(Gender));

//            Dictionary<eStat, int> statDict;
//            GlobalConstants.STARTING_STATS_DICT.TryGetValue((eRace)Race, out statDict);

//            ChangeBaseStat(eStat.STR, (short)statDict[eStat.STR]);
//            ChangeBaseStat(eStat.CON, (short)statDict[eStat.CON]);
//            ChangeBaseStat(eStat.DEX, (short)statDict[eStat.DEX]);
//            ChangeBaseStat(eStat.QUI, (short)statDict[eStat.QUI]);
//            ChangeBaseStat(eStat.INT, (short)statDict[eStat.INT]);
//            ChangeBaseStat(eStat.PIE, (short)statDict[eStat.PIE]);
//            ChangeBaseStat(eStat.EMP, (short)statDict[eStat.EMP]);
//            ChangeBaseStat(eStat.CHR, (short)statDict[eStat.CHR]);

//            foreach (KeyValuePair<eRealm, List<eCharacterClass>> keyValuePair in GlobalConstants.STARTING_CLASSES_DICT)
//            {
//                if (keyValuePair.Value.Contains((eCharacterClass)CharacterClass.ID))
//                {
//                    Realm = keyValuePair.Key;
//                    log.Info("Realm: " + Realm);
//                    break;
//                }
//            }
//        }

//        public eObjectType GetObjectType(string weapon)
//        {
//            eObjectType objectType = 0;

//            switch (weapon)
//            {
//                case "Staff": objectType = eObjectType.Staff; break;

//                case "Slash": objectType = eObjectType.SlashingWeapon; break;
//                case "Thrust": objectType = eObjectType.ThrustWeapon; break;
//                case "Crush": objectType = eObjectType.CrushingWeapon; break;
//                case "Flexible": objectType = eObjectType.Flexible; break;
//                case "Polearm": objectType = eObjectType.PolearmWeapon; break;
//                case "Two Handed": objectType = eObjectType.TwoHandedWeapon; break;

//                case "Blades": objectType = eObjectType.Blades; break;
//                case "Piercing": objectType = eObjectType.Piercing; break;
//                case "Blunt": objectType = eObjectType.Blunt; break;
//                case "Large Weapons": objectType = eObjectType.LargeWeapons; break;
//                case "Celtic Spear": objectType = eObjectType.CelticSpear; break;
//                case "Scythe": objectType = eObjectType.Scythe; break;

//                case "Sword": objectType = eObjectType.Sword; break;
//                case "Axe": objectType = eObjectType.Axe; break;
//                case "Hammer": objectType = eObjectType.Hammer; break;
//                case "Hand to Hand": objectType = eObjectType.HandToHand; break;
//            }

//            return objectType;
//        }

//        public bool SetMeleeWeapon(string weapType, bool dualWield = false, eWeaponDamageType damageType = 0, eHand hand = eHand.None)
//        {
//            eObjectType objectType = GetObjectType(weapType);

//            int min = Math.Max(0, Level - 6);
//            int max = Math.Min(50, Level + 4);

//            IList<ItemTemplate> itemList;

//            itemList = GameServer.Database.SelectObjects<ItemTemplate>(DB.Column("Level").IsGreaterOrEqualTo(min).And(
//                                                                       DB.Column("Level").IsLessOrEqualTo(max).And(
//                                                                       DB.Column("Object_Type").IsEqualTo((int)objectType).And(
//                                                                       DB.Column("Realm").IsEqualTo((int)Realm)).And(
//                                                                       DB.Column("IsPickable").IsEqualTo(1)))));

//            if (dualWield)
//            {
//                List<ItemTemplate> leftHandItems = new List<ItemTemplate>();

//                foreach (ItemTemplate item in itemList)
//                {
//                    if (item.Item_Type == Slot.LEFTHAND)
//                        leftHandItems.Add(item);
//                }

//                if (leftHandItems.Count > 0)
//                    AddItem(leftHandItems[Util.Random(leftHandItems.Count - 1)]);
//            }

//            if (hand != eHand.None)
//            {
//                List<ItemTemplate> itemsToKeep = new List<ItemTemplate>();

//                foreach (ItemTemplate item in itemList)
//                {
//                    if (item.Hand == (int)hand)
//                        itemsToKeep.Add(item);
//                }

//                if (itemsToKeep.Count > 0)
//                {
//                    AddItem(itemsToKeep[Util.Random(itemsToKeep.Count - 1)]);

//                    return true;
//                }
//                else
//                    return false;
//            }

//            if (objectType != eObjectType.TwoHandedWeapon && objectType != eObjectType.PolearmWeapon && objectType != eObjectType.Staff)
//            {
//                foreach (ItemTemplate template in itemList)
//                {
//                    if (template.Item_Type == Slot.LEFTHAND)
//                        template.Item_Type = Slot.RIGHTHAND;
//                }
//            }

//            if ((int)damageType != 0)
//            {
//                List<ItemTemplate> itemsToKeep = new List<ItemTemplate>();

//                foreach (ItemTemplate item in itemList)
//                {
//                    if (item.Type_Damage == (int)damageType)
//                    {
//                        itemsToKeep.Add(item);
//                    }
//                }

//                if (itemsToKeep.Count > 0)
//                {
//                    ItemTemplate template = itemsToKeep[Util.Random(itemsToKeep.Count - 1)];
//                    return AddItem(template);
//                }

//                return false;
//            }
//            else if (itemList.Count > 0)
//            {
//                ItemTemplate template = itemList[Util.Random(itemList.Count - 1)];

//                return AddItem(template);
//            }
//            else
//            {
//                log.Debug("Could not find any fucking items for this peice of shit.");
//                return false;
//            }
//        }

//        public bool SetRangedWeapon(eObjectType weapType)
//        {
//            int min = Math.Max(1, Level - 6);
//            int max = Math.Min(51, Level + 3);

//            IList<ItemTemplate> itemList;
//            itemList = GameServer.Database.SelectObjects<ItemTemplate>(DB.Column("Level").IsGreaterOrEqualTo(min).And(
//                                                                       DB.Column("Level").IsLessOrEqualTo(max).And(
//                                                                       DB.Column("Object_Type").IsEqualTo((int)weapType).And(
//                                                                       DB.Column("Realm").IsEqualTo((int)Realm)).And(
//                                                                       DB.Column("IsPickable").IsEqualTo(1)))));

//            if (itemList.Count > 0)
//            {
//                AddItem(itemList[Util.Random(itemList.Count - 1)]);

//                return true;
//            }
//            else
//            {
//                log.Debug("No Ranged weapon.");
//                return false;
//            }
//        }

//        public void SetShield(int shieldSize)
//        {
//            int min = Math.Max(1, Level - 6);
//            int max = Math.Min(51, Level + 3);

//            IList<ItemTemplate> itemList;

//            itemList = GameServer.Database.SelectObjects<ItemTemplate>(DB.Column("Level").IsGreaterOrEqualTo(min).And(
//                                                                       DB.Column("Level").IsLessOrEqualTo(max).And(
//                                                                       DB.Column("Object_Type").IsEqualTo((int)eObjectType.Shield).And(
//                                                                       DB.Column("Realm").IsEqualTo((int)Realm)).And(
//                                                                       DB.Column("Type_Damage").IsEqualTo(shieldSize).And(
//                                                                       DB.Column("IsPickable").IsEqualTo(1))))));

//            ItemTemplate item = itemList[Util.Random(itemList.Count - 1)];

//            AddItem(item);
//        }

//        public void SetArmor(eObjectType armorType)
//        {
//            int min = Math.Max(1, Level - 6);
//            int max = Math.Min(51, Level + 3);

//            IList<ItemTemplate> itemList;

//            List<ItemTemplate> armsList = new List<ItemTemplate>();
//            List<ItemTemplate> handsList = new List<ItemTemplate>();
//            List<ItemTemplate> legsList = new List<ItemTemplate>();
//            List<ItemTemplate> feetList = new List<ItemTemplate>();
//            List<ItemTemplate> torsoList = new List<ItemTemplate>();
//            List<ItemTemplate> helmList = new List<ItemTemplate>();
//            log.Info("Realm: " + (int)Realm);
//            itemList = GameServer.Database.SelectObjects<ItemTemplate>(DB.Column("Level").IsGreaterOrEqualTo(min).And(
//                                                                       DB.Column("Level").IsLessOrEqualTo(max).And(
//                                                                       DB.Column("Object_Type").IsEqualTo((int)armorType).And(
//                                                                       DB.Column("Realm").IsEqualTo((int)Realm)).And(
//                                                                       DB.Column("IsPickable").IsEqualTo(1)))));

//            foreach (ItemTemplate template in itemList)
//            {
//                if (template.Item_Type == Slot.ARMS)
//                    armsList.Add(template);
//                else if (template.Item_Type == Slot.HANDS)
//                    handsList.Add(template);
//                else if (template.Item_Type == Slot.LEGS)
//                    legsList.Add(template);
//                else if (template.Item_Type == Slot.FEET)
//                    feetList.Add(template);
//                else if (template.Item_Type == Slot.TORSO)
//                    torsoList.Add(template);
//                else if (template.Item_Type == Slot.HELM)
//                    helmList.Add(template);
//            }

//            AddItem(armsList[Util.Random(armsList.Count - 1)]);
//            AddItem(handsList[Util.Random(handsList.Count - 1)]);
//            AddItem(legsList[Util.Random(legsList.Count - 1)]);
//            AddItem(feetList[Util.Random(feetList.Count - 1)]);
//            AddItem(torsoList[Util.Random(torsoList.Count - 1)]);
//            AddItem(helmList[Util.Random(helmList.Count - 1)]);
//        }

//        public void SetJewelry()
//        {
//            int min = Math.Max(1, Level - 30);
//            int max = Math.Min(51, Level + 1);

//            IList<ItemTemplate> itemList;
//            List<ItemTemplate> cloakList = new List<ItemTemplate>();
//            List<ItemTemplate> jewelryList = new List<ItemTemplate>();
//            List<ItemTemplate> ringList = new List<ItemTemplate>();
//            List<ItemTemplate> wristList = new List<ItemTemplate>();
//            List<ItemTemplate> neckList = new List<ItemTemplate>();
//            List<ItemTemplate> waistList = new List<ItemTemplate>();

//            itemList = GameServer.Database.SelectObjects<ItemTemplate>(DB.Column("Level").IsGreaterOrEqualTo(min).And(
//                                                                       DB.Column("Level").IsLessOrEqualTo(max).And(
//                                                                       DB.Column("Object_Type").IsEqualTo((int)eObjectType.Magical).And(
//                                                                       DB.Column("Realm").IsEqualTo((int)Realm)).And(
//                                                                       DB.Column("IsPickable").IsEqualTo(1)))));

//            foreach (ItemTemplate template in itemList)
//            {
//                if (template.Item_Type == Slot.CLOAK)
//                    cloakList.Add(template);
//                else if (template.Item_Type == Slot.JEWELRY)
//                    jewelryList.Add(template);
//                else if (template.Item_Type == Slot.LEFTRING || template.Item_Type == Slot.RIGHTRING)
//                    ringList.Add(template);
//                else if (template.Item_Type == Slot.LEFTWRIST || template.Item_Type == Slot.RIGHTWRIST)
//                    wristList.Add(template);
//                else if (template.Item_Type == Slot.NECK)
//                    neckList.Add(template);
//                else if (template.Item_Type == Slot.WAIST)
//                    waistList.Add(template);
//            }

//            List<List<ItemTemplate>> masterList = new List<List<ItemTemplate>>
//            {
//                cloakList,
//                jewelryList,
//                neckList,
//                waistList
//            };

//            foreach (List<ItemTemplate> list in masterList)
//            {
//                if (list.Count > 0)
//                {
//                    AddItem(list[Util.Random(list.Count - 1)]);
//                }
//            }

//            for (int i = 0; i < 2; i++)
//            {
//                if (ringList.Count > 0)
//                {
//                    AddItem(ringList[Util.Random(ringList.Count - 1)]);
//                }

//                if (wristList.Count > 0)
//                {
//                    AddItem(wristList[Util.Random(wristList.Count - 1)]);
//                }
//            }

//            if (Inventory.GetItem(eInventorySlot.Cloak) == null)
//            {
//                ItemTemplate cloak = GameServer.Database.FindObjectByKey<ItemTemplate>("cloak");
//                int color = Util.Random(500);
//                log.Debug("Color: " + color);
//                cloak.Color = color;
//                AddItem(cloak);
//            }

//            IsCloakHoodUp = Util.RandomBool();
//        }

//        public bool AddItem(ItemTemplate itemTemplate)
//        {
//            if (itemTemplate == null)
//            {
//                log.Debug("itemTemplate in AddItem is null");
//                return false;
//            }

//            InventoryItem item = GameInventoryItem.Create(itemTemplate);

//            if (item != null)
//            {
//                if (itemTemplate.Item_Type == Slot.LEFTRING || itemTemplate.Item_Type == Slot.RIGHTRING)
//                {
//                    return Inventory.AddItem(Inventory.FindFirstEmptySlot(eInventorySlot.LeftRing, eInventorySlot.RightRing), item);
//                }
//                else if (itemTemplate.Item_Type == Slot.LEFTWRIST || itemTemplate.Item_Type == Slot.RIGHTWRIST)
//                {
//                    return Inventory.AddItem(Inventory.FindFirstEmptySlot(eInventorySlot.LeftBracer, eInventorySlot.RightBracer), item);
//                }
//                else
//                    return Inventory.AddItem((eInventorySlot)item.Item_Type, item);
//            }
//            else
//            {
//                log.Debug("Item failed to be created.");
//                return false;
//            }
//        }

//        private void SendReply(GamePlayer player, string msg)
//        {
//            player.Out.SendMessage(msg, eChatType.CT_System, eChatLoc.CL_PopupWindow);
//        }

//        protected override int RespawnTimerCallback(RegionTimer respawnTimer)
//        {
//            return 0;
//        }

//        /// <summary>
//        /// Weaponskill thats shown to the player
//        /// </summary>
//        public virtual int DisplayedWeaponSkill
//        {
//            get
//            {
//                int itemBonus = WeaponSpecLevel(AttackWeapon) - WeaponBaseSpecLevel(AttackWeapon) - RealmLevel / 10;
//                double m = 0.56 + itemBonus / 70.0;
//                double weaponSpec = WeaponSpecLevel(AttackWeapon) + itemBonus * m;
//                return (int)(GetWeaponSkill(AttackWeapon) * (1.00 + weaponSpec * 0.01));
//            }
//        }

//        #region Misc

//        /// <summary>
//        /// Gets or sets the BindYpos for this player
//        /// (delegate to property in DBCharacter)
//        /// </summary>
//        ///
//        public override bool ReceiveItem(GameLiving source, InventoryItem item)
//        {
//            if (source == null || item == null) return false;

//            InventoryItem oldItem = Inventory.GetItem((eInventorySlot)item.SlotPosition);

//            if (oldItem != null)
//            {
//                Inventory.RemoveItem(oldItem);

//                if (!(source.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item)))
//                {
//                    Inventory.AddItem((eInventorySlot)oldItem.SlotPosition, oldItem);
//                }
//                else
//                {
//                    Inventory.AddItem((eInventorySlot)item.SlotPosition, item);
//                    BroadcastLivingEquipmentUpdate();
//                    return true;
//                }
//            }
//            else
//            {
//                Inventory.AddItem((eInventorySlot)item.SlotPosition, item);
//                BroadcastLivingEquipmentUpdate();
//                return true;
//            }

//            return false;
//        }

//        public byte DeathCount
//        {
//            get { return DBCharacter != null ? DBCharacter.DeathCount : (byte)0; }
//            set { if (DBCharacter != null) DBCharacter.DeathCount = value; }
//        }

//        /// <summary>
//        /// Gets or sets the Database MaxEndurance for this player
//        /// (delegate to property in DBCharacter)
//        /// </summary>
//        public int DBMaxEndurance
//        {
//            get { return DBCharacter != null ? DBCharacter.MaxEndurance : 100; }
//            set { if (DBCharacter != null) DBCharacter.MaxEndurance = value; }
//        }

//        /// <summary>
//        /// Gets or sets the gain XP flag for this player
//        /// (delegate to property in DBCharacter)
//        /// </summary>
//        public bool GainXP
//        {
//            get { return DBCharacter != null ? DBCharacter.GainXP : true; }
//            set { if (DBCharacter != null) DBCharacter.GainXP = value; }
//        }

//        /// <summary>
//        /// Gets or sets the gain RP flag for this player
//        /// (delegate to property in DBCharacter)
//        /// </summary>
//        public bool GainRP
//        {
//            get { return (DBCharacter != null ? DBCharacter.GainRP : true); }
//            set { if (DBCharacter != null) DBCharacter.GainRP = value; }
//        }

//        #endregion Misc

//        #region Health/Mana/Endurance/Regeneration

//        /// <summary>
//        /// Change a stat value
//        /// (delegate to PlayerCharacter)
//        /// </summary>
//        /// <param name="stat">The stat to change</param>
//        /// <param name="val">The new value</param>
//        public override void ChangeBaseStat(eStat stat, short val)
//        {
//            int oldstat = GetBaseStat(stat);
//            m_charStat[stat - eStat._First] += val;
//            int newstat = GetBaseStat(stat);
//            ////DOLCharacters character = DBCharacter; // to call it only once, if in future there will be some special code to get the character
//            //Graveen: always positive and not null. This allows /player stats to substract values safely
//            if (newstat < 1)
//                newstat = 1;

//            if (oldstat != newstat)
//            {
//                switch (stat)
//                {
//                    case eStat.STR: Strength = (short)newstat; break;
//                    case eStat.DEX: Dexterity = (short)newstat; break;
//                    case eStat.CON: Constitution = (short)newstat; break;
//                    case eStat.QUI: Quickness = (short)newstat; break;
//                    case eStat.INT: Intelligence = (short)newstat; break;
//                    case eStat.PIE: Piety = (short)newstat; break;
//                    case eStat.EMP: Empathy = (short)newstat; break;
//                    case eStat.CHR: Charisma = (short)newstat; break;
//                }
//            }
//        }

//        /// <summary>
//        /// Gets player's constitution
//        /// </summary>
//        public override short Constitution
//        {
//            get { return (short)GetModified(eProperty.Constitution); }
//            set { m_charStat[eStat.CON - eStat._First] = value; }
//        }

//        /// <summary>
//        /// Gets player's dexterity
//        /// </summary>
//        public override short Dexterity
//        {
//            get { return (short)GetModified(eProperty.Dexterity); }
//            set { m_charStat[eStat.DEX - eStat._First] = value; }
//        }

//        /// <summary>
//        /// Gets player's strength
//        /// </summary>
//        public override short Strength
//        {
//            get { return (short)GetModified(eProperty.Strength); }
//            set { m_charStat[eStat.STR - eStat._First] = value; }
//        }

//        /// <summary>
//        /// Gets player's quickness
//        /// </summary>
//        public override short Quickness
//        {
//            get { return (short)GetModified(eProperty.Quickness); }
//            set { m_charStat[eStat.QUI - eStat._First] = value; }
//        }

//        /// <summary>
//        /// Gets player's intelligence
//        /// </summary>
//        public override short Intelligence
//        {
//            get { return (short)GetModified(eProperty.Intelligence); }
//            set { m_charStat[eStat.INT - eStat._First] = value; }
//        }

//        /// <summary>
//        /// Gets player's piety
//        /// </summary>
//        public override short Piety
//        {
//            get { return (short)GetModified(eProperty.Piety); }
//            set { m_charStat[eStat.PIE - eStat._First] = value; }
//        }

//        /// <summary>
//        /// Gets player's empathy
//        /// </summary>
//        public override short Empathy
//        {
//            get { return (short)GetModified(eProperty.Empathy); }
//            set { m_charStat[eStat.EMP - eStat._First] = value; }
//        }

//        /// <summary>
//        /// Gets player's charisma
//        /// </summary>
//        public override short Charisma
//        {
//            get { return (short)GetModified(eProperty.Charisma); }
//            set { m_charStat[eStat.CHR - eStat._First] = value; }
//        }

//        private int m_specpoints;

//        /// <summary>
//        /// Gets/sets the mimics specpoints
//        /// </summary>
//        public int SpecPoints
//        {
//            get { return m_specpoints; }
//            set { m_specpoints = value; }
//        }

//        /// <summary>
//        /// Gets/sets the player efficacy percent
//        /// (delegate to PlayerCharacter)
//        /// </summary>
//        public virtual int TotalConstitutionLostAtDeath
//        {
//            get { return DBCharacter != null ? DBCharacter.ConLostAtDeath : 0; }
//            set { if (DBCharacter != null) DBCharacter.ConLostAtDeath = value; }
//        }

//        /// <summary>
//        /// the moving state of this player
//        /// </summary>
//        public override bool IsMoving
//        {
//            get { return base.IsMoving || IsStrafing; }
//        }

//        /// <summary>
//        /// The sprint effect of this player
//        /// </summary>
//        protected SprintEffect m_sprintEffect = null;

//        /// <summary>
//        /// Gets sprinting flag
//        /// </summary>
//        public bool IsSprinting
//        {
//            get { return m_sprintEffect != null; }
//        }

//        /// <summary>
//        /// Change sprint state of this player
//        /// </summary>
//        /// <param name="state">new state</param>
//        /// <returns>sprint state after command</returns>
//        public virtual bool Sprint(bool state)
//        {
//            if (state == IsSprinting)
//                return state;

//            if (state)
//            {
//                // can't start sprinting with 10 endurance on 1.68 server
//                if (Endurance <= 10)
//                {
//                    return false;
//                }
//                if (IsStealthed)
//                {
//                    return false;
//                }
//                if (!IsAlive)
//                {
//                    return false;
//                }

//                m_sprintEffect = new SprintEffect();
//                m_sprintEffect.Start(this);
//                //Out.SendUpdateMaxSpeed();
//                return true;
//            }
//            else
//            {
//                m_sprintEffect.Stop();
//                m_sprintEffect = null;
//                //Out.SendUpdateMaxSpeed();
//                return false;
//            }
//        }

//        /// <summary>
//        /// The strafe state of this player
//        /// </summary>
//        protected bool m_strafing;

//        /// <summary>
//        /// Gets/sets the current strafing mode
//        /// </summary>
//        public override bool IsStrafing
//        {
//            set
//            {
//                m_strafing = value;
//                if (value)
//                {
//                    //OnPlayerMove();
//                }
//            }
//            get { return m_strafing; }
//        }

//        /// <summary>
//        /// Starts the health regeneration.
//        /// Overriden. No lazy timers for GamePlayers.
//        /// </summary>
//        public override void StartHealthRegeneration()
//        {
//            if (ObjectState != eObjectState.Active) return;
//            if (m_healthRegenerationTimer.IsAlive) return;
//            m_healthRegenerationTimer.Start(m_healthRegenerationPeriod);
//        }

//        /// <summary>
//        /// Starts the power regeneration.
//        /// Overriden. No lazy timers for GamePlayers.
//        /// </summary>
//        public override void StartPowerRegeneration()
//        {
//            if (ObjectState != eObjectState.Active) return;
//            if (m_powerRegenerationTimer.IsAlive) return;
//            m_powerRegenerationTimer.Start(m_powerRegenerationPeriod);
//        }

//        /// <summary>
//        /// Starts the endurance regeneration.
//        /// Overriden. No lazy timers for GamePlayers.
//        /// </summary>
//        public override void StartEnduranceRegeneration()
//        {
//            if (ObjectState != eObjectState.Active) return;
//            if (m_enduRegenerationTimer.IsAlive) return;
//            m_enduRegenerationTimer.Start(m_enduranceRegenerationPeriod);
//        }

//        /// <summary>
//        /// Stop the health regeneration.
//        /// Overriden. No lazy timers for GamePlayers.
//        /// </summary>
//        public override void StopHealthRegeneration()
//        {
//            if (m_healthRegenerationTimer == null) return;
//            m_healthRegenerationTimer.Stop();
//        }

//        /// <summary>
//        /// Stop the power regeneration.
//        /// Overriden. No lazy timers for GamePlayers.
//        /// </summary>
//        public override void StopPowerRegeneration()
//        {
//            if (m_powerRegenerationTimer == null) return;
//            m_powerRegenerationTimer.Stop();
//        }

//        /// <summary>
//        /// Stop the endurance regeneration.
//        /// Overriden. No lazy timers for GamePlayers.
//        /// </summary>
//        public override void StopEnduranceRegeneration()
//        {
//            if (m_enduRegenerationTimer == null) return;
//            m_enduRegenerationTimer.Stop();
//        }

//        /// <summary>
//        /// Override HealthRegenTimer because if we are not connected anymore
//        /// we DON'T regenerate health, even if we are not garbage collected yet!
//        /// </summary>
//        /// <param name="callingTimer">the timer</param>
//        /// <returns>the new time</returns>
//        protected override int HealthRegenerationTimerCallback(RegionTimer callingTimer)
//        {
//            if (Health < MaxHealth)
//            {
//                ChangeHealth(this, eHealthChangeType.Regenerate, GetModified(eProperty.HealthRegenerationRate));
//            }

//            #region PVP DAMAGE

//            if (DamageRvRMemory > 0)
//                DamageRvRMemory -= (long)Math.Max(GetModified(eProperty.HealthRegenerationRate), 0);

//            #endregion PVP DAMAGE

//            //If we are fully healed, we stop the timer
//            if (Health >= MaxHealth)
//            {
//                #region PVP DAMAGE

//                // Fully Regenerated, Set DamageRvRMemory to 0
//                if (DamageRvRMemory > 0)
//                    DamageRvRMemory = 0;

//                #endregion PVP DAMAGE

//                //We clean all damagedealers if we are fully healed,
//                //no special XP calculations need to be done
//                lock (m_xpGainers.SyncRoot)
//                {
//                    m_xpGainers.Clear();
//                }

//                return 0;
//            }

//            if (InCombat)
//            {
//                // in combat each tic is aprox 6 seconds - tolakram
//                return HealthRegenerationPeriod * 2;
//            }

//            //Heal at standard rate
//            return HealthRegenerationPeriod;
//        }

//        /// <summary>
//        /// Override PowerRegenTimer because if we are not connected anymore
//        /// we DON'T regenerate mana, even if we are not garbage collected yet!
//        /// </summary>
//        /// <param name="selfRegenerationTimer">the timer</param>
//        /// <returns>the new time</returns>
//        protected override int PowerRegenerationTimerCallback(RegionTimer selfRegenerationTimer)
//        {
//            int interval = base.PowerRegenerationTimerCallback(selfRegenerationTimer);

//            if (Group != null)
//                Group.UpdateMember(this, true, false);

//            return interval;
//        }

//        /// <summary>
//        /// Override EnduranceRegenTimer because if we are not connected anymore
//        /// we DON'T regenerate endurance, even if we are not garbage collected yet!
//        /// </summary>
//        /// <param name="selfRegenerationTimer">the timer</param>
//        /// <returns>the new time</returns>
//        protected override int EnduranceRegenerationTimerCallback(RegionTimer selfRegenerationTimer)
//        {
//            bool sprinting = IsSprinting;

//            if (Endurance < MaxEndurance || sprinting)
//            {
//                int regen = GetModified(eProperty.EnduranceRegenerationRate);  //default is 1
//                int endchant = GetModified(eProperty.FatigueConsumption);      //Pull chant/buff value

//                int longwind = 5;
//                if (sprinting && IsMoving)
//                {
//                    //TODO : cache LongWind level when char is loaded and on train ability
//                    LongWindAbility ra = GetAbility<LongWindAbility>();
//                    if (ra != null)
//                        longwind = 5 - (ra.GetAmountForLevel(CalculateSkillLevel(ra)) * 5 / 100);

//                    regen -= longwind;

//                    if (endchant > 1) regen = (int)Math.Ceiling(regen * endchant * 0.01);
//                    if (Endurance + regen > MaxEndurance - longwind)
//                    {
//                        regen -= (Endurance + regen) - (MaxEndurance - longwind);
//                    }
//                }

//                if (regen != 0)
//                {
//                    ChangeEndurance(this, eEnduranceChangeType.Regenerate, regen);
//                }
//            }
//            if (!sprinting)
//            {
//                if (Endurance >= MaxEndurance) return 0;
//            }
//            else
//            {
//                //long lastmove = TempProperties.getProperty<long>(PlayerPositionUpdateHandler.LASTMOVEMENTTICK);
//                //if ((lastmove > 0 && lastmove + 5000 < CurrentRegion.Time) //cancel sprint after 5sec without moving?
//                //	|| Endurance - 5 <= 0)
//                //	Sprint(false);
//            }

//            if (Group != null)
//                Group.UpdateMember(this, true, false);

//            return 500 + Util.Random(EnduranceRegenerationPeriod);
//        }

//        /// <summary>
//        /// Gets/sets the object health
//        /// </summary>
//        public override int Health
//        {
//            get { return DBCharacter != null ? DBCharacter.Health : base.Health; }
//            set
//            {
//                value = value.Clamp(0, MaxHealth);
//                //If it is already set, don't do anything
//                if (Health == value)
//                {
//                    base.Health = value; //needed to start regeneration
//                    return;
//                }

//                int oldPercent = HealthPercent;
//                if (DBCharacter != null)
//                    DBCharacter.Health = value;
//                base.Health = value;
//                if (oldPercent != HealthPercent)
//                {
//                    if (Group != null)
//                        Group.UpdateMember(this, false, false);
//                    //UpdatePlayerStatus();
//                }
//            }
//        }

//        /// <summary>
//        /// Calculates the maximum health for a specific playerlevel and constitution
//        /// </summary>
//        /// <param name="level">The level of the player</param>
//        /// <param name="constitution">The constitution of the player</param>
//        /// <returns></returns>
//        public virtual int CalculateMaxHealth(int level, int constitution)
//        {
//            constitution -= 50;
//            if (constitution < 0) constitution *= 2;

//            // hp1 : from level
//            // hp2 : from constitution
//            // hp3 : from champions level
//            // hp4 : from artifacts such Spear of Kings charge
//            int hp1 = CharacterClass.BaseHP * level;
//            int hp2 = hp1 * constitution / 10000;
//            int hp3 = 0;
//            //if (ChampionLevel >= 1)
//            //	hp3 = ServerProperties.Properties.HPS_PER_CHAMPIONLEVEL * ChampionLevel;
//            double hp4 = 20 + hp1 / 50 + hp2 + hp3;
//            if (GetModified(eProperty.ExtraHP) > 0)
//                hp4 += Math.Round(hp4 * (double)GetModified(eProperty.ExtraHP) / 100);

//            return Math.Max(1, (int)hp4);
//        }

//        public override byte HealthPercentGroupWindow
//        {
//            get
//            {
//                return HealthPercentGroupWindow;
//            }
//        }

//        /// <summary>
//        /// Calculate max mana for this player based on level and mana stat level
//        /// </summary>
//        /// <param name="level"></param>
//        /// <param name="manaStat"></param>
//        /// <returns></returns>
//        public virtual int CalculateMaxMana(int level, int manaStat)
//        {
//            int maxpower = 0;

//            //Special handling for Vampiirs:
//            /* There is no stat that affects the Vampiir's power pool or the damage done by its power based spells.
//			 * The Vampiir is not a focus based class like, say, an Enchanter.
//			 * The Vampiir is a lot more cut and dried than the typical casting class.
//			 * EDIT, 12/13/04 - I was told today that this answer is not entirely accurate.
//			 * While there is no stat that affects the damage dealt (in the way that intelligence or piety affects how much damage a more traditional caster can do),
//			 * the Vampiir's power pool capacity is intended to be increased as the Vampiir's strength increases.
//			 *
//			 * This means that strength ONLY affects a Vampiir's mana pool
//			 *
//			 * http://www.camelotherald.com/more/1913.shtml
//			 * Strength affects the amount of damage done by spells in all of the Vampiir's spell lines.
//			 * The amount of said affecting was recently increased slightly (fixing a bug), and that minor increase will go live in 1.74 next week.
//			 *
//			 * Strength ALSO affects the size of the power pool for a Vampiir sort of.
//			 * Your INNATE strength (the number of attribute points your character has for strength) has no effect at all.
//			 * Extra points added through ITEMS, however, does increase the size of your power pool.

//			 */
//            if (CharacterClass.ManaStat != eStat.UNDEFINED || CharacterClass.ID == (int)eCharacterClass.Vampiir)
//            {
//                maxpower = Math.Max(5, (level * 5) + (manaStat - 50));
//            }
//            //else if (CharacterClass.ManaStat == eStat.UNDEFINED && Champion && ChampionLevel > 0)
//            //{
//            //	maxpower = 100; // This is a guess, need feedback
//            //}

//            if (maxpower < 0)
//                maxpower = 0;

//            return maxpower;
//        }

//        /// <summary>
//        /// Gets/sets the object mana
//        /// </summary>
//        public override int Mana
//        {
//            get { return DBCharacter != null ? DBCharacter.Mana : base.Mana; }
//            set
//            {
//                value = Math.Min(value, MaxMana);
//                value = Math.Max(value, 0);
//                //If it is already set, don't do anything
//                if (Mana == value)
//                {
//                    base.Mana = value; //needed to start regeneration
//                    return;
//                }
//                int oldPercent = ManaPercent;
//                base.Mana = value;
//                if (DBCharacter != null)
//                    DBCharacter.Mana = value;
//                if (oldPercent != ManaPercent)
//                {
//                    if (Group != null)
//                        Group.UpdateMember(this, false, false);
//                    //UpdatePlayerStatus();
//                }
//            }
//        }

//        /// <summary>
//        /// Gets/sets the object max mana
//        /// </summary>
//        public override int MaxMana
//        {
//            get { return GetModified(eProperty.MaxMana); }
//        }

//        /// <summary>
//        /// Gets/sets the object endurance
//        /// </summary>
//        public override int Endurance
//        {
//            get { return DBCharacter != null ? DBCharacter.Endurance : base.Endurance; }
//            set
//            {
//                value = Math.Min(value, MaxEndurance);
//                value = Math.Max(value, 0);
//                //If it is already set, don't do anything
//                if (Endurance == value)
//                {
//                    base.Endurance = value; //needed to start regeneration
//                    return;
//                }
//                else if (IsAlive && value < MaxEndurance)
//                    StartEnduranceRegeneration();
//                int oldPercent = EndurancePercent;
//                base.Endurance = value;
//                if (DBCharacter != null)
//                    DBCharacter.Endurance = value;
//                if (oldPercent != EndurancePercent)
//                {
//                    //ogre: 1.69+ endurance is displayed on group window
//                    if (Group != null)
//                        Group.UpdateMember(this, false, false);
//                    //end ogre
//                    //UpdatePlayerStatus();
//                }
//            }
//        }

//        /// <summary>
//        /// Gets/sets the objects maximum endurance
//        /// </summary>
//        public override int MaxEndurance
//        {
//            get { return GetModified(eProperty.Fatigue); }
//            set
//            {
//                //If it is already set, don't do anything
//                if (MaxEndurance == value)
//                    return;
//                base.MaxEndurance = value;
//                DBMaxEndurance = value;
//                //UpdatePlayerStatus();
//            }
//        }

//        /// <summary>
//        /// Gets the concentration left
//        /// </summary>
//        public override int Concentration
//        {
//            get { return MaxConcentration - ConcentrationEffects.UsedConcentration; }
//        }

//        /// <summary>
//        /// Gets the maximum concentration for this player
//        /// </summary>
//        public override int MaxConcentration
//        {
//            get { return GetModified(eProperty.MaxConcentration); }
//        }

//        #endregion Health/Mana/Endurance/Regeneration

//        #region Class/Race

//        /// <summary>
//        /// This holds the character this player is
//        /// based on!
//        /// (renamed and private, cause if derive is needed overwrite PlayerCharacter)
//        /// </summary>
//        protected DOLCharacters m_dbCharacter;

//        /// <summary>
//        /// The character the player is based on
//        /// </summary>
//        internal DOLCharacters DBCharacter
//        {
//            get { return m_dbCharacter; }
//        }

//        /// <summary>
//        /// Gets/sets the player's race name
//        /// </summary>
//        public virtual string RaceName
//        {
//            get { return string.Format("!{0} - {1}!", ((eRace)Race).ToString("F"), Gender.ToString("F")); }
//        }

//        /// <summary>
//        /// Gets or sets this player's race id
//        /// (delegate to DBCharacter)
//        /// </summary>
//        //public override short Race
//        //{
//        //	get { return (short)(DBCharacter != null ? DBCharacter.Race : 0); }
//        //	set { if (DBCharacter != null) DBCharacter.Race = value; }
//        //}

//        public bool SetCharacterClass(CharacterClass charClass)
//        {
//            if (charClass.Equals(GS.CharacterClass.None))
//            {
//                if (log.IsErrorEnabled) log.ErrorFormat($"Unknown CharacterClass has been set for Player {Name}.");
//                return false;
//            }

//            // No Bainsheeeessss
//            //if (charClass.Equals(GS.CharacterClass.Bainshee)) new BainsheeMorphEffect(this);

//            CharacterClass = charClass;
//            DBCharacter.Class = CharacterClass.ID;

//            if (Group != null)
//            {
//                Group.UpdateMember(this, false, true);
//            }

//            return true;
//        }

//        /// <summary>
//        /// Hold all player face custom attibutes
//        /// </summary>
//        protected byte[] m_customFaceAttributes = new byte[(int)eCharFacePart._Last + 1];

//        /// <summary>
//        /// Get the character face attribute you want
//        /// </summary>
//        /// <param name="part">face part</param>
//        /// <returns>attribute</returns>
//        public byte GetFaceAttribute(eCharFacePart part)
//        {
//            return m_customFaceAttributes[(int)part];
//        }

//        #endregion Class/Race

//        #region Realm-/Region-/Bount-/Skillpoints...

//        /// <summary>
//        /// Gets/sets player bounty points
//        /// (delegate to PlayerCharacter)
//        /// </summary>
//        public virtual long BountyPoints
//        {
//            get { return DBCharacter != null ? DBCharacter.BountyPoints : 0; }
//            set { if (DBCharacter != null) DBCharacter.BountyPoints = value; }
//        }

//        /// <summary>
//        /// Gets/sets player realm points
//        /// (delegate to PlayerCharacter)
//        /// </summary>
//        public virtual long RealmPoints
//        {
//            get { return DBCharacter != null ? DBCharacter.RealmPoints : 0; }
//            set { if (DBCharacter != null) DBCharacter.RealmPoints = value; }
//        }

//        /// <summary>
//        /// Gets/sets player skill specialty points
//        /// </summary>
//        //public virtual int SkillSpecialtyPoints
//        //{
//        //	get { return VerifySpecPoints(); }
//        //}

//        /// <summary>
//        /// Gets/sets player realm specialty points
//        /// </summary>
//        //public virtual int RealmSpecialtyPoints
//        //{
//        //	get
//        //	{
//        //		return GameServer.ServerRules.GetPlayerRealmPointsTotal(this)
//        //		  - GetRealmAbilities().Where(ab => !(ab is RR5RealmAbility))
//        //		  .Sum(ab => Enumerable.Range(0, ab.Level).Sum(i => ab.CostForUpgrade(i)));
//        //	}
//        //}

//        /// <summary>
//        /// Gets/sets player realm rank
//        /// </summary>
//        public virtual int RealmLevel
//        {
//            get { return DBCharacter != null ? DBCharacter.RealmLevel : 0; }
//            set
//            {
//                if (DBCharacter != null)
//                    DBCharacter.RealmLevel = value;
//                //CharacterClass.OnRealmLevelUp(this);
//            }
//        }

//        /// <summary>
//        /// Returns the translated realm rank title of the player.
//        /// </summary>
//        /// <param name="language"></param>
//        /// <returns></returns>
//        public virtual string RealmRankTitle(string language)
//        {
//            string translationId = string.Empty;

//            if (Realm != eRealm.None && Realm != eRealm.Door)
//            {
//                int RR = 0;

//                if (RealmLevel > 1)
//                    RR = RealmLevel / 10 + 1;

//                string realm = string.Empty;
//                if (Realm == eRealm.Albion)
//                    realm = "Albion";
//                else if (Realm == eRealm.Midgard)
//                    realm = "Midgard";
//                else
//                    realm = "Hibernia";

//                string gender = Gender == eGender.Female ? "Female" : "Male";

//                translationId = string.Format("{0}.RR{1}.{2}", realm, RR, gender);
//            }
//            else
//            {
//                translationId = "UnknownRealm";
//            }

//            string translation;
//            if (!LanguageMgr.TryGetTranslation(out translation, language, string.Format("GamePlayer.RealmTitle.{0}", translationId)))
//                translation = RealmTitle;

//            return translation;
//        }

//        /// <summary>
//        /// Gets player realm rank name
//        /// sirru mod 20.11.06
//        /// </summary>
//        public virtual string RealmTitle
//        {
//            get
//            {
//                if (Realm == eRealm.None)
//                    return "Unknown Realm";

//                try
//                {
//                    return GlobalConstants.REALM_RANK_NAMES[(int)Realm - 1, (int)Gender - 1, (RealmLevel / 10)];
//                }
//                catch
//                {
//                    return "Unknown Rank"; // why aren't all the realm ranks defined above?
//                }
//            }
//        }

//        /// <summary>
//        /// Called when this player gains realm points
//        /// </summary>
//        /// <param name="amount">The amount of realm points gained</param>
//        public override void GainRealmPoints(long amount)
//        {
//            GainRealmPoints(amount, true, true);
//        }

//        /// <summary>
//        /// Called when this living gains realm points
//        /// </summary>
//        /// <param name="amount">The amount of realm points gained</param>
//        public void GainRealmPoints(long amount, bool modify)
//        {
//            GainRealmPoints(amount, modify, true);
//        }

//        /// <summary>
//        /// Called when this player gains realm points
//        /// </summary>
//        /// <param name="amount"></param>
//        /// <param name="modify"></param>
//        /// <param name="sendMessage"></param>
//        public void GainRealmPoints(long amount, bool modify, bool sendMessage)
//        {
//            GainRealmPoints(amount, modify, true, true);
//        }

//        /// <summary>
//        /// Called when this player gains realm points
//        /// </summary>
//        /// <param name="amount">The amount of realm points gained</param>
//        /// <param name="modify">Should we apply the rp modifer</param>
//        /// <param name="sendMessage">Wether to send a message like "You have gained N realmpoints"</param>
//        /// <param name="notify"></param>
//        public virtual void GainRealmPoints(long amount, bool modify, bool sendMessage, bool notify)
//        {
//            //if (!GainRP)
//            //	return;

//            //if (modify)
//            //{
//            //	//rp rate modifier
//            //	double modifier = ServerProperties.Properties.RP_RATE;
//            //	if (modifier != -1)
//            //		amount = (long)(amount * modifier);

//            //	//[StephenxPimente]: Zone Bonus Support
//            //	if (ServerProperties.Properties.ENABLE_ZONE_BONUSES)
//            //	{
//            //		int zoneBonus = (((int)amount * ZoneBonus.GetRPBonus(this)) / 100);
//            //		if (zoneBonus > 0)
//            //		{
//            //			Out.SendMessage(ZoneBonus.GetBonusMessage(this, (int)(zoneBonus * ServerProperties.Properties.RP_RATE), ZoneBonus.eZoneBonusType.RP),
//            //							eChatType.CT_Important, eChatLoc.CL_SystemWindow);
//            //			GainRealmPoints((long)(zoneBonus * ServerProperties.Properties.RP_RATE), false, false, false);
//            //		}
//            //	}

//            //	//[Freya] Nidel: ToA Rp Bonus
//            //	long rpBonus = GetModified(eProperty.RealmPoints);
//            //	if (rpBonus > 0)
//            //	{
//            //		amount += (amount * rpBonus) / 100;
//            //	}
//            //}

//            //if (notify)
//            //	base.GainRealmPoints(amount);

//            //RealmPoints += amount;

//            //if (m_guild != null && Client.Account.PrivLevel == 1)
//            //	m_guild.RealmPoints += amount;

//            //if (sendMessage == true && amount > 0)
//            //	Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.GainRealmPoints.YouGet", amount.ToString()), eChatType.CT_Important, eChatLoc.CL_SystemWindow);

//            //while (RealmPoints >= CalculateRPsFromRealmLevel(RealmLevel + 1) && RealmLevel < (REALMPOINTS_FOR_LEVEL.Length - 1))
//            //{
//            //	RealmLevel++;
//            //	//Out.SendUpdatePlayer();
//            //	if (RealmLevel % 10 == 0)
//            //	{
//            //		//Out.SendUpdatePlayerSkills();
//            //		foreach (GamePlayer plr in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
//            //			plr.Out.SendLivingDataUpdate(this, true);
//            //		Notify(GamePlayerEvent.RRLevelUp, this);
//            //	}
//            //	else
//            //		Notify(GamePlayerEvent.RLLevelUp, this);
//            //	if (CanGenerateNews && ((RealmLevel >= 40 && RealmLevel % 10 == 0) || RealmLevel >= 60))
//            //	{
//            //		string message = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.GainRealmPoints.ReachedRankNews", Name, RealmLevel + 10, LastPositionUpdateZone.Description);
//            //		string newsmessage = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.GainRealmPoints.ReachedRankNews", Name, RealmLevel + 10, LastPositionUpdateZone.Description);
//            //		NewsMgr.CreateNews(newsmessage, this.Realm, eNewsType.RvRLocal, true);
//            //	}
//            //	if (CanGenerateNews && RealmPoints >= 1000000 && RealmPoints - amount < 1000000)
//            //	{
//            //		string message = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.GainRealmPoints.Earned", Name, LastPositionUpdateZone.Description);
//            //		string newsmessage = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.GainRealmPoints.Earned", Name, LastPositionUpdateZone.Description);
//            //		NewsMgr.CreateNews(newsmessage, this.Realm, eNewsType.RvRLocal, true);
//            //	}
//            //}
//            //Out.SendUpdatePoints();
//        }

//        /// <summary>
//        /// Called when this living buy something with realm points
//        /// </summary>
//        /// <param name="amount">The amount of realm points loosed</param>
//        public bool RemoveBountyPoints(long amount)
//        {
//            return RemoveBountyPoints(amount, null);
//        }

//        /// <summary>
//        /// Called when this living buy something with realm points
//        /// </summary>
//        /// <param name="amount"></param>
//        /// <param name="str"></param>
//        /// <returns></returns>
//        public bool RemoveBountyPoints(long amount, string str)
//        {
//            return RemoveBountyPoints(amount, str, eChatType.CT_Say, eChatLoc.CL_SystemWindow);
//        }

//        /// <summary>
//        /// Called when this living buy something with realm points
//        /// </summary>
//        /// <param name="amount">The amount of realm points loosed</param>
//        /// <param name="loc">The chat location</param>
//        /// <param name="str">The message</param>
//        /// <param name="type">The chat type</param>
//        public virtual bool RemoveBountyPoints(long amount, string str, eChatType type, eChatLoc loc)
//        {
//            if (BountyPoints < amount)
//                return false;
//            BountyPoints -= amount;
//            //Out.SendUpdatePoints();
//            //if (str != null && amount != 0)
//            //	Out.SendMessage(str, type, loc);
//            return true;
//        }

//        /// <summary>
//        /// Player gains bounty points
//        /// </summary>
//        /// <param name="amount">The amount of bounty points</param>
//        public override void GainBountyPoints(long amount)
//        {
//            GainBountyPoints(amount, true, true);
//        }

//        /// <summary>
//        /// Player gains bounty points
//        /// </summary>
//        /// <param name="amount">The amount of bounty points</param>
//        public void GainBountyPoints(long amount, bool modify)
//        {
//            GainBountyPoints(amount, modify, true);
//        }

//        /// <summary>
//        /// Called when player gains bounty points
//        /// </summary>
//        /// <param name="amount"></param>
//        /// <param name="modify"></param>
//        /// <param name="sendMessage"></param>
//        public void GainBountyPoints(long amount, bool modify, bool sendMessage)
//        {
//            GainBountyPoints(amount, modify, true, true);
//        }

//        /// <summary>
//        /// Called when player gains bounty points
//        /// </summary>
//        /// <param name="amount">The amount of bounty points gained</param>
//        /// <param name="multiply">Should this amount be multiplied by the BP Rate</param>
//        /// <param name="sendMessage">Wether to send a message like "You have gained N bountypoints"</param>
//        public virtual void GainBountyPoints(long amount, bool modify, bool sendMessage, bool notify)
//        {
//            if (modify)
//            {
//                //bp rate modifier
//                double modifier = ServerProperties.Properties.BP_RATE;
//                if (modifier != -1)
//                    amount = (long)(amount * modifier);

//                //[StephenxPimente]: Zone Bonus Support
//                //if (ServerProperties.Properties.ENABLE_ZONE_BONUSES)
//                //{
//                //	int zoneBonus = ((int)amount * ZoneBonus.GetBPBonus(this) / 100);
//                //	if (zoneBonus > 0)
//                //	{
//                //		GainBountyPoints((long)(zoneBonus * ServerProperties.Properties.BP_RATE), false, false, false);
//                //	}
//                //}

//                //[Freya] Nidel: ToA Bp Bonus
//                long bpBonus = GetModified(eProperty.BountyPoints);

//                if (bpBonus > 0)
//                {
//                    amount += (amount * bpBonus) / 100;
//                }
//            }

//            if (notify)
//                base.GainBountyPoints(amount);

//            BountyPoints += amount;

//            //if (m_guild != null && Client.Account.PrivLevel == 1)
//            //	m_guild.BountyPoints += amount;

//            //if (sendMessage == true)
//            //	Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.GainBountyPoints.YouGet", amount.ToString()), eChatType.CT_Important, eChatLoc.CL_SystemWindow);

//            //Out.SendUpdatePoints();
//        }

//        /// <summary>
//        /// Holds realm points needed for special realm level
//        /// </summary>
//        public static readonly long[] REALMPOINTS_FOR_LEVEL =
//        {
//            0,	// for level 0
//			0,	// for level 1
//			25,	// for level 2
//			125,	// for level 3
//			350,	// for level 4
//			750,	// for level 5
//			1375,	// for level 6
//			2275,	// for level 7
//			3500,	// for level 8
//			5100,	// for level 9
//			7125,	// for level 10
//			9625,	// for level 11
//			12650,	// for level 12
//			16250,	// for level 13
//			20475,	// for level 14
//			25375,	// for level 15
//			31000,	// for level 16
//			37400,	// for level 17
//			44625,	// for level 18
//			52725,	// for level 19
//			61750,	// for level 20
//			71750,	// for level 21
//			82775,	// for level 22
//			94875,	// for level 23
//			108100,	// for level 24
//			122500,	// for level 25
//			138125,	// for level 26
//			155025,	// for level 27
//			173250,	// for level 28
//			192850,	// for level 29
//			213875,	// for level 30
//			236375,	// for level 31
//			260400,	// for level 32
//			286000,	// for level 33
//			313225,	// for level 34
//			342125,	// for level 35
//			372750,	// for level 36
//			405150,	// for level 37
//			439375,	// for level 38
//			475475,	// for level 39
//			513500,	// for level 40
//			553500,	// for level 41
//			595525,	// for level 42
//			639625,	// for level 43
//			685850,	// for level 44
//			734250,	// for level 45
//			784875,	// for level 46
//			837775,	// for level 47
//			893000,	// for level 48
//			950600,	// for level 49
//			1010625,	// for level 50
//			1073125,	// for level 51
//			1138150,	// for level 52
//			1205750,	// for level 53
//			1275975,	// for level 54
//			1348875,	// for level 55
//			1424500,	// for level 56
//			1502900,	// for level 57
//			1584125,	// for level 58
//			1668225,	// for level 59
//			1755250,	// for level 60
//			1845250,	// for level 61
//			1938275,	// for level 62
//			2034375,	// for level 63
//			2133600,	// for level 64
//			2236000,	// for level 65
//			2341625,	// for level 66
//			2450525,	// for level 67
//			2562750,	// for level 68
//			2678350,	// for level 69
//			2797375,	// for level 70
//			2919875,	// for level 71
//			3045900,	// for level 72
//			3175500,	// for level 73
//			3308725,	// for level 74
//			3445625,	// for level 75
//			3586250,	// for level 76
//			3730650,	// for level 77
//			3878875,	// for level 78
//			4030975,	// for level 79
//			4187000,	// for level 80
//			4347000,	// for level 81
//			4511025,	// for level 82
//			4679125,	// for level 83
//			4851350,	// for level 84
//			5027750,	// for level 85
//			5208375,	// for level 86
//			5393275,	// for level 87
//			5582500,	// for level 88
//			5776100,	// for level 89
//			5974125,	// for level 90
//			6176625,	// for level 91
//			6383650,	// for level 92
//			6595250,	// for level 93
//			6811475,	// for level 94
//			7032375,	// for level 95
//			7258000,	// for level 96
//			7488400,	// for level 97
//			7723625,	// for level 98
//			7963725,	// for level 99
//			8208750,	// for level 100
//			9111713,	// for level 101
//			10114001,	// for level 102
//			11226541,	// for level 103
//			12461460,	// for level 104
//			13832221,	// for level 105
//			15353765,	// for level 106
//			17042680,	// for level 107
//			18917374,	// for level 108
//			20998286,	// for level 109
//			23308097,	// for level 110
//			25871988,	// for level 111
//			28717906,	// for level 112
//			31876876,	// for level 113
//			35383333,	// for level 114
//			39275499,	// for level 115
//			43595804,	// for level 116
//			48391343,	// for level 117
//			53714390,	// for level 118
//			59622973,	// for level 119
//			66181501,	// for level 120
//			73461466,	// for level 121
//			81542227,	// for level 122
//			90511872,	// for level 123
//			100468178,	// for level 124
//			111519678,	// for level 125
//			123786843,	// for level 126
//			137403395,	// for level 127
//			152517769,	// for level 128
//			169294723,	// for level 129
//			187917143,	// for level 130
//		};

//        /// <summary>
//        /// Calculates amount of RealmPoints needed for special realm level
//        /// </summary>
//        /// <param name="realmLevel">realm level</param>
//        /// <returns>amount of realm points</returns>
//        protected virtual long CalculateRPsFromRealmLevel(int realmLevel)
//        {
//            if (realmLevel < REALMPOINTS_FOR_LEVEL.Length)
//                return REALMPOINTS_FOR_LEVEL[realmLevel];

//            // thanks to Linulo from http://daoc.foren.4players.de/viewtopic.php?t=40839&postdays=0&postorder=asc&start=0
//            return (long)(25.0 / 3.0 * (realmLevel * realmLevel * realmLevel) - 25.0 / 2.0 * (realmLevel * realmLevel) + 25.0 / 6.0 * realmLevel);
//        }

//        /// <summary>
//        /// Calculates realm level from realm points. SLOW.
//        /// </summary>
//        /// <param name="realmPoints">amount of realm points</param>
//        /// <returns>realm level: RR5L3 = 43, RR1L2 = 2</returns>
//        protected virtual int CalculateRealmLevelFromRPs(long realmPoints)
//        {
//            if (realmPoints == 0)
//                return 0;

//            int i;

//            for (i = REALMPOINTS_FOR_LEVEL.Length - 1; i > 0; i--)
//            {
//                if (REALMPOINTS_FOR_LEVEL[i] <= realmPoints)
//                    break;
//            }

//            return i;
//        }

//        /// <summary>
//        /// Realm point value of this player
//        /// </summary>
//        public override int RealmPointsValue
//        {
//            get
//            {
//                // http://www.camelotherald.com/more/2275.shtml
//                // new 1.81D formula
//                // Realm point value = (level - 20)squared + (realm rank level x 5) + (champion level x 10) + (master level (squared)x 5)
//                //we use realm level 1L0 = 0, mythic uses 1L0 = 10, so we + 10 the realm level
//                int level = Math.Max(0, Level - 20);
//                if (level == 0)
//                    return Math.Max(1, (RealmLevel + 10) * 5);

//                return Math.Max(1, level * level + (RealmLevel + 10) * 5);
//            }
//        }

//        /// <summary>
//        /// Bounty point value of this player
//        /// </summary>
//        public override int BountyPointsValue
//        {
//            // TODO: correct formula!
//            get { return (int)(1 + Level * 0.6); }
//        }

//        /// <summary>
//        /// Returns the amount of experience this player is worth
//        /// </summary>
//        public override long ExperienceValue
//        {
//            get
//            {
//                return base.ExperienceValue * 4;
//            }
//        }

//        public static readonly int[] prcRestore =
//        {
//			// http://www.silicondragon.com/Gaming/DAoC/Misc/XPs.htm
//			1,//0
//			3,//1
//			6,//2
//			10,//3
//			15,//4
//			21,//5
//			33,//6
//			53,//7
//			82,//8
//			125,//9
//			188,//10
//			278,//11
//			352,//12
//			443,//13
//			553,//14
//			688,//15
//			851,//16
//			1048,//17
//			1288,//18
//			1578,//19
//			1926,//20
//			2347,//21
//			2721,//22
//			3146,//23
//			3633,//24
//			4187,//25
//			4820,//26
//			5537,//27
//			6356,//28
//			7281,//29
//			8337,//30
//			9532,//31 - from logs
//			10886,//32 - from logs
//			12421,//33 - from logs
//			14161,//34
//			16131,//35
//			18360,//36 - recheck
//			19965,//37 - guessed
//			21857,//38
//			23821,//39
//			25928,//40 - guessed
//			28244,//41
//			30731,//42
//			33411,//43
//			36308,//44
//			39438,//45
//			42812,//46
//			46454,//47
//			50385,//48
//			54625,//49
//			59195,//50
//		};

//        /// <summary>
//        /// Money value of this player
//        /// </summary>
//        public override long MoneyValue
//        {
//            get
//            {
//                return 3 * prcRestore[Level < GamePlayer.prcRestore.Length ? Level : GamePlayer.prcRestore.Length - 1];
//            }
//        }

//        #endregion Realm-/Region-/Bount-/Skillpoints...

//        #region Level/Experience

//        public void DistributeSkillPoints()
//        {
//            int totalSpecPoints = VerifySpecPoints();
//            log.Info("totalSpecPoints: " + totalSpecPoints);

//            while (true)
//            {
//                if (!SpendSpecPoints(totalSpecPoints, true))
//                    break;
//            }
//        }

//        private bool SpendSpecPoints(int totalSpecPoints, bool ratio)
//        {
//            bool spentPoints = false;

//            while (true)
//            {
//                foreach (SpecLine specLine in MimicSpec.SpecLines)
//                {
//                    Specialization spec = GetSpecializationByName(specLine.SpecName);

//                    if (spec.Level < specLine.SpecCap && spec.Level < Level)
//                    {
//                        int specRatio = (int)(Level * specLine.levelRatio);

//                        if (ratio)
//                        {
//                            if (spec.Level >= specRatio)
//                                continue;
//                        }

//                        int totalCost = spec.Level + 1;

//                        // Indicates a dump stat for lvl 50
//                        if (specRatio <= 0 && Level < 50)
//                            continue;

//                        if (totalSpecPoints >= totalCost)
//                        {
//                            totalSpecPoints -= totalCost;
//                            spec.Level++;

//                            spentPoints = true;
//                        }
//                    }
//                }

//                if (spentPoints)
//                    spentPoints = false;
//                else
//                    break;
//            }

//            if (ratio)
//            {
//                return SpendSpecPoints(totalSpecPoints, false);
//            }

//            return false;
//        }

//        /// <summary>
//        /// What is the maximum level a player can achieve?
//        /// To alter this in a custom GamePlayer class you must override this method and
//        /// provide your own XPForLevel array with MaxLevel + 1 entries
//        /// </summary>
//        public virtual byte MaxLevel
//        {
//            get { return 50; }
//        }

//        /// <summary>
//        /// How much experience is needed for a given level?
//        /// </summary>
//        public virtual long GetExperienceNeededForLevel(int level)
//        {
//            if (level > MaxLevel)
//                return GetExperienceAmountForLevel(MaxLevel);

//            if (level <= 0)
//                return GetExperienceAmountForLevel(0);

//            return GetExperienceAmountForLevel(level - 1);
//        }

//        /// <summary>
//        /// How Much Experience Needed For Level
//        /// </summary>
//        /// <param name="level"></param>
//        /// <returns></returns>
//        public static long GetExperienceAmountForLevel(int level)
//        {
//            try
//            {
//                return XPForLevel[level];
//            }
//            catch
//            {
//                return 0;
//            }
//        }

//        /// <summary>
//        /// A table that holds the required XP/Level
//        /// This must include a final entry for MaxLevel + 1
//        /// </summary>
//        private static readonly long[] XPForLevel =
//        {
//            0, // xp to level 1
//			50, // xp to level 2
//			250, // xp to level 3
//			850, // xp to level 4
//			2300, // xp to level 5
//			6350, // xp to level 6
//			15950, // xp to level 7
//			37950, // xp to level 8
//			88950, // xp to level 9
//			203950, // xp to level 10
//			459950, // xp to level 11
//			839950, // xp to level 12
//			1399950, // xp to level 13
//			2199950, // xp to level 14
//			3399950, // xp to level 15
//			5199950, // xp to level 16
//			7899950, // xp to level 17
//			11799950, // xp to level 18
//			17499950, // xp to level 19
//			25899950, // xp to level 20
//			38199950, // xp to level 21
//			54699950, // xp to level 22
//			76999950, // xp to level 23
//			106999950, // xp to level 24
//			146999950, // xp to level 25
//			199999950, // xp to level 26
//			269999950, // xp to level 27
//			359999950, // xp to level 28
//			479999950, // xp to level 29
//			639999950, // xp to level 30
//			849999950, // xp to level 31
//			1119999950, // xp to level 32
//			1469999950, // xp to level 33
//			1929999950, // xp to level 34
//			2529999950, // xp to level 35
//			3319999950, // xp to level 36
//			4299999950, // xp to level 37
//			5499999950, // xp to level 38
//			6899999950, // xp to level 39
//			8599999950, // xp to level 40
//			12899999950, // xp to level 41
//			20699999950, // xp to level 42
//			29999999950, // xp to level 43
//			40799999950, // xp to level 44
//			53999999950, // xp to level 45
//			69599999950, // xp to level 46
//			88499999950, // xp to level 47
//			110999999950, // xp to level 48
//			137999999950, // xp to level 49
//			169999999950, // xp to level 50
//			999999999950, // xp to level 51
//		};

//        /// <summary>
//        /// Gets or sets the current xp of this player
//        /// </summary>
//        public virtual long Experience
//        {
//            get { return DBCharacter != null ? DBCharacter.Experience : 0; }
//            set
//            {
//                if (DBCharacter != null)
//                    DBCharacter.Experience = value;
//            }
//        }

//        /// <summary>
//        /// Returns the xp that are needed for the next level
//        /// </summary>
//        public virtual long ExperienceForNextLevel
//        {
//            get
//            {
//                return GetExperienceNeededForLevel(Level + 1);
//            }
//        }

//        /// <summary>
//        /// Returns the xp that were needed for the current level
//        /// </summary>
//        public virtual long ExperienceForCurrentLevel
//        {
//            get
//            {
//                return GetExperienceNeededForLevel(Level);
//            }
//        }

//        /// <summary>
//        /// Returns the xp that is needed for the second stage of current level
//        /// </summary>
//        public virtual long ExperienceForCurrentLevelSecondStage
//        {
//            get { return 1 + ExperienceForCurrentLevel + (ExperienceForNextLevel - ExperienceForCurrentLevel) / 2; }
//        }

//        /// <summary>
//        /// Returns how far into the level we have progressed
//        /// A value between 0 and 1000 (1 bubble = 100)
//        /// </summary>
//        public virtual ushort LevelPermill
//        {
//            get
//            {
//                //No progress if we haven't even reached current level!
//                if (Experience < ExperienceForCurrentLevel)
//                    return 0;
//                //No progess after maximum level
//                if (Level > MaxLevel)
//                    return 0;
//                return (ushort)(1000 * (Experience - ExperienceForCurrentLevel) / (ExperienceForNextLevel - ExperienceForCurrentLevel));
//            }
//        }

//        /// <summary>
//        /// Called whenever this player gains experience
//        /// </summary>
//        /// <param name="expTotal"></param>
//        /// <param name="expCampBonus"></param>
//        /// <param name="expGroupBonus"></param>
//        /// <param name="expOutpostBonus"></param>
//        /// <param name="sendMessage"></param>
//        public void GainExperience(eXPSource xpSource, long expTotal, long expCampBonus, long expGroupBonus, long expOutpostBonus, bool sendMessage)
//        {
//            GainExperience(xpSource, expTotal, expCampBonus, expGroupBonus, expOutpostBonus, sendMessage, true);
//        }

//        /// <summary>
//        /// Called whenever this player gains experience
//        /// </summary>
//        /// <param name="expTotal"></param>
//        /// <param name="expCampBonus"></param>
//        /// <param name="expGroupBonus"></param>
//        /// <param name="expOutpostBonus"></param>
//        /// <param name="sendMessage"></param>
//        /// <param name="allowMultiply"></param>
//        public void GainExperience(eXPSource xpSource, long expTotal, long expCampBonus, long expGroupBonus, long expOutpostBonus, bool sendMessage, bool allowMultiply)
//        {
//            GainExperience(xpSource, expTotal, expCampBonus, expGroupBonus, expOutpostBonus, sendMessage, allowMultiply, true);
//        }

//        /// <summary>
//        /// Called whenever this player gains experience
//        /// </summary>
//        /// <param name="expTotal"></param>
//        /// <param name="expCampBonus"></param>
//        /// <param name="expGroupBonus"></param>
//        /// <param name="expOutpostBonus"></param>
//        /// <param name="sendMessage"></param>
//        /// <param name="allowMultiply"></param>
//        /// <param name="notify"></param>
//        public override void GainExperience(eXPSource xpSource, long expTotal, long expCampBonus, long expGroupBonus, long expOutpostBonus, bool sendMessage, bool allowMultiply, bool notify)
//        {
//            if (!GainXP && expTotal > 0)
//                return;

//            //xp rate modifier
//            if (allowMultiply)
//            {
//                //we only want to modify the base rate, not the group or camp bonus
//                expTotal -= expGroupBonus;
//                expTotal -= expCampBonus;
//                expTotal -= expOutpostBonus;

//                //[StephenxPimentel] - Zone Bonus XP Support
//                //if (ServerProperties.Properties.ENABLE_ZONE_BONUSES)
//                //{
//                //	int zoneBonus = (((int)expTotal * ZoneBonus.GetXPBonus(this)) / 100);
//                //	if (zoneBonus > 0)
//                //	{
//                //		GainExperience(eXPSource.Other, (long)(zoneBonus * ServerProperties.Properties.XP_RATE), 0, 0, 0, false, false, false);
//                //	}
//                //}

//                if (this.CurrentRegion.IsRvR)
//                    expTotal = (long)(expTotal * ServerProperties.Properties.RvR_XP_RATE);
//                else
//                    expTotal = (long)(expTotal * ServerProperties.Properties.XP_RATE);

//                // [Freya] Nidel: ToA Xp Bonus
//                long xpBonus = GetModified(eProperty.XpPoints);
//                if (xpBonus != 0)
//                {
//                    expTotal += (expTotal * xpBonus) / 100;
//                }

//                long hardXPCap = (long)(GameServer.ServerRules.GetExperienceForLiving(Level) * ServerProperties.Properties.XP_HARDCAP_PERCENT / 100);

//                if (expTotal > hardXPCap)
//                    expTotal = hardXPCap;

//                expTotal += expOutpostBonus;
//                expTotal += expGroupBonus;
//                expTotal += expCampBonus;
//            }

//            // Get Champion Experience too
//            //GainChampionExperience(expTotal);

//            base.GainExperience(xpSource, expTotal, expCampBonus, expGroupBonus, expOutpostBonus, sendMessage, allowMultiply, notify);

//            if (IsLevelSecondStage)
//            {
//                if (Experience + expTotal < ExperienceForCurrentLevelSecondStage)
//                {
//                    expTotal = ExperienceForCurrentLevelSecondStage - Experience;
//                }
//            }
//            else if (Experience + expTotal < ExperienceForCurrentLevel)
//            {
//                expTotal = ExperienceForCurrentLevel - Experience;
//            }

//            Experience += expTotal;

//            if (expTotal >= 0)
//            {
//                //Level up
//                if (Level >= 40 && Level < MaxLevel && !IsLevelSecondStage && Experience >= ExperienceForCurrentLevelSecondStage)
//                {
//                    OnLevelSecondStage();
//                    Notify(GamePlayerEvent.LevelSecondStage, this);
//                }
//                else if (Level < MaxLevel && Experience >= ExperienceForNextLevel)
//                {
//                    Level++;
//                }
//            }
//            //Out.SendUpdatePoints();
//        }

//        /// <summary>
//        /// Gets or sets the level of the player
//        /// (delegate to PlayerCharacter)
//        /// </summary>
//        public override byte Level
//        {
//            //get { return DBCharacter != null ? (byte)DBCharacter.Level : base.Level; }
//            get { return base.Level; }
//            set
//            {
//                int oldLevel = Level;

//                base.Level = value;

//                if (DBCharacter != null)
//                    DBCharacter.Level = value;

//                if (oldLevel > 0)
//                {
//                    if (value > oldLevel)
//                    {
//                        OnLevelUp(oldLevel);
//                    }
//                }
//            }
//        }

//        public override void AutoSetStats()
//        {
//            return;
//        }

//        /// <summary>
//        /// What is the base, unmodified level of this character.
//        /// </summary>
//        public override byte BaseLevel
//        {
//            get { return DBCharacter != null ? (byte)DBCharacter.Level : base.BaseLevel; }
//        }

//        /// <summary>
//        /// What level is displayed to another player
//        /// </summary>
//        public override byte GetDisplayLevel(GamePlayer player)
//        {
//            return Math.Min((byte)50, Level);
//        }

//        /// <summary>
//        /// Is this player in second stage of current level
//        /// (delegate to PlayerCharacter)
//        /// </summary>
//        public virtual bool IsLevelSecondStage
//        {
//            get { return DBCharacter != null ? DBCharacter.IsLevelSecondStage : false; }
//            set { if (DBCharacter != null) DBCharacter.IsLevelSecondStage = value; }
//        }

//        /// <summary>
//        /// Called when this player levels
//        /// </summary>
//        /// <param name="previouslevel"></param>
//        public virtual void OnLevelUp(int previouslevel)
//        {
//            IsLevelSecondStage = false;

//            //level 20 changes realm title and gives 1 realm skill point
//            if (Level == 20)
//                GainRealmPoints(0);

//            // Adjust stats

//            // stat increases start at level 6
//            if (Level > 5)
//            {
//                for (int i = Level; i > Math.Max(previouslevel, 5); i--)
//                {
//                    if (CharacterClass.PrimaryStat != eStat.UNDEFINED)
//                    {
//                        ChangeBaseStat(CharacterClass.PrimaryStat, 1);
//                    }
//                    if (CharacterClass.SecondaryStat != eStat.UNDEFINED && ((i - 6) % 2 == 0))
//                    { // base level to start adding stats is 6
//                        ChangeBaseStat(CharacterClass.SecondaryStat, 1);
//                    }
//                    if (CharacterClass.TertiaryStat != eStat.UNDEFINED && ((i - 6) % 3 == 0))
//                    { // base level to start adding stats is 6
//                        ChangeBaseStat(CharacterClass.TertiaryStat, 1);
//                    }
//                }
//            }

//            //CharacterClass.OnLevelUp(this, previouslevel);

//            RefreshSpecDependantSkills(true);

//            // Echostorm - Code for display of new title on level up
//            // Get old and current rank titles
//            //string currenttitle = CharacterClass.GetTitle(this, Level);

//            // spec points
//            int specpoints = 0;
//            for (int i = Level; i > previouslevel; i--)
//            {
//                if (i <= 5)
//                    specpoints += i + 1; //start levels. Kyle - One extra per to play with.
//                else
//                    specpoints += CharacterClass.SpecPointsMultiplier * i / 10; //spec levels
//            }

//            SpecPoints += specpoints;

//            if (IsAlive)
//            {
//                // workaround for starting regeneration
//                StartHealthRegeneration();
//                StartPowerRegeneration();
//            }

//            DeathCount = 0;

//            if (Group != null)
//            {
//                Group.UpdateGroupWindow();
//            }
//            //Out.SendUpdatePlayer(); // Update player level
//            //Out.SendCharStatsUpdate(); // Update Stats and MaxHitpoints
//            //Out.SendCharResistsUpdate();
//            //Out.SendUpdatePlayerSkills();
//            //Out.SendUpdatePoints();
//            //UpdatePlayerStatus();

//            // update color on levelup
//            if (ObjectState == eObjectState.Active)
//            {
//                foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
//                {
//                    if (player == null) continue;
//                    player.Out.SendEmoteAnimation(this, eEmote.LvlUp);
//                }
//            }

//            // Level up pets and subpets
//            //if (DOL.GS.ServerProperties.Properties.PET_LEVELS_WITH_OWNER && ControlledBrain is ControlledNpcBrain brain && brain.Body is GamePet pet)
//            //{
//            //    if (pet.SetPetLevel())
//            //    {
//            //        if (DOL.GS.ServerProperties.Properties.PET_SCALE_SPELL_MAX_LEVEL > 0 && pet.Spells.Count > 0)
//            //            pet.SortSpells();

//            //        brain.UpdatePetWindow();
//            //    }

//            //    // subpets
//            //    if (pet.ControlledNpcList != null)
//            //        foreach (ABrain subBrain in pet.ControlledNpcList)
//            //            if (subBrain != null && subBrain.Body is GamePet subPet)
//            //                if (subPet.SetPetLevel()) // Levels up subpet
//            //                    if (DOL.GS.ServerProperties.Properties.PET_SCALE_SPELL_MAX_LEVEL > 0)
//            //                        subPet.SortSpells();
//            //}

//            // save player to database
//            //SaveIntoDatabase();
//        }

//        public List<Specialization> SpecPlans = new List<Specialization>();

//        public void SetSpecPlans(Specialization[] specializations)
//        {
//            if (SpecPlans.Count > 0)
//                SpecPlans.Clear();

//            SpecPlans = specializations.ToList();
//        }

//        public void Spec()
//        {
//            Dictionary<string, int> NameCost = new Dictionary<string, int>();

//            foreach (KeyValuePair<string, Specialization> index in m_specialization)
//            {
//                NameCost.Add(index.Value.Name, index.Value.Level + 1);
//            }

//            for (int i = 0; i < SpecPlans.Count; i++)
//            {
//                int specCost;
//                NameCost.TryGetValue(SpecPlans[i].Name, out specCost);

//                if (SpecPoints >= specCost + (specCost / SpecPoints) && specCost - 1 < Level)
//                {
//                    GetSpecializationByName(SpecPlans[i].Name).Level++;
//                    SpecPoints -= specCost;
//                }
//            }
//        }

//        /// <summary>
//        /// Called when this player reaches second stage of the current level
//        /// </summary>
//        public virtual void OnLevelSecondStage()
//        {
//            IsLevelSecondStage = true;

//            //death penalty reset on mini-ding
//            DeathCount = 0;

//            if (Group != null)
//            {
//                Group.UpdateGroupWindow();
//            }
//            //Out.SendUpdatePlayer(); // Update player level
//            //Out.SendCharStatsUpdate(); // Update Stats and MaxHitpoints
//            //Out.SendUpdatePlayerSkills();
//            //Out.SendUpdatePoints();
//            //UpdatePlayerStatus();

//            //SaveIntoDatabase();
//        }

//        /// <summary>
//        /// Calculate the Autotrain points.
//        /// </summary>
//        /// <param name="spec">Specialization</param>
//        /// <param name="mode">various AT related calculations (amount of points, level of AT...)</param>
//        public virtual int GetAutoTrainPoints(Specialization spec, int Mode)
//        {
//            int max_autotrain = Level / 4;
//            if (max_autotrain == 0) max_autotrain = 1;

//            foreach (string autotrainKey in CharacterClass.GetAutotrainableSkills())
//            {
//                if (autotrainKey == spec.KeyName)
//                {
//                    switch (Mode)
//                    {
//                        case 0:// return sum of all AT points in the spec
//                        {
//                            int pts_to_refund = Math.Min(max_autotrain, spec.Level);
//                            return ((pts_to_refund * (pts_to_refund + 1) - 2) / 2);
//                        }
//                        case 1: // return max AT level + message
//                        {
//                            if (Level % 4 == 0)
//                                if (spec.Level >= max_autotrain)
//                                    return max_autotrain;
//                            return 0;
//                        }
//                        case 2: // return next free points due to AT change on levelup
//                        {
//                            if (spec.Level < max_autotrain)
//                                return (spec.Level + 1);
//                            else
//                                return 0;
//                        }
//                        case 3: // return sum of all free AT points
//                        {
//                            if (spec.Level < max_autotrain)
//                                return (((max_autotrain * (max_autotrain + 1) - 2) / 2) - ((spec.Level * (spec.Level + 1) - 2) / 2));
//                            else
//                                return ((max_autotrain * (max_autotrain + 1) - 2) / 2);
//                        }
//                        case 4: // spec is autotrainable
//                        {
//                            return 1;
//                        }
//                    }
//                }
//            }
//            return 0;
//        }

//        #endregion Level/Experience

//        #region Spells/Skills/Abilities/Effects

//        /// <summary>
//        /// Holds the player choosen list of Realm Abilities.
//        /// </summary>
//        protected readonly ReaderWriterList<RealmAbility> m_realmAbilities = new ReaderWriterList<RealmAbility>();

//        /// <summary>
//        /// Holds the player specializable skills and style lines
//        /// (KeyName -> Specialization)
//        /// </summary>
//        protected readonly Dictionary<string, Specialization> m_specialization = new Dictionary<string, Specialization>();

//        /// <summary>
//        /// Holds the Spell lines the player can use
//        /// </summary>
//        protected readonly List<SpellLine> m_spellLines = new List<SpellLine>();

//        /// <summary>
//        /// Object to use when locking the SpellLines list
//        /// </summary>
//        protected readonly Object lockSpellLinesList = new Object();

//        /// <summary>
//        /// Holds all styles of the player
//        /// </summary>
//        protected readonly Dictionary<int, Style> m_styles = new Dictionary<int, Style>();

//        /// <summary>
//        /// Used to lock the style list
//        /// </summary>
//        protected readonly Object lockStyleList = new Object();

//        /// <summary>
//        /// Temporary Stats Boni
//        /// </summary>
//        protected readonly int[] m_statBonus = new int[8];

//        /// <summary>
//        /// Temporary Stats Boni in percent
//        /// </summary>
//        protected readonly int[] m_statBonusPercent = new int[8];

//        /// <summary>
//		/// Does an attacker interrupt this livings cast?
//		/// </summary>
//		/// <param name="attacker"></param>
//		/// <returns></returns>
//		public override bool ChanceSpellInterrupt(GameLiving attacker)
//        {
//            double mod = GetConLevel(attacker);
//            double chance = BaseInterruptChance;

//            chance += mod * 10;

//            chance = Math.Max(1, chance);
//            chance = Math.Min(99, chance);

//            if (attacker is GamePlayer || attacker is MimicNPC)
//                chance = 99;
//            {
//                return Util.Chance((int)chance);
//            }
//        }

//        /// <summary>
//        /// give player a new Specialization or improve existing one
//        /// </summary>
//        /// <param name="skill"></param>
//        public void AddSpecialization(Specialization skill)
//        {
//            AddSpecialization(skill, true);
//        }

//        /// <summary>
//        /// give player a new Specialization or improve existing one
//        /// </summary>
//        /// <param name="skill"></param>
//        protected virtual void AddSpecialization(Specialization skill, bool notify)
//        {
//            if (skill == null)
//                return;

//            lock (((ICollection)m_specialization).SyncRoot)
//            {
//                // search for existing key
//                if (!m_specialization.ContainsKey(skill.KeyName))
//                {
//                    // Adding
//                    m_specialization.Add(skill.KeyName, skill);
//                }
//                else
//                {
//                    // Updating
//                    m_specialization[skill.KeyName].Level = skill.Level;
//                }
//            }
//        }

//        /// <summary>
//        /// Removes the existing specialization from the player
//        /// </summary>
//        /// <param name="specKeyName">The spec keyname to remove</param>
//        /// <returns>true if removed</returns>
//        public virtual bool RemoveSpecialization(string specKeyName)
//        {
//            Specialization playerSpec = null;

//            lock (((ICollection)m_specialization).SyncRoot)
//            {
//                if (!m_specialization.TryGetValue(specKeyName, out playerSpec))
//                    return false;

//                m_specialization.Remove(specKeyName);
//            }

//            return true;
//        }

//        /// <summary>
//        /// Removes the existing spellline from the player, the line instance should be called with GamePlayer.GetSpellLine ONLY and NEVER SkillBase.GetSpellLine!!!!!
//        /// </summary>
//        /// <param name="line">The spell line to remove</param>
//        /// <returns>true if removed</returns>
//        protected virtual bool RemoveSpellLine(SpellLine line)
//        {
//            lock (lockSpellLinesList)
//            {
//                if (!m_spellLines.Contains(line))
//                {
//                    return false;
//                }

//                m_spellLines.Remove(line);
//            }

//            return true;
//        }

//        /// <summary>
//        /// Removes the existing specialization from the player
//        /// </summary>
//        /// <param name="lineKeyName">The spell line keyname to remove</param>
//        /// <returns>true if removed</returns>
//        public virtual bool RemoveSpellLine(string lineKeyName)
//        {
//            SpellLine line = GetSpellLine(lineKeyName);
//            if (line == null)
//                return false;

//            return RemoveSpellLine(line);
//        }

//        /// <summary>
//        /// Reset this player to level 1, respec all skills, remove all spec points, and reset stats
//        /// </summary>
//        public virtual void Reset()
//        {
//            byte originalLevel = Level;
//            Level = 1;
//            Experience = 0;
//            RespecAllLines();

//            if (Level < originalLevel && originalLevel > 5)
//            {
//                for (int i = 6; i <= originalLevel; i++)
//                {
//                    if (CharacterClass.PrimaryStat != eStat.UNDEFINED)
//                    {
//                        ChangeBaseStat(CharacterClass.PrimaryStat, -1);
//                    }
//                    if (CharacterClass.SecondaryStat != eStat.UNDEFINED && ((i - 6) % 2 == 0))
//                    {
//                        ChangeBaseStat(CharacterClass.SecondaryStat, -1);
//                    }
//                    if (CharacterClass.TertiaryStat != eStat.UNDEFINED && ((i - 6) % 3 == 0))
//                    {
//                        ChangeBaseStat(CharacterClass.TertiaryStat, -1);
//                    }
//                }
//            }

//            //CharacterClass.OnLevelUp(this, originalLevel);
//        }

//        public virtual bool RespecAll()
//        {
//            if (RespecAllLines())
//            {
//                return true;
//            }

//            return false;
//        }

//        public virtual bool RespecDOL()
//        {
//            if (RespecAllLines()) // Wipe skills and styles.
//            {
//                return true;
//            }

//            return false;
//        }

//        public virtual int RespecSingle(Specialization specLine)
//        {
//            int specPoints = RespecSingleLine(specLine); // Wipe skills and styles.

//            return specPoints;
//        }

//        public virtual bool RespecRealm()
//        {
//            bool any = m_realmAbilities.Count > 0;

//            foreach (Ability ab in m_realmAbilities)
//                RemoveAbility(ab.KeyName);

//            m_realmAbilities.Clear();

//            return any;
//        }

//        protected virtual bool RespecAllLines()
//        {
//            bool ok = false;
//            int specPoints = 0;

//            IList<Specialization> specList = GetSpecList().Where(e => e.Trainable).ToList();

//            foreach (Specialization cspec in specList)
//            {
//                log.Info("Spec: " + cspec.Name);

//                if (cspec.Level < 2)
//                    continue;

//                specPoints += RespecSingleLine(cspec);
//                ok = true;
//            }

//            return ok;
//        }

//        /// <summary>
//        /// Respec single line
//        /// </summary>
//        /// <param name="specLine">spec line being respec'd</param>
//        /// <returns>Amount of points spent in that line</returns>
//        protected virtual int RespecSingleLine(Specialization specLine)
//        {
//            int specPoints = (specLine.Level * (specLine.Level + 1) - 2) / 2;
//            // Graveen - autotrain 1.87
//            specPoints -= GetAutoTrainPoints(specLine, 0);

//            //setting directly the autotrain points in the spec
//            if (GetAutoTrainPoints(specLine, 4) == 1 && Level >= 8)
//            {
//                specLine.Level = (int)Math.Floor((double)Level / 4);
//            }
//            else specLine.Level = 1;

//            // If BD subpet spells scaled and capped by BD spec, respecing a spell line
//            //	requires re-scaling the spells for all subpets from that line.
//            if (CharacterClass.Equals(GS.CharacterClass.Bonedancer)
//                && DOL.GS.ServerProperties.Properties.PET_SCALE_SPELL_MAX_LEVEL > 0
//                && DOL.GS.ServerProperties.Properties.PET_CAP_BD_MINION_SPELL_SCALING_BY_SPEC
//                && ControlledBody is GamePet pet && pet.ControlledNpcList != null)
//                foreach (ABrain subBrain in pet.ControlledNpcList)
//                    if (subBrain != null && subBrain.Body is BDSubPet subPet && subPet.PetSpecLine == specLine.KeyName)
//                        subPet.SortSpells();

//            return specPoints;
//        }

//        /// <summary>
//        /// returns a list with all specializations
//        /// in the order they were added
//        /// </summary>
//        /// <returns>list of Spec's</returns>
//        public virtual IList<Specialization> GetSpecList()
//        {
//            List<Specialization> list;

//            lock (((ICollection)m_specialization).SyncRoot)
//            {
//                // sort by Level and ID to simulate "addition" order... (try to sort your DB if you want to change this !)
//                list = m_specialization.Select(item => item.Value).OrderBy(it => it.LevelRequired).ThenBy(it => it.ID).ToList();
//            }

//            return list;
//        }

//        /// <summary>
//        /// returns a list with all non trainable skills without styles
//        /// This is a copy of Ability until any unhandled Skill subclass needs to go in there...
//        /// </summary>
//        /// <returns>list of Skill's</returns>
//        public virtual IList GetNonTrainableSkillList()
//        {
//            return GetAllAbilities();
//        }

//        /// <summary>
//        /// Retrives a specific specialization by name
//        /// </summary>
//        /// <param name="name">the name of the specialization line</param>
//        /// <param name="caseSensitive">false for case-insensitive compare</param>
//        /// <returns>found specialization or null</returns>
//        public virtual Specialization GetSpecializationByName(string name, bool caseSensitive = false)
//        {
//            Specialization spec = null;

//            lock (((ICollection)m_specialization).SyncRoot)
//            {
//                if (caseSensitive && m_specialization.ContainsKey(name))
//                    spec = m_specialization[name];

//                foreach (KeyValuePair<string, Specialization> entry in m_specialization)
//                {
//                    if (entry.Key.ToLower().Equals(name.ToLower()))
//                    {
//                        spec = entry.Value;
//                        break;
//                    }
//                }
//            }

//            return spec;
//        }

//        /// <summary>
//        /// The best armor level this player can use.
//        /// </summary>
//        public virtual int BestArmorLevel
//        {
//            get
//            {
//                int bestLevel = -1;
//                bestLevel = Math.Max(bestLevel, GetAbilityLevel(GS.Abilities.AlbArmor));
//                bestLevel = Math.Max(bestLevel, GetAbilityLevel(GS.Abilities.HibArmor));
//                bestLevel = Math.Max(bestLevel, GetAbilityLevel(GS.Abilities.MidArmor));
//                return bestLevel;
//            }
//        }

//        #region Abilities

//        /// <summary>
//        /// Adds a new Ability to the player
//        /// </summary>
//        /// <param name="ability"></param>
//        /// <param name="sendUpdates"></param>
//        public override void AddAbility(Ability ability, bool sendUpdates)
//        {
//            if (ability == null)
//                return;

//            base.AddAbility(ability, sendUpdates);
//        }

//        /// <summary>
//        /// Adds a Realm Ability to the player
//        /// </summary>
//        /// <param name="ability"></param>
//        /// <param name="sendUpdates"></param>
//        public virtual void AddRealmAbility(RealmAbility ability, bool sendUpdates)
//        {
//            if (ability == null)
//                return;

//            m_realmAbilities.FreezeWhile(list =>
//            {
//                int index = list.FindIndex(ab => ab.KeyName == ability.KeyName);
//                if (index > -1)
//                {
//                    list[index].Level = ability.Level;
//                }
//                else
//                {
//                    list.Add(ability);
//                }
//            });

//            RefreshSpecDependantSkills(true);
//        }

//        #endregion Abilities

//        public virtual void RemoveAllAbilities()
//        {
//            lock (m_lockAbilities)
//            {
//                m_abilities.Clear();
//            }
//        }

//        public virtual void RemoveAllSpecs()
//        {
//            lock (((ICollection)m_specialization).SyncRoot)
//            {
//                m_specialization.Clear();
//            }
//        }

//        public virtual void RemoveAllSpellLines()
//        {
//            lock (lockSpellLinesList)
//            {
//                m_spellLines.Clear();
//            }
//        }

//        public virtual void RemoveAllStyles()
//        {
//            lock (lockStyleList)
//            {
//                m_styles.Clear();
//            }
//        }

//        public virtual void AddStyle(Style st, bool notify)
//        {
//            lock (lockStyleList)
//            {
//                if (m_styles.ContainsKey(st.ID))
//                {
//                    m_styles[st.ID].Level = st.Level;
//                    Styles = m_styles.Values.ToList();
//                }
//                else
//                {
//                    m_styles.Add(st.ID, st);
//                    Styles = m_styles.Values.ToList();
//                }
//            }
//        }

//        /// <summary>
//        /// Retrieve this player Realm Abilities.
//        /// </summary>
//        /// <returns></returns>
//        public virtual List<RealmAbility> GetRealmAbilities()
//        {
//            return m_realmAbilities.ToList();
//        }

//        /// <summary>
//        /// Asks for existance of specific specialization
//        /// </summary>
//        /// <param name="keyName"></param>
//        /// <returns></returns>
//        public virtual bool HasSpecialization(string keyName)
//        {
//            bool hasit = false;

//            lock (((ICollection)m_specialization).SyncRoot)
//            {
//                hasit = m_specialization.ContainsKey(keyName);
//            }

//            return hasit;
//        }

//        /// <summary>
//        /// Checks whether Living has ability to use lefthanded weapons
//        /// </summary>
//        public override bool CanUseLefthandedWeapon
//        {
//            get
//            {
//                return CharacterClass.CanUseLefthandedWeapon;
//            }
//        }

//        /// <summary>
//        /// Calculates how many times left hand swings
//        /// </summary>
//        public override int CalculateLeftHandSwingCount()
//        {
//            if (CanUseLefthandedWeapon == false)
//                return 0;

//            if (GetBaseSpecLevel(Specs.Left_Axe) > 0)
//                return 1; // always use left axe

//            int specLevel = Math.Max(GetModifiedSpecLevel(Specs.Celtic_Dual), GetModifiedSpecLevel(Specs.Dual_Wield));
//            specLevel = Math.Max(specLevel, GetModifiedSpecLevel(Specs.Fist_Wraps));
//            if (specLevel > 0)
//            {
//                return Util.Chance(25 + (specLevel - 1) * 68 / 100) ? 1 : 0;
//            }

//            // HtH chance
//            specLevel = GetModifiedSpecLevel(Specs.HandToHand);
//            InventoryItem attackWeapon = AttackWeapon;
//            InventoryItem leftWeapon = (Inventory == null) ? null : Inventory.GetItem(eInventorySlot.LeftHandWeapon);
//            if (specLevel > 0 && ActiveWeaponSlot == eActiveWeaponSlot.Standard
//                && attackWeapon != null && attackWeapon.Object_Type == (int)eObjectType.HandToHand &&
//                leftWeapon != null && leftWeapon.Object_Type == (int)eObjectType.HandToHand)
//            {
//                specLevel--;
//                int randomChance = Util.Random(99);
//                int hitChance = specLevel >> 1;
//                if (randomChance < hitChance)
//                    return 1; // 1 hit = spec/2

//                hitChance += specLevel >> 2;
//                if (randomChance < hitChance)
//                    return 2; // 2 hits = spec/4

//                hitChance += specLevel >> 4;
//                if (randomChance < hitChance)
//                    return 3; // 3 hits = spec/16

//                return 0;
//            }

//            return 0;
//        }

//        /// <summary>
//        /// Returns a multiplier to adjust left hand damage
//        /// </summary>
//        /// <returns></returns>
//        public override double CalculateLeftHandEffectiveness(InventoryItem mainWeapon, InventoryItem leftWeapon)
//        {
//            double effectiveness = 1.0;

//            if (CanUseLefthandedWeapon && leftWeapon != null && leftWeapon.Object_Type == (int)eObjectType.LeftAxe && mainWeapon != null &&
//                (mainWeapon.Item_Type == Slot.RIGHTHAND || mainWeapon.Item_Type == Slot.LEFTHAND))
//            {
//                int LASpec = GetModifiedSpecLevel(Specs.Left_Axe);
//                if (LASpec > 0)
//                {
//                    effectiveness = 0.625 + 0.0034 * LASpec;
//                }
//            }

//            return effectiveness;
//        }

//        /// <summary>
//        /// Returns a multiplier to adjust right hand damage
//        /// </summary>
//        /// <param name="leftWeapon"></param>
//        /// <returns></returns>
//        public override double CalculateMainHandEffectiveness(InventoryItem mainWeapon, InventoryItem leftWeapon)
//        {
//            double effectiveness = 1.0;

//            if (CanUseLefthandedWeapon && leftWeapon != null && leftWeapon.Object_Type == (int)eObjectType.LeftAxe && mainWeapon != null &&
//                (mainWeapon.Item_Type == Slot.RIGHTHAND || mainWeapon.Item_Type == Slot.LEFTHAND))
//            {
//                int LASpec = GetModifiedSpecLevel(Specs.Left_Axe);
//                if (LASpec > 0)
//                {
//                    effectiveness = 0.625 + 0.0034 * LASpec;
//                }
//            }

//            return effectiveness;
//        }

//        /// <summary>
//        /// returns the level of a specialization
//        /// if 0 is returned, the spec is non existent on player
//        /// </summary>
//        /// <param name="keyName"></param>
//        /// <returns></returns>
//        public override int GetBaseSpecLevel(string keyName)
//        {
//            Specialization spec = null;
//            int level = 0;

//            lock (((ICollection)m_specialization).SyncRoot)
//            {
//                if (m_specialization.TryGetValue(keyName, out spec))
//                    level = m_specialization[keyName].Level;
//            }

//            return level;
//        }

//        /// <summary>
//        /// returns the level of a specialization + bonuses from RR and Items
//        /// if 0 is returned, the spec is non existent on the player
//        /// </summary>
//        /// <param name="keyName"></param>
//        /// <returns></returns>
//        public override int GetModifiedSpecLevel(string keyName)
//        {
//            if (keyName.StartsWith(GlobalSpellsLines.Champion_Lines_StartWith))
//                return 50;

//            Specialization spec = null;
//            int level = 0;
//            lock (((ICollection)m_specialization).SyncRoot)
//            {
//                if (!m_specialization.TryGetValue(keyName, out spec))
//                {
//                    if (keyName == GlobalSpellsLines.Combat_Styles_Effect)
//                    {
//                        if (CharacterClass.ID == (int)eCharacterClass.Reaver || CharacterClass.ID == (int)eCharacterClass.Heretic)
//                            level = GetModifiedSpecLevel(Specs.Flexible);
//                        if (CharacterClass.ID == (int)eCharacterClass.Valewalker)
//                            level = GetModifiedSpecLevel(Specs.Scythe);
//                        if (CharacterClass.ID == (int)eCharacterClass.Savage)
//                            level = GetModifiedSpecLevel(Specs.Savagery);
//                    }

//                    level = 0;
//                }
//            }

//            if (spec != null)
//            {
//                level = spec.Level;
//                // TODO: should be all in calculator later, right now
//                // needs specKey -> eProperty conversion to find calculator and then
//                // needs eProperty -> specKey conversion to find how much points player has spent
//                eProperty skillProp = SkillBase.SpecToSkill(keyName);
//                if (skillProp != eProperty.Undefined)
//                    level += GetModified(skillProp);
//            }

//            return level;
//        }

//        /// <summary>
//        /// Adds a spell line to the player
//        /// </summary>
//        /// <param name="line"></param>
//        public virtual void AddSpellLine(SpellLine line)
//        {
//            AddSpellLine(line, true);
//        }

//        /// <summary>
//        /// Adds a spell line to the player
//        /// </summary>
//        /// <param name="line"></param>
//        public virtual void AddSpellLine(SpellLine line, bool notify)
//        {
//            if (line == null)
//                return;

//            SpellLine oldline = GetSpellLine(line.KeyName);
//            if (oldline == null)
//            {
//                lock (lockSpellLinesList)
//                {
//                    m_spellLines.Add(line);
//                }
//            }
//            else
//            {
//                oldline.Level = line.Level;
//            }
//        }

//        /// <summary>
//        /// return a list of spell lines in the order they were added
//        /// this is a copy only.
//        /// </summary>
//        /// <returns></returns>
//        public virtual List<SpellLine> GetSpellLines()
//        {
//            List<SpellLine> list = new List<SpellLine>();
//            lock (lockSpellLinesList)
//            {
//                list = new List<SpellLine>(m_spellLines);
//            }

//            return list;
//        }

//        /// <summary>
//        /// find a spell line on player and return them
//        /// </summary>
//        /// <param name="keyname"></param>
//        /// <returns></returns>
//        public virtual SpellLine GetSpellLine(string keyname)
//        {
//            lock (lockSpellLinesList)
//            {
//                foreach (SpellLine line in m_spellLines)
//                {
//                    if (line.KeyName == keyname)
//                        return line;
//                }
//            }
//            return null;
//        }

//        /// <summary>
//        /// Gets a list of available styles
//        /// This creates a copy
//        /// </summary>
//        public virtual IList GetStyleList()
//        {
//            List<Style> list = new List<Style>();
//            lock (lockStyleList)
//            {
//                list = m_styles.Values.OrderBy(x => x.SpecLevelRequirement).ThenBy(y => y.ID).ToList();
//            }
//            return list;
//        }

//        /// <summary>
//        /// Skill cache, maintained for network order on "skill use" request...
//        /// Second item is for "Parent" Skill if applicable
//        /// </summary>
//        protected ReaderWriterList<Tuple<Skill, Skill>> m_usableSkills = new ReaderWriterList<Tuple<Skill, Skill>>();

//        /// <summary>
//        /// List Cast cache, maintained for network order on "spell use" request...
//        /// Second item is for "Parent" SpellLine if applicable
//        /// </summary>
//        protected ReaderWriterList<Tuple<SpellLine, List<Skill>>> m_usableListSpells = new ReaderWriterList<Tuple<SpellLine, List<Skill>>>();

//        /// <summary>
//        /// Get All Usable Spell for a list Caster.
//        /// </summary>
//        /// <param name="update"></param>
//        /// <returns></returns>
//        public virtual List<Tuple<SpellLine, List<Skill>>> GetAllUsableListSpells(bool update = false)
//        {
//            List<Tuple<SpellLine, List<Skill>>> results = new List<Tuple<SpellLine, List<Skill>>>();

//            if (!update)
//            {
//                if (m_usableListSpells.Count > 0)
//                    results = new List<Tuple<SpellLine, List<Skill>>>(m_usableListSpells);

//                // return results if cache is valid.
//                if (results.Count > 0)
//                    return results;
//            }

//            // lock during all update, even if replace only take place at end...
//            m_usableListSpells.FreezeWhile(innerList =>
//            {
//                List<Tuple<SpellLine, List<Skill>>> finalbase = new List<Tuple<SpellLine, List<Skill>>>();
//                List<Tuple<SpellLine, List<Skill>>> finalspec = new List<Tuple<SpellLine, List<Skill>>>();

//                // Add Lists spells ordered.
//                foreach (Specialization spec in GetSpecList().Where(item => !item.HybridSpellList))
//                {
//                    var spells = GetLinesSpellsForLiving(this, spec.Level, spec);

//                    foreach (SpellLine sl in GetSpellLinesForLiving(this, Level, spec.KeyName))
//                    {
//                        List<Tuple<SpellLine, List<Skill>>> working;
//                        if (sl.IsBaseLine)
//                        {
//                            working = finalbase;
//                        }
//                        else
//                        {
//                            working = finalspec;
//                        }

//                        List<Skill> sps = new List<Skill>();
//                        SpellLine key = spells.Keys.FirstOrDefault(el => el.KeyName == sl.KeyName);

//                        if (key != null && spells.ContainsKey(key))
//                        {
//                            foreach (Skill sp in spells[key])
//                            {
//                                sps.Add(sp);
//                            }
//                        }

//                        working.Add(new Tuple<SpellLine, List<Skill>>(sl, sps));
//                    }
//                }

//                // Linq isn't used, we need to keep order ! (SelectMany, GroupBy, ToDictionary can't be used !)
//                innerList.Clear();
//                foreach (var tp in finalbase)
//                {
//                    innerList.Add(tp);
//                    results.Add(tp);
//                }

//                foreach (var tp in finalspec)
//                {
//                    innerList.Add(tp);
//                    results.Add(tp);
//                }
//            });

//            return results;
//        }

//        public void SetSpells()
//        {
//            IList<Specialization> specs = GetSpecList();

//            List<Skill> skills;
//            List<Spell> spells = new List<Spell>();

//            foreach (Specialization spec in specs)
//            {
//                var dict = GetLinesSpellsForLiving(this, spec.Level, spec);

//                if (dict != null && dict.Count > 0)
//                {
//                    foreach (KeyValuePair<SpellLine, List<Skill>> kvp in dict)
//                    {
//                        dict.TryGetValue(kvp.Key, out skills);

//                        foreach (Skill skill in skills)
//                        {
//                            if (skill is Spell)
//                            {
//                                Spell spell = skill as Spell;

//                                spells.Add(spell);
//                            }
//                        }
//                    }
//                }
//            }

//            List<Spell> result = GetHighestLevelSpells(spells);

//            foreach (Spell spell in result)
//                log.Info(spell.Name + " " + spell.Level);

//            if (result.Count > 0)
//                Spells = result;
//        }

//        public void SetCasterSpells()
//        {
//            IList<Specialization> specs = GetSpecList();
//            List<Spell> spells = new List<Spell>();

//            foreach (Specialization spec in specs)
//            {
//                var dict = GetAllUsableListSpells();

//                if (dict != null && dict.Count > 0)
//                {
//                    foreach (Tuple<SpellLine, List<Skill>> tuple in dict)
//                    {
//                        switch (tuple.Item1.Name)
//                        {
//                            case "Banelord":
//                            case "Battlemaster":
//                            case "Convoker":
//                            case "Perfecter:":
//                            case "Sojourner":
//                            case "Spymaster":
//                            case "Stormlord":
//                            case "Warlord":
//                            continue;
//                        }

//                        if (tuple.Item2.Count > 0)
//                        {
//                            foreach (Skill skill in tuple.Item2)
//                            {
//                                if (skill is Spell)
//                                {
//                                    Spell spell = skill as Spell;
//                                    spells.Add(spell);
//                                }
//                            }
//                        }
//                    }
//                }
//            }

//            List<Spell> result = GetHighestLevelSpells(spells);

//            foreach (Spell spell in result)
//                log.Info(spell.Name + " " + spell.Level);

//            if (result.Count > 0)
//                Spells = result;
//        }

//        public List<Spell> GetHighestLevelSpells(List<Spell> spells)
//        {
//            List<Spell> result = new List<Spell>();

//            if (CharacterClass == CharacterClass.Cleric)
//            {
//            }

//            for (int i = 0; i < spells.Count; i++)
//            {
//                Spell currentSpell = spells[i];
//                Spell matchingItem = result.FirstOrDefault(x => x.DamageType == currentSpell.DamageType
//                                                             && x.SpellType == currentSpell.SpellType
//                                                             && x.Radius == currentSpell.Radius
//                                                             && x.Frequency == currentSpell.Frequency
//                                                             && x.CastTime == currentSpell.CastTime
//                                                             && x.Target == currentSpell.Target);

//                if (matchingItem == null || currentSpell.Level > matchingItem.Level)
//                {
//                    //if (currentSpell.SpellType == "Heal")
//                    //{
//                    //    if (matchingItem != null && matchingItem.Level != 46 && )
//                    //    {
//                    //        result.Add(currentSpell);
//                    //        continue;
//                    //    }
//                    //}

//                    result.RemoveAll(x => x.DamageType == currentSpell.DamageType
//                                       && x.SpellType == currentSpell.SpellType
//                                       && x.Radius == currentSpell.Radius
//                                       && x.Frequency == currentSpell.Frequency
//                                       && x.CastTime == currentSpell.CastTime
//                                       && x.Target == currentSpell.Target);

//                    result.Add(currentSpell);
//                }
//            }

//            return result;
//        }

//        /// <summary>
//		/// Sort spells into specific lists
//		/// </summary>
//		public override void SortSpells()
//        {
//            if (Spells.Count < 1)
//                return;

//            // Clear the lists
//            if (InstantHarmfulSpells != null)
//                InstantHarmfulSpells.Clear();

//            if (HarmfulSpells != null)
//                HarmfulSpells.Clear();

//            if (InstantHealSpells != null)
//                InstantHealSpells.Clear();

//            if (HealSpells != null)
//                HealSpells.Clear();

//            if (InstantMiscSpells != null)
//                InstantMiscSpells.Clear();

//            if (MiscSpells != null)
//                MiscSpells.Clear();

//            // Sort spells into lists
//            foreach (Spell spell in m_spells)
//            {
//                if (spell == null)
//                    continue;

//                if (spell.IsHarmful)
//                {
//                    if (spell.IsInstantCast)
//                    {
//                        if (InstantHarmfulSpells == null)
//                            InstantHarmfulSpells = new List<Spell>(1);

//                        InstantHarmfulSpells.Add(spell);
//                    }
//                    else
//                    {
//                        if (HarmfulSpells == null)
//                            HarmfulSpells = new List<Spell>(1);

//                        HarmfulSpells.Add(spell);
//                    }
//                }
//                else if (spell.IsHealing)
//                {
//                    if (spell.IsInstantCast)
//                    {
//                        if (InstantHealSpells == null)
//                            InstantHealSpells = new List<Spell>(1);

//                        InstantHealSpells.Add(spell);
//                    }
//                    else
//                    {
//                        if (HealSpells == null)
//                            HealSpells = new List<Spell>(1);

//                        HealSpells.Add(spell);
//                    }
//                }
//                else
//                {
//                    if (spell.IsInstantCast)
//                    {
//                        if (InstantMiscSpells == null)
//                            InstantMiscSpells = new List<Spell>(1);

//                        InstantMiscSpells.Add(spell);
//                    }
//                    else
//                    {
//                        if (MiscSpells == null)
//                            MiscSpells = new List<Spell>(1);

//                        MiscSpells.Add(spell);
//                    }
//                }
//            } // foreach

//            if (InstantHarmfulSpells != null)
//            {
//                log.Info("--------------------");
//                log.Info("InstantHarmfulSpells");

//                foreach (Spell spell in InstantHarmfulSpells)
//                {
//                    log.Info(spell.Name + " " + spell.Level);
//                }

//                log.Info("--------------------");
//            }

//            if (HarmfulSpells != null)
//            {
//                log.Info("--------------------");
//                log.Info("HarmfulSpells");

//                foreach (Spell spell in HarmfulSpells)
//                {
//                    log.Info(spell.Name + " " + spell.Level);
//                }

//                log.Info("--------------------");
//            }

//            if (InstantHealSpells != null)
//            {
//                log.Info("--------------------");
//                log.Info("InstantHealSpells");

//                foreach (Spell spell in InstantHealSpells)
//                {
//                    log.Info(spell.Name + " " + spell.Level);
//                }

//                log.Info("--------------------");
//            }

//            if (HealSpells != null)
//            {
//                log.Info("--------------------");
//                log.Info("HealSpells");

//                foreach (Spell spell in HealSpells)
//                {
//                    log.Info(spell.Name + " " + spell.Level);
//                }

//                log.Info("--------------------");
//            }

//            if (InstantMiscSpells != null)
//            {
//                log.Info("--------------------");
//                log.Info("InstantMiscSpells");

//                foreach (Spell spell in InstantMiscSpells)
//                {
//                    log.Info(spell.Name + " " + spell.Level);
//                }

//                log.Info("--------------------");
//            }

//            if (MiscSpells != null)
//            {
//                log.Info("--------------------");
//                log.Info("MiscSpells");

//                foreach (Spell spell in MiscSpells)
//                {
//                    log.Info(spell.Name + " " + spell.Level);
//                }

//                log.Info("--------------------");
//            }
//        }

//        /// <summary>
//		/// Sorts styles by type for more efficient style selection later
//		/// </summary>
//		public override void SortStyles()
//        {
//            if (StylesStealth != null)
//                StylesStealth.Clear();

//            if (StylesChain != null)
//                StylesChain.Clear();

//            if (StylesDefensive != null)
//                StylesDefensive.Clear();

//            if (StylesBack != null)
//                StylesBack.Clear();

//            if (StylesSide != null)
//                StylesSide.Clear();

//            if (StylesFront != null)
//                StylesFront.Clear();

//            if (StylesAnytime != null)
//                StylesAnytime.Clear();

//            if (m_styles == null)
//                return;

//            foreach (KeyValuePair<int, Style> kv in m_styles)
//            {
//                if (kv.Value == null)
//                {
//                    continue; // Keep sorting, as a later style may not be null
//                }// if (s == null)

//                if (kv.Value.StealthRequirement)
//                {
//                    if (StylesStealth == null)
//                        StylesStealth = new List<Style>(1);

//                    StylesStealth.Add(kv.Value);
//                }

//                switch (kv.Value.OpeningRequirementType)
//                {
//                    case Style.eOpening.Defensive:
//                    if (StylesDefensive == null)
//                        StylesDefensive = new List<Style>(1);

//                    StylesDefensive.Add(kv.Value);

//                    break;

//                    case Style.eOpening.Positional:
//                    switch ((Style.eOpeningPosition)kv.Value.OpeningRequirementValue)
//                    {
//                        case Style.eOpeningPosition.Back:
//                        if (StylesBack == null)
//                            StylesBack = new List<Style>(1);

//                        StylesBack.Add(kv.Value);

//                        break;

//                        case Style.eOpeningPosition.Side:
//                        if (StylesSide == null)
//                            StylesSide = new List<Style>(1);

//                        StylesSide.Add(kv.Value);

//                        break;

//                        case Style.eOpeningPosition.Front:
//                        if (StylesFront == null)
//                            StylesFront = new List<Style>(1);

//                        StylesFront.Add(kv.Value);

//                        break;

//                        default:

//                        log.Warn($"GameNPC.SortStyles(): Invalid OpeningRequirementValue for positional style {kv.Value.Name}, ID {kv.Value.ID}, ClassId {kv.Value.ClassID}");
//                        break;
//                    }
//                    break;

//                    default:
//                    if (kv.Value.OpeningRequirementValue > 0)
//                    {
//                        if (StylesChain == null)
//                            StylesChain = new List<Style>(1);
//                        StylesChain.Add(kv.Value);
//                    }
//                    else
//                    {
//                        if (Level > 1 && kv.Value.Level == 1)
//                            continue;

//                        if (StylesAnytime == null)
//                            StylesAnytime = new List<Style>(1);
//                        StylesAnytime.Add(kv.Value);
//                    }
//                    break;
//                }
//            }
//        }

//        /// <summary>
//        /// Get All Player Usable Skill Ordered in Network Order (usefull to check for useskill)
//        /// This doesn't get player's List Cast Specs...
//        /// </summary>
//        /// <param name="update"></param>
//        /// <returns></returns>
//        public virtual List<Tuple<Skill, Skill>> GetAllUsableSkills(bool update = false)
//        {
//            List<Tuple<Skill, Skill>> results = new List<Tuple<Skill, Skill>>();

//            if (!update)
//            {
//                if (m_usableSkills.Count > 0)
//                    results = new List<Tuple<Skill, Skill>>(m_usableSkills);

//                // return results if cache is valid.
//                if (results.Count > 0)
//                    return results;
//            }

//            // need to lock for all update.
//            m_usableSkills.FreezeWhile(innerList =>
//            {
//                IList<Specialization> specs = GetSpecList();
//                List<Tuple<Skill, Skill>> copylist = new List<Tuple<Skill, Skill>>(innerList);

//                // Add Spec
//                foreach (Specialization spec in specs.Where(item => item.Trainable))
//                {
//                    int index = innerList.FindIndex(e => (e.Item1 is Specialization) && ((Specialization)e.Item1).KeyName == spec.KeyName);

//                    if (index < 0)
//                    {
//                        // Specs must be appended to spec list
//                        innerList.Insert(innerList.Count(e => e.Item1 is Specialization), new Tuple<Skill, Skill>(spec, spec));
//                    }
//                    else
//                    {
//                        copylist.Remove(innerList[index]);
//                        // Replace...
//                        innerList[index] = new Tuple<Skill, Skill>(spec, spec);
//                    }
//                }

//                // Add Abilities (Realm ability should be a custom spec)
//                // Abilities order should be saved to db and loaded each time
//                foreach (Specialization spec in specs)
//                {
//                    foreach (Ability abv in GetAbilitiesForLiving(this, Level, spec.KeyName))
//                    {
//                        // We need the Instantiated Ability Object for Displaying Correctly According to Player "Activation" Method (if Available)
//                        Ability ab = GetAbility(abv.KeyName);

//                        if (ab == null)
//                            ab = abv;

//                        int index = innerList.FindIndex(k => (k.Item1 is Ability) && ((Ability)k.Item1).KeyName == ab.KeyName);

//                        if (index < 0)
//                        {
//                            // add
//                            innerList.Add(new Tuple<Skill, Skill>(ab, spec));
//                        }
//                        else
//                        {
//                            copylist.Remove(innerList[index]);
//                            // replace
//                            innerList[index] = new Tuple<Skill, Skill>(ab, spec);
//                        }
//                    }
//                }

//                // Add Hybrid spell
//                foreach (Specialization spec in specs.Where(item => item.HybridSpellList))
//                {
//                    int index = -1;
//                    foreach (KeyValuePair<SpellLine, List<Skill>> sl in spec.GetLinesSpellsForLiving(this))
//                    {
//                        foreach (Spell sp in sl.Value.Where(it => (it is Spell) && !((Spell)it).NeedInstrument).Cast<Spell>())
//                        {
//                            if (index < innerList.Count)
//                                index = innerList.FindIndex(index + 1, e => ((e.Item2 is SpellLine) && ((SpellLine)e.Item2).Spec == sl.Key.Spec) && (e.Item1 is Spell) && !((Spell)e.Item1).NeedInstrument);

//                            if (index < 0 || index >= innerList.Count)
//                            {
//                                // add
//                                innerList.Add(new Tuple<Skill, Skill>(sp, sl.Key));
//                                // disable replace
//                                index = innerList.Count;
//                            }
//                            else
//                            {
//                                copylist.Remove(innerList[index]);
//                                // replace
//                                innerList[index] = new Tuple<Skill, Skill>(sp, sl.Key);
//                            }
//                        }
//                    }
//                }

//                // Add Songs
//                int songIndex = -1;
//                foreach (Specialization spec in specs.Where(item => item.HybridSpellList))
//                {
//                    foreach (KeyValuePair<SpellLine, List<Skill>> sl in spec.GetLinesSpellsForLiving(this))
//                    {
//                        foreach (Spell sp in sl.Value.Where(it => (it is Spell) && ((Spell)it).NeedInstrument).Cast<Spell>())
//                        {
//                            if (songIndex < innerList.Count)
//                                songIndex = innerList.FindIndex(songIndex + 1, e => (e.Item1 is Spell) && ((Spell)e.Item1).NeedInstrument);

//                            if (songIndex < 0 || songIndex >= innerList.Count)
//                            {
//                                // add
//                                innerList.Add(new Tuple<Skill, Skill>(sp, sl.Key));
//                                // disable replace
//                                songIndex = innerList.Count;
//                            }
//                            else
//                            {
//                                copylist.Remove(innerList[songIndex]);
//                                // replace
//                                innerList[songIndex] = new Tuple<Skill, Skill>(sp, sl.Key);
//                            }
//                        }
//                    }
//                }

//                //// Add Styles
//                //foreach (Specialization spec in specs)
//                //{
//                //    foreach (Style st in spec.GetStylesForLiving(this))
//                //    {
//                //        int index = innerList.FindIndex(e => (e.Item1 is Style) && e.Item1.ID == st.ID);
//                //        if (index < 0)
//                //        {
//                //            // add
//                //            innerList.Add(new Tuple<Skill, Skill>(st, spec));
//                //        }
//                //        else
//                //        {
//                //            copylist.Remove(innerList[index]);
//                //            // replace
//                //            innerList[index] = new Tuple<Skill, Skill>(st, spec);
//                //        }
//                //    }
//                //}

//                // clean all not re-enabled skills
//                foreach (Tuple<Skill, Skill> item in copylist)
//                {
//                    innerList.Remove(item);
//                }

//                foreach (Tuple<Skill, Skill> el in innerList)
//                    results.Add(el);
//            });

//            return results;
//        }

//        /// <summary>
//        /// Default getter for Ability
//        /// Return Abilities it lists depending on spec level
//        /// Override to change the condition...
//        /// </summary>
//        /// <param name="living"></param>
//        /// <param name="level">level is only used when called for pretending some level (for trainer display)</param>
//        /// <returns></returns>
//        protected virtual List<Ability> GetAbilitiesForLiving(GameLiving living, int level, string KeyName)
//        {
//            // Select only Enabled and Max Level Abilities
//            List<Ability> abs = SkillBase.GetSpecAbilityList(KeyName, living is MimicNPC ? ((MimicNPC)living).CharacterClass.ID : 0);

//            // Get order of first appearing skills
//            IOrderedEnumerable<Ability> order = abs.GroupBy(item => item.KeyName)
//                .Select(ins => ins.OrderBy(it => it.SpecLevelRequirement).First())
//                .Where(item => item.SpecLevelRequirement <= level)
//                .OrderBy(item => item.SpecLevelRequirement)
//                .ThenBy(item => item.ID);

//            // Get best of skills
//            List<Ability> best = abs.Where(item => item.SpecLevelRequirement <= level)
//                .GroupBy(item => item.KeyName)
//                .Select(ins => ins.OrderByDescending(it => it.SpecLevelRequirement).First()).ToList();

//            List<Ability> results = new List<Ability>();
//            // make some kind of "Join" between the order of appearance and the best abilities.
//            foreach (Ability ab in order)
//            {
//                for (int r = 0; r < best.Count; r++)
//                {
//                    if (best[r].KeyName == ab.KeyName)
//                    {
//                        results.Add(best[r]);
//                        best.RemoveAt(r);
//                        break;
//                    }
//                }
//            }

//            return results;
//        }

//        /// <summary>
//        /// Default Getter For Spells
//        /// Retrieve Spell index by SpellLine, List Spell by Level Order
//        /// Select Only enabled Spells by spec or living level constraint.
//        /// </summary>
//        /// <param name="living"></param>
//        /// <param name="level">level is only used when called for pretending some level (for trainer display)</param>
//        /// <returns></returns>
//        protected virtual IDictionary<SpellLine, List<Skill>> GetLinesSpellsForLiving(GameLiving living, int level, Specialization spec)
//        {
//            IDictionary<SpellLine, List<Skill>> dict = new Dictionary<SpellLine, List<Skill>>();

//            foreach (SpellLine sl in GetSpellLinesForLiving(living, level, spec.KeyName))
//            {
//                dict.Add(sl, SkillBase.GetSpellList(sl.KeyName)
//                         .Where(item => item.Level <= sl.Level)
//                         .OrderBy(item => item.Level)
//                         .ThenBy(item => item.ID).Cast<Skill>().ToList());
//            }

//            return dict;
//        }

//        /// <summary>
//        /// Default getter for SpellLines
//        /// Retrieve spell line depending on advanced class and class hint
//        /// Order by Baseline
//        /// </summary>
//        /// <param name="living"></param>
//        /// <param name="level">level is only used when called for pretending some level (for trainer display)</param>
//        /// <returns></returns>
//        protected virtual List<SpellLine> GetSpellLinesForLiving(GameLiving living, int level, string KeyName)
//        {
//            List<SpellLine> list = new List<SpellLine>();
//            IList<Tuple<SpellLine, int>> spsl = SkillBase.GetSpecsSpellLines(KeyName);

//            // Get Spell Lines by order of appearance
//            if (living is MimicNPC)
//            {
//                MimicNPC mimic = (MimicNPC)living;

//                // select only spec line if is advanced class...
//                var tmp = spsl.Where(item => (item.Item1.IsBaseLine || mimic.CharacterClass.HasAdvancedFromBaseClass()))
//                    .OrderBy(item => (item.Item1.IsBaseLine ? 0 : 1)).ThenBy(item => item.Item1.ID);

//                // try with class hint
//                var baseline = tmp.Where(item => item.Item1.IsBaseLine && item.Item2 == mimic.CharacterClass.ID);
//                if (baseline.Any())
//                {
//                    foreach (Tuple<SpellLine, int> ls in baseline)
//                    {
//                        ls.Item1.Level = mimic.Level;
//                        list.Add(ls.Item1);
//                    }
//                }
//                else
//                {
//                    foreach (Tuple<SpellLine, int> ls in tmp.Where(item => item.Item1.IsBaseLine && item.Item2 == 0))
//                    {
//                        ls.Item1.Level = mimic.Level;
//                        list.Add(ls.Item1);
//                    }
//                }

//                // try spec with class hint
//                var specline = tmp.Where(item => !item.Item1.IsBaseLine && item.Item2 == mimic.CharacterClass.ID);
//                if (specline.Any())
//                {
//                    foreach (Tuple<SpellLine, int> ls in specline)
//                    {
//                        ls.Item1.Level = level;
//                        list.Add(ls.Item1);
//                    }
//                }
//                else
//                {
//                    foreach (Tuple<SpellLine, int> ls in tmp.Where(item => !item.Item1.IsBaseLine && item.Item2 == 0))
//                    {
//                        ls.Item1.Level = level;
//                        list.Add(ls.Item1);
//                    }
//                }
//            }
//            else
//            {
//                // default - not a player, add all...
//                foreach (Tuple<SpellLine, int> ls in spsl.OrderBy(item => (item.Item1.IsBaseLine ? 0 : 1)).ThenBy(item => item.Item1.ID))
//                {
//                    // default living spec is (Level * 0.66 + 1) on Live (no real proof...)
//                    // here : Level - (Level / 4) = 0.75
//                    if (ls.Item1.IsBaseLine)
//                        ls.Item1.Level = living.Level;
//                    else
//                        ls.Item1.Level = Math.Max(1, living.Level - (living.Level >> 2));
//                    list.Add(ls.Item1);
//                }
//            }

//            return list;
//        }

//        /// <summary>
//        /// updates the list of available skills (dependent on caracter specs)
//        /// </summary>
//        /// <param name="sendMessages">sends "you learn" messages if true</param>
//        public virtual void RefreshSpecDependantSkills(bool sendMessages)
//        {
//            // refresh specs
//            LoadClassSpecializations(sendMessages);

//            // lock specialization while refreshing...
//            lock (((ICollection)m_specialization).SyncRoot)
//            {
//                foreach (Specialization spec in m_specialization.Values)
//                {
//                    // check for new Abilities
//                    foreach (Ability ab in GetAbilitiesForMimic(spec.KeyName, this, spec.Level))
//                    {
//                        if (!HasAbility(ab.KeyName) || GetAbility(ab.KeyName).Level < ab.Level)
//                            AddAbility(ab, sendMessages);
//                    }

//                    // check for new Styles
//                    foreach (Style st in GetStylesForMimic(spec.KeyName, this, spec.Level))
//                    {
//                        if (st.SpecLevelRequirement == 1 && Level > 5)
//                        {
//                            if (m_styles.ContainsKey(st.ID))
//                                m_styles.Remove(st.ID);

//                            continue;
//                        }

//                        AddStyle(st, sendMessages);
//                    }

//                    // check for new SpellLine
//                    foreach (SpellLine sl in GetSpellLinesForMimic(spec.KeyName, this, spec.Level))
//                    {
//                        AddSpellLine(sl, sendMessages);
//                    }
//                }
//            }
//        }

//        /// <summary>
//        /// Default getter for Ability
//        /// Return Abilities it lists depending on spec level
//        /// Override to change the condition...
//        /// </summary>
//        /// <param name="living"></param>
//        /// <param name="level">level is only used when called for pretending some level (for trainer display)</param>
//        /// <returns></returns>
//        protected virtual List<Ability> GetAbilitiesForMimic(string keyName, GameLiving living, int level)
//        {
//            // Select only Enabled and Max Level Abilities
//            List<Ability> abs = SkillBase.GetSpecAbilityList(keyName, living is MimicNPC ? ((MimicNPC)living).CharacterClass.ID : 0);

//            // Get order of first appearing skills
//            IOrderedEnumerable<Ability> order = abs.GroupBy(item => item.KeyName)
//                .Select(ins => ins.OrderBy(it => it.SpecLevelRequirement).First())
//                .Where(item => item.SpecLevelRequirement <= level)
//                .OrderBy(item => item.SpecLevelRequirement)
//                .ThenBy(item => item.ID);

//            // Get best of skills
//            List<Ability> best = abs.Where(item => item.SpecLevelRequirement <= level)
//                .GroupBy(item => item.KeyName)
//                .Select(ins => ins.OrderByDescending(it => it.SpecLevelRequirement).First()).ToList();

//            List<Ability> results = new List<Ability>();
//            // make some kind of "Join" between the order of appearance and the best abilities.
//            foreach (Ability ab in order)
//            {
//                for (int r = 0; r < best.Count; r++)
//                {
//                    if (best[r].KeyName == ab.KeyName)
//                    {
//                        results.Add(best[r]);
//                        best.RemoveAt(r);
//                        break;
//                    }
//                }
//            }

//            return results;
//        }

//        /// <summary>
//        /// Default Getter For Styles
//        /// Return Styles depending on spec level
//        /// </summary>
//        /// <param name="living"></param>
//        /// <param name="level">level is only used when called for pretending some level (for trainer display)</param>
//        /// <returns></returns>
//        protected virtual List<Style> GetStylesForMimic(string keyName, GameLiving living, int level)
//        {
//            // Try with Class ID 0 if no class id styles
//            int classid = ((MimicNPC)living).CharacterClass.ID;

//            List<Style> styles = SkillBase.GetStyleList(keyName, classid);

//            if (styles.Count == 0)
//                styles = SkillBase.GetStyleList(keyName, 0);

//            // Select only enabled Styles and Order them
//            return styles.Where(item => item.SpecLevelRequirement <= level)
//                .OrderBy(item => item.SpecLevelRequirement)
//                .ThenBy(item => item.ID).ToList();
//        }

//        protected virtual List<SpellLine> GetSpellLinesForMimic(string keyName, GameLiving living, int level)
//        {
//            List<SpellLine> list = new List<SpellLine>();
//            IList<Tuple<SpellLine, int>> spsl = SkillBase.GetSpecsSpellLines(keyName);

//            // Get Spell Lines by order of appearance
//            if (living is MimicNPC)
//            {
//                MimicNPC player = (MimicNPC)living;

//                // select only spec line if is advanced class...
//                var tmp = spsl.Where(item => (item.Item1.IsBaseLine || player.CharacterClass.HasAdvancedFromBaseClass()))
//                    .OrderBy(item => (item.Item1.IsBaseLine ? 0 : 1)).ThenBy(item => item.Item1.ID);

//                // try with class hint
//                var baseline = tmp.Where(item => item.Item1.IsBaseLine && item.Item2 == player.CharacterClass.ID);
//                if (baseline.Any())
//                {
//                    foreach (Tuple<SpellLine, int> ls in baseline)
//                    {
//                        ls.Item1.Level = player.Level;
//                        list.Add(ls.Item1);
//                    }
//                }
//                else
//                {
//                    foreach (Tuple<SpellLine, int> ls in tmp.Where(item => item.Item1.IsBaseLine && item.Item2 == 0))
//                    {
//                        ls.Item1.Level = player.Level;
//                        list.Add(ls.Item1);
//                    }
//                }

//                // try spec with class hint
//                var specline = tmp.Where(item => !item.Item1.IsBaseLine && item.Item2 == player.CharacterClass.ID);
//                if (specline.Any())
//                {
//                    foreach (Tuple<SpellLine, int> ls in specline)
//                    {
//                        ls.Item1.Level = level;
//                        list.Add(ls.Item1);
//                    }
//                }
//                else
//                {
//                    foreach (Tuple<SpellLine, int> ls in tmp.Where(item => !item.Item1.IsBaseLine && item.Item2 == 0))
//                    {
//                        ls.Item1.Level = level;
//                        list.Add(ls.Item1);
//                    }
//                }
//            }
//            else
//            {
//                // default - not a player, add all...
//                foreach (Tuple<SpellLine, int> ls in spsl.OrderBy(item => (item.Item1.IsBaseLine ? 0 : 1)).ThenBy(item => item.Item1.ID))
//                {
//                    // default living spec is (Level * 0.66 + 1) on Live (no real proof...)
//                    // here : Level - (Level / 4) = 0.75
//                    if (ls.Item1.IsBaseLine)
//                        ls.Item1.Level = living.Level;
//                    else
//                        ls.Item1.Level = Math.Max(1, living.Level - (living.Level >> 2));
//                    list.Add(ls.Item1);
//                }
//            }

//            return list;
//        }

//        /// <summary>
//        /// Called by trainer when specialization points were added to a skill
//        /// </summary>
//        /// <param name="skill"></param>
//        public void OnSkillTrained(Specialization skill)
//        {
//            //CharacterClass.OnSkillTrained(this, skill);
//            RefreshSpecDependantSkills(true);

//            //Out.SendUpdatePlayerSkills();
//        }

//        /// <summary>
//        /// effectiveness of the player (resurrection illness)
//        /// Effectiveness is used in physical/magic damage (exept dot), in weapon skill and max concentration formula
//        /// </summary>
//        protected double m_playereffectiveness = 1.0;

//        /// <summary>
//        /// get / set the player's effectiveness.
//        /// Effectiveness is used in physical/magic damage (exept dot), in weapon skill and max concentration
//        /// </summary>
//        public override double Effectiveness
//        {
//            get { return m_playereffectiveness; }
//            set { m_playereffectiveness = value; }
//        }

//        /// <summary>
//        /// Creates new effects list for this living.
//        /// </summary>
//        /// <returns>New effects list instance</returns>
//        protected override GameEffectList CreateEffectsList()
//        {
//            return new MimicGameEffectPlayerList(this);
//        }

//        #endregion Spells/Skills/Abilities/Effects

//        #region Database

//        /// <summary>
//        /// Saves the player's skills
//        /// </summary>
//        protected virtual void SaveSkillsToCharacter()
//        {
//            StringBuilder ab = new StringBuilder();
//            StringBuilder sp = new StringBuilder();

//            // Build Serialized Spec list
//            List<Specialization> specs = null;
//            lock (((ICollection)m_specialization).SyncRoot)
//            {
//                specs = m_specialization.Values.Where(s => s.AllowSave).ToList();
//                foreach (Specialization spec in specs)
//                {
//                    if (sp.Length > 0)
//                    {
//                        sp.Append(";");
//                    }
//                    sp.AppendFormat("{0}|{1}", spec.KeyName, spec.GetSpecLevelForLiving(this));
//                }
//            }

//            // Build Serialized Ability List to save Order
//            foreach (Ability ability in m_usableSkills.Where(e => e.Item1 is Ability).Select(e => e.Item1).Cast<Ability>())
//            {
//                if (ability != null)
//                {
//                    if (ab.Length > 0)
//                    {
//                        ab.Append(";");
//                    }
//                    ab.AppendFormat("{0}|{1}", ability.KeyName, ability.Level);
//                }
//            }

//            // Build Serialized disabled Spell/Ability
//            StringBuilder disabledSpells = new StringBuilder();
//            StringBuilder disabledAbilities = new StringBuilder();

//            ICollection<Skill> disabledSkills = GetAllDisabledSkills();

//            foreach (Skill skill in disabledSkills)
//            {
//                int duration = GetSkillDisabledDuration(skill);

//                if (duration <= 0)
//                    continue;

//                if (skill is Spell)
//                {
//                    Spell spl = (Spell)skill;

//                    if (disabledSpells.Length > 0)
//                        disabledSpells.Append(";");

//                    disabledSpells.AppendFormat("{0}|{1}", spl.ID, duration);
//                }
//                else if (skill is Ability)
//                {
//                    Ability ability = (Ability)skill;

//                    if (disabledAbilities.Length > 0)
//                        disabledAbilities.Append(";");

//                    disabledAbilities.AppendFormat("{0}|{1}", ability.KeyName, duration);
//                }
//                else
//                {
//                    if (log.IsWarnEnabled)
//                        log.WarnFormat("{0}: Can't save disabled skill {1}", Name, skill.GetType().ToString());
//                }
//            }

//            StringBuilder sra = new StringBuilder();

//            foreach (RealmAbility rab in m_realmAbilities)
//            {
//                if (sra.Length > 0)
//                    sra.Append(";");

//                sra.AppendFormat("{0}|{1}", rab.KeyName, rab.Level);
//            }

//            if (DBCharacter != null)
//            {
//                DBCharacter.SerializedAbilities = ab.ToString();
//                DBCharacter.SerializedSpecs = sp.ToString();
//                DBCharacter.SerializedRealmAbilities = sra.ToString();
//                DBCharacter.DisabledSpells = disabledSpells.ToString();
//                DBCharacter.DisabledAbilities = disabledAbilities.ToString();
//            }
//        }

//        /// <summary>
//        /// Loads the Skills from the Character
//        /// Called after the default skills / level have been set!
//        /// </summary>
//        protected virtual void LoadSkillsFromCharacter()
//        {
//            DOLCharacters character = DBCharacter; // if its derived and filled with some code
//            if (character == null) return; // no character => exit

//            #region load class spec

//            // first load spec's career
//            LoadClassSpecializations(false);

//            //Load Remaining spec and levels from Database (custom spec can still be added here...)
//            string tmpStr = character.SerializedSpecs;
//            if (tmpStr != null && tmpStr.Length > 0)
//            {
//                foreach (string spec in Util.SplitCSV(tmpStr))
//                {
//                    string[] values = spec.Split('|');
//                    if (values.Length >= 2)
//                    {
//                        Specialization tempSpec = SkillBase.GetSpecialization(values[0], false);

//                        if (tempSpec != null)
//                        {
//                            if (tempSpec.AllowSave)
//                            {
//                                int level;
//                                if (int.TryParse(values[1], out level))
//                                {
//                                    if (HasSpecialization(tempSpec.KeyName))
//                                    {
//                                        GetSpecializationByName(tempSpec.KeyName).Level = level;
//                                    }
//                                    else
//                                    {
//                                        tempSpec.Level = level;
//                                        AddSpecialization(tempSpec, false);
//                                    }
//                                }
//                                else if (log.IsErrorEnabled)
//                                {
//                                    log.ErrorFormat("{0} : error in loading specs => '{1}'", Name, tmpStr);
//                                }
//                            }
//                        }
//                        else if (log.IsErrorEnabled)
//                        {
//                            log.ErrorFormat("{0}: can't find spec '{1}'", Name, values[0]);
//                        }
//                    }
//                }
//            }

//            // Add Serialized Abilities to keep Database Order
//            // Custom Ability will be disabled as soon as they are not in any specs...
//            tmpStr = character.SerializedAbilities;
//            if (tmpStr != null && tmpStr.Length > 0 && m_usableSkills.Count == 0)
//            {
//                foreach (string abilities in Util.SplitCSV(tmpStr))
//                {
//                    string[] values = abilities.Split('|');
//                    if (values.Length >= 2)
//                    {
//                        int level;
//                        if (int.TryParse(values[1], out level))
//                        {
//                            Ability ability = SkillBase.GetAbility(values[0], level);
//                            if (ability != null)
//                            {
//                                // this is for display order only
//                                m_usableSkills.Add(new Tuple<Skill, Skill>(ability, ability));
//                            }
//                        }
//                    }
//                }
//            }

//            // Retrieve Realm Abilities From Database to be handled by Career Spec
//            tmpStr = character.SerializedRealmAbilities;
//            if (tmpStr != null && tmpStr.Length > 0)
//            {
//                foreach (string abilities in Util.SplitCSV(tmpStr))
//                {
//                    string[] values = abilities.Split('|');
//                    if (values.Length >= 2)
//                    {
//                        int level;
//                        if (int.TryParse(values[1], out level))
//                        {
//                            Ability ability = SkillBase.GetAbility(values[0], level);
//                            if (ability != null && ability is RealmAbility)
//                            {
//                                // this enable realm abilities for Career Computing.
//                                m_realmAbilities.Add((RealmAbility)ability);
//                            }
//                        }
//                    }
//                }
//            }

//            // Load dependent skills
//            RefreshSpecDependantSkills(false);

//            #endregion load class spec

//            #region disable ability

//            //Since we added all the abilities that this character has, let's now disable the disabled ones!
//            tmpStr = character.DisabledAbilities;
//            if (tmpStr != null && tmpStr.Length > 0)
//            {
//                foreach (string str in Util.SplitCSV(tmpStr))
//                {
//                    string[] values = str.Split('|');
//                    if (values.Length >= 2)
//                    {
//                        string keyname = values[0];
//                        int duration;
//                        if (HasAbility(keyname) && int.TryParse(values[1], out duration))
//                        {
//                            DisableSkill(GetAbility(keyname), duration);
//                        }
//                        else if (log.IsErrorEnabled)
//                        {
//                            log.ErrorFormat("{0}: error in loading disabled abilities => '{1}'", Name, tmpStr);
//                        }
//                    }
//                }
//            }

//            #endregion disable ability

//            //Load the disabled spells
//            tmpStr = character.DisabledSpells;
//            if (!string.IsNullOrEmpty(tmpStr))
//            {
//                foreach (string str in Util.SplitCSV(tmpStr))
//                {
//                    string[] values = str.Split('|');
//                    int spellid;
//                    int duration;
//                    if (values.Length >= 2 && int.TryParse(values[0], out spellid) && int.TryParse(values[1], out duration))
//                    {
//                        Spell sp = SkillBase.GetSpellByID(spellid);
//                        // disable
//                        if (sp != null)
//                            DisableSkill(sp, duration);
//                    }
//                    else if (log.IsErrorEnabled)
//                    {
//                        log.ErrorFormat("{0}: error in loading disabled spells => '{1}'", Name, tmpStr);
//                    }
//                }
//            }

//            //CharacterClass.OnLevelUp(this, Level); // load all skills from DB first to keep the order
//            //CharacterClass.OnRealmLevelUp(this);
//        }

//        /// <summary>
//        /// Load this player Classes Specialization.
//        /// </summary>
//        public virtual void LoadClassSpecializations(bool sendMessages)
//        {
//            // Get this Attached Class Specialization from SkillBase.
//            IDictionary<Specialization, int> careers = SkillBase.GetSpecializationCareer(CharacterClass.ID);

//            // Remove All Trainable Specialization or "Career Spec" that aren't managed by This Data Career anymore
//            var speclist = GetSpecList();
//            var careerslist = careers.Keys.Select(k => k.KeyName.ToLower());

//            foreach (var spec in speclist.Where(sp => sp.Trainable || !sp.AllowSave))
//            {
//                if (!careerslist.Contains(spec.KeyName.ToLower()))
//                    RemoveSpecialization(spec.KeyName);
//            }

//            // sort ML Spec depending on ML Line
//            byte mlindex = 0;
//            foreach (KeyValuePair<Specialization, int> constraint in careers)
//            {
//                if (constraint.Key is IMasterLevelsSpecialization)
//                {
//                    //if (mlindex != MLLine)
//                    //{
//                    //	if (HasSpecialization(constraint.Key.KeyName))
//                    //		RemoveSpecialization(constraint.Key.KeyName);

//                    //	mlindex++;
//                    //	continue;
//                    //}

//                    //mlindex++;

//                    //if (!MLGranted || MLLevel < 1)
//                    //{
//                    //	continue;
//                    //}
//                }

//                // load if the spec doesn't exists
//                if (Level >= constraint.Value)
//                {
//                    if (!HasSpecialization(constraint.Key.KeyName))
//                        AddSpecialization(constraint.Key, sendMessages);
//                }
//                else
//                {
//                    if (HasSpecialization(constraint.Key.KeyName))
//                        RemoveSpecialization(constraint.Key.KeyName);
//                }
//            }
//        }

//        /// <summary>
//        /// Verify this player has the correct number of spec points for the players level
//        /// </summary>
//        public virtual int VerifySpecPoints()
//        {
//            // calc normal spec points for the level & classe
//            int allpoints = -1;
//            for (int i = 1; i <= Level; i++)
//            {
//                if (i <= 5) allpoints += i; //start levels
//                if (i > 5) allpoints += CharacterClass.SpecPointsMultiplier * i / 10; //normal levels
//                if (i > 40) allpoints += CharacterClass.SpecPointsMultiplier * (i - 1) / 20; //half levels
//            }
//            if (IsLevelSecondStage && Level != MaxLevel)
//                allpoints += CharacterClass.SpecPointsMultiplier * Level / 20; // add current half level

//            // calc spec points player have (autotrain is not anymore processed here - 1.87 livelike)
//            int usedpoints = 0;
//            foreach (Specialization spec in GetSpecList().Where(e => e.Trainable))
//            {
//                usedpoints += (spec.Level * (spec.Level + 1) - 2) / 2;
//                usedpoints -= GetAutoTrainPoints(spec, 0);
//            }

//            allpoints -= usedpoints;

//            // check if correct, if not respec. Not applicable to GMs
//            if (allpoints < 0)
//            {
//                log.WarnFormat("Spec points total for player {0} incorrect: {1} instead of {2}.", Name, usedpoints, allpoints + usedpoints);
//                RespecAllLines();
//                return allpoints + usedpoints;
//            }

//            return allpoints;
//        }

//        /// <summary>
//        /// Loads this player from a character table slot
//        /// </summary>
//        /// <param name="obj">DOLCharacter</param>
//        //public override void LoadFromDatabase(DataObject obj)
//        //{
//        //	base.LoadFromDatabase(obj);
//        //	if (!(obj is DOLCharacters))
//        //		return;
//        //	m_dbCharacter = (DOLCharacters)obj;

//        //	// Money
//        //	m_Copper = DBCharacter.Copper;
//        //	m_Silver = DBCharacter.Silver;
//        //	m_Gold = DBCharacter.Gold;
//        //	m_Platinum = DBCharacter.Platinum;
//        //	m_Mithril = DBCharacter.Mithril;

//        //	Model = (ushort)DBCharacter.CurrentModel;

//        //	m_customFaceAttributes[(int)eCharFacePart.EyeSize] = DBCharacter.EyeSize;
//        //	m_customFaceAttributes[(int)eCharFacePart.LipSize] = DBCharacter.LipSize;
//        //	m_customFaceAttributes[(int)eCharFacePart.EyeColor] = DBCharacter.EyeColor;
//        //	m_customFaceAttributes[(int)eCharFacePart.HairColor] = DBCharacter.HairColor;
//        //	m_customFaceAttributes[(int)eCharFacePart.FaceType] = DBCharacter.FaceType;
//        //	m_customFaceAttributes[(int)eCharFacePart.HairStyle] = DBCharacter.HairStyle;
//        //	m_customFaceAttributes[(int)eCharFacePart.MoodType] = DBCharacter.MoodType;

//        //	#region guild handling
//        //	//TODO: overwork guild handling (VaNaTiC)
//        //	m_guildId = DBCharacter.GuildID;
//        //	if (m_guildId != null)
//        //		m_guild = GuildMgr.GetGuildByGuildID(m_guildId);
//        //	else
//        //		m_guild = null;

//        //	if (m_guild != null)
//        //	{
//        //		foreach (DBRank rank in m_guild.Ranks)
//        //		{
//        //			if (rank == null) continue;
//        //			if (rank.RankLevel == DBCharacter.GuildRank)
//        //			{
//        //				m_guildRank = rank;
//        //				break;
//        //			}
//        //		}

//        //		m_guildName = m_guild.Name;
//        //		m_guild.AddOnlineMember(this);
//        //	}
//        //	#endregion

//        //	#region setting world-init-position (delegate to PlayerCharacter dont make sense)
//        //	m_x = DBCharacter.Xpos;
//        //	m_y = DBCharacter.Ypos;
//        //	m_z = DBCharacter.Zpos;
//        //	m_Heading = (ushort)DBCharacter.Direction;
//        //	//important, use CurrentRegion property
//        //	//instead because it sets the Region too
//        //	CurrentRegionID = (ushort)DBCharacter.Region;
//        //	if (CurrentRegion == null || CurrentRegion.GetZone(m_x, m_y) == null)
//        //	{
//        //		log.WarnFormat("Invalid region/zone on char load ({0}): x={1} y={2} z={3} reg={4}; moving to bind point.", DBCharacter.Name, X, Y, Z, DBCharacter.Region);
//        //		m_x = DBCharacter.BindXpos;
//        //		m_y = DBCharacter.BindYpos;
//        //		m_z = DBCharacter.BindZpos;
//        //		m_Heading = (ushort)DBCharacter.BindHeading;
//        //		CurrentRegionID = (ushort)DBCharacter.BindRegion;
//        //	}

//        //	for (int i = 0; i < m_lastUniqueLocations.Length; i++)
//        //	{
//        //		m_lastUniqueLocations[i] = new GameLocation(null, CurrentRegionID, m_x, m_y, m_z);
//        //	}
//        //	#endregion

//        //	// stats first
//        //	m_charStat[eStat.STR - eStat._First] = (short)DBCharacter.Strength;
//        //	m_charStat[eStat.DEX - eStat._First] = (short)DBCharacter.Dexterity;
//        //	m_charStat[eStat.CON - eStat._First] = (short)DBCharacter.Constitution;
//        //	m_charStat[eStat.QUI - eStat._First] = (short)DBCharacter.Quickness;
//        //	m_charStat[eStat.INT - eStat._First] = (short)DBCharacter.Intelligence;
//        //	m_charStat[eStat.PIE - eStat._First] = (short)DBCharacter.Piety;
//        //	m_charStat[eStat.EMP - eStat._First] = (short)DBCharacter.Empathy;
//        //	m_charStat[eStat.CHR - eStat._First] = (short)DBCharacter.Charisma;

//        //	SetCharacterClass(DBCharacter.Class);

//        //	m_currentSpeed = 0;
//        //	if (MaxSpeedBase == 0)
//        //		MaxSpeedBase = PLAYER_BASE_SPEED;

//        //	m_inventory.LoadFromDatabase(InternalID);

//        //	SwitchQuiver((eActiveQuiverSlot)(DBCharacter.ActiveWeaponSlot & 0xF0), false);
//        //	SwitchWeapon((eActiveWeaponSlot)(DBCharacter.ActiveWeaponSlot & 0x0F));

//        //	if (DBCharacter.PlayedTime < 1) //added to make character start with 100% Health and Mana/Endurance when DB Start Lvl >1 :Loki
//        //	{
//        //		Health = MaxHealth;
//        //		Mana = MaxMana;
//        //		Endurance = MaxEndurance;
//        //	}
//        //	else
//        //	{
//        //		Health = DBCharacter.Health;
//        //		Mana = DBCharacter.Mana;
//        //		Endurance = DBCharacter.Endurance; // has to be set after max, same applies to other values with max properties
//        //	}

//        //	if (Health <= 0)
//        //	{
//        //		Health = 1;
//        //	}

//        //	if (RealmLevel == 0)
//        //		RealmLevel = CalculateRealmLevelFromRPs(RealmPoints);

//        //	//Need to load the skills at the end, so the stored values modify the
//        //	//existing skill levels for this player
//        //	LoadSkillsFromCharacter();
//        //	LoadCraftingSkills();

//        //	VerifySpecPoints();

//        //	LoadQuests();

//        //	// Load Task object of player ...
//        //	var tasks = DOLDB<DBTask>.SelectObjects(DB.Column(nameof(DBTask.Character_ID)).IsEqualTo(InternalID));
//        //	if (tasks.Count == 1)
//        //	{
//        //		m_task = AbstractTask.LoadFromDatabase(this, tasks[0]);
//        //	}
//        //	else if (tasks.Count > 1)
//        //	{
//        //		if (log.IsErrorEnabled)
//        //			log.Error("More than one DBTask Object found for player " + Name);
//        //	}

//        //	// Load ML steps of player ...
//        //	var mlsteps = DOLDB<DBCharacterXMasterLevel>.SelectObjects(DB.Column(nameof(DBCharacterXMasterLevel.Character_ID)).IsEqualTo(QuestPlayerID));
//        //	if (mlsteps.Count > 0)
//        //	{
//        //		foreach (DBCharacterXMasterLevel mlstep in mlsteps)
//        //			m_mlSteps.Add(mlstep);
//        //	}

//        //	m_previousLoginDate = DBCharacter.LastPlayed;

//        //	// Has to be updated on load to ensure time offline isn't added to character /played.
//        //	DBCharacter.LastPlayed = DateTime.Now;

//        //	m_titles.Clear();
//        //	foreach (IPlayerTitle ttl in PlayerTitleMgr.GetPlayerTitles(this))
//        //		m_titles.Add(ttl);

//        //	IPlayerTitle t = PlayerTitleMgr.GetTitleByTypeName(DBCharacter.CurrentTitleType);
//        //	if (t == null)
//        //		t = PlayerTitleMgr.ClearTitle;
//        //	m_currentTitle = t;

//        //	//let's only check if we can use /level once shall we,
//        //	//this is nice because i want to check the property often for the new catacombs classes

//        //	//find all characters in the database
//        //	foreach (DOLCharacters plr in Client.Account.Characters)
//        //	{
//        //		//where the level of one of the characters if 50
//        //		if (plr.Level == ServerProperties.Properties.SLASH_LEVEL_REQUIREMENT && GameServer.ServerRules.CountsTowardsSlashLevel(plr))
//        //		{
//        //			m_canUseSlashLevel = true;
//        //			break;
//        //		}
//        //	}

//        //	// check the account for the Muted flag
//        //	if (Client.Account.IsMuted)
//        //		IsMuted = true;
//        //}

//        /// <summary>
//        /// Save the player into the database
//        /// </summary>
//        //public override void SaveIntoDatabase()
//        //{
//        //	try
//        //	{
//        //		// Ff this player is a GM always check and set the IgnoreStatistics flag
//        //		if (Client.Account.PrivLevel > (uint)ePrivLevel.Player && DBCharacter.IgnoreStatistics == false)
//        //		{
//        //			DBCharacter.IgnoreStatistics = true;
//        //		}

//        //		SaveSkillsToCharacter();
//        //		SaveCraftingSkills();
//        //		DBCharacter.PlayedTime = PlayedTime;  //We have to set the PlayedTime on the character before setting the LastPlayed
//        //		DBCharacter.LastPlayed = DateTime.Now;

//        //		DBCharacter.ActiveWeaponSlot = (byte)((byte)ActiveWeaponSlot | (byte)ActiveQuiverSlot);
//        //		if (m_stuckFlag)
//        //		{
//        //			lock (m_lastUniqueLocations)
//        //			{
//        //				GameLocation loc = m_lastUniqueLocations[m_lastUniqueLocations.Length - 1];
//        //				DBCharacter.Xpos = loc.X;
//        //				DBCharacter.Ypos = loc.Y;
//        //				DBCharacter.Zpos = loc.Z;
//        //				DBCharacter.Region = loc.RegionID;
//        //				DBCharacter.Direction = loc.Heading;
//        //			}
//        //		}
//        //		GameServer.Database.SaveObject(DBCharacter);
//        //		Inventory.SaveIntoDatabase(InternalID);

//        //		DOLCharacters cachedCharacter = null;

//        //		foreach (DOLCharacters accountChar in Client.Account.Characters)
//        //		{
//        //			if (accountChar.ObjectId == InternalID)
//        //			{
//        //				cachedCharacter = accountChar;
//        //				break;
//        //			}
//        //		}

//        //		if (cachedCharacter != null)
//        //		{
//        //			cachedCharacter = DBCharacter;
//        //		}

//        //		if (m_mlSteps != null)
//        //			GameServer.Database.SaveObject(m_mlSteps.OfType<DBCharacterXMasterLevel>());

//        //		if (log.IsInfoEnabled)
//        //			log.InfoFormat("{0} saved!", DBCharacter.Name);
//        //		Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.SaveIntoDatabase.CharacterSaved"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
//        //	}
//        //	catch (Exception e)
//        //	{
//        //		if (log.IsErrorEnabled)
//        //			log.ErrorFormat("Error saving player {0}! - {1}", Name, e);
//        //	}
//        //}

//        #endregion Database

//        private bool m_isTorchLighted = false;

//        /// <summary>
//        /// Is player Torch lighted ?
//        /// </summary>
//        public bool IsTorchLighted
//        {
//            get { return m_isTorchLighted; }
//            set { m_isTorchLighted = value; }
//        }

//        #region Combat

//        private MimicNPC GetMimicAttacker(GameLiving living)
//        {
//            if (living is MimicNPC)
//                return living as MimicNPC;

//            GameNPC npc = living as GameNPC;

//            if (npc != null)
//            {
//                if (npc.Brain is IControlledBrain && (npc.Brain as IControlledBrain).Owner is GamePlayer)
//                    return (npc.Brain as IControlledBrain).Owner as MimicNPC;
//            }

//            return null;
//        }

//        /// <summary>
//        /// Returns the result of an enemy attack,
//        /// yes this means WE decide if an enemy hits us or not :-)
//        /// </summary>
//        /// <param name="ad">AttackData</param>
//        /// <param name="weapon">the weapon used for attack</param>
//        /// <returns>the result of the attack</returns>
//        public override eAttackResult CalculateEnemyAttackResult(AttackData ad, InventoryItem weapon)
//        {
//            if (!IsValidTarget)
//                return eAttackResult.NoValidTarget;

//            //1.To-Hit modifiers on styles do not any effect on whether your opponent successfully Evades, Blocks, or Parries.  Grab Bag 2/27/03
//            //2.The correct Order of Resolution in combat is Intercept, Evade, Parry, Block (Shield), Guard, Hit/Miss, and then Bladeturn.  Grab Bag 2/27/03, Grab Bag 4/4/03
//            //3.For every person attacking a monster, a small bonus is applied to each player's chance to hit the enemy. Allowances are made for those who don't technically hit things when they are participating in the raid  for example, a healer gets credit for attacking a monster when he heals someone who is attacking the monster, because that's what he does in a battle.  Grab Bag 6/6/03
//            //4.Block, parry, and bolt attacks are affected by this code, as you know. We made a fix to how the code counts people as "in combat." Before this patch, everyone grouped and on the raid was counted as "in combat." The guy AFK getting Mountain Dew was in combat, the level five guy hovering in the back and hoovering up some exp was in combat  if they were grouped with SOMEONE fighting, they were in combat. This was a bad thing for block, parry, and bolt users, and so we fixed it.  Grab Bag 6/6/03
//            //5.Positional degrees - Side Positional combat styles now will work an extra 15 degrees towards the rear of an opponent, and rear position styles work in a 60 degree arc rather than the original 90 degree standard. This change should even out the difficulty between side and rear positional combat styles, which have the same damage bonus. Please note that front positional styles are not affected by this change. 1.62
//            //http://daoc.catacombs.com/forum.cfm?ThreadKey=511&DefMessage=681444&forum=DAOCMainForum#Defense

//            GuardEffect guard = null;
//            DashingDefenseEffect dashing = null;
//            InterceptEffect intercept = null;
//            GameSpellEffect bladeturn = null;
//            EngageEffect engage = null;
//            // ML effects
//            GameSpellEffect phaseshift = null;
//            GameSpellEffect grapple = null;
//            GameSpellEffect brittleguard = null;

//            AttackData lastAD = TempProperties.getProperty<AttackData>(LAST_ATTACK_DATA, null);
//            bool defenseDisabled = ad.Target.IsMezzed | ad.Target.IsStunned | ad.Target.IsSitting;

//            // If berserk is on, no defensive skills may be used: evade, parry, ...
//            // unfortunately this as to be check for every action itself to kepp oder of actions the same.
//            // Intercept and guard can still be used on berserked
//            //			BerserkEffect berserk = null;

//            // get all needed effects in one loop
//            lock (EffectList)
//            {
//                foreach (IGameEffect effect in EffectList)
//                {
//                    if (effect is GuardEffect)
//                    {
//                        if (guard == null && ((GuardEffect)effect).GuardTarget == this)
//                            guard = (GuardEffect)effect;
//                        continue;
//                    }

//                    //if (effect is DashingDefenseEffect)
//                    //{
//                    //    if (dashing == null && ((DashingDefenseEffect)effect).GuardTarget == this)
//                    //        dashing = (DashingDefenseEffect)effect; //Dashing
//                    //    continue;
//                    //}

//                    if (effect is BerserkEffect)
//                    {
//                        defenseDisabled = true;
//                        continue;
//                    }

//                    if (effect is EngageEffect)
//                    {
//                        if (engage == null)
//                            engage = (EngageEffect)effect;
//                        continue;
//                    }

//                    if (effect is GameSpellEffect)
//                    {
//                        switch ((effect as GameSpellEffect).Spell.SpellType)
//                        {
//                            case "Phaseshift":
//                            if (phaseshift == null)
//                                phaseshift = (GameSpellEffect)effect;
//                            continue;
//                            case "Grapple":
//                            if (grapple == null)
//                                grapple = (GameSpellEffect)effect;
//                            continue;
//                            case "BrittleGuard":
//                            if (brittleguard == null)
//                                brittleguard = (GameSpellEffect)effect;
//                            continue;
//                            case "Bladeturn":
//                            if (bladeturn == null)
//                                bladeturn = (GameSpellEffect)effect;
//                            continue;
//                        }
//                    }

//                    // We check if interceptor can intercept

//                    // we can only intercept attacks on livings, and can only intercept when active
//                    // you cannot intercept while you are sitting
//                    // if you are stuned or mesmeried you cannot intercept...
//                    InterceptEffect inter = effect as InterceptEffect;
//                    if (intercept == null && inter != null && inter.InterceptTarget == this && !inter.InterceptSource.IsStunned && !inter.InterceptSource.IsMezzed
//                        && !inter.InterceptSource.IsSitting && inter.InterceptSource.ObjectState == eObjectState.Active && inter.InterceptSource.IsAlive
//                        && this.IsWithinRadius(inter.InterceptSource, InterceptAbilityHandler.INTERCEPT_DISTANCE) && Util.Chance(inter.InterceptChance))
//                    {
//                        intercept = inter;
//                        continue;
//                    }
//                }
//            }

//            bool stealthStyle = false;
//            if (ad.Style != null && ad.Style.StealthRequirement && ad.Attacker is MimicNPC && StyleProcessor.CanUseStyle((MimicNPC)ad.Attacker, ad.Style, weapon))
//            {
//                stealthStyle = true;
//                defenseDisabled = true;
//                //Eden - brittle guard should not intercept PA
//                intercept = null;
//                brittleguard = null;
//            }

//            // Bodyguard - the Aredhel way. Alas, this is not perfect yet as clearly,
//            // this code belongs in GamePlayer, but it's a start to end this clutter.
//            // Temporarily saving the below information here.
//            // Defensive chances (evade/parry) are reduced by 20%, but target of bodyguard
//            // can't be attacked in melee until bodyguard is killed or moves out of range.

//            //if (this is MimicNPC)
//            //{
//            //	MimicNPC mimicAttacker = GetMimicAttacker(ad.Attacker);

//            //	if (mimicAttacker != null)
//            //	{
//            //		GameLiving attacker = ad.Attacker;

//            //		if (attacker.ActiveWeaponSlot != eActiveWeaponSlot.Distance)
//            //		{
//            //			MimicNPC target = this as MimicNPC;
//            //			MimicNPC bodyguard = target.Bodyguard;
//            //			if (bodyguard != null)
//            //			{
//            //				target.Out.SendMessage(String.Format(LanguageMgr.GetTranslation(target.Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.YouWereProtected"), bodyguard.Name, attacker.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);

//            //				bodyguard.Out.SendMessage(String.Format(LanguageMgr.GetTranslation(bodyguard.Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.YouHaveProtected"), target.Name, attacker.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);

//            //				if (attacker == mimicAttacker)
//            //					mimicAttacker.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(mimicAttacker.Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.YouAttempt"), target.Name, target.Name, bodyguard.Name), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
//            //				else
//            //					mimicAttacker.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(mimicAttacker.Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.YourPetAttempts"), target.Name, target.Name, bodyguard.Name), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
//            //				return eAttackResult.Bodyguarded;
//            //			}
//            //		}
//            //	}
//            //}

//            if (phaseshift != null)
//                return eAttackResult.Missed;

//            if (grapple != null)
//                return eAttackResult.Grappled;

//            //if (brittleguard != null)
//            //{
//            //	if (this is GamePlayer)
//            //		((GamePlayer)this).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)this).Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.BlowIntercepted"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
//            //	if (ad.Attacker is GamePlayer)
//            //		((GamePlayer)ad.Attacker).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)ad.Attacker).Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.StrikeIntercepted"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
//            //	brittleguard.Cancel(false);
//            //	return eAttackResult.Missed;
//            //}

//            if (intercept != null && !stealthStyle)
//            {
//                ad.Target = intercept.InterceptSource;
//                if (intercept.InterceptSource is GamePlayer || intercept.InterceptSource is MimicNPC)
//                    intercept.Cancel(false); // can be canceled only outside of the loop
//                return eAttackResult.HitUnstyled;
//            }

//            // i am defender, what con is attacker to me?
//            // orange+ should make it harder to block/evade/parry
//            double attackerConLevel = -GetConLevel(ad.Attacker);
//            //			double levelModifier = -((ad.Attacker.Level - Level) / (Level / 10.0 + 1));

//            int attackerCount = m_attackers.Count;

//            if (!defenseDisabled)
//            {
//                double evadeChance = TryEvade(ad, lastAD, attackerConLevel, attackerCount);

//                if (Util.ChanceDouble(evadeChance))
//                    return eAttackResult.Evaded;

//                if (ad.IsMeleeAttack)
//                {
//                    double parryChance = TryParry(ad, lastAD, attackerConLevel, attackerCount);

//                    if (Util.ChanceDouble(parryChance))
//                        return eAttackResult.Parried;
//                }

//                double blockChance = TryBlock(ad, lastAD, attackerConLevel, attackerCount, engage);

//                if (Util.ChanceDouble(blockChance))
//                {
//                    // reactive effects on block moved to GamePlayer
//                    return eAttackResult.Blocked;
//                }
//            }

//            // Guard
//            if (guard != null &&
//                guard.GuardSource.ObjectState == eObjectState.Active &&
//                guard.GuardSource.IsStunned == false &&
//                guard.GuardSource.IsMezzed == false &&
//                guard.GuardSource.ActiveWeaponSlot != eActiveWeaponSlot.Distance &&
//                //				guard.GuardSource.AttackState &&
//                guard.GuardSource.IsAlive &&
//                !stealthStyle)
//            {
//                // check distance
//                if (guard.GuardSource.IsWithinRadius(guard.GuardTarget, GuardAbilityHandler.GUARD_DISTANCE))
//                {
//                    // check player is wearing shield and NO two handed weapon
//                    InventoryItem leftHand = guard.GuardSource.Inventory.GetItem(eInventorySlot.LeftHandWeapon);
//                    InventoryItem rightHand = guard.GuardSource.AttackWeapon;

//                    if (((rightHand == null || rightHand.Hand != 1) && leftHand != null && leftHand.Object_Type == (int)eObjectType.Shield) || guard.GuardSource is GameNPC)
//                    {
//                        // TODO
//                        // insert actual formula for guarding here, this is just a guessed one based on block.
//                        int guardLevel = guard.GuardSource.GetAbilityLevel(GS.Abilities.Guard); // multiply by 3 to be a bit qorse than block (block woudl be 5 since you get guard I with shield 5, guard II with shield 10 and guard III with shield 15)
//                        double guardchance = 0;

//                        if (guard.GuardSource is GameNPC)
//                            guardchance = guard.GuardSource.GetModified(eProperty.BlockChance) * 0.001;
//                        else
//                            guardchance = guard.GuardSource.GetModified(eProperty.BlockChance) * leftHand.Quality * 0.00001;

//                        guardchance *= guardLevel * 0.3 + 0.05;

//                        guardchance += attackerConLevel * 0.05;

//                        int shieldSize = 0;

//                        if (leftHand != null)
//                            shieldSize = leftHand.Type_Damage;

//                        if (guard.GuardSource is GameNPC)
//                            shieldSize = 1;

//                        if (guardchance < 0.01)
//                            guardchance = 0.01;
//                        else if (ad.Attacker is GamePlayer || ad.Attacker is MimicNPC && guardchance > .6)
//                            guardchance = .6;
//                        else if (shieldSize == 1 && ad.Attacker is GameNPC && guardchance > .8)
//                            guardchance = .8;
//                        else if (shieldSize == 2 && ad.Attacker is GameNPC && guardchance > .9)
//                            guardchance = .9;
//                        else if (shieldSize == 3 && ad.Attacker is GameNPC && guardchance > .99)
//                            guardchance = .99;

//                        if (ad.AttackType == AttackData.eAttackType.MeleeDualWield)
//                            guardchance /= 2;

//                        if (Util.ChanceDouble(guardchance))
//                        {
//                            ad.Target = guard.GuardSource;
//                            return eAttackResult.Blocked;
//                        }
//                    }
//                }
//            }

//            //Dashing Defense
//            if (dashing != null &&
//                dashing.GuardSource.ObjectState == eObjectState.Active &&
//                dashing.GuardSource.IsStunned == false &&
//                dashing.GuardSource.IsMezzed == false &&
//                dashing.GuardSource.ActiveWeaponSlot != eActiveWeaponSlot.Distance &&
//                dashing.GuardSource.IsAlive &&
//                !stealthStyle)
//            {
//                // check distance
//                if (dashing.GuardSource.IsWithinRadius(dashing.GuardTarget, DashingDefenseEffect.GUARD_DISTANCE))
//                {
//                    // check player is wearing shield and NO two handed weapon
//                    InventoryItem leftHand = dashing.GuardSource.Inventory.GetItem(eInventorySlot.LeftHandWeapon);
//                    InventoryItem rightHand = dashing.GuardSource.AttackWeapon;
//                    InventoryItem twoHand = dashing.GuardSource.Inventory.GetItem(eInventorySlot.TwoHandWeapon);

//                    if ((rightHand == null || rightHand.Hand != 1) && leftHand != null && leftHand.Object_Type == (int)eObjectType.Shield)
//                    {
//                        int guardLevel = dashing.GuardSource.GetAbilityLevel(GS.Abilities.Guard); // multiply by 3 to be a bit qorse than block (block woudl be 5 since you get guard I with shield 5, guard II with shield 10 and guard III with shield 15)
//                        double guardchance = dashing.GuardSource.GetModified(eProperty.BlockChance) * leftHand.Quality * 0.00001;
//                        guardchance *= guardLevel * 0.25 + 0.05;
//                        guardchance += attackerConLevel * 0.05;

//                        if (guardchance > 0.99)
//                            guardchance = 0.99;

//                        if (guardchance < 0.01)
//                            guardchance = 0.01;

//                        int shieldSize = 0;

//                        if (leftHand != null)
//                            shieldSize = leftHand.Type_Damage;
//                        if (m_attackers.Count > shieldSize)
//                            guardchance /= (m_attackers.Count - shieldSize + 1);
//                        if (ad.AttackType == AttackData.eAttackType.MeleeDualWield) guardchance /= 2;

//                        double parrychance = double.MinValue;
//                        parrychance = dashing.GuardSource.GetModified(eProperty.ParryChance);
//                        if (parrychance != double.MinValue)
//                        {
//                            parrychance *= 0.001;
//                            parrychance += 0.05 * attackerConLevel;
//                            if (parrychance > 0.99) parrychance = 0.99;
//                            if (parrychance < 0.01) parrychance = 0.01;
//                            if (m_attackers.Count > 1) parrychance /= m_attackers.Count / 2;
//                        }

//                        if (Util.ChanceDouble(guardchance))
//                        {
//                            ad.Target = dashing.GuardSource;
//                            return eAttackResult.Blocked;
//                        }
//                        else if (Util.ChanceDouble(parrychance))
//                        {
//                            ad.Target = dashing.GuardSource;
//                            return eAttackResult.Parried;
//                        }
//                    }
//                    //Check if Player is wearing Twohanded Weapon or nothing in the lefthand slot
//                    else
//                    {
//                        double parrychance = double.MinValue;

//                        parrychance = dashing.GuardSource.GetModified(eProperty.ParryChance);

//                        if (parrychance != double.MinValue)
//                        {
//                            parrychance *= 0.001;
//                            parrychance += 0.05 * attackerConLevel;

//                            if (parrychance > 0.99)
//                                parrychance = 0.99;

//                            if (parrychance < 0.01)
//                                parrychance = 0.01;

//                            if (m_attackers.Count > 1)
//                                parrychance /= m_attackers.Count / 2;
//                        }
//                        if (Util.ChanceDouble(parrychance))
//                        {
//                            ad.Target = dashing.GuardSource;
//                            return eAttackResult.Parried;
//                        }
//                    }
//                }
//            }

//            // Missrate
//            int missrate = (ad.Attacker is GamePlayer || ad.Attacker is MimicNPC) ? 20 : 25; //player vs player tests show 20% miss on any level
//            missrate -= ad.Attacker.GetModified(eProperty.ToHitBonus);

//            // experimental missrate adjustment for number of attackers
//            if ((this is MimicNPC && ad.Attacker is GamePlayer || ad.Attacker is MimicNPC) == false)
//            {
//                missrate -= (Math.Max(0, Attackers.Count - 1) * ServerProperties.Properties.MISSRATE_REDUCTION_PER_ATTACKERS);
//            }

//            // weapon/armor bonus
//            int armorBonus = 0;
//            if (ad.Target is MimicNPC)
//            {
//                ad.ArmorHitLocation = ((MimicNPC)ad.Target).CalculateArmorHitLocation(ad);

//                InventoryItem armor = null;

//                if (ad.Target.Inventory != null)
//                    armor = ad.Target.Inventory.GetItem((eInventorySlot)ad.ArmorHitLocation);

//                if (armor != null)
//                    armorBonus = armor.Bonus;
//            }

//            if (weapon != null)
//            {
//                armorBonus -= weapon.Bonus;
//            }

//            if (ad.Target is MimicNPC && (ad.Attacker is GamePlayer || ad.Attacker is MimicNPC))
//            {
//                missrate += armorBonus;
//            }
//            else
//            {
//                missrate += missrate * armorBonus / 100;
//            }

//            if (ad.Style != null)
//            {
//                missrate -= ad.Style.BonusToHit; // add style bonus
//            }

//            if (lastAD != null && lastAD.AttackResult == eAttackResult.HitStyle && lastAD.Style != null)
//            {
//                // add defence bonus from last executed style if any
//                missrate += lastAD.Style.BonusToDefense;
//            }

//            if (this is MimicNPC && ad.Attacker is GamePlayer || ad.Attacker is MimicNPC && weapon != null)
//            {
//                missrate -= (int)((ad.Attacker.WeaponSpecLevel(weapon) - 1) * 0.1);
//            }

//            if (ad.Attacker.ActiveWeaponSlot == eActiveWeaponSlot.Distance)
//            {
//                InventoryItem ammo = RangeAttackAmmo;
//                if (ammo != null)
//                    switch ((ammo.SPD_ABS >> 4) & 0x3)
//                    {
//                        // http://rothwellhome.org/guides/archery.htm
//                        case 0: missrate += 15; break; // Rough
//                                                       //						case 1: missrate -= 0; break;
//                        case 2: missrate -= 15; break; // doesn't exist (?)
//                        case 3: missrate -= 25; break; // Footed
//                    }
//            }
//            if (this is MimicNPC && ((MimicNPC)this).IsSitting)
//            {
//                missrate >>= 1; //halved
//            }

//            if (Util.Chance(missrate))
//            {
//                return eAttackResult.Missed;
//            }

//            if (ad.IsRandomFumble)
//                return eAttackResult.Fumbled;

//            if (ad.IsRandomMiss)
//                return eAttackResult.Missed;

//            // Bladeturn
//            // TODO: high level mob attackers penetrate bt, players are tested and do not penetrate (lv30 vs lv20)
//            /*
//			 * http://www.camelotherald.com/more/31.shtml
//			 * - Bladeturns can now be penetrated by attacks from higher level monsters and
//			 * players. The chance of the bladeturn deflecting a higher level attack is
//			 * approximately the caster's level / the attacker's level.
//			 * Please be aware that everything in the game is
//			 * level/chance based - nothing works 100% of the time in all cases.
//			 * It was a bug that caused it to work 100% of the time - now it takes the
//			 * levels of the players involved into account.
//			 */
//            // "The blow penetrated the magical barrier!"
//            if (bladeturn != null)
//            {
//                bool penetrate = false;

//                if (stealthStyle)
//                    penetrate = true;

//                if (ad.Attacker.RangedAttackType == eRangedAttackType.Long // stealth styles pierce bladeturn
//                    || (ad.AttackType == AttackData.eAttackType.Ranged && ad.Target != bladeturn.SpellHandler.Caster && ad.Attacker is GamePlayer && ((GamePlayer)ad.Attacker).HasAbility(GS.Abilities.PenetratingArrow)))  // penetrating arrow attack pierce bladeturn
//                    penetrate = true;

//                if (ad.IsMeleeAttack && !Util.ChanceDouble((double)bladeturn.SpellHandler.Caster.Level / (double)ad.Attacker.Level))
//                    penetrate = true;
//                if (penetrate)
//                {
//                    bladeturn.Cancel(false);
//                }
//                else
//                {
//                    bladeturn.Cancel(false);
//                    if (this is MimicNPC)
//                        ((MimicNPC)this).Stealth(false);
//                    return eAttackResult.Missed;
//                }
//            }

//            //if (this is GamePlayer && ((GamePlayer)this).IsOnHorse)
//            //	((GamePlayer)this).IsOnHorse = false;

//            return eAttackResult.HitUnstyled;
//        }

//        protected override double TryEvade(AttackData ad, AttackData lastAD, double attackerConLevel, int attackerCount)
//        {
//            // Evade
//            // 1. A: It isn't possible to give a simple answer. The formula includes such elements
//            // as your level, your target's level, your level of evade, your QUI, your DEX, your
//            // buffs to QUI and DEX, the number of people attacking you, your target's weapon
//            // level, your target's spec in the weapon he is wielding, the kind of attack (DW,
//            // ranged, etc), attack radius, angle of attack, the style you used most recently,
//            // target's offensive RA, debuffs, and a few others. (The type of weapon - large, 1H,
//            // etc - doesn't matter.) ...."

//            double evadeChance = 0;
//            MimicNPC mimic = this as MimicNPC;

//            GameSpellEffect evadeBuff = SpellHandler.FindEffectOnTarget(this, "EvadeBuff");

//            if (evadeBuff == null)
//                evadeBuff = SpellHandler.FindEffectOnTarget(this, "SavageEvadeBuff");

//            if (mimic != null)
//            {
//                if (mimic.HasAbility(GS.Abilities.Advanced_Evade) ||
//                    mimic.EffectList.GetOfType<CombatAwarenessEffect>() != null ||
//                    mimic.EffectList.GetOfType<RuneOfUtterAgilityEffect>() != null)
//                    evadeChance = GetModified(eProperty.EvadeChance);
//                else if (IsObjectInFront(ad.Attacker, 180) && (evadeBuff != null || mimic.HasAbility(GS.Abilities.Evade)))
//                {
//                    int res = GetModified(eProperty.EvadeChance);
//                    if (res > 0)
//                        evadeChance = res;
//                }
//            }

//            if (evadeChance > 0 && !ad.Target.IsStunned && !ad.Target.IsSitting)
//            {
//                if (attackerCount > 1)
//                    evadeChance -= (attackerCount - 1) * 0.03;

//                evadeChance *= 0.001;
//                evadeChance += 0.01 * attackerConLevel; // 1% per con level distance multiplied by evade level

//                if (lastAD != null && lastAD.Style != null)
//                {
//                    evadeChance += lastAD.Style.BonusToDefense * 0.01;
//                }

//                if (ad.AttackType == AttackData.eAttackType.Ranged)
//                    evadeChance /= 5.0;

//                if (evadeChance < 0.01)
//                    evadeChance = 0.01;
//                else if (evadeChance > ServerProperties.Properties.EVADE_CAP && ad.Attacker is GamePlayer && ad.Target is GamePlayer)
//                    evadeChance = ServerProperties.Properties.EVADE_CAP; //50% evade cap RvR only; http://www.camelotherald.com/more/664.shtml
//                else if (evadeChance > 0.995)
//                    evadeChance = 0.995;
//            }
//            if (ad.AttackType == AttackData.eAttackType.MeleeDualWield)
//            {
//                evadeChance = Math.Max(evadeChance - 0.25, 0);
//            }
//            //Excalibur : infi RR5
//            GamePlayer p = ad.Attacker as GamePlayer;
//            //MimicNPC m = ad.Attacker as MimicNPC;
//            if (p != null)
//            {
//                OverwhelmEffect Overwhelm = (OverwhelmEffect)p.EffectList.GetOfType<OverwhelmEffect>();

//                if (Overwhelm != null)
//                {
//                    evadeChance = Math.Max(evadeChance - OverwhelmAbility.BONUS, 0);
//                }
//            }

//            //log.Debug("End of TryEvade evade chance: " + evadeChance);
//            return evadeChance;
//        }

//        protected override double TryParry(AttackData ad, AttackData lastAD, double attackerConLevel, int attackerCount)
//        {
//            // Parry

//            //1.  Dual wielding does not grant more chances to parry than a single weapon.  Grab Bag 9/12/03
//            //2.  There is no hard cap on ability to Parry.  Grab Bag 8/13/02
//            //3.  Your chances of doing so are best when you are solo, trying to block or parry a style from someone who is also solo. The chances of doing so decrease with grouped, simultaneous attackers.  Grab Bag 7/19/02
//            //4.  The parry chance is divided up amongst the attackers, such that if you had a 50% chance to parry normally, and were under attack by two targets, you would get a 25% chance to parry one, and a 25% chance to parry the other. So, the more people or monsters attacking you, the lower your chances to parry any one attacker. -   Grab Bag 11/05/04
//            //Your chance to parry is affected by the number of attackers, the size of the weapon youre using, and your spec in parry.

//            //Parry % = (5% + 0.5% * Parry) / # of Attackers
//            //Parry: (((Dex*2)-100)/40)+(Parry/2)+(Mastery of P*3)+5. < Possible relation to buffs
//            //So, if you have parry of 20 you will have a chance of parrying 15% if there is one attacker. If you have parry of 20 you will have a chance of parrying 7.5%, if there are two attackers.
//            //From Grab Bag: "Dual wielders throw an extra wrinkle in. You have half the chance of shield blocking a dual wielder as you do a player using only one weapon. Your chance to parry is halved if you are facing a two handed weapon, as opposed to a one handed weapon."
//            //So, when facing a 2H weapon, you may see a penalty to your evade.
//            //
//            //http://www.camelotherald.com/more/453.php

//            //Also, before this comparison happens, the game looks to see if your opponent is in your forward arc  to determine that arc, make a 120 degree angle, and put yourself at the point.

//            double parryChance = 0;

//            if (ad.IsMeleeAttack)
//            {
//                MimicNPC mimic = this as MimicNPC;
//                BladeBarrierEffect BladeBarrier = null;

//                GameSpellEffect parryBuff = SpellHandler.FindEffectOnTarget(this, "ParryBuff");
//                if (parryBuff == null)
//                    parryBuff = SpellHandler.FindEffectOnTarget(this, "SavageParryBuff");

//                if (mimic != null)
//                {
//                    //BladeBarrier overwrites all parrying, 90% chance to parry any attack, does not consider other bonuses to parry
//                    BladeBarrier = mimic.EffectList.GetOfType<BladeBarrierEffect>();
//                    //They still need an active weapon to parry with BladeBarrier
//                    if (BladeBarrier != null && (AttackWeapon != null))
//                    {
//                        parryChance = 0.90;
//                    }
//                    else if (IsObjectInFront(ad.Attacker, 120))
//                    {
//                        if ((mimic.HasSpecialization(Specs.Parry) || parryBuff != null) && (AttackWeapon != null))
//                            parryChance = GetModified(eProperty.ParryChance);
//                    }
//                }
//                else if (this is GameNPC && IsObjectInFront(ad.Attacker, 120))
//                    parryChance = GetModified(eProperty.ParryChance);

//                //If BladeBarrier is up, do not adjust the parry chance.
//                if (BladeBarrier != null && !ad.Target.IsStunned && !ad.Target.IsSitting)
//                {
//                    return parryChance;
//                }
//                else if (parryChance > 0 && !ad.Target.IsStunned && !ad.Target.IsSitting)
//                {
//                    if (attackerCount > 1)
//                        parryChance /= attackerCount / 2;

//                    parryChance *= 0.001;
//                    parryChance += 0.05 * attackerConLevel;

//                    if (parryChance < 0.01)
//                        parryChance = 0.01;
//                    else if (parryChance > ServerProperties.Properties.PARRY_CAP && ad.Attacker is GamePlayer && ad.Target is GamePlayer)
//                        parryChance = ServerProperties.Properties.PARRY_CAP;
//                    else if (parryChance > 0.995)
//                        parryChance = 0.995;
//                }
//            }

//            //TODO: Implement infil RR5 Overwhelm.
//            GamePlayer p = ad.Attacker as GamePlayer;
//            MimicNPC m = ad.Attacker as MimicNPC;
//            //if (p != null || m != null)
//            //{
//            //    OverwhelmEffect Overwhelm = (OverwhelmEffect)p.EffectList.GetOfType<OverwhelmEffect>();
//            //    if (Overwhelm != null)
//            //    {
//            //        parryChance = Math.Max(parryChance - OverwhelmAbility.BONUS, 0);
//            //    }
//            //}
//            //log.Debug("End of TryParry parryChance: " + parryChance);
//            return parryChance;
//        }

//        protected override double TryBlock(AttackData ad, AttackData lastAD, double attackerConLevel, int attackerCount, EngageEffect engage)
//        {
//            // Block

//            //1.Quality does not affect the chance to block at this time.  Grab Bag 3/7/03
//            //2.Condition and enchantment increases the chance to block  Grab Bag 2/27/03
//            //3.There is currently no hard cap on chance to block  Grab Bag 2/27/03 and 8/16/02
//            //4.Dual Wielders (enemy) decrease the chance to block  Grab Bag 10/18/02
//            //5.Block formula: Shield = base 5% + .5% per spec point. Then modified by dex (.1% per point of dex above 60 and below 300?). Further modified by condition, bonus and shield level
//            //8.The shields size only makes a difference when multiple things are attacking you  a small shield can block one attacker, a medium shield can block two at once, and a large shield can block three.  Grab Bag 4/4/03
//            //Your chance to block is affected by the number of attackers, the size of the shield youre using, and your spec in block.
//            //Shield% = (5% + 0.5% * Shield)
//            //Small Shield = 1 attacker
//            //Medium Shield = 2 attacker
//            //Large Shield = 3 attacker
//            //Each attacker above these numbers will reduce your chance to block.
//            //From Grab Bag: "Dual wielders throw an extra wrinkle in. You have half the chance of shield blocking a dual wielder as you do a player using only one weapon. Your chance to parry is halved if you are facing a two handed weapon, as opposed to a one handed weapon."
//            //Block: (((Dex*2)-100)/40)+(Shield/2)+(Mastery of B*3)+5. < Possible relation to buffs
//            //
//            //http://www.camelotherald.com/more/453.php

//            //Also, before this comparison happens, the game looks to see if your opponent is in your forward arc  to determine that arc, make a 120 degree angle, and put yourself at the point.
//            //your friend is most likely using a player crafted shield. The quality of the player crafted item will make a significant difference  try it and see.

//            double blockChance = 0;
//            MimicNPC mimic = this as MimicNPC;
//            InventoryItem lefthand = null;

//            if (this is MimicNPC && mimic != null && IsObjectInFront(ad.Attacker, 120) && mimic.HasAbility(GS.Abilities.Shield))
//            {
//                lefthand = Inventory.GetItem(eInventorySlot.LeftHandWeapon);
//                if (lefthand != null && (mimic.AttackWeapon == null || mimic.AttackWeapon.Item_Type == Slot.RIGHTHAND || mimic.AttackWeapon.Item_Type == Slot.LEFTHAND))
//                {
//                    if (lefthand.Object_Type == (int)eObjectType.Shield && IsObjectInFront(ad.Attacker, 120))
//                        blockChance = GetModified(eProperty.BlockChance) * lefthand.Quality * 0.01;
//                }
//            }

//            if (blockChance > 0 && IsObjectInFront(ad.Attacker, 120) && !ad.Target.IsStunned && !ad.Target.IsSitting)
//            {
//                // Reduce block chance if the shield used is too small (valable only for player because npc inventory does not store the shield size but only the model of item)
//                int shieldSize = 0;
//                if (lefthand != null)
//                    shieldSize = lefthand.Type_Damage;
//                if (mimic != null && attackerCount > shieldSize)
//                    blockChance *= (shieldSize / attackerCount);

//                blockChance *= 0.001;
//                // no chance bonus with ranged attacks?
//                //					if (ad.Attacker.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.Distance)
//                //						blockChance += 0.25;
//                blockChance += attackerConLevel * 0.05;

//                if (blockChance < 0.01)
//                    blockChance = 0.01;
//                else if (blockChance > ServerProperties.Properties.BLOCK_CAP && ad.Attacker is GamePlayer || ad.Attacker is MimicNPC)
//                    blockChance = ServerProperties.Properties.BLOCK_CAP;
//                else if (shieldSize == 1 && ad.Attacker is GameNPC && blockChance > .8)
//                    blockChance = .8;
//                else if (shieldSize == 2 && ad.Attacker is GameNPC && blockChance > .9)
//                    blockChance = .9;
//                else if (shieldSize == 3 && ad.Attacker is GameNPC && blockChance > .99)
//                    blockChance = .99;

//                // Engage raised block change to 85% if attacker is engageTarget and player is in attackstate
//                if (engage != null && AttackState && engage.EngageTarget == ad.Attacker)
//                {
//                    // You cannot engage a mob that was attacked within the last X seconds...
//                    if (engage.EngageTarget.LastAttackedByEnemyTick > engage.EngageTarget.CurrentRegion.Time - EngageAbilityHandler.ENGAGE_ATTACK_DELAY_TICK)

//                    {
//                    }
//                    // Check if player has enough endurance left to engage
//                    else if (engage.Owner.Endurance >= EngageAbilityHandler.ENGAGE_DURATION_LOST)
//                    {
//                        engage.Owner.Endurance -= EngageAbilityHandler.ENGAGE_DURATION_LOST;

//                        if (blockChance < 0.85)
//                            blockChance = 0.85;
//                    }
//                    // if player ran out of endurance cancel engage effect
//                    else
//                        engage.Cancel(false);
//                }
//            }
//            if (ad.AttackType == AttackData.eAttackType.MeleeDualWield)
//            {
//                blockChance = Math.Max(blockChance - 0.25, 0);
//            }
//            //Excalibur : infi RR5
//            //TODO: Implement this
//            //GamePlayer p = ad.Attacker as GamePlayer;
//            //MimicNPC m = ad.Attacker as MimicNPC;

//            //if (p != null || m != null)
//            //{
//            //    OverwhelmEffect Overwhelm = (OverwhelmEffect)p.EffectList.GetOfType<OverwhelmEffect>();
//            //    if (Overwhelm != null)
//            //    {
//            //        blockChance = Math.Max(blockChance - OverwhelmAbility.BONUS, 0);
//            //    }
//            //}
//            //log.Debug("End of TryBlock blockChance: " + blockChance);
//            return blockChance;
//        }

//        public override void TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount)
//        {
//            #region PVP DAMAGE

//            if (source is GamePlayer || (source is GameNPC && (source as GameNPC).Brain is IControlledBrain && ((source as GameNPC).Brain as IControlledBrain).GetPlayerOwner() != null) || source is MimicNPC)
//            {
//                if (Realm != source.Realm && source.Realm != 0)
//                    DamageRvRMemory += (long)(damageAmount + criticalAmount);
//            }

//            #endregion PVP DAMAGE

//            base.TakeDamage(source, damageType, damageAmount, criticalAmount);
//            if (this.HasAbility(DOL.GS.Abilities.DefensiveCombatPowerRegeneration))
//            {
//                this.Mana += (int)((damageAmount + criticalAmount) * 0.25);
//            }
//        }

//        //TODO: Mimic checks for mimics with pets. GetPlayerOwner -> GetMimicOwner? who fuckin knows. Might not be needed.
//        public override void StartAttack(GameObject target)
//        {
//            if (target == null)
//                return;

//            TargetObject = target;

//            long lastTick = this.TempProperties.getProperty<long>(LAST_LOS_TICK_PROPERTY);

//            //if ((ServerProperties.Properties.ALWAYS_CHECK_PET_LOS &&
//            //    Brain is IControlledBrain &&
//            //    (target is GamePlayer || (target is GameNPC && (target as GameNPC).Brain != null && (target as GameNPC).Brain is IControlledBrain))) ||
//            //    ServerProperties.Properties.ALWAYS_CHECK_PET_LOS &&
//            //    ((target is GamePlayer || target is MimicNPC) || (target is GameNPC && (target as GameNPC).Brain != null && (target as GameNPC).Brain is IControlledBrain)))
//            //{
//            if (target is GameLiving)
//            {
//                GameObject lastTarget = (GameObject)this.TempProperties.getProperty<object>(LAST_LOS_TARGET_PROPERTY, null);
//                if (lastTarget != null && lastTarget == target)
//                {
//                    if (lastTick != 0 && CurrentRegion.Time - lastTick < ServerProperties.Properties.LOS_PLAYER_CHECK_FREQUENCY * 1000)
//                        return;
//                }

//                GamePlayer losChecker = null;

//                if (target is GamePlayer)
//                {
//                    losChecker = target as GamePlayer;
//                }
//                else if (target is GameNPC && (target as GameNPC).Brain is IControlledBrain)
//                {
//                    losChecker = ((target as GameNPC).Brain as IControlledBrain).GetPlayerOwner();
//                }
//                else
//                {
//                    // try to find another player to use for checking line of site
//                    foreach (GamePlayer player in this.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
//                    {
//                        losChecker = player;
//                        break;
//                    }
//                }

//                if (losChecker == null)
//                {
//                    return;
//                }

//                lock (LOS_LOCK)
//                {
//                    int count = TempProperties.getProperty<int>(NUM_LOS_CHECKS_INPROGRESS, 0);

//                    if (count > 10)
//                    {
//                        log.DebugFormat("{0} LOS count check exceeds 10, aborting LOS check!", Name);

//                        // Now do a safety check.  If it's been a while since we sent any check we should clear count
//                        if (lastTick == 0 || CurrentRegion.Time - lastTick > ServerProperties.Properties.LOS_PLAYER_CHECK_FREQUENCY * 1000)
//                        {
//                            log.Debug("LOS count reset!");
//                            TempProperties.setProperty(NUM_LOS_CHECKS_INPROGRESS, 0);
//                        }

//                        return;
//                    }

//                    count++;
//                    TempProperties.setProperty(NUM_LOS_CHECKS_INPROGRESS, count);

//                    TempProperties.setProperty(LAST_LOS_TARGET_PROPERTY, target);
//                    TempProperties.setProperty(LAST_LOS_TICK_PROPERTY, CurrentRegion.Time);
//                    m_targetLOSObject = target;
//                }

//                losChecker.Out.SendCheckLOS(this, target, new CheckLOSResponse(this.NPCStartAttackCheckLOS));
//                return;
//            }

//            ContinueStartAttack(target);
//        }

//        //private void StartArriveAtTargetAction(int requiredTicks, Action<GameNPC> goToNextNodeCallback = null)
//        //{
//        //    m_arriveAtTargetAction = new MimicArriveAtTargetAction(this, goToNextNodeCallback);
//        //    m_arriveAtTargetAction.Start((requiredTicks > 1) ? requiredTicks : 1);
//        //}

//        private bool InPosition;
//        private bool m_positionSelected;

//        public override void Follow(GameObject target, int minDistance, int maxDistance)
//        {
//            base.Follow(target, minDistance, maxDistance);

//            InPosition = false;
//            m_positionSelected = false;
//        }

//        public override void StopFollowing()
//        {
//            base.StopFollowing();

//            InPosition = false;
//            m_positionSelected = false;
//        }

//        /// <summary>
//		/// Will be called if follow mode is active
//		/// and we reached the follow target
//		/// </summary>
//        public override void FollowTargetInRange()
//        {
//            if (AttackState)
//            {
//                // if in last attack the enemy was out of range, we can attack him now immediately
//                AttackData ad = (AttackData)TempProperties.getProperty<object>(LAST_ATTACK_DATA, null);
//                if (ad != null && ad.AttackResult == eAttackResult.OutOfRange)
//                {
//                    m_attackAction.Start(1);// schedule for next tick
//                }
//            }
//            else if (m_attackers.Count == 0 && this.Spells.Count > 0 && this.TargetObject != null && GameServer.ServerRules.IsAllowedToAttack(this, (this.TargetObject as GameLiving), true))
//            {
//                if (TargetObject.Realm == 0 || Realm == 0)
//                    m_lastAttackTickPvE = m_CurrentRegion.Time;
//                else
//                    m_lastAttackTickPvP = m_CurrentRegion.Time;

//                if (this.CurrentRegion.Time - LastAttackedByEnemyTick > 10 * 1000)
//                {
//                    // Aredhel: Erm, checking for spells in a follow method, what did we create
//                    // brain classes for again?

//                    //Check for negatively casting spells
//                    MimicBrain stanBrain = (MimicBrain)Brain;

//                    if (stanBrain != null)
//                        stanBrain.CheckSpells(MimicBrain.eCheckSpellType.Offensive);
//                }
//            }
//        }

//        public override void WalkTo(IPoint3D target, short speed)
//        {
//            base.WalkTo(target, speed);
//        }

//        /// <summary>
//		/// Walk to the spawn point
//		/// </summary>
//		public override void WalkToSpawn()
//        {
//            WalkToSpawn(MaxSpeed);
//        }

//        //protected MimicArriveAtTargetAction m_combatArriveAtTargetAction;

//        //public void MimicCancelWalkToTimer()
//        //{
//        //    if (m_combatArriveAtTargetAction != null)
//        //    {
//        //        m_combatArriveAtTargetAction.Stop();
//        //        m_combatArriveAtTargetAction = null;
//        //    }
//        //}

//        private void WalkToCombatPosition(IPoint3D target, short speed)
//        {
//            if (IsTurningDisabled)
//                return;

//            if (speed > MaxSpeed)
//                speed = MaxSpeed;

//            if (speed <= 0)
//                return;

//            TargetPosition = target; // this also saves the current position

//            if (IsWithinRadius(TargetPosition, 1))
//            {
//                // No need to start walking.
//                log.Info("No need");
//                Notify(GameNPCEvent.ArriveAtTarget, this);
//                return;
//            }

//            //MimicCancelWalkToTimer();

//            m_Heading = GetHeading(TargetPosition);
//            m_currentSpeed = speed;

//            UpdateTickSpeed();
//            Notify(GameNPCEvent.WalkTo, this, new WalkToEventArgs(TargetPosition, speed));

//            //StartArriveAtTargetAction(GetTicksToArriveAt(TargetPosition, speed));
//            BroadcastUpdate();
//        }

//        //     protected class MimicArriveAtTargetAction : ArriveAtTargetAction
//        //     {
//        //         private Action<GameNPC> m_goToNodeCallback;

//        //         /// <summary>
//        ///// Constructs a new ArriveAtTargetAction
//        ///// </summary>
//        ///// <param name="actionSource">The action source</param>
//        //public MimicArriveAtTargetAction(GameNPC actionSource, Action<GameNPC> goToNodeCallback = null)
//        //             : base(actionSource)
//        //         {
//        //             m_goToNodeCallback = goToNodeCallback;
//        //         }

//        //         /// <summary>
//        //         /// This function is called when the Mob arrives at its target spot
//        //         /// This time was estimated using walking speed and distance.
//        //         /// It fires the ArriveAtTarget event
//        //         /// </summary>
//        //         protected override void OnTick()
//        //         {
//        //             GameNPC npc = (GameNPC)m_actionSource;
//        //             if (m_goToNodeCallback != null)
//        //             {
//        //                 m_goToNodeCallback(npc);
//        //                 return;
//        //             }

//        //             bool arriveAtSpawnPoint = npc.IsReturningToSpawnPoint;

//        //             npc.StopMoving();
//        //             npc.Notify(GameNPCEvent.ArriveAtTarget, npc);

//        //             if (arriveAtSpawnPoint)
//        //                 npc.Notify(GameNPCEvent.ArriveAtSpawnPoint, npc);

//        //             ((MimicNPC)npc).InPosition = true;
//        //         }
//        //     }

//        /// <summary>
//        /// Keep following a specific object at a max distance
//        /// </summary>
//        protected override int FollowTimerCallback(RegionTimer callingTimer)
//        {
//            if (IsCasting)
//                return ServerProperties.Properties.GAMENPC_FOLLOWCHECK_TIME;

//            bool wasInRange = m_followTimer.Properties.getProperty(FOLLOW_TARGET_IN_RANGE, false);
//            m_followTimer.Properties.removeProperty(FOLLOW_TARGET_IN_RANGE);

//            GameObject followTarget = (GameObject)m_followTarget.Target;
//            GameLiving followLiving = followTarget as GameLiving;

//            //Stop following if target living is dead
//            if (followLiving != null && !followLiving.IsAlive)
//            {
//                StopFollowing();
//                Notify(GameNPCEvent.FollowLostTarget, this, new FollowLostTargetEventArgs(followTarget));
//                return 0;
//            }

//            //Stop following if we have no target
//            if (followTarget == null || followTarget.ObjectState != eObjectState.Active || CurrentRegionID != followTarget.CurrentRegionID)
//            {
//                StopFollowing();
//                Notify(GameNPCEvent.FollowLostTarget, this, new FollowLostTargetEventArgs(followTarget));
//                return 0;
//            }

//            //Calculate the difference between our position and the players position
//            float diffx = (long)followTarget.X - X;
//            float diffy = (long)followTarget.Y - Y;
//            float diffz = (long)followTarget.Z - Z;

//            //SH: Removed Z checks when one of the two Z values is zero(on ground)
//            //Tolakram: a Z of 0 does not indicate on the ground.  Z varies based on terrain  Removed 0 Z check
//            float distance = (float)Math.Sqrt(diffx * diffx + diffy * diffy + diffz * diffz);

//            //if distance is greater then the max follow distance, stop following and return home
//            if ((int)distance > m_followMaxDist)
//            {
//                StopFollowing();
//                Notify(GameNPCEvent.FollowLostTarget, this, new FollowLostTargetEventArgs(followTarget));
//                this.WalkToSpawn();
//                return 0;
//            }

//            int newX, newY, newZ;

//            if (this.Brain is StandardMobBrain)
//            {
//                StandardMobBrain brain = this.Brain as StandardMobBrain;

//                //if the npc hasn't hit or been hit in a while, stop following and return home
//                if (!(Brain is IControlledBrain))
//                {
//                    if (AttackState && brain != null && followLiving != null)
//                    {
//                        long seconds = 20 + ((brain.GetAggroAmountForLiving(followLiving) / (MaxHealth + 1)) * 100);
//                        long lastattacked = LastAttackTick;
//                        long lasthit = LastAttackedByEnemyTick;
//                        if (CurrentRegion.Time - lastattacked > seconds * 1000 && CurrentRegion.Time - lasthit > seconds * 1000)
//                        {
//                            //StopFollow();
//                            Notify(GameNPCEvent.FollowLostTarget, this, new FollowLostTargetEventArgs(followTarget));
//                            //brain.ClearAggroList();
//                            this.WalkToSpawn();
//                            return 0;
//                        }
//                    }
//                }

//                //If we're part of a formation, we can get out early.
//                newX = followTarget.X;
//                newY = followTarget.Y;
//                newZ = followTarget.Z;

//                if (brain.CheckFormation(ref newX, ref newY, ref newZ))
//                {
//                    WalkTo(newX, newY, (ushort)newZ, MaxSpeed);
//                    return ServerProperties.Properties.GAMENPC_FOLLOWCHECK_TIME;
//                }
//            }

//            // Tolakram - Distances under 100 do not calculate correctly leading to the mob always being told to walkto
//            int minAllowedFollowDistance = MIN_ALLOWED_FOLLOW_DISTANCE;

//            // pets can follow closer.  need to implement /fdistance command to make this adjustable
//            if (this.Brain is IControlledBrain)
//                minAllowedFollowDistance = MIN_ALLOWED_PET_FOLLOW_DISTANCE;

//            //Are we in range yet?
//            if ((int)distance <= (m_followMinDist < minAllowedFollowDistance ? minAllowedFollowDistance : m_followMinDist))
//            {
//                StopMoving();
//                TurnTo(followTarget);

//                if (!wasInRange)
//                {
//                    m_followTimer.Properties.setProperty(FOLLOW_TARGET_IN_RANGE, true);
//                    FollowTargetInRange();
//                }
//                return
//                    ServerProperties.Properties.GAMENPC_FOLLOWCHECK_TIME;

//                //if (InPosition)
//                //{
//                //    StopMoving();
//                //    TurnTo(followTarget);

//                //    if (!wasInRange)
//                //    {
//                //        m_followTimer.Properties.setProperty(FOLLOW_TARGET_IN_RANGE, true);
//                //        FollowTargetInRange();
//                //    }
//                //}

//                //IPoint3D walkToPosition = null;

//                //if (AttackState && !m_positionSelected)
//                //{
//                //    GameLiving gameLivingTarget = followTarget as GameLiving;

//                //    if (gameLivingTarget != null)
//                //    {
//                //        if (gameLivingTarget.TargetObject != this && !gameLivingTarget.IsMoving)
//                //        {
//                //            Point3D targetsPosition = new Point3D(gameLivingTarget.X, gameLivingTarget.Y, gameLivingTarget.Z);

//                //            bool sideStyles = false;
//                //            bool backStyles = false;

//                //            if (StylesSide != null && StylesSide.Count > 0)
//                //            {
//                //                sideStyles = true;
//                //            }

//                //            if (StylesBack != null && StylesBack.Count > 0)
//                //            {
//                //                backStyles = true;
//                //            }

//                //            if (sideStyles && backStyles)
//                //            {
//                //                if (Util.RandomBool())
//                //                {
//                //                    walkToPosition = GetSidePoint(targetsPosition, gameLivingTarget.Heading);
//                //                }
//                //                else
//                //                {
//                //                    walkToPosition = GetBackPoint(targetsPosition, gameLivingTarget.Heading);
//                //                }
//                //            }
//                //            else if (sideStyles)
//                //            {
//                //                walkToPosition = GetSidePoint(targetsPosition, gameLivingTarget.Heading);
//                //            }
//                //            else if (backStyles)
//                //            {
//                //                walkToPosition = GetBackPoint(targetsPosition, gameLivingTarget.Heading);
//                //            }

//                //            m_positionSelected = true;
//                //        }
//                //    }

//                //    if (walkToPosition == null)
//                //    {
//                //        //log.Debug("walkToPosition is null");
//                //    }

//                //    if (walkToPosition != null && m_positionSelected && !InPosition)
//                //    {
//                //        log.Info("Calling WalkTo " + walkToPosition);

//                //        //WalkToCombatPosition(walkToPosition, MaxSpeed);
//                //        PathTo(walkToPosition, MaxSpeed);
//                //    }
//                //}

//                //return ServerProperties.Properties.GAMENPC_FOLLOWCHECK_TIME;
//            }

//            InPosition = false;
//            m_positionSelected = false;

//            // follow on distance
//            diffx = (diffx / distance) * m_followMinDist;
//            diffy = (diffy / distance) * m_followMinDist;
//            diffz = (diffz / distance) * m_followMinDist;

//            //Subtract the offset from the target's position to get
//            //our target position
//            newX = (int)(followTarget.X - diffx);
//            newY = (int)(followTarget.Y - diffy);
//            newZ = (int)(followTarget.Z - diffz);

//            if (InCombat || Brain is BomberBrain)
//                PathTo(new Point3D(newX, newY, (ushort)newZ), MaxSpeed);
//            else
//            {
//                PathTo(new Point3D(newX, newY, (ushort)newZ), (short)GetDistance(new Point2D(newX, newY)));
//            }

//            return ServerProperties.Properties.GAMENPC_FOLLOWCHECK_TIME;
//        }

//        private Point3D GetSidePoint(IPoint3D targetPosition, ushort targetHeading)
//        {
//            log.Info("Side Point");
//            int headingRadians = (int)(targetHeading * Math.PI / 180.0);

//            int newHeadingRadians = headingRadians + (int)(Math.PI / 2.0);

//            int xOffset = (int)(50 * Math.Cos(newHeadingRadians));
//            int yOffset = (int)(50 * Math.Sin(newHeadingRadians));

//            int newX = targetPosition.X + xOffset;
//            int newY = targetPosition.Y + yOffset;

//            if (Group != null)
//            {
//                foreach (GamePlayer player in Group.GetPlayersInTheGroup())
//                {
//                    //player.SetGroundTarget(walkToPosition.X, walkToPosition.Y, walkToPosition.Z);
//                    player.Out.SendChangeGroundTarget(new Point3D(newX, newY, targetPosition.Z));
//                }
//            }

//            return new Point3D(newX, newY, targetPosition.Z);
//        }

//        private Point3D GetBackPoint(IPoint3D targetPosition, ushort targetHeading)
//        {
//            log.Info("Back Point");
//            // Convert the heading to radians
//            int targetHeadingRad = (int)(targetHeading * Math.PI / 180f);

//            // Calculate the position behind the target
//            int xOffset = -(int)(50 * Math.Sin(targetHeadingRad));
//            int yOffset = (int)(50 * Math.Cos(targetHeadingRad));

//            int newX = targetPosition.X + xOffset;
//            int newY = targetPosition.Y + yOffset;

//            if (Group != null)
//            {
//                foreach (GamePlayer player in Group.GetPlayersInTheGroup())
//                {
//                    //player.SetGroundTarget(walkToPosition.X, walkToPosition.Y, walkToPosition.Z);
//                    player.Out.SendChangeGroundTarget(new Point3D(newX, newY, targetPosition.Z));
//                }
//            }

//            return new Point3D(newX, newY, targetPosition.Z);
//        }

//        public override int EffectiveOverallAF
//        {
//            get
//            {
//                int eaf = 0;
//                int abs = 0;
//                foreach (InventoryItem item in Inventory.VisibleItems)
//                {
//                    double factor = 0;
//                    switch (item.Item_Type)
//                    {
//                        case Slot.TORSO:
//                        factor = 2.2;
//                        break;

//                        case Slot.LEGS:
//                        factor = 1.3;
//                        break;

//                        case Slot.ARMS:
//                        factor = 0.75;
//                        break;

//                        case Slot.HELM:
//                        factor = 0.5;
//                        break;

//                        case Slot.HANDS:
//                        factor = 0.25;
//                        break;

//                        case Slot.FEET:
//                        factor = 0.25;
//                        break;
//                    }

//                    int itemAFCap = Level << 1;
//                    if (RealmLevel > 39)
//                        itemAFCap += 2;
//                    switch ((eObjectType)item.Object_Type)
//                    {
//                        case eObjectType.Cloth:
//                        abs = 0;
//                        itemAFCap >>= 1;
//                        break;

//                        case eObjectType.Leather:
//                        abs = 10;
//                        break;

//                        case eObjectType.Reinforced:
//                        abs = 19;
//                        break;

//                        case eObjectType.Studded:
//                        abs = 19;
//                        break;

//                        case eObjectType.Scale:
//                        abs = 27;
//                        break;

//                        case eObjectType.Chain:
//                        abs = 27;
//                        break;

//                        case eObjectType.Plate:
//                        abs = 34;
//                        break;
//                    }

//                    if (factor > 0)
//                    {
//                        int af = item.DPS_AF;
//                        if (af > itemAFCap)
//                            af = itemAFCap;
//                        double piece_eaf = af * item.Quality / 100.0 * item.ConditionPercent / 100.0 * (1 + abs / 100.0);
//                        eaf += (int)(piece_eaf * factor);
//                    }
//                }

//                // Overall AF CAP = 10 * level * (1 + abs%/100)
//                int bestLevel = -1;
//                bestLevel = Math.Max(bestLevel, GetAbilityLevel(GS.Abilities.AlbArmor));
//                bestLevel = Math.Max(bestLevel, GetAbilityLevel(GS.Abilities.HibArmor));
//                bestLevel = Math.Max(bestLevel, GetAbilityLevel(GS.Abilities.MidArmor));
//                switch (bestLevel)
//                {
//                    default: abs = 0; break; // cloth etc
//                    case ArmorLevel.Leather: abs = 10; break;
//                    case ArmorLevel.Studded: abs = 19; break;
//                    case ArmorLevel.Chain: abs = 27; break;
//                    case ArmorLevel.Plate: abs = 34; break;
//                }

//                eaf += BaseBuffBonusCategory[(int)eProperty.ArmorFactor]; // base buff before cap
//                int eafcap = (int)(10 * Level * (1 + abs * 0.01));
//                if (eaf > eafcap)
//                    eaf = eafcap;
//                eaf += (int)Math.Min(Level * 1.875, SpecBuffBonusCategory[(int)eProperty.ArmorFactor])
//                    - DebuffCategory[(int)eProperty.ArmorFactor]
//                    + BuffBonusCategory4[(int)eProperty.ArmorFactor]
//                    + Math.Min(Level, ItemBonus[(int)eProperty.ArmorFactor]);

//                eaf = (int)(eaf * BuffBonusMultCategory1.Get((int)eProperty.ArmorFactor));

//                return eaf;
//            }
//        }

//        /// <summary>
//        /// Calc Armor hit location when player is hit by enemy
//        /// </summary>
//        /// <returns>slotnumber where enemy hits</returns>
//        /// attackdata(ad) changed
//        public virtual eArmorSlot CalculateArmorHitLocation(AttackData ad)
//        {
//            if (ad.Style != null)
//            {
//                if (ad.Style.ArmorHitLocation != eArmorSlot.NOTSET)
//                    return ad.Style.ArmorHitLocation;
//            }

//            int chancehit = Util.Random(1, 100);

//            if (chancehit <= 40)
//            {
//                return eArmorSlot.TORSO;
//            }
//            else if (chancehit <= 65)
//            {
//                return eArmorSlot.LEGS;
//            }
//            else if (chancehit <= 80)
//            {
//                return eArmorSlot.ARMS;
//            }
//            else if (chancehit <= 90)
//            {
//                return eArmorSlot.HEAD;
//            }
//            else if (chancehit <= 95)
//            {
//                return eArmorSlot.HAND;
//            }
//            else
//            {
//                return eArmorSlot.FEET;
//            }
//        }

//        /// <summary>
//        /// Picks a style, prioritizing reactives an	d chains over positionals and anytimes
//        /// </summary>
//        /// <returns>Selected style</returns>
//        protected override Style GetStyleToUse()
//        {
//            if (m_styles == null || m_styles.Count < 1 || TargetObject == null)
//                return null;

//            if (StylesStealth != null && StylesStealth.Count > 0 && IsStealthed)
//                foreach (Style s in StylesStealth)
//                    if (StyleProcessor.CanUseStyle(this, s, AttackWeapon))
//                        return s;

//            if (StylesChain != null && StylesChain.Count > 0)
//                foreach (Style s in StylesChain)
//                    if (StyleProcessor.CanUseStyle(this, s, AttackWeapon))
//                        return s;

//            if (StylesDefensive != null && StylesDefensive.Count > 0)
//                foreach (Style s in StylesDefensive)
//                    if (StyleProcessor.CanUseStyle(this, s, AttackWeapon)
//                        && CheckStyleStun(s)) // Make sure we don't spam stun styles like Brutalize
//                        return s;

//            if (Util.Chance(95))
//            {
//                // Check positional styles
//                // Picking random styles allows mobs to use multiple styles from the same position
//                //	e.g. a mob with both Pincer and Ice Storm side styles will use both of them.
//                if (StylesBack != null && StylesBack.Count > 0)
//                {
//                    Style s = StylesBack[Util.Random(0, StylesBack.Count - 1)];
//                    if (StyleProcessor.CanUseStyle(this, s, AttackWeapon))
//                        return s;
//                }

//                if (StylesSide != null && StylesSide.Count > 0)
//                {
//                    Style s = StylesSide[Util.Random(0, StylesSide.Count - 1)];

//                    if (StyleProcessor.CanUseStyle(this, s, AttackWeapon))
//                        return s;
//                }

//                if (StylesFront != null && StylesFront.Count > 0)
//                {
//                    Style s = StylesFront[Util.Random(0, StylesFront.Count - 1)];
//                    if (StyleProcessor.CanUseStyle(this, s, AttackWeapon))
//                        return s;
//                }

//                // Pick a random anytime style
//                if (StylesAnytime != null && StylesAnytime.Count > 0)
//                    return StylesAnytime[Util.Random(0, StylesAnytime.Count - 1)];
//            }

//            return null;
//        } // GetStyleToUse()

//        /// <summary>
//        /// Gets the weaponskill of weapon
//        /// </summary>
//        /// <param name="weapon"></param>
//        public override double GetWeaponSkill(InventoryItem weapon)
//        {
//            if (weapon == null)
//                return 0;

//            var baseRangedWeaponRange = 440;

//            double classbase =
//                (weapon.SlotPosition == (int)eInventorySlot.DistanceWeapon
//                 ? baseRangedWeaponRange
//                 : CharacterClass.WeaponSkillBase);

//            //added for WS Poisons
//            double preBuff = ((Level * classbase * 0.02 * (1 + (GetWeaponStat(weapon) - 50) * 0.005)) * Effectiveness);

//            //return ((Level * classbase * 0.02 * (1 + (GetWeaponStat(weapon) - 50) * 0.005)) * PlayerEffectiveness);
//            return Math.Max(0, preBuff * GetModified(eProperty.WeaponSkill) * 0.01);
//        }

//        /// <summary>
//        /// calculates weapon stat
//        /// </summary>
//        /// <param name="weapon"></param>
//        /// <returns></returns>
//        public override int GetWeaponStat(InventoryItem weapon)
//        {
//            if (weapon != null)
//            {
//                switch ((eObjectType)weapon.Object_Type)
//                {
//                    // DEX modifier
//                    case eObjectType.Staff:
//                    case eObjectType.Fired:
//                    case eObjectType.Longbow:
//                    case eObjectType.Crossbow:
//                    case eObjectType.CompositeBow:
//                    case eObjectType.RecurvedBow:
//                    case eObjectType.Thrown:
//                    case eObjectType.Shield:
//                    return GetModified(eProperty.Dexterity);

//                    // STR+DEX modifier
//                    case eObjectType.ThrustWeapon:
//                    case eObjectType.Piercing:
//                    case eObjectType.Spear:
//                    case eObjectType.Flexible:
//                    case eObjectType.HandToHand:
//                    return (GetModified(eProperty.Strength) + GetModified(eProperty.Dexterity)) >> 1;
//                }
//            }

//            // STR modifier for others
//            return GetModified(eProperty.Strength);
//        }

//        /// <summary>
//        /// calculate item armor factor influenced by quality, con and duration
//        /// </summary>
//        /// <param name="slot"></param>
//        /// <returns></returns>
//        public override double GetArmorAF(eArmorSlot slot)
//        {
//            if (slot == eArmorSlot.NOTSET) return 0;
//            InventoryItem item = Inventory.GetItem((eInventorySlot)slot);
//            if (item == null) return 0;
//            double eaf = item.DPS_AF + BaseBuffBonusCategory[(int)eProperty.ArmorFactor]; // base AF buff

//            int itemAFcap = Level;
//            if (RealmLevel > 39)
//                itemAFcap++;
//            if (item.Object_Type != (int)eObjectType.Cloth)
//            {
//                itemAFcap <<= 1;
//            }

//            eaf = Math.Min(eaf, itemAFcap);
//            eaf *= 4.67; // compensate *4.67 in damage formula

//            // my test shows that qual is added after AF buff
//            eaf *= item.Quality * 0.01 * item.Condition / item.MaxCondition;

//            eaf += GetModified(eProperty.ArmorFactor);

//            /*GameSpellEffect effect = SpellHandler.FindEffectOnTarget(this, typeof(VampiirArmorDebuff));
//			if (effect != null && slot == (effect.SpellHandler as VampiirArmorDebuff).Slot)
//			{
//				eaf -= (int)(effect.SpellHandler as VampiirArmorDebuff).Spell.Value;
//			}*/

//            //log.Debug("End of GetArmorAF eaf: " + eaf);
//            return eaf;
//        }

//        /// <summary>
//        /// Calculates armor absorb level
//        /// </summary>
//        /// <param name="slot"></param>
//        /// <returns></returns>
//        public override double GetArmorAbsorb(eArmorSlot slot)
//        {
//            if (slot == eArmorSlot.NOTSET) return 0;
//            InventoryItem item = Inventory.GetItem((eInventorySlot)slot);
//            if (item == null) return 0;
//            // vampiir random armor debuff change ~
//            double eaf = (item.SPD_ABS + GetModified(eProperty.ArmorAbsorption)) * 0.01;
//            return eaf;
//        }

//        /// <summary>
//        /// Max. Damage possible without style
//        /// </summary>
//        /// <param name="weapon">attack weapon</param>
//        public override double UnstyledDamageCap(InventoryItem weapon)
//        {
//            if (weapon != null)
//            {
//                int DPS = weapon.DPS_AF;
//                int cap = 12 + 3 * Level;

//                if (RealmLevel > 39)
//                    cap += 3;

//                if (DPS > cap)
//                    DPS = cap;

//                double result = DPS * weapon.SPD_ABS * 0.03 * (0.94 + 0.003 * weapon.SPD_ABS);

//                if (weapon.Hand == 1) //2h
//                {
//                    result *= 1.1 + (WeaponSpecLevel(weapon) - 1) * 0.005;

//                    if (weapon.Item_Type == Slot.RANGED)
//                    {
//                        // http://home.comcast.net/~shadowspawn3/bowdmg.html
//                        //ammo damage bonus
//                        double ammoDamageBonus = 1;
//                        if (RangeAttackAmmo != null)
//                        {
//                            switch ((RangeAttackAmmo.SPD_ABS) & 0x3)
//                            {
//                                case 0: ammoDamageBonus = 0.85; break;  //Blunt       (light) -15%
//                                case 1: ammoDamageBonus = 1; break;     //Bodkin     (medium)   0%
//                                case 2: ammoDamageBonus = 1.15; break;  //doesn't exist on live
//                                case 3: ammoDamageBonus = 1.25; break;  //Broadhead (X-heavy) +25%
//                            }
//                        }
//                        result *= ammoDamageBonus;
//                    }
//                }

//                if (weapon.Item_Type == Slot.RANGED && (weapon.Object_Type == (int)eObjectType.Longbow || weapon.Object_Type == (int)eObjectType.RecurvedBow || weapon.Object_Type == (int)eObjectType.CompositeBow))
//                {
//                    if (ServerProperties.Properties.ALLOW_OLD_ARCHERY == true)
//                    {
//                        result += GetModified(eProperty.RangedDamage) * 0.01;
//                    }
//                    else if (ServerProperties.Properties.ALLOW_OLD_ARCHERY == false)
//                    {
//                        result += GetModified(eProperty.SpellDamage) * 0.01;
//                        result += GetModified(eProperty.RangedDamage) * 0.01;
//                    }
//                }
//                else if (weapon.Item_Type == Slot.RANGED)
//                {
//                    //Ranged damage buff,debuff,Relic,RA
//                    result += GetModified(eProperty.RangedDamage) * 0.01;
//                }
//                else if (weapon.Item_Type == Slot.RIGHTHAND || weapon.Item_Type == Slot.LEFTHAND || weapon.Item_Type == Slot.TWOHAND)
//                {
//                    result += GetModified(eProperty.MeleeDamage) * 0.01;
//                }

//                //log.Debug("End of UnstyledDamageCap result: " + result);
//                return result;
//            }
//            else
//            { // TODO: whats the damage cap without weapon?
//                log.Debug("weapon == null in unstyleddamgecap");
//                //return AttackDamage(weapon) * 3 * (1 + (AttackSpeed(weapon) * 0.001 - 2) * .03);
//                return 0;
//            }
//        }

//        /// <summary>
//        /// The chance for a critical hit
//        /// </summary>
//        /// <param name="weapon">attack weapon</param>
//        public override int AttackCriticalChance(InventoryItem weapon)
//        {
//            if (weapon != null && weapon.Item_Type == Slot.RANGED && RangedAttackType == eRangedAttackType.Critical)
//                return 0; // no crit damage for crit shots

//            // check for melee attack
//            if (weapon != null && weapon.Item_Type != Slot.RANGED)
//            {
//                return GetModified(eProperty.CriticalMeleeHitChance);
//            }

//            // check for ranged attack
//            if (weapon != null && weapon.Item_Type == Slot.RANGED)
//            {
//                return GetModified(eProperty.CriticalArcheryHitChance);
//            }

//            // base 10% chance of critical for all with melee weapons
//            return 10;
//        }

//        /// <summary>
//        /// Returns the damage type of the current attack
//        /// </summary>
//        /// <param name="weapon">attack weapon</param>
//        public override eDamageType AttackDamageType(InventoryItem weapon)
//        {
//            if (weapon == null)
//                return eDamageType.Natural;

//            switch ((eObjectType)weapon.Object_Type)
//            {
//                case eObjectType.Crossbow:
//                case eObjectType.Longbow:
//                case eObjectType.CompositeBow:
//                case eObjectType.RecurvedBow:
//                case eObjectType.Fired:

//                InventoryItem ammo = RangeAttackAmmo;

//                if (ammo == null)
//                    return (eDamageType)weapon.Type_Damage;

//                return (eDamageType)ammo.Type_Damage;

//                case eObjectType.Shield:
//                return eDamageType.Crush; // TODO: shields do crush damage (!) best is if Type_Damage is used properly

//                default:
//                return (eDamageType)weapon.Type_Damage;
//            }
//        }

//        /// <summary>
//        /// Gets the weapondamage of currently used weapon
//        /// Used to display weapon damage in stats, 16.5dps = 1650
//        /// </summary>
//        /// <param name="weapon">the weapon used for attack</param>
//        public override double WeaponDamage(InventoryItem weapon)
//        {
//            if (weapon != null)
//            {
//                //TODO if attackweapon is ranged -> attackdamage is arrow damage
//                int DPS = weapon.DPS_AF;

//                // apply relic bonus prior to cap
//                DPS = (int)((double)DPS * (1.0 + RelicMgr.GetRelicBonusModifier(Realm, eRelicType.Strength)));

//                // apply damage cap before quality
//                // http://www.classesofcamelot.com/faq.asp?mode=view&cat=10
//                int cap = 12 + 3 * Level;
//                if (RealmLevel > 39)
//                    cap += 3;

//                if (DPS > cap)
//                {
//                    DPS = cap;
//                }
//                //(1.0 + BuffBonusCategory1[(int)eProperty.DPS]/100.0 - BuffBonusCategory3[(int)eProperty.DPS]/100.0)
//                DPS = (int)(DPS * (1 + (GetModified(eProperty.DPS) * 0.01)));

//                // beware to use always ConditionPercent, because Condition is abolute value
//                //				return (int) ((DPS/10.0)*(weapon.Quality/100.0)*(weapon.Condition/(double)weapon.MaxCondition)*100.0);
//                double wdamage = (0.001 * DPS * weapon.Quality * weapon.Condition) / weapon.MaxCondition;
//                return wdamage;
//            }
//            else
//            {
//                return 0;
//            }
//        }

//        /// <summary>
//        /// Gets the current attackspeed of this living in milliseconds
//        /// </summary>
//        /// <param name="weapons">attack weapons</param>
//        /// <returns>effective speed of the attack. average if more than one weapon.</returns>
//        public override int AttackSpeed(params InventoryItem[] weapons)
//        {
//            if (weapons == null || weapons.Length < 1)
//                return 0;

//            int count = 0;
//            double speed = 0;
//            bool bowWeapon = true;

//            for (int i = 0; i < weapons.Length; i++)
//            {
//                if (weapons[i] != null)
//                {
//                    speed += weapons[i].SPD_ABS;
//                    count++;

//                    switch (weapons[i].Object_Type)
//                    {
//                        case (int)eObjectType.Fired:
//                        case (int)eObjectType.Longbow:
//                        case (int)eObjectType.Crossbow:
//                        case (int)eObjectType.RecurvedBow:
//                        case (int)eObjectType.CompositeBow:
//                        break;

//                        default:
//                        bowWeapon = false;
//                        break;
//                    }
//                }
//            }

//            if (count < 1)
//                return 0;

//            speed /= count;

//            int qui = Math.Min((short)250, Quickness); //250 soft cap on quickness

//            if (bowWeapon)
//            {
//                if (ServerProperties.Properties.ALLOW_OLD_ARCHERY)
//                {
//                    //Draw Time formulas, there are very many ...
//                    //Formula 2: y = iBowDelay * ((100 - ((iQuickness - 50) / 5 + iMasteryofArcheryLevel * 3)) / 100)
//                    //Formula 1: x = (1 - ((iQuickness - 60) / 500 + (iMasteryofArcheryLevel * 3) / 100)) * iBowDelay
//                    //Table a: Formula used: drawspeed = bowspeed * (1-(quickness - 50)*0.002) * ((1-MoA*0.03) - (archeryspeedbonus/100))
//                    //Table b: Formula used: drawspeed = bowspeed * (1-(quickness - 50)*0.002) * (1-MoA*0.03) - ((archeryspeedbonus/100 * basebowspeed))

//                    //For now use the standard weapon formula, later add ranger haste etc.
//                    speed *= (1.0 - (qui - 60) * 0.002);
//                    double percent = 0;
//                    // Calcul ArcherySpeed bonus to substract
//                    percent = speed * 0.01 * GetModified(eProperty.ArcherySpeed);
//                    // Apply RA difference
//                    speed -= percent;
//                    //log.Debug("speed = " + speed + " percent = " + percent + " eProperty.archeryspeed = " + GetModified(eProperty.ArcherySpeed));
//                    if (RangedAttackType == eRangedAttackType.Critical)
//                        speed = speed * 2 - (GetAbilityLevel(GS.Abilities.Critical_Shot) - 1) * speed / 10;
//                }
//                else
//                {
//                    // no archery bonus
//                    speed *= (1.0 - (qui - 60) * 0.002);
//                }
//            }
//            else
//            {
//                // TODO use haste
//                //Weapon Speed*(1-(Quickness-60)/500]*(1-Haste)
//                speed *= (1.0 - (qui - 60) * 0.002) * 0.01 * GetModified(eProperty.MeleeSpeed);
//            }

//            // apply speed cap
//            if (speed < 15)
//            {
//                speed = 15;
//            }
//            //log.Debug("End of AttackSpeed speed: " + (int)(speed * 100));
//            return (int)(speed * 100);
//        }

//        /// <summary>
//        /// Gets the attack damage
//        /// </summary>
//        /// <param name="weapon">the weapon used for attack</param>
//        /// <returns>the weapon damage</returns>
//        public override double AttackDamage(InventoryItem weapon)
//        {
//            if (weapon == null)
//                return 0;

//            double effectiveness = 1.00;
//            double damage = WeaponDamage(weapon) * weapon.SPD_ABS * 0.1;

//            if (weapon.Hand == 1) // two-hand
//            {
//                // twohanded used weapons get 2H-Bonus = 10% + (Skill / 2)%
//                int spec = WeaponSpecLevel(weapon) - 1;
//                damage *= 1.1 + spec * 0.005;
//            }

//            if (weapon.Item_Type == Slot.RANGED)
//            {
//                //ammo damage bonus
//                if (RangeAttackAmmo != null)
//                {
//                    switch ((RangeAttackAmmo.SPD_ABS) & 0x3)
//                    {
//                        case 0: damage *= 0.85; break; //Blunt       (light) -15%
//                                                       //case 1: damage *= 1;	break; //Bodkin     (medium)   0%
//                        case 2: damage *= 1.15; break; //doesn't exist on live
//                        case 3: damage *= 1.25; break; //Broadhead (X-heavy) +25%
//                    }
//                }
//                //Ranged damage buff,debuff,Relic,RA
//                effectiveness += GetModified(eProperty.RangedDamage) * 0.01;
//            }
//            else if (weapon.Item_Type == Slot.RIGHTHAND || weapon.Item_Type == Slot.LEFTHAND || weapon.Item_Type == Slot.TWOHAND)
//            {
//                //Melee damage buff,debuff,Relic,RA
//                effectiveness += GetModified(eProperty.MeleeDamage) * 0.01;
//            }

//            damage *= effectiveness;

//            return damage;
//        }

//        public override int WeaponSpecLevel(InventoryItem weapon)
//        {
//            if (weapon == null)
//                return 0;

//            // use axe spec if left hand axe is not in the left hand slot
//            if (weapon.Object_Type == (int)eObjectType.LeftAxe && weapon.SlotPosition != Slot.LEFTHAND)
//                return GetObjectSpecLevel(this, eObjectType.Axe);
//            // use left axe spec if axe is in the left hand slot
//            if (weapon.SlotPosition == Slot.LEFTHAND
//                && (weapon.Object_Type == (int)eObjectType.Axe
//                    || weapon.Object_Type == (int)eObjectType.Sword
//                    || weapon.Object_Type == (int)eObjectType.Hammer))
//                return GetObjectSpecLevel(this, eObjectType.LeftAxe);
//            return GetObjectSpecLevel(this, (eObjectType)weapon.Object_Type);
//        }

//        public int WeaponBaseSpecLevel(InventoryItem weapon)
//        {
//            if (weapon == null)
//                return 0;

//            // use axe spec if left hand axe is not in the left hand slot
//            if (weapon.Object_Type == (int)eObjectType.LeftAxe && weapon.SlotPosition != Slot.LEFTHAND)
//                return GetBaseObjectSpecLevel(this, eObjectType.Axe);
//            // use left axe spec if axe is in the left hand slot
//            if (weapon.SlotPosition == Slot.LEFTHAND
//                && (weapon.Object_Type == (int)eObjectType.Axe
//                    || weapon.Object_Type == (int)eObjectType.Sword
//                    || weapon.Object_Type == (int)eObjectType.Hammer))
//                return GetBaseObjectSpecLevel(this, eObjectType.LeftAxe);
//            return GetBaseObjectSpecLevel(this, (eObjectType)weapon.Object_Type);
//        }

//        /// <summary>
//        /// Stores the amount of realm points gained by other players on last death
//        /// </summary>
//        protected long m_lastDeathRealmPoints;

//        /// <summary>
//        /// Gets/sets the amount of realm points gained by other players on last death
//        /// </summary>
//        public long LastDeathRealmPoints
//        {
//            get { return m_lastDeathRealmPoints; }
//            set { m_lastDeathRealmPoints = value; }
//        }

//        /// <summary>
//        /// Called when the player dies
//        /// </summary>
//        /// <param name="killer">the killer</param>
//        //public override void Die(GameObject killer)
//        //{
//        //	// ambiant talk
//        //	if (killer is GameNPC)
//        //		(killer as GameNPC).FireAmbientSentence(GameNPC.eAmbientTrigger.killing, this);

//        //	CharacterClass.Die(killer);

//        //	bool realmDeath = killer != null && killer.Realm != eRealm.None;

//        //	TargetObject = null;
//        //	Diving(waterBreath.Normal);
//        //	if (IsOnHorse)
//        //		IsOnHorse = false;

//        //	// cancel task if active
//        //	if (Task != null && Task.TaskActive)
//        //		Task.ExpireTask();

//        //	string playerMessage;
//        //	string publicMessage;
//        //	ushort messageDistance = WorldMgr.DEATH_MESSAGE_DISTANCE;
//        //	m_releaseType = eReleaseType.Normal;

//        //	string location = "";
//        //	if (CurrentAreas.Count > 0 && (CurrentAreas[0] is Area.BindArea) == false)
//        //		location = (CurrentAreas[0] as AbstractArea).Description;
//        //	else
//        //		location = CurrentZone.Description;

//        //	if (killer == null)
//        //	{
//        //		if (realmDeath)
//        //		{
//        //			playerMessage = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.KilledLocation", GetName(0, true), location);
//        //			publicMessage = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.KilledLocation", GetName(0, true), location);
//        //		}
//        //		else
//        //		{
//        //			playerMessage = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.Killed", GetName(0, true));
//        //			publicMessage = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.Killed", GetName(0, true));
//        //		}
//        //	}
//        //	else
//        //	{
//        //		if (DuelTarget == killer)
//        //		{
//        //			m_releaseType = eReleaseType.Duel;
//        //			messageDistance = WorldMgr.YELL_DISTANCE;
//        //			playerMessage = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.DuelDefeated", GetName(0, true), killer.GetName(1, false));
//        //			publicMessage = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.DuelDefeated", GetName(0, true), killer.GetName(1, false));
//        //		}
//        //		else
//        //		{
//        //			messageDistance = 0;
//        //			if (realmDeath)
//        //			{
//        //				playerMessage = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.KilledByLocation", GetName(0, true), killer.GetName(1, false), location);
//        //				publicMessage = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.KilledByLocation", GetName(0, true), killer.GetName(1, false), location);
//        //			}
//        //			else
//        //			{
//        //				playerMessage = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.KilledBy", GetName(0, true), killer.GetName(1, false));
//        //				publicMessage = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.KilledBy", GetName(0, true), killer.GetName(1, false));
//        //			}
//        //		}
//        //	}

//        //	DuelStop();

//        //	eChatType messageType;
//        //	if (m_releaseType == eReleaseType.Duel)
//        //		messageType = eChatType.CT_Emote;
//        //	else if (killer == null)
//        //	{
//        //		messageType = eChatType.CT_PlayerDied;
//        //	}
//        //	else
//        //	{
//        //		switch ((eRealm)killer.Realm)
//        //		{
//        //			case eRealm.Albion: messageType = eChatType.CT_KilledByAlb; break;
//        //			case eRealm.Midgard: messageType = eChatType.CT_KilledByMid; break;
//        //			case eRealm.Hibernia: messageType = eChatType.CT_KilledByHib; break;
//        //			default: messageType = eChatType.CT_PlayerDied; break; // killed by mob
//        //		}
//        //	}

//        //	if (killer is GamePlayer && killer != this)
//        //	{
//        //		((GamePlayer)killer).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)killer).Client.Account.Language, "GamePlayer.Die.YouKilled", GetName(0, false)), eChatType.CT_PlayerDied, eChatLoc.CL_SystemWindow);
//        //	}

//        //	ArrayList players = new ArrayList();
//        //	if (messageDistance == 0)
//        //	{
//        //		foreach (GameClient client in WorldMgr.GetClientsOfRegion(CurrentRegionID))
//        //		{
//        //			players.Add(client.Player);
//        //		}
//        //	}
//        //	else
//        //	{
//        //		foreach (GamePlayer player in GetPlayersInRadius(messageDistance))
//        //		{
//        //			if (player == null) continue;
//        //			players.Add(player);
//        //		}
//        //	}

//        //	foreach (GamePlayer player in players)
//        //	{
//        //		// on normal server type send messages only to the killer and dead players realm
//        //		// check for gameplayer is needed because killers realm don't see deaths by guards
//        //		if (
//        //			(player != killer) && (
//        //				(killer != null && killer is GamePlayer && GameServer.ServerRules.IsSameRealm((GamePlayer)killer, player, true))
//        //				|| (GameServer.ServerRules.IsSameRealm(this, player, true))
//        //				|| ServerProperties.Properties.DEATH_MESSAGES_ALL_REALMS)
//        //		)
//        //			if (player == this)
//        //				player.Out.SendMessage(playerMessage, messageType, eChatLoc.CL_SystemWindow);
//        //			else player.Out.SendMessage(publicMessage, messageType, eChatLoc.CL_SystemWindow);
//        //	}

//        //	//Dead ppl. dismount ...
//        //	if (Steed != null)
//        //		DismountSteed(true);
//        //	//Dead ppl. don't sit ...
//        //	if (IsSitting)
//        //	{
//        //		IsSitting = false;
//        //		UpdatePlayerStatus();
//        //	}

//        //	// then buffs drop messages
//        //	base.Die(killer);

//        //	lock (m_LockObject)
//        //	{
//        //		if (m_releaseTimer != null)
//        //		{
//        //			m_releaseTimer.Stop();
//        //			m_releaseTimer = null;
//        //		}

//        //		if (m_quitTimer != null)
//        //		{
//        //			m_quitTimer.Stop();
//        //			m_quitTimer = null;
//        //		}
//        //		m_automaticRelease = m_releaseType == eReleaseType.Duel;
//        //		m_releasePhase = 0;
//        //		m_deathTick = Environment.TickCount; // we use realtime, because timer window is realtime

//        //		Out.SendTimerWindow(LanguageMgr.GetTranslation(Client.Account.Language, "System.ReleaseTimer"), (m_automaticRelease ? RELEASE_MINIMUM_WAIT : RELEASE_TIME));
//        //		m_releaseTimer = new RegionTimer(this);
//        //		m_releaseTimer.Callback = new RegionTimerCallback(ReleaseTimerCallback);
//        //		m_releaseTimer.Start(1000);

//        //		Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.ReleaseToReturn"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);

//        //		// clear target object so no more actions can used on this target, spells, styles, attacks...
//        //		TargetObject = null;

//        //		foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
//        //		{
//        //			if (player == null) continue;
//        //			player.Out.SendPlayerDied(this, killer);
//        //		}

//        //		// first penalty is 5% of expforlevel, second penalty comes from release
//        //		int xpLossPercent;
//        //		if (Level < 40)
//        //		{
//        //			xpLossPercent = MaxLevel - Level;
//        //		}
//        //		else
//        //		{
//        //			xpLossPercent = MaxLevel - 40;
//        //		}

//        //		if (realmDeath) //Live PvP servers have 3 con loss on pvp death, can be turned off in server properties -Unty
//        //		{
//        //			int conpenalty = 0;
//        //			switch (GameServer.Instance.Configuration.ServerType)
//        //			{
//        //				case eGameServerType.GST_Normal:
//        //					Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.DeadRVR"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
//        //					xpLossPercent = 0;
//        //					m_deathtype = eDeathType.RvR;
//        //					break;

//        //				case eGameServerType.GST_PvP:
//        //					Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.DeadRVR"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
//        //					xpLossPercent = 0;
//        //					m_deathtype = eDeathType.PvP;
//        //					if (ServerProperties.Properties.PVP_DEATH_CON_LOSS)
//        //					{
//        //						conpenalty = 3;
//        //						TempProperties.setProperty(DEATH_CONSTITUTION_LOSS_PROPERTY, conpenalty);
//        //					}
//        //					break;
//        //			}

//        //		}
//        //		else
//        //		{
//        //			if (Level >= ServerProperties.Properties.PVE_EXP_LOSS_LEVEL)
//        //			{
//        //				Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.LoseExperience"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
//        //				// if this is the first death in level, you lose only half the penalty
//        //				switch (DeathCount)
//        //				{
//        //					case 0:
//        //						Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.DeathN1"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
//        //						xpLossPercent /= 3;
//        //						break;
//        //					case 1:
//        //						Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.DeathN2"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
//        //						xpLossPercent = xpLossPercent * 2 / 3;
//        //						break;
//        //				}

//        //				DeathCount++;
//        //				m_deathtype = eDeathType.PvE;
//        //				long xpLoss = (ExperienceForNextLevel - ExperienceForCurrentLevel) * xpLossPercent / 1000;
//        //				GainExperience(eXPSource.Other, -xpLoss, 0, 0, 0, false, true);
//        //				TempProperties.setProperty(DEATH_EXP_LOSS_PROPERTY, xpLoss);
//        //			}

//        //			if (Level >= ServerProperties.Properties.PVE_CON_LOSS_LEVEL)
//        //			{
//        //				int conLoss = DeathCount;
//        //				if (conLoss > 3)
//        //					conLoss = 3;
//        //				else if (conLoss < 1)
//        //					conLoss = 1;
//        //				TempProperties.setProperty(DEATH_CONSTITUTION_LOSS_PROPERTY, conLoss);
//        //			}
//        //		}
//        //		GameEventMgr.AddHandler(this, GamePlayerEvent.Revive, new DOLEventHandler(OnRevive));
//        //	}

//        //	if (this.ControlledBrain != null)
//        //		CommandNpcRelease();

//        //	if (this.SiegeWeapon != null)
//        //		SiegeWeapon.ReleaseControl();

//        //	// sent after buffs drop
//        //	// GamePlayer.Die.CorpseLies:		{0} just died. {1} corpse lies on the ground.
//        //	Message.SystemToOthers2(this, eChatType.CT_PlayerDied, "GamePlayer.Die.CorpseLies", GetName(0, true), GetPronoun(this.Client, 1, true));

//        //	if (m_releaseType == eReleaseType.Duel)
//        //	{
//        //		Message.SystemToOthers(this, killer.Name + "GamePlayer.Die.DuelWinner", eChatType.CT_Emote);
//        //	}

//        //	// deal out exp and realm points based on server rules
//        //	// no other way to keep correct message order...
//        //	GameServer.ServerRules.OnPlayerKilled(this, killer);
//        //	if (m_releaseType != eReleaseType.Duel)
//        //		DeathTime = PlayedTime;

//        //	IsSwimming = false;
//        //}

//        //public override void EnemyKilled(GameLiving enemy)
//        //{
//        //	if (Group != null)
//        //	{
//        //		foreach (GamePlayer player in Group.GetPlayersInTheGroup())
//        //		{
//        //			if (player == this) continue;
//        //			if (enemy.Attackers.Contains(player)) continue;
//        //			if (this.IsWithinRadius(player, WorldMgr.MAX_EXPFORKILL_DISTANCE))
//        //			{
//        //				Notify(GameLivingEvent.EnemyKilled, player, new EnemyKilledEventArgs(enemy));
//        //			}

//        //			if (player.Attackers.Contains(enemy))
//        //				player.RemoveAttacker(enemy);

//        //			if (player.ControlledBrain != null && player.ControlledBrain.Body.Attackers.Contains(enemy))
//        //				player.ControlledBrain.Body.RemoveAttacker(enemy);
//        //		}
//        //	}

//        //	if (ControlledBrain != null && ControlledBrain.Body.Attackers.Contains(enemy))
//        //		ControlledBrain.Body.RemoveAttacker(enemy);

//        //	base.EnemyKilled(enemy);
//        //}

//        /// <summary>
//        /// Check this flag to see wether this living is involved in combat
//        /// </summary>
//        public override bool InCombat
//        {
//            get
//            {
//                IControlledBrain npc = ControlledBrain;

//                if (npc != null && npc.Body.InCombat)
//                    return true;

//                return base.InCombat;
//            }
//        }

//        protected override AttackData MakeAttack(GameObject target, InventoryItem weapon, Style style, double effectiveness, int interruptDuration, bool dualWield)
//        {
//            //if (IsCrafting)
//            //{
//            //    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.InterruptedCrafting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
//            //    CraftTimer.Stop();
//            //    CraftTimer = null;
//            //    Out.SendCloseTimerWindow();
//            //}

//            AttackData ad = MimicBaseMakeAttack(target, weapon, style, effectiveness * Effectiveness, interruptDuration, dualWield);

//            //Clear the styles for the next round!
//            //NextCombatStyle = null;
//            //NextCombatBackupStyle = null;

//            switch (ad.AttackResult)
//            {
//                case eAttackResult.HitStyle:
//                case eAttackResult.HitUnstyled:
//                {
//                    //keep component
//                    if ((ad.Target is GameKeepComponent || ad.Target is GameKeepDoor || ad.Target is GameSiegeWeapon) && ad.Attacker is MimicNPC && ad.Attacker.GetModified(eProperty.KeepDamage) > 0)
//                    {
//                        int keepdamage = (int)Math.Floor((double)ad.Damage * ((double)ad.Attacker.GetModified(eProperty.KeepDamage) / 100));
//                        int keepstyle = (int)Math.Floor((double)ad.StyleDamage * ((double)ad.Attacker.GetModified(eProperty.KeepDamage) / 100));
//                        ad.Damage += keepdamage;
//                        ad.StyleDamage += keepstyle;
//                    }
//                    // vampiir
//                    if (CharacterClass.Equals(GS.CharacterClass.Vampiir)
//                        && target is GameKeepComponent == false
//                        && target is GameKeepDoor == false
//                        && target is GameSiegeWeapon == false)
//                    {
//                        int perc = Convert.ToInt32(((double)(ad.Damage + ad.CriticalDamage) / 100) * (55 - this.Level));
//                        perc = (perc < 1) ? 1 : ((perc > 15) ? 15 : perc);
//                        this.Mana += Convert.ToInt32(Math.Ceiling(((Decimal)(perc * this.MaxMana) / 100)));
//                    }

//                    //only miss when strafing when attacking a player
//                    //30% chance to miss
//                    if (IsStrafing && ad.Target is GamePlayer && Util.Chance(30))
//                    {
//                        ad.AttackResult = eAttackResult.Missed;
//                        //Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.StrafMiss"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
//                        break;
//                    }
//                    break;
//                }
//            }

//            switch (ad.AttackResult)
//            {
//                case eAttackResult.Blocked:
//                case eAttackResult.Fumbled:
//                case eAttackResult.HitStyle:
//                case eAttackResult.HitUnstyled:
//                case eAttackResult.Missed:
//                case eAttackResult.Parried:
//                //Condition percent can reach 70%
//                //durability percent can reach zero
//                // if item durability reachs 0, item is useless and become broken item

//                if (weapon != null && weapon is GameInventoryItem)
//                {
//                    (weapon as GameInventoryItem).OnStrikeTarget(this, target);
//                }
//                //Camouflage - Camouflage will be disabled only when attacking a GamePlayer or ControlledNPC of a GamePlayer.
//                //if (HasAbility(Abilities.Camouflage) && target is GamePlayer || (target is GameNPC && (target as GameNPC).Brain is IControlledBrain && ((target as GameNPC).Brain as IControlledBrain).GetPlayerOwner() != null))
//                //{
//                //    CamouflageEffect camouflage = EffectList.GetOfType<CamouflageEffect>();

//                //    if (camouflage != null)// Check if Camo is active, if true, cancel ability.
//                //    {
//                //        camouflage.Cancel(false);
//                //    }
//                //    Skill camo = SkillBase.GetAbility(Abilities.Camouflage); // now we find the ability
//                //    DisableSkill(camo, CamouflageSpecHandler.DISABLE_DURATION); // and here we disable it.
//                //}

//                // Multiple Hit check
//                if (ad.AttackResult == eAttackResult.HitStyle)
//                {
//                    byte numTargetsCanHit = 0;
//                    int random;
//                    IList extraTargets = new ArrayList();
//                    IList listAvailableTargets = new ArrayList();
//                    InventoryItem attackWeapon = AttackWeapon;
//                    InventoryItem leftWeapon = (Inventory == null) ? null : Inventory.GetItem(eInventorySlot.LeftHandWeapon);
//                    switch (style.ID)
//                    {
//                        case 374: numTargetsCanHit = 1; break; //Tribal Assault:   Hits 2 targets
//                        case 377: numTargetsCanHit = 1; break; //Clan's Might:      Hits 2 targets
//                        case 379: numTargetsCanHit = 2; break; //Totemic Wrath:      Hits 3 targets
//                        case 384: numTargetsCanHit = 3; break; //Totemic Sacrifice:   Hits 4 targets
//                        case 600: numTargetsCanHit = 255; break; //Shield Swipe: No Cap on Targets
//                        default: numTargetsCanHit = 0; break; //For others;
//                    }
//                    if (numTargetsCanHit > 0)
//                    {
//                        if (style.ID != 600) // Not Shield Swipe
//                        {
//                            foreach (GamePlayer pl in GetPlayersInRadius(false, (ushort)AttackRange))
//                            {
//                                if (pl == null) continue;
//                                if (GameServer.ServerRules.IsAllowedToAttack(this, pl, true))
//                                {
//                                    listAvailableTargets.Add(pl);
//                                }
//                            }
//                            foreach (GameNPC npc in GetNPCsInRadius(false, (ushort)AttackRange))
//                            {
//                                if (GameServer.ServerRules.IsAllowedToAttack(this, npc, true))
//                                {
//                                    listAvailableTargets.Add(npc);
//                                }
//                            }

//                            // remove primary target
//                            listAvailableTargets.Remove(target);
//                            numTargetsCanHit = (byte)Math.Min(numTargetsCanHit, listAvailableTargets.Count);

//                            if (listAvailableTargets.Count > 1)
//                            {
//                                while (extraTargets.Count < numTargetsCanHit)
//                                {
//                                    random = Util.Random(listAvailableTargets.Count - 1);
//                                    if (!extraTargets.Contains(listAvailableTargets[random]))
//                                        extraTargets.Add(listAvailableTargets[random] as GameObject);
//                                }
//                                foreach (GameObject obj in extraTargets)
//                                {
//                                    if (obj is GamePlayer && ((GamePlayer)obj).IsSitting)
//                                    {
//                                        effectiveness *= 2;
//                                    }
//                                    new WeaponOnTargetAction(this, obj as GameObject, attackWeapon, leftWeapon, effectiveness, AttackSpeed(attackWeapon), null).Start(1);  // really start the attack
//                                }
//                            }
//                        }
//                        else // shield swipe
//                        {
//                            foreach (GameNPC npc in GetNPCsInRadius(false, (ushort)AttackRange))
//                            {
//                                if (GameServer.ServerRules.IsAllowedToAttack(this, npc, true))
//                                {
//                                    listAvailableTargets.Add(npc);
//                                }
//                            }

//                            listAvailableTargets.Remove(target);
//                            numTargetsCanHit = (byte)Math.Min(numTargetsCanHit, listAvailableTargets.Count);

//                            if (listAvailableTargets.Count > 1)
//                            {
//                                while (extraTargets.Count < numTargetsCanHit)
//                                {
//                                    random = Util.Random(listAvailableTargets.Count - 1);
//                                    if (!extraTargets.Contains(listAvailableTargets[random]))
//                                    {
//                                        extraTargets.Add(listAvailableTargets[random] as GameObject);
//                                    }
//                                }
//                                foreach (GameNPC obj in extraTargets)
//                                {
//                                    if (obj != ad.Target)
//                                    {
//                                        this.MakeAttack(obj, attackWeapon, null, 1, ServerProperties.Properties.SPELL_INTERRUPT_DURATION, false, false);
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }
//                break;
//            }
//            return ad;
//        }

//        /// <summary>
//		/// This method is called to make an attack, it is called from the
//		/// attacktimer and should not be called manually
//		/// </summary>
//		/// <param name="target">the target that is attacked</param>
//		/// <param name="weapon">the weapon used for attack</param>
//		/// <param name="style">the style used for attack</param>
//		/// <param name="effectiveness">damage effectiveness (0..1)</param>
//		/// <param name="interruptDuration">the interrupt duration</param>
//		/// <param name="dualWield">indicates if both weapons are used for attack</param>
//		/// <returns>the object where we collect and modifiy all parameters about the attack</returns>
//		protected virtual AttackData MimicBaseMakeAttack(GameObject target, InventoryItem weapon, Style style, double effectiveness, int interruptDuration, bool dualWield)
//        {
//            return MimicBaseMakeAttack(target, weapon, style, effectiveness, interruptDuration, dualWield, false);
//        }

//        protected virtual AttackData MimicBaseMakeAttack(GameObject target, InventoryItem weapon, Style style, double effectiveness, int interruptDuration, bool dualWield, bool ignoreLOS)
//        {
//            AttackData ad = new AttackData();
//            ad.Attacker = this;
//            ad.Target = target as GameLiving;
//            ad.Damage = 0;
//            ad.CriticalDamage = 0;
//            ad.Style = style;
//            ad.WeaponSpeed = AttackSpeed(weapon) / 100;
//            ad.DamageType = AttackDamageType(weapon);
//            ad.ArmorHitLocation = eArmorSlot.NOTSET;
//            ad.Weapon = weapon;
//            ad.IsOffHand = weapon == null ? false : weapon.Hand == 2;

//            if (dualWield)
//                ad.AttackType = AttackData.eAttackType.MeleeDualWield;
//            else if (weapon == null)
//                ad.AttackType = AttackData.eAttackType.MeleeOneHand;
//            else
//                switch (weapon.Item_Type)
//                {
//                    default:
//                    case Slot.RIGHTHAND:
//                    case Slot.LEFTHAND: ad.AttackType = AttackData.eAttackType.MeleeOneHand; break;
//                    case Slot.TWOHAND: ad.AttackType = AttackData.eAttackType.MeleeTwoHand; break;
//                    case Slot.RANGED: ad.AttackType = AttackData.eAttackType.Ranged; break;
//                }

//            //No target, stop the attack
//            if (ad.Target == null)
//            {
//                ad.AttackResult = (target == null) ? eAttackResult.NoTarget : eAttackResult.NoValidTarget;
//                return ad;
//            }

//            // check region
//            if (ad.Target.CurrentRegionID != CurrentRegionID || ad.Target.ObjectState != eObjectState.Active)
//            {
//                ad.AttackResult = eAttackResult.NoValidTarget;
//                return ad;
//            }

//            //Check if the target is in front of attacker
//            if (!ignoreLOS && ad.AttackType != AttackData.eAttackType.Ranged && this is MimicNPC &&
//                !(ad.Target is GameKeepComponent) && !(IsObjectInFront(ad.Target, 120, true) && TargetInView))
//            {
//                ad.AttackResult = eAttackResult.TargetNotVisible;
//                return ad;
//            }

//            //Target is dead already
//            if (!ad.Target.IsAlive)
//            {
//                ad.AttackResult = eAttackResult.TargetDead;
//                return ad;
//            }

//            //We have no attacking distance!
//            if (!this.IsWithinRadius(ad.Target, ad.Target.ActiveWeaponSlot == eActiveWeaponSlot.Standard ? Math.Max(AttackRange, ad.Target.AttackRange) : AttackRange))
//            {
//                ad.AttackResult = eAttackResult.OutOfRange;
//                return ad;
//            }

//            if (RangedAttackType == eRangedAttackType.Long)
//            {
//                RangedAttackType = eRangedAttackType.Normal;
//            }

//            if (!GameServer.ServerRules.IsAllowedToAttack(ad.Attacker, ad.Target, false))
//            {
//                ad.AttackResult = eAttackResult.NotAllowed_ServerRules;
//                return ad;
//            }

//            if (SpellHandler.FindEffectOnTarget(this, "Phaseshift") != null)
//            {
//                ad.AttackResult = eAttackResult.Phaseshift;
//                return ad;
//            }

//            // Apply Mentalist RA5L
//            SelectiveBlindnessEffect SelectiveBlindness = EffectList.GetOfType<SelectiveBlindnessEffect>();

//            if (SelectiveBlindness != null)
//            {
//                GameLiving EffectOwner = SelectiveBlindness.EffectSource;

//                if (EffectOwner == ad.Target)
//                {
//                    ad.AttackResult = eAttackResult.NoValidTarget;
//                    return ad;
//                }
//            }

//            // DamageImmunity Ability
//            if ((GameLiving)target != null && ((GameLiving)target).HasAbility(DOL.GS.Abilities.DamageImmunity))
//            {
//                //if (ad.Attacker is GamePlayer) ((GamePlayer)ad.Attacker).Out.SendMessage(string.Format("{0} can't be attacked!", ad.Target.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
//                ad.AttackResult = eAttackResult.NoValidTarget;
//                return ad;
//            }

//            //Calculate our attack result and attack damage
//            ad.AttackResult = ad.Target.CalculateEnemyAttackResult(ad, weapon);

//            // calculate damage only if we hit the target
//            if (ad.AttackResult == eAttackResult.HitUnstyled
//                || ad.AttackResult == eAttackResult.HitStyle)
//            {
//                double damage = AttackDamage(weapon) * effectiveness;

//                InventoryItem armor = null;

//                if (ad.Target.Inventory != null)
//                    armor = ad.Target.Inventory.GetItem((eInventorySlot)ad.ArmorHitLocation);

//                InventoryItem weaponTypeToUse = null;

//                if (weapon != null)
//                {
//                    weaponTypeToUse = new InventoryItem();
//                    weaponTypeToUse.Object_Type = weapon.Object_Type;
//                    weaponTypeToUse.SlotPosition = weapon.SlotPosition;

//                    if ((this is MimicNPC) && Realm == eRealm.Albion
//                        && (GameServer.ServerRules.IsObjectTypesEqual((eObjectType)weapon.Object_Type, eObjectType.TwoHandedWeapon)
//                        || GameServer.ServerRules.IsObjectTypesEqual((eObjectType)weapon.Object_Type, eObjectType.PolearmWeapon))
//                        && ServerProperties.Properties.ENABLE_ALBION_ADVANCED_WEAPON_SPEC)
//                    {
//                        // Albion dual spec penalty, which sets minimum damage to the base damage spec
//                        if (weapon.Type_Damage == (int)eDamageType.Crush)
//                        {
//                            weaponTypeToUse.Object_Type = (int)eObjectType.CrushingWeapon;
//                        }
//                        else if (weapon.Type_Damage == (int)eDamageType.Slash)
//                        {
//                            weaponTypeToUse.Object_Type = (int)eObjectType.SlashingWeapon;
//                        }
//                        else
//                        {
//                            weaponTypeToUse.Object_Type = (int)eObjectType.ThrustWeapon;
//                        }
//                    }
//                }

//                int lowerboundary = (WeaponSpecLevel(weaponTypeToUse) - 1) * 50 / (ad.Target.EffectiveLevel + 1) + 75;
//                lowerboundary = Math.Max(lowerboundary, 75);
//                lowerboundary = Math.Min(lowerboundary, 125);
//                damage *= (GetWeaponSkill(weapon) + 90.68) / (ad.Target.GetArmorAF(ad.ArmorHitLocation) + 20 * 4.67);

//                // Badge Of Valor Calculation 1+ absorb or 1- absorb
//                if (ad.Attacker.EffectList.GetOfType<BadgeOfValorEffect>() != null)
//                {
//                    damage *= 1.0 + Math.Min(0.85, ad.Target.GetArmorAbsorb(ad.ArmorHitLocation));
//                }
//                else
//                {
//                    damage *= 1.0 - Math.Min(0.85, ad.Target.GetArmorAbsorb(ad.ArmorHitLocation));
//                }

//                damage *= (lowerboundary + Util.Random(50)) * 0.01;
//                ad.Modifier = (int)(damage * (ad.Target.GetResist(ad.DamageType) + SkillBase.GetArmorResist(armor, ad.DamageType)) * -0.01);
//                damage += ad.Modifier;
//                // RA resist check
//                int resist = (int)(damage * ad.Target.GetDamageResist(GetResistTypeForDamage(ad.DamageType)) * -0.01);

//                eProperty property = ad.Target.GetResistTypeForDamage(ad.DamageType);
//                int secondaryResistModifier = ad.Target.SpecBuffBonusCategory[(int)property];
//                int resistModifier = 0;
//                resistModifier += (int)((ad.Damage + (double)resistModifier) * (double)secondaryResistModifier * -0.01);

//                damage += resist;
//                damage += resistModifier;
//                ad.Modifier += resist;
//                ad.Damage = (int)damage;

//                // apply total damage cap
//                ad.UncappedDamage = ad.Damage;
//                ad.Damage = Math.Min(ad.Damage, (int)(UnstyledDamageCap(weapon) * effectiveness));

//                if ((this is MimicNPC || (this is GameNPC && (this as GameNPC).Brain is IControlledBrain && this.Realm != 0)) && (target is GamePlayer || target is MimicNPC))
//                {
//                    ad.Damage = (int)((double)ad.Damage * ServerProperties.Properties.PVP_MELEE_DAMAGE);
//                }
//                else if ((this is MimicNPC || (this is GameNPC && (this as GameNPC).Brain is IControlledBrain && this.Realm != 0)) && (target is GameNPC && target is not MimicNPC))
//                {
//                    ad.Damage = (int)((double)ad.Damage * ServerProperties.Properties.PVE_MELEE_DAMAGE);
//                }

//                ad.UncappedDamage = ad.Damage;

//                //Eden - Conversion Bonus (Crocodile Ring)  - tolakram - critical damage is always 0 here, needs to be moved
//                if ((ad.Target is GamePlayer || ad.Target is MimicNPC) && ad.Target.GetModified(eProperty.Conversion) > 0)
//                {
//                    int manaconversion = (int)Math.Round(((double)ad.Damage + (double)ad.CriticalDamage) * (double)ad.Target.GetModified(eProperty.Conversion) / 100);
//                    //int enduconversion=(int)Math.Round((double)manaconversion*(double)ad.Target.MaxEndurance/(double)ad.Target.MaxMana);
//                    int enduconversion = (int)Math.Round(((double)ad.Damage + (double)ad.CriticalDamage) * (double)ad.Target.GetModified(eProperty.Conversion) / 100);

//                    if (ad.Target.Mana + manaconversion > ad.Target.MaxMana)
//                        manaconversion = ad.Target.MaxMana - ad.Target.Mana;

//                    if (ad.Target.Endurance + enduconversion > ad.Target.MaxEndurance)
//                        enduconversion = ad.Target.MaxEndurance - ad.Target.Endurance;

//                    if (manaconversion < 1)
//                        manaconversion = 0;

//                    if (enduconversion < 1)
//                        enduconversion = 0;

//                    if (manaconversion >= 1 && ad.Target is not MimicNPC)
//                        (ad.Target as GamePlayer).Out.SendMessage(string.Format(LanguageMgr.GetTranslation((ad.Target as GamePlayer).Client.Account.Language, "GameLiving.AttackData.GainPowerPoints"), manaconversion), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);

//                    if (enduconversion >= 1 && ad.Target is not MimicNPC)
//                        (ad.Target as GamePlayer).Out.SendMessage(string.Format(LanguageMgr.GetTranslation((ad.Target as GamePlayer).Client.Account.Language, "GameLiving.AttackData.GainEndurancePoints"), enduconversion), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);

//                    ad.Target.Endurance += enduconversion;

//                    if (ad.Target.Endurance > ad.Target.MaxEndurance)
//                        ad.Target.Endurance = ad.Target.MaxEndurance;

//                    ad.Target.Mana += manaconversion;

//                    if (ad.Target.Mana > ad.Target.MaxMana)
//                        ad.Target.Mana = ad.Target.MaxMana;
//                }

//                // Tolakram - let's go ahead and make it 1 damage rather than spamming a possible error
//                if (ad.Damage == 0)
//                {
//                    ad.Damage = 1;

//                    // log this as a possible error if we should do some damage to target
//                    //if (ad.Target.Level <= Level + 5 && weapon != null)
//                    //{
//                    //    log.ErrorFormat("Possible Damage Error: {0} Damage = 0 -> miss vs {1}.  AttackDamage {2}, weapon name {3}", Name, (ad.Target == null ? "null" : ad.Target.Name), AttackDamage(weapon), (weapon == null ? "None" : weapon.Name));
//                    //}

//                    //ad.AttackResult = eAttackResult.Missed;
//                }
//            }

//            //TODO: MIMIC ENDURANCE DRAIN
//            //Add styled damage if style hits and remove endurance if missed
//            if (MimicExecuteStyle(this, ad, weapon))
//            {
//                ad.AttackResult = GameLiving.eAttackResult.HitStyle;
//            }

//            if ((ad.AttackResult == eAttackResult.HitUnstyled || ad.AttackResult == eAttackResult.HitStyle))
//            {
//                ad.CriticalDamage = GetMeleeCriticalDamage(ad, weapon);
//            }

//            // Attacked living may modify the attack data.  Primarily used for keep doors and components.
//            ad.Target.ModifyAttack(ad);

//            if (ad.AttackResult == eAttackResult.HitStyle)
//            {
//                if (Group != null)
//                {
//                    string damageAmount = (ad.StyleDamage > 0) ? " (+" + ad.StyleDamage + ")" : "";
//                    Group.SendMessageToGroupMembers(damageAmount, eChatType.CT_Group, eChatLoc.CL_ChatWindow);
//                    //mimic.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "StyleProcessor.ExecuteStyle.PerformPerfectly", ad.Style.Name, damageAmount), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
//                }
//            }

//            string message = "";
//            bool broadcast = true;
//            ArrayList excludes = new ArrayList();
//            excludes.Add(ad.Attacker);
//            excludes.Add(ad.Target);

//            switch (ad.AttackResult)
//            {
//                case eAttackResult.Parried: message = string.Format("{0} attacks {1} and is parried!", ad.Attacker.GetName(0, true), ad.Target.GetName(0, false)); break;
//                case eAttackResult.Evaded: message = string.Format("{0} attacks {1} and is evaded!", ad.Attacker.GetName(0, true), ad.Target.GetName(0, false)); break;
//                case eAttackResult.Missed: message = string.Format("{0} attacks {1} and misses!", ad.Attacker.GetName(0, true), ad.Target.GetName(0, false)); break;

//                case eAttackResult.Blocked:
//                {
//                    message = string.Format("{0} attacks {1} and is blocked!", ad.Attacker.GetName(0, true), ad.Target.GetName(0, false));
//                    // guard messages
//                    if (target != null && target != ad.Target)
//                    {
//                        excludes.Add(target);

//                        // another player blocked for real target
//                        if (target is GamePlayer)
//                            ((GamePlayer)target).Out.SendMessage(string.Format(LanguageMgr.GetTranslation(((GamePlayer)target).Client.Account.Language, "GameLiving.AttackData.BlocksYou"), ad.Target.GetName(0, true), ad.Attacker.GetName(0, false)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);

//                        // blocked for another player
//                        if (ad.Target is GamePlayer)
//                            ((GamePlayer)ad.Target).Out.SendMessage(string.Format(LanguageMgr.GetTranslation(((GamePlayer)ad.Target).Client.Account.Language, "GameLiving.AttackData.YouBlock"), ad.Attacker.GetName(0, false), target.GetName(0, false)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);

//                        ad.Target.Stealth(false);
//                    }
//                    else if (ad.Target is GamePlayer)
//                    {
//                        ((GamePlayer)ad.Target).Out.SendMessage(string.Format(LanguageMgr.GetTranslation(((GamePlayer)ad.Target).Client.Account.Language, "GameLiving.AttackData.AttacksYou"), ad.Attacker.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
//                    }
//                    break;
//                }
//                case eAttackResult.HitUnstyled:
//                case eAttackResult.HitStyle:
//                {
//                    if (target != null && target != ad.Target)
//                    {
//                        message = string.Format("{0} attacks {1} but hits {2}!", ad.Attacker.GetName(0, true), target.GetName(0, false), ad.Target.GetName(0, false));
//                        excludes.Add(target);

//                        // intercept for another player
//                        if (target is GamePlayer)
//                            ((GamePlayer)target).Out.SendMessage(string.Format(LanguageMgr.GetTranslation(((GamePlayer)target).Client.Account.Language, "GameLiving.AttackData.StepsInFront"), ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

//                        // intercept by player
//                        if (ad.Target is GamePlayer)
//                            ((GamePlayer)ad.Target).Out.SendMessage(string.Format(LanguageMgr.GetTranslation(((GamePlayer)ad.Target).Client.Account.Language, "GameLiving.AttackData.YouStepInFront"), target.GetName(0, false)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
//                    }
//                    else
//                    {
//                        if (ad.Attacker is GamePlayer)
//                        {
//                            string hitWeapon = "weapon";
//                            if (weapon != null)
//                                hitWeapon = GlobalConstants.NameToShortName(weapon.Name);
//                            message = string.Format("{0} attacks {1} with {2} {3}!", ad.Attacker.GetName(0, true), ad.Target.GetName(0, false), ad.Attacker.GetPronoun(1, false), hitWeapon);
//                        }
//                        else
//                        {
//                            message = string.Format("{0} attacks {1} and hits!", ad.Attacker.GetName(0, true), ad.Target.GetName(0, false));
//                        }
//                    }
//                    break;
//                }
//                default: broadcast = false; break;
//            }

//            #region Prevent Flight

//            if (ad.Attacker is GamePlayer)
//            {
//                //GamePlayer attacker = ad.Attacker as GamePlayer;

//                //TODO: Implement Prevent Flight

//                //if (attacker.HasAbility(Abilities.PreventFlight) && Util.Chance(10))
//                //{
//                //    if (IsObjectInFront(ad.Target, 120) && ad.Target.IsMoving)
//                //    {
//                //        bool preCheck = false;
//                //        if (ad.Target is GamePlayer) //only start if we are behind the player
//                //        {
//                //            float angle = ad.Target.GetAngle(ad.Attacker);
//                //            if (angle >= 150 && angle < 210) preCheck = true;
//                //        }
//                //        else preCheck = true;

//                //        if (preCheck)
//                //        {
//                //            Spell spell = SkillBase.GetSpellByID(7083);
//                //            if (spell != null)
//                //            {
//                //                ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(this, spell, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
//                //                if (spellHandler != null)
//                //                {
//                //                    spellHandler.StartSpell(ad.Target);
//                //                }
//                //            }
//                //        }
//                //    }
//                //}
//            }

//            #endregion Prevent Flight

//            if (ad.Target is GameNPC)
//            {
//                IControlledBrain brain = ((GameNPC)ad.Target).Brain as IControlledBrain;

//                if (brain != null)
//                {
//                    GameLiving owner_living = brain.GetLivingOwner();
//                    excludes.Add(owner_living);
//                    if (owner_living is GamePlayer owner && ad.Target == owner.ControlledBody)
//                    {
//                        switch (ad.AttackResult)
//                        {
//                            case eAttackResult.Blocked:
//                            owner.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(owner.Client.Account.Language, "GameLiving.AttackData.Blocked"), ad.Attacker.GetName(0, true), ad.Target.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
//                            break;

//                            case eAttackResult.Parried:
//                            owner.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(owner.Client.Account.Language, "GameLiving.AttackData.Parried"), ad.Attacker.GetName(0, true), ad.Target.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
//                            break;

//                            case eAttackResult.Evaded:
//                            owner.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(owner.Client.Account.Language, "GameLiving.AttackData.Evaded"), ad.Attacker.GetName(0, true), ad.Target.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
//                            break;

//                            case eAttackResult.Fumbled:
//                            owner.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(owner.Client.Account.Language, "GameLiving.AttackData.Fumbled"), ad.Attacker.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
//                            break;

//                            case eAttackResult.Missed:
//                            if (ad.AttackType != AttackData.eAttackType.Spell)
//                                owner.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(owner.Client.Account.Language, "GameLiving.AttackData.Misses"), ad.Attacker.GetName(0, true), ad.Target.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
//                            break;

//                            case eAttackResult.HitStyle:
//                            case eAttackResult.HitUnstyled:
//                            {
//                                string modmessage = "";
//                                if (ad.Modifier > 0) modmessage = " (+" + ad.Modifier + ")";
//                                if (ad.Modifier < 0) modmessage = " (" + ad.Modifier + ")";
//                                owner.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(owner.Client.Account.Language, "GameLiving.AttackData.HitsForDamage"), ad.Attacker.GetName(0, true), ad.Target.Name, ad.Damage, modmessage), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
//                                if (ad.CriticalDamage > 0)
//                                {
//                                    owner.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(owner.Client.Account.Language, "GameLiving.AttackData.CriticallyHitsForDamage"), ad.Attacker.GetName(0, true), ad.Target.Name, ad.CriticalDamage), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
//                                }
//                                break;
//                            }
//                            default: break;
//                        }
//                    }
//                }
//            }

//            #endregion Combat

//            // broadcast messages
//            if (broadcast)
//            {
//                Message.SystemToArea(ad.Attacker, message, eChatType.CT_OthersCombat, (GameObject[])excludes.ToArray(typeof(GameObject)));
//            }

//            ad.Target.StartInterruptTimer(ad, interruptDuration);
//            //Return the result
//            return ad;
//        }

//        public static bool MimicExecuteStyle(GameLiving living, AttackData attackData, InventoryItem weapon)
//        {
//            //First thing in processors, lock the objects you modify
//            //This way it makes sure the objects are not modified by
//            //several different threads at the same time!

//            MimicNPC mimic = living as MimicNPC;

//            lock (living)
//            {
//                //Does the player want to execute a style at all?
//                if (attackData.Style == null)
//                    return false;

//                if (weapon != null && weapon.Object_Type == (int)eObjectType.Shield)
//                {
//                    attackData.AnimationId = (weapon.Hand != 1) ? attackData.Style.Icon : attackData.Style.TwoHandAnimation; // 2h shield?
//                }

//                int fatCost = 0;

//                if (weapon != null)
//                    fatCost = StyleProcessor.CalculateEnduranceCost(living, attackData.Style, weapon.SPD_ABS);

//                //Reduce endurance if styled attack missed
//                switch (attackData.AttackResult)
//                {
//                    case GameLiving.eAttackResult.Blocked:
//                    case GameLiving.eAttackResult.Evaded:
//                    case GameLiving.eAttackResult.Missed:
//                    case GameLiving.eAttackResult.Parried:

//                    if (mimic != null) //No mob endu lost yet
//                        living.Endurance -= Math.Max(1, fatCost / 2);

//                    return false;
//                }

//                //Ignore all other attack results
//                if (attackData.AttackResult != GameLiving.eAttackResult.HitUnstyled
//                    && attackData.AttackResult != GameLiving.eAttackResult.HitStyle)
//                    return false;

//                //Did primary and backup style fail?
//                if (!StyleProcessor.CanUseStyle(living, attackData.Style, weapon))
//                {
//                    if (mimic != null)
//                    {
//                        // reduce players endurance, full endurance if failed style
//                        mimic.Endurance -= fatCost;

//                        //"You must be hidden to perform this style!"
//                        //Print a style-fail message
//                        //player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "StyleProcessor.ExecuteStyle.ExecuteFail", attackData.Style.Name), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
//                    }
//                    return false;
//                }
//                else
//                {
//                    //Style worked! Print out some nice info and add the damage! :)
//                    //Growth * Style Spec * Effective Speed / Unstyled Damage Cap

//                    bool staticGrowth = attackData.Style.StealthRequirement;  //static growth is not a function of (effective) weapon speed
//                    double absorbRatio = 0;

//                    if (weapon.DPS_AF >= 15)
//                        absorbRatio = attackData.Damage / living.UnstyledDamageCap(weapon);

//                    double effectiveWeaponSpeed = living.AttackSpeed(weapon) * 0.001;
//                    double styleGrowth = Math.Max(0, attackData.Style.GrowthOffset + attackData.Style.GrowthRate * living.GetModifiedSpecLevel(attackData.Style.Spec));
//                    double styleDamageBonus = living.GetModified(eProperty.StyleDamage) * 0.01 - 1;

//                    if (staticGrowth)
//                    {
//                        if (living.AttackWeapon.Item_Type == Slot.TWOHAND)
//                        {
//                            styleGrowth = styleGrowth * 1.25 + living.WeaponDamage(living.AttackWeapon) * Math.Max(0, living.AttackWeapon.SPD_ABS - 21) * 10 / 66d;
//                        }

//                        attackData.StyleDamage = (int)(absorbRatio * styleGrowth * ServerProperties.Properties.CS_OPENING_EFFECTIVENESS);
//                    }
//                    else
//                        attackData.StyleDamage = (int)(absorbRatio * styleGrowth * effectiveWeaponSpeed);

//                    attackData.StyleDamage += (int)(attackData.Damage * styleDamageBonus);

//                    //Eden - style absorb bonus
//                    int absorb = 0;

//                    if ((attackData.Target is GamePlayer || attackData.Target is MimicNPC) && attackData.Target.GetModified(eProperty.StyleAbsorb) > 0)
//                    {
//                        absorb = (int)Math.Floor((double)attackData.StyleDamage * ((double)attackData.Target.GetModified(eProperty.StyleAbsorb) / 100));
//                        attackData.StyleDamage -= absorb;
//                    }

//                    //Increase regular damage by styledamage ... like on live servers
//                    attackData.Damage += attackData.StyleDamage;

//                    if (mimic != null)
//                    {
//                        // reduce mimics endurance
//                        mimic.Endurance -= fatCost;

//                        if (absorb > 0)
//                        {
//                            //player.Out.SendMessage("A barrier absorbs " + absorb + " damage!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
//                            if (living is GamePlayer)
//                                (living as GamePlayer).Out.SendMessage("A barrier absorbs " + absorb + " damage!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
//                        }
//                    }

//                    #region StyleProcs

//                    if (attackData.Style.Procs.Count > 0)
//                    {
//                        ISpellHandler effect;

//                        // If ClassID = 0, use the proc for any class, unless there is also a proc with a ClassID
//                        // that matches the player's CharacterClass.ID, or for mobs, the style's ClassID - then use
//                        // the class-specific proc instead of the ClassID=0 proc
//                        if (!attackData.Style.RandomProc)
//                        {
//                            List<Tuple<Spell, int, int>> procsToExecute = new List<Tuple<Spell, int, int>>();
//                            bool onlyExecuteClassSpecific = false;

//                            foreach (Tuple<Spell, int, int> proc in attackData.Style.Procs)
//                            {
//                                if (mimic != null && proc.Item2 == mimic.CharacterClass.ID)
//                                {
//                                    procsToExecute.Add(proc);
//                                    onlyExecuteClassSpecific = true;
//                                }
//                                else if (proc.Item2 == attackData.Style.ClassID || proc.Item2 == 0)
//                                {
//                                    procsToExecute.Add(proc);
//                                }
//                            }

//                            foreach (Tuple<Spell, int, int> procToExecute in procsToExecute)
//                            {
//                                if (onlyExecuteClassSpecific && procToExecute.Item2 == 0)
//                                    continue;

//                                if (Util.Chance(procToExecute.Item3))
//                                {
//                                    effect = CreateMagicEffect(living, attackData.Target, procToExecute.Item1.ID);
//                                    //effect could be null if the SpellID is bigger than ushort
//                                    if (effect != null)
//                                    {
//                                        attackData.StyleEffects.Add(effect);
//                                        if ((attackData.Style.OpeningRequirementType == Style.eOpening.Offensive && attackData.Style.OpeningRequirementValue > 0)
//                                            || attackData.Style.OpeningRequirementType == Style.eOpening.Defensive)
//                                        {
//                                            effect.UseMinVariance = true;
//                                        }
//                                    }
//                                }
//                            }
//                        }
//                        else
//                        {
//                            //Add one proc randomly
//                            int random = Util.Random(attackData.Style.Procs.Count - 1);
//                            //effect could be null if the SpellID is bigger than ushort
//                            effect = CreateMagicEffect(living, attackData.Target, attackData.Style.Procs[random].Item1.ID);
//                            if (effect != null)
//                            {
//                                attackData.StyleEffects.Add(effect);
//                                if ((attackData.Style.OpeningRequirementType == Style.eOpening.Offensive && attackData.Style.OpeningRequirementValue > 0)
//                                    || attackData.Style.OpeningRequirementType == Style.eOpening.Defensive)
//                                {
//                                    effect.UseMinVariance = true;
//                                }
//                            }
//                        }
//                    }

//                    #endregion StyleProcs

//                    #region Animation

//                    if (weapon != null)
//                        attackData.AnimationId = (weapon.Hand != 1) ? attackData.Style.Icon : attackData.Style.TwoHandAnimation; // special animation for two-hand
//                    else if (living.Inventory != null)
//                        attackData.AnimationId = (living.Inventory.GetItem(eInventorySlot.RightHandWeapon) != null) ? attackData.Style.Icon : attackData.Style.TwoHandAnimation; // special animation for two-hand
//                    else
//                        attackData.AnimationId = attackData.Style.Icon;

//                    #endregion Animation

//                    return true;
//                }
//            }
//        }

//        public override int GetMeleeCriticalDamage(AttackData ad, InventoryItem weapon)
//        {
//            if (Util.Chance(AttackCriticalChance(weapon)))
//            {
//                // triple wield prevents critical hits
//                if (ad.Target.EffectList.GetOfType<TripleWieldEffect>() != null) return 0;

//                int critMin;
//                int critMax;
//                BerserkEffect berserk = EffectList.GetOfType<BerserkEffect>();

//                if (berserk != null)
//                {
//                    int level = GetAbilityLevel(DOL.GS.Abilities.Berserk);
//                    // According to : http://daoc.catacombs.com/forum.cfm?ThreadKey=10833&DefMessage=922046&forum=37
//                    // Zerk 1 = 1-25%
//                    // Zerk 2 = 1-50%
//                    // Zerk 3 = 1-75%
//                    // Zerk 4 = 1-99%
//                    critMin = (int)(0.01 * ad.Damage);
//                    critMax = (int)(Math.Min(0.99, (level * 0.25)) * ad.Damage);
//                }
//                else
//                {
//                    //think min crit dmage is 10% of damage
//                    critMin = ad.Damage / 10;
//                    // Critical damage to players is 50%, low limit should be around 20% but not sure
//                    // zerkers in Berserk do up to 99%
//                    if (ad.Target is GamePlayer || ad.Target is MimicNPC)
//                        critMax = ad.Damage >> 1;
//                    else
//                        critMax = ad.Damage;
//                }
//                critMin = Math.Max(critMin, 0);
//                critMax = Math.Max(critMin, critMax);

//                return Util.Random(critMin, critMax);
//            }
//            return 0;
//        }

//        protected static ISpellHandler CreateMagicEffect(GameLiving caster, GameLiving target, int spellID)
//        {
//            SpellLine styleLine = SkillBase.GetSpellLine(GlobalSpellsLines.Combat_Styles_Effect);
//            if (styleLine == null) return null;

//            List<Spell> spells = SkillBase.GetSpellList(styleLine.KeyName);

//            Spell styleSpell = null;
//            foreach (Spell spell in spells)
//            {
//                if (spell.ID == spellID)
//                {
//                    // We have to scale style procs when cast
//                    if (caster is GamePet pet)
//                        pet.ScalePetSpell(spell);

//                    styleSpell = spell;
//                    break;
//                }
//            }

//            ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(caster, styleSpell, styleLine);
//            if (spellHandler == null && styleSpell != null && caster is GamePlayer)
//            {
//                ((GamePlayer)caster).Out.SendMessage(styleSpell.Name + " not implemented yet (" + styleSpell.SpellType + ")", eChatType.CT_System, eChatLoc.CL_SystemWindow);
//            }

//            // No negative effects can be applied on a keep door or via attacking a keep door
//            if ((target is GameKeepComponent || target is GameKeepDoor) && spellHandler.HasPositiveEffect == false)
//            {
//                return null;
//            }

//            return spellHandler;
//        }

//        public CharacterClass CharacterClass { get; protected set; }

//        /// <summary>
//        /// Easy method to get the resist of a certain damage type
//        /// Good for when we add RAs
//        /// </summary>
//        /// <param name="property"></param>
//        /// <returns></returns>
//        public override int GetDamageResist(eProperty property)
//        {
//            int res = 0;
//            int classResist = 0;
//            int secondResist = 0;

//            //Q: Do the Magic resist bonuses from Bedazzling Aura and Empty Mind stack with each other?
//            //A: Nope.
//            switch ((eResist)property)
//            {
//                case eResist.Body:
//                case eResist.Cold:
//                case eResist.Energy:
//                case eResist.Heat:
//                case eResist.Matter:
//                case eResist.Spirit:
//                res += BaseBuffBonusCategory[(int)eProperty.MagicAbsorption];
//                break;

//                default:
//                break;
//            }
//            return (int)((res + classResist) - 0.01 * secondResist * (res + classResist) + secondResist);
//        }

//        /// <summary>
//        /// Get object specialization level based on server type
//        /// </summary>
//        /// <param name="player">player whom specializations are checked</param>
//        /// <param name="objectType">object type</param>
//        /// <returns>specialization in object or 0</returns>
//        public virtual int GetObjectSpecLevel(MimicNPC mimic, eObjectType objectType)
//        {
//            int res = 0;

//            foreach (eObjectType obj in GetCompatibleObjectTypes(objectType))
//            {
//                int spec = mimic.GetModifiedSpecLevel(SkillBase.ObjectTypeToSpec(obj));
//                if (res < spec)
//                    res = spec;
//            }
//            return res;
//        }

//        /// <summary>
//        /// Get object specialization level based on server type
//        /// </summary>
//        /// <param name="mimic">player whom specializations are checked</param>
//        /// <param name="objectType">object type</param>
//        /// <returns>specialization in object or 0</returns>
//        public virtual int GetBaseObjectSpecLevel(MimicNPC mimic, eObjectType objectType)
//        {
//            int res = 0;

//            foreach (eObjectType obj in GetCompatibleObjectTypes(objectType))
//            {
//                int spec = mimic.GetBaseSpecLevel(SkillBase.ObjectTypeToSpec(obj));

//                if (res < spec)
//                    res = spec;
//            }
//            return res;
//        }

//        /// <summary>
//        /// Holds arrays of compatible object types
//        /// </summary>
//        protected Hashtable m_compatibleObjectTypes = null;

//        /// <summary>
//        /// Translates object type to compatible object types based on server type
//        /// </summary>
//        /// <param name="objectType">The object type</param>
//        /// <returns>An array of compatible object types</returns>
//        protected virtual eObjectType[] GetCompatibleObjectTypes(eObjectType objectType)
//        {
//            if (m_compatibleObjectTypes == null)
//            {
//                m_compatibleObjectTypes = new Hashtable();
//                m_compatibleObjectTypes[(int)eObjectType.Staff] = new eObjectType[] { eObjectType.Staff };
//                m_compatibleObjectTypes[(int)eObjectType.Fired] = new eObjectType[] { eObjectType.Fired };

//                m_compatibleObjectTypes[(int)eObjectType.FistWraps] = new eObjectType[] { eObjectType.FistWraps };
//                m_compatibleObjectTypes[(int)eObjectType.MaulerStaff] = new eObjectType[] { eObjectType.MaulerStaff };

//                //alb
//                m_compatibleObjectTypes[(int)eObjectType.CrushingWeapon] = new eObjectType[] { eObjectType.CrushingWeapon, eObjectType.Blunt, eObjectType.Hammer };
//                m_compatibleObjectTypes[(int)eObjectType.SlashingWeapon] = new eObjectType[] { eObjectType.SlashingWeapon, eObjectType.Blades, eObjectType.Sword, eObjectType.Axe };
//                m_compatibleObjectTypes[(int)eObjectType.ThrustWeapon] = new eObjectType[] { eObjectType.ThrustWeapon, eObjectType.Piercing };
//                m_compatibleObjectTypes[(int)eObjectType.TwoHandedWeapon] = new eObjectType[] { eObjectType.TwoHandedWeapon, eObjectType.LargeWeapons };
//                m_compatibleObjectTypes[(int)eObjectType.PolearmWeapon] = new eObjectType[] { eObjectType.PolearmWeapon, eObjectType.CelticSpear, eObjectType.Spear };
//                m_compatibleObjectTypes[(int)eObjectType.Flexible] = new eObjectType[] { eObjectType.Flexible };
//                m_compatibleObjectTypes[(int)eObjectType.Longbow] = new eObjectType[] { eObjectType.Longbow };
//                m_compatibleObjectTypes[(int)eObjectType.Crossbow] = new eObjectType[] { eObjectType.Crossbow };
//                //TODO: case 5: abilityCheck = Abilities.Weapon_Thrown; break;

//                //mid
//                m_compatibleObjectTypes[(int)eObjectType.Hammer] = new eObjectType[] { eObjectType.Hammer, eObjectType.CrushingWeapon, eObjectType.Blunt };
//                m_compatibleObjectTypes[(int)eObjectType.Sword] = new eObjectType[] { eObjectType.Sword, eObjectType.SlashingWeapon, eObjectType.Blades };
//                m_compatibleObjectTypes[(int)eObjectType.LeftAxe] = new eObjectType[] { eObjectType.LeftAxe };
//                m_compatibleObjectTypes[(int)eObjectType.Axe] = new eObjectType[] { eObjectType.Axe, eObjectType.SlashingWeapon, eObjectType.Blades, eObjectType.LeftAxe };
//                m_compatibleObjectTypes[(int)eObjectType.HandToHand] = new eObjectType[] { eObjectType.HandToHand };
//                m_compatibleObjectTypes[(int)eObjectType.Spear] = new eObjectType[] { eObjectType.Spear, eObjectType.CelticSpear, eObjectType.PolearmWeapon };
//                m_compatibleObjectTypes[(int)eObjectType.CompositeBow] = new eObjectType[] { eObjectType.CompositeBow };
//                m_compatibleObjectTypes[(int)eObjectType.Thrown] = new eObjectType[] { eObjectType.Thrown };

//                //hib
//                m_compatibleObjectTypes[(int)eObjectType.Blunt] = new eObjectType[] { eObjectType.Blunt, eObjectType.CrushingWeapon, eObjectType.Hammer };
//                m_compatibleObjectTypes[(int)eObjectType.Blades] = new eObjectType[] { eObjectType.Blades, eObjectType.SlashingWeapon, eObjectType.Sword, eObjectType.Axe };
//                m_compatibleObjectTypes[(int)eObjectType.Piercing] = new eObjectType[] { eObjectType.Piercing, eObjectType.ThrustWeapon };
//                m_compatibleObjectTypes[(int)eObjectType.LargeWeapons] = new eObjectType[] { eObjectType.LargeWeapons, eObjectType.TwoHandedWeapon };
//                m_compatibleObjectTypes[(int)eObjectType.CelticSpear] = new eObjectType[] { eObjectType.CelticSpear, eObjectType.Spear, eObjectType.PolearmWeapon };
//                m_compatibleObjectTypes[(int)eObjectType.Scythe] = new eObjectType[] { eObjectType.Scythe };
//                m_compatibleObjectTypes[(int)eObjectType.RecurvedBow] = new eObjectType[] { eObjectType.RecurvedBow };

//                m_compatibleObjectTypes[(int)eObjectType.Shield] = new eObjectType[] { eObjectType.Shield };
//                m_compatibleObjectTypes[(int)eObjectType.Poison] = new eObjectType[] { eObjectType.Poison };
//                //TODO: case 45: abilityCheck = Abilities.instruments; break;
//            }

//            eObjectType[] res = (eObjectType[])m_compatibleObjectTypes[(int)objectType];
//            if (res == null)
//                return Array.Empty<eObjectType>();
//            return res;
//        }

//        public override bool AddToWorld()
//        {
//            //m_invulnerabilityTick = 0;
//            m_healthRegenerationTimer = new RegionTimer(this);
//            m_powerRegenerationTimer = new RegionTimer(this);
//            m_enduRegenerationTimer = new RegionTimer(this);
//            m_healthRegenerationTimer.Callback = new RegionTimerCallback(HealthRegenerationTimerCallback);
//            m_powerRegenerationTimer.Callback = new RegionTimerCallback(PowerRegenerationTimerCallback);
//            m_enduRegenerationTimer.Callback = new RegionTimerCallback(EnduranceRegenerationTimerCallback);

//            StartHealthRegeneration();
//            StartEnduranceRegeneration();
//            StartPowerRegeneration();

//            return base.AddToWorld();
//        }
//    }
//}