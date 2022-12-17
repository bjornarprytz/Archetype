namespace Archetype.Game;

internal static class Static
{
    private static Random _random;
    public static Random Random => _random ??= new Random();

    public static Random SetRandomSeed(int seed)
    {
        if (_random != null)
            throw new InvalidOperationException("Random seed already set");
        
        _random = new Random(seed);
        
        Console.WriteLine($"Random seed set to <{seed}>");

        return _random;
    }
}