using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class GameLoop
    {
        public event DecisionPrompt PromptUser;
        public Timeline Time { get; set; }
        public GameState State { get; set; }

        public GameLoop(GameState initialState, DecisionPrompt handlePrompt)
        {
            State = initialState;
            PromptUser += handlePrompt;
        }

        public void Start()
        {
            Upkeep();
            Turns();
        }

        private void Turns()
        {
            Time.ResolveTurns(State, PromptUser);
        }

        private void Upkeep()
        {
            Time.ResolveEffects(PromptUser);
        }
    }
}
