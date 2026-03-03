## ADDED Requirements

### Requirement: D9 constructor signature matches the D14 API surface
The D9 section of `docs/architecture.md` SHALL reference `IReadOnlyDictionary<string, IPlayerStrategy>` (not `IPromptChannel`) as the second parameter of the `ActionResolver` constructor in its consequences block. The documented constructor SHALL be consistent with the constructor established by D14 and its A15 addendum.

#### Scenario: No IPromptChannel reference in D9 constructor
- **WHEN** a reader inspects the `ActionResolver` constructor signature documented in D9's consequences block
- **THEN** the signature reads `ActionResolver(GameDefinition, IReadOnlyDictionary<string, IPlayerStrategy>, IRandomSource, IEngineObserver?)`
- **THEN** the text contains no reference to `IPromptChannel` as a constructor parameter in that section

#### Scenario: Constructor is consistent across D9 and D14
- **WHEN** a reader compares the `ActionResolver` constructor listed in D9's consequences with the constructor listed in D14's addendum
- **THEN** both show the same parameter types in the same order
