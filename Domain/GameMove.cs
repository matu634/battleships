using System.Collections.Generic;

namespace Domain
{
    public class GameMove
    {
        public int GameMoveId { get; set; }

        public Player Target { get; set; }
        public Tile Tile { get; set; }

        public GameMove(Player target, Tile tile)
        {
            Target = target;
            Tile = tile;
        }

        private GameMove()
        {
        }
    }
}