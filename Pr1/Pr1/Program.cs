int[] scores = new int[10];
for (int i = 0; i < scores.Length; i++)
{
    scores[i] = Random.Shared.Next(0, 101);
}

while (true)
{
    Console.WriteLine("\nСтатистика результатів тестування");
    Console.WriteLine("1. Вихід");
    Console.WriteLine("2. Додати результат");
    Console.WriteLine("3. Показати всі результати");
    Console.WriteLine("4. Показати середній бал");
    Console.WriteLine("5. Показати найвищий і найнижчий бал");
    Console.WriteLine("6. Показати розподіл за категоріями");

    int choice = ReadInt("Оберіть дію: ", 1, 6);

    if (choice == 1)
    {
        Console.WriteLine("Роботу завершено");
        break;
    }

    if (choice == 2)
    {
        int newScore = ReadInt("Введіть бал (0-100): ", 0, 100);
        Array.Resize(ref scores, scores.Length + 1);
        scores[scores.Length - 1] = newScore;
        Console.WriteLine($"Результат {newScore} додано");
        continue;
    }

    if (scores.Length == 0)
    {
        Console.WriteLine("Немає даних");
        continue;
    }

    switch (choice)
    {
        case 3:
            PrintScores(scores);
            break;
        case 4:
            Console.WriteLine($"Середній бал: {CalculateAverage(scores):F2}");
            break;
        case 5:
            int maxIndex = FindMaxIndex(scores);
            int minIndex = FindMinIndex(scores);
            Console.WriteLine($"Найвищий бал: {scores[maxIndex]} (позиція в масиві: {maxIndex})");
            Console.WriteLine($"Найнижчий бал: {scores[minIndex]} (позиція в масиві: {minIndex})");
            break;
        case 6:
            PrintDistribution(scores);
            break;
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

static void PrintScores(int[] scores)
{
    Console.Write($"Усі результати ({scores.Length} шт.): ");
    for (int i = 0; i < scores.Length; i++)
    {
        Console.Write(scores[i]);
        if (i < scores.Length - 1)
        {
            Console.Write(", ");
        }
    }
    Console.WriteLine();
}

static double CalculateAverage(int[] scores)
{
    int sum = 0;
    for (int i = 0; i < scores.Length; i++)
    {
        sum += scores[i];
    }
    return (double)sum / scores.Length;
}

static int FindMaxIndex(int[] scores)
{
    int maxIndex = 0;
    for (int i = 1; i < scores.Length; i++)
    {
        if (scores[i] > scores[maxIndex])
        {
            maxIndex = i;
        }
    }
    return maxIndex;
}

static int FindMinIndex(int[] scores)
{
    int minIndex = 0;
    for (int i = 1; i < scores.Length; i++)
    {
        if (scores[i] < scores[minIndex])
        {
            minIndex = i;
        }
    }
    return minIndex;
}

static string GetCategory(int score) => score switch
{
    >= 90 => "відмінно",
    >= 75 => "добре",
    >= 60 => "задовільно",
    _ => "незадовільно"
};

static void PrintDistribution(int[] scores)
{
    int excellent = 0, good = 0, satisfactory = 0, unsatisfactory = 0;

    for (int i = 0; i < scores.Length; i++)
    {
        switch (GetCategory(scores[i]))
        {
            case "відмінно": excellent++; break;
            case "добре": good++; break;
            case "задовільно": satisfactory++; break;
            default: unsatisfactory++; break;
        }
    }

    Console.WriteLine("Розподіл за категоріями:");
    Console.WriteLine($"Відмінно (90+): {excellent}");
    Console.WriteLine($"Добре (75-89): {good}");
    Console.WriteLine($"Задовільно (60-74): {satisfactory}");
    Console.WriteLine($"Незадовільно (<60): {unsatisfactory}");
}