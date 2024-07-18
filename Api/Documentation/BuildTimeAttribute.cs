namespace Reservant.Api.Documentation;

/// <summary>
/// Used to store the build time for the assembly
/// </summary>
/// <param name="time">String representation of the build time</param>
[AttributeUsage(AttributeTargets.Assembly)]
public class BuildTimeAttribute(string time) : Attribute
{
    /// <summary>
    /// Build time
    /// </summary>
    public DateTime BuildTime { get; } = DateTime.Parse(time);
}
