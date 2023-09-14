// Global using directives

global using ParseKeyword = System.Func<string, Archetype.Rules.Proto.KeywordInstance>;

global using CheckCost = System.Func<Archetype.Rules.CostPayload, int, bool>;
global using CheckEvent = System.Func<Archetype.Rules.IEvent, Archetype.Runtime.State.ICard, bool>;
global using CheckState = System.Func<Archetype.Runtime.State.IAtom, Archetype.Runtime.State.IGameState, bool>;
global using ComputeProperty = System.Func<Archetype.Runtime.State.IAtom, Archetype.Runtime.State.IGameState, object>;

global using ResolveCost = System.Func<Archetype.Runtime.State.IGameState, Archetype.Runtime.IDefinitions, Archetype.Rules.CostPayload, Archetype.Rules.IEvent>;
global using ResolveEffect = System.Func<Archetype.Runtime.State.IGameState, Archetype.Runtime.IDefinitions, Archetype.Rules.Effect, Archetype.Rules.ResolutionContext, Archetype.Rules.IEvent>;
