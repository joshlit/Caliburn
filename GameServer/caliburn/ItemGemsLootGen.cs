using DOL.AI.Brain;
using System;
using System.Collections.Generic;
using System.Linq;
using DOL.Database;

namespace DOL.GS {

    public class ItemGemsLootGen : LootGeneratorBase {



        /// <summary>
        /// Generate loot for given mob
        /// </summary>
        /// <param name="mob"></param>
        /// <param name="killer"></param>
        /// <returns></returns>
        public override LootList GenerateLoot(GameNPC mob, GameObject killer)
        {
            LootList loot = base.GenerateLoot(mob, killer);

            try
            {
                GamePlayer player = killer as GamePlayer;
                if (killer is GameNPC && ((GameNPC)killer).Brain is IControlledBrain)
                {
                    player = ((ControlledNpcBrain)((GameNPC)killer).Brain).GetPlayerOwner();
                }

                if (player == null)
                {
                    return loot;
                }

                int killedcon = (int)player.GetConLevel(mob); 

                //grey con dont drop loot
                if (killedcon <= -3)
                {
                    return loot;
                }
                if (Util.Chance(80)) return loot;

                ItemUnique tmp = new ItemUnique();
                tmp.Name = "Stat Gem";
                tmp.Model = 117;
                tmp.Object_Type = 200;
                tmp.Item_Type = 0;
                tmp.Realm = (int)killer.Realm;
                tmp.Price = 1;
                tmp.Level = mob.Level;
                tmp.ClassType = "DOL.GS.ItemStatGem";
                if (Util.Chance(33))
                {
                    tmp.Name = "Resist Gem";
                    int diff = (int)eProperty.Resist_Last - (int)eProperty.Resist_First;
                    int stat = (int)eProperty.Resist_First + Util.Random(0, diff - 1);
                    tmp.Bonus1Type = stat;

                    int min = mob.Level / 12;
                    int max = min * 4;
                    tmp.Bonus1 = Util.Random(min, max);
                }
                else
                {
                    int diff = (int)eProperty.Stat_Last - (int)eProperty.Stat_First;
                    int stat = (int)eProperty.Stat_First + Util.Random(0, diff - 1);
                    tmp.Bonus1Type = stat;

                    int min = mob.Level / 10;
                    int max = min * 5;
                    tmp.Bonus1 = Util.Random(min, max);
                }

                tmp.MaxCount = 1;
                loot.AddFixed(tmp, 1);



            }
            catch
            {
                return loot;
            }

            return loot;
        }


        private ItemTemplate GenerateItemTemplate(GamePlayer player, eCharacterClass classForLoot, byte lootLevel, int killedcon)
        {
            ItemTemplate item = null;
                
                
            GeneratedUniqueItem tmp = AtlasROGManager.GenerateMonsterLootROG(GetRealmFromClass(classForLoot), classForLoot, lootLevel, player.CurrentZone?.IsOF ?? false);
            tmp.GenerateItemQuality(killedcon);
            //tmp.CapUtility(mob.Level + 1);
            item = tmp;
            item.MaxCount = 1;

            return item;
        }
        
        private eRealm GetRealmFromClass(eCharacterClass charClass){
            switch (charClass)
            {
                 case eCharacterClass.Armsman:
                 case eCharacterClass.Paladin:
                 case eCharacterClass.Mercenary:
                 case eCharacterClass.Reaver:
                 case eCharacterClass.Cleric:
                 case eCharacterClass.Friar:
                 case eCharacterClass.Infiltrator:
                 case eCharacterClass.Minstrel:
                 case eCharacterClass.Scout:
                 case eCharacterClass.Cabalist:
                 case eCharacterClass.Sorcerer:
                 case eCharacterClass.Theurgist:
                 case eCharacterClass.Wizard:
                 case eCharacterClass.Necromancer:
                     return eRealm.Albion;
                 case eCharacterClass.Bard:
                 case eCharacterClass.Druid:
                 case eCharacterClass.Warden:
                 case eCharacterClass.Blademaster:
                 case eCharacterClass.Hero:
                 case eCharacterClass.Champion:
                 case eCharacterClass.Eldritch:
                 case eCharacterClass.Enchanter:
                 case eCharacterClass.Mentalist:
                 case eCharacterClass.Nightshade:
                 case eCharacterClass.Ranger:
                 case eCharacterClass.Animist:
                 case eCharacterClass.Valewalker:
                     return eRealm.Hibernia;
                 default:
                     return eRealm.Midgard;
            }
        }

