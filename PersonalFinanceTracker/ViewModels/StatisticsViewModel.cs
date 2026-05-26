using System.Collections.ObjectModel;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.ViewModels;

//ViewModel окна статистики
public class StatisticsViewModel : BaseViewModel
{
    public ObservableCollection<StatisticsItemViewModel> Items { get; } = new();

    public decimal TotalAmount { get; }

    public StatisticsViewModel(IEnumerable<Expense> expenses, ExpenseAnalyticsService analytics)
    {
        var snapshot = expenses.ToList();

        var summary = analytics.GetSummaryByCategory(snapshot);
        var counts = snapshot
            .GroupBy(e => e.Category)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        TotalAmount = analytics.GetTotalAmount(snapshot);

        //сортируем по убыванию сумм
        foreach (var pair in summary.OrderByDescending(p => p.Value))
        {
            var percent = TotalAmount > 0 ? pair.Value / TotalAmount * 100m : 0m;

            Items.Add(new StatisticsItemViewModel
            {
                Category = pair.Key,
                Amount = pair.Value,
                Count = counts.TryGetValue(pair.Key, out var c) ? c : 0,
                Percent = percent
            });
        }
    }
}

//строка таблицы статистики
public class StatisticsItemViewModel
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Count { get; set; }
    public decimal Percent { get; set; }
}
