using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class Decision
    {
        public bool Aborted { get; set; }
        public List<GamePiece> ChosenPieces { get; private set; }

        public Decision()
        {
            ChosenPieces = new List<GamePiece>();
        }
    }
}
