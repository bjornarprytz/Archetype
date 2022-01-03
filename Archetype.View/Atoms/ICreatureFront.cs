using Archetype.View.Atoms.MetaData;

namespace Archetype.View.Atoms;

public interface ICreatureFront : IUnitFront
{
    CreatureMetaData MetaData { get; }
    int Strength { get; }
    int Movement { get; }
}