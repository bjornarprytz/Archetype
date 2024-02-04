using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Core.Structure;
using Archetype.Framework.Design;
using Archetype.Framework.State;
using Archetype.Tests.MockUtilities;
using FluentAssertions;
using NSubstitute;

namespace Archetype.Tests.Infrastructure;

[TestFixture]
public class ActionQueueTests
{
    private IResolutionContext _context = default!;
    private IEventBus _eventBus;
    private IRules _rules;
    private IEffectDefinition _primitiveDefinition;
    private IEffectDefinition _compositeDefinition;
    private ActionQueue _sut = null!;
    

    [SetUp]
    public void SetUp()
    {
        _context = Substitute.For<IResolutionContext>();
        _rules = Substitute.For<IRules>();
        
        _primitiveDefinition = Substitute.For<IEffectDefinition>();
        _primitiveDefinition.Resolve(default!, default!).ReturnsForAnyArgs(Substitute.For<IEffectResult>());
        _primitiveDefinition.Keyword.Returns("PrimitiveTestKeyword");
        _rules.GetDefinition("PrimitiveTestKeyword").Returns(_primitiveDefinition);
        
        _compositeDefinition = Substitute.For<IEffectDefinition>();
        _compositeDefinition.Resolve(default!, default!).ReturnsForAnyArgs(Substitute.For<IKeywordFrame>());
        _compositeDefinition.Keyword.Returns("CompositeTestKeyword");
        _rules.GetDefinition("CompositeTestKeyword").Returns(_compositeDefinition);
        
        _eventBus = Substitute.For<IEventBus>();
        
        _sut = new ActionQueue(_eventBus, _rules);
    }

    [Test]
    public void ResolveNextKeyword_WhenKeywordStackIsEmpty_ReturnsNull()
    {
        var result = _sut.ResolveNextKeyword();
        
        result.Should().BeNull();
    }
    
    // TODO: Add more tests
}
