using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Archetype.Game.Attributes;
using Archetype.Game.Factory;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Context.Effect;
using Archetype.Game.Payloads.Context.Effect.Base;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.MetaData;
using Archetype.Game.Payloads.Pieces.Base;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Pieces
{
    [Target("Card")]
    public interface ICard : IGameAtom, IZoned<ICard>
    {
        CardMetaData MetaData { get; }
        int Cost { get; }
        
        int Range { get; }
        
        [Template("Reduce cost of {0} by {1}")]
        IResult<ICard, int> ReduceCost(int x);
        
        IEnumerable<ITarget> Targets { get; }
        IEnumerable<IEffect<ICardContext>> Effects { get; }
    }
    
    public class Card : Piece<ICard>, ICard
    {
        private readonly List<ITarget> _targets;
        private readonly List<IEffect<ICardContext>> _effects;

        public Card(ICardProtoData protoData, IGameAtom owner) : base(owner)
        {
            Name = protoData.Name;
            _targets = protoData.Targets.ToList(); // TODO: Maybe just point to the protoData targets/effects?
            _effects = protoData.Effects.ToList();
            MetaData = protoData.MetaData;
            Cost = protoData.Cost;
            Range = protoData.Range;
        }

        public int Cost { get; private set; }
        public int Range { get; private set; }
        public CardMetaData MetaData { get; }
        public IEnumerable<ITarget> Targets => _targets;
        public IEnumerable<IEffect<ICardContext>> Effects => _effects;
        
        
        public IResult<ICard, int> ReduceCost(int x)
        {
            Console.WriteLine($"Reducing cost by {x}!");

            Cost -= x;

            return ResultFactory.Create(this, x);
        }

        public string GenerateRulesText(IGameState gameState)
        {
            var sb = new StringBuilder();
            
            var context = new MinimalContext(gameState, gameState.Player); 
            
            foreach (var effect in _effects)
            {
                sb.AppendLine(effect.ContextRulesText(context));
            }

            return sb.ToString();
        }

        protected override ICard Self => this;

        private record MinimalContext(IGameState GameState, IGameAtom Owner) : IContext
        {
            public IResolution PartialResults { get; } = new ResolutionCollector();
        }
    }
}