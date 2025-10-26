using System.Globalization;

namespace BuildingStatePredictionApp.Models.Extensions;

public static class StringExtensions
{
    public static bool TryParseInvariant(this string s, out double result)
    {
        return double.TryParse(s.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }
}
