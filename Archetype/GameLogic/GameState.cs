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

        public EventHandler<Choose> OnChoose { get; private set; }

        public Queue<ActionInfo> EffectQueue { get; private set; }

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

        public GameState(EventHandler<Choose> choiceHandler)
        {
            Battlefield = new Battlefield();
            EffectQueue = new Queue<ActionInfo>();

            if (choiceHandler == null) throw new Exception("Please provide an event handler for choices");

            OnChoose = choiceHandler;
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

        public void TakeAction(IGameAction action)
        {
            if (!action.CanExecute(this)) return;

            action.Execute(this);
        }

        public void Choose<T>(Choose<T> actionPrompt) where T : GamePiece
        {
            OnChoose?.Invoke(this, actionPrompt);
        }
        public void Enqueue(ActionInfo action)
        {
            EffectQueue.Enqueue(action);
        }
    }
}
