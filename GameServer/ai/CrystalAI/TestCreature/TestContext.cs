using Crystal;

namespace DOL.GS;

public class TestContext : IContext
{
    public string Name;
    public int DISTANCE_TO_CHECK = 1000;
    
    public GameLiving Owner;
    
    float _hunger;
    public float Hunger { 
        get { return _hunger; } 
        set { _hunger = value.Clamp(0f, 100f); } 
    }
    float _thirst;
    public float Thirst { 
        get { return _thirst; } 
        set { _thirst = value.Clamp(0f, 100f); } 
    }

    private float _minDistance;
    public float MinDistance
    {
        get { return _minDistance; }
        set { _minDistance = value.Clamp(0f, 100f); }
    }

    public GameLiving NearestLiving;
    
    string _lastAction;
    public void Report(string what) {
        _lastAction = what;
    }

    public TestContext(GameLiving owner) {
        // Just assign some random starting values to mix things up.
        _minDistance = 0f;
        Owner = owner;
    }
}