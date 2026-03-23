using Archetype.Core;

namespace Archetype.Tests.StateMap;

public class StateFieldDeclTests
{
    [Fact]
    public void StateFieldDecl_DefaultTextTemplate_IsNull()
    {
        var decl = new StateFieldDecl("health", StateFieldType.Number);
        Assert.Null(decl.TextTemplate);
    }

    [Fact]
    public void StateFieldDecl_Equality_SameName_SameType_Equal()
    {
        var a = new StateFieldDecl("health", StateFieldType.Number, "current health");
        var b = new StateFieldDecl("health", StateFieldType.Number, "current health");
        Assert.Equal(a, b);
    }

    [Fact]
    public void StateFieldDecl_Equality_DifferentType_NotEqual()
    {
        var a = new StateFieldDecl("poisoned", StateFieldType.Bool);
        var b = new StateFieldDecl("poisoned", StateFieldType.Number);
        Assert.NotEqual(a, b);
    }
}
