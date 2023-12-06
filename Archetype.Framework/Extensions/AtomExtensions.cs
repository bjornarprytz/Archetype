using Archetype.Framework.Core.Primitives;
using Archetype.Framework.State;

namespace Archetype.Framework.Extensions;

public static class AtomExtensions
{
    public static bool HasCharacteristic/*<T>*/(this IAtom atom, string key, string stringValue, IResolutionContext context)
    {
        return atom.Characteristics.TryGetValue(key, out var instance) 
               // && instance is CharacteristicInstance<T> { TypedValue: { } typedValue } 
               && (stringValue == "any" || instance.Operands[0].GetValue(context) is {} v && v.Equals(stringValue));
    }
    
    public static bool HasCharacteristic(this IAtom atom, string key)
    {
        return atom.Characteristics.ContainsKey(key);
    }
    
    public static T? GetState<T>(this IAtom atom, string key)
    {
        if (!atom.State.TryGetValue(key, out var value)) return default;
        
        if (value is T typedValue)
            return typedValue;
            
        throw new InvalidOperationException($"State key {key} is not of type {typeof(T).Name}");

    }
    
    public static IKeywordInstance? GetCharacteristic(this IAtom atom, string key)
    {
        return !atom.Characteristics.TryGetValue(key, out var value) ? null : value;
    }
    
    public static int GetCharacteristicValue(this IAtom atom, string key, IResolutionContext context)
    {
        var keywordInstance = atom.GetCharacteristic(key);
        if (keywordInstance is null)
            return 0;

        if (keywordInstance.Operands.Count != 1)
            throw new InvalidOperationException($"Characteristic {key} does not have exactly one operand");
        
        return keywordInstance.Operands[0].GetValue(context) switch
        {
            int intValue => intValue,
            string stringValue when int.TryParse(stringValue, out var intValueFromString) => intValueFromString,
            _ => throw new InvalidOperationException($"Characteristic {key} is not of type integer, or cannot be parsed as an integer")
        };
    }
    
    public static int GetResourceValue(this IAtom atom)
    {
        var value = atom.GetCharacteristic("RESOURCE_VALUE")?.Operands[0].GetValue(null);

        return value switch
        {
            int intValue => intValue,
            string stringValue when int.TryParse(stringValue, out var intValueFromString) => intValueFromString,
            _ => 0
        };
    }
}