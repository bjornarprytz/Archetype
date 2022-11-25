using Archetype.Core.Atoms;

namespace Archetype.Core.Effects;

public interface ITargetProvider
{
    T GetTarget<T>(int index) where T : IAtom;
}