        private eCharacterClass GetRandomClassFromGroup(Group group)
        {
            List<eCharacterClass> validClasses = new List<eCharacterClass>();

            foreach (GamePlayer player in group.GetMembersInTheGroup())
            {
                validClasses.Add((eCharacterClass)player.CharacterClass.ID);
            }
            eCharacterClass ranClass = validClasses[Util.Random(validClasses.Count - 1)];

            return ranClass;
        }
        
        private eCharacterClass GetRandomClassFromBattlegroup(BattleGroup battlegroup)
        {
            List<eCharacterClass> validClasses = new List<eCharacterClass>();

            foreach (GamePlayer player in battlegroup.Members.Keys)
            {
                validClasses.Add((eCharacterClass)player.CharacterClass.ID);
            }
            eCharacterClass ranClass = validClasses[Util.Random(validClasses.Count - 1)];

            return ranClass;
        }

        private eCharacterClass GetRandomClassFromRealm(eRealm realm)
        {
            List<eCharacterClass> classesForRealm = new List<eCharacterClass>();
            switch (realm)
            {
                case eRealm.Albion:
                    classesForRealm.Add(eCharacterClass.Armsman);
                    classesForRealm.Add(eCharacterClass.Cabalist);
                    classesForRealm.Add(eCharacterClass.Cleric);
                    classesForRealm.Add(eCharacterClass.Friar);
                    classesForRealm.Add(eCharacterClass.Infiltrator);
                    classesForRealm.Add(eCharacterClass.Mercenary);
                    classesForRealm.Add(eCharacterClass.Necromancer);
                    classesForRealm.Add(eCharacterClass.Paladin);
                    classesForRealm.Add(eCharacterClass.Reaver);
                    classesForRealm.Add(eCharacterClass.Scout);
                    classesForRealm.Add(eCharacterClass.Sorcerer);
                    classesForRealm.Add(eCharacterClass.Theurgist);
                    classesForRealm.Add(eCharacterClass.Wizard);
                    break;
                case eRealm.Midgard:
                    classesForRealm.Add(eCharacterClass.Berserker);
                    classesForRealm.Add(eCharacterClass.Bonedancer);
                    classesForRealm.Add(eCharacterClass.Healer);
                    classesForRealm.Add(eCharacterClass.Hunter);
                    classesForRealm.Add(eCharacterClass.Runemaster);
                    classesForRealm.Add(eCharacterClass.Savage);
                    classesForRealm.Add(eCharacterClass.Shadowblade);
                    classesForRealm.Add(eCharacterClass.Skald);
                    classesForRealm.Add(eCharacterClass.Spiritmaster);
                    classesForRealm.Add(eCharacterClass.Thane);
                    classesForRealm.Add(eCharacterClass.Warrior);
                    break;
                case eRealm.Hibernia:
                    classesForRealm.Add(eCharacterClass.Animist);
                    classesForRealm.Add(eCharacterClass.Bard);
                    classesForRealm.Add(eCharacterClass.Blademaster);
                    classesForRealm.Add(eCharacterClass.Champion);
                    classesForRealm.Add(eCharacterClass.Druid);
                    classesForRealm.Add(eCharacterClass.Eldritch);
                    classesForRealm.Add(eCharacterClass.Enchanter);
                    classesForRealm.Add(eCharacterClass.Hero);
                    classesForRealm.Add(eCharacterClass.Mentalist);
                    classesForRealm.Add(eCharacterClass.Nightshade);
                    classesForRealm.Add(eCharacterClass.Ranger);
                    classesForRealm.Add(eCharacterClass.Valewalker);
                    classesForRealm.Add(eCharacterClass.Warden);
                    break;
            }

            return classesForRealm[Util.Random(classesForRealm.Count - 1)];
        }
    }
}