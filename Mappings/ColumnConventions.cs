namespace Ecommerce.Api.Mappings;

/// <summary>
/// Shared column conventions so every money column is stored identically.
/// </summary>
public static class ColumnConventions
{
    /// <summary>decimal(18, 2) - used for every monetary value (prices, totals, fees, discounts).</summary>
    public const string Money = "decimal(18, 2)";

    public const int MaxUrlLength = 500;
    public const int MaxNameLength = 200;
    public const int MaxShortTextLength = 100;
    public const int MaxLongTextLength = 2000;
}
