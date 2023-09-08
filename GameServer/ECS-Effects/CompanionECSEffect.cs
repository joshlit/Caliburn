namespace DOL.GS
{
    public class CompanionECSGameEffect : ECSGameSpellEffect
    {
        public CompanionECSGameEffect(ECSGameEffectInitParams initParams) : base(initParams) { }

        public override void OnStopEffect()
        {
            Caster.UpdatePetCount(false);
            Owner.Health = 0; // to send proper remove packet
            Owner.Delete();
        }
    }
}
