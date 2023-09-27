// Global using directives

global using CheckEvent = System.Func<Archetype.Framework.Runtime.IEvent, Archetype.Framework.Runtime.State.ICard, bool>;
global using CheckState = System.Func<Archetype.Framework.Runtime.State.IAtom, Archetype.Framework.Runtime.State.IGameState, bool>;
