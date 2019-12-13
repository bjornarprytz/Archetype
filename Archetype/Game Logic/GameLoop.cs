using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class GameLoop
    {
        public event RequiredAction PromptUser;
        public Timeline Timeline { get; set; }
        public GameState State { get; set; }

        public GameLoop(GameState initialState, RequiredAction handlePrompt)
        {
            State = initialState;
            PromptUser = handlePrompt;
        }

        internal void EndTurn(Unit unit)
        {
            unit.EndTurn();
        }

        private void Upkeep()
        {
            Timeline.ResolveEffects(PromptUser);
        }
    }
}
