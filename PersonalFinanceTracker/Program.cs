using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PersonalFinanceTracker.Repositories;
using PersonalFinanceTracker.Services;
using PersonalFinanceTracker.ViewModels;

namespace PersonalFinanceTracker;

public static class Program
{
    [STAThread]
    public static void Main()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IExpenseRepository, ExpenseRepository>();
        services.AddSingleton<ICategoryRepository, CategoryRepository>();
        services.AddSingleton<ExpenseAnalyticsService>();
        services.AddSingleton<ImportExportService>();
        services.AddSingleton<AppStateService>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();

        var provider = services.BuildServiceProvider();

        // загружаем сохраненное состояние из темп-папки
        var appState = provider.GetRequiredService<AppStateService>();
        var expenseRepo = provider.GetRequiredService<IExpenseRepository>();
        var categoryRepo = provider.GetRequiredService<ICategoryRepository>();

        var state = appState.Load();
        foreach (var category in state.Categories)
            categoryRepo.Add(category);
        foreach (var expense in state.Expenses)
            expenseRepo.Add(expense);


        var app = new Application();
        var window = provider.GetRequiredService<MainWindow>();

        //сохранение состояния при закрытии окна
        window.Closing += SaveState;

        

        void SaveState(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            appState.Save(expenseRepo.GetAll(), categoryRepo.GetAll());
        }
        app.Run(window);
    }
}