using System.Collections.Immutable;

using Gori.StaticInstances.Analyzers;
using Gori.StaticInstances.Analyzers.CodeFixes;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Gori.StaticInstances.Testing;

public class EnforcePartialCodeFixTests
{
    /// <summary>
    /// Builds an <see cref="AdhocWorkspace"/> containing a single C# document, runs
    /// <see cref="SingletonAnalyzer"/> against it, applies the first
    /// <see cref="EnforcePartialCodeFix"/> action for SGLE001, and returns the resulting
    /// source text.
    /// </summary>
    private static async Task<string> ApplyCodeFixAsync(string source)
    {
        using var workspace = new AdhocWorkspace();

        var projectId  = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);

        // Build a solution with the required metadata references so the compilation succeeds.
        Solution solution = workspace.CurrentSolution
            .AddProject(projectId, "Test", "Test", LanguageNames.CSharp)
            .WithProjectCompilationOptions(
                projectId,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        foreach (MetadataReference reference in TestHelpers.GetBaseReferences())
        {
            solution = solution.AddMetadataReference(projectId, reference);
        }

        solution = solution.AddDocument(documentId, "Test.cs", source);
        Assert.True(workspace.TryApplyChanges(solution));

        Document document    = workspace.CurrentSolution.GetDocument(documentId)!;
        Compilation? compilation = await document.Project.GetCompilationAsync();
        Assert.NotNull(compilation);

        // Run the analyzer on the compilation that belongs to this workspace project so
        // that diagnostic SourceTree references match the document's SyntaxTree.
        CompilationWithAnalyzers withAnalyzers = compilation!.WithAnalyzers([new SingletonAnalyzer()]);
        ImmutableArray<Diagnostic> analyzerDiagnostics = await withAnalyzers.GetAnalyzerDiagnosticsAsync();

        Diagnostic? diagnostic = analyzerDiagnostics.FirstOrDefault(d => d.Id == "GSSI001");
        Assert.NotNull(diagnostic);

        // Register the code fix and capture the resulting CodeAction.
        CodeAction? action = null;
        var context = new CodeFixContext(
            document,
            diagnostic!,
            (a, _) => action = a,
            CancellationToken.None);

        await new EnforcePartialCodeFix().RegisterCodeFixesAsync(context);
        Assert.NotNull(action);

        // Apply the code action and retrieve the updated document text.
        ImmutableArray<CodeActionOperation> operations = await action!.GetOperationsAsync(CancellationToken.None);
        ApplyChangesOperation applyOp    = operations.OfType<ApplyChangesOperation>().Single();
        Document newDocument = applyOp.ChangedSolution.GetDocument(documentId)!;
        return (await newDocument.GetTextAsync()).ToString();
    }

    // -------------------------------------------------------------------------
    // Simple non-partial class
    // -------------------------------------------------------------------------

    [Fact]
    public async Task NonPartialClass_AddsPartialKeyword()
    {
        string source = """
        using Gori.StaticInstances;
        [Singleton]
        public class MyService { }
        """;

        string result = await ApplyCodeFixAsync(source);

        Assert.Contains("partial class MyService", result);
    }

    [Fact]
    public async Task NonPartialClass_PreservesExistingModifiers()
    {
        string source = """
        using Gori.StaticInstances;
        [Singleton]
        public sealed class MyService { }
        """;

        string result = await ApplyCodeFixAsync(source);

        // The fix inserts partial; existing modifiers must still be present.
        Assert.Contains("public", result);
        Assert.Contains("partial", result);
        Assert.Contains("class MyService", result);
    }

    // -------------------------------------------------------------------------
    // Non-partial containing type
    // -------------------------------------------------------------------------

    [Fact]
    public async Task NonPartialContainingType_AddsPartialToContainingType()
    {
        string source = """
        using Gori.StaticInstances;
        public class Outer
        {
            [Singleton]
            public partial class Inner { }
        }
        """;

        // The SGLE001 diagnostic is on Outer (non-partial containing type).
        string result = await ApplyCodeFixAsync(source);

        Assert.Contains("partial class Outer", result);
    }
}
