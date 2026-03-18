using Archetype.Core;
using Archetype.Engine;

namespace Archetype.Build;

/// <summary>
/// Extension methods for <see cref="GameSessionBuilder"/> that require types
/// defined in <c>Archetype.Build</c> (e.g. <see cref="HostManifestBuilder"/>).
/// These cannot live in <c>Archetype.Engine</c> because that project does not
/// reference <c>Archetype.Build</c> (D15 module boundary).
/// </summary>
public static class GameSessionBuilderExtensions
{
    /// <summary>
    /// Fluent overload of <see cref="GameSessionBuilder.WithHostManifest(HostManifest)"/>
    /// using a builder callback.
    /// </summary>
    public static GameSessionBuilder WithHostManifest(
        this GameSessionBuilder builder,
        Action<HostManifestBuilder> configure)
    {
        var hb = new HostManifestBuilder();
        configure(hb);
        return builder.WithHostManifest(hb.Build());
    }
}
