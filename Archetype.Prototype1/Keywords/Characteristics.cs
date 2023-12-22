using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Meta;
using Archetype.Framework.State;

namespace Archetype.Prototype1.Keywords;

[StaticSyntax("HEALTH", typeof(OperandDeclaration<int>))]
public class Health : StaticDefinition<int>{ }

[StaticSyntax("VALUE", typeof(OperandDeclaration<int>))]
public class Value : StaticDefinition<int>{ }