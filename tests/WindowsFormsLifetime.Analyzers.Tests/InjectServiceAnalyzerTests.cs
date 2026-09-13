using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace WindowsFormsLifetime.Analyzers.Tests;

public class InjectServiceAnalyzerTests
{
    [Theory]
    [InlineData("public static object {|WFLDI001:Service|} { get; set; }")]
    [InlineData("private object {|WFLDI001:Service|} { get; set; }")]
    [InlineData("protected object {|WFLDI001:Service|} { get; set; }")]
    [InlineData("internal object {|WFLDI001:Service|} { get; set; }")]
    [InlineData("public object {|WFLDI001:Service|} { get; private set; }")]
    [InlineData("public object {|WFLDI001:Service|} { get; protected set; }")]
    [InlineData("public object {|WFLDI001:Service|} { get; internal set; }")]
    [InlineData("public object {|WFLDI001:Service|} { get; init; }")]
    [InlineData("public object {|WFLDI001:Service|} { get; }")]
    [InlineData("public object {|WFLDI001:Service|} => new object();")]
    [InlineData("public object {|WFLDI001:this|}[int index] { get => null; set { } }")]
    public async Task RejectsInvalidPropertiesWithoutMetadataCascades(string declaration)
    {
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            ("Control.cs", $$"""
                using WindowsFormsLifetime;
                public class Control : System.Windows.Forms.UserControl
                {
                    [InjectService]
                    {{declaration}}
                }
                """)
        });

        await test.VerifyAsync();
        Assert.All(await test.DiagnosticsAsync(), diagnostic => Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity));
    }

    [Theory]
    [InlineData("class Control")]
    [InlineData("struct Control")]
    [InlineData("interface Control")]
    public async Task RejectsPropertiesOutsideUserControls(string declaration)
    {
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            ("Control.cs", $$"""
                public {{declaration}}
                {
                    [WindowsFormsLifetime.InjectService]
                    public object {|WFLDI001:Service|} { get; set; }
                }
                """)
        });

        await test.VerifyAsync();
    }

    [Fact]
    public async Task RejectsExplicitInterfaceProperties()
    {
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            ("Control.cs", """
                public interface IServiceControl { object Service { get; set; } }
                public class Control : System.Windows.Forms.UserControl, IServiceControl
                {
                    [WindowsFormsLifetime.InjectService]
                    object IServiceControl.{|WFLDI001:Service|} { get; set; }
                }
                """)
        });

        await test.VerifyAsync();
    }

    [Theory]
    [InlineData("", "WFLDI002,WFLDI003")]
    [InlineData("[System.ComponentModel.Browsable(true)]", "WFLDI002,WFLDI003")]
    [InlineData("[System.ComponentModel.Browsable(false)]", "WFLDI003")]
    [InlineData("[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Content)]", "WFLDI002,WFLDI003")]
    [InlineData("[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]", "WFLDI002,WFLDI003")]
    [InlineData("[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]", "WFLDI002")]
    public async Task ReportsMissingOrIncorrectDesignerMetadata(string attributes, string ids)
    {
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            ("Control.cs", $$"""
                public class Control : System.Windows.Forms.UserControl
                {
                    [WindowsFormsLifetime.InjectService]
                    {{attributes}}
                    public object {|{{ids}}:Service|} { get; set; }
                }
                """)
        });

        await test.VerifyAsync();
        Assert.All(await test.DiagnosticsAsync(), diagnostic => Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity));
    }

    [Fact]
    public async Task AcceptsAliasesConstantsNullableAndWriteOnlyPropertiesWithoutRegistrationOrCallbacks()
    {
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            ("Control.cs", """
                #nullable enable
                using Inject = WindowsFormsLifetime.InjectServiceAttribute;
                using Browse = System.ComponentModel.BrowsableAttribute;
                using Serialize = System.ComponentModel.DesignerSerializationVisibilityAttribute;
                using Visibility = System.ComponentModel.DesignerSerializationVisibility;
                public class Control : System.Windows.Forms.UserControl
                {
                    private const bool IsBrowsable = false;
                    private const Visibility Serialization = Visibility.Hidden;
                    public Control(int runtimeParameter) { }

                    [Inject, Browse(IsBrowsable), Serialize(Serialization)]
                    public object? Service { get; set; }

                    [Inject, Browse(false), Serialize((Visibility)0)]
                    public object WriteOnly { set { } }

                    public object? Unmarked { get; set; }
                }
                """)
        });

        await test.VerifyAsync();
    }

    [Fact]
    public async Task IgnoresUnrelatedAttributesWithTheSameShortNames()
    {
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            ("Control.cs", """
                using System;
                public class InjectServiceAttribute : Attribute { }
                public class BrowsableAttribute : Attribute { public BrowsableAttribute(bool value) { } }
                public class DesignerSerializationVisibilityAttribute : Attribute
                {
                    public DesignerSerializationVisibilityAttribute(int value) { }
                }
                public class Control : System.Windows.Forms.UserControl
                {
                    [InjectService]
                    private object Unrelated { get; set; }

                    [WindowsFormsLifetime.InjectService, Browsable(false), DesignerSerializationVisibility(0)]
                    public object {|WFLDI002,WFLDI003:Service|} { get; set; }

                    public Control() { _ = Unrelated; }
                }
                """)
        });

        await test.VerifyAsync();
    }

    [Fact]
    public async Task DoesNothingWhenInjectionContractIsMissing()
    {
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            ("Control.cs", """
                public class InjectServiceAttribute : System.Attribute { }
                public class Control : System.Windows.Forms.UserControl
                {
                    [InjectService]
                    private object Service { get; set; }
                    public Control() { _ = Service; }
                }
                namespace System.Windows.Forms { public class UserControl { } }
                """)
        }, includeContracts: false);

        await test.VerifyAsync();
    }

    [Fact]
    public async Task DoesNothingWhenUserControlContractIsMissing()
    {
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            ("Control.cs", """
                public class Control
                {
                    [WindowsFormsLifetime.InjectService]
                    private object Service { get; set; }
                }
                namespace WindowsFormsLifetime { public class InjectServiceAttribute : System.Attribute { } }
                """)
        }, includeContracts: false);

        await test.VerifyAsync();
    }

    [Fact]
    public async Task HonorsInheritedAttributesAndReportsOnlyIncorrectOverrides()
    {
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            ("Control.cs", """
                using System.ComponentModel;
                using WindowsFormsLifetime;
                public class BaseControl : System.Windows.Forms.UserControl
                {
                    [InjectService, Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
                    public virtual object Service { get; set; }
                }
                public class MiddleControl : BaseControl
                {
                    public override object Service { get; set; }
                }
                public class Control : MiddleControl
                {
                    [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
                    public override object {|WFLDI002,WFLDI003:Service|} { get; set; }
                    public Control() { _ = {|WFLDI004:Service|}; }
                }
                public class HiddenControl : BaseControl
                {
                    public new object Service { get; set; }
                    public HiddenControl() { _ = Service; }
                }
                """)
        });

        await test.VerifyAsync();
    }

    [Fact]
    public async Task InheritsAttributesFromReferencedAssembliesWithoutLoadingRuntime()
    {
        using AnalyzerTest baseTest = new AnalyzerTest(new[]
        {
            ("BaseControl.cs", """
                using System.ComponentModel;
                public class BaseControl : System.Windows.Forms.UserControl
                {
                    [WindowsFormsLifetime.InjectService, Browsable(false)]
                    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
                    public virtual object Service { get; set; }
                }
                """)
        });
        MetadataReference reference = await baseTest.EmitReferenceAsync();
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            ("Control.cs", """
                public class Control : BaseControl
                {
                    public override object Service { get; set; }
                    public Control() { _ = {|WFLDI004:base.Service|}; }
                }
                """)
        }, includeContracts: false, additionalReferences: new[] { reference });

        await test.VerifyAsync();
    }

    [Theory]
    [InlineData("Control.Designer.cs", "")]
    [InlineData("Control.g.cs", "")]
    [InlineData("Control.g.i.cs", "")]
    [InlineData("Control.generated.cs", "")]
    [InlineData("TemporaryGeneratedFile_Control.cs", "")]
    [InlineData("Generated.cs", "// <auto-generated/>\n")]
    [InlineData("Generated.cs", "/* <auto-generated> */\n")]
    public async Task IgnoresGeneratedDeclarationsButUsesTheirSymbols(string path, string header)
    {
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            (path, header + """
                public partial class Control : System.Windows.Forms.UserControl
                {
                    [WindowsFormsLifetime.InjectService]
                    public object GeneratedService { get; set; }
                    [WindowsFormsLifetime.InjectService]
                    private object Invalid { get; set; }
                    public Control(int value) { _ = GeneratedService; }
                }
                """),
            ("Control.cs", """
                public partial class Control
                {
                    [WindowsFormsLifetime.InjectService]
                    public object {|WFLDI002,WFLDI003:Service|} { get; set; }
                    public Control() { _ = {|WFLDI004:GeneratedService|}; }
                }
                """)
        });

        await test.VerifyAsync();
    }

    [Fact]
    public async Task HonorsGeneratedCodeAttributes()
    {
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            ("Control.cs", """
                using System.CodeDom.Compiler;
                [GeneratedCode("test", "1")]
                public class Control : System.Windows.Forms.UserControl
                {
                    [WindowsFormsLifetime.InjectService]
                    private object Service { get; set; }
                    public Control() { _ = Service; }
                }
                public class OtherControl : System.Windows.Forms.UserControl
                {
                    [GeneratedCode("test", "1"), WindowsFormsLifetime.InjectService]
                    public object Service { get; set; }
                    [GeneratedCode("test", "1")]
                    public OtherControl() { _ = Service; }
                }
                """)
        });

        await test.VerifyAsync();
    }

    [Fact]
    public async Task InheritsMetadataAcrossGeneratedBaseProperties()
    {
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            ("BaseControl.Designer.cs", """
                using System.ComponentModel;
                public class BaseControl : System.Windows.Forms.UserControl
                {
                    [WindowsFormsLifetime.InjectService, Browsable(false)]
                    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
                    public virtual object Service { get; set; }
                }
                """),
            ("Control.cs", """
                public partial class Control : BaseControl
                {
                    public override object Service { get; set; }
                    public Control() { _ = {|WFLDI004:Service|}; }
                }
                """)
        });

        await test.VerifyAsync();
    }

    [Theory]
    [InlineData("_ = {|WFLDI004:Service|};")]
    [InlineData("_ = {|WFLDI004:this.Service|};")]
    [InlineData("_ = {|WFLDI004:((Control)this).Service|};")]
    [InlineData("_ = this?{|WFLDI004:.Service|};")]
    [InlineData("System.Console.WriteLine({|WFLDI004:Service|});")]
    [InlineData("{|WFLDI004:Service|}.ToString();")]
    [InlineData("_ = {|WFLDI004:Service|}?.ToString();")]
    [InlineData("Service = {|WFLDI004:Service|};")]
    [InlineData("{|WFLDI004:Service|} ??= new object();")]
    [InlineData("System.Func<string> callback = {|WFLDI004:Service|}.ToString;")]
    public async Task ReportsDirectConstructorReads(string body)
    {
        using AnalyzerTest test = CreateConstructorTest(body);

        await test.VerifyAsync();
    }

    [Theory]
    [InlineData("_ = nameof(Service);")]
    [InlineData("_ = nameof(this.Service);")]
    [InlineData("Service = new object();")]
    [InlineData("this.Service = new object();")]
    [InlineData("(Service, Other) = (new object(), new object());")]
    [InlineData("System.Action callback = () => Service.ToString();")]
    [InlineData("System.Action callback = delegate { Service.ToString(); };")]
    [InlineData("System.Func<object> callback = () => { return Service; };")]
    [InlineData("object ReadService() => Service;")]
    [InlineData("void ReadService() { _ = this.Service; }")]
    [InlineData("System.Linq.Expressions.Expression<System.Func<object>> expression = () => Service;")]
    [InlineData("_ = other.Service;")]
    [InlineData("_ = other?.Service;")]
    [InlineData("_ = ((Control)other).Service;")]
    [InlineData("_ = new Control(null).Service;")]
    [InlineData("_ = Other;")]
    [InlineData("ReadLater();")]
    public async Task IgnoresNonReadsDeferredContextsAndOtherInstances(string body)
    {
        using AnalyzerTest test = CreateConstructorTest(body);

        await test.VerifyAsync();
    }

    [Fact]
    public async Task ReportsCompoundAndIncrementReadsButNotWrites()
    {
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            ("Control.cs", """
                using System.ComponentModel;
                public class Control : System.Windows.Forms.UserControl
                {
                    [WindowsFormsLifetime.InjectService, Browsable(false)]
                    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
                    public int Service { get; set; }
                    public Control()
                    {
                        {|WFLDI004:Service|}++;
                        ++{|WFLDI004:this.Service|};
                        {|WFLDI004:Service|} += 1;
                        Service = 3;
                    }
                }
                """)
        });

        await test.VerifyAsync();
    }

    [Fact]
    public async Task ReportsExpressionBodiedConstructorsButNotPostConstructionMethods()
    {
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            ("Control.cs", """
                using System.ComponentModel;
                public class Control : System.Windows.Forms.UserControl, WindowsFormsLifetime.IOnServicesInjected
                {
                    [WindowsFormsLifetime.InjectService, Browsable(false)]
                    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
                    public object Service { get; set; }
                    public Control() => {|WFLDI004:Service|}.ToString();
                    public void OnServicesInjected() => Service.ToString();
                    public void OnLoad() => Service.ToString();
                }
                """)
        });

        await test.VerifyAsync();
    }

    [Fact]
    public async Task SupportsPragmaSuppressionAndSeverityConfiguration()
    {
        CSharpCompilationOptions options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic>
            {
                ["WFLDI002"] = ReportDiagnostic.Suppress,
                ["WFLDI003"] = ReportDiagnostic.Error
            });
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            ("Control.cs", """
                public class Control : System.Windows.Forms.UserControl
                {
                    [WindowsFormsLifetime.InjectService]
                    public object {|WFLDI003:Service|} { get; set; }
                    public Control()
                    {
                #pragma warning disable WFLDI004
                        _ = Service;
                #pragma warning restore WFLDI004
                    }
                }
                """)
        }, compilationOptions: options);

        await test.VerifyAsync();
        Assert.Equal(DiagnosticSeverity.Error, Assert.Single(await test.DiagnosticsAsync()).Severity);
    }

    [Fact]
    public async Task HonorsEditorConfigSeverityAndSuppression()
    {
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            ("Control.cs", """
                public class Control : System.Windows.Forms.UserControl
                {
                    [WindowsFormsLifetime.InjectService]
                    public object {|WFLDI003:Service|} { get; set; }
                }
                """)
        }, editorConfig: """
            root = true
            [*.cs]
            dotnet_diagnostic.WFLDI002.severity = none
            dotnet_diagnostic.WFLDI003.severity = error
            """);

        await test.VerifyAsync();
        Assert.Equal(DiagnosticSeverity.Error, Assert.Single(await test.DiagnosticsAsync()).Severity);
    }

    [Fact]
    public async Task HonorsEditorConfigGeneratedCode()
    {
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            ("Control.cs", """
                public class Control : System.Windows.Forms.UserControl
                {
                    [WindowsFormsLifetime.InjectService]
                    public object Service { get; set; }
                    public Control() { _ = Service; }
                }
                """)
        }, editorConfig: """
            root = true
            [Control.cs]
            generated_code = true
            """);

        await test.VerifyAsync();
    }

    [Fact]
    public async Task DoesNotTreatUserDefinedConversionsAsTheCurrentInstance()
    {
        using AnalyzerTest test = new AnalyzerTest(new[]
        {
            ("Control.cs", """
                using System.ComponentModel;
                public class OtherControl : System.Windows.Forms.UserControl
                {
                    [WindowsFormsLifetime.InjectService, Browsable(false)]
                    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
                    public object Service { get; set; }
                }
                public class Control : System.Windows.Forms.UserControl
                {
                    public static explicit operator OtherControl(Control control) => new OtherControl();
                    public Control() { _ = ((OtherControl)this).Service; }
                }
                """)
        });

        await test.VerifyAsync();
    }

    [Fact]
    public void ExposesStableEnabledRulesAndHelpLinks()
    {
        ImmutableArray<DiagnosticDescriptor> rules = new InjectServiceAnalyzer().SupportedDiagnostics;
        Assert.Equal(new[] { "WFLDI001", "WFLDI002", "WFLDI003", "WFLDI004" }, rules.Select(rule => rule.Id));
        Assert.All(rules, rule =>
        {
            Assert.True(rule.IsEnabledByDefault);
            Assert.NotEmpty(rule.Description.ToString());
            Assert.EndsWith("README.md#" + rule.Id.ToLowerInvariant(), rule.HelpLinkUri);
        });
    }

    [Fact]
    public void CompilerAnalyzerHasNoRuntimeLibraryOrWorkspaceDependency()
    {
        string[] references = typeof(InjectServiceAnalyzer).Assembly.GetReferencedAssemblies()
            .Select(assembly => assembly.Name!).ToArray();

        Assert.DoesNotContain("WindowsFormsLifetime", references);
        Assert.DoesNotContain("System.Windows.Forms", references);
        Assert.DoesNotContain(references, reference => reference.Contains("Workspaces", StringComparison.Ordinal));
    }

    private static AnalyzerTest CreateConstructorTest(string body) => new AnalyzerTest(new[]
    {
        ("Control.cs", $$"""
            using System.ComponentModel;
            public class Control : System.Windows.Forms.UserControl
            {
                [WindowsFormsLifetime.InjectService, Browsable(false)]
                [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
                public object Service { get; set; }
                public object Other { get; set; }
                public Control(Control other)
                {
                    {{body}}
                }
                public void ReadLater() { _ = Service; }
            }
            """)
    });
}
