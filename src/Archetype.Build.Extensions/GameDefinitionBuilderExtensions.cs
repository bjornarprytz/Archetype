using Archetype.Build;
using Archetype.Core;

namespace Archetype.Build.Extensions;

/// <summary>
/// <c>Action&lt;Builder&gt;</c> convenience overloads for <see cref="GameDefinitionBuilder"/>.
/// </summary>
public static class GameDefinitionBuilderExtensions
{
    /// <summary>Registers a card definition via a <see cref="CardDefinitionBuilder"/> callback.</summary>
    public static GameDefinitionBuilder AddCard(
        this GameDefinitionBuilder builder,
        string name,
        Action<CardDefinitionBuilder> configure)
    {
        var b = new CardDefinitionBuilder(name);
        configure(b);
        return builder.AddCard(b.Build());
    }

    /// <summary>Registers a zone definition via a <see cref="ZoneDefinitionBuilder"/> callback.</summary>
    public static GameDefinitionBuilder AddZone(
        this GameDefinitionBuilder builder,
        string name,
        Action<ZoneDefinitionBuilder> configure)
    {
        var b = new ZoneDefinitionBuilder(name);
        configure(b);
        var zone = b.Build();
        return builder.AddZone(zone);
    }

    /// <summary>Registers a player definition via a <see cref="PlayerDefinitionBuilder"/> callback.</summary>
    public static GameDefinitionBuilder AddPlayer(
        this GameDefinitionBuilder builder,
        string name,
        Action<PlayerDefinitionBuilder> configure)
    {
        var b = new PlayerDefinitionBuilder();
        configure(b);
        return builder.AddPlayer(name, b.Build());
    }

    /// <summary>Appends a phase via a <see cref="PhaseDefinitionBuilder"/> callback.</summary>
    public static GameDefinitionBuilder AddPhase(
        this GameDefinitionBuilder builder,
        string name,
        Action<PhaseDefinitionBuilder> configure)
    {
        var b = new PhaseDefinitionBuilder(name);
        configure(b);
        return builder.AddPhase(b.Build());
    }

    /// <summary>Registers a keyword via a <see cref="KeywordDefinitionBuilder"/> callback.</summary>
    public static GameDefinitionBuilder RegisterKeyword(
        this GameDefinitionBuilder builder,
        string name,
        Action<KeywordDefinitionBuilder> configure)
    {
        var b = new KeywordDefinitionBuilder(name);
        configure(b);
        return builder.RegisterKeyword(b.Build());
    }
}
