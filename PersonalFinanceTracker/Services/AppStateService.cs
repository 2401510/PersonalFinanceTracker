using System.IO;
using System.Text;
using Newtonsoft.Json;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services;

//сохранение и загрузка состояния приложения в JSON в темп-папке
public class AppStateService
{
    private readonly string _filePath;

    public AppStateService()
    {
        var dir = Path.Combine(Path.GetTempPath(), "PersonalFinanceTracker");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "state.json");
    }

    public string FilePath => _filePath;

    public AppState Load()
    {
        if (!File.Exists(_filePath))
            return new AppState();

        try
        {
            var json = File.ReadAllText(_filePath, Encoding.UTF8);
            return JsonConvert.DeserializeObject<AppState>(json) ?? new AppState();
        }
        catch
        {
            // если файл сломан то игнорируем и запускем с пустым состоянием
            return new AppState();
        }
    }

    public void Save(IEnumerable<Expense> expenses, IEnumerable<Category> categories)
    {
        var state = new AppState
        {
            Expenses = expenses.ToList(),
            Categories = categories.ToList()
        };

        var json = JsonConvert.SerializeObject(state, Formatting.Indented);
        File.WriteAllText(_filePath, json, Encoding.UTF8);
    }
}

public class AppState
{
    public List<Expense> Expenses { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
}
