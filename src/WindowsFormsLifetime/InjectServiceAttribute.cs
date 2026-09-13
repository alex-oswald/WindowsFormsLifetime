namespace WindowsFormsLifetime;

/// <summary>
/// Marks a public, settable user control property for required runtime service injection.
/// </summary>
/// <remarks>
/// Services are assigned after form construction. Hide this property from the designer with
/// <see cref="System.ComponentModel.BrowsableAttribute"/> and
/// <see cref="System.ComponentModel.DesignerSerializationVisibilityAttribute"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class InjectServiceAttribute : Attribute
{
}
