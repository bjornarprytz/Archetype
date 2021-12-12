using System;
using System.Collections.Generic;
using Archetype.Game.Payloads.Context.Effect;
using Archetype.Game.Payloads.Context.Trigger;
using Archetype.Game.Payloads.MetaData;
using Archetype.Game.Payloads.Pieces;

namespace Archetype.Game.Payloads.Proto
{
    public interface IUnitProtoData : IProtoData
    {
        int Health { get; }
        int Defense { get; }
        
        UnitMetaData BaseMetaData { get; }
    }
    
    public interface ICreatureProtoData : IUnitProtoData
    {
        int Movement { get; }
        int Strength { get; }

        CreatureMetaData MetaData { get; }
    }

    public interface IStructureProtoData : IUnitProtoData
    {
        StructureMetaData MetaData { get; }
        
        IEnumerable<IEffect<ITriggerContext<IStructure>>> Effects { get; }
    }
    
    public class CreatureProtoData : UnitProtoData, ICreatureProtoData
    {
        public int Movement { get; set; }
        public int Strength { get; set; }
        public CreatureMetaData MetaData { get; set; }
        public override UnitMetaData BaseMetaData => MetaData;
    }

    public class StructureProtoData : UnitProtoData, IStructureProtoData
    {
        private readonly List<IEffect<ITriggerContext<IStructure>>> _effects;

        public StructureProtoData(List<IEffect<ITriggerContext<IStructure>>> effects)
        {
            _effects = effects;
        }
        
        public StructureMetaData MetaData { get; set; }
        public override UnitMetaData BaseMetaData => MetaData;
        public IEnumerable<IEffect<ITriggerContext<IStructure>>> Effects => _effects;
    }
    
    public abstract class UnitProtoData : ProtoData, IUnitProtoData
    {
        public int Health { get; set; }
        public int Defense { get; set; }
        public abstract UnitMetaData BaseMetaData { get; }
    }
}