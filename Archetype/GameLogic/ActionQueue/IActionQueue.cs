using System.Collections.Generic;

namespace Archetype
{
    public interface IActionQueue
    {
        void EnqueueAction(ActionInfo action);
        void EnqueueActions(IEnumerable<ActionInfo> actions);
        void ResolveAll();
        void ResolveNext();
    }
}
