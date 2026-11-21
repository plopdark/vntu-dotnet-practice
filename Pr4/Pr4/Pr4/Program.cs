using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

record Offer(string Shop, string Product, string Category, decimal Price, bool InStock);

record WishItem(string Product, decimal MaxPrice);

class Program
{
    static List<Offer> _offers = new();

    static readonly List<WishItem> _wishes = new()
    {
        new("Ноутбук Lenovo IdeaPad", 23500m),
        new("Смартфон Samsung Galaxy A55", 18500m),
        new("Навушники Sony WH-1000XM5", 13000m),
        new("Павербанк Anker 20000", 1450m),
        new("Клавіатура Keychron K2", 3500m)
    };

    static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        bool isRunning = true;

        while (isRunning)
        {
            Console.WriteLine("\nПорівняння цін");
            Console.WriteLine("1. Вихід");
            Console.WriteLine("2. Завантажити пропозиції (одночасно)");
            Console.WriteLine("3. Завантажити пропозиції (послідовно)");
            Console.WriteLine("4. Найдешевша пропозиція кожного товару");
            Console.WriteLine("5. Статистика за категоріями");
            Console.WriteLine("6. Пошук у ціновому діапазоні");
            Console.WriteLine("7. Список бажань");
            Console.WriteLine("8. Зберегти звіт у JSON-файл");

            int choice = ReadInt("Оберіть дію: ", 1, 8);

            isRunning = choice switch
            {
                1 => false,
                2 => await HandleLoadAsync(parallel: true),
                3 => await HandleLoadAsync(parallel: false),
                4 => HandleCheapest(),
                5 => HandleCategoryStats(),
                6 => HandlePriceRange(),
                7 => HandleWishList(),
                8 => await HandleSaveReportAsync(),
                _ => true
            };
        }

        Console.WriteLine("Роботу завершено");
    }
    
    static async Task<List<Offer>> LoadTechnoMarketAsync()
    {
        await Task.Delay(2000);

        return new List<Offer>
        {
            new("ТехноМаркет", "Ноутбук Lenovo IdeaPad", "Ноутбуки", 23999m, true),
            new("ТехноМаркет", "Ноутбук ASUS Vivobook", "Ноутбуки", 26499m, true),
            new("ТехноМаркет", "Смартфон Samsung Galaxy A55", "Смартфони", 18999m, true),
            new("ТехноМаркет", "Смартфон Apple iPhone 15", "Смартфони", 38999m, true),
            new("ТехноМаркет", "Навушники Sony WH-1000XM5", "Аксесуари", 13499m, false),
            new("ТехноМаркет", "Миша Logitech MX Master 3S", "Аксесуари", 3799m, true),
            new("ТехноМаркет", "Павербанк Anker 20000", "Аксесуари", 1499m, true),
            new("ТехноМаркет", "Клавіатура Keychron K2", "Аксесуари", 3299m, false)
        };
    }

    static async Task<List<Offer>> LoadDigitalPlusAsync()
    {
        await Task.Delay(1500);

        return new List<Offer>
        {
            new("ЦифраПлюс", "Ноутбук Lenovo IdeaPad", "Ноутбуки", 22999m, true),
            new("ЦифраПлюс", "Ноутбук HP Pavilion", "Ноутбуки", 27999m, true),
            new("ЦифраПлюс", "Смартфон Samsung Galaxy A55", "Смартфони", 18499m, true),
            new("ЦифраПлюс", "Смартфон Xiaomi Redmi Note 13", "Смартфони", 9999m, true),
            new("ЦифраПлюс", "Смартфон Apple iPhone 15", "Смартфони", 39499m, false),
            new("ЦифраПлюс", "Навушники Sony WH-1000XM5", "Аксесуари", 12999m, true),
            new("ЦифраПлюс", "Миша Logitech MX Master 3S", "Аксесуари", 3999m, true),
            new("ЦифраПлюс", "Павербанк Anker 20000", "Аксесуари", 1399m, true)
        };
    }

    static async Task<List<Offer>> LoadElectroWorldAsync()
    {
        await Task.Delay(1000);

        return new List<Offer>
        {
            new("Електросвіт", "Ноутбук ASUS Vivobook", "Ноутбуки", 25999m, true),
            new("Електросвіт", "Ноутбук HP Pavilion", "Ноутбуки", 28499m, false),
            new("Електросвіт", "Ноутбук Lenovo IdeaPad", "Ноутбуки", 24499m, true),
            new("Електросвіт", "Смартфон Xiaomi Redmi Note 13", "Смартфони", 10499m, true),
            new("Електросвіт", "Смартфон Samsung Galaxy A55", "Смартфони", 19299m, false),
            new("Електросвіт", "Навушники Sony WH-1000XM5", "Аксесуари", 13299m, true),
            new("Електросвіт", "Павербанк Anker 20000", "Аксесуари", 1599m, true),
            new("Електросвіт", "Клавіатура Keychron K2", "Аксесуари", 3199m, false)
        };
    }
    
    static async Task<bool> HandleLoadAsync(bool parallel)
    {
        Console.WriteLine(parallel ? "Одночасне завантаження..." : "Послідовне завантаження...");

        var sw = Stopwatch.StartNew();
        List<Offer>[] parts;

        if (parallel)
        {
            parts = await Task.WhenAll(LoadTechnoMarketAsync(), LoadDigitalPlusAsync(), LoadElectroWorldAsync());
        }
        else
        {
            parts = new[]
            {
                await LoadTechnoMarketAsync(),
                await LoadDigitalPlusAsync(),
                await LoadElectroWorldAsync()
            };
        }

        sw.Stop();

        _offers = parts.SelectMany(p => p).ToList();

        Console.WriteLine($"Завантажено пропозицій: {_offers.Count}. Час: {sw.ElapsedMilliseconds} мс");
        return true;
    }

    static bool HandleCheapest()
    {
        if (!HasData())
        {
            return true;
        }

        var cheapest = _offers
            .GroupBy(o => o.Product)
            .Select(g => new
            {
                Product = g.Key,
                Category = g.First().Category,
                Available = g.Any(o => o.InStock),
                Best = g.Where(o => o.InStock).MinBy(o => o.Price) 
            })
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Product);

        Console.WriteLine("\nНайдешевші пропозиції:");

        foreach (var item in cheapest)
        {
            if (item.Available && item.Best is not null)
            {
                Console.WriteLine($"- {item.Product} ({item.Category}): {item.Best.Price:0.##} грн, {item.Best.Shop}");
            }
            else
            {
                Console.WriteLine($"- {item.Product} ({item.Category}): немає в наявності");
            }
        }

        return true;
    }

    static bool HandleCategoryStats()
    {
        if (!HasData())
        {
            return true;
        }

        var stats = _offers
            .GroupBy(o => o.Category)
            .Select(g => new
            {
                Category = g.Key,
                Count = g.Count(),
                Min = g.Min(o => o.Price),
                Avg = g.Average(o => o.Price),
                Max = g.Max(o => o.Price)
            })
            .OrderBy(s => s.Category);

        Console.WriteLine("\nСтатистика за категоріями (усі пропозиції):");

        foreach (var s in stats)
        {
            Console.WriteLine($"{s.Category}: {s.Count} шт., мін. {s.Min:0.##} грн, сер. {s.Avg:0.##} грн, макс. {s.Max:0.##} грн");
        }

        return true;
    }

    static bool HandlePriceRange()
    {
        if (!HasData())
        {
            return true;
        }

        decimal min = ReadDecimal("Мінімальна ціна, грн: ");
        decimal max = ReadDecimal("Максимальна ціна, грн: ");

        if (min > max)
        {
            Console.WriteLine("Помилка: мінімальна ціна більша за максимальну");
            return true;
        }

        var found = _offers
            .Where(o => o.InStock && o.Price >= min && o.Price <= max)
            .OrderBy(o => o.Price)
            .ThenBy(o => o.Shop)
            .Select(o => new { o.Product, o.Shop, o.Price })
            .ToList();

        if (found.Count == 0)
        {
            Console.WriteLine("Пропозицій не знайдено");
            return true;
        }

        foreach (var f in found)
        {
            Console.WriteLine($"{f.Price:0.##} грн - {f.Product} ({f.Shop})");
        }

        return true;
    }

    static bool HandleWishList()
    {
        if (!HasData())
        {
            return true;
        }

        var matches =
            (from w in _wishes
             join o in _offers on w.Product equals o.Product
             where o.InStock && o.Price <= w.MaxPrice
             orderby w.Product, o.Price
             select new { w.Product, w.MaxPrice, o.Shop, o.Price })
            .ToList();

        Console.WriteLine("\nСписок бажань:");

        foreach (WishItem wish in _wishes)
        {
            Console.WriteLine($"{wish.Product} (до {wish.MaxPrice:0.##} грн):");

            var forWish = matches.Where(m => m.Product == wish.Product).ToList();

            if (!forWish.Any())
            {
                Console.WriteLine("  підходящих пропозицій немає");
                continue;
            }

            foreach (var m in forWish)
            {
                Console.WriteLine($"  {m.Price:0.##} грн - {m.Shop}");
            }
        }

        return true;
    }

    static async Task<bool> HandleSaveReportAsync()
    {
        if (!HasData())
        {
            return true;
        }

        var report = _offers
            .GroupBy(o => o.Product)
            .Select(g =>
            {
                Offer? best = g.Where(o => o.InStock).MinBy(o => o.Price);

                return new
                {
                    Product = g.Key,
                    Category = g.First().Category,
                    Status = best is null ? "немає в наявності" : "в наявності",
                    Shop = best?.Shop,
                    Price = best?.Price
                };
            })
            .OrderBy(r => r.Category)
            .ThenBy(r => r.Product)
            .ToList();

        string json = JsonSerializer.Serialize(report, _jsonOptions);

        try
        {
            await File.WriteAllTextAsync("report.json", json);
            Console.WriteLine("Звіт збережено у файл report.json");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Не вдалося зберегти звіт: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Немає доступу до файлу: {ex.Message}");
        }

        return true;
    }
    
    static bool HasData()
    {
        if (_offers.Count > 0)
        {
            return true;
        }

        Console.WriteLine("Немає даних. Спочатку завантажте їх (пункт 2 або 3)");
        return false;
    }

    static decimal ReadDecimal(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);

            if (decimal.TryParse(Console.ReadLine(), out decimal value) && value >= 0)
            {
                return value;
            }

            Console.WriteLine("Некоректне значення. Введіть невід'ємне число");
        }
    }

    static int ReadInt(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);

            if (int.TryParse(Console.ReadLine(), out int value) && value >= min && value <= max)
            {
                return value;
            }

            Console.WriteLine($"Помилка: введіть ціле число від {min} до {max}");
        }
    }
}