// Global using directives

global using ParseKeyword = System.Func<string, Archetype.Framework.Proto.KeywordInstance>;

global using CheckCost = System.Func<Archetype.Framework.Runtime.CostPayload, int, bool>;
global using CheckEvent = System.Func<Archetype.Framework.Runtime.IEvent, Archetype.Framework.Runtime.State.ICard, bool>;
global using CheckState = System.Func<Archetype.Framework.Runtime.State.IAtom, Archetype.Framework.Runtime.State.IGameState, bool>;
global using ComputeProperty = System.Func<Archetype.Framework.Runtime.State.IAtom, Archetype.Framework.Runtime.State.IGameState, object>;

global using ResolveCost = System.Func<Archetype.Framework.Runtime.State.IGameState, Archetype.Framework.Runtime.IDefinitions, Archetype.Framework.Runtime.CostPayload, Archetype.Framework.Runtime.IEvent>;
global using ResolveEffect = System.Func<Archetype.Framework.Runtime.State.IGameState, Archetype.Framework.Runtime.IDefinitions, Archetype.Framework.Runtime.Effect, Archetype.Framework.Runtime.ResolutionContext, Archetype.Framework.Runtime.IEvent>;
