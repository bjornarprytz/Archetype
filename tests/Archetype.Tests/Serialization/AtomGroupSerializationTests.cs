using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Archetype.Core;
using Xunit;

namespace Archetype.Tests.Serialization;

public class AtomGroupSerializationTests
{
    [Fact]
    public void AtomGroup_GameDefinition_RoundTripJson()
    {
        var atomGroup = new AtomGroupDef(
            Name: "g1",
            Kinds: new[] { AtomKind.Card },
            Matcher: new MatcherByName("che*", Regex: false),
            Transformations: new TransformationDef[] { new TransformSetStaticPropertyIfMissing("cost", 1.0) },
            Priority: 1,
            OverrideLocal: false,
            ApplyPhase: "PreBuild");

        var def = new GameDefinition(
            Keywords: BuiltInKeywords.All.ToDictionary(k => k.Name, k => k),
            CardDefinitions: new Dictionary<string, CardDefinition>(),
            ZoneDefinitions: new Dictionary<string, ZoneDefinition>(),
            StateBasedRules: new List<StateBasedRule>(),
            Phases: new List<PhaseDefinition> { new PhaseDefinition("main") },
            ActionRules: new Dictionary<string, IReadOnlyList<ActionRuleDefinition>>(),
            TriggerResolutionOrder: TriggerResolutionOrder.OldestFirst,
            PlayerDefinitions: new Dictionary<string, PlayerDefinition>(),
            InitManifest: InitManifest.Empty,
            PlayableZoneNames: null,
            Id: "test-game",
            SessionStateMapDeclarations: null,
            AtomGroups: new[] { atomGroup });

        var opts = GameDefinitionJsonOptions.Build();
        var json = JsonSerializer.Serialize(def, opts);
        var got = JsonSerializer.Deserialize<GameDefinition>(json, opts);
        Assert.NotNull(got);
        Assert.NotNull(got.AtomGroups);
        Assert.Single(got.AtomGroups);
        Assert.Equal("g1", got.AtomGroups[0].Name);
        Assert.IsType<MatcherByName>(got.AtomGroups[0].Matcher);
        var m = (MatcherByName)got.AtomGroups[0].Matcher;
        Assert.Equal("che*", m.Pattern);
        var t = Assert.Single(got.AtomGroups[0].Transformations);
        Assert.IsType<TransformSetStaticPropertyIfMissing>(t);
    }
}
