using System;
using System.Collections.Generic;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Context.Phase;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Infrastructure
{
    public interface IHistoryReader
    {
        IReadOnlyList<IHistoryEntry> Entries { get; }
        IReadOnlyDictionary<Guid, IList<ICardEntry>> CardEntriesByProtoGuid { get; }
        IReadOnlyDictionary<ICard, IList<ICardEntry>> CardEntriesByInstance { get; }
    }

    public interface IHistoryWriter
    {
        void Append(ICard card, ICardResolutionContext context, IResolution result);
        void Append(IPhaseResolver phase, IResolution result);
    }

    public interface IHistoryEntry
    {
        string Header { get; }
        IResolution Result { get; }
    }

    public interface ICardEntry : IHistoryEntry
    {
        ICard Card { get; init; }
        IPlayer Caster { get; init; }
        IEnumerable<IGameAtom> Targets { get; }
    }

    public interface IPhaseEntry : IHistoryEntry
    {
        
    }
    
    public class History : IHistoryReader, IHistoryWriter
    {
        private readonly List<IHistoryEntry> _entries = new();
        private readonly Dictionary<Guid, IList<ICardEntry>> _entriesByProtoGuid = new();
        private readonly Dictionary<ICard, IList<ICardEntry>> _entriesByCardInstance = new();

        public IReadOnlyList<IHistoryEntry> Entries => _entries;
        public IReadOnlyDictionary<Guid, IList<ICardEntry>> CardEntriesByProtoGuid => _entriesByProtoGuid;
        public IReadOnlyDictionary<ICard, IList<ICardEntry>> CardEntriesByInstance => _entriesByCardInstance;


        public void Append(ICard card, ICardResolutionContext context, IResolution result)
        {
            var newEntry = new CardEntry
            {
                Card = card,
                Caster = context.Caster,
                Targets = context.Targets,
                Result = result
            };
            
            _entries.Add(newEntry);
            (_entriesByProtoGuid[card.ProtoGuid] ??= new List<ICardEntry>()).Add(newEntry);
            (_entriesByCardInstance[card] ??= new List<ICardEntry>()).Add(newEntry);
        }

        public void Append(IPhaseResolver phase, IResolution result)
        {
            _entries.Add(
                new PhaseEntry(
                    phase.Name,
                    result
                ));
        }

        private record CardEntry : ICardEntry
        {
            internal CardEntry() { }
            public string Header => Card.MetaData.Name;
            public ICard Card { get; init; }
            public IPlayer Caster { get; init; }
            public IEnumerable<IGameAtom> Targets { get; init; }
            public IResolution Result { get; init; }
        }

        private record PhaseEntry(string Header, IResolution Result)
            : IPhaseEntry;
    }
}