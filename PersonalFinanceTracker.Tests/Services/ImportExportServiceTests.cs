using System.IO;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.Tests.Services;

public class ImportExportServiceTests : IDisposable
{
    private readonly ImportExportService _service = new();
    private readonly List<string> _tempFiles = new();

    private string GetTempFile(string extension)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + extension);
        _tempFiles.Add(path);
        return path;
    }

    public void Dispose()
    {
        foreach (var f in _tempFiles)
        {
            if (File.Exists(f))
                File.Delete(f);
        }
    }

    private static List<Expense> CreateSample() => new()
    {
        new Expense { Id = Guid.NewGuid(), Category = "Еда", Amount = 123.45m, Date = new DateTime(2026, 1, 5), Description = "обед" },
        new Expense { Id = Guid.NewGuid(), Category = "Транспорт", Amount = 50m, Date = new DateTime(2026, 1, 6), Description = "автобус" },
        new Expense { Id = Guid.NewGuid(), Category = "Развлечения", Amount = 1500.99m, Date = new DateTime(2026, 1, 7), Description = "кино, попкорн и \"кола\"" },
    };

    private static void AssertExpensesEqual(IList<Expense> expected, IList<Expense> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Id, actual[i].Id);
            Assert.Equal(expected[i].Category, actual[i].Category);
            Assert.Equal(expected[i].Amount, actual[i].Amount);
            Assert.Equal(expected[i].Date, actual[i].Date);
            Assert.Equal(expected[i].Description, actual[i].Description);
        }
    }

    [Fact]
    public void ExportToJson_ThenImportFromJson_RoundTripPreservesData()
    {
        var path = GetTempFile(".json");
        var expenses = CreateSample();

        _service.ExportToJson(expenses, path);
        var imported = _service.ImportFromJson(path).ToList();

        Assert.True(File.Exists(path));
        AssertExpensesEqual(expenses, imported);
    }

    [Fact]
    public void ExportToCsv_ThenImportFromCsv_RoundTripPreservesData()
    {
        var path = GetTempFile(".csv");
        var expenses = CreateSample();

        _service.ExportToCsv(expenses, path);
        var imported = _service.ImportFromCsv(path).ToList();

        Assert.True(File.Exists(path));
        AssertExpensesEqual(expenses, imported);
    }

    [Fact]
    public void ExportToCsv_FirstLine_IsHeader()
    {
        var path = GetTempFile(".csv");

        _service.ExportToCsv(CreateSample(), path);

        var firstLine = File.ReadAllLines(path).First();
        Assert.Equal("Id,Category,Amount,Date,Description", firstLine);
    }

    [Fact]
    public void ImportFromJson_EmptyArray_ReturnsEmptyCollection()
    {
        var path = GetTempFile(".json");
        File.WriteAllText(path, "[]");

        var imported = _service.ImportFromJson(path);

        Assert.Empty(imported);
    }

    [Fact]
    public void ImportFromCsv_OnlyHeader_ReturnsEmptyCollection()
    {
        var path = GetTempFile(".csv");
        File.WriteAllText(path, "Id,Category,Amount,Date,Description\n");

        var imported = _service.ImportFromCsv(path);

        Assert.Empty(imported);
    }
}
