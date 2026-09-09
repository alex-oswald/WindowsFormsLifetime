using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using WindowsFormsLifetime.Analyzers;
using WindowsFormsLifetime.CodeFixes;
using Xunit;

namespace WindowsFormsLifetime.Analyzers.Tests;

internal sealed class AnalyzerTest : IDisposable
{
    internal const string Contracts = """
        namespace WindowsFormsLifetime
        {
            [System.AttributeUsage(System.AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
            public sealed class InjectServiceAttribute : System.Attribute { }

            public interface IOnServicesInjected
            {
                void OnServicesInjected();
            }
        }

        namespace System.Windows.Forms
        {
            public class UserControl { }
        }
        """;

    private static readonly ImmutableArray<MetadataReference> References =
        ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
        .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path)).ToImmutableArray();

    private readonly AdhocWorkspace _workspace = new AdhocWorkspace();
    private readonly List<ExpectedDiagnostic> _expected = new List<ExpectedDiagnostic>();
    private readonly ProjectId _projectId;
    private readonly string? _sourceRoot;

    internal AnalyzerTest(
        (string Path, string Source)[] sources,
        bool includeContracts = true,
        CSharpCompilationOptions? compilationOptions = null,
        IEnumerable<MetadataReference>? additionalReferences = null,
        string? editorConfig = null)
    {
        _projectId = ProjectId.CreateNewId();
        _sourceRoot = editorConfig == null ? null : Path.GetFullPath("AnalyzerConsumer");
        Solution solution = _workspace.CurrentSolution.AddProject(_projectId, "Consumer", "Consumer", LanguageNames.CSharp)
            .WithProjectParseOptions(_projectId, new CSharpParseOptions(LanguageVersion.CSharp12))
            .WithProjectCompilationOptions(_projectId, compilationOptions ?? new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .WithProjectMetadataReferences(_projectId, References.Concat(additionalReferences ?? Array.Empty<MetadataReference>()));

        if (includeContracts)
        {
            solution = solution.AddDocument(DocumentId.CreateNewId(_projectId), "Contracts.cs", Contracts, filePath: SourcePath("Contracts.cs"));
        }

        if (editorConfig != null)
        {
            solution = solution.AddAnalyzerConfigDocument(DocumentId.CreateNewId(_projectId), ".editorconfig",
                SourceText.From(editorConfig), filePath: SourcePath(".editorconfig"));
        }

        foreach ((string path, string markedSource) in sources)
        {
            string source = Parse(markedSource, path);
            solution = solution.AddDocument(DocumentId.CreateNewId(_projectId), path, source, filePath: SourcePath(path));
        }

        Assert.True(_workspace.TryApplyChanges(solution));
    }

    internal Project Project => _workspace.CurrentSolution.GetProject(_projectId)!;

    internal Document Document(string path = "Control.cs") => Project.Documents.Single(document => document.Name == path);

    internal async Task<ImmutableArray<Diagnostic>> DiagnosticsAsync(bool allowCompilerErrors = false)
    {
        Compilation compilation = (await Project.GetCompilationAsync())!;
        if (!allowCompilerErrors)
        {
            Assert.Empty(compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
        }

        return await compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new InjectServiceAnalyzer()), Project.AnalyzerOptions)
            .GetAnalyzerDiagnosticsAsync();
    }

    internal async Task VerifyAsync(bool allowCompilerErrors = false)
    {
        ImmutableArray<Diagnostic> diagnostics = await DiagnosticsAsync(allowCompilerErrors);
        Diagnostic[] actual = diagnostics.OrderBy(diagnostic => diagnostic.Location.SourceTree?.FilePath)
            .ThenBy(diagnostic => diagnostic.Location.SourceSpan.Start).ThenBy(diagnostic => diagnostic.Id).ToArray();
        ExpectedDiagnostic[] expected = _expected.OrderBy(diagnostic => diagnostic.Path)
            .ThenBy(diagnostic => diagnostic.Span.Start).ThenBy(diagnostic => diagnostic.Id).ToArray();
        Assert.Equal(expected.Length, actual.Length);
        for (int index = 0; index < expected.Length; index++)
        {
            Assert.Equal(expected[index].Id, actual[index].Id);
            Assert.Equal(SourcePath(expected[index].Path), actual[index].Location.SourceTree!.FilePath);
            Assert.Equal(expected[index].Span, actual[index].Location.SourceSpan);
            SourceText text = await Document(expected[index].Path).GetTextAsync();
            Assert.Equal(text.Lines.GetLinePositionSpan(expected[index].Span), actual[index].Location.GetLineSpan().Span);
        }
    }

    internal async Task<IReadOnlyList<CodeAction>> CodeActionsAsync(Document document, Diagnostic diagnostic)
    {
        List<CodeAction> actions = new List<CodeAction>();
        InjectServiceCodeFixProvider provider = new InjectServiceCodeFixProvider();
        await provider.RegisterCodeFixesAsync(new CodeFixContext(
            document, diagnostic, (action, _) => actions.Add(action), CancellationToken.None));
        return actions;
    }

    internal async Task FixAsync(string diagnosticId, string path = "Control.cs")
    {
        Diagnostic diagnostic = (await DiagnosticsAsync()).First(
            candidate => candidate.Id == diagnosticId && candidate.Location.SourceTree!.FilePath == SourcePath(path));
        CodeAction action = Assert.Single(await CodeActionsAsync(Document(path), diagnostic));
        Assert.Equal(diagnosticId, action.EquivalenceKey);
        await ApplyAsync(action);
    }

    internal async Task FixAllAsync(string diagnosticId, FixAllScope scope)
    {
        InjectServiceCodeFixProvider provider = new InjectServiceCodeFixProvider();
        FixAllContext context = new FixAllContext(Document(), provider, scope, diagnosticId,
            new[] { diagnosticId }, new TestDiagnosticProvider(), CancellationToken.None);
        CodeAction? action = await provider.GetFixAllProvider().GetFixAsync(context);
        Assert.NotNull(action);
        await ApplyAsync(action);
    }

    internal async Task<string> TextAsync(string path = "Control.cs") => (await Document(path).GetTextAsync()).ToString();

    internal async Task<MetadataReference> EmitReferenceAsync()
    {
        Compilation compilation = (await Project.GetCompilationAsync())!;
        using MemoryStream stream = new MemoryStream();
        Microsoft.CodeAnalysis.Emit.EmitResult result = compilation.Emit(stream);
        Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
        return MetadataReference.CreateFromImage(stream.ToArray());
    }

    public void Dispose() => _workspace.Dispose();

    private string SourcePath(string path) => _sourceRoot == null ? path : Path.Combine(_sourceRoot, path);

    private async Task ApplyAsync(CodeAction action)
    {
        ImmutableArray<CodeActionOperation> operations = await action.GetOperationsAsync(CancellationToken.None);
        ApplyChangesOperation change = Assert.Single(operations.OfType<ApplyChangesOperation>());
        Assert.True(_workspace.TryApplyChanges(change.ChangedSolution));
    }

    private string Parse(string markedSource, string path)
    {
        StringBuilder source = new StringBuilder();
        int previousEnd = 0;
        foreach (Match match in Regex.Matches(markedSource, @"\{\|(?<ids>WFLDI\d{3}(?:,WFLDI\d{3})*):(?<text>.*?)\|\}", RegexOptions.Singleline))
        {
            source.Append(markedSource, previousEnd, match.Index - previousEnd);
            string text = match.Groups["text"].Value;
            TextSpan span = new TextSpan(source.Length, text.Length);
            foreach (string id in match.Groups["ids"].Value.Split(','))
            {
                _expected.Add(new ExpectedDiagnostic(id, path, span));
            }

            source.Append(text);
            previousEnd = match.Index + match.Length;
        }

        source.Append(markedSource, previousEnd, markedSource.Length - previousEnd);
        return source.ToString();
    }

    private sealed record ExpectedDiagnostic(string Id, string Path, TextSpan Span);

    private sealed class TestDiagnosticProvider : FixAllContext.DiagnosticProvider
    {
        public override async Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken)
        {
            SyntaxTree? tree = await document.GetSyntaxTreeAsync(cancellationToken);
            return (await GetAllDiagnosticsAsync(document.Project, cancellationToken))
                .Where(diagnostic => diagnostic.Location.SourceTree == tree);
        }

        public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken) =>
            Task.FromResult(Enumerable.Empty<Diagnostic>());

        public override async Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken)
        {
            Compilation compilation = (await project.GetCompilationAsync(cancellationToken))!;
            return await compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new InjectServiceAnalyzer()), project.AnalyzerOptions)
                .GetAnalyzerDiagnosticsAsync(cancellationToken);
        }
    }
}
