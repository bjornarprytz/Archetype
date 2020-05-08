using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    public class GameState : IPromptable
    {
        private IEnumerator<Unit> TurnOrder { get; set; }
        public Battlefield Battlefield { get; private set; }
        public Graveyard Graveyard { get; private set; }

        public Queue<ActionPrompt> PromptQueue { get; private set; }

        public GameState(IEnumerable<Unit> units)
        {
            Battlefield = new Battlefield(units);
            PromptQueue = new Queue<ActionPrompt>();
        }

        internal bool HasTurn(Unit unit)
        {
            return TurnOrder.Current != null && TurnOrder.Current == unit;
        }

        internal void EndTurn(Unit unit)
        {
            unit.EndTurn();
            if (!TurnOrder.MoveNext()) EndTick();
        }

        internal void EndTick()
        {
            // TODO: Determine turn order.
            Upkeep();
            if (TurnOrder.Current == null) EndTick(); // TODO: Avoid getting stuck in endless recursion
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

        private void Upkeep()
        {
            // TODO: Anything here?
        }

        
    }
}
