class InvalidShapeSizeException : Exception
{
    public InvalidShapeSizeException() { }

    public InvalidShapeSizeException(string message) : base(message) { }

    public InvalidShapeSizeException(string message, Exception inner) : base(message, inner) { }
}

abstract class Shape : IComparable<Shape>
{
    private string _name = "";

    public string Name
    {
        get => _name;
        set => _name = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Назва фігури не може бути порожньою")
            : value;
    }

    protected Shape(string name)
    {
        Name = name;
    }

    public abstract double GetArea();

    public abstract double GetPerimeter();

    public virtual void PrintInfo()
    {
        Console.WriteLine($"{Name}: площа = {GetArea():F2}, периметр = {GetPerimeter():F2}");
    }

    public int CompareTo(Shape? other)
    {
        if (other is null)
        {
            return 1;
        }

        return GetArea().CompareTo(other.GetArea());
    }

    protected static double ValidateSize(double value, string paramName)
    {
        if (value <= 0 || !double.IsFinite(value))
        {
            throw new InvalidShapeSizeException($"{paramName} має бути скінченним додатним числом (отримано {value})");
        }

        return value;
    }
}

class Circle : Shape
{
    private double _radius;

    public double Radius
    {
        get => _radius;
        set => _radius = ValidateSize(value, "Радіус");
    }

    public Circle(double radius) : base("Коло")
    {
        Radius = radius;
    }

    public Circle() : this(1)
    {
    }

    public override double GetArea() => Math.PI * Radius * Radius;

    public override double GetPerimeter() => 2 * Math.PI * Radius;

    public override void PrintInfo()
    {
        base.PrintInfo();
        Console.WriteLine($"Радіус: {Radius}");
    }
}

class Rectangle : Shape
{
    private double _width;
    private double _height;

    public double Width
    {
        get => _width;
        set => _width = ValidateSize(value, "Ширина");
    }

    public double Height
    {
        get => _height;
        set => _height = ValidateSize(value, "Висота");
    }

    public Rectangle(double width, double height) : base("Прямокутник")
    {
        Width = width;
        Height = height;
    }
    
    public Rectangle(double side) : this(side, side)
    {
    }

    public override double GetArea() => Width * Height;

    public override double GetPerimeter() => 2 * (Width + Height);

    public override void PrintInfo()
    {
        base.PrintInfo();
        Console.WriteLine($"  Ширина: {Width}, висота: {Height}");
    }
}

class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        List<Shape> shapes = new()
        {
            new Circle(5),
            new Rectangle(4, 6),
            new Circle(2.5),
            new Rectangle(3),
            new Circle()
        };

        bool isRunning = true;

        while (isRunning)
        {
            Console.WriteLine("\nГеометричні фігури");
            Console.WriteLine("1. Вихід");
            Console.WriteLine("2. Додати фігуру");
            Console.WriteLine("3. Показати всі фігури");
            Console.WriteLine("4. Відсортувати фігури за площею");
            Console.WriteLine("5. Показати сумарну площу та периметр");

            int choice = ReadInt("Оберіть дію: ", 1, 5);

            isRunning = choice switch
            {
                1 => false,
                2 => HandleAddShape(shapes),
                3 => HandleShowAll(shapes),
                4 => HandleSort(shapes),
                5 => HandleTotals(shapes),
                _ => true
            };
        }

        Console.WriteLine("Роботу завершено");
    }
    
    static bool HandleAddShape(List<Shape> shapes)
    {
        Console.WriteLine("Тип фігури: 1 - коло, 2 - прямокутник");
        int type = ReadInt("Ваш вибір: ", 1, 2);

        try
        {
            if (type == 1)
            {
                double radius = ReadDouble("Радіус: ");
                shapes.Add(new Circle(radius));
            }
            else
            {
                double width = ReadDouble("Ширина: ");
                double height = ReadDouble("Висота: ");
                shapes.Add(new Rectangle(width, height));
            }

            Console.WriteLine("Фігуру додано");
        }
        catch (InvalidShapeSizeException ex)
        {
            Console.WriteLine($"Помилка розміру: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Помилка аргументу: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Непередбачена помилка: {ex.Message}");
        }

        return true;
    }

    static bool HandleShowAll(List<Shape> shapes)
    {
        if (shapes.Count == 0)
        {
            Console.WriteLine("Немає даних");
            return true;
        }

        Console.WriteLine($"\nУсього фігур: {shapes.Count}");

        foreach (Shape shape in shapes)
        {
            shape.PrintInfo();
        }

        return true;
    }

    static bool HandleSort(List<Shape> shapes)
    {
        if (shapes.Count == 0)
        {
            Console.WriteLine("Немає даних");
            return true;
        }

        shapes.Sort(); 
        Console.WriteLine("\nФігури відсортовано за площею (за зростанням):");

        foreach (Shape shape in shapes)
        {
            shape.PrintInfo();
        }

        return true;
    }

    static bool HandleTotals(List<Shape> shapes)
    {
        if (shapes.Count == 0)
        {
            Console.WriteLine("Немає даних");
            return true;
        }

        double totalArea = 0;
        double totalPerimeter = 0;

        foreach (Shape shape in shapes)
        {
            totalArea += shape.GetArea();
            totalPerimeter += shape.GetPerimeter();
        }

        Console.WriteLine($"\nСумарна площа: {totalArea:F2}");
        Console.WriteLine($"Сумарний периметр: {totalPerimeter:F2}");

        return true;
    }
    
    static double ReadDouble(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);

            if (double.TryParse(Console.ReadLine(), out double value))
            {
                return value;
            }

            Console.WriteLine("Некоректне значення. Введіть число");
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