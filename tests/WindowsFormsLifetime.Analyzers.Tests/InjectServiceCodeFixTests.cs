using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WindowsFormsLifetime.CodeFixes;
using Xunit;

namespace WindowsFormsLifetime.Analyzers.Tests;

public class InjectServiceCodeFixTests
{
    [Theory]
    [InlineData("WFLDI002", "global::System.ComponentModel.BrowsableAttribute(false)")]
    [InlineData("WFLDI003", "global::System.ComponentModel.DesignerSerializationVisibilityAttribute(global::System.ComponentModel.DesignerSerializationVisibility.Hidden)")]
    public async Task AddsMissingMetadataWithoutChangingUnrelatedCode(string id, string attribute)
    {
        string source = """
            using WindowsFormsLifetime;
            public class Control : System.Windows.Forms.UserControl
            {
                // This service is borrowed.
                [InjectService]
                public object Service { get; set; }

                public void Unrelated() {    }
            }
            """;
        using AnalyzerTest test = new AnalyzerTest(new[] { ("Control.cs", source) });

        await test.FixAsync(id);

        string expected = source.Replace("[InjectService]", "[InjectService]\n    [" + attribute + "]", StringComparison.Ordinal);
        Assert.Equal(Normalize(expected), Normalize(await test.TextAsync()));
        Assert.DoesNotContain(await test.DiagnosticsAsync(), diagnostic => diagnostic.Id == id);
    }

    [Theory]
    [InlineData("Browse(true)", "Browse(false)")]
    [InlineData("Browse(browsable: true)", "Browse(browsable: false)")]
    [InlineData("Browse(/* before */ true /* after */)", "Browse(/* before */ false /* after */)")]
    [InlineData("Browse(value)", "Browse(false)")]
    public async Task CorrectsBrowsableKeepingAliasesCommentsAndFormatting(string before, string after)
    {
        string source = $$"""
            using Inject = WindowsFormsLifetime.InjectServiceAttribute;
            using Browse = System.ComponentModel.BrowsableAttribute;
            using System.ComponentModel;
            public class Control : System.Windows.Forms.UserControl
            {
                private const bool value = true;
                [Inject, {{before}}] // property browser
                [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
                public object Service { get; set; }
            }
            """;
        using AnalyzerTest test = new AnalyzerTest(new[] { ("Control.cs", source) });

        await test.FixAsync("WFLDI002");

        Assert.Equal(source.Replace(before, after, StringComparison.Ordinal), await test.TextAsync());
        Assert.Empty(await test.DiagnosticsAsync());
    }

    [Theory]
    [InlineData("Visibility.Content", "Visibility.Hidden")]
    [InlineData("Visibility.Visible", "Visibility.Hidden")]
    [InlineData("Visibility. /* keep */ Content", "Visibility. /* keep */ Hidden")]
    [InlineData("(Visibility)1", "global::System.ComponentModel.DesignerSerializationVisibility.Hidden")]
    [InlineData("/* before */ Visibility.Content /* after */", "/* before */ Visibility.Hidden /* after */")]
    public async Task CorrectsSerializationKeepingAliasesCommentsAndFormatting(string before, string after)
    {
        string source = $$"""
            using Inject = WindowsFormsLifetime.InjectServiceAttribute;
            using Serialize = System.ComponentModel.DesignerSerializationVisibilityAttribute;
            using Visibility = System.ComponentModel.DesignerSerializationVisibility;
            public class Control : System.Windows.Forms.UserControl
            {
                [Inject, System.ComponentModel.Browsable(false)]
                [Serialize(visibility: {{before}})] // keep this note
                public object Service { get; set; }
            }
            """;
        using AnalyzerTest test = new AnalyzerTest(new[] { ("Control.cs", source) });

        await test.FixAsync("WFLDI003");

        Assert.Equal(source.Replace(before, after, StringComparison.Ordinal), await test.TextAsync());
        Assert.Empty(await test.DiagnosticsAsync());
    }

