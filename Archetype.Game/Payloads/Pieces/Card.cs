using System;
using System.Collections.Generic;
using System.Linq;
using Archetype.Core;
using Archetype.Game.Payloads.Metadata;

namespace Archetype.Game.Payloads.Pieces
{
    public class Card : ICard
    {
        private readonly CardData _data;
        public Card(CardData data)
        {
            _data = data;
        }

        public IList<ITarget> Targets { get; } = new List<ITarget>();
        public IList<IEffect> Effects { get; } = new List<IEffect>();

        public IZone CurrentZone { get; }
        public long OwnerId { get; }
        public long Id { get; }

        public int Cost => _data.Cost;

        public void AffectSomehow(int x)
        {
            Console.WriteLine($"Affecting card somehow! {x}");
        }
        
        public CardData CreateReadonlyData()
        {
            _data.Targets = Targets.Select(t => t.CreateReadOnlyData()).ToList();
            _data.Effects = Effects.Select(e => e.CreateReadOnlyData()).ToList();
            
            return _data;
        }
    }
}