﻿using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;

namespace Archetype.Framework.State;

public interface IPlayer : IAtom
{
    IOrderedZone Deck { get; }
    IZone Hand { get; }
}

public class Player : Atom, IPlayer
{
    public override IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; } = 
        Declare.Characteristics(
            ("TYPE", "player")
        );

    public IOrderedZone Deck { get; }
    public IZone Hand { get; }
}