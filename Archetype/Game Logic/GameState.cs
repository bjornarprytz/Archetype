using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class GameState
    {
        Timeline TimeLine { get; set; }
        Player Player { get; set; }
        List<Enemy> Enemies { get; set; }

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

        private GameState()
        {
            TimeLine = new Timeline();
        }
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
