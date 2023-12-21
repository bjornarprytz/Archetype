using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Meta;

namespace Archetype.Framework.BaseRules.Keywords.Primitive;

[StaticSyntax("TYPE", typeof(OperandDeclaration<string>))]
public class TypeDefinition : StaticDefinition<string> { }