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
    /// <c>[set-name].json</c> in <paramref name="outputDir"/>.
    /// </param>
    /// <param name="outputDir">
    /// Directory to write all output files.  Created if it does not exist.
    /// Existing files are overwritten.
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
        Directory.CreateDirectory(outputDir);

        var setList = sets.ToList();

        // Serialize each card set to JSON.
        foreach (var set in setList)
        {
            var json = JsonSerializer.Serialize(set, JsonOptions);
            File.WriteAllText(Path.Combine(outputDir, $"{set.Name}.json"), json);
        }

        // Merge card sets into definition for signal derivation.
        // (Merges all sets so signal scan sees all defined cards.)
        var fullDefinition = definition.WithCardSets(setList);

        // Emit Godot interop files (D32, D33).
        GodotEmitter.EmitKeywordConstants(fullDefinition, outputDir);
        GodotEmitter.EmitSignals(fullDefinition, outputDir, noSignalKeywords);
        GodotEmitter.EmitArchetypeNode(fullDefinition, outputDir, noSignalKeywords);
        GodotEmitter.EmitInteropScripts(fullDefinition, outputDir, noSignalKeywords);
    }
}
