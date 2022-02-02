using System;
using System.Collections.Generic;
using Archetype.Builder.Builders.Base;
using Archetype.Builder.Exceptions;
using Archetype.Builder.Extensions;
using Archetype.Builder.Factory;
using Archetype.Core.Atoms;
using Archetype.Core.Proto;
using Archetype.View.Atoms.MetaData;

namespace Archetype.Builder.Builders;

public interface ISetBuilder : IBuilder<ISet>
{
    ISetBuilder Name(string name);
    public ISetBuilder ChangeCardTemplate(Func<CardMetaData, CardMetaData> changeFunc);
    public ISetBuilder ChangeCreatureTemplate(Func<CreatureMetaData, CreatureMetaData> changeFunc);
    public ISetBuilder ChangeStructureTemplate(Func<StructureMetaData, StructureMetaData> changeFunc);
    public ISetBuilder Card(Action<ICardBuilder> builderProvider);
    public ISetBuilder Creature(Action<ICreatureBuilder> builderProvider);
    public ISetBuilder Structure(Action<IStructureBuilder> builderProvider);
}

internal class SetBuilder : ISetBuilder
{
    private readonly IFactory<IStructureBuilder> _structureBuilderFactory;
    private readonly IFactory<ICreatureBuilder> _creatureBuilderFactory;
    private readonly IFactory<ICardBuilder> _cardBuilderFactory;

    private readonly ISet _setData;
    private readonly Dictionary<string, ICardProtoData> _cards = new();
    private readonly Dictionary<string, ICreatureProtoData> _creatures = new();
    private readonly Dictionary<string, IStructureProtoData> _structures = new();

    private CardMetaData _cardTemplate = new();
    private StructureMetaData _structureTemplate = new();
    private CreatureMetaData _creatureTemplate = new();

    public SetBuilder(
        IFactory<IStructureBuilder> structureBuilderFactory,
        IFactory<ICreatureBuilder> creatureBuilderFactory,
        IFactory<ICardBuilder> cardBuilderFactory
    )
    {
        _structureBuilderFactory = structureBuilderFactory;
        _creatureBuilderFactory = creatureBuilderFactory;
        _cardBuilderFactory = cardBuilderFactory;
        _setData = new Set(_cards, _creatures, _structures);
    }

    public ISetBuilder Name(string name)
    {
        _setData.Name = name;

        _cardTemplate = _cardTemplate with { SetName = name };
        _structureTemplate = _structureTemplate with { SetName = name };
        _creatureTemplate = _creatureTemplate with { SetName = name };

        return this;
    }

    public ISetBuilder ChangeCardTemplate(Func<CardMetaData, CardMetaData> changeFunc)
    {
        _cardTemplate = changeFunc(_cardTemplate) with { SetName = _setData.Name };

        return this;
    }
        
    public ISetBuilder ChangeCreatureTemplate(Func<CreatureMetaData, CreatureMetaData> changeFunc)
    {
        _creatureTemplate = changeFunc(_creatureTemplate) with { SetName = _setData.Name };

        return this;
    }
        
    public ISetBuilder ChangeStructureTemplate(Func<StructureMetaData, StructureMetaData> changeFunc)
    {
        _structureTemplate = changeFunc(_structureTemplate) with { SetName = _setData.Name };

        return this;
    }

    public ISetBuilder Card(Action<ICardBuilder> builderProvider)
    {
        if (_setData.Name.IsMissing())
            throw new InvalidOperationException("Set name before building cards.");

        var cbc = 
            _cardBuilderFactory
                .Create()
                .MetaData(_cardTemplate);

        builderProvider(cbc);

        var card = cbc.Build();
            
        _cards.Add(card.Name, card);

        return this;
    }

    public ISetBuilder Creature(Action<ICreatureBuilder> builderProvider)
    {
        if (_setData.Name.IsMissing())
            throw new InvalidOperationException("Set name before building creatures.");
            
        var cbc = 
            _creatureBuilderFactory
                .Create()
                .MetaData(_creatureTemplate);

        builderProvider(cbc);

        var creature = cbc.Build();
            
        _creatures.Add(creature.Name, creature);

        return this;
    }
        
    public ISetBuilder Structure(Action<IStructureBuilder> builderProvider)
    {
        if (_setData.Name.IsMissing())
            throw new InvalidOperationException("Set name before building structures.");
            
        var cbc = 
            _structureBuilderFactory
                .Create()
                .MetaData(_structureTemplate);

        builderProvider(cbc);

        var structure = cbc.Build();
            
        _structures.Add(structure.Name, structure);

        return this;
    }

    public ISet Build()
    {
        if (_setData.Name.IsMissing())
            throw new MissingNameException();
            
        Console.WriteLine($"Created set {_setData.Name} with {_cards.Count} cards");

        return _setData;
    }
}