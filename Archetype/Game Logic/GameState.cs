using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class GameState
    {
        public Player Player { get; set; }
        public List<Enemy> Enemies { get; set; }

        public List<Unit> ActiveUnits
        {
            get
            {
                List<Unit> activeUnits = new List<Unit>();

                activeUnits.AddRange(Player.ActiveHeroes);
                activeUnits.AddRange(Enemies);

                return activeUnits;
            }
        }

        public bool Unresolved
        {
            get
            {
                bool defeat = Player.ActiveHeroes.All(u => !u.IsAlive);

                bool victory = Enemies.All(e => !e.IsAlive);

                return defeat || victory;
            }
        }

        private GameState() { }

        public static GameState InitiateBattle(Player player, List<Enemy> enemies)
        {
            return new GameState()
            {
                Player = player,
                Enemies = enemies,
            };
        }
    }
}
