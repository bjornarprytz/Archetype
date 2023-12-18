using System.Reflection;
using Archetype.Framework.BaseRules.Keywords.Primitive;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.Meta;
using FluentAssertions;

namespace Archetype.Tests.Meta;

[TestFixture]
public class KeywordTests
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
    public void NoEmptyStringKeywords()
    {
        var emptyStringKeywords = GetKeywordSyntaxAttributes()
            .Where(a => string.IsNullOrWhiteSpace(a!.Keyword))
            .ToList();
        
        emptyStringKeywords.Should().BeEmpty();
    }
    
    [Test]
    public void NoDuplicateKeywords()
    {
        var duplicateKeywords = _keywordDefinitions
            .Select(t => t.Construct<IKeywordDefinition>())
            .Where(d => d != null)
            .GroupBy(d => d!.Keyword)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        
        duplicateKeywords.Should().BeEmpty();
    }
    
    [Test]
    public void NoDuplicateSyntax()
    {
        var attributes = GetKeywordSyntaxAttributes().ToList();
        var duplicateKeywords = attributes
            .GroupBy(s => s.Keyword)
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
            if (definition.GetCustomAttribute<KeywordSyntaxAttribute>()?.Operands.GetType() 
                is not { } attributeOperandDeclaration)
                continue;
            
            var instance = definition.Construct<IKeywordDefinition>();
            
            instance.Should().NotBeNull();
            
            attributeOperandDeclaration.Should().Be(instance!.Operands.GetType());
        }
    }
    
    private IEnumerable<KeywordSyntaxAttribute> GetKeywordSyntaxAttributes()
    {
        return _keywordDefinitions
            .Select(t => t.GetCustomAttribute<KeywordSyntaxAttribute>())
            .Where(a => a != null)!;
    }
}