using CommunityToolkit.Mvvm.ComponentModel;
using PersonalFinanceTracker.Models;
namespace PersonalFinanceTracker.ViewModels;

public partial class ExpenseViewModel : BaseViewModel
{
    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private decimal _amount;

    [ObservableProperty]
    private DateTime _date = DateTime.Today;

    [ObservableProperty]
    private string _description = string.Empty;

    public static ExpenseViewModel FromModel(Expense e) => new()
    {
        Id = e.Id,
        Category = e.Category,
        Amount = e.Amount,
        Date = e.Date,
        Description = e.Description
    };

    public Expense ToModel() => new()
    {
        Id = Id,
        Category = Category,
        Amount = Amount,
        Date = Date,
        Description = Description
    };
}
