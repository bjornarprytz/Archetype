using System;
using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Proto;
using Archetype.View;
using Archetype.View.Infrastructure;

namespace Archetype.Game.Payloads.Infrastructure
{
    public interface IProtoPool : IProtoPoolFront
    {
        void AddSet(ISet set);
        
        
        new IEnumerable<ISet> Sets { get; }
        IEnumerable<ICardProtoData> Cards { get; }
        IEnumerable<ICreatureProtoData> Creatures { get; }
        IEnumerable<IStructureProtoData> Structures { get; }

        ICardProtoData GetCard(string name);
        ICreatureProtoData GetCreature(string name);
        IStructureProtoData GetStructure(string name);
    }
    
    internal class ProtoPool : IProtoPool
    {
        private readonly List<ISet> _sets = new();
        
        public void AddSet(ISet set)
        {
            _sets.Add(set);
        }
        
        public IEnumerable<ICardProtoData> Cards => _sets.SelectMany(set => set.Cards.Values);
        public IEnumerable<ICreatureProtoData> Creatures => _sets.SelectMany(set => set.Creatures.Values);
        public IEnumerable<IStructureProtoData> Structures => _sets.SelectMany(set => set.Structures.Values);

        public IEnumerable<ISet> Sets => _sets ;
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

        IEnumerable<ISetFront> IProtoPoolFront.Sets => Sets;
    }
}