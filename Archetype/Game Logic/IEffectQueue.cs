using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public interface IEffectQueue
    {
        void Enqueue(Effect effect);
    }
}
