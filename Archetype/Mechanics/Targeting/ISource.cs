
using System;
using System.Collections.Generic;

namespace Archetype
{
    public interface ISource
    {
        event EventHandler<ActionInfo> OnSourceOfActionBefore;
        event EventHandler<ActionInfo> OnSourceOfActionAfter;

        void PostActionAsSource(ActionInfo action);
        void PreActionAsSource(ActionInfo action);
    }
}
