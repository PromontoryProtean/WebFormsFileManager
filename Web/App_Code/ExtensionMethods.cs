using System;
using System.Globalization;
using System.Text.RegularExpressions;

public static class ExtensionMethods
{
    public static string Slugify(this string s)
    {
        s = s.ToLower();
        s = s.Replace("-", " ");
        s = Regex.Replace(s, "[^a-z0-9\\s-]", " ");
        s = Regex.Replace(s, "\\s+", " ").Trim();
        s = s.Replace(" ", "-");
        return s;
    }
    public static bool IsNumeric(this object obj)
    {
        if (obj == null)
            return false;
        double number;
        return double.TryParse(Convert.ToString(obj, CultureInfo.InvariantCulture), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out number);
    }
}