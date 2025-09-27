using MudBlazor;

namespace WellandPoolLeagueMud.Helpers
{
    public static class ColorHelper
    {
        public static Color GetWinPercentageColor(decimal winPercentage)
        {
            return winPercentage switch
            {
                >= 75 => Color.Success,
                >= 60 => Color.Info,
                >= 50 => Color.Warning,
                >= 25 => Color.Secondary,
                _ => Color.Error
            };
        }
    }
}