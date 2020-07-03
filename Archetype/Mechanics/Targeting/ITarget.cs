using System;

namespace Archetype
{
    public interface ITarget
    {
        event EventHandler<ActionInfo> OnTargetOfActionBefore;
        event EventHandler<ActionInfo> OnTargetOfActionAfter;

        void PostActionAsTarget(ActionInfo action);
        void PreActionAsTarget(ActionInfo action);
    }
}
