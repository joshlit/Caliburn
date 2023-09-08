using DOL.AI;
using DOL.AI.Brain;
using DOL.GS;

public class Companion : GameNPC
{
    // Store the info we need from the summoning spell to calculate pet level.
    public double SummonSpellDamage { get; set; } = -88.0;
    public double SummonSpellValue { get; set; } = 44.0;
    public Companion(INpcTemplate template) : base(template)
    {
    }

    public Companion(ABrain brain) : base(brain)
    {
    }
    
    public virtual bool SetPetLevel()
    {
        // Changing Level calls additional code, so only do it at the end
        byte newLevel = 0;

        if (SummonSpellDamage >= 0)
            newLevel = (byte)SummonSpellDamage;
        else if (!(Owner is GameSummonedPet))
            newLevel = (byte)((Owner?.Level ?? 0) * SummonSpellDamage * -0.01);
        else if (RootOwner is GameLiving summoner)
            newLevel = (byte)(summoner?.Level * SummonSpellDamage * -0.01);

        if (SummonSpellValue > 0  && newLevel > SummonSpellValue)
            newLevel = (byte)SummonSpellValue;

        if (newLevel < 1)
            newLevel = 1;

        if (Level == newLevel)
            return false;

        Level = newLevel;
        return true;
    }

    public GameLiving Owner;

    /// <summary>
    /// The root owner of this pet, the person at the top of the owner chain.
    /// </summary>
    public GameLiving RootOwner;

    public override void LoadTemplate(INpcTemplate template)
    {
        base.LoadTemplate(template);
        m_ownBrain = new CrystalBrain(this);
    }
}