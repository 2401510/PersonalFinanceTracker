using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Repositories;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.ViewModels;

//главный ViewModel
public partial class MainViewModel : BaseViewModel
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ExpenseAnalyticsService _analytics;

    //список расходов, видимый в ui с учётом фильтра
    public ObservableCollection<ExpenseViewModel> Expenses { get; } = new();

    //список названий категорий для выпадающего списка
    public ObservableCollection<string> Categories { get; } = new();

    [ObservableProperty]
    private ExpenseViewModel? _selectedExpense;

    [ObservableProperty]
    private string? _filterCategory;

    [ObservableProperty]
    private DateTime? _dateFrom;

    [ObservableProperty]
    private DateTime? _dateTo;

    //общая сумма расходов из текущего отфильтрованного списка
    public decimal TotalAmount => _analytics.GetTotalAmount(Expenses.Select(vm => vm.ToModel()));

    public MainViewModel(IExpenseRepository expenseRepository, ExpenseAnalyticsService analytics)
    {
        _expenseRepository = expenseRepository;
        _analytics = analytics;

        ReloadExpenses(_expenseRepository.GetAll());
        RefreshCategories();

        Expenses.CollectionChanged += OnExpensesChanged;
    }

    private void OnExpensesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(TotalAmount));
    }

    private void RefreshCategories()
    {
        var distinct = _expenseRepository.GetAll()
            .Select(e => e.Category)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        Categories.Clear();
        foreach (var c in distinct)
            Categories.Add(c);
    }

    private void ReloadExpenses(IEnumerable<Expense> source)
    {
        Expenses.Clear();
        foreach (var expense in source)
            Expenses.Add(ExpenseViewModel.FromModel(expense));
    }

    [RelayCommand]
    private void AddExpense()
    {
        var newExpense = new Expense
        {
            Id = Guid.NewGuid(),
            Date = DateTime.Today,
            Category = FilterCategory ?? string.Empty
        };
        _expenseRepository.Add(newExpense);

        var vm = ExpenseViewModel.FromModel(newExpense);
        Expenses.Add(vm);
        SelectedExpense = vm;

        RefreshCategories();
    }

    [RelayCommand]
    private void EditExpense(ExpenseViewModel? expense)
    {
        if (expense is null)
            return;

        _expenseRepository.Update(expense.ToModel());

        RefreshCategories();
        OnPropertyChanged(nameof(TotalAmount));
    }

    [RelayCommand]
    private void DeleteExpense(ExpenseViewModel? expense)
    {
        if (expense is null)
            return;

        _expenseRepository.Delete(expense.Id);
        Expenses.Remove(expense);

        if (ReferenceEquals(SelectedExpense, expense))
            SelectedExpense = null;

        RefreshCategories();
    }

    [RelayCommand]
    private void ApplyFilter()
    {
        var filtered = _expenseRepository.GetAll();

        if (!string.IsNullOrWhiteSpace(FilterCategory))
            filtered = _analytics.FilterByCategory(filtered, FilterCategory);

        if (DateFrom.HasValue && DateTo.HasValue)
            filtered = _analytics.FilterByDateRange(filtered, DateFrom.Value, DateTo.Value);

        ReloadExpenses(filtered);
    }
}
