using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WellandPoolLeagueMud.Data.ViewModels;
using WellandPoolLeagueMud.ViewModels;

namespace WellandPoolLeagueMud.Reports;

public class PlayerStandingsReport : IDocument
{
    private readonly List<PlayerStandingViewModel> _standings;
    private readonly byte[] _logoImageData;

    public PlayerStandingsReport(List<PlayerStandingViewModel> standings, byte[] logoImageData)
    {
        _standings = standings;
        _logoImageData = logoImageData;
    }

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(50);
            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
        });
    }

    void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.ConstantItem(80).Image(_logoImageData);
            row.RelativeItem().PaddingLeft(20).Column(column =>
            {
                column.Item().Text("Welland Pool League").SemiBold().FontSize(20);
                column.Item().Text("Player Standings Report");
            });
        });
    }

    void ComposeContent(IContainer container)
    {
        container.PaddingVertical(40).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3); // Player
                columns.RelativeColumn(2); // Team
                columns.ConstantColumn(50); // GP
                columns.ConstantColumn(50); // GW
                columns.ConstantColumn(50); // GL
                columns.ConstantColumn(60); // Win %
            });

            table.Header(header =>
            {
                header.Cell().Text("Player").Bold();
                header.Cell().Text("Team").Bold();
                header.Cell().Text("GP").Bold();
                header.Cell().Text("GW").Bold();
                header.Cell().Text("GL").Bold();
                header.Cell().Text("Win %").Bold();
            });

            foreach (var player in _standings)
            {
                table.Cell().Text(player.PlayerName);
                table.Cell().Text(player.TeamName);
                table.Cell().Text(player.GamesPlayed.ToString());
                table.Cell().Text(player.Wins.ToString());
                table.Cell().Text(player.Losses.ToString());
                table.Cell().Text(player.WinPercentage.ToString("F1"));
            }
        });
    }
}