using System.Threading.Tasks;

namespace Archetype.Core
{
    public interface IEffectQueue
    {
        void Enqueue(IEffect effect);
        Task ResolveNextAsync();
    }
}
