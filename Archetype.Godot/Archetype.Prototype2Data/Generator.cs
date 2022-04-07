namespace Archetype.Prototype2Data;

public class Generator
{
    public static IGameState Create()
    {
        return new GameState(
            new Player(1, InitialDeck()), 
            new Map(InitialMap()),
            new WaveEmitter(EnemyDeck()));
    }

    private static IEnumerable<ICard> InitialDeck() =>
        new List<ICard>
        {
            /*
             * 
            Wall,
            Militia,
            Militia,
            Barracks,
            WatchTower,
            WatchTower,
            Necromancer,
             */
        };

    private static IEnumerable<IMapNode> InitialMap()
    {
        var a = new MapNode(2);
        var b = new MapNode(3);
        var c = new MapNode(1);
        var d = new MapNode(4);
/*
 * 
        a.Building = Base;
 */
        
        a.Connect(b);
        b.Connect(c);
        c.Connect(d);

        return new List<IMapNode> { a, b, c, d };
    }
    private static IEnumerable<IEnemyCard> EnemyDeck() =>
        new List<IEnemyCard>
        {
            /*
             * 
            new Wave(Piglet, Piglet),
            new Wave(BigPig,  Goat),
            new Wave(Goat, Cow),
            new Wave(Piglet, Piglet, Piglet, Goat),
            new Wave(Cow, Crocodile),
             */
        };

/*
 * 
    private static ICard Base => new Card("Base", 0, 1, 5, 10, Keyword.Draw);
    private static ICard Woodcutter => new Card("Woodcutter", 1, 1, 0, 2, Keyword.ClearCutting);
    private static ICard Militia => new Card("Militia", 1, 2, 2, 2);
    private static ICard Wall => new Card("Wall", 1, 0, 2, 6);
    private static ICard WatchTower => new Card("Watch Tower", 1, 0, 0, 6, Keyword.Ranged);
    private static ICard Barracks => new Card("Barracks", 2, 2, 2, 4, Keyword.Repair);
    private static ICard Necromancer => new Card("Necromancer", 2, 0, 1, 3, Keyword.RaiseDead);

    private static IEnemyCard Piglet => new Enemy("Piglet",  1, 2);
    private static IEnemyCard BigPig => new Enemy("Big Pig", 1, 3);
    private static IEnemyCard Goat => new Enemy("Goat", 3, 1);
    private static IEnemyCard Cow => new Enemy("Cow", 2, 4);
    private static IEnemyCard Crocodile => new Enemy("Crocodile", 4, 3);
 */
}
