namespace FMS.Application.Configuration;

/// <summary>
/// Root configuration section for FMS settings.
/// Bound to the "Fms" configuration section.
/// </summary>
public sealed class FmsOptions
{
    public const string SectionName = "Fms";

    /// <summary>
    /// Runtime mode: Lan or Cloud.
    /// </summary>
    public string Mode { get; set; } = "Lan";

    /// <summary>
    /// Application display version (injected at build/deploy time).
    /// </summary>
    public string Version { get; set; } = "0.1.0";
}
