using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Repositories;

//реализация репозитория категорий в памяти
public class CategoryRepository : ICategoryRepository
{
    private readonly List<Category> _categories = new();

    public IEnumerable<Category> GetAll() => _categories;

    public void Add(Category category)
    {
        //если id не задан, генерируем новый
        if (category.Id == Guid.Empty)
            category.Id = Guid.NewGuid();

        _categories.Add(category);
    }

    public void Update(Category category)
    {
        var index = _categories.FindIndex(c => c.Id == category.Id);
        if (index >= 0)
            _categories[index] = category;
    }

    public void Delete(Guid id)
    {
        _categories.RemoveAll(c => c.Id == id);
    }
}
