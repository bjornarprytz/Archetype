# Archetype

A card game engine where a single card definition drives both rules execution and human-readable card text.

## Using Archetype in a Godot Project

### 1. Add NuGet packages

In your Godot project's `.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="Archetype.Core"   Version="0.1.0" />
  <PackageReference Include="Archetype.Engine" Version="0.1.0" />
  <PackageReference Include="Archetype.Text"   Version="0.1.0" />
  <PackageReference Include="Archetype.Build"  Version="0.1.0" />
</ItemGroup>
```

### 2. Define your game

Create a standalone C# project (or a script inside your Godot project) that defines your game and generates the Godot integration files:

```csharp
using Archetype.Build;
using Archetype.Core;

var definition = new GameDefinitionBuilder()
    .WithId("my-game")
    .RegisterKeyword(
        name: "strike",
        parameters: [
            new ParameterDecl("target", TypeName.Card),
            new ParameterDecl("power",  TypeName.Number),
        ],
        body: Kw.ModifyAccumulator(Kw.Param("target"), Kw.Str("health"), Kw.Multiply(Kw.Param("power"), Kw.Num(-1))),
        textTemplate: "Deal {power} damage to {target}")
    .WithInitManifest(InitManifest.Empty)
    .Build();

var cardSet = new CardSet("core", 1, [
    new CardDefinition(
        Name: "Swordsman",
        StaticProperties: new Dictionary<string, object> { ["health"] = 5, ["cost"] = 2 },
        PrimaryEffect: new EffectBlockDef([new EffectBlockStep("strike", [Kw.Param("source"), Kw.Num(3)])]),
        AdditionalEffects: [],
        StaticEffects: []),
]);

// Point outputDir at your Godot project folder
BuildRunner.Run(definition, [cardSet], outputDir: "path/to/godot/project");
```

Run this once whenever your game definition or card sets change. It is not called at runtime.

### 3. Review the generated files

`BuildRunner.Run` writes these files into your Godot project:

```
<outputDir>/
└── archetype/
    ├── ArchetypeNode.cs
    ├── archetype_interop.gd
    ├── archetype_keywords.gd
    ├── archetype_signals.gd
    ├── game_events.gd
    └── card-sets/
        ├── core.json
        └── ...
```

| File | What it is |
|---|---|
| `archetype/ArchetypeNode.cs` | C# `Node` subclass; hosts the game session and bridges signals to GDScript |
| `archetype/archetype_interop.gd` | Autoload that decouples UI scripts from the scene tree |
| `archetype/archetype_keywords.gd` | `class_name ArchetypeKeywords` — string constants for every keyword |
| `archetype/archetype_signals.gd` | `class_name ArchetypeSignals` — string constants for derived signals |
| `archetype/game_events.gd` | Typed signal declarations for every keyword referenced in your cards |
| `archetype/card-sets/*.json` | Serialised card sets; load these at runtime |

### 4. Configure Godot

**Register the autoload** — in *Project → Project Settings → Autoload*, add `res://archetype/archetype_interop.gd` with the name `ArchetypeInterop`.

**Add `ArchetypeNode` to your scene** — attach it to a node in your main scene.

**Wire it up in `_ready`:**

```gdscript
func _ready() -> void:
    ArchetypeInterop.register($ArchetypeNode)
    ArchetypeInterop.start()
    # Or with explicit paths:
    # ArchetypeInterop.start({}, "res://archetype/game_definition.json", [
    #     "res://archetype/card-sets/core.json"
    # ])
```

`start()` with no arguments automatically loads `res://archetype/game_definition.json` and all `.json` files in `res://archetype/card-sets/`.

### 5. Connect signals in GDScript

```gdscript
func _ready() -> void:
    ArchetypeInterop.register($ArchetypeNode)
    ArchetypeInterop.action_requested.connect(_on_action_requested)
    ArchetypeInterop.action_resolved.connect(_on_action_resolved)
    ArchetypeInterop.game_over.connect(_on_game_over)

    # Keyword-level signals are emitted directly on ArchetypeNode
    $ArchetypeNode.on_strike.connect(_on_strike)
    $ArchetypeNode.on_heal.connect(_on_heal)

func _on_action_requested(player_name: String, available: Dictionary) -> void:
    # available["playable_cards"]        → Array of card atom IDs
    # available["activatable_abilities"] → Array of {source, effect_name}
    # available["can_pass"]              → bool
    pass

func _on_action_resolved() -> void:
    pass

func _on_game_over(winner_name: String) -> void:
    pass

func _on_strike(target: int, power: float) -> void:
    pass

func _on_heal(target: int, amount: float) -> void:
    pass
```

### 6. Submit player actions

```gdscript
# Play a card
ArchetypeInterop.submit_play_card(player_name, card_atom_id)

# Activate an ability
ArchetypeInterop.submit_activate_ability(player_name, source_atom_id, effect_name)

# Pass
ArchetypeInterop.submit_pass(player_name)
```

## Claude Code Integration

Archetype ships Claude Code skills that give Claude accurate knowledge of the builder API, keyword DSL, and card definition types. Install them once per project, then re-run after upgrading the package.

### Install

```bash
dotnet tool install --global Archetype.Tools
dotnet archetype install-skills
```

Or pin to a specific version via a tool manifest so the whole team stays in sync:

```bash
dotnet new tool-manifest      # creates .config/dotnet-tools.json
dotnet tool install Archetype.Tools
dotnet tool restore            # teammates run this after pulling
dotnet archetype install-skills
```

### Update after a package upgrade

```bash
dotnet tool update Archetype.Tools   # or bump the version in dotnet-tools.json
dotnet archetype install-skills --overwrite
```

The skills are versioned alongside the library — the version you installed matches the API that Claude will reference.

---

### Signal flow

```
ArchetypeInterop.start()
    └── engine starts async loop
            └── ActionRequested  →  UI shows available actions
                    └── submit_play_card / submit_pass / ...
                            └── ActionResolved + keyword signals (on_strike, on_heal, ...)
                                    └── ActionRequested  (next turn)
                                            └── ...
                                                    └── GameOver
```
