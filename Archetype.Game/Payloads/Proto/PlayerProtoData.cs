using System.Collections.Generic;
using Archetype.Game.Factory;
using Archetype.Game.Payloads.Context;

namespace Archetype.Game.Payloads.Proto
{
    public interface IPlayerData
    {
        int StartingResources { get; } 
        int MaxHandSize { get; }
        int MinDeckSize { get; }
        
        IStructureProtoData Headquarters { get; }
        IEnumerable<IStructureProtoData> StructurePool { get; }
        IEnumerable<ICardProtoData> CardPool { get; }
        IEnumerable<ICardProtoData> DeckList { get; set; }

        IResult<IStructureProtoData> SetHeadQuarters(IStructureProtoData newHq);
        IResult<ICardProtoData> AddToCardPool(ICardProtoData card);
        IResult<IStructureProtoData> AddToStructurePool(IStructureProtoData structure);
    }
    
    public class PlayerData : IPlayerData
    {
        private readonly List<ICardProtoData> _cardPool = new();
        private readonly List<IStructureProtoData> _structurePool = new();
        
        public int StartingResources { get; } = 100;
        public int MaxHandSize { get; } = 2;
        public int MinDeckSize { get; } = 4;
        
        public IStructureProtoData Headquarters { get; set; }
        public IEnumerable<IStructureProtoData> StructurePool => _structurePool;
        public IEnumerable<ICardProtoData> CardPool => _cardPool;
        public IEnumerable<ICardProtoData> DeckList { get; set; }

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