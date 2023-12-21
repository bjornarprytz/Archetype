using Archetype.Framework.BaseRules.Keywords.Primitive;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.Meta;
using Archetype.Framework.State;

namespace Archetype.Prototype1.Keywords;

[EffectSyntax("DAMAGE", typeof(OperandDeclaration<IAtom, int>))]
public class Damage : ChangeState<IAtom, int>
{
    protected override string Property { get; } = "DAMAGE";
}