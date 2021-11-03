using System;
using System.Collections.Generic;
using Archetype.Game.Payloads.Metadata;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Core
{
    public class CardProtoData : ICardProtoData
    {
        private readonly List<ITarget> _targets;
        private readonly List<IEffect> _effects;
        
        public CardProtoData(Guid id, CardMetaData metaData, List<ITarget> targets, List<IEffect> effects)
        {
            Id = id;
            MetaData = metaData;
            _targets = targets;
            _effects = effects;
        }

        public Guid Id { get; }
        public int Cost { get; set; }
        public CardMetaData MetaData { get; }
        public IEnumerable<ITarget> Targets => _targets;
        public IEnumerable<IEffect> Effects => _effects;
    }
}
