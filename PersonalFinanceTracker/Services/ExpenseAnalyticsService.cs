using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services;

//сервис аналитики по расходам
public class ExpenseAnalyticsService
{
    public IEnumerable<Expense> FilterByCategory(IEnumerable<Expense> expenses, string category)
    {
        //если пустая категория то фильтр не применяем
        if (string.IsNullOrWhiteSpace(category))
            return expenses;

        return expenses.Where(e =>
            string.Equals(e.Category, category, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<Expense> FilterByDateRange(IEnumerable<Expense> expenses, DateTime from, DateTime to)
    {
        return expenses.Where(e => e.Date >= from && e.Date <= to);
    }
    public decimal GetTotalAmount(IEnumerable<Expense> expenses)
    {
        return expenses.Sum(e => e.Amount);
    }

    public Dictionary<string, decimal> GetSummaryByCategory(IEnumerable<Expense> expenses)
    {
        return expenses
            .GroupBy(e => e.Category)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
    }
}
