using System.Collections.Generic;
using System.Text;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Context.Effect.Base;
using Archetype.View;
using Archetype.View.Atoms.MetaData;
using Archetype.View.Proto;

namespace Archetype.Game.Payloads.Proto
{
    public interface ICardProtoData : ICardProtoDataFront
    {
        IEnumerable<ITarget> Targets { get; }
        IEnumerable<IEffect<ICardContext>> Effects { get; }
    }

    public class CardProtoData : ProtoData, ICardProtoData
    {
        private readonly List<ITarget> _targets;
        private readonly List<IEffect<ICardContext>> _effects;
        private string _rulesText;

        public CardProtoData(List<ITarget> targets, List<IEffect<ICardContext>> effects)
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
        public IEnumerable<ITarget> Targets => _targets;
        public IEnumerable<IEffect<ICardContext>> Effects => _effects;

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