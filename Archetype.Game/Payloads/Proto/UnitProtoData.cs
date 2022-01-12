using System.Collections.Generic;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Context.Effect;
using Archetype.Game.Payloads.Context.Effect.Base;
using Archetype.Game.Payloads.Context.Trigger;
using Archetype.View;
using Archetype.View.Atoms.MetaData;
using Archetype.View.Proto;

namespace Archetype.Game.Payloads.Proto
{
    public interface IUnitProtoData : IUnitProtoDataFront { }

    public interface ICreatureProtoData : IUnitProtoData, ICreatureProtoDataFront { }

    public interface IStructureProtoData : IUnitProtoData, IStructureProtoDataFront
    {
        IEnumerable<IEffect<ITriggerContext<IStructure>>> Effects { get; }
    }

    public class CreatureProtoData : UnitProtoData, ICreatureProtoData
    {
        public string RulesText => "TODO: Generate rules text for creatures!";
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
        public string RulesText => "TODO: Generate rules text for structures!";
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