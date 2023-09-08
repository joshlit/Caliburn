using System;
using Crystal;
using DOL.GS;


public class GameLoopDecisionMaker : DecisionMakerBase
{
    public GameLoopDecisionMaker(IUtilityAi ai, IContextProvider contextProvider) : base(ai, contextProvider)
    {
    }

    protected override void OnStart()
    {
        Console.WriteLine($"GameLoopDecisionMaker started");
    }

    protected override void OnStop()
    {
        Console.WriteLine($"GameLoopDecisionMaker stopped");
    }

    protected override void OnPause()
    {
        Console.WriteLine($"GameLoopDecisionMaker paused");
    }

    protected override void OnResume()
    {
        Console.WriteLine($"GameLoopDecisionMaker resumed");
    }
}
