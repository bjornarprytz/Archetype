using System;
using System.Collections.Generic;
using System.Linq;
using Archetype.Dto.MetaData;
using Archetype.Game.Payloads.Pieces.Base;
using Archetype.Game.Payloads.PlayContext;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Pieces
{
    public interface ICard : IGameAtom, IZoned<ICard>
    {
        CardMetaData MetaData { get; }
        int Cost { get; }
        void AffectSomehow(int x);
        
        IEnumerable<ITarget> Targets { get; }
        IEnumerable<IEffect> Effects { get; }
    }
    
    public class Card : Piece<ICard>, ICard
    {
        private readonly List<ITarget> _targets;
        private readonly List<IEffect> _effects;

        public Card(ICardProtoData protoData, IGameAtom owner) : base(owner)
        {
            _targets = protoData.Targets.ToList();
            _effects = protoData.Effects.ToList();
            MetaData = protoData.MetaData;
            Cost = protoData.Cost;
        }

        public int Cost { get; }
        public CardMetaData MetaData { get; }
        public IEnumerable<ITarget> Targets => _targets;
        public IEnumerable<IEffect> Effects => _effects;
        

        public void AffectSomehow(int x)
        {
            Console.WriteLine($"Affecting card somehow! {x}");
        }
    }
}