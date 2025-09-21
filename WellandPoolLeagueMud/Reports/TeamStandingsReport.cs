using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WellandPoolLeagueMud.Data.ViewModels;

namespace WellandPoolLeagueMud.Reports;

public class TeamStandingsReport : IDocument
{
    private readonly List<TeamStandingViewModel> _standings;
    private readonly byte[] _logoImageData;

    public TeamStandingsReport(List<TeamStandingViewModel> standings, byte[] logoImageData)
    {
        _standings = standings;
        _logoImageData = logoImageData;
    }

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Margin(50);
                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                });
            });
    }

    void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.ConstantItem(80).Image(_logoImageData);

            row.RelativeItem().PaddingLeft(20).Column(column =>
            {
                column.Item().Text("Welland Pool League")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);
                column.Item().Text("Overall Team Standings Report");
            });
        });
    }

    void ComposeContent(IContainer container)
    {
        container.PaddingVertical(40).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
                columns.ConstantColumn(75);
                columns.ConstantColumn(75);
            });
            table.Header(header =>
            {
                header.Cell().Text("Team Name").Bold();
                header.Cell().Text("Captain").Bold();
                header.Cell().Text("Weeks Won").Bold();
                header.Cell().Text("Games Won").Bold();
            });
            foreach (var team in _standings.OrderByDescending(s => s.WeeksWon).ThenByDescending(s => s.Wins))
            {
                table.Cell().Text(team.TeamName);
                table.Cell().Text(team.CaptainName);
                table.Cell().Text(team.WeeksWon.ToString());
                table.Cell().Text(team.Wins.ToString());
            }
        });
    }
}