    [Fact]
    public async Task AddsQualifiedAttributesWithoutCollidingWithUnrelatedNames()
    {
        string source = """
            using System;
            public class BrowsableAttribute : Attribute { public BrowsableAttribute(bool value) { } }
            public class DesignerSerializationVisibilityAttribute : Attribute
            {
                public DesignerSerializationVisibilityAttribute(int value) { }
            }
            public class Control : System.Windows.Forms.UserControl
            {
                [WindowsFormsLifetime.InjectService, Browsable(true), DesignerSerializationVisibility(1)]
                public object Service { get; set; }
            }
            """;
        using AnalyzerTest test = new AnalyzerTest(new[] { ("Control.cs", source) });

        await test.FixAsync("WFLDI002");
        await test.FixAsync("WFLDI003");

        string updated = await test.TextAsync();
        Assert.Contains("[WindowsFormsLifetime.InjectService, Browsable(true), DesignerSerializationVisibility(1)]", updated);
        Assert.Contains("[global::System.ComponentModel.BrowsableAttribute(false)]", updated);
        Assert.Contains("[global::System.ComponentModel.DesignerSerializationVisibilityAttribute(global::System.ComponentModel.DesignerSerializationVisibility.Hidden)]", updated);
        Assert.Empty(await test.DiagnosticsAsync());
    }

