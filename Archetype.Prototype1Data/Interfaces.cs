namespace Archetype.Prototype1Data
{
    public interface IGameView
    {
        IGameState GameState { get; }

        void PlayCard(ICard card, IMapNode target);
        void EngageEnemy(IBuilding building, IEnemy enemy);
        void EndTurn();
    }
    
    public interface IGameState
    {
        IPlayer Player { get; }
        IMap Map { get; }
        
        IWaveEmitter WaveEmitter { get; }
    }

    public interface IWaveEmitter
    {
        IWave EmitNext();
    }
    public interface IWave
    {
        IEnumerable<IEnemy> Enemies { get; }
    }
    
    public interface IPlayer
    {
        int Resources { get; }
        
        int CardsInDeck { get; }
        IEnumerable<ICard> Hand { get; }
    }

    public interface IMap
    {
        IEnumerable<IMapNode> Nodes { get; }

        IMapNode? StagingArea { get; }
    }

    public interface IMapNode
    {
        int Presence { get; }
        IEnumerable<IMapNode> Neighbours { get; }
        IEnumerable<IEnemy> Enemies { get; }

        IBuilding? Building { get; }
    }

    public interface IEnemy
    {
        string Name { get; }
        int Health { get; }
        int Strength { get; }
        
        IMapNode? Node { get; }
        IBuilding? Building { get; }
    }
    
    public interface IBuilding
    {
        bool IsBase { get; }
        int Health { get; }
        int Strength { get; }
        int Presence { get; }
        IEnumerable<Keyword> Keywords { get; }
        IEnumerable<ICard> Cards { get; }
        IMapNode? Node { get; }
        IEnumerable<IEnemy> EngagedEnemies { get; }
    }
    
    public interface ICard
    {
        string Name { get; }
        int Cost { get; }
        int Health { get; }
        int Strength { get; }
        int Presence { get; }
        IEnumerable<Keyword> Keywords { get; }
    }

    public enum Keyword
    {
        Draw,
        ClearCutting,
        Ranged,
        Repair,
        RaiseDead,
    }
}