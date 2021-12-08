using System;
using Archetype.Game.Payloads.MetaData;

namespace Archetype.Game.Payloads.Proto
{
    public interface IUnitProtoData
    {
        Guid Guid { get; }
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
        public StructureMetaData MetaData { get; set; }
        public override UnitMetaData BaseMetaData => MetaData;
    }
    
    public abstract class UnitProtoData : IUnitProtoData
    {
        protected UnitProtoData()
        {
            Guid = Guid.NewGuid();
        }
        
        public Guid Guid { get; }
        public int Health { get; set; }
        public int Defense { get; set; }
        public abstract UnitMetaData BaseMetaData { get; }
    }
}