using System;
using System.Collections.Generic;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Infrastructure
{
    public interface IHistoryReader
    {
        IReadOnlyList<IHistoryEntry> Entries { get; }
        IReadOnlyDictionary<Guid, IList<IHistoryEntry>> EntriesByProtoGuid { get; }
        IReadOnlyDictionary<ICard, IList<IHistoryEntry>> EntriesByCardInstance { get; }
    }

    public interface IHistoryWriter
    {
        void Append(ICard card, ICardResolutionContext context, IResolution result);
    }

    public interface IHistoryEntry
    {
        ICard Card { get; }
        IGameAtom Caster { get; }
        IEnumerable<IGameAtom> Targets { get; }
        IResolution Result { get; }
    }
    
    public class History : IHistoryReader, IHistoryWriter
    {
        private readonly List<IHistoryEntry> _entries = new();
        private readonly Dictionary<Guid, IList<IHistoryEntry>> _entriesByProtoGuid = new();
        private readonly Dictionary<ICard, IList<IHistoryEntry>> _entriesByCardInstance = new();

        public IReadOnlyList<IHistoryEntry> Entries => _entries;
        public IReadOnlyDictionary<Guid, IList<IHistoryEntry>> EntriesByProtoGuid => _entriesByProtoGuid;
        public IReadOnlyDictionary<ICard, IList<IHistoryEntry>> EntriesByCardInstance => _entriesByCardInstance;


        public void Append(ICard card, ICardResolutionContext context, IResolution result)
        {
            var newEntry = new Entry
            {
                Card = card,
                Caster = context.Caster,
                Targets = context.Targets,
                Result = result
            };
            
            _entries.Add(newEntry);
            (_entriesByProtoGuid[card.ProtoGuid] ??= new List<IHistoryEntry>()).Add(newEntry);
            (_entriesByCardInstance[card] ??= new List<IHistoryEntry>()).Add(newEntry);
        }

        private record Entry : IHistoryEntry
        {
            internal Entry() { }
            
            public ICard Card { get; init; }
            public IGameAtom Caster { get; init; }
            public IEnumerable<IGameAtom> Targets { get; init; }
            public IResolution Result { get; init; }
        }
    }
}