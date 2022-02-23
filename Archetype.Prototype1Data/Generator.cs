namespace Archetype.Prototype1Data
{
    public class Generator
    {
        public static IGameState Create()
        {
            return new GameState(
                new Player(1, InitialDeck()), 
                new Map(InitialMap()),
                Waves());
        }

        private static IEnumerable<ICard> InitialDeck() =>
            new List<ICard>
            {
                Palissade,
                Militia,
                Militia,
                Barracks,
                WatchTower,
                WatchTower,
                Necromancer,
            };

        private static IEnumerable<IMapNode> InitialMap()
        {
            var a = new MapNode();
            var b = new MapNode();
            var c = new MapNode();
            var d = new MapNode();

            a.Building = new Building((Card)Base);
            
            a.Connect(b);
            b.Connect(c);
            c.Connect(d);

            return new List<IMapNode> { a, b, c, d };
        }
        private static IEnumerable<IWave> Waves() =>
            new List<IWave>
            {
                new Wave(Piglet, Piglet),
                new Wave(BigPig, Goat),
                new Wave(Goat, Cow),
                new Wave(Piglet, Piglet, Piglet, Goat),
                new Wave(Cow, Crocodile),
            };


        private static ICard Base => new Card("Base", 0, 10, 1, 5);
        private static ICard Woodcutter => new Card("Woodcutter", 1, 2, 1, 0, Keyword.ClearCutting);
        private static ICard Militia => new Card("Militia", 1, 2, 2, 2);
        private static ICard Palissade => new Card("Palissade", 1, 6, 0, 2);
        private static ICard WatchTower => new Card("Watch Tower", 1, 6, 0, 0, Keyword.Ranged);
        private static ICard Barracks => new Card("Barracks", 2, 4, 2, 2, Keyword.Repair);
        private static ICard Necromancer => new Card("Barracks", 2, 3, 0, 1, Keyword.RaiseDead);

        private static IEnemy Piglet => new Enemy("Piglet", 2, 1);
        private static IEnemy BigPig => new Enemy("Big Pig", 3, 1);
        private static IEnemy Goat => new Enemy("Goat", 1, 3);
        private static IEnemy Cow => new Enemy("Cow", 4, 2);
        private static IEnemy Crocodile => new Enemy("Crocodile", 3, 4);
    }
}