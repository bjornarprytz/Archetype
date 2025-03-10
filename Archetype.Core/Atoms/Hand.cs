﻿using Archetype.Core.Atoms.Base;
using Archetype.View.Atoms;
using Archetype.View.Atoms.Zones;

namespace Archetype.Core.Atoms;

public interface IHand  : IZone<ICard>, IHandFront { }

internal class Hand : Zone<ICard>, IHand
{
    public Hand(IGameAtom owner) : base(owner) { }
    public IEnumerable<ICardFront> Cards => Contents;
}