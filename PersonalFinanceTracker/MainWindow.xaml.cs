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
        var analytics = new ExpenseAnalyticsService();

        DataContext = new MainViewModel(expenseRepository, analytics);
    }
}
