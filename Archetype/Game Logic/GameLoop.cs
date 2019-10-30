using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class GameLoop
    {
        public Timeline Time { get; set; }
        public GameState State { get; set; }
        
        public GameLoop(GameState initialState)
        {
            State = initialState;
        }

        public void NextLoop()
        {
            Time.ResolveTick(State);
            Time.AdvanceTime();
        }
    }
}
