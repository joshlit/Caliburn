// GPL v3 License
// 
// Copyright (c) 2016-2017 Bismur Studios Ltd.
// Copyright (c) 2016-2017 Ioannis Giagkiozis
// 
// Character.cs is part of Crystal AI.
//  
// Crystal AI is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// Crystal AI is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Crystal AI.  If not, see <http://www.gnu.org/licenses/>.
using System;
using Crystal;


namespace ExampleAI {

  public class Character : IContextProvider {
    CharacterContext _context;
    string _lastAction;

    public string Name { get; }

    public IContext Context() {
      return _context;
    }

    public void Update() {
      // Do something with the context.
      _context.Energy -= 0.35f;
      _context.Hunger += 0.4f;
      _context.Thirst += 0.5f;
      _context.Bladder += 0.5f;
      _context.Cleanliness -= 0.3f;
      _context.Fitness -= 0.5f;
    }

    public void Report(string lastAction) {
      _lastAction = lastAction;
    }
    
    public Character(string name) {
      Name = name;
      _context = new CharacterContext(this);
    }

    public override string ToString() {
      return string.Format("[{0,-8}, {1} - Action: {2,15}]", Name, _context, _lastAction);
    }
  }

}