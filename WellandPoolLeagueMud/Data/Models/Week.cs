using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WellandPoolLeagueMud.Data.Models;

public partial class Week
{
    [Key]
    public int Id { get; set; }

    public int WeekNumber { get; set; }

    public int Away_Team { get; set; }

    public int Home_Team { get; set; }

    public bool Forfeit { get; set; }

    public bool Playoff { get; set; }

    public int? WinningTeamId { get; set; }
}
