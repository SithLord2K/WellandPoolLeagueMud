using Microsoft.EntityFrameworkCore;

namespace WellandPoolLeagueMud.Data.Models;

[Keyless]
public partial class TeamDetail
{
    public int Id { get; set; }
    public string TeamName { get; set; } = null!;
    public int Captain_Player_Id { get; set; }
}
