using Aqua.TypeExtensions;
using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Base;
using Archetype.Core.Exceptions;
using Archetype.Core.Extensions;
using Archetype.View.Infrastructure;

namespace Archetype.Core.Play;

public interface ITargetProvider
{
    T GetTarget<T>() where T : IGameAtom;
    T GetTarget<T>(int index) where T : IGameAtom;
}

