using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Archetype.Game.Attributes;
using Archetype.Game.Factory;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Context.Effect;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.MetaData;
using Archetype.Game.Payloads.Pieces.Base;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Pieces
{
    [Target("Card")]
    public interface ICard : IGameAtom, IZoned<ICard>
    {
        Guid ProtoGuid { get; }
        CardMetaData MetaData { get; }
        int Cost { get; }
        
        [Template("Reduce cost of {0}")]
        IEffectResult<ICard, int> ReduceCost(int x);
        
        IEnumerable<ITarget> Targets { get; }
        IEnumerable<IEffect> Effects { get; }
    }
    
    public class Card : Piece<ICard>, ICard
    {
        private readonly List<ITarget> _targets;
        private readonly List<IEffect> _effects;

        public Card(ICardProtoData protoData, IGameAtom owner) : base(owner)
        {
            ProtoGuid = protoData.Guid;
            _targets = protoData.Targets.ToList(); // TODO: Maybe just point to the protoData?
            _effects = protoData.Effects.ToList();
            MetaData = protoData.MetaData;
            Cost = protoData.Cost;
        }

        public int Cost { get; private set; }
        public Guid ProtoGuid { get; }
        public CardMetaData MetaData { get; }
        public IEnumerable<ITarget> Targets => _targets;
        public IEnumerable<IEffect> Effects => _effects;
        
        
        public IEffectResult<ICard, int> ReduceCost(int x)
        {
            Console.WriteLine($"Reducing cost by {x}!");

            Cost -= x;

            return ResultFactory.Create(this, x);
        }

        public string GenerateRulesText(IGameState gameState)
        {
            var sb = new StringBuilder();
            // TODO: figure something out here, to give the best card text possible
            var cardResolutionContext = new CardResolutionContext(gameState, gameState.Player, new History()); 
            
            foreach (var effect in _effects)
            {
                sb.AppendLine(effect.ContextSensitiveRulesText(cardResolutionContext));
            }

            return sb.ToString();
        }

        protected override ICard Self => this;
    }
}