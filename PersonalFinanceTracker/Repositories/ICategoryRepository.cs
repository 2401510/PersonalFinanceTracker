using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Repositories;

//репозиторий категорий
public interface ICategoryRepository
{
    IEnumerable<Category> GetAll();

    void Add(Category category);

    void Update(Category category);

    void Delete(Guid id);
    
}
