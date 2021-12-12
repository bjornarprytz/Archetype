using System;
using System.Collections.Generic;
using Aqua.EnumerableExtensions;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Infrastructure
{
    public interface IInstanceFinder
    {
        T FindAtom<T>(Guid instanceGuid) where T : IGameAtom;
        IGameAtom FindAtom(Guid instanceGuid);
    }

    public interface IInstanceFactory
    {
        ICard CreateCard(string name, IGameAtom owner);
        IStructure CreateStructure(string name, IGameAtom owner);
        ICreature CreateCreature(string name, IGameAtom owner);
    }
    
    public class InstanceManager : IInstanceFinder, IInstanceFactory
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

        public ICard CreateCard(string name, IGameAtom owner)
        {
            var protoData = _protoPool.GetCard(name);

            if (protoData is null)
                throw new InvalidOperationException($"Could not find a {typeof(ICard)} named {name} in the pool");
            
            var card = new Card(protoData, owner);

            AddAtom(card);
            
            return card;
        }

        public IStructure CreateStructure(string name, IGameAtom owner)
        {
            var protoData = _protoPool.GetStructure(name);

            if (protoData is null)
                throw new InvalidOperationException($"Could not find a a {typeof(IStructure)} named {name} in the pool");
            
            var structure = new Structure(protoData, owner);

            AddAtom(structure);
            
            return structure;
        }

        public ICreature CreateCreature(string name, IGameAtom owner)
        {
            var protoData = _protoPool.GetCreature(name);

            if (protoData is null)
                throw new InvalidOperationException($"Could not find a {typeof(ICreature)} named {name} in the pool");
            
            var creature = new Creature(protoData, owner);

            AddAtom(creature);

            return creature;
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