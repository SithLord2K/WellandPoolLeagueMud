using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WellandPoolLeagueMud.Data.Models;

[Keyless]
public partial class WeeksView
{
    public int Week_Id { get; set; }
    public int Home_Team { get; set; }
    public int Away_Team { get; set; }
    public bool Forfeit { get; set; }
    public bool Home_Won { get; set; }
    public bool Playoff { get; set; }
    public int WinningTeamId { get; set; }
}
