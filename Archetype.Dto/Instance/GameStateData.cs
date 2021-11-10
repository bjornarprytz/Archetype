namespace Archetype.Dto.Instance
{
    public class GameStateData
    {
        public bool IsPlayerTurn { get; set; }
        public PlayerData Player { get; set; }
        
        public MapData Map { get; set; }
    }
}