    [Theory]
    [InlineData("WFLDI002", "[System.ComponentModel.Browsable(true /* rationale */ && true)]")]
    [InlineData("WFLDI002", "[System.ComponentModel.Browsable(true // rationale\n        && true)]")]
    [InlineData("WFLDI003", "[System.ComponentModel.DesignerSerializationVisibility((/* rationale */ System.ComponentModel.DesignerSerializationVisibility)1)]")]
    public async Task PreservesCommentsInsideReplacedConstantExpressions(string id, string attribute)
    {
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            ("Control.cs", $$"""
                public class Control : System.Windows.Forms.UserControl
                {
                    [WindowsFormsLifetime.InjectService]
                    {{attribute}}
                    public object Service { get; set; }
                }
                """)
        });

        await test.FixAsync(id);

        Assert.Contains("rationale", await test.TextAsync());
        Assert.DoesNotContain(await test.DiagnosticsAsync(), diagnostic => diagnostic.Id == id);
    }

    [Fact]
    public async Task DoesNotRewriteAttributeExpressionsWithPreprocessorDirectives()
    {
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            ("Control.cs", """
                public class Control : System.Windows.Forms.UserControl
                {
                    [WindowsFormsLifetime.InjectService]
                    [System.ComponentModel.Browsable(
                #if DEBUG
                        false
                #else
                        true
                #endif
                    )]
                    public object Service { get; set; }
                }
                """)
        });
        Diagnostic diagnostic = (await test.DiagnosticsAsync()).Single(candidate => candidate.Id == "WFLDI002");

        Assert.Empty(await test.CodeActionsAsync(test.Document(), diagnostic));
    }

    [Fact]
    public async Task PreservesAlreadyCorrectAttributesWhileAddingTheOtherMetadata()
    {
        string source = """
            using System.ComponentModel;
            public class Control : System.Windows.Forms.UserControl
            {
                private const bool NotBrowsable = false;
                // Preserve this metadata.
                [WindowsFormsLifetime.InjectService, Browsable(/* intentional */ NotBrowsable)]
                public object Service { get; set; }
            }
            """;
        using AnalyzerTest test = new AnalyzerTest(new[] { ("Control.cs", source) });

        await test.FixAsync("WFLDI003");

        Assert.Contains("// Preserve this metadata.", await test.TextAsync());
        Assert.Contains("[WindowsFormsLifetime.InjectService, Browsable(/* intentional */ NotBrowsable)]", await test.TextAsync());
        Assert.Empty(await test.DiagnosticsAsync());
        await AssertSingleMetadataAttributesAsync(test);
    }

    [Theory]
    [InlineData("WFLDI002", "System.ComponentModel.BrowsableAttribute")]
    [InlineData("WFLDI003", "System.ComponentModel.DesignerSerializationVisibilityAttribute")]
    public async Task OverridesBadInheritedMetadataWithoutEditingTheBase(string id, string attributeName)
    {
        string baseSource = """
            using System.ComponentModel;
            public class BaseControl : System.Windows.Forms.UserControl
            {
                [WindowsFormsLifetime.InjectService, Browsable(true)]
                [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
                public virtual object Service { get; set; }
            }
            """;
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            ("BaseControl.Designer.cs", baseSource),
            ("Control.cs", """
                public class Control : BaseControl
                {
                    // Override metadata here, not in the generated base.
                    public override object Service { get; set; }
                }
                """)
        });

        await test.FixAsync(id);

        Assert.Equal(baseSource, await test.TextAsync("BaseControl.Designer.cs"));
        Assert.Contains("// Override metadata here, not in the generated base.", await test.TextAsync());
        Assert.Contains("[global::" + attributeName + "(", await test.TextAsync());
        Assert.DoesNotContain(await test.DiagnosticsAsync(), diagnostic => diagnostic.Id == id);
    }

    [Fact]
    public async Task FixesUserPartialWithoutTouchingDesignerPartial()
    {
        string designer = """
            public partial class Control : System.Windows.Forms.UserControl
            {
                private void InitializeComponent() { }
            }
            """;
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            ("Control.Designer.cs", designer),
            ("Control.cs", """
                public partial class Control
                {
                    [WindowsFormsLifetime.InjectService]
                    public object Service { get; set; }
                    public Control() { InitializeComponent(); }
                }
                """)
        });

        await test.FixAsync("WFLDI002");
        await test.FixAsync("WFLDI003");

        Assert.Equal(designer, await test.TextAsync("Control.Designer.cs"));
        Assert.Contains("public Control() { InitializeComponent(); }", await test.TextAsync());
        Assert.Empty(await test.DiagnosticsAsync());
        await AssertSingleMetadataAttributesAsync(test);
    }

    [Theory]
    [InlineData("Control.Designer.cs", "")]
    [InlineData("Control.g.cs", "")]
    [InlineData("Control.cs", "// <auto-generated/>\n")]
    [InlineData("Control.cs", "[System.CodeDom.Compiler.GeneratedCode(\"test\", \"1\")]\n")]
    public async Task DoesNotOfferFixesInGeneratedCodeEvenForStaleDiagnostics(string path, string header)
    {
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            (path, header + """
                public class Control : System.Windows.Forms.UserControl
                {
                    [WindowsFormsLifetime.InjectService]
                    public object Service { get; set; }
                }
                """)
        });
        Document document = test.Document(path);
        SyntaxNode root = (await document.GetSyntaxRootAsync())!;
        PropertyDeclarationSyntax property = root.DescendantNodes().OfType<PropertyDeclarationSyntax>().Single();
        foreach (DiagnosticDescriptor descriptor in new InjectServiceAnalyzer().SupportedDiagnostics.Where(
            descriptor => descriptor.Id == "WFLDI002" || descriptor.Id == "WFLDI003"))
        {
            Diagnostic diagnostic = Diagnostic.Create(descriptor, property.Identifier.GetLocation(), "Service");
            IReadOnlyList<CodeAction> actions = await test.CodeActionsAsync(document, diagnostic);
            Assert.Empty(actions);
        }
    }

    [Fact]
    public async Task DoesNotOfferApiChangingOrCodeMovementFixes()
    {
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            ("Control.cs", """
                public class Control : System.Windows.Forms.UserControl
                {
                    [WindowsFormsLifetime.InjectService]
                    private object Service { get; set; }
                    public Control() { _ = Service; }
                }
                """)
        });
        InjectServiceCodeFixProvider provider = new InjectServiceCodeFixProvider();

        Assert.Equal(new[] { "WFLDI002", "WFLDI003" }, provider.FixableDiagnosticIds);
        foreach (Diagnostic diagnostic in await test.DiagnosticsAsync())
        {
            Assert.Empty(await test.CodeActionsAsync(test.Document(), diagnostic));
        }
    }

    [Fact]
    public async Task DoesNotOfferFixesForEditorConfigGeneratedCode()
    {
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            ("Control.cs", """
                public class Control : System.Windows.Forms.UserControl
                {
                    [WindowsFormsLifetime.InjectService]
                    public object Service { get; set; }
                }
                """)
        }, editorConfig: """
            root = true
            [Control.cs]
            generated_code = true
            """);
        Document document = test.Document();
        SyntaxNode root = (await document.GetSyntaxRootAsync())!;
        PropertyDeclarationSyntax property = root.DescendantNodes().OfType<PropertyDeclarationSyntax>().Single();
        DiagnosticDescriptor descriptor = new InjectServiceAnalyzer().SupportedDiagnostics.Single(rule => rule.Id == "WFLDI002");
        Diagnostic diagnostic = Diagnostic.Create(descriptor, property.Identifier.GetLocation(), "Service");

        Assert.Empty(await test.CodeActionsAsync(document, diagnostic));
    }

    [Theory]
    [InlineData(FixAllScope.Document)]
    [InlineData(FixAllScope.Project)]
    [InlineData(FixAllScope.Solution)]
    public async Task FixAllCorrectsAndAddsAttributesWithoutDuplicates(FixAllScope scope)
    {
        string source = """
            using System.ComponentModel;
            public class Control : System.Windows.Forms.UserControl
            {
                [WindowsFormsLifetime.InjectService]
                public object Missing { get; set; }

                [WindowsFormsLifetime.InjectService, Browsable(true)]
                [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
                public object Incorrect { get; set; }

                [WindowsFormsLifetime.InjectService, Browsable(false)]
                [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
                public object Correct { get; set; }
            }
            """;
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            ("Control.cs", source),
            ("OtherControl.cs", source.Replace("class Control", "class OtherControl", StringComparison.Ordinal)),
            ("GeneratedControl.Designer.cs", source.Replace("class Control", "class GeneratedControl", StringComparison.Ordinal))
        });

        await test.FixAllAsync("WFLDI002", scope);
        await test.FixAllAsync("WFLDI003", scope);

        Assert.DoesNotContain(await test.DiagnosticsAsync(), diagnostic => diagnostic.Location.SourceTree!.FilePath == "Control.cs");
        if (scope == FixAllScope.Document)
        {
            Assert.Equal(source.Replace("class Control", "class OtherControl", StringComparison.Ordinal), await test.TextAsync("OtherControl.cs"));
        }
        else
        {
            Assert.Empty(await test.DiagnosticsAsync());
            await AssertSingleMetadataAttributesAsync(test, "OtherControl.cs");
        }

        Assert.Equal(source.Replace("class Control", "class GeneratedControl", StringComparison.Ordinal),
            await test.TextAsync("GeneratedControl.Designer.cs"));
        Assert.Contains("[WindowsFormsLifetime.InjectService, Browsable(false)]", await test.TextAsync());
        await AssertSingleMetadataAttributesAsync(test);
    }

    private static string Normalize(string text) => text.Replace("\r\n", "\n", StringComparison.Ordinal);

    private static async Task AssertSingleMetadataAttributesAsync(AnalyzerTest test, string path = "Control.cs")
    {
        Document document = test.Document(path);
        SemanticModel model = (await document.GetSemanticModelAsync())!;
        SyntaxNode root = (await document.GetSyntaxRootAsync())!;
        foreach (PropertyDeclarationSyntax syntax in root.DescendantNodes().OfType<PropertyDeclarationSyntax>())
        {
            IPropertySymbol symbol = (IPropertySymbol)model.GetDeclaredSymbol(syntax)!;
            Assert.Single(symbol.GetAttributes().Where(attribute =>
                attribute.AttributeClass?.ToDisplayString() == "System.ComponentModel.BrowsableAttribute"));
            Assert.Single(symbol.GetAttributes().Where(attribute =>
                attribute.AttributeClass?.ToDisplayString() == "System.ComponentModel.DesignerSerializationVisibilityAttribute"));
        }
    }
}
