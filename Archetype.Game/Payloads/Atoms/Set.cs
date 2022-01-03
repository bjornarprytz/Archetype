using System.Collections.Generic;
using Archetype.Game.Payloads.Proto;
using Archetype.View.Infrastructure;
using Archetype.View.Proto;

namespace Archetype.Game.Payloads.Atoms
{
    public interface ISet : ISetFront
    {
        new IReadOnlyDictionary<string, ICardProtoData> Cards { get; }
        new IReadOnlyDictionary<string, ICreatureProtoData> Creatures { get; }
        new IReadOnlyDictionary<string, IStructureProtoData> Structures { get; }
    }

    public class Set : ISet
    {
        private readonly Dictionary<string, ICardProtoData> _cards;
        private readonly Dictionary<string, ICreatureProtoData> _creatures;
        private readonly Dictionary<string, IStructureProtoData> _structures;


        public Set(
            Dictionary<string, ICardProtoData> cards, 
            Dictionary<string, ICreatureProtoData> creatures, 
            Dictionary<string, IStructureProtoData> structures)
        {
            _cards = cards;
            _creatures = creatures;
            _structures = structures;
        }

        public string Name { get; set; }
        
        public IReadOnlyDictionary<string, ICardProtoData> Cards => _cards;
        public IReadOnlyDictionary<string, ICreatureProtoData> Creatures => _creatures;
        public IReadOnlyDictionary<string, IStructureProtoData> Structures => _structures;

        IEnumerable<ICreatureProtoDataFront> ISetFront.Creatures => _creatures.Values;
        IEnumerable<IStructureProtoDataFront> ISetFront.Structures => _structures.Values;
        IEnumerable<ICardProtoDataFront> ISetFront.Cards => _cards.Values;

    }
}