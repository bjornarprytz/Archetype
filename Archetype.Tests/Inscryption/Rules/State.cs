using Archetype.Framework.Runtime.State;
using Archetype.Tests.Inscryption.Cards;

namespace Archetype.Tests.Rules.Inscryption;

public interface ILane : INode
{
    ICard? HomeCritter { get; }
    ICard? AwayCritter { get; }
    ICard? StagingCritter { get; }
}

public interface IBone : IAtom { }

public interface IInscryptionPlayer : IAtom // TODO: Remember to add bones to the State in the implementation
{
    int MyTeeth { get; set; }
    int TheirTeeth { get; set; }
}