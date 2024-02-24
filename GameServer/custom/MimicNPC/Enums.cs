using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOL.GS.Scripts
{
    public enum eHand
    {
        None = -1,

        oneHand,
        twoHand,
        leftHand,
    }

    public enum eMimicClass
    {
        None = -1,

        Armsman,
        Cabalist,
        Cleric,
        Friar,
        Infiltrator,
        Mercenary,
        Minstrel,
        Paladin,
        Reaver,
        Scout,
        Sorcerer,
        Theurgist,
        Wizard,

        Bard,
        Blademaster,
        Champion,
        Druid,
        Eldritch,
        Enchanter,
        Hero,
        Mentalist,
        Nightshade,
        Ranger,
        Valewalker,
        Warden,

        Berserker,
        Bonedancer,
        Healer,
        Hunter,
        Runemaster,
        Savage,
        Shadowblade,
        Shaman,
        Skald,
        Spiritmaster,
        Thane,
        Warrior
    }

    public enum eMimicClassesHib
    {
        None = -1,

        Bard,
        Blademaster,
        Champion,
        Druid,
        Eldritch,
        Enchanter,
        Hero,
        Mentalist,
        Nightshade,
        Ranger,
        Valewalker,
        Warden,
    }

    public enum eMimicClassesMid
    {
        None = -1,

        Berserker,
        Bonedancer,
        Healer,
        Hunter,
        Runemaster,
        Savage,
        Shadowblade,
        Shaman,
        Skald,
        Spiritmaster,
        Thane,
        Warrior
    }

    public enum eMimicGroupRole
    {
        None = -1,

        Leader,
        MainAssist,
        MainTank,
        MainCC
    }

    public enum eQueueMessage
    {
        None = -1,

        WaitForBuffs,

        LowPower,
        OutOfPower,

        LowEndurance,
        OutOfEndurance
    }

    public enum eQueueMessageResult
    {
        None = -1,

        Accept,
        Deny,
        OnHold
    }
}