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
using Microsoft.CodeAnalysis.Formatting;
using WindowsFormsLifetime.Analyzers;

namespace WindowsFormsLifetime.CodeFixes;

/// <summary>
/// Adds or corrects runtime-only designer metadata on injected service properties.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InjectServiceCodeFixProvider)), Shared]
public sealed class InjectServiceCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(InjectServiceAnalyzer.BrowsableId, InjectServiceAnalyzer.SerializationId);

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        SemanticModel? semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null || semanticModel == null
            || GeneratedCode.IsGenerated(root.SyntaxTree, context.CancellationToken)
            || (context.Document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(root.SyntaxTree)
                .TryGetValue("generated_code", out string? generated) && generated == "true"))
        {
            return;
        }

        foreach (Diagnostic diagnostic in context.Diagnostics)
        {
            if (!FixableDiagnosticIds.Contains(diagnostic.Id))
            {
                continue;
            }

            PropertyDeclarationSyntax? property = root.FindNode(diagnostic.Location.SourceSpan)
                .FirstAncestorOrSelf<PropertyDeclarationSyntax>();
            INamedTypeSymbol? attributeType = semanticModel.Compilation.GetTypeByMetadataName(GetAttributeName(diagnostic.Id));
            if (property == null || !(semanticModel.GetDeclaredSymbol(property, context.CancellationToken) is IPropertySymbol symbol)
                || GeneratedCode.IsGenerated(symbol, semanticModel.Compilation)
                || attributeType == null)
            {
                continue;
            }

            AttributeData? existing = symbol.GetAttributes().FirstOrDefault(
                attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeType));
            if (existing?.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).ContainsDirectives == true)
            {
                continue;
            }

            string title = diagnostic.Id == InjectServiceAnalyzer.BrowsableId
                ? "Hide the injected property with Browsable(false)"
                : "Hide the injected property from designer serialization";
            context.RegisterCodeFix(
                CodeAction.Create(title,
                    cancellationToken => FixAsync(context.Document, property, diagnostic.Id, cancellationToken),
                    equivalenceKey: diagnostic.Id),
                diagnostic);
        }
    }

    private static async Task<Document> FixAsync(
        Document document, PropertyDeclarationSyntax property, string diagnosticId, CancellationToken cancellationToken)
    {
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        SemanticModel? semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (root == null || semanticModel == null
            || !(semanticModel.GetDeclaredSymbol(property, cancellationToken) is IPropertySymbol symbol))
        {
            return document;
        }

        INamedTypeSymbol? attributeType = semanticModel.Compilation.GetTypeByMetadataName(GetAttributeName(diagnosticId));
        AttributeData? existing = symbol.GetAttributes().FirstOrDefault(
            attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeType));
        AttributeSyntax? attributeSyntax = existing?.ApplicationSyntaxReference?.GetSyntax(cancellationToken) as AttributeSyntax;
        ExpressionSyntax value = diagnosticId == InjectServiceAnalyzer.BrowsableId
            ? SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression)
            : SyntaxFactory.ParseExpression("global::System.ComponentModel.DesignerSerializationVisibility.Hidden");

        if (attributeSyntax != null && attributeSyntax.SyntaxTree == property.SyntaxTree)
        {
            AttributeArgumentSyntax? argument = attributeSyntax.ArgumentList?.Arguments.FirstOrDefault(
                candidate => candidate.NameEquals == null);
            AttributeSyntax replacement;
            if (argument == null)
            {
                AttributeArgumentListSyntax arguments = attributeSyntax.ArgumentList ?? SyntaxFactory.AttributeArgumentList();
                replacement = attributeSyntax.WithArgumentList(arguments.WithArguments(
                    arguments.Arguments.Insert(0, SyntaxFactory.AttributeArgument(value))));
            }
            else
            {
                bool keepsExpressionTrivia = false;
                if (diagnosticId == InjectServiceAnalyzer.SerializationId
                    && argument.Expression is MemberAccessExpressionSyntax memberAccess
                    && SymbolEqualityComparer.Default.Equals(
                        semanticModel.GetSymbolInfo(memberAccess.Expression, cancellationToken).Symbol,
                        semanticModel.Compilation.GetTypeByMetadataName("System.ComponentModel.DesignerSerializationVisibility")))
                {
                    value = memberAccess.WithName(SyntaxFactory.IdentifierName("Hidden").WithTriviaFrom(memberAccess.Name));
                    keepsExpressionTrivia = true;
                }

                value = keepsExpressionTrivia
                    ? value.WithTriviaFrom(argument.Expression)
                    : PreserveComments(value, argument.Expression);
                replacement = attributeSyntax.ReplaceNode(argument.Expression, value);
            }

            return document.WithSyntaxRoot(root.ReplaceNode(attributeSyntax, replacement));
        }

        AttributeSyntax attributeToAdd = SyntaxFactory.Attribute(SyntaxFactory.ParseName("global::" + GetAttributeName(diagnosticId)))
            .WithArgumentList(SyntaxFactory.AttributeArgumentList(
                SyntaxFactory.SingletonSeparatedList(SyntaxFactory.AttributeArgument(value))));
        AttributeListSyntax list = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attributeToAdd))
            .WithAdditionalAnnotations(Formatter.Annotation);
        PropertyDeclarationSyntax updated = property.AddAttributeLists(list);
        return document.WithSyntaxRoot(root.ReplaceNode(property, updated));
    }

    private static string GetAttributeName(string diagnosticId) => diagnosticId == InjectServiceAnalyzer.BrowsableId
        ? "System.ComponentModel.BrowsableAttribute"
        : "System.ComponentModel.DesignerSerializationVisibilityAttribute";

    private static ExpressionSyntax PreserveComments(ExpressionSyntax value, ExpressionSyntax original)
    {
        SyntaxTriviaList interior = SyntaxFactory.TriviaList(original.DescendantTrivia().Where(
            trivia => trivia.SpanStart >= original.SpanStart && trivia.Span.End <= original.Span.End));
        if (interior.Any(trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)))
        {
            return value.WithLeadingTrivia(original.GetLeadingTrivia().AddRange(interior))
                .WithTrailingTrivia(original.GetTrailingTrivia());
        }

        return value.WithTriviaFrom(original);
    }
}
