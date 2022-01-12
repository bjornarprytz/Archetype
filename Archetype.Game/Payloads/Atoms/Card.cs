using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Archetype.Game.Attributes;
using Archetype.Game.Factory;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Context.Effect.Base;
using Archetype.Game.Payloads.Proto;
using Archetype.View.Atoms;
using Archetype.View.Atoms.MetaData;
using Archetype.View.Infrastructure;

namespace Archetype.Game.Payloads.Atoms
{
    [Target("Card")]
    public interface ICard : 
        IZoned<ICard>, 
        ICardFront, 
        IEffectProvider
    {
        new IEnumerable<IEffect> Effects { get; }
        
        [Template("Reduce cost of {0} by {1}")]
        IResult<ICard, int> ReduceCost(int x);
    }

    internal class Card : Piece<ICard>, ICard
    {
        private readonly List<ITargetDescriptor> _targets;
        private readonly List<IEffect> _effects;

        public Card(ICardProtoData protoData, IGameAtom owner) : base(owner)
        {
            Name = protoData.Name;
            _targets = protoData.Targets.ToList();
            _effects = protoData.Effects.ToList();
            MetaData = protoData.MetaData;
            Cost = protoData.Cost;
            Range = protoData.Range;
        }

        public int Cost { get; private set; }
        public int Range { get; private set; }
        public string RulesText { get; } // TODO: Update this based on context
        public IEnumerable<ITargetDescriptor> Targets => _targets;
        
        public CardMetaData MetaData { get; }
        public IEnumerable<IEffect> Effects => _effects;
        
        
        public IResult<ICard, int> ReduceCost(int x)
        {
            Console.WriteLine($"Reducing cost by {x}!");

            Cost -= x;

            return ResultFactory.Create(this, x);
        }

        public string GenerateRulesText(IContext context)
        {
            var sb = new StringBuilder(); 
            
            foreach (var effect in _effects)
            {
                sb.AppendLine(effect.ContextRulesText(context));
            }

            return sb.ToString();
        }

        protected override ICard Self => this;
    }
}