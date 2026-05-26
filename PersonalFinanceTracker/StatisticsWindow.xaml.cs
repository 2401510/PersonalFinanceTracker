using System.Windows;
using PersonalFinanceTracker.ViewModels;

namespace PersonalFinanceTracker;

/// <summary>
/// Interaction logic for StatisticsWindow.xaml
/// </summary>
public partial class StatisticsWindow : Window
{
    public StatisticsWindow(StatisticsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
