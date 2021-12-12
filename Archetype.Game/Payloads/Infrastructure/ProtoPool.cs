using System;
using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Infrastructure
{
    public interface IProtoPool
    {
        IEnumerable<ISet> Sets { get; }
        IEnumerable<ICardProtoData> Cards { get; }
        IEnumerable<ICreatureProtoData> Creatures { get; }
        IEnumerable<IStructureProtoData> Structures { get; }

        public ICardProtoData GetCard(string name);
        public ICreatureProtoData GetCreature(string name);
        public IStructureProtoData GetStructure(string name);
    }
    
    public class ProtoPool : IProtoPool
    {
        private readonly List<ISet> _sets;
        
        public ProtoPool(List<ISet> sets)
        {
            _sets = sets;
        }
        
        public IEnumerable<ICardProtoData> Cards => _sets.SelectMany(set => set.Cards.Values);
        public IEnumerable<ICreatureProtoData> Creatures => _sets.SelectMany(set => set.Creatures.Values);
        public IEnumerable<IStructureProtoData> Structures => _sets.SelectMany(set => set.Structures.Values);
        public IEnumerable<ISet> Sets => _sets;
        public ICardProtoData GetCard(string name)
        {
            return _sets.Where(set => set.Cards[name] != null).Select(set => set.Cards[name]).FirstOrDefault();
        }

        public ICreatureProtoData GetCreature(string name)
        {
            return _sets.Where(set => set.Creatures[name] != null).Select(set => set.Creatures[name]).FirstOrDefault();
        }

        public IStructureProtoData GetStructure(string name)
        {
            return _sets.Where(set => set.Structures[name] != null).Select(set => set.Structures[name]).FirstOrDefault();
        }
    }
}