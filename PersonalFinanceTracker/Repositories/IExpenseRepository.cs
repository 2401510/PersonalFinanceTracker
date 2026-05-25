using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Repositories;

//репозиторий расходов
public interface IExpenseRepository
{
    IEnumerable<Expense> GetAll();

    Expense? GetById(Guid id);

    void Add(Expense expense);

    void Update(Expense expense);

    void Delete(Guid id);
}
