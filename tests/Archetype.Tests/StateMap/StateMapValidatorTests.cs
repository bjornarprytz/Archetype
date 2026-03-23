using Archetype.Build;
using Archetype.Core;

namespace Archetype.Tests.StateMap;

/// <summary>
/// Tests for <see cref="StateMapValidator"/> integration through
/// <see cref="GameDefinitionBuilder.Build"/> (D38).
/// </summary>
public class StateMapValidatorTests
{
    private static readonly IReadOnlyDictionary<string, object> NoProps =
        new Dictionary<string, object>();

    // Helpers — build a keyword whose body uses a typed atom parameter so the
    // validator can statically determine the atom kind.

    private static GameDefinitionBuilder BuilderWithCardField(string fieldName, StateFieldType type)
    {
        var decls = new List<StateFieldDecl> { new(fieldName, type) };
        var card = new CardDefinition(
            Name: "TestCard",
            StaticProperties: NoProps,
            PrimaryEffect: EffectBlockDef.Empty,
            AdditionalEffects: [],
            StaticEffects: [],
            StateMapDeclarations: decls);
        return new GameDefinitionBuilder()
            .WithId("test")
            .WithInitManifest(InitManifest.Empty)
            .AddCard(card);
    }

    /// <summary>
    /// Registers a keyword with a Card-typed parameter and a body that calls
    /// <paramref name="kwName"/> on the card with the given field name.
    /// </summary>
    private static GameDefinitionBuilder WithCardKeyword(
        GameDefinitionBuilder builder,
        string kwName,
        string fieldName,
        params KeywordNode[] extraArgs)
    {
        var allArgs = new List<KeywordNode>
        {
            Kw.Param("target"),
            Kw.Str(fieldName),
        };
        allArgs.AddRange(extraArgs);

        return builder.RegisterKeyword(
            name: "test-kw",
            parameters: [new ParameterDecl("target", TypeName.Card)],
            body: new Invocation(kwName, allArgs.ToArray()));
    }

    // -----------------------------------------------------------------------
    //  modify-accumulator
    // -----------------------------------------------------------------------

    [Fact]
    public void ModifyAccumulator_UndeclaredField_ThrowsDefinitionException()
    {
        // Card has "mana" (Number) but keyword references "health" (not declared).
        var builder = BuilderWithCardField("mana", StateFieldType.Number);
        builder = WithCardKeyword(builder, "modify-accumulator", "health", Kw.Num(-1));
        Assert.Throws<DefinitionException>(() => builder.Build());
    }

    [Fact]
    public void ModifyAccumulator_DeclaredNumberField_Passes()
    {
        var builder = BuilderWithCardField("health", StateFieldType.Number);
        builder = WithCardKeyword(builder, "modify-accumulator", "health", Kw.Num(-1));
        var def = builder.Build(); // Should not throw.
        Assert.NotNull(def);
    }

    // -----------------------------------------------------------------------
    //  apply-condition
    // -----------------------------------------------------------------------

    [Fact]
    public void ApplyCondition_UndeclaredField_ThrowsDefinitionException()
    {
        var builder = BuilderWithCardField("poisoned", StateFieldType.Bool);
        // References "frozen" which is not declared.
        builder = WithCardKeyword(builder, "apply-condition", "frozen",
            new Invocation("session")); // lifetime placeholder
        Assert.Throws<DefinitionException>(() => builder.Build());
    }

    [Fact]
    public void ApplyCondition_DeclaredBoolField_Passes()
    {
        var builder = BuilderWithCardField("poisoned", StateFieldType.Bool);
        builder = WithCardKeyword(builder, "apply-condition", "poisoned",
            new Invocation("session")); // lifetime placeholder
        var def = builder.Build();
        Assert.NotNull(def);
    }

    [Fact]
    public void ApplyCondition_WrongType_NumberField_ThrowsDefinitionException()
    {
        // "health" is a Number field, but apply-condition requires Bool.
        var builder = BuilderWithCardField("health", StateFieldType.Number);
        builder = WithCardKeyword(builder, "apply-condition", "health",
            new Invocation("session"));
        Assert.Throws<DefinitionException>(() => builder.Build());
    }

    // -----------------------------------------------------------------------
    //  clear-accumulator
    // -----------------------------------------------------------------------

    [Fact]
    public void ClearAccumulator_UnknownKeyword_ThrowsDefinitionException()
    {
        // "clear-accumulator" is not in BuiltInKeywords.All. A keyword body that
        // invokes it is rejected by GameDefinitionBuilder.Build() with "invokes
        // unknown keyword" — this is the correct and expected failure mode.
        // The StateMapValidator is not reached in this case.
        var builder = new GameDefinitionBuilder()
            .WithId("test")
            .WithInitManifest(InitManifest.Empty)
            .RegisterKeyword(
                name: "test-kw",
                parameters: [new ParameterDecl("target", TypeName.Card)],
                body: new Invocation("clear-accumulator",
                    Kw.Param("target"), Kw.Str("health")));
        Assert.Throws<DefinitionException>(() => builder.Build());
    }

