using System.Collections.Generic;
using System.Text;
using Archetype.Game.Payloads.Context.Effect.Base;
using Archetype.View.Atoms.MetaData;
using Archetype.View.Infrastructure;
using Archetype.View.Proto;

namespace Archetype.Game.Payloads.Proto
{
    public interface ICardProtoData : ICardProtoDataFront
    {
        IEnumerable<ITargetDescriptor> Targets { get; }
        IEnumerable<IEffect> Effects { get; }
    }

    public class CardProtoData : ProtoData, ICardProtoData
    {
        private readonly List<ITargetDescriptor> _targets;
        private readonly List<IEffect> _effects;
        private string _rulesText;

        public CardProtoData(List<ITargetDescriptor> targets, List<IEffect> effects)
        {
            _targets = targets;
            _effects = effects;
        }

        public string RulesText
        {
            get
            {
                _rulesText ??= GenerateRulesText();

                return _rulesText;
            }
        }

        public int Cost { get; set; }
        public int Range { get; set; }
        public CardMetaData MetaData { get; set; }
        public IEnumerable<ITargetDescriptor> Targets => _targets;
        public IEnumerable<IEffect> Effects => _effects;

        private string GenerateRulesText()
        {
            var sb = new StringBuilder();

            foreach (var effect in _effects)
            {
                sb.AppendLine(effect.PrintedRulesText());
            }

            return sb.ToString();
        }
    }
}