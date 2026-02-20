
using System.Text;

namespace Pages;

public sealed record Label(string Name, string SubLabel);
public sealed record Summary(string Name, string SubLabel, decimal Out, decimal In);
public sealed class Transactions
{
    public readonly Dictionary<DateOnly, Dictionary<string, Transaction>> Data;
    public Transactions(Dictionary<DateOnly, Dictionary<string, Transaction>> data)
    {
        Data = data;
    }

    private string RenderMonthView(DateOnly currentMonth)
    {

        var data = Data[currentMonth].Values.GroupBy(x => x.Date).ToDictionary(t=> t.Key.Day, t=> new {
                Out = t.Sum(t => t.Out),
                In = t.Sum(t => t.In)  });
        
        var maxOut = data.Max(x => x.Value.Out);
        var maxIn = data.Max(x => x.Value.In);

        var sb = new StringBuilder();

        for(var dayNumber = 1; dayNumber <= DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month); dayNumber++)
        {
            
            if( data.ContainsKey(dayNumber)) {
            sb.AppendFormat("""
            <div>
                {0}
                <span class="out">{1:C}</span>
                <span class="in">{2:C}</span>
            </div>
            """, dayNumber, data[dayNumber].Out, data[dayNumber].In);
            } else
            {
                sb.AppendFormat("""
                <div>{0}</div>
                """, dayNumber);
            }
        }
        

