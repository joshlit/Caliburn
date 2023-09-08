using System;
using Crystal;
using DOL.GS;


public class GameLoopDecisionMaker : DecisionMakerBase
{
    private static int placeholderInt = 0;
    public static GameLoopDecisionMaker CreateDecisionMaker(GameLiving owner)
    {
        var testGuy = new TestGuy($"Test{++placeholderInt}", owner);
        var testBrain = new TestGuyConstructor().Create($"TestGuyAI");
        Console.WriteLine($"TestGuy: {testGuy} | TestBrain: {testBrain} | Body Name {owner?.Name}");
        return new GameLoopDecisionMaker(testBrain, testGuy);
    }
    
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
