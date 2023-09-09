using Crystal;

namespace DOL.GS;

public class Warrior : IContextProvider
{
    // All information related to our toon is stored here. 
    // This need not necessarily be the case in your implementation. 
    // A rule of thumb is for the context to be given access only to
    // information that is required by the AI to make decisions. 
    // More on that later. 
    WarriorContext _context;

    // IContextProvider implementation
    public IContext Context() {
        return _context;
    }

    public Warrior(string name, GameLiving owner) {
        _context = new WarriorContext(owner);
        _context.Name = name;
    }
}