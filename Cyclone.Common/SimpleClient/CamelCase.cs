namespace Cyclone.Common.SimpleClient;

public static class CamelCase
{
    public static string ToCamelCase(this string s)
    {
        if (string.IsNullOrEmpty(s) || !char.IsUpper(s[0]))
            return s;

        var chars = s.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (i == 0 || (i + 1 < chars.Length && char.IsUpper(chars[i + 1])))
                chars[i] = char.ToLowerInvariant(chars[i]);
            else
                break;
        }

        return new string(chars);
    }

}