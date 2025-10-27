using System.Drawing;

namespace CS2Tags_VipTag
{
    public class PluginUtilities()
    {
        public static string? FromNameToHex(string name)
        {
            try
            {
                Dictionary<string, string> predefinedColors = new Dictionary<string, string>
                {
                {"CTBlue", "#5F99D9"},
                {"BlueGrey", "#B1C4D9"},
                {"Grey", "#C6CBD0"},
                {"LightPurple", "#BB82F0"},
                {"LightRed", "#EB4C4C"},
            };
                Color color = Color.FromName(name);
                if (!color.IsKnownColor && !color.IsNamedColor) return null;
                if (predefinedColors.ContainsKey(name))
                {
                    return predefinedColors[name];
                }
                return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            }
            catch
            {
                return null;
            }
        }
    }
}