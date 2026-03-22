using System.Text.Json;
using Archetype.Core;

namespace Archetype.Build;

/// <summary>
/// Programmatic build API for Archetype game definition projects (D32).
/// <para>
/// Game developers write their own <c>Program.cs</c> that builds the
/// <see cref="GameDefinition"/>, constructs <see cref="CardSet"/> instances,
/// and calls <see cref="Run"/>.  <c>BuildRunner</c> handles serialization and
/// Godot interop artifact generation.
/// </para>
/// <example>
/// <code>
/// var definition = MyGame.Rules.BuildDefinition();
/// var coreSet    = new MyGame.CoreSet.SetBuilder().Build();
/// BuildRunner.Run(definition, [coreSet], outputDir: "godot/archetype-export/");
/// </code>
/// </example>
/// </summary>
public static class BuildRunner
{
    private static readonly JsonSerializerOptions JsonOptions =
        GameDefinitionJsonOptions.Build();

    /// <summary>
    /// Serializes each <see cref="CardSet"/> to JSON and generates Godot
    /// interop artifacts in <paramref name="outputDir"/>.
    /// </summary>
    /// <param name="definition">
    /// The base <see cref="GameDefinition"/> (rules only; no card definitions
    /// required at this stage — they are supplied via <paramref name="sets"/>).
    /// </param>
    /// <param name="sets">
    /// Card sets to serialize.  Each set is written to
    /// <c>archetype/card-sets/[set-name].json</c> under <paramref name="outputDir"/>.
    /// </param>
    /// <param name="outputDir">
    /// Root output directory (typically your Godot project root).  The runner
    /// creates <c>archetype/</c> for interop files and <c>archetype/card-sets/</c>
    /// for JSON card sets.  Created if it does not exist; existing files are overwritten.
    /// </param>
    /// <param name="noSignalKeywords">
    /// Optional set of game-creator keyword names to exclude from
    /// <c>game_events.gd</c> signal generation.
    /// </param>
    public static void Run(
        GameDefinition definition,
        IEnumerable<CardSet> sets,
        string outputDir,
        IEnumerable<string>? noSignalKeywords = null)
    {
        var archetypeDir = Path.Combine(outputDir, "archetype");
        var cardSetsDir  = Path.Combine(archetypeDir, "card-sets");
        Directory.CreateDirectory(archetypeDir);
        Directory.CreateDirectory(cardSetsDir);

        var setList = sets.ToList();

        // Serialize each card set to JSON.
        foreach (var set in setList)
        {
            var json = JsonSerializer.Serialize(set, JsonOptions);
            File.WriteAllText(Path.Combine(cardSetsDir, $"{set.Name}.json"), json);
        }

        // Merge card sets into definition for signal derivation.
        // (Merges all sets so signal scan sees all defined cards.)
        var fullDefinition = definition.WithCardSets(setList);

        // Serialize the rules-only definition (no cards merged) so GDScript can load
        // it at runtime and then merge the card-set JSONs itself via StartGame.
        // Serializing fullDefinition here would cause duplicate card names at runtime.
        var defJson = JsonSerializer.Serialize(definition, JsonOptions);
        File.WriteAllText(Path.Combine(archetypeDir, "game_definition.json"), defJson);

        // Emit Godot interop files (D32, D33).
        GodotEmitter.EmitKeywordConstants(fullDefinition, archetypeDir);
        GodotEmitter.EmitSignals(fullDefinition, archetypeDir, noSignalKeywords);
        GodotEmitter.EmitArchetypeNode(fullDefinition, archetypeDir, noSignalKeywords);
        GodotEmitter.EmitInteropScripts(fullDefinition, archetypeDir, noSignalKeywords);
    }
}
