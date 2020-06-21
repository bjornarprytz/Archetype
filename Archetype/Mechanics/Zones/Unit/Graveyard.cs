
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class Graveyard : Zone<Unit>
    {
        public IEnumerable<Unit> Allies => _units.Where(u => u.Team == Faction.Player);
        public IEnumerable<Unit> Enemies => _units.Where(u => u.Team == Faction.Enemy);
        private List<Unit> _units;

        public Graveyard()
        {
            _units = new List<Unit>();
        }

        public override IEnumerator<Unit> GetEnumerator() => _units.GetEnumerator();

        protected override void InsertInternal(Unit pieceToMove)
        {
            _units.Add(pieceToMove);
        }

        protected override void EjectInternal(Unit pieceToEject)
        {
            _units.Remove(pieceToEject);
        }
    }
}
