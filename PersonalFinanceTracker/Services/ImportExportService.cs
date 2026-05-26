using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services;

//экспорт и импорт расходов в JSON и CSV
public class ImportExportService
{
    public void ExportToJson(IEnumerable<Expense> expenses, string filePath)
    {
        var json = JsonConvert.SerializeObject(expenses, Formatting.Indented);
        File.WriteAllText(filePath, json, Encoding.UTF8);
    }

    public IEnumerable<Expense> ImportFromJson(string filePath)
    {
        var json = File.ReadAllText(filePath, Encoding.UTF8);
        return JsonConvert.DeserializeObject<List<Expense>>(json) ?? new List<Expense>();
    }

    public void ExportToCsv(IEnumerable<Expense> expenses, string filePath)
    {
        using var writer = new StreamWriter(filePath, append: false, Encoding.UTF8);
        writer.WriteLine("Id,Category,Amount,Date,Description");

        foreach (var e in expenses)
        {
            writer.Write(e.Id);
            writer.Write(',');
            writer.Write(EscapeCsv(e.Category));
            writer.Write(',');
            writer.Write(e.Amount.ToString(CultureInfo.InvariantCulture));
            writer.Write(',');
            writer.Write(e.Date.ToString("o", CultureInfo.InvariantCulture));
            writer.Write(',');
            writer.WriteLine(EscapeCsv(e.Description));
        }
    }

    public IEnumerable<Expense> ImportFromCsv(string filePath)
    {
        var result = new List<Expense>();
        using var reader = new StreamReader(filePath, Encoding.UTF8);

        var header = reader.ReadLine();
        if (header is null)
            return result;

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var fields = ParseCsvLine(line);
            if (fields.Count < 5)
                continue;

            try
            {
                result.Add(new Expense
                {
                    Id = Guid.TryParse(fields[0], out var id) ? id : Guid.NewGuid(),
                    Category = fields[1],
                    Amount = decimal.Parse(fields[2], CultureInfo.InvariantCulture),
                    Date = DateTime.Parse(fields[3], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    Description = fields[4]
                });
            }
            catch
            {
            }
        }

        return result;
    }

    private static string EscapeCsv(string value)
    {
        value ??= string.Empty;
        if (value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0)
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var sb = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];

            if (inQuotes)
            {
                if (ch == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    sb.Append(ch);
                }
            }
            else
            {
                if (ch == ',')
                {
                    fields.Add(sb.ToString());
                    sb.Clear();
                }
                else if (ch == '"')
                {
                    inQuotes = true;
                }
                else
                {
                    sb.Append(ch);
                }
            }
        }

        fields.Add(sb.ToString());
        return fields;
    }
}
