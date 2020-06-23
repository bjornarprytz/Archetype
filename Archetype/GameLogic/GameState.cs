using System;
using System.Collections.Generic;

namespace Archetype
{
    public class GameState
    {
        public Battlefield Battlefield { get; private set; }
        public Graveyard Graveyard { get; private set; }

        public IActionQueue ActionQueue { get; private set; }

        public Unit ActiveUnit { get; private set; }

        public void StartTurn(Unit unit)
        {
            ActiveUnit = unit;
        }
        public void EndTurn(Unit unit)
        {
            ActiveUnit = null;
        }
        public bool HasTurn(Unit unit)
        {
            return ActiveUnit == unit;
        }

        public GameState()
        {
            Battlefield = new Battlefield();
            Graveyard = new Graveyard();

            ActionQueue = new ActionQueue();
        }

        public void AddUnits(IEnumerable<Unit> units)
        {
            Battlefield.Add(units);
        }

        public void Update()
        {
            ActionQueue.ResolveAll();
        }

        public void TakeAction(IGameAction action)
        {
            if (!action.CanExecute(this)) return;

            action.Execute(this);
        }
    }
}
