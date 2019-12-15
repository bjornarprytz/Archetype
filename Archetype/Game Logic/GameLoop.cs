using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    public class GameLoop
    {
        public event RequiredAction PromptUser;
        public Timeline Timeline { get; private set; }
        private IEnumerator<Unit> TurnOrder { get; set; }
        public GameState State { get; private set; }

        public GameLoop(GameState initialState, RequiredAction handlePrompt)
        {
            State = initialState;
            PromptUser = handlePrompt;
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
            Timeline.AdvanceTime();
            TurnOrder = State.ActiveUnits.Where(u => u.HasTurn(Timeline.CurrentTick)).GetEnumerator();
            Upkeep();
            if (TurnOrder.Current == null) EndTick(); // TODO: Avoid getting stuck in endless recursion 
        }

        private void Upkeep()
        {
            Timeline.ResolveEffects(PromptUser);
        }
    }
}
