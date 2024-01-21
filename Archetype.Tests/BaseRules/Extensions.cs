using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.State;
using NSubstitute;

namespace Archetype.Tests.BaseRules;

public static class CharacteristicsExtensions
{
    public static void SetupCharacteristicReturn<T>(this IAtom atom, string key, T value)
    {
        var characteristic = Substitute.For<IKeywordInstance>();
            
        characteristic.ResolveFuncName.Returns(key);
        characteristic.Operands.Returns(Declare.Operands(Declare.Operand(value)));
        
        atom.Characteristics.Returns( new Dictionary<string, IKeywordInstance> { [key] = characteristic });
    }
}