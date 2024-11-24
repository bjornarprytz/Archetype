using System.Reflection;
using Archetype.Framework.Core;
using Archetype.Framework.Data;
using Archetype.Framework.Effects;
using Archetype.Framework.GameLoop;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;

namespace Archetype.Framework.Parsing;

public class ExpressionParser
{
    private readonly Dictionary<string, MethodInfo> _effectMethods;

    public ExpressionParser(IEnumerable<MethodInfo> effectMethods)
    {
        _effectMethods = effectMethods.ToDictionary(m => m.Name);
    }
    
    public CardProto ParseCard(CardData cardData)
    {
        return new CardProto()
        {
            Name = cardData.Name,
            Costs = cardData.Costs.ToDictionary(kvp => kvp.Key, kvp => ParseNumberExpression(kvp.Value)),
            Targets = cardData.Targets.Select(ParseTargets).ToArray(),
            
            Stats = cardData.Stats.ToDictionary(kvp => kvp.Key, kvp => ParseNumberExpression(kvp.Value)),
            Facets = cardData.Facets,
            Tags = cardData.Tags,
            
            Effects = cardData.Effects.Select(ParseEffect).ToArray(),
            Variables = cardData.Variables.ToDictionary(kvp => kvp.Key, kvp => ParseNumberExpression(kvp.Value)),
        };
    }
    
    private TargetProto ParseTargets(TargetData targetData)
    {
        return new TargetProto()
        {
            Predicates = targetData.ConditionalExpressions.Select(ParsePredicate).ToArray(),
        };
    }

    private INumber ParseNumberExpression(Expression expression)
    {
        if (int.TryParse(expression.Text, out var immediateValue))
        {
            return new ImmediateNumber(immediateValue);
        }
        
        
        return new ReferenceNumber(ParsePathExpression(expression.Text));
    }
    
    private string[] ParsePathExpression(string expression)
    {
        return expression.Split('.').Select(t => t.ToLower()).ToArray();
    }

    private EffectProto ParseEffect(EffectData effectData)
    {
        return new EffectProto()
        {
            Keyword = effectData.Keyword,
            Parameters = effectData.ArgumentExpressions.Select(ParseNumberExpression).ToArray(),
        };
    }
    
    private IAtomPredicate ParsePredicate(Expression expression)
    {
        var parts = expression.Text.Split(' ').Select(t => t.Trim()).ToArray();

        if (parts.Length != 3)
            throw new InvalidOperationException($"Invalid predicate: {expression.Text}");

        var atomValue = new ReferenceValue(parts[0].Split('.'));
        
        var comparisonOperator = ParseComparisonExpression(parts[1]);

        var compareValue = new ReferenceValue(parts[2].Split('.'));
        
        return new AtomPredicate(atomValue, comparisonOperator, compareValue);
    }
    
    

    private record AtomPredicate : IAtomPredicate
    {
        public AtomPredicate(IValue atomValue, ComparisonOperator @operator, IValue compareValue)
        {
            AtomValue = atomValue;
            Operator = @operator;
            CompareValue = compareValue;
            
            if (atomValue.ValueType != compareValue.ValueType)
                throw new InvalidOperationException($"Mismatched value types: {atomValue.ValueType} != {compareValue.ValueType}");

            if (atomValue.ValueType == typeof(string) && @operator is not (ComparisonOperator.Equal or ComparisonOperator.NotEqual))
                throw new InvalidOperationException($"Invalid comparison operator for string: {@operator}");

            if (atomValue.ValueType == typeof(int) &&
                @operator is ComparisonOperator.Contains or ComparisonOperator.NotContains)
            {
                throw new InvalidOperationException($"Invalid comparison operator for int: {@operator}");
            }
            
            if (atomValue.ValueType == typeof(IEnumerable<IAtom>) &&
                @operator is not (ComparisonOperator.Contains or ComparisonOperator.NotContains))
            {
                throw new InvalidOperationException($"Invalid comparison operator for IEnumerable<IAtom>: {@operator}");
            }
        }

        public IValue AtomValue { get; init; }
        public ComparisonOperator Operator { get; init; }
        public IValue CompareValue { get; init; }
    }

    private record ReferenceValue : IValue
    {
        public ReferenceValue(IEnumerable<string> path)
        {
            
            Path = path.ToArray();
            
            Whence = ParseWhence(Path[0]);
            
            ValueType = GetValueType(Path);
            
        }
        
        public string[] Path { get; }
        public Whence Whence { get; }
        
        public object? Immediate => null;
        
        public Type ValueType { get; }
        
    }
    
