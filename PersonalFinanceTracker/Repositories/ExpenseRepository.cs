using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Repositories;

//реализация репозитория расходов в памяти
public class ExpenseRepository : IExpenseRepository
{
    private readonly List<Expense> _expenses = new();

    public IEnumerable<Expense> GetAll() => _expenses;

    public Expense? GetById(Guid id) => _expenses.FirstOrDefault(e => e.Id == id);

    public void Add(Expense expense)
    {
        //если id не задан, генерируем новый
        if (expense.Id == Guid.Empty)
            expense.Id = Guid.NewGuid();

        _expenses.Add(expense);
    }

    public void Update(Expense expense)
    {
        var index = _expenses.FindIndex(e => e.Id == expense.Id);
        if (index >= 0)
            _expenses[index] = expense;
    }

    public void Delete(Guid id)
    {
        _expenses.RemoveAll(e => e.Id == id);
    }
}
