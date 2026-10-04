using System.Text.RegularExpressions;
using DistributedOrderApi.Application.Common.Interfaces;
using DistributedOrderApi.Application.Dtos;
using DistributedOrderApi.Domain.Enums;

namespace DistributedOrderApi.Infrastructure.Services;

public class MockLlmSearchFilterService : ILlmSearchFilterService
{
    public Task<(OrderSearchFilter Filter, string Explanation)> TranslateQueryAsync(string naturalQuery, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(naturalQuery))
        {
            return Task.FromResult((new OrderSearchFilter(), "No filter applied."));
        }

        var lowerQuery = naturalQuery.ToLowerInvariant();
        string? status = null;
        decimal? minAmount = null;
        decimal? maxAmount = null;
        string? customerId = null;
        DateTime? startDate = null;
        DateTime? endDate = null;
        string? searchKeyword = null;

        // Parse Status
        if (lowerQuery.Contains("delayed") || lowerQuery.Contains("delay"))
        {
            status = OrderStatus.Processing.ToString();
        }
        else if (lowerQuery.Contains("submitted"))
        {
            status = OrderStatus.Submitted.ToString();
        }
        else if (lowerQuery.Contains("processing"))
        {
            status = OrderStatus.Processing.ToString();
        }
        else if (lowerQuery.Contains("completed"))
        {
            status = OrderStatus.Completed.ToString();
        }
        else if (lowerQuery.Contains("cancelled") || lowerQuery.Contains("canceled"))
        {
            status = OrderStatus.Cancelled.ToString();
        }
        else if (lowerQuery.Contains("draft"))
        {
            status = OrderStatus.Draft.ToString();
        }

        // Parse Min Amount (e.g., "over $500", "above 500", "> 500", "more than $100")
        var minMatch = Regex.Match(lowerQuery, @"(?:over|above|more than|>|\>=)\s*\$?(\d+(?:\.\d+)?)");
        if (minMatch.Success && decimal.TryParse(minMatch.Groups[1].Value, out var minVal))
        {
            minAmount = minVal;
        }

        // Parse Max Amount (e.g., "under $200", "below 200", "< 200", "less than $200")
        var maxMatch = Regex.Match(lowerQuery, @"(?:under|below|less than|<|\<=)\s*\$?(\d+(?:\.\d+)?)");
        if (maxMatch.Success && decimal.TryParse(maxMatch.Groups[1].Value, out var maxVal))
        {
            maxAmount = maxVal;
        }

        // Parse Date Ranges
        if (lowerQuery.Contains("last week") || lowerQuery.Contains("past week") || lowerQuery.Contains("last 7 days"))
        {
            startDate = DateTime.UtcNow.AddDays(-7);
        }
        else if (lowerQuery.Contains("last month") || lowerQuery.Contains("past month") || lowerQuery.Contains("last 30 days"))
        {
            startDate = DateTime.UtcNow.AddDays(-30);
        }
        else if (lowerQuery.Contains("today"))
        {
            startDate = DateTime.UtcNow.Date;
        }

        // Parse Customer ID (e.g., "cust-100", "cust-int-01")
        var custMatch = Regex.Match(naturalQuery, @"(CUST-[A-Za-z0-9-]+)", RegexOptions.IgnoreCase);
        if (custMatch.Success)
        {
            customerId = custMatch.Groups[1].Value;
        }

        // Parse Keyword Search (e.g. keyboard, monitor, headphones, cable)
        string[] commonKeywords = ["keyboard", "monitor", "headphones", "cable", "mouse"];
        foreach (var kw in commonKeywords)
        {
            if (lowerQuery.Contains(kw))
            {
                searchKeyword = kw;
                break;
            }
        }

        var filter = new OrderSearchFilter(
            Status: status,
            MinAmount: minAmount,
            MaxAmount: maxAmount,
            CustomerId: customerId,
            StartDate: startDate,
            EndDate: endDate,
            SearchKeyword: searchKeyword
        );

        var explanationParts = new List<string>();
        if (status != null) explanationParts.Add($"status={status}");
        if (minAmount != null) explanationParts.Add($"total >= ${minAmount}");
        if (maxAmount != null) explanationParts.Add($"total <= ${maxAmount}");
        if (customerId != null) explanationParts.Add($"customer={customerId}");
        if (startDate != null) explanationParts.Add($"created after {startDate.Value:yyyy-MM-dd}");
        if (searchKeyword != null) explanationParts.Add($"keyword='{searchKeyword}'");

        var explanation = explanationParts.Count > 0
            ? $"Showing orders with {string.Join(", ", explanationParts)} based on query: \"{naturalQuery}\"."
            : $"Showing orders matching query: \"{naturalQuery}\".";

        return Task.FromResult((filter, explanation));
    }
}
