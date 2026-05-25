using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Repositories;

namespace PersonalFinanceTracker.Tests.Repositories;

public class ExpenseRepositoryTests
{
    private static Expense CreateExpense(
        string category = "Еда",
        decimal amount = 100m,
        string description = "обед")
    {
        return new Expense
        {
            Id = Guid.NewGuid(),
            Category = category,
            Amount = amount,
            Date = new DateTime(2026, 1, 1),
            Description = description
        };
    }

    [Fact]
    public void Add_NewExpense_AppearsInGetAll()
    {
        var repository = new ExpenseRepository();
        var expense = CreateExpense();

        repository.Add(expense);

        var all = repository.GetAll().ToList();
        Assert.Single(all);
        Assert.Contains(all, e => e.Id == expense.Id);
    }

    [Fact]
    public void Add_ExpenseWithEmptyId_GeneratesNewId()
    {
        

        var repository = new ExpenseRepository();
        var expense = new Expense { Id = Guid.Empty, Category = "Еда", Amount = 10m };

        repository.Add(expense);

        Assert.NotEqual(Guid.Empty, expense.Id);
    }

    [Fact]
    public void Delete_ExistingExpense_RemovesItFromRepository()
    {
        var repository = new ExpenseRepository();
        var expense = CreateExpense();
        repository.Add(expense);

        repository.Delete(expense.Id);

        Assert.Empty(repository.GetAll());
    }

    [Fact]
    public void Delete_NonExistingId_DoesNotThrow()
    {
        var repository = new ExpenseRepository();
        var expense = CreateExpense();
        repository.Add(expense);
        repository.Delete(Guid.NewGuid());

        Assert.Single(repository.GetAll());
    }

    [Fact]
    public void Update_ExistingExpense_ChangesStoredValues()
    {
        var repository = new ExpenseRepository();
        var expense = CreateExpense();
        repository.Add(expense);

        var updated = new Expense
        {
            Id = expense.Id,
            Category = "Транспорт",
            Amount = 250m,
            Date = new DateTime(2026, 2, 2),
            Description = "такси"
        };
        repository.Update(updated);

        var stored = repository.GetById(expense.Id);
        Assert.NotNull(stored);
        Assert.Equal("Транспорт", stored!.Category);
        Assert.Equal(250m, stored.Amount);
        Assert.Equal("такси", stored.Description);
    }

    [Fact]
    public void GetById_ExistingId_ReturnsExpense()
    {
        var repository = new ExpenseRepository();
        var expense = CreateExpense();
        repository.Add(expense);

        var result = repository.GetById(expense.Id);

        Assert.NotNull(result);
        Assert.Equal(expense.Id, result!.Id);
    }

    [Fact]
    public void GetById_NonExistingId_ReturnsNull()
    {
        var repository = new ExpenseRepository();
        repository.Add(CreateExpense());

        var result = repository.GetById(Guid.NewGuid());

        Assert.Null(result);
    }
}
