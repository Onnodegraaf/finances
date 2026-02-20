using System.Runtime.InteropServices;
using System.Text;

namespace Pages;

public class SummaryPage
{
    public readonly Dictionary<DateOnly, Dictionary<string, Transaction>> Data;
    public SummaryPage(Dictionary<DateOnly, Dictionary<string, Transaction>> data)
    {
        Data = data;
    }

    private string ColumnHeader(DateOnly from, DateOnly to)
    {
        var sb = new StringBuilder();
        for (DateOnly date = from; date <= to; date = date.AddMonths(1))
        {
            sb.AppendFormat("""
                <th><span class="year">{0:yyyy}<span><br><span class="month">{0:MMMM}</span></th>
            """, date);
        }

        return $"""
        <tr>
        <th></th>
        <span>{sb}</span>
        </tr>
        """;
    }

    internal IResult Render(string from, string to)
    {
        var labels = Data.Values.SelectMany(x => x.Values).Select(t => new Label(t.Label, t.SubLabel)).Distinct().GroupBy(l => l.Name);
        var minDate = Data.Keys.Min();
        var maxDate = Data.Keys.Max();

        var htmlContent = $"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="utf-8">
            <title>Summary</title>
            {Style.Render(labels)}
        </head>
        <body>
            {MenuBar.Render(Data.Keys)}
            <h1>Summary per label</h1>
            <p>Data from {minDate:yyyy-MM} to {maxDate:yyyy-MM}</p>
            <h1>Spending</h1>
            <table class="summary">
            <thead>
            {ColumnHeader(minDate, maxDate)}
            <thead>
            <tbody>
            {RenderLabels(labels, minDate, maxDate)}
            </tbody>
            </table>

            <h1>Income</h1>
            <table class="summary">
            <thead>
            {ColumnHeader(minDate, maxDate)}
            <thead>
            <tbody>
            {RenderLabels(labels, minDate, maxDate, showIncome: true )}
            </tbody>
            </table>
            
        </body>
        """;
        return Results.Content(htmlContent, "text/html");
    }

    private string RenderLabels(IEnumerable<IGrouping<string, Label>> labels, DateOnly from, DateOnly to, bool showIncome=false)
    {
        var sb = new StringBuilder();
        foreach (var label in labels.OrderBy(kv => kv.Key))
        {
            sb.Append(RenderLabelColumn(label.Key, from, to, showIncome));
            foreach (var ss in label.OrderBy(l => l.SubLabel))
            {
                sb.Append(RenderSubLabelColumn(label.Key, ss.SubLabel, from, to, showIncome));
            }
        }
        return sb.ToString();
    }

    private string RenderLabelColumn(string label, DateOnly from, DateOnly to, bool showIncome)
    {
        var sb = new StringBuilder($"""
        <tr class="label-row">
            <th>{label}</th>
        """);

        for (DateOnly date = from; date <= to; date = date.AddMonths(1))
        {
            var totalOut = Data[date].Where(t => t.Value.Label == label).Sum(t => t.Value.Out);
            var totalIn = Data[date].Where(t => t.Value.Label == label).Sum(t => t.Value.In);
            var total = totalOut > totalIn ? totalOut - totalIn : totalIn - totalOut;

            if ((totalOut > totalIn && !showIncome) || (showIncome && totalIn > totalOut) ) {
                sb.AppendFormat("""
                <td><span class="currency">{0:C}</span></td>
            """, total);
            } else {
                sb.Append("""
                <td></td>
            """);
            }
        }
        sb.AppendLine("</tr>");
        return sb.ToString();
    }
    private string RenderSubLabelColumn(string label, string subLabel, DateOnly from, DateOnly to, bool showIncome)
    {
        var sb = new StringBuilder($"""
        <tr class="sub-label-row">
            <th>{subLabel}</th>
        """);

        for (DateOnly date = from; date <= to; date = date.AddMonths(1))
        {
            var totalOut = Data[date].Where(t => t.Value.Label == label && t.Value.SubLabel == subLabel).Sum(t => t.Value.Out);
            var totalIn = Data[date].Where(t => t.Value.Label == label && t.Value.SubLabel == subLabel).Sum(t => t.Value.In);
            var total = totalOut > totalIn ? totalOut - totalIn : totalIn - totalOut;
            if ((totalOut > totalIn && !showIncome) || (showIncome && totalIn > totalOut) ) {
                sb.AppendFormat("""
                <td><span class="currency">{0:C}</span></td>
            """, total);
            }
            else
            {
                sb.Append("""
                <td></td>
            """);

            }
        }
        sb.AppendLine("</tr>");
        return sb.ToString();
    }


}

