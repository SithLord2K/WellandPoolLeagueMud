using Microsoft.EntityFrameworkCore;

namespace WellandPoolLeagueMud.Data.Models;

[Keyless]
public partial class PlayerDatum
{
    public int PlayerId { get; set; }
    public int GamesWon { get; set; }
    public int GamesLost { get; set; }
    public int WeekNumber { get; set; }
    public int GamesPlayed { get; set; }
}
