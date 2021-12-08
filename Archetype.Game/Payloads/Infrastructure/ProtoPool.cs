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

        public ICardProtoData GetCard(Guid guid);
        public ICreatureProtoData GetCreature(Guid guid);
        public IStructureProtoData GetStructure(Guid guid);
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
        public ICardProtoData GetCard(Guid guid)
        {
            return _sets.Where(set => set.Cards[guid] != null).Select(set => set.Cards[guid]).FirstOrDefault();
        }

        public ICreatureProtoData GetCreature(Guid guid)
        {
            return _sets.Where(set => set.Creatures[guid] != null).Select(set => set.Creatures[guid]).FirstOrDefault();
        }

        public IStructureProtoData GetStructure(Guid guid)
        {
            return _sets.Where(set => set.Structures[guid] != null).Select(set => set.Structures[guid]).FirstOrDefault();
        }
    }
}