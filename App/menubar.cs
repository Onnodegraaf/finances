using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;

namespace Pages;

public sealed class MenuBar()
{
    public static string Render (IEnumerable<DateOnly> months, int? currentYear = null, int? currentMonth = null)
    {
        var minDate = months.Min();
        var maxDate = months.Max();
        DateOnly? current = currentYear == null ? null : new DateOnly(currentYear.Value, currentMonth!.Value, 1);
        DateOnly? prev = current.HasValue && current.Value.AddMonths(-1) >= minDate ? current.Value.AddMonths(-1) : null;
        DateOnly? next = current.HasValue && current.Value.AddMonths(1) <= maxDate ? current.Value.AddMonths(1) : null;

        var prevLink = prev.HasValue ? $"""<li><a href="/{prev!.Value.Year}/{prev!.Value.Month}/transactions.html">Previous Month</a></li>""": "";
        var nextLink = next.HasValue ? $"""<li><a href="/{next!.Value.Year}/{next!.Value.Month}/transactions.html">Next Month</a></li>""": "";

        return $"""
            <ul>
                <li><a href="/summary.html">Summary</a></li>
                {prevLink}
                {nextLink}
                <li><a href="/{maxDate.Year}/{maxDate.Month}/transactions.html">Latest Transactions</a></li>
            </ul>
        """;
    }
}