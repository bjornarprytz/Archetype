using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class ActionQueue : IActionQueue
    {
        private Queue<ActionInfo> _actions;

        public ActionQueue()
        {
            _actions = new Queue<ActionInfo>();

        }

        public void EnqueueAction(ActionInfo action)
        {
            _actions.Enqueue(action);
        }

        public void ResolveAll()
        {
            while (_actions.Any())
            {
                _actions.Dequeue().Execute();
            }
        }

        public void ResolveNext()
        {
            if (_actions.Any()) _actions.Dequeue().Execute();
        }
    }
}
