using System;
using System.Collections.Generic;
using Archetype.Core;

namespace Archetype.Build;

/// <summary>
/// Minimal AtomGroup implementation for build-time transformations.
/// Supports card-level matching and transformation via delegates.
/// </summary>
public sealed class AtomGroup
{
    public string Name { get; }
    public IReadOnlyList<AtomKind> Kinds { get; }
    public int Priority { get; }
    public bool OverrideLocal { get; }

    private readonly Func<string, CardDefinition, bool>? _cardMatcher;
    private readonly Func<CardDefinition, CardDefinition>? _cardTransform;

    public AtomGroup(string name, IReadOnlyList<AtomKind> kinds,
        Func<string, CardDefinition, bool>? cardMatcher = null,
        Func<CardDefinition, CardDefinition>? cardTransform = null,
        int priority = 0,
        bool overrideLocal = false)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Kinds = kinds ?? Array.Empty<AtomKind>();
        _cardMatcher = cardMatcher;
        _cardTransform = cardTransform;
        Priority = priority;
        OverrideLocal = overrideLocal;
    }

    public bool MatchesCard(string name, CardDefinition card) => _cardMatcher?.Invoke(name, card) ?? false;
    public CardDefinition TransformCard(CardDefinition card) => _cardTransform?.Invoke(card) ?? card;
}