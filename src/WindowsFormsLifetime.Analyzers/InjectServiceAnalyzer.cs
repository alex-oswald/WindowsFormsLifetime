using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace WindowsFormsLifetime.Analyzers;

/// <summary>
/// Validates injected user-control properties and their use during construction.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InjectServiceAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for an unsupported injection property.
    /// </summary>
    public const string InvalidPropertyId = "WFLDI001";

    /// <summary>
    /// The diagnostic ID for missing or incorrect Browsable metadata.
    /// </summary>
    public const string BrowsableId = "WFLDI002";

    /// <summary>
    /// The diagnostic ID for missing or incorrect serialization metadata.
    /// </summary>
    public const string SerializationId = "WFLDI003";

    /// <summary>
    /// The diagnostic ID for reading an injected service during construction.
    /// </summary>
    public const string ConstructorReadId = "WFLDI004";

    private const string HelpLink = "https://github.com/alex-oswald/WindowsFormsLifetime/blob/main/README.md#";

    private static readonly DiagnosticDescriptor InvalidProperty = new DiagnosticDescriptor(
        InvalidPropertyId,
        "Injected service property has an unsupported declaration",
        "Injected property '{0}' must be a public instance, non-indexed property on a UserControl with a public non-init setter",
        "WindowsFormsLifetime.Injection",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "InjectService supports only public instance properties on UserControl types with an ordinary public setter.",
        helpLinkUri: HelpLink + "wfldi001");

    private static readonly DiagnosticDescriptor Browsable = new DiagnosticDescriptor(
        BrowsableId,
        "Hide the injected service from the property browser",
        "Injected property '{0}' must have effective Browsable(false) metadata",
        "WindowsFormsLifetime.Injection",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Injected services are runtime-only properties and must not appear in the Windows Forms property browser.",
        helpLinkUri: HelpLink + "wfldi002");

    private static readonly DiagnosticDescriptor Serialization = new DiagnosticDescriptor(
        SerializationId,
        "Do not serialize the injected service",
        "Injected property '{0}' must have effective DesignerSerializationVisibility.Hidden metadata",
        "WindowsFormsLifetime.Injection",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Injected services must be hidden from designer serialization so the designer does not generate service assignments.",
        helpLinkUri: HelpLink + "wfldi003");

    private static readonly DiagnosticDescriptor ConstructorRead = new DiagnosticDescriptor(
        ConstructorReadId,
        "Injected services are unavailable during construction",
        "Injected property '{0}' is read during construction; use OnServicesInjected or later runtime interaction",
        "WindowsFormsLifetime.Injection",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Services are injected after construction. Direct reads of this control's injected properties in its constructor occur too early.",
        helpLinkUri: HelpLink + "wfldi004");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(InvalidProperty, Browsable, Serialization, ConstructorRead);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(startContext =>
        {
            INamedTypeSymbol? injectService = startContext.Compilation.GetTypeByMetadataName("WindowsFormsLifetime.InjectServiceAttribute");
            INamedTypeSymbol? userControl = startContext.Compilation.GetTypeByMetadataName("System.Windows.Forms.UserControl");
            if (injectService == null || userControl == null)
            {
                return;
            }

            InjectionSymbols symbols = new InjectionSymbols(startContext.Compilation, injectService, userControl);
            startContext.RegisterSymbolAction(symbolContext => AnalyzeProperty(symbolContext, symbols), SymbolKind.Property);
            startContext.RegisterOperationAction(operationContext => AnalyzeRead(operationContext, symbols), OperationKind.PropertyReference);
        });
    }

    private static void AnalyzeProperty(SymbolAnalysisContext context, InjectionSymbols symbols)
    {
        IPropertySymbol property = (IPropertySymbol)context.Symbol;
        if (InjectionSymbols.GetEffectiveAttribute(property, symbols.InjectService) == null
            || symbols.IsGenerated(property))
        {
            return;
        }

        Location? location = null;
        foreach (SyntaxReference reference in property.DeclaringSyntaxReferences)
        {
            if (GeneratedCode.IsGenerated(reference.SyntaxTree, context.CancellationToken))
            {
                continue;
            }

            SyntaxNode declaration = reference.GetSyntax(context.CancellationToken);
            if (declaration is PropertyDeclarationSyntax propertyDeclaration)
            {
                location = propertyDeclaration.Identifier.GetLocation();
                break;
            }

            if (declaration is IndexerDeclarationSyntax indexerDeclaration)
            {
                location = indexerDeclaration.ThisKeyword.GetLocation();
                break;
            }
        }

        if (location == null)
        {
            return;
        }

        if (!symbols.IsSupportedProperty(property))
        {
            context.ReportDiagnostic(Diagnostic.Create(InvalidProperty, location, property.Name));
            return;
        }

        AttributeData? browsable = InjectionSymbols.GetEffectiveAttribute(property, symbols.Browsable);
        if (symbols.Browsable != null
            && (browsable == null || browsable.ConstructorArguments.Length != 1
                || !(browsable.ConstructorArguments[0].Value is false)))
        {
            context.ReportDiagnostic(Diagnostic.Create(Browsable, location, property.Name));
        }

        AttributeData? serialization = InjectionSymbols.GetEffectiveAttribute(property, symbols.DesignerSerializationVisibility);
        if (symbols.DesignerSerializationVisibility != null && symbols.HiddenSerializationValue != null
            && (serialization == null || serialization.ConstructorArguments.Length != 1
                || !Equals(serialization.ConstructorArguments[0].Value, symbols.HiddenSerializationValue)))
        {
            context.ReportDiagnostic(Diagnostic.Create(Serialization, location, property.Name));
        }
    }

    private static void AnalyzeRead(OperationAnalysisContext context, InjectionSymbols symbols)
    {
        IPropertyReferenceOperation reference = (IPropertyReferenceOperation)context.Operation;
        if (!(context.ContainingSymbol is IMethodSymbol method)
            || method.MethodKind != MethodKind.Constructor
            || !symbols.IsUserControl(method.ContainingType)
            || !IsCurrentInstance(reference.Instance)
            || InjectionSymbols.GetEffectiveAttribute(reference.Property, symbols.InjectService) == null
            || GeneratedCode.IsGenerated(reference.Syntax.SyntaxTree, context.CancellationToken)
            || symbols.IsGenerated(method))
        {
            return;
        }

        for (IOperation? current = reference.Parent; current != null; current = current.Parent)
        {
            if (current is INameOfOperation || current is IAnonymousFunctionOperation || current is ILocalFunctionOperation)
            {
                return;
            }
        }

        if (IsWriteOnly(reference))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(ConstructorRead, reference.Syntax.GetLocation(), reference.Property.Name));
    }

    private static bool IsCurrentInstance(IOperation? operation)
    {
        while (operation is IConversionOperation || operation is IParenthesizedOperation)
        {
            if (operation is IConversionOperation conversion && conversion.OperatorMethod != null)
            {
                return false;
            }

            operation = operation is IConversionOperation currentConversion
                ? currentConversion.Operand
                : ((IParenthesizedOperation)operation).Operand;
        }

        if (operation is IConditionalAccessInstanceOperation)
        {
            for (IOperation? current = operation.Parent; current != null; current = current.Parent)
            {
                if (current is IConditionalAccessOperation conditionalAccess)
                {
                    return IsCurrentInstance(conditionalAccess.Operation);
                }
            }
        }

        return operation is IInstanceReferenceOperation instance
            && instance.ReferenceKind == InstanceReferenceKind.ContainingTypeInstance;
    }

    private static bool IsWriteOnly(IPropertyReferenceOperation reference)
    {
        IOperation target = reference;
        while (target.Parent is IParenthesizedOperation || target.Parent is ITupleOperation)
        {
            target = target.Parent;
        }

        return (target.Parent is ISimpleAssignmentOperation assignment && assignment.Target == target)
            || (target.Parent is IDeconstructionAssignmentOperation deconstruction && deconstruction.Target == target)
            || (target.Parent is IArgumentOperation argument && argument.Parameter?.RefKind == RefKind.Out);
    }
}
