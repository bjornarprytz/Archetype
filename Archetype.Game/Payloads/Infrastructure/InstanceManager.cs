using System;
using System.Collections.Generic;
using Aqua.EnumerableExtensions;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Infrastructure
{
    public interface IInstanceFinder
    {
        T FindAtom<T>(Guid instanceGuid) where T : IGameAtom;
        IGameAtom FindAtom(Guid instanceGuid);
    }

    public interface IInstanceFactory
    {
        IMapNode CreateMapNode(IMapNodeProtoData nodeData);
        ICard CreateCard(ICardProtoData cardData);
        ICard CreateCard(string name);
        IStructure CreateStructure(IStructureProtoData structureData);
        IStructure CreateStructure(string name);
        ICreature CreateCreature(ICreatureProtoData creatureData);
        ICreature CreateCreature(string name);
    }
    
    internal class InstanceManager : IInstanceFinder, IInstanceFactory
    {
        private readonly IGameState _gameState;
        private readonly IProtoPool _protoPool;

        private readonly Dictionary<Guid, IGameAtom> _atoms = new(); 

        public InstanceManager(IGameState gameState, IProtoPool protoPool)
        {
            _gameState = gameState;
            _protoPool = protoPool;

            RegisterKnownGameState();
        }

        public T FindAtom<T>(Guid instanceGuid) where T : IGameAtom
        {
            var atom = FindAtom(instanceGuid);
            
            if (atom is not T typedAtom)
                throw new InvalidOperationException(
                    $"Atom is of unexpected type. Expected: {typeof(T)}, Actual: {atom.GetType()}");

            return typedAtom;
        }

        public IGameAtom FindAtom(Guid instanceGuid)
        {
            var atom = _atoms[instanceGuid];

            if (atom is null)
                throw new InvalidOperationException($"Atom with guid {instanceGuid} could not be found");
            
            return atom;
        }

        public IMapNode CreateMapNode(IMapNodeProtoData nodeData)
        {
            var node = new MapNode(this);

            AddAtom(node);

            return node;
        }

        public ICard CreateCard(ICardProtoData cardData)
        {
            var card = new Card(cardData);

            AddAtom(card);
            
            return card;
        }

        public ICard CreateCard(string name)
        {
            var protoData = _protoPool.GetCard(name);

            if (protoData is null)
                throw new InvalidOperationException($"Could not find a {typeof(ICard)} named {name} in the pool");

            return CreateCard(protoData);
        }

        public IStructure CreateStructure(IStructureProtoData structureData)
        {
            var structure = new Structure(structureData);

            AddAtom(structure);
            
            return structure;
        }

        public IStructure CreateStructure(string name)
        {
            var protoData = _protoPool.GetStructure(name);

            if (protoData is null)
                throw new InvalidOperationException($"Could not find a a {typeof(IStructure)} named {name} in the pool");

            return CreateStructure(protoData);
        }

        public ICreature CreateCreature(ICreatureProtoData creatureData)
        {
            var creature = new Creature(creatureData);

            AddAtom(creature);

            return creature;
        }

        public ICreature CreateCreature(string name)
        {
            var protoData = _protoPool.GetCreature(name);

            if (protoData is null)
                throw new InvalidOperationException($"Could not find a {typeof(ICreature)} named {name} in the pool");

            return CreateCreature(protoData);
        }

        private void RegisterKnownGameState()
        {
            AddAtom(_gameState.Player);
            
            _gameState.Player.Hand.Contents.ForEach(AddAtom);
            _gameState.Player.Deck.Contents.ForEach(AddAtom);

            foreach (var node in _gameState.Map.Nodes)
            {
                AddAtom(node);
                
                node.Contents.ForEach(AddAtom);
                node.Graveyard.Contents.ForEach(AddAtom);
                node.DiscardPile.Contents.ForEach(AddAtom);
            }
        }

        private void AddAtom(IGameAtom atom) => _atoms.Add(atom.Guid, atom);
    }
}