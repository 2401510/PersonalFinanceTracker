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
    public MainWindow()
    {
        InitializeComponent();
        var expenseRepository = new ExpenseRepository();
        var categoryRepository = new CategoryRepository();
        var analytics = new ExpenseAnalyticsService();
        var importExport = new ImportExportService();

        DataContext = new MainViewModel(expenseRepository, categoryRepository, analytics, importExport);
    }
}
