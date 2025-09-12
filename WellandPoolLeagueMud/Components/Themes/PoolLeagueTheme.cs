using MudBlazor;

namespace WellandPoolLeagueMud.Components.Themes
{

    /// <summary>
    /// A MudBlazor theme inspired by a classic pool hall.
    /// Features a dark felt green, a rich dark chestnut brown with a wood grain effect, and chalk blue accents.
    /// </summary>
    public class PoolLeagueTheme : MudTheme
    {
        public PoolLeagueTheme()
        {
            // The light theme palette (for users who prefer it)
            PaletteLight = new PaletteLight()
            {
                Primary = "#006442",      // Rich Felt Green
                Secondary = "#5C2C06",    // Dark Chestnut Brown
                Tertiary = "#4A90E2",     // Chalk Blue
                AppbarBackground = "#5C2C06", // Dark Chestnut for App Bar
                AppbarText = "#FFFFFF",
                Background = "#F5F5F5",   // Light gray background
                Surface = "#FFFFFF",      // White for cards, etc.
                TextPrimary = "#212121",
                Success = "#2E7D32",
                Info = "#0288D1",
                Warning = "#F57C00",
                Error = "#C62828"
            };

            // The dark theme palette (as seen in the image)
            PaletteDark = new PaletteDark()
            {
                Primary = "#008356",      // A slightly brighter Felt Green for better contrast
                Secondary = "#5C2C06",    // Dark Chestnut Brown
                Tertiary = "#80bfff",     // A brighter Chalk Blue for highlights
                AppbarBackground = "#5C2C06", // Dark Chestnut for App Bar
                Background = "#1E1E1E",   // A very dark gray, not quite black
                Surface = "#2c2c2c",      // The surface color for cards and dialogs
                TextPrimary = "rgba(255,255,255, 0.85)",
                TextSecondary = "rgba(255,255,255, 0.5)",
                Success = "#4CAF50",
                Info = "#2196F3",
                Warning = "#FF9800",
                Error = "#F44336"
            };

            // Define the typography by creating a new Typography object
            Typography = new Typography()
            {
                Default = new DefaultTypography
                {
                    FontFamily = new[] { "Lato", "sans-serif" }
                },
                H1 = new H1Typography
                {
                    FontFamily = new[] { "Merriweather", "serif" },
                    FontWeight = "700"
                },
                H2 = new H2Typography
                {
                    FontFamily = new[] { "Merriweather", "serif" },
                    FontWeight = "700"
                },
                H3 = new H3Typography
                {
                    FontFamily = new[] { "Merriweather", "serif" },
                    FontWeight = "700"
                },
                H4 = new H4Typography
                {
                    FontFamily = new[] { "Merriweather", "serif" },
                    FontWeight = "700"
                },
                H5 = new H5Typography
                {
                    FontFamily = new[] { "Merriweather", "serif" },
                    FontWeight = "700"
                },
                H6 = new H6Typography
                {
                    FontFamily = new[] { "Merriweather", "serif" },
                    FontWeight = "400"
                }
            };


            // General layout properties
            LayoutProperties = new LayoutProperties()
            {
                DefaultBorderRadius = "8px"
            };
        }
    }
}