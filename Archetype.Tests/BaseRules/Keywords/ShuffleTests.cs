using Archetype.Framework.BaseRules.Keywords.Primitive;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;

namespace Archetype.Tests.BaseRules;

[TestFixture]
public class ShuffleTests
{
        
        private Shuffle _sut = default!;
        
        private IOrderedZone _targetZone = default!;
        
        [SetUp]
        public void Setup()
        {
            _sut = new Shuffle();
            _targetZone = Substitute.For<IOrderedZone>();
        }
        
        [Test]
        public void ShouldHaveKeywordName()
        {
            _sut.Name.Should().Be("SHUFFLE");
        }
        
        [Test]
        public void ShouldShuffleZone()
        {
            // Arrange
            var payload = new EffectPayload(
                Guid.NewGuid(),
                Substitute.For<IAtom>(),
                _sut.Name,
                new object[] { },
                new [] { _targetZone }
            );
    
            // Act
            var result = _sut.Resolve(Substitute.For<IResolutionContext>(), payload);
    
            // Assert
            _targetZone.Received().Shuffle();
            result.Should().BeOfType<ShuffleEvent>();
            result.As<ShuffleEvent>().Zone.Should().Be(_targetZone);
            result.As<ShuffleEvent>().Source.Should().Be(payload.Source);
        }
}