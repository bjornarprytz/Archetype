using System.Collections.Generic;
using Archetype.Game.Payloads.Context.Effect;
using Archetype.Game.Payloads.Context.Effect.Base;
using Archetype.Game.Payloads.Context.Trigger;
using Archetype.Game.Payloads.MetaData;
using Archetype.Game.Payloads.Pieces;

namespace Archetype.Game.Payloads.Proto
{

    public interface IUnitProtoDataFront
    {
        int Health { get; }
        int Defense { get; }
        
        UnitMetaData BaseMetaData { get; }
    }
    public interface ICreatureProtoDataFront
    {
        string RulesText { get; }
        int Movement { get; }
        int Strength { get; }

        CreatureMetaData MetaData { get; }
    }

    public interface IStructureProtoDataFront
    {
        StructureMetaData MetaData { get; }
        string RulesText { get; }
    }

    internal interface IUnitProtoData : IProtoData, IUnitProtoDataFront { }
    internal interface ICreatureProtoData : IUnitProtoData, ICreatureProtoDataFront { }

    internal interface IStructureProtoData : IUnitProtoData, IStructureProtoDataFront
    {
        IEnumerable<IEffect<ITriggerContext<IStructure>>> Effects { get; }
    }
    
    internal class CreatureProtoData : UnitProtoData, ICreatureProtoData
    {
        public string RulesText => "TODO: Generate rules text for creatures!";
        public int Movement { get; set; }
        public int Strength { get; set; }
        public CreatureMetaData MetaData { get; set; }
        public override UnitMetaData BaseMetaData => MetaData;
    }

    internal class StructureProtoData : UnitProtoData, IStructureProtoData
    {
        private readonly List<IEffect<ITriggerContext<IStructure>>> _effects;

        public StructureProtoData(List<IEffect<ITriggerContext<IStructure>>> effects)
        {
            _effects = effects;
        }
        
        public StructureMetaData MetaData { get; set; }
        public string RulesText => "TODO: Generate rules text for structures!";
        public override UnitMetaData BaseMetaData => MetaData;
        public IEnumerable<IEffect<ITriggerContext<IStructure>>> Effects => _effects;
    }
    
    internal abstract class UnitProtoData : ProtoData, IUnitProtoData
    {
        public int Health { get; set; }
        public int Defense { get; set; }
        public abstract UnitMetaData BaseMetaData { get; }
    }
}