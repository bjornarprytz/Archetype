using System;
using System.Collections.Generic;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Pieces
{
    public interface ISet
    {
        string Name { get; set; }
        IReadOnlyDictionary<Guid, ICardProtoData> Cards { get; }
        IReadOnlyDictionary<Guid, ICreatureProtoData> Creatures { get; }
        IReadOnlyDictionary<Guid, IStructureProtoData> Structures { get; }
    }
    
    public class Set : ISet
    {
        private readonly Dictionary<Guid, ICardProtoData> _cards;
        private readonly Dictionary<Guid, ICreatureProtoData> _creatures;
        private readonly Dictionary<Guid, IStructureProtoData> _structures;


        public Set(
            Dictionary<Guid, ICardProtoData> cards, 
            Dictionary<Guid, ICreatureProtoData> creatures, 
            Dictionary<Guid, IStructureProtoData> structures)
        {
            _cards = cards;
            _creatures = creatures;
            _structures = structures;
        }

        public string Name { get; set; }
        public IReadOnlyDictionary<Guid, ICardProtoData> Cards => _cards;
        public IReadOnlyDictionary<Guid, ICreatureProtoData> Creatures => _creatures;
        public IReadOnlyDictionary<Guid, IStructureProtoData> Structures => _structures;
    }
}