using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using PersonStructure;

public class Program
{

    static async Task Main()
    {
        List<Person> persons = new List<Person>();
        List<InfoPersonXML> infoPersonsXML = new List<InfoPersonXML>();

        /**
            Блок ввода данных сотрудников
        **/

        string confirmationLoadPersons = "";
        
        // Проверяем наличие файла Persons.json (в них информация о сотрудниках, которую вводили раньше)
        if (File.Exists("Persons.json"))
        {
            while (confirmationLoadPersons != "да" && confirmationLoadPersons != "нет")
            {
                Console.WriteLine("Обнаружено сохранение сотрудников. Хотите загрузить их?");
                confirmationLoadPersons = Console.ReadLine().ToLower();
            }
        }
        else
        {
            confirmationLoadPersons = "нет";
        }

        if (confirmationLoadPersons == "нет")
        {
            // Запускаем цикл столько раз, сколько сотрудников
            for (int i = 0; i < Person.GetCountPersons(); i--)
            {
                Console.WriteLine("\n-----------------------------------------");
                Console.WriteLine("Введите информацию о сотруднике: ");
                Console.WriteLine("-----------------------------------------");

                string fullname = "";
                while (!IsFullnameValid(fullname))
                {
                    Console.Write("Введите имя и фамилию сотрудника: ");
                    fullname = Console.ReadLine();
                }

                // Создаём объект сотрудника
                Person person = new Person(fullname);

                // Создаём объект должности сотрудника
                Position position = null;
                string positionName = null; 
                while (position == null)
                {
                    // Если уже добавляли сотрудников - показываем существующие должности
                    if (persons.Count > 0)
                    {
                        List<Position> namePositions = Position.GetPositions();
                        Console.WriteLine($"Существующие должности:");
                        foreach (Position namePosition in namePositions)
                        {
                            Console.WriteLine($"{namePosition.GetTitle()}");
                        }
                    }
                    Console.Write("Введите должность сотрудника: ");
                    positionName = Console.ReadLine();

                    // Создание новой должности
                    if (Position.IsPositionExist(positionName))
                    {
                        position = Position.GetPositionByTitle(positionName);
                    }
                    else
                    {
                        Console.Write("Вы уверены, что хотите создать новую должность? Введите \"Да\", чтобы создать: ");
                        if (Console.ReadLine().ToLower() == "да")
                        {
                            position = Position.CreatePosition(positionName);
                        }
                    }

                    // Ограничение кол-ва сотрудников по должностям
                    if (position?.GetPersons().Count >= 3)
                    {
                        Console.WriteLine("На одной должности не может быть больше трёх сотрудников.");
                    }

                }

                double salary = -1D;
                while (salary < 0D)
                {
                    Console.Write("Введите зарплату сотрудника: ");
                    if (double.TryParse(Console.ReadLine().Replace(".", ","), out double inputSalary))
                    {
                        if (inputSalary >= 0)
                        {
                            salary = inputSalary;
                        }
                        else
                        {
                            Console.WriteLine("Сотрудник не может работать в минус. Даже у нас.");
                        }
                    }
                }
                Console.WriteLine("-----------------------------------------");

                // Заносим информацию о сотруднике в объект для занесения данных в JSON
                string[] parts = fullname.Split(' ');
                InfoPersonXML infoPersonXML = new InfoPersonXML(parts[0], parts[1], positionName, salary);

                Console.WriteLine($"Сохранить информацию о сотруднике?");
                Console.WriteLine("-----------------------------------------");

                // Выводим информацию, которую ввели о сотруднике
                PrintPerson(person.GetFirstname(), person.GetLastname(), position, salary);
                Console.WriteLine("-----------------------------------------");

                string confirmationSavePerson = "";
                while (confirmationSavePerson != "да" && confirmationSavePerson != "нет")
                {
                    Console.Write($"Введите \"Да\", чтобы сохранить, или \"Нет\", чтобы удалить сотрудника: ");
                    confirmationSavePerson = Console.ReadLine().ToLower();
                }

                if (confirmationSavePerson == "да")
                {
                    // Заполняем объект сотрудника
                    person.SetPosition(position);
                    person.SetSalary(salary);
                    persons.Add(person);
                    infoPersonsXML.Add(infoPersonXML);
                }
                else
                {
                    position.RemovePositionIfNoPersons();
                }
            }
            // Если JSON файл создан - пересоздаём
            if (File.Exists("Persons.json")) { File.Delete("Persons.json"); }

            // Заносим информацию о сотруднике в JSON файл
            using (FileStream fs = new FileStream("Persons.json", FileMode.OpenOrCreate))
            {
                var options = new JsonSerializerOptions()
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                    WriteIndented = true
                };
                await JsonSerializer.SerializeAsync<List<InfoPersonXML>>(fs, infoPersonsXML, options);
            }
        }
        else
        {
            // Если пользователю нужно показать информацию из файла Persons.json - сначала заполняем объект Person и Position
            using (FileStream fs = new FileStream("Persons.json", FileMode.OpenOrCreate))
            {
                List<InfoPersonXML> infoPersons = await JsonSerializer.DeserializeAsync<List<InfoPersonXML>>(fs);
                foreach (InfoPersonXML infoPerson in infoPersons)
                {
                    Person person = new Person($"{infoPerson.Firstname} {infoPerson.Lastname}");
                    string positionName = infoPerson.Title;
                    double salary = infoPerson.Salary;

                    Position position = null;
                    if (Position.IsPositionExist(positionName))
                    {
                        position = Position.GetPositionByTitle(positionName);
                    }
                    else
                    {
                        position = Position.CreatePosition(positionName);
                    }
                    person.SetPosition(position);
                    person.SetSalary(salary);
                    persons.Add(person);
                }
            }
        }

