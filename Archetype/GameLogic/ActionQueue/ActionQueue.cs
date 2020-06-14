using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class ActionQueue : IActionQueue
    {
        private Queue<ActionInfo> _queue;

        public ActionQueue()
        {
            _queue = new Queue<ActionInfo>();
        }

        public void Enqueue(ActionInfo action)
        {
            _queue.Enqueue(action);
        }

        public void ResolveAll()
        {
            while (_queue.Any())
            {
                _queue.Dequeue().Execute();
            }
        }

        public void ResolveNext()
        {
            if (_queue.Any()) _queue.Dequeue().Execute();
        }
    }
}
