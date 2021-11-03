using System;
using System.Collections.Generic;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Pieces
{
    public interface ICard : IGamePiece
    {
        ICardProtoData ProtoData { get; }
        int Cost { get; }
        void AffectSomehow(int x);
        
        IZone CurrentZone { get; set; }
        
        IEnumerable<ITarget> Targets { get; }
        IEnumerable<IEffect> Effects { get; }
    }
    
    public class Card : GamePiece, ICard
    {
        public Card(ICardProtoData protoData, IGamePiece owner) : base(owner)
        {
            ProtoData = protoData;
        }
        
        public ICardProtoData ProtoData { get; }
        
        public IEnumerable<ITarget> Targets => ProtoData.Targets;
        public IEnumerable<IEffect> Effects => ProtoData.Effects;
        
        public IZone CurrentZone { get; set; } 

        public int Cost => ProtoData.Cost;

        public void AffectSomehow(int x)
        {
            Console.WriteLine($"Affecting card somehow! {x}");
        }
    }
}