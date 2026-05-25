namespace PersonalFinanceTracker.Models;

//модель категории расходов
public class Category
{
    //уникальный идентификатор
    public Guid Id { get; set; }

    //название категории
    public string Name { get; set; } = string.Empty;

    //лимит расходов, может быть null
    public decimal? LimitAmount { get; set; }
}
