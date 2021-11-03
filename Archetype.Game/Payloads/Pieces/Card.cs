using System;
using System.Collections.Generic;
using Archetype.Core;
using Archetype.Game.Payloads.Metadata;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Pieces
{
    public class Card : ICard
    {
        public Card(ICardProtoData protoData)
        {
            ProtoData = protoData;
        }
        
        public ICardProtoData ProtoData { get; }
        
        public IEnumerable<ITarget> Targets => ProtoData.Targets;
        public IEnumerable<IEffect> Effects => ProtoData.Effects;

        public IZone CurrentZone { get; } 
        public long OwnerId { get; }
        public long Id { get; }

        public int Cost => ProtoData.Cost;

        public void AffectSomehow(int x)
        {
            Console.WriteLine($"Affecting card somehow! {x}");
        }
    }
}