using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public abstract class ActionPrompt
    {
        public bool Aborted { get; set; }


        public virtual void Abort()
        {
            Aborted = true;
        }
    }
}
