namespace Archetype.Framework.Core.Primitives;

public class ComputeDefinition(Delegate handler) : IComputeDefinition
{
    public string Keyword { get; } = handler.Method.Name;
    public int Compute(IResolutionContext context, IKeywordInstance keywordInstance)
    {
        var parameters = new List<object>
                { context }
            .Append(keywordInstance.Operands.Select(o => o.GetValue(context)))
            .ToArray();
                
        if (handler.DynamicInvoke(parameters) is int result)
            return result;
        
        throw new InvalidOperationException($"ComputeDefinition ({Keyword}) did not return an int");
    }
}