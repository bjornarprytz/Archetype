
namespace Archetype
{
    public interface ITarget : ITriggerHost, IHoldCounters
    {
        void PostActionAsTarget(ActionInfo action);
        void PreActionAsTarget(ActionInfo action);
    }
}
