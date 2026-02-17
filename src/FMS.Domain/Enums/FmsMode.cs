namespace FMS.Domain.Enums;

/// <summary>
/// Runtime mode for the FMS system.
/// </summary>
public enum FmsMode
{
    /// <summary>Local area network operation — no internet required.</summary>
    Lan,

    /// <summary>Cloud-ready deployment with managed services.</summary>
    Cloud
}
