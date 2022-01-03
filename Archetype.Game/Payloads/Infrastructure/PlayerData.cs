using System.Collections.Generic;
using Archetype.Game.Factory;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Proto;
using Archetype.View.Proto;

namespace Archetype.Game.Payloads.Infrastructure
{
    

    public interface IPlayerData : IPlayerDataFront
    {
        new IStructureProtoData Headquarters { get; }
        new IEnumerable<IStructureProtoData> StructurePool { get; }
        new IEnumerable<ICardProtoData> CardPool { get; }
        new IEnumerable<ICardProtoData> DeckList { get; }

        IResult<IStructureProtoData> SetHeadQuarters(IStructureProtoData newHq);
        IResult<ICardProtoData> AddToCardPool(ICardProtoData card);
        IResult<IStructureProtoData> AddToStructurePool(IStructureProtoData structure);
    }
    
    internal class PlayerData : IPlayerData
    {
        private readonly List<ICardProtoData> _cardPool = new();
        private readonly List<IStructureProtoData> _structurePool = new();
        
        public int StartingResources { get; } = 100;
        public int MaxHandSize { get; } = 2;
        public int MinDeckSize { get; } = 4;

        IStructureProtoDataFront IPlayerDataFront.Headquarters => Headquarters;
        IEnumerable<IStructureProtoDataFront> IPlayerDataFront.StructurePool => StructurePool;

        IEnumerable<ICardProtoDataFront> IPlayerDataFront.CardPool => CardPool;

        IEnumerable<ICardProtoDataFront> IPlayerDataFront.DeckList => DeckList;

        public IStructureProtoData Headquarters { get; set; }
        public IEnumerable<IStructureProtoData> StructurePool => _structurePool;
        public IEnumerable<ICardProtoData> CardPool => _cardPool;
        public IEnumerable<ICardProtoData> DeckList { get; }

        public IResult<IStructureProtoData> SetHeadQuarters(IStructureProtoData newHq)
        {
            Headquarters = newHq;
            
            return ResultFactory.Create(newHq);
        }
        
        public IResult<ICardProtoData> AddToCardPool(ICardProtoData card)
        {
            _cardPool.Add(card);
            
            return ResultFactory.Create(card);
        }
        
        public IResult<IStructureProtoData> AddToStructurePool(IStructureProtoData structure)
        {
            _structurePool.Add(structure);
            
            return ResultFactory.Create(structure);
        }
    }
}