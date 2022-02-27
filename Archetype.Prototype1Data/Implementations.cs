using System.Reactive.Subjects;
using System.Security.Principal;

namespace Archetype.Prototype1Data
{
    internal class GameView : IGameView
    {
        private readonly GameState _gameState;

        public GameView(IGameState gameState)
        {
            _gameState = (GameState)gameState;
        }


        public IGameState GameState => _gameState;

        public void StartGame()
        {
            ((Player)_gameState.Player).Draw();
            ((Player)_gameState.Player).Draw();
            
            Movement();
        }

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
                var engagedEnemies = building.EngagedEnemies.OfType<Enemy>().ToList();

                var attackStrength = building.Strength;
                
                foreach (var enemy in engagedEnemies.Where(e => e.IsAlive()))
                {
                    var damageAppliedToEnemy = attackStrength.Clamp(0, enemy.Health);
                    
                    enemy.Attack(damageAppliedToEnemy);
                    attackStrength -= damageAppliedToEnemy;
                }
                
                foreach (var enemy in engagedEnemies.Where(e => e.IsAlive()))
                {
                    building.Attack(enemy.Strength);
                }
            }
        }

        private void Movement()
        {
            var path = _gameState.Map.PathToBase();

            var roamingEnemies = _gameState.EachRoamingEnemy().ToList();
            
            foreach (var enemy in roamingEnemies)
            {
                var from = (MapNode)enemy.Node;
                var to = path[from];
                
                from.RemoveEnemy(enemy);
                to.AddEnemy(enemy);
            }

            foreach (var enemy in _gameState.WaveEmitter.EmitNext().Enemies.OfType<Enemy>())
            {
                ((MapNode)_gameState.Map.StagingArea).AddEnemy(enemy);
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
            }
            
            foreach (var building in _gameState.BuildingsWithKeyword(Keyword.Draw))
            {
                ((Player)_gameState.Player).Draw();
            }
        }
    }

    internal class GameState : IGameState
    {
        public GameState(IPlayer player, IMap map, IWaveEmitter waveEmitter)
        {
            WaveEmitter = waveEmitter;
            Player = player;
            Map = map;
        }

        public IPlayer Player { get; }
        public IMap Map { get; }
        public IWaveEmitter WaveEmitter { get; }
    }

    internal class Player : IPlayer
    {
        private readonly Subject<ICard> _cardDrawn = new Subject<ICard>();
        private readonly Subject<ICard> _cardRemoved = new Subject<ICard>();
        private readonly Stack<ICard> _deck = new Stack<ICard>();
        private readonly List<ICard> _hand = new List<ICard>();

        public Player(int resources, IEnumerable<ICard> deck)
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
        public IObservable<ICard> OnCardDrawn => _cardDrawn;
        public IObservable<ICard> OnCardRemoved => _cardRemoved;

        internal void Draw()
        {
            if (_deck.IsEmpty())
            {
                return;
            }

            var card = _deck.Pop();

            _hand.Add(card);
            
            _cardDrawn.OnNext(card);
        }

        internal void Commit(Card card)
        {
            Resources -= card.Cost;
            _hand.Remove(card);
            
            _cardRemoved.OnNext(card);
        }
    }

    internal class Map : IMap
    {
        private readonly List<IMapNode> _nodes = new List<IMapNode>();
        
        public Map(IEnumerable<IMapNode> nodes)
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

    internal class WaveEmitter : IWaveEmitter
    {
        private readonly Stack<IWave> _waves = new Stack<IWave>();

        public WaveEmitter(IEnumerable<IWave> waves)
        {
            foreach (var wave in waves.Reverse())
            {
                _waves.Push(wave);
            }
        }


        public IWave EmitNext()
        {
            return _waves.Pop();
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

        internal void Attack(int damage)
        {
            Health -= damage;
        }
    }

    internal class Building : IBuilding
    {
        private readonly List<ICard> _cards = new List<ICard>();
        private readonly List<IEnemy> _engagedEnemies = new List<IEnemy>();

        public Building(Card foundation)
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