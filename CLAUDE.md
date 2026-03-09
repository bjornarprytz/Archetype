# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Archetype is a card game engine whose core goal is a **single card definition** that drives both rules execution and human-readable card text. It is currently in the requirements/design phase. The old implementation on `master` is a stale reference — the current branch (`back-to-basics`) starts fresh.

## Personas

Personas live in `./prompts/`. Each is a self-contained role definition to be loaded at the start of a relevant session (--system-prompt-file). Defined personas:

- `./prompts/project-manager.md` — coordinates changes across all other personas; start here when you're unsure what to do next
- `./prompts/requirements-analyst.md`
- `./prompts/domain-modeler.md`
- `./prompts/technical-architect.md`
- `./prompts/implementer.md`
- `./prompts/reviewer.md`

## Core Domain Concepts

These are the foundational concepts agreed on during design. All personas should treat this vocabulary as stable unless a requirements change is explicitly approved.

### Keyword
The primitive unit of the system. A keyword is a named function with typed parameters that either mutates game state directly or composes other keywords. Keywords declare what they contribute to the event log. Example:

```
take_damage(atom, amount)  // primitive — directly mutates state
attack(atom, amount)       // composite — calls take_damage(atom, max(0, amount - atom.defense))
```

### Effect Block
An ordered sequence of keyword invocations that executes atomically — no triggers or state-based effects resolve mid-block. An effect block has a local scope (the event log filtered to this block) and appends its events to the parent action's scope.

### Action
The unit of player agency. A single turn contains multiple actions (e.g. play card, activate ability, end turn). Effect blocks execute within an action. Triggers resolve between actions, not mid-block.

### Event Log
An append-only log of everything that happens in a game. Every keyword execution appends structured events. The log is queryable by scope:

```
events.this_block   // events in the current effect block
events.this_action  // events in the current action
events.this_turn    // events in the current turn
events.this_game    // all events
```

### Scope Hierarchy
```
game
  └── turn
        └── action  (play card, activate ability, end turn, ...)
              └── effect block
```

### Static vs Activated Effects
- **Activated effect**: an effect block that executes when explicitly triggered (playing a card, activating an ability).
- **Static effect**: a standing condition with a lifetime (while in play, for N turns). Can have a trigger — a subscription on the event log — that fires an activated effect when the condition is met.

### Dual-Use Definition
The same keyword/effect-block definition must support both:
1. **Execution** — resolving game state changes
2. **Text rendering** — generating human-readable card text, with the full composition tree available so the game layer can decide what level of detail to show the player

## Artifact Conventions

### Status Frontmatter

Every artifact in `docs/` must begin with a YAML frontmatter block before the `#` title:

```yaml
---
status: signed-off        # draft | in-progress | signed-off
owner: domain-modeler     # persona responsible for this artifact
signed-off: 2026-03-02    # omit if not yet signed off
last-updated: 2026-03-08
depends-on: []            # paths of artifacts this one depends on
---
```

Agents read the frontmatter to assess artifact status without reading body content. The prose `## Status` section may remain for human-readable history.

### Task Annotations

Each task in `tasks.md` should carry `reads:` and `writes:` annotations on indented lines immediately below:

```markdown
- [ ] 1.1 Add `Id: string` to `GameDefinition` record
  - reads: `docs/architecture.md#D17`, `src/Archetype.Core/GameDefinition.cs`
  - writes: `src/Archetype.Core/GameDefinition.cs`
```

Use document anchor notation (`#section`) for large docs. An agent reads only the listed files before starting a task — nothing more.

### File Naming

Each file should contain exactly one primary type, named after that type (`ValidationResult.cs` for `ValidationResult`, etc.). When writing task annotations, use the correct target file name — do not group unrelated types under a generic name like `ActionArgs.cs`.

### Summaries and Responses

When summarising completed work or presenting proposals to the user, always include relative file links (with `#anchor` where applicable) so the user can navigate directly to the relevant sections. Example:

```
- Updated [`docs/architecture.md#D25`](docs/architecture.md) with DiagnosticEvent shape
- Tasks written to [`openspec/changes/foo/tasks.md`](openspec/changes/foo/tasks.md)
```
