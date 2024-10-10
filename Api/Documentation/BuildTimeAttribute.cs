using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Reservant.Api.Documentation;

/// <summary>
/// Used to store the build time for the assembly
/// </summary>
/// <param name="time">String representation of the build time</param>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class BuildTimeAttribute(
    [SuppressMessage("Design", "CA1019:Define accessors for attribute arguments",
        Justification = "Available through BuildTime")] string time
    ) : Attribute
{
    /// <summary>
    /// Build time
    /// </summary>
    public DateTime BuildTime { get; } = DateTime.Parse(time, CultureInfo.InvariantCulture);
}
