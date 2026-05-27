using System.Windows;
using PersonalFinanceTracker.Repositories;
using PersonalFinanceTracker.Services;
using PersonalFinanceTracker.ViewModels;

namespace PersonalFinanceTracker;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ExpenseRepository _expenseRepository;
    private readonly CategoryRepository _categoryRepository;
    private readonly AppStateService _appState;

    public MainWindow()
    {
        InitializeComponent();

        _expenseRepository = new ExpenseRepository();
        _categoryRepository = new CategoryRepository();
        var analytics = new ExpenseAnalyticsService();
        var importExport = new ImportExportService();
        _appState = new AppStateService();

        // загружаем сохраненное состояние из темп-папки
        var state = _appState.Load();
        foreach (var category in state.Categories)
            _categoryRepository.Add(category);
        foreach (var expense in state.Expenses)
            _expenseRepository.Add(expense);

        DataContext = new MainViewModel(_expenseRepository, _categoryRepository, analytics, importExport);

        Closing += OnClosing;
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _appState.Save(_expenseRepository.GetAll(), _categoryRepository.GetAll());
    }
}
