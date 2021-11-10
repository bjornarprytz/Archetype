using System;
using System.Collections.Generic;
using Archetype.Dto.MetaData;
using Archetype.Game.Payloads.PlayContext;

namespace Archetype.Game.Payloads.Proto
{
    public interface ICardProtoData
    {
        Guid Guid { get; }
        
        int Cost { get; }
        CardMetaData MetaData { get; }
        IEnumerable<ITarget> Targets { get; }
        IEnumerable<IEffect> Effects { get; }
    }
    
    public class CardProtoData : ICardProtoData
    {
        private readonly List<ITarget> _targets;
        private readonly List<IEffect> _effects;
        
        public CardProtoData(List<ITarget> targets, List<IEffect> effects)
        {
            Guid = Guid.NewGuid();
            _targets = targets;
            _effects = effects;
        }

        public Guid Guid { get; }
        public int Cost { get; set; }
        public CardMetaData MetaData { get; set; }
        public IEnumerable<ITarget> Targets => _targets;
        public IEnumerable<IEffect> Effects => _effects;
    }
}
