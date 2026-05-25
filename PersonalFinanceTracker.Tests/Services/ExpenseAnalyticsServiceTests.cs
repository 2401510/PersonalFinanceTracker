using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.Tests.Services;

public class ExpenseAnalyticsServiceTests
{
    

    private static List<Expense> CreateSampleExpenses() => new()
    {
        new Expense { Id = Guid.NewGuid(), Category = "Еда",       Amount = 100m, Date = new DateTime(2026, 1, 5) },
        new Expense { Id = Guid.NewGuid(), Category = "Еда",       Amount = 50m,  Date = new DateTime(2026, 1, 10) },
        new Expense { Id = Guid.NewGuid(), Category = "Транспорт", Amount = 200m, Date = new DateTime(2026, 1, 15) },
        new Expense { Id = Guid.NewGuid(), Category = "Транспорт", Amount = 75m,  Date = new DateTime(2026, 2, 1) },
        new Expense { Id = Guid.NewGuid(), Category = "Здоровье",  Amount = 300m, Date = new DateTime(2026, 3, 20) },
    };

    [Fact]
    public void FilterByCategory_ExistingCategory_ReturnsOnlyMatching()
    {
        var service = new ExpenseAnalyticsService();
        var expenses = CreateSampleExpenses();

        var result = service.FilterByCategory(expenses, "Еда").ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Equal("Еда", e.Category));
    }

    [Fact]
    public void FilterByCategory_UnknownCategory_ReturnsEmpty()
    {
        var service = new ExpenseAnalyticsService();
        var expenses = CreateSampleExpenses();

        var result = service.FilterByCategory(expenses, "Неизвестная");

        Assert.Empty(result);
    }

    [Fact]
    public void FilterByDateRange_GivenRange_ReturnsOnlyExpensesInRange()
    {
        var service = new ExpenseAnalyticsService();
        var expenses = CreateSampleExpenses();
        var from = new DateTime(2026, 1, 1);
        var to = new DateTime(2026, 1, 31);

        var result = service.FilterByDateRange(expenses, from, to).ToList();

        Assert.Equal(3, result.Count);
        Assert.All(result, e => Assert.InRange(e.Date, from, to));
    }

    [Fact]
    public void GetTotalAmount_MultipleExpenses_ReturnsSumOfAmounts()
    {
        var service = new ExpenseAnalyticsService();
        var expenses = CreateSampleExpenses();

        var total = service.GetTotalAmount(expenses);

        Assert.Equal(725m, total);
    }

    [Fact]
    public void GetTotalAmount_EmptyCollection_ReturnsZero()
    {
        var service = new ExpenseAnalyticsService();

        var total = service.GetTotalAmount(Enumerable.Empty<Expense>());

        Assert.Equal(0m, total);
    }

    [Fact]
    public void GetSummaryByCategory_MixedExpenses_ReturnsCorrectGrouping()
    {
        var service = new ExpenseAnalyticsService();
        var expenses = CreateSampleExpenses();

        var summary = service.GetSummaryByCategory(expenses);

        Assert.Equal(3, summary.Count);
        Assert.Equal(150m, summary["Еда"]);
        Assert.Equal(275m, summary["Транспорт"]);
        Assert.Equal(300m, summary["Здоровье"]);
    }
}
