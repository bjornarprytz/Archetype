using System.Collections.Generic;
using Archetype.Game.Payloads.Context.Effect.Base;
using Archetype.View.Atoms.MetaData;
using Archetype.View.Infrastructure;
using Archetype.View.Proto;

namespace Archetype.Game.Payloads.Proto
{
    public interface ICardProtoData : ICardProtoDataFront
    {
        IEnumerable<IEffect> Effects { get; }
    }

    public class CardProtoData : ProtoData, ICardProtoData
    {
        private readonly List<ITargetDescriptor> _targets; // TODO: Fill this
        private readonly List<IEffect> _effects;

        public CardProtoData(List<ITargetDescriptor> targets, List<IEffect> effects)
        {
            _targets = targets;
            _effects = effects;
        }
        
        public int Cost { get; set; }
        public int Range { get; set; }
        public CardMetaData MetaData { get; set; }
        public IEnumerable<ITargetDescriptor> TargetDescriptors => _targets;
        public IEnumerable<IEffectDescriptor> EffectDescriptors { get; set; }
        public IEnumerable<IEffect> Effects => _effects;
    }
}