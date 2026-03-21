using Archetype.Build;
using Archetype.Core;

// ── Output directory (next to this project) ─────────────────────────────────
var outputDir = Path.Combine(AppContext.BaseDirectory, "export");

// ── Game rules ───────────────────────────────────────────────────────────────
//
// Two game-creator keywords:
//   strike(target, power)  — deal power damage to target
//   heal(target, amount)   — restore amount health to target
//
var definition = new GameDefinitionBuilder()
    .WithId("sample-game")
    .RegisterKeyword(
        name: "strike",
        parameters:
        [
            new ParameterDecl("target", TypeName.Card),
            new ParameterDecl("power",  TypeName.Number),
        ],
        body: Kw.ModifyAccumulator(
            Kw.Param("target"),
            Kw.Str("health"),
            Kw.Multiply(Kw.Param("power"), Kw.Num(-1))),
        textTemplate: "Deal {power} damage to {target}")
    .RegisterKeyword(
        name: "heal",
        parameters:
        [
            new ParameterDecl("target", TypeName.Card),
            new ParameterDecl("amount", TypeName.Number),
        ],
        body: Kw.ModifyAccumulator(
            Kw.Param("target"),
            Kw.Str("health"),
            Kw.Param("amount")),
        textTemplate: "Restore {amount} health to {target}")
    .WithInitManifest(InitManifest.Empty)
    .Build();

// ── Core card set ─────────────────────────────────────────────────────────────
//
// Swordsman — deals 3 damage
// Cleric    — heals 2 health
//
var coreSet = new CardSet("core", 1,
[
    new CardDefinition(
        Name: "Swordsman",
        StaticProperties: new Dictionary<string, object>
        {
            ["health"] = 5,
            ["cost"]   = 2,
        },
        PrimaryEffect: new EffectBlockDef(
        [
            new EffectBlockStep("strike",
                [Kw.Param("source"), Kw.Num(3)]),
        ]),
        AdditionalEffects: [],
        StaticEffects: []),

    new CardDefinition(
        Name: "Cleric",
        StaticProperties: new Dictionary<string, object>
        {
            ["health"] = 4,
            ["cost"]   = 2,
        },
        PrimaryEffect: new EffectBlockDef(
        [
            new EffectBlockStep("heal",
                [Kw.Param("source"), Kw.Num(2)]),
        ]),
        AdditionalEffects: [],
        StaticEffects: []),
]);

// ── Build ─────────────────────────────────────────────────────────────────────
BuildRunner.Run(definition, [coreSet], outputDir);

Console.WriteLine($"Export written to: {outputDir}");
foreach (var file in Directory.GetFiles(outputDir))
    Console.WriteLine($"  {Path.GetFileName(file)}");
