namespace PersonalFinanceTracker.Models;

//модель расхода
public class Expense
{
    //уникальный идентификатор
    public Guid Id { get; set; }

    //категория расхода
    public string Category { get; set; } = string.Empty;

    //сумма расхода
    public decimal Amount { get; set; }

    //дата операции
    public DateTime Date { get; set; }

    //описание расхода
    public string Description { get; set; } = string.Empty;
}
