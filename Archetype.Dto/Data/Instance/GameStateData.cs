namespace Archetype.Core.Data.Instance
{
    public class GameStateData
    {
        public bool IsPlayerTurn { get; set; }
        public PlayerData Player { get; set; }
        
        public MapData Map { get; set; }
    }
}