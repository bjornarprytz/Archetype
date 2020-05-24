using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public interface IActionQueue
    {
        void Enqueue(ActionInfo action);
    }
}
