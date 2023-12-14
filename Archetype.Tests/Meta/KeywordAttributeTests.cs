using System.Reflection;
using Archetype.Framework.BaseRules.Keywords.Primitive;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Meta;
using FluentAssertions;

namespace Archetype.Tests.Meta;

[TestFixture]
public class KeywordAttributeTests
{
    private List<Type> _keywordDefinitions = null!;
    
    [SetUp]
    public void SetUp()
    {
        _keywordDefinitions = typeof(Prompt).Assembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IKeywordDefinition)) && t is { IsAbstract: false, IsInterface: false })
            .ToList();
    }
    
    [Test]
    public void AllKeywordDefinitionsHaveKeywordAttribute()
    {
        var keywordDefinitionsWithoutKeywordAttribute = _keywordDefinitions
            .Where(t => t.GetCustomAttribute<KeywordAttribute>() == null)
            .ToList();
        
        keywordDefinitionsWithoutKeywordAttribute.Should().BeEmpty();
    }
    
    [Test]
    public void NoEmptyStringKeywords()
    {
        var emptyStringKeywords = _keywordDefinitions
            .Where(t => string.IsNullOrWhiteSpace(t.GetCustomAttribute<KeywordAttribute>()?.Keyword))
            .ToList();
        
        emptyStringKeywords.Should().BeEmpty();
    }
    
    [Test]
    public void NoDuplicateKeywords()
    {
        var duplicateKeywords = _keywordDefinitions
            .GroupBy(t => t.GetCustomAttribute<KeywordAttribute>()?.Keyword)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        
        duplicateKeywords.Should().BeEmpty();
    }
    
    [Test]
    public void OperandDeclarationMatches()
    {
        foreach (var definition in _keywordDefinitions)
        {
            var attributeOperandDeclaration = definition.GetCustomAttribute<KeywordAttribute>()?.Operands.GetType();
            
            attributeOperandDeclaration.Should().Be(definition);
        }
    }
}