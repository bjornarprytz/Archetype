// Global using directives

global using ParseKeyword = System.Func<string, Archetype.Rules.Proto.KeywordInstance>;

global using CheckCost = System.Func<Archetype.Rules.CostPayload, int, bool>;
global using CheckEvent = System.Func<Archetype.Rules.Event, Archetype.Rules.State.ICard, bool>;
global using CheckState = System.Func<Archetype.Rules.State.IAtom, Archetype.Rules.State.IGameState, bool>;
global using ComputeProperty = System.Func<Archetype.Rules.State.IAtom, Archetype.Rules.State.IGameState, object>;

global using ResolveCost = System.Func<Archetype.Rules.State.IGameState, Archetype.Rules.State.Definitions, Archetype.Rules.CostPayload, Archetype.Rules.Event>;
global using ResolveEffect = System.Func<Archetype.Rules.State.IGameState, Archetype.Rules.State.Definitions, Archetype.Rules.Effect, Archetype.Rules.ResolutionContext, Archetype.Rules.Event>;
