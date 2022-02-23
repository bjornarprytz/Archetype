using System.Security.Principal;

namespace Archetype.Prototype1Data
{
    internal class GameView : IGameView
    {
        private readonly GameState _gameState;

        internal GameView(IGameState gameState)
        {
            _gameState = (GameState)gameState;
        }


        public IGameState GameState => _gameState;

        public void PlayCard(ICard card, IMapNode target)
        {
            if (card is not Card c || target is not MapNode n)
            {
                return;
            }

            if (c.Cost > _gameState.Player.Resources)
            {
                return;
            }

            ((Player) _gameState.Player).Commit(c);

            if (n.Building is Building building)
            {
                building.Add(c);
            }
            else
            {
                n.Building = new Building(c);
            }
        }

        public void EngageEnemy(IBuilding building, IEnemy enemy)
        {
            if (building is not Building b || enemy is not Enemy e)
            {
                return;
            }
            
            b.Engage(e);
        }

        public void EndTurn()
        {
            Combat();
            Movement();
            Upkeep();
        }

        private void Combat()
        {
            foreach (var building in _gameState.EachBuilding())
            {
                foreach (var enemy in building.EngagedEnemies.OfType<Enemy>())
                {
                    building.Attack(enemy.Strength);
                }
            }
        }

        private void Movement()
        {
            foreach (var enemy in _gameState.EachIncomingEnemy())
            {
                // TODO: Do the path finding
            }
            
            
        }

        private void Upkeep()
        {
            foreach (var building in _gameState.BuildingsWithKeyword(Keyword.ClearCutting))
            {
                ((MapNode)building.Node!).Presence++;
                ((Player)_gameState.Player).Resources++;
            }
            
            foreach (var building in _gameState.BuildingsWithKeyword(Keyword.Repair))
            {
                building.Health++;
                
                ((Player)_gameState.Player).Draw();
            }
            
            foreach (var building in _gameState.BuildingsWithKeyword(Keyword.Draw))
            {
                ((Player)_gameState.Player).Draw();
            }
        }
    }

    internal class GameState : IGameState
    {
        private readonly List<IWave> _waves = new List<IWave>();

        internal GameState(IPlayer player, IMap map, IEnumerable<IWave> waves)
        {
            Player = player;
            Map = map;
            _waves.AddRange(waves);
        }

        public IPlayer Player { get; }
        public IMap Map { get; }
        public IEnumerable<IWave> Waves => _waves;
    }

    internal class Player : IPlayer
    {
        private readonly Stack<ICard> _deck = new Stack<ICard>();
        private readonly List<ICard> _hand = new List<ICard>();

        internal Player(int resources, IEnumerable<ICard> deck)
        {
            Resources = resources;

            foreach (var card in deck.Shuffle())
            {
                _deck.Push(card);
            }
        }

        public int Resources { get; internal set; }
        public int CardsInDeck => _deck.Count;
        public IEnumerable<ICard> Hand => _hand;

        internal void Draw()
        {
            if (_deck.IsEmpty())
            {
                return;
            }

            var card = _deck.Pop();

            _hand.Add(card);
        }

        internal void Commit(Card card)
        {
            Resources -= card.Cost;
            _hand.Remove(card);
        }
    }

    internal class Map : IMap
    {
        private readonly List<IMapNode> _nodes = new List<IMapNode>();
        
        internal Map(IEnumerable<IMapNode> nodes)
        {
            _nodes.AddRange(nodes);

            StagingArea = _nodes.LastOrDefault();
        }

        public IEnumerable<IMapNode> Nodes => _nodes;
        public IMapNode? StagingArea { get; }
    }

    internal class MapNode : IMapNode
    {
        private readonly List<IEnemy> _enemies = new List<IEnemy>();
        private readonly List<IMapNode> _neighbours = new List<IMapNode>();

        public MapNode(int presence=0)
        {
            Presence = presence;
        }

        public int Presence { get; internal set; }
        public IEnumerable<IMapNode> Neighbours => _neighbours;
        public IEnumerable<IEnemy> Enemies => _enemies;
        public IBuilding? Building { get; internal set; }

        internal void AddEnemy(Enemy enemy)
        {
            _enemies.Add(enemy);
            enemy.Node = this;
        }

        internal void RemoveEnemy(Enemy enemy)
        {
            _enemies.Remove(enemy);
            enemy.Node = null;
            enemy.Building = null;
        }

        internal void AddNeighbour(MapNode node)
        {
            if (_neighbours.Contains(node))
                return;

            _neighbours.Add(node);
        }

        internal void RemoveNeighbour(MapNode node)
        {
            if (!_neighbours.Contains(node))
                return;

            _neighbours.Remove(node);
        }
    }

    internal class Wave : IWave
    {
        private readonly List<IEnemy> _enemies = new List<IEnemy>();

        public Wave(params IEnemy[] enemies)
        {
            _enemies.AddRange(enemies);
        }

        public IEnumerable<IEnemy> Enemies => _enemies;
    }

    internal class Enemy : IEnemy
    {
        public Enemy(string name, int health, int strength)
        {
            Name = name;
            Health = health;
            Strength = strength;
        }

        public string Name { get; }
        public int Health { get; internal set; }
        public int Strength { get; internal set; }
        public IMapNode? Node { get; internal set; }
        public IBuilding? Building { get; internal set; }
    }

    internal class Building : IBuilding
    {
        private readonly List<ICard> _cards = new List<ICard>();
        private readonly List<IEnemy> _engagedEnemies = new List<IEnemy>();

        internal Building(Card foundation)
        {
             
            IsBase = foundation.Name == "Base";
            
            Add(foundation);
        }

        public bool IsBase { get; }
        public int Health { get; internal set; }
        public int Strength => _cards.Sum(c => c.Strength);
        public int Presence => _cards.Sum(c => c.Presence) + (Node?.Presence ?? 0);
        public IEnumerable<Keyword> Keywords => _cards.SelectMany(c => c.Keywords).Distinct();

        public IEnumerable<ICard> Cards => _cards;
        public IEnumerable<IEnemy> EngagedEnemies => _engagedEnemies;
        public IMapNode? Node { get; internal set; }

        internal void Add(Card card)
        {
            _cards.Add(card);
            Health += card.Health;
        }

        internal void Attack(int damage)
        {
            Health -= damage;
        }

        internal void Engage(Enemy enemy)
        {
            if (_engagedEnemies.Count >= Presence)
            {
                return;
            }

            _engagedEnemies.Add(enemy);
            enemy.Building = this;
        }

        internal void Disengage(Enemy enemy)
        {
            if (!_engagedEnemies.Contains(enemy))
            {
                return;
            }
            
            _engagedEnemies.Remove(enemy);
            enemy.Building = null;
        }
    }

    internal class Card : ICard
    {
        private readonly List<Keyword> _keywords = new List<Keyword>();

        public Card(string name, int cost, int health, int strength, int presence, params Keyword[] keywords)
        {
            Name = name;
            Health = health;
            Strength = strength;
            Presence = presence;
            Cost = cost;

            _keywords.AddRange(keywords);
        }

        public string Name { get; }
        public int Cost { get; }
        public int Health { get; }
        public int Strength { get; }
        public int Presence { get; }
        public IEnumerable<Keyword> Keywords => _keywords;
    }
}