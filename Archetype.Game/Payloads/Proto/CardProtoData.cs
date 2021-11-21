using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Archetype.Dto.MetaData;
using Archetype.Game.Payloads.PlayContext;

namespace Archetype.Game.Payloads.Proto
{
    /*
     * Represents the concept of a card. These should be singular, and is the base from which we create instances
     */
    public interface ICardProtoData
    {
        Guid Guid { get; }
        
        string RulesText { get; }
        int Cost { get; }
        CardMetaData MetaData { get; }
        IEnumerable<ITarget> Targets { get; }
        IEnumerable<IEffect> Effects { get; }
    }
    
    public class CardProtoData : ICardProtoData
    {
        private readonly List<ITarget> _targets;
        private readonly List<IEffect> _effects;
        private string _rulesText;

        public CardProtoData(List<ITarget> targets, List<IEffect> effects)
        {
            Guid = Guid.NewGuid();
            _targets = targets;
            _effects = effects;
        }

        public Guid Guid { get; }

        public string RulesText
        {
            get
            {
                _rulesText ??= GenerateRulesText();

                return _rulesText;
            }  
        } 

        public int Cost { get; set; }
        public CardMetaData MetaData { get; set; }
        public IEnumerable<ITarget> Targets => _targets;
        public IEnumerable<IEffect> Effects => _effects;

        private string GenerateRulesText()
        {
            // TODO: Take target "tags" into account.
            
            var sb = new StringBuilder();
            
            foreach (var effect in _effects)
            {
                sb.Append(effect.PrintedRulesText());
            }

            return sb.ToString();
        }
    }
}
