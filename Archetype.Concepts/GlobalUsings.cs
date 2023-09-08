// Global using directives

global using ParseKeyword = System.Func<string, Archetype.Rules.Proto.ProtoData>;

global using CheckCard = System.Func<Archetype.Rules.State.Card, bool>;
global using CheckCost = System.Func<Archetype.Rules.CostPayload, int, bool>;
global using CheckEvent = System.Func<Archetype.Rules.Event, Archetype.Rules.State.Card, bool>;
global using CheckState = System.Func<Archetype.Rules.State.Card, Archetype.Rules.State.GameState, bool>;

global using ResolveCost = System.Func<Archetype.Rules.State.GameState, Archetype.Rules.State.Definitions, Archetype.Rules.CostPayload, Archetype.Rules.Event>;
global using ResolveEffect = System.Func<Archetype.Rules.State.GameState, Archetype.Rules.State.Definitions, Archetype.Rules.Effect, Archetype.Rules.Event>;

global using ApplyAura = System.Func<Archetype.Rules.State.Card, Archetype.Rules.Event>;
global using RemoveAura = System.Func<Archetype.Rules.State.Card, Archetype.Rules.Event>;

global using CreateEffect = System.Func<Archetype.Rules.State.Card, System.Collections.Generic.IEnumerable<Archetype.Rules.State.Card>, Archetype.Rules.Effect>;
global using CreateCardEffects = System.Func<System.Collections.Generic.IEnumerable<Archetype.Rules.State.Card>, System.Collections.Generic.IEnumerable<Archetype.Rules.Effect>>;
global using CreateAbilityEffects = System.Func<System.Collections.Generic.IEnumerable<Archetype.Rules.State.Card>, System.Collections.Generic.IEnumerable<Archetype.Rules.Effect>>;

