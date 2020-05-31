
namespace Archetype
{
    public interface ITarget
    {
        void PostActionAsTarget(ActionInfo action);
        void PreActionAsTarget(ActionInfo action);
    }
}
