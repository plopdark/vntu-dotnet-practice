interface IPriced
{
    string Name { get; }
    decimal Price { get; }
}

class Product : IPriced
{
    private string _name = "";
    private decimal _price;

    public string Name
    {
        get => _name;
        set => _name = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Назва товару не може бути порожньою")
            : value;
    }

    public decimal Price
    {
        get => _price;
        set => _price = value > 0
            ? value
            : throw new ArgumentException("Ціна має бути додатною");
    }

    public Product(string name, decimal price)
    {
        Name = name;
        Price = price;
    }
}

class TotalChangedEventArgs : EventArgs
{
    public decimal OldTotal { get; }
    public decimal NewTotal { get; }

    public TotalChangedEventArgs(decimal oldTotal, decimal newTotal)
    {
        OldTotal = oldTotal;
        NewTotal = newTotal;
    }
}

class Cart<T> where T : IPriced
{
    private readonly List<T> _items = new();

    private Func<decimal, decimal>? _discount; 

    public event EventHandler<TotalChangedEventArgs>? TotalChanged;

    public int Count => _items.Count;

    public void Add(T item)
    {
        decimal oldTotal = GetTotal();
        _items.Add(item);
        OnTotalChanged(new TotalChangedEventArgs(oldTotal, GetTotal()));
    }

    public bool Remove(string name)
    {
        int index = _items.FindIndex(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (index < 0)
        {
            return false;
        }

        decimal oldTotal = GetTotal();
        _items.RemoveAt(index);
        OnTotalChanged(new TotalChangedEventArgs(oldTotal, GetTotal()));
        return true;
    }

    public decimal GetTotal()
    {
        decimal sum = 0;

        foreach (T item in _items)
        {
            sum += item.Price;
        }

        return _discount?.Invoke(sum) ?? sum;
    }

    public void ForEach(Action<T> action)
    {
        foreach (T item in _items)
        {
            action(item);
        }
    }

    public void SetDiscount(Func<decimal, decimal>? discount)
    {
        decimal oldTotal = GetTotal();
        _discount = discount;
        OnTotalChanged(new TotalChangedEventArgs(oldTotal, GetTotal()));
    }

    protected virtual void OnTotalChanged(TotalChangedEventArgs e) => TotalChanged?.Invoke(this, e);
}

class Program
{
    private const decimal FreeDeliveryFrom = 1000m;

    private static string? _activePromo; 

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var cart = new Cart<Product>();
        cart.Add(new Product("Навушники", 800));
        cart.Add(new Product("Чохол", 300));

        var promoCodes = new Dictionary<string, Func<decimal, decimal>>(StringComparer.OrdinalIgnoreCase)
        {
            ["SALE10"] = total => total * 0.9m,
            ["MINUS100"] = total => Math.Max(total - 100, 0),
            ["BIG20"] = total => total >= 2000 ? total * 0.8m : total
        };

        cart.TotalChanged += ShowTotal;

        decimal freeDeliveryFrom = FreeDeliveryFrom;
        cart.TotalChanged += (sender, e) =>
        {
            decimal left = freeDeliveryFrom - e.NewTotal;

            Console.WriteLine(left <= 0
                ? "Доставка безкоштовна!"
                : $"До безкоштовної доставки: {left:0.##} грн");
        };

        bool isRunning = true;

        while (isRunning)
        {
            Console.WriteLine("\nКошик інтернет-магазину");
            Console.WriteLine("1. Вихід");
            Console.WriteLine("2. Додати товар");
            Console.WriteLine("3. Прибрати товар");
            Console.WriteLine("4. Показати кошик");
            Console.WriteLine("5. Ввести промокод");
            Console.WriteLine("6. Скасувати промокод");

            int choice = ReadInt("Оберіть дію: ", 1, 6);

            isRunning = choice switch
            {
                1 => false,
                2 => HandleAdd(cart),
                3 => HandleRemove(cart),
                4 => HandleShow(cart),
                5 => HandleApplyPromo(cart, promoCodes),
                6 => HandleCancelPromo(cart),
                _ => true
            };
        }

        Console.WriteLine("Роботу завершено");
    }
    
    static void ShowTotal(object? sender, TotalChangedEventArgs e)
    {
        Console.WriteLine($"Сума: {e.NewTotal:0.##} грн (було: {e.OldTotal:0.##} грн)");
    }
    
    static bool HandleAdd(Cart<Product> cart)
    {
        string name = ReadNonEmpty("Назва товару: ");
        decimal price = ReadDecimal("Ціна, грн: ");

        cart.Add(new Product(name, price));
        return true;
    }

    static bool HandleRemove(Cart<Product> cart)
    {
        if (cart.Count == 0)
        {
            Console.WriteLine("Немає даних.");
            return true;
        }

        string name = ReadNonEmpty("Назва товару для видалення: ");

        if (!cart.Remove(name))
        {
            Console.WriteLine("Такого товару в кошику немає");
        }

        return true;
    }

    static bool HandleShow(Cart<Product> cart)
    {
        if (cart.Count == 0)
        {
            Console.WriteLine("Немає даних");
            return true;
        }

        Console.WriteLine("\nТовари в кошику:");

        cart.ForEach(p => Console.WriteLine($"- {p.Name}: {p.Price:0.##} грн"));

        Console.WriteLine($"Промокод: {_activePromo ?? "не задано"}");
        Console.WriteLine($"Сума до сплати: {cart.GetTotal():0.##} грн");
        return true;
    }

    static bool HandleApplyPromo(Cart<Product> cart, Dictionary<string, Func<decimal, decimal>> promoCodes)
    {
        string code = ReadNonEmpty("Промокод: ");

        if (promoCodes.TryGetValue(code, out Func<decimal, decimal>? discount))
        {
            _activePromo = code.ToUpper();
            cart.SetDiscount(discount);
        }
        else
        {
            Console.WriteLine("Невідомий промокод");
        }

        return true;
    }

    static bool HandleCancelPromo(Cart<Product> cart)
    {
        if (_activePromo is null)
        {
            Console.WriteLine("Промокод не задано");
            return true;
        }

        _activePromo = null;
        cart.SetDiscount(null); 
        return true;
    }
    
    static string ReadNonEmpty(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(input))
            {
                return input.Trim();
            }

            Console.WriteLine("Значення не може бути порожнім");
        }
    }

    static decimal ReadDecimal(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);

            if (decimal.TryParse(Console.ReadLine(), out decimal value) && value > 0)
            {
                return value;
            }

            Console.WriteLine("Некоректне значення. Введіть додатне число");
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