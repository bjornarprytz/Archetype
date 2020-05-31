using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class Battlefield : Zone<Unit>
    {
        public IEnumerable<Unit> Allies => _units.Where(u => u.Team == Faction.Player);
        public IEnumerable<Unit> Enemies => _units.Where(u => u.Team == Faction.Enemy);
        private List<Unit> _units;

        public Battlefield()
        {
            _units = new List<Unit>();
        }

        public void Add(IEnumerable<Unit> units)
        {
            _units.AddRange(units);
        }

        public override IEnumerator<Unit> GetEnumerator() => _units.GetEnumerator();
    }
}
