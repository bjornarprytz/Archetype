using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class ChaosBag
    {
        public Dictionary<Alignment, int> _chaosPool;
        public ChaosBag()
        {
            _chaosPool = new Dictionary<Alignment, int>();
        }

        public ChaosToken DrawToken()
        {
            var alignment = _chaosPool.Select(s => (s.Key, s.Value)).WeightedChoice();

            _chaosPool[alignment]--;

            return new ChaosToken(alignment);
        }

        public void AddToken(Alignment alignment)
        {
            if (!_chaosPool.ContainsKey(alignment)) _chaosPool.Add(alignment, 0);

            _chaosPool[alignment]++;
        }

        public void AddTokens(Alignment alignment, int n)
        {
            if (!_chaosPool.ContainsKey(alignment)) _chaosPool.Add(alignment, 0);

            _chaosPool[alignment] += n;
        }
    }
}
