using DOL.AI.Brain;
using System;
using System.Collections.Generic;
using System.Linq;
using DOL.Database;
using DOL.Events;

namespace DOL.GS {

    public class ItemGemsLootGen : LootGeneratorBase {


        public static List<int> ProcsChargesSpellIds = new List<int>();

        [ScriptLoadedEvent]
        public static async void OnScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            //WhereClause realmFilter = DB.Column("Realm").IsEqualTo((byte)playerRealm).Or(DB.Column("Realm").IsEqualTo(0)).Or(DB.Column("Realm").IsNull());
            SpellLine line = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
            List<DbLineXSpell> procsChargesLineXSpells = GameServer.Database.SelectObjects<DbLineXSpell>(DB.Column("LineName").IsEqualTo("Item Effects")).ToList();
            ProcsChargesSpellIds = procsChargesLineXSpells.Select(a => a.SpellID).Where(SpellID =>
            {

                Spell procSpell = SkillBase.GetSpellByID(SpellID);
                return procSpell != null && procSpell.SpellType.ToString().Contains("Summon") == false;

            }).ToList();

        }
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
                    player = ((ControlledMobBrain)((GameNPC)killer).Brain).GetPlayerOwner();
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

                DbItemUnique tmp = new DbItemUnique();
                tmp.Name = "Stat Gem";
                tmp.Model = 117;
                tmp.Object_Type = 200;
                tmp.Item_Type = 0;
                tmp.Realm = (int)killer.Realm;
                tmp.Price = 1;
                tmp.Level = mob.Level;
                tmp.ClassType = "DOL.GS.ItemStatGem";
                if (Util.Chance(25))
                {
                    tmp.Name = "Resist Gem";    
                    int diff = (int)eProperty.Resist_Last - (int)eProperty.Resist_First;
                    int stat = (int)eProperty.Resist_First + Util.Random(0, diff - 1);
                    tmp.Bonus1Type = stat;

                    int min = mob.Level / 12;
                    int max = min * 4;
                    tmp.Bonus1 = Util.Random(min, max);
                }
                else if (Util.Chance(10) && mob.Level >= 40)
                {
                    tmp.Name = "Proc Gem";
                    tmp.ProcSpellID = ProcsChargesSpellIds.OrderBy(a => System.Guid.NewGuid()).FirstOrDefault();
                    tmp.ClassType = "DOL.GS.ItemProcGem";
                }
                else if (Util.Chance(10) && mob.Level >= 40)
                {
                    tmp.Name = "Charge Gem";
                    tmp.SpellID = ProcsChargesSpellIds.OrderBy(a => System.Guid.NewGuid()).FirstOrDefault();
                    tmp.ClassType = "DOL.GS.ItemChargeGem";
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
    }
}