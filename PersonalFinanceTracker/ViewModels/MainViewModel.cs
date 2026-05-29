using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Repositories;
using PersonalFinanceTracker.Services;
using PersonalFinanceTracker;

namespace PersonalFinanceTracker.ViewModels;

//главный ViewModel
public partial class MainViewModel : BaseViewModel
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ExpenseAnalyticsService _analytics;
    private readonly ImportExportService _importExport;

    public ObservableCollection<ExpenseViewModel> Expenses { get; } = new();
    public ObservableCollection<string> Categories { get; } = new();

    [ObservableProperty]
    private ExpenseViewModel? _selectedExpense;

    //форма ввода (правая панель): к ней привязаны TextBox'ы и DatePicker
    [ObservableProperty]
    private ExpenseViewModel _draftExpense = new() { Date = DateTime.Today };

    // Лимит расходов для категории
    [ObservableProperty]
    private decimal? _draftLimit;

    [ObservableProperty]
    private string? _filterCategory;

    [ObservableProperty]
    private DateTime? _dateFrom;

    [ObservableProperty]
    private DateTime? _dateTo;

    public decimal TotalAmount =>
        _analytics.GetTotalAmount(Expenses.Select(vm => vm.ToModel()));

    public MainViewModel(
        IExpenseRepository expenseRepository,
        ICategoryRepository categoryRepository,
        ExpenseAnalyticsService analytics,
        ImportExportService importExport)
    {
        _expenseRepository = expenseRepository;
        _categoryRepository = categoryRepository;
        _analytics = analytics;
        _importExport = importExport;

        ReloadExpenses(_expenseRepository.GetAll());
        RefreshCategories();

        //пересчёт TotalAmount при изменении коллекции
        Expenses.CollectionChanged += OnExpensesChanged;
    }

    private void OnExpensesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(TotalAmount));
    }

    // При выборе строки копируем её значения в форму, чтобы можно было редактировать
    partial void OnSelectedExpenseChanged(ExpenseViewModel? value)
    {
        if (value is null)
            return;

        DraftExpense = new ExpenseViewModel
        {
            Id = value.Id,
            Category = value.Category,
            Amount = value.Amount,
            Date = value.Date,

            Description = value.Description
        };

        // подтягиваем текущий лимит выбранной категории если есть
        DraftLimit = _categoryRepository.GetAll()
            .FirstOrDefault(c => string.Equals(c.Name, value.Category, StringComparison.OrdinalIgnoreCase))
            ?.LimitAmount;
    }

    //сброс формы к пустой записи на сегодня
    private void ResetDraft()
    {
        DraftExpense = new ExpenseViewModel { Date = DateTime.Today };
        DraftLimit = null;
    }

    private void RefreshCategories()
    {
        //собираем категории и из расходов, и из репозитория категорий
        var fromExpenses = _expenseRepository.GetAll()
            .Select(e => e.Category)
            .Where(c => !string.IsNullOrWhiteSpace(c));
        var fromCategoryRepo = _categoryRepository.GetAll().Select(c => c.Name);

        var distinct = fromExpenses
            .Concat(fromCategoryRepo)
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

    // Проверка лимита по категории и предупреждение при превышении
    private void CheckCategoryLimit(string? categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            return;

        var category = _categoryRepository.GetAll()
            .FirstOrDefault(c => string.Equals(c.Name, categoryName, StringComparison.OrdinalIgnoreCase));

        if (category?.LimitAmount is not decimal limit)
            return;

        var spent = _analytics.GetTotalAmount(
            _analytics.FilterByCategory(_expenseRepository.GetAll(), categoryName));

        if (spent > limit)
        {
            MessageBox.Show(
                $"Расходы по категории «{categoryName}» ({spent:N2}) превысили лимит ({limit:N2}).",
                "Превышение лимита",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    //создает категорию или обновляет ее лимит если категория уже есть
    private void UpsertCategory(string? categoryName, decimal? limit)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            return;

        var category = _categoryRepository.GetAll()
            .FirstOrDefault(c => string.Equals(c.Name, categoryName, StringComparison.OrdinalIgnoreCase));

        if (category is null)
        {
            _categoryRepository.Add(new Category
            {
                Name = categoryName,
                LimitAmount = limit
            });
        }
        else if (limit.HasValue)
        {
            //обновляем лимит только если пользователь явно ввел значение
            category.LimitAmount = limit;
            _categoryRepository.Update(category);
        }
    }

    [RelayCommand]
    private void AddExpense()
    {
        //берём значения из формы (DraftExpense), а не создаём пустую запись
        var newExpense = new Expense
        {
            Id = Guid.NewGuid(),
            Category = DraftExpense.Category,
            Amount = DraftExpense.Amount,
            Date = DraftExpense.Date,
            Description = DraftExpense.Description
        };
        _expenseRepository.Add(newExpense);

        Expenses.Add(ExpenseViewModel.FromModel(newExpense));

        UpsertCategory(newExpense.Category, DraftLimit);

        //сбрасываем выбор и форму, чтобы можно было сразу ввести следующую запись
        SelectedExpense = null;
        ResetDraft();

        RefreshCategories();
        CheckCategoryLimit(newExpense.Category);
    }

    [RelayCommand]
    private void EditExpense(ExpenseViewModel? expense)
    {
        if (expense is null)
            return;

        // Применяем значения из формы к выбранной записи
        expense.Category = DraftExpense.Category;
        expense.Amount = DraftExpense.Amount;
        expense.Date = DraftExpense.Date;
        expense.Description = DraftExpense.Description;

        _expenseRepository.Update(expense.ToModel());

        UpsertCategory(expense.Category, DraftLimit);

        RefreshCategories();
        OnPropertyChanged(nameof(TotalAmount));
        CheckCategoryLimit(expense.Category);

        SelectedExpense = null;
        ResetDraft();
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

        ResetDraft();
        RefreshCategories();
    }

    [RelayCommand]
    private void ResetFilter()
    {
        FilterCategory = null;
        DateFrom = null;
        DateTo = null;
        ApplyFilter();
    }

    [RelayCommand]
    private void ApplyFilter()
    {
        var filtered = _expenseRepository.GetAll();

        if (!string.IsNullOrWhiteSpace(FilterCategory))
            filtered = _analytics.FilterByCategory(filtered, FilterCategory);

        if (DateFrom.HasValue || DateTo.HasValue)
        {
            var from = DateFrom ?? DateTime.MinValue;
            var to = DateTo ?? DateTime.MaxValue;
            filtered = _analytics.FilterByDateRange(filtered, from, to);
        }

        ReloadExpenses(filtered);
    }

    [RelayCommand]
    private void ShowStatistics()
    {
        // Строим статистику по тому, что сейчас видит пользователь (с учётом фильтра)
        var snapshot = Expenses.Select(vm => vm.ToModel()).ToList();
        var statsVm = new StatisticsViewModel(snapshot, _analytics);

        var window = new StatisticsWindow(statsVm)
        {
            Owner = Application.Current?.MainWindow
        };
        window.ShowDialog();
    }

    [RelayCommand]
    private void ExportJson()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            DefaultExt = ".json",
            FileName = "expenses.json"
        };

        if (dialog.ShowDialog() != true)
            return;

        _importExport.ExportToJson(_expenseRepository.GetAll(), dialog.FileName);
    }

    [RelayCommand]
    private void ImportJson()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            DefaultExt = ".json"
        };

        if (dialog.ShowDialog() != true)
            return;

        var imported = _importExport.ImportFromJson(dialog.FileName);
        ReplaceAll(imported);
    }

    [RelayCommand]
    private void ExportCsv()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            DefaultExt = ".csv",
            FileName = "expenses.csv"
        };

        if (dialog.ShowDialog() != true)
            return;

        _importExport.ExportToCsv(_expenseRepository.GetAll(), dialog.FileName);
    }

    [RelayCommand]
    private void ImportCsv()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            DefaultExt = ".csv"
        };

        if (dialog.ShowDialog() != true)
            return;

        var imported = _importExport.ImportFromCsv(dialog.FileName);
        ReplaceAll(imported);
    }

    // Полная замена содержимого репозитория импортированными данными
    private void ReplaceAll(IEnumerable<Expense> imported)
    {
        foreach (var e in _expenseRepository.GetAll().ToList())
            _expenseRepository.Delete(e.Id);

        foreach (var e in imported)
            _expenseRepository.Add(e);

        ApplyFilter();
        RefreshCategories();
    }
}