        return $"""
        <div id="calendar-grouping">
            {sb}
        </div>
        """;
    }



    public IResult Render(int year, int month)
    {

        var labels = Data.Values.SelectMany(x => x.Values).Select(t => new Label(t.Label, t.SubLabel)).GroupBy(l => l.Name);
        var lableBuilder = new StringBuilder();
        var totalbuilder = new StringBuilder();

        foreach (var sub in labels.SelectMany(s => s.Select(l => l.SubLabel)).Distinct().OrderBy(l => l))
        {
            lableBuilder.AppendLine($"<option value=\"{sub}\"></option>");
        }

        var sb = new StringBuilder();
        Dictionary<string, decimal?> currencyOut = [];
        Dictionary<string, decimal?> currencyIn = [];
        var currentMonth = new DateOnly(year, month, 1);

        if (Data.ContainsKey(currentMonth))
        {
            currencyOut = Data[currentMonth].Values.GroupBy(t => t.Label).ToDictionary(t => t.Key, t => t.Sum(c => c.Out) - t.Sum(c => c.In));
            currencyIn = Data[currentMonth].Values.GroupBy(t => t.Label).ToDictionary(t => t.Key, t => t.Sum(c => c.In) - t.Sum(c => c.Out));


            var subTotals = Data[currentMonth].Values.GroupBy(t => t.Label).ToDictionary(t => t.Key, t => new { TotalIn = t.Sum(c => c.In), TotalOut = t.Sum(c => c.Out) });

            foreach (var total in subTotals)
            {
                if (total.Value.TotalOut == 0M && total.Value.TotalIn == 0M) continue;
                totalbuilder.AppendFormat("""
                    <tr class="label-{0}">
                        <td></td>
                        <td>{0}</td>
                        <td><input class="currency" name="Out" value="{1:C}" readonly/></td>
                        <td><input class="currency" name="In" value="{2:C}" readonly/></td>
                        <td></td>
                        <td></td>
                    </tr>
                    """, total.Key, total.Value.TotalOut, total.Value.TotalIn);
            }

            foreach (var t in Data[currentMonth].Values.OrderBy(x => x.Date))
            {

                sb.AppendFormat("""
                    <tr class="label-{4} label-all">
                        <td>
                            <input type="hidden" name="ID" value="{5}" />
                            <input name="Date" value="{0}" readonly />
                        </td>
                        <td><input name="Description" value="{1}" readonly/></td>
                        <td><input class="currency" name="Out" value="{2:C}" readonly/></td>
                        <td><input class="currency" name="In" value="{3:C}" readonly/></td>
                        <td>
                            <input list="label-list" name="Label" value="{4}" />
                        </td>
                        <td>
                            <input list="sub-label-list" name="SubLabel" value="{6}" />
                        </td>
                    </tr>
                    """, t.Date, t.Description, t.Out, t.In, t.Label, t.ID, t.SubLabel);
            }
        }


        var totalOut = new StringBuilder();
        foreach (var t in currencyOut.Where(tt => tt.Value > 0M))
        {
            totalOut.AppendFormat("""
                <tr>
                    <td>{0}</td>
                    <td>{1:C}</td>
                </tr>
            """, t.Key, t.Value);
        }
        totalOut.AppendFormat("""
                <tr>
                    <td>Total</td>
                    <td>{0:C}</td>
                </tr>
            """, currencyOut.Where(c => c.Value > 0M).Sum(c => c.Value));

        var totalIn = new StringBuilder();
        foreach (var t in currencyIn.Where(tt => tt.Value > 0M))
        {
            totalIn.AppendFormat("""
                <tr>
                    <td>{0}</td>
                    <td>{1:C}</td>
                </tr>
            """, t.Key, t.Value);
        }
        totalIn.AppendFormat("""
                <tr>
                    <td>Total</td>
                    <td>{0:C}</td>
                </tr>
            """, currencyIn.Where(c => c.Value > 0M).Sum(c => c.Value));
        string summary = $"""

        <h2>Out</h2>
        <table>
        <thead>
        <tr><th></th><th>Total</th></tr>
        </thead>
        <tbody>
         {totalOut.ToString()}
        </tbody>
        </table>

        <h2>In</h2>
        <table>
        <thead>
        <tr><th></th><th>Total</th></tr>
        </thead>
        <tbody>
         {totalIn.ToString()}
        </tbody>
        </table>
    """;
        string htmlContent = $"""
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <meta charset="utf-8">
        <title>Money in {currentMonth:MMMM}</title>
        {Style.Render(labels)}
    </head>
    <body>
        <h1>Transactions for {currentMonth:MMMM yyyy}</h1>
        {LabelSelector(labels)}
        <input type="checkbox" id="toggle-Bills" class="toggle-Bills">
        <label for="toggle-Bills">Bills</label>
    
        <form action="/update" method="post" name="transactions">
        <datalist id="label-list">
            {String.Join('\n', labels.OrderBy(l => l.Key).Select(l => $"<option value=\"{l.Key}\"></option>"))}
        </datalist>
        <datalist id="sub-label-list">{lableBuilder}</datalist>
        <table>
        <thead>
        <tr><th>Date</th><th>Description</th><th>Out</th><th>In</th><th>Label</th><th>Sub Label</th></tr>
        </thead>
        <tbody>
        {sb}
        </tbody>
        <tfoot>
            {totalbuilder}
            <tr class="label-all"><td></td><td></td><td class="currency">{Data[currentMonth].Values.Sum(i => i.Out):C}</td><td class="currency">{Data[currentMonth].Values.Sum(i => i.In):C}</td><td></td><td></td></tr>
        </tfoot>

        </table>
        <input type="submit">Save Changes</button>

        </form>
        {summary}

        <form action="/import" method="post" enctype="multipart/form-data">
        <label for="file-upload">Select file:</label>
        <input type="file" id="file-upload" name="file" accept=".csv" />
        <input type="submit">Import File</button>
        </form>

        <h2>calendar view</h2>
        {RenderMonthView(currentMonth)}
    </body>
    </html>
    """;
        return Results.Content(htmlContent, "text/html");
    }

    private string LabelSelector(IEnumerable<IGrouping<string, Label>> labels)
    {
        var sb = new StringBuilder("""
            <div id="label-selector">
            <label for="label-checkbox-everything">All
            <input type="radio" name="label-selector" id="label-checkbox-everything"  checked/>
            </label>
        """);
        foreach (var label in labels)
        {
            sb.AppendFormat("""
                <label for="label-checkbox-{0}">{0}
                <input type="radio" name="label-selector" id="label-checkbox-{0}" />
                </label>

            """, label.Key);
        }
        sb.AppendLine("</div>");
        return sb.ToString();
    }

    

}
