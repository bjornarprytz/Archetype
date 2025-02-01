namespace Archetype.Tests;

public static class ResultAssertions
{
    public static Dictionary<string, object?[]> NoOp(string keyword) => new()
    {
        { keyword, new object?[] { null } }
    };
    
    public static Dictionary<string, object?[]> Atomic<T>(string keyword, T result) => new()
    {
        { keyword, new object?[] { result } }
    };
}