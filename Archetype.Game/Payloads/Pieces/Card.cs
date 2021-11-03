using System;
using System.Collections.Generic;
using Archetype.Game.Payloads.Metadata;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Pieces
{
    public class Card : ICard
    {
        public Card(long id, ICardProtoData protoData, IGamePiece owner, IZone currentZone)
        {
            ProtoData = protoData;
            Id = id;
            Owner = owner;
            CurrentZone = currentZone;
        }
        
        public ICardProtoData ProtoData { get; }
        
        public IEnumerable<ITarget> Targets => ProtoData.Targets;
        public IEnumerable<IEffect> Effects => ProtoData.Effects;
        
        public long Id { get; }
        public IGamePiece Owner { get; }
        public IZone CurrentZone { get; } 

        public int Cost => ProtoData.Cost;

        public void AffectSomehow(int x)
        {
            Console.WriteLine($"Affecting card somehow! {x}");
        }
    }
}