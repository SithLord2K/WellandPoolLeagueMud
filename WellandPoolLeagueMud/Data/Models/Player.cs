using Microsoft.EntityFrameworkCore;

namespace WellandPoolLeagueMud.Data.Models;

[Keyless]
public class Player
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string? LastName { get; set; }
    public int TeamId { get; set; }
}