    // -----------------------------------------------------------------------
    //  get-state
    // -----------------------------------------------------------------------

    [Fact]
    public void GetState_DeclaredNumberField_Passes()
    {
        var builder = BuilderWithCardField("health", StateFieldType.Number);
        builder = WithCardKeyword(builder, "get-state", "health");
        var def = builder.Build();
        Assert.NotNull(def);
    }

    [Fact]
    public void GetState_DeclaredBoolField_Passes()
    {
        var builder = BuilderWithCardField("poisoned", StateFieldType.Bool);
        builder = WithCardKeyword(builder, "get-state", "poisoned");
        var def = builder.Build();
        Assert.NotNull(def);
    }

    [Fact]
    public void GetState_UndeclaredField_ThrowsDefinitionException()
    {
        var builder = BuilderWithCardField("mana", StateFieldType.Number);
        builder = WithCardKeyword(builder, "get-state", "health");
        Assert.Throws<DefinitionException>(() => builder.Build());
    }

    // -----------------------------------------------------------------------
    //  apply-modifier
    // -----------------------------------------------------------------------

    [Fact]
    public void ApplyModifier_UndeclaredField_ThrowsDefinitionException()
    {
        var builder = BuilderWithCardField("attack", StateFieldType.Number);
        // References "defense" which is not declared.
        builder = builder.RegisterKeyword(
            name: "test-kw",
            parameters: [new ParameterDecl("target", TypeName.Card)],
            body: new Invocation("apply-modifier",
                Kw.Param("target"),
                Kw.Str("defense"),  // undeclared
                Kw.Num(0),
                Kw.Num(1),
                new Invocation("session")));
        Assert.Throws<DefinitionException>(() => builder.Build());
    }

    // -----------------------------------------------------------------------
    //  remove-condition
    // -----------------------------------------------------------------------

    [Fact]
    public void RemoveCondition_UndeclaredField_ThrowsDefinitionException()
    {
        var builder = BuilderWithCardField("poisoned", StateFieldType.Bool);
        // References "frozen" which is not declared.
        builder = builder.RegisterKeyword(
            name: "test-kw",
            parameters: [new ParameterDecl("target", TypeName.Card)],
            body: new Invocation("remove-condition",
                Kw.Param("target"),
                Kw.Str("frozen")));  // undeclared
        Assert.Throws<DefinitionException>(() => builder.Build());
    }

    // -----------------------------------------------------------------------
    //  Skip conditions
    // -----------------------------------------------------------------------

    [Fact]
    public void Validation_GenericAtomParam_Skipped()
    {
        // TypeName.Atom is generic — cannot statically determine atom kind → skip.
        var builder = new GameDefinitionBuilder()
            .WithId("test")
            .WithInitManifest(InitManifest.Empty)
            .RegisterKeyword(
                name: "test-kw",
                parameters: [new ParameterDecl("target", TypeName.Atom)],
                body: new Invocation("modify-accumulator",
                    Kw.Param("target"),
                    Kw.Str("anything"),  // not declared on any specific kind
                    Kw.Num(1)));
        // Should NOT throw — atom kind is not statically determinable.
        var def = builder.Build();
        Assert.NotNull(def);
    }

    [Fact]
    public void Validation_NonLiteralFieldArg_Skipped()
    {
        // The field name argument is a ParameterRef, not a string literal → skip.
        var builder = new GameDefinitionBuilder()
            .WithId("test")
            .WithInitManifest(InitManifest.Empty)
            .RegisterKeyword(
                name: "test-kw",
                parameters: [
                    new ParameterDecl("target", TypeName.Card),
                    new ParameterDecl("field", TypeName.PropertyName),
                ],
                body: new Invocation("modify-accumulator",
                    Kw.Param("target"),
                    Kw.Param("field"),  // parameter ref — not a literal
                    Kw.Num(1)));
        // Should NOT throw — field name is not a string literal.
        var def = builder.Build();
        Assert.NotNull(def);
    }

    // -----------------------------------------------------------------------
    //  Session reserved fields
    // -----------------------------------------------------------------------

    [Fact]
    public void Validation_SessionReservedField_TurnNumber_Passes()
    {
        // turn-number is implicitly declared on the session atom.
        var builder = new GameDefinitionBuilder()
            .WithId("test")
            .WithInitManifest(InitManifest.Empty)
            .RegisterKeyword(
                name: "test-kw",
                parameters: [],
                body: new Invocation("get-state",
                    new Invocation("session"),
                    Kw.Str("turn-number")));
        var def = builder.Build();
        Assert.NotNull(def);
    }

    [Fact]
    public void Validation_SessionReservedField_PhaseIndex_Passes()
    {
        // phase-index is implicitly declared on the session atom.
        var builder = new GameDefinitionBuilder()
            .WithId("test")
            .WithInitManifest(InitManifest.Empty)
            .RegisterKeyword(
                name: "test-kw",
                parameters: [],
                body: new Invocation("get-state",
                    new Invocation("session"),
                    Kw.Str("phase-index")));
        var def = builder.Build();
        Assert.NotNull(def);
    }
}
