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

        private void Upkeep()
        {
            Timeline.ResolveEffects(PromptUser);
        }

        private void EndTick() // TODO: Make this trigger by command instead
        {
            Timeline.AdvanceTime();
        }
    }
}
