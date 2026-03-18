using Archetype.Tooling.Server;

namespace Archetype.Tests.Tooling;

// ---------------------------------------------------------------------------
//  Validator tests (task 3.7)
// ---------------------------------------------------------------------------

public sealed class ValidatorTests
{
    // -----------------------------------------------------------------------
    //  3.7-V1  Validate_CleanProject_NoDiagnostics
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_CleanProject_NoDiagnostics()
    {
        var state = new ProjectState { Id = "clean" };
        // Add a valid keyword with a valid built-in body.
        state.Keywords["give-damage"] = new KeywordEntry
        {
            Name       = "give-damage",
            ReturnType = Archetype.Core.TypeName.Atom,
            Parameters = [new("target", Archetype.Core.TypeName.Card),
                          new("amount", Archetype.Core.TypeName.Number)],
            BodyDsl    = "modify-accumulator(target, \"damage\", amount)",
            BodyNode   = DslParser.Parse(
                "modify-accumulator(target, \"damage\", amount)").Node,
        };

        ReferenceGraph.Build(state);
        Validator.Validate(state);

        Assert.DoesNotContain(state.Diagnostics, d => d.Severity == "error");
    }

    // -----------------------------------------------------------------------
    //  3.7-V2  Validate_MissingTranslation_WarningSeverity
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_MissingTranslation_WarningSeverity()
    {
        var state = new ProjectState();
        state.Localization.SourceLanguage = "en";
        state.Localization.Strings["en"] = new Dictionary<string, string>
        {
            ["take-damage"] = "Deal {amount} damage",
        };
        state.Localization.Strings["fr"] = new Dictionary<string, string>(); // nothing translated

        ReferenceGraph.Build(state);
        Validator.Validate(state);

        // Must produce a warning (not an error) for the missing French translation.
        var warnings = state.Diagnostics.Where(d => d.Severity == "warning").ToList();
        Assert.NotEmpty(warnings);
        Assert.All(warnings, w => Assert.Equal("warning", w.Severity));
        Assert.All(warnings, w => Assert.Contains("fr", w.Message));
        // Missing translations must NOT produce errors.
        Assert.DoesNotContain(state.Diagnostics,
            d => d.Severity == "error" && d.Message.Contains("fr"));
    }

    // -----------------------------------------------------------------------
    //  3.7-V3  Validate_DuplicateKeywordName_ErrorDiagnostic
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_DuplicateKeywordName_ErrorDiagnostic()
    {
        // Game-creator keyword that shadows a built-in name is an error (D2).
        var state = new ProjectState();
        state.Keywords["modify-accumulator"] = new KeywordEntry
        {
            Name    = "modify-accumulator",
            BodyDsl = "",
        };

        ReferenceGraph.Build(state);
        Validator.Validate(state);

        var errors = state.Diagnostics.Where(d => d.Severity == "error").ToList();
        Assert.NotEmpty(errors);
        Assert.Contains(errors,
            e => e.EntryName == "modify-accumulator" &&
                 e.Message.Contains("conflicts with a built-in"));
    }

    // -----------------------------------------------------------------------
    //  BUG-FIX-1  Missing ReturnType → error diagnostic (D27)
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_KeywordMissingReturnType_ErrorDiagnostic()
    {
        // A keyword with no ReturnType must produce an error-severity diagnostic.
        var state = new ProjectState();
        state.Keywords["no-return"] = new KeywordEntry
        {
            Name       = "no-return",
            ReturnType = null, // explicitly absent
            BodyDsl    = "",
        };

        ReferenceGraph.Build(state);
        Validator.Validate(state);

        var errors = state.Diagnostics.Where(d => d.Severity == "error").ToList();
        Assert.Contains(errors,
            e => e.EntryName == "no-return" &&
                 e.Message.Contains("missing a return type declaration"));
    }

    // -----------------------------------------------------------------------
    //  BUG-FIX-1b  ReturnType present → no missing-return-type error (D27)
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_KeywordWithReturnType_NoReturnTypeError()
    {
        var state = new ProjectState();
        state.Keywords["has-return"] = new KeywordEntry
        {
            Name       = "has-return",
            ReturnType = Archetype.Core.TypeName.Number,
            BodyDsl    = "get-accumulator(has-return, \"score\")",
            BodyNode   = DslParser.Parse("get-accumulator(has-return, \"score\")").Node,
        };

        ReferenceGraph.Build(state);
        Validator.Validate(state);

        // Must not produce a missing-return-type error for this keyword.
        Assert.DoesNotContain(state.Diagnostics,
            d => d.EntryName == "has-return" &&
                 d.Message.Contains("missing a return type declaration"));
    }
}
