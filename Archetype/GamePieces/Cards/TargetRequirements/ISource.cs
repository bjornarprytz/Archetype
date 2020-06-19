
namespace Archetype
{
    public interface ISource
    {
        void PostActionAsSource(ActionInfo action);
        void PreActionAsSource(ActionInfo action);
    }
}
