using Archetype.View.Atoms.MetaData;

namespace Archetype.View.Atoms;

public interface IStructureFront : IUnitFront
{
    StructureMetaData MetaData { get; }
}