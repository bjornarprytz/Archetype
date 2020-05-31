using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    public class GameState : IPromptable, IActionQueue
    {
        public Battlefield Battlefield { get; private set; }
        public Graveyard Graveyard { get; private set; }


        public Queue<ActionPrompt> PromptQueue { get; private set; }
        public Queue<ActionInfo> EffectQueue { get; private set; }

        internal void EndTurn(Unit unit)
        {
            throw new NotImplementedException();
        }
        internal bool HasTurn(Unit unit)
        {
            throw new NotImplementedException();
        }

        public GameState()
        {
            Battlefield = new Battlefield();
            PromptQueue = new Queue<ActionPrompt>();
            EffectQueue = new Queue<ActionInfo>();
        }

        public void AddUnits(IEnumerable<Unit> units)
        {
            Battlefield.Add(units);
        }

        public void Update()
        {
            // Progress game loop here

            while (EffectQueue.Any())
            {
                EffectQueue.Dequeue().Execute();

            }
        }

        public void Prompt(ActionPrompt actionPrompt)
        {
            PromptQueue.Enqueue(actionPrompt);
            // TODO: event?
        }

        public PromptResponse PromptImmediate(ActionPrompt actionPrompt)
        {
            // TODO: call an event handler?
            return actionPrompt.Abort();
        }
        public void Enqueue(ActionInfo action)
        {
            EffectQueue.Enqueue(action);
        }
    }
}