    private record ReferenceNumber : INumber
    {
        public ReferenceNumber(IEnumerable<string> path)
        {
            Path = path.ToArray();
            
            Whence = ParseWhence(Path[0]);
            
            var valueType = GetValueType(Path);
            
            if (valueType != ValueType)
                throw new InvalidOperationException($"Invalid value type for number: {valueType}");
        }

        public int? ImmediateValue => null;
        public object? Immediate => null;
        public Type ValueType => typeof(int);
        public Whence Whence { get; }
        public string[] Path { get; }
    }

    private record ReferenceWord : IWord
    {
        public ReferenceWord(IEnumerable<string> path)
        {
            Path = path.ToArray();
            
            Whence = ParseWhence(Path[0]);
            
            var valueType = GetValueType(Path);
            
            if (valueType != ValueType)
                throw new InvalidOperationException($"Invalid value type for word: {valueType}");
        }
        
        public string? ImmediateValue => null;
        public object? Immediate => null;
        public Type ValueType => typeof(string);
        public Whence Whence { get; }
        public string[] Path { get; }
    }
    
    private record ReferenceGroup : IAtomGroup
    {
        public ReferenceGroup(IEnumerable<string> path)
        {
            Path = path.ToArray();
            
            Whence = ParseWhence(Path[0]);
            
            var valueType = GetValueType(Path);
            
            if (valueType != ValueType)
                throw new InvalidOperationException($"Invalid value type for group: {valueType}");
        }
        
        public object? Immediate => null;
        public Type ValueType => typeof(IEnumerable<IAtom>);
        public Whence Whence { get; }
        public string[] Path { get; }
    }

    private record ImmediateWord(string Value) : IWord
    {
        public string? ImmediateValue => Value;
        public object? Immediate => ImmediateValue;
        public string[]? Path { get; private set;}
        public Type ValueType => typeof(string);
        public Whence Whence => Whence.Immediate;
    }
    private record ImmediateNumber(int Value) : INumber
    {
        public int? ImmediateValue => Value;
        public object? Immediate => ImmediateValue;
        public string[]? Path => null;
        public Type ValueType => typeof(int);
        public Whence Whence => Whence.Immediate;
    }
    
    private static ComparisonOperator ParseComparisonExpression(string text)
    {
        return text switch
        {
            ">" => ComparisonOperator.GreaterThan,
            ">=" => ComparisonOperator.GreaterThanOrEqual,
            "<" => ComparisonOperator.LessThan,
            "<=" => ComparisonOperator.LessThanOrEqual,
            "=" => ComparisonOperator.Equal,
            "!=" => ComparisonOperator.NotEqual,
            "has" => ComparisonOperator.Contains,
            "!has" => ComparisonOperator.NotContains,
            _ => throw new InvalidOperationException($"Unknown comparison operator: {text}")
        };
    }

    private static Whence ParseWhence(string pathSegment)
    {
        return pathSegment switch
        {
            "state" => Whence.State,
            "scope" => Whence.Scope,
            "atom" => Whence.Atom,
            _ => throw new InvalidOperationException($"Unknown whence: {pathSegment}")
        };
    }
    
    private static Type GetValueType(string[] path)
    {
        var whence = ParseWhence(path[0]);

        var rootType = whence switch
        {
            Whence.State => typeof(IGameState),
            Whence.Scope => typeof(IScope),
            Whence.Atom => typeof(IAtom),
            Whence.Immediate => throw new InvalidOperationException(
                "Value type from immediate value should not be determined by path."),
            _ => throw new InvalidOperationException($"Unknown whence: {whence}")
        };

        var currentType = rootType;
        
        foreach (var part in path[1..])
        {
            currentType = currentType.GetPartType(part);
            
            if (currentType is null)
                throw new InvalidOperationException($"Invalid path: {part} of {string.Join('.', path)}");
        }
        
        return currentType;
    }
}

file static class ReflectionExtensions
{
    public static Type? GetPartType(this Type type, string part)
    {
        // TODO: Account for collection accessors ( e.g. ..stats:health )
        
        var partAccessorMethod = type.GetMethods()
            .FirstOrDefault(m => m.GetCustomAttributes()
                .Any(a => a is PathPartAttribute { Name: { } name } && name == part));
        
        if (partAccessorMethod?.ReturnType is { } returnType)
            return returnType;
        
        var partProperty = type.GetProperties()
            .FirstOrDefault( p => p.GetCustomAttributes()
                .Any(a => a is PathPartAttribute { Name: { } name } && name == part));
        
        return partProperty?.PropertyType;
    }
}