namespace Reservant.Api.Identity;

/// <summary>
/// Strings used as role names. Having them as constants helps with
/// </summary>
internal static class Roles
{
    /// <summary>
    /// Klient.
    /// </summary>
    public const string Customer = "Customer";

    /// <summary>
    /// Właściciel lokalu.
    /// </summary>
    public const string RestaurantOwner = "RestaurantOwner";

    /// <summary>
    /// Pracownik lokalu.
    /// </summary>
    public const string RestaurantEmployee = "RestaurantEmployee";

    /// <summary>
    /// Pracownik zaplecza.
    /// </summary>
    public const string RestaurantBackdoorsEmployee = "RestaurantBackdoorsEmployee";

    /// <summary>
    /// Pracownik sali.
    /// </summary>
    public const string RestaurantHallEmployee = "RestaurantHallEmployee";

    /// <summary>
    /// Pracownik BOK.
    /// </summary>
    public const string CustomerSupportAgent = "CustomerSupportAgent";

    /// <summary>
    /// Kierownik BOK.
    /// </summary>
    public const string CustomerSupportManager = "CustomerSupportManager";
}
