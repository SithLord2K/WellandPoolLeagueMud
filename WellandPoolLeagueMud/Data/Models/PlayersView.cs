using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WellandPoolLeagueMud.Data.Models;

[Keyless]
public partial class PlayersView
{
    public int PlayerId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int GamesWon { get; set; }
    public int GamesLost { get; set; }
    public int GamesPlayed { get; set; }
    public decimal Average { get; set; }
}
