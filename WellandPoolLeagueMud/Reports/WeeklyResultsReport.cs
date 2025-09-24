using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WellandPoolLeagueMud.Data.ViewModels;

namespace WellandPoolLeagueMud.Reports;

public class WeeklyResultsReport : IDocument
{
    private readonly List<ScheduleViewModel> _results;
    private readonly int _weekNumber;
    private readonly byte[] _logoImageData;

    public WeeklyResultsReport(List<ScheduleViewModel> results, int weekNumber, byte[] logoImageData)
    {
        _results = results;
        _weekNumber = weekNumber;
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
                column.Item().Text("Welland Pool League")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);
                column.Item().Text($"Week {_weekNumber} - Match Results");
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
                columns.ConstantColumn(25);
                columns.RelativeColumn(3);
                columns.RelativeColumn(4);
            });
            table.Header(header =>
            {
                header.Cell().Text("Home Team").Bold();
                header.Cell().Text("");
                header.Cell().Text("Away Team").Bold();
                header.Cell().Text("Winning Team").Bold();
            });
            foreach (var game in _results.OrderBy(g => g.HomeTeamName))
            {
                var winnerName = game.WinningTeamId == game.HomeTeamId ? game.HomeTeamName : game.AwayTeamName;
                table.Cell().Text(game.HomeTeamName);
                table.Cell().AlignCenter().Text("vs.");
                table.Cell().Text(game.AwayTeamName);
                table.Cell().Text(winnerName).SemiBold();
            }
        });
    }
}