// Global using directives

global using CheckCard = System.Func<Archetype.Framework.Card, bool>;
global using CheckCost = System.Func<Archetype.Framework.CostPayload, bool>;
global using CheckEvent = System.Func<Archetype.Framework.Event, Archetype.Framework.Card, bool>;
global using CheckState = System.Func<Archetype.Framework.Card, Archetype.Framework.GameState, bool>;

global using ResolveCost = System.Func<Archetype.Framework.GameState, Archetype.Framework.Definitions, Archetype.Framework.CostPayload, Archetype.Framework.Event>;
global using ResolveEffect = System.Func<Archetype.Framework.GameState, Archetype.Framework.Definitions, Archetype.Framework.EffectPayload, Archetype.Framework.Event>;

global using ApplyAura = System.Func<Archetype.Framework.Card, Archetype.Framework.Event>;
global using RemoveAura = System.Func<Archetype.Framework.Card, Archetype.Framework.Event>;

global using CreateEffect = System.Func<Archetype.Framework.Card, Archetype.Framework.EffectArgs, Archetype.Framework.EffectPayload>;
global using CreateCardEffects = System.Func<Archetype.Framework.CardArgs, System.Collections.Generic.IEnumerable<Archetype.Framework.EffectPayload>>;
global using CreateAbilityEffects = System.Func<Archetype.Framework.AbilityArgs, System.Collections.Generic.IEnumerable<Archetype.Framework.EffectPayload>>;

