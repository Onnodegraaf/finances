using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Pages;


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


Dictionary<DateOnly, Dictionary<string, Transaction>> transactionData = [];
if(File.Exists("data.json")) {
    transactionData = JsonSerializer.Deserialize<Dictionary<DateOnly, Dictionary<string, Transaction>>>(File.ReadAllText("data.json"));
}
var transactionsPage = new Transactions(transactionData);
var summaryPage = new SummaryPage(transactionData);

app.MapPost("/update",async (HttpRequest request) =>
{
    var form = await request.ReadFormAsync();
    var recipients = new List<Transaction>();
    TextInfo myTI = new CultureInfo("en-US",false).TextInfo;
    // Manual processing requires knowing the form field names/structure
    // For example, if form fields are like Recipient[0].Name, Recipient[0].Email, etc.
    
    Console.WriteLine($"trying to update data: {form["ID"].Count}");
    for(var index = 0; index!= form["ID"].Count; index++)
    {
        decimal? amountOut = null;
        decimal? amountIn = null;
        if(decimal.TryParse(form["Out"][index], NumberStyles.Currency, CultureInfo.CurrentCulture.NumberFormat, out var amount))  amountOut = amount ;
        if(decimal.TryParse(form["In"][index], NumberStyles.Currency, CultureInfo.CurrentCulture.NumberFormat,out amount))  amountIn = amount ;
            

        var transaction = new Transaction( ) {
            Date= DateOnly.Parse(form["Date"][index]),
            Description = form["Description"][index],
            Out = amountOut,
            In = amountIn,
            Label = myTI.ToTitleCase(form["Label"][index].Trim()),
            SubLabel = myTI.ToTitleCase(form["SubLabel"][index].Trim()),
            };
        var currentMonth = new DateOnly(transaction.Date.Year, transaction.Date.Month, 1);
        Console.WriteLine($"trying to resolve: {currentMonth}: {form["ID"][index]} with {transaction}");
        if(transactionData.ContainsKey(currentMonth))
        {
            if(transactionData[currentMonth].ContainsKey(form["ID"][index]))
            {
                Console.WriteLine($"Updated transaction: {transaction}");
                transactionData[currentMonth][form["ID"][index]] = transaction;
            } else
            {
                transactionData[currentMonth].Add(transaction.ID, transaction);
            }
        }  else
        {
            transactionData.Add(currentMonth, new Dictionary<string, Transaction>{{transaction.ID, transaction}});
        }
        
    }
    File.WriteAllText("data.json", JsonSerializer.Serialize(transactionData));
    

    var date3 = DateOnly.Parse("2026-02-01");
    return Results.Redirect($"/{date3.Year}/{date3.Month}/transactions.html"); 

    
}).DisableAntiforgery();

app.MapPost("/import", async (IFormFile file) =>
{
    Transaction [] transactions = [];
    Console.WriteLine(file.FileName);
    using (var reader = new StreamReader(file.OpenReadStream()))
    {
        transactions = Transaction.ReadCsvRecords(reader);
    }

    foreach(Transaction trans in transactions)
    {
        var month = new DateOnly(trans.Date.Year, trans.Date.Month, 1);
        if(!transactionData.ContainsKey(month))
        {
            transactionData.Add(month, []);
        }
        if(transactionData[month].ContainsKey(trans.ID)) {
            transactionData[month][trans.ID] = trans;
        } else
        {
            transactionData[month].Add(trans.ID, trans);
        }
    }

    File.WriteAllText("data.json", JsonSerializer.Serialize(transactionData));
    
    var date = transactions.Max(x => x.Date);
    return Results.Redirect($"/{date.Year}/{date.Month}/transactions.html") ;
}).DisableAntiforgery();

app.MapGet("/{year}/{month}/transactions.html", (int year, int month) => transactionsPage.Render(year, month));
app.MapGet("/summary.html", ([FromQuery]string?from, [FromQuery]string? to) => summaryPage.Render(from, to));
app.Run();