        // Сортируем по имени и фамилии (А-Я)
        List<Person> sortedPersons = persons.OrderBy(person => $"{person.GetFirstname()} {person.GetLastname()}").ToList();

        double totalSalary = 0.0D;
        Console.WriteLine("-----------------------------------------");
        Console.WriteLine($"Список сотрудников:");

        // Показываем всех сохранённых сотрудников
        foreach (Person person in sortedPersons)
        {
            Console.WriteLine($"--------------------{person.GetFirstname()} {person.GetLastname()}---------------------");
            PrintPerson(person.GetFirstname(), person.GetLastname(), person.GetPosition(), person.GetSalary());
            totalSalary += person.GetSalary();
        }

        // Показываем сколько сотрудников находятся на должностях
        List<Position> positions = Position.GetPositions();
        foreach (Position position in positions)
        {
            if (position.GetPersons().Count == 0)
            {
                continue;
            }

            double positionSalary = 0.0D;
            foreach (Person person in position.GetPersons())
            {
                positionSalary += person.GetSalary();
            }

            Console.WriteLine("\n-----------------------------------------");
            Console.WriteLine($"{position.GetTitle()}:");
            Console.WriteLine($"\tКоличество сотрудников: {position.GetPersons().Count}");
            Console.WriteLine($"\tСредняя зарплата: " + (positionSalary / position.GetPersons().Count).ToString("C", new CultureInfo("en-US")));
        }
        Console.WriteLine($"Общая сумма зарплат: {totalSalary.ToString("C", new CultureInfo("en-US"))}");
        Console.ReadLine();
    }

    public static bool IsFullnameValid(string fullname)
    {
        return ((!string.IsNullOrEmpty(fullname)) && (fullname.Split(' ').Length == 2));
    }

    public static void PrintPerson(string firstname, string lastname, Position position, double salary)
    {
        Console.WriteLine($"Имя: {firstname}");
        Console.WriteLine($"Фамилия: {lastname}");
        Console.WriteLine($"Должность: {position.GetTitle()}");
        Console.WriteLine($"Зарплата: " + salary.ToString("C", new CultureInfo("en-US")));
    }
}