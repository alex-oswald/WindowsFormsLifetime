using Microsoft.CodeAnalysis;

namespace WindowsFormsLifetime.Analyzers;

internal sealed class InjectionSymbols
{
    private readonly INamedTypeSymbol? _generatedCode;
    private readonly INamedTypeSymbol? _compilerGenerated;

    internal InjectionSymbols(Compilation compilation, INamedTypeSymbol injectService, INamedTypeSymbol userControl)
    {
        InjectService = injectService;
        UserControl = userControl;
        Browsable = compilation.GetTypeByMetadataName("System.ComponentModel.BrowsableAttribute");
        DesignerSerializationVisibility = compilation.GetTypeByMetadataName("System.ComponentModel.DesignerSerializationVisibilityAttribute");
        _generatedCode = compilation.GetTypeByMetadataName("System.CodeDom.Compiler.GeneratedCodeAttribute");
        _compilerGenerated = compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.CompilerGeneratedAttribute");
        INamedTypeSymbol? visibility = compilation.GetTypeByMetadataName("System.ComponentModel.DesignerSerializationVisibility");
        if (visibility != null)
        {
            foreach (ISymbol member in visibility.GetMembers("Hidden"))
            {
                if (member is IFieldSymbol field && field.HasConstantValue)
                {
                    HiddenSerializationValue = field.ConstantValue;
                    break;
                }
            }
        }
    }

    internal INamedTypeSymbol InjectService { get; }

    internal INamedTypeSymbol UserControl { get; }

    internal INamedTypeSymbol? Browsable { get; }

    internal INamedTypeSymbol? DesignerSerializationVisibility { get; }

    internal object? HiddenSerializationValue { get; }

    internal bool IsGenerated(ISymbol symbol) => GeneratedCode.IsGenerated(symbol, _generatedCode, _compilerGenerated);

    internal bool IsUserControl(INamedTypeSymbol type)
    {
        for (INamedTypeSymbol? current = type; current != null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, UserControl))
            {
                return true;
            }
        }

        return false;
    }

    internal static AttributeData? GetEffectiveAttribute(IPropertySymbol property, INamedTypeSymbol? attributeType)
    {
        if (attributeType == null)
        {
            return null;
        }

        for (IPropertySymbol? current = property; current != null; current = current.OverriddenProperty)
        {
            foreach (AttributeData attribute in current.GetAttributes())
            {
                if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeType))
                {
                    return attribute;
                }
            }
        }

        return null;
    }

    internal bool IsSupportedProperty(IPropertySymbol property)
    {
        return IsUserControl(property.ContainingType)
            && property.DeclaredAccessibility == Accessibility.Public
            && !property.IsStatic
            && !property.IsIndexer
            && property.ExplicitInterfaceImplementations.Length == 0
            && property.SetMethod != null
            && property.SetMethod.DeclaredAccessibility == Accessibility.Public
            && !property.SetMethod.IsInitOnly;
    }
}
