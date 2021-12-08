using System;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Extensions
{
    public static class ProtoExtensions
    {
        public static ICard CreateCard(this IProtoPool pool, Guid guid, IGameAtom owner)
        {
            var protoData = pool.GetCard(guid);

            return protoData is null 
                ? null 
                : new Card(protoData, owner);
        }
        
        public static IStructure CreateStructure(this IProtoPool pool, Guid guid, IGameAtom owner)
        {
            var protoData = pool.GetStructure(guid);

            return protoData is null 
                ? null 
                : new Structure(protoData, owner);
        }
        
        public static ICreature CreateCreature(this IProtoPool pool, Guid guid, IGameAtom owner)
        {
            var protoData = pool.GetCreature(guid);

            return protoData is null 
                ? null 
                : new Creature(protoData, owner);
        }
    }
}