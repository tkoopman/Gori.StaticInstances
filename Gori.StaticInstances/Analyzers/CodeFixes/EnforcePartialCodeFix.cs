using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Gori.StaticInstances.Analyzers.CodeFixes;

/// <summary>
/// Code fix for when partial keyword is missing from class using either
/// <see cref="SingletonAttribute"/> or <see cref="StaticInstanceAttribute"/>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EnforcePartialCodeFix))]
[Shared]
public sealed class EnforcePartialCodeFix : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = [DiagnosticDescriptors.NonPartialType.Id, "CS0260"];

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return;
        }

        // Only act when SGLE001 is present; for CS0260-only situations let the standard fix handle it
        if (!context.Diagnostics.Any(d => d.Id == DiagnosticDescriptors.NonPartialType.Id))
        {
            return;
        }

        Diagnostic diagnostic = context.Diagnostics.First(d => d.Id == DiagnosticDescriptors.NonPartialType.Id);
        TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the type declaration identified by the diagnostic
        SyntaxNode node = root.FindNode(diagnosticSpan);
        TypeDeclarationSyntax? typeDeclaration = node.FirstAncestorOrSelf<TypeDeclarationSyntax>();

        if (typeDeclaration == null)
        {
            return;
        }

        var codeAction = CodeAction.Create(
        title: "Make Singleton type partial",
        createChangedDocument: ct => MakeTypePartialAsync(context.Document, typeDeclaration, ct),
        equivalenceKey: "MakeTypePartial");

        context.RegisterCodeFix(codeAction, diagnostic);
    }

    private static async Task<Document> MakeTypePartialAsync(
        Document document,
        TypeDeclarationSyntax typeDeclaration,
        CancellationToken cancellationToken)
    {
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return document;
        }

        // Collect the target type and all non-partial ancestor type declarations
        var toFix = new List<TypeDeclarationSyntax>();
        TypeDeclarationSyntax? current = typeDeclaration;
        while (current != null)
        {
            if (!current.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                toFix.Add(current);
            }

            current = current.Parent as TypeDeclarationSyntax;
        }

        SyntaxNode newRoot = root.ReplaceNodes(toFix, (original, rewritten) =>
        {
            SyntaxToken partialToken = SyntaxFactory.Token(SyntaxKind.PartialKeyword)
                .WithTrailingTrivia(SyntaxFactory.Space);
            int typeKeywordIndex = original.Modifiers.Count;
            return rewritten.WithModifiers(original.Modifiers.Insert(typeKeywordIndex, partialToken));
        });

        return document.WithSyntaxRoot(newRoot);
    }
}
