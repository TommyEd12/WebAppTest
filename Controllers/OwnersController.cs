using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebAppTest.ViewModels;

namespace WebAppTest.Controllers
{
    public class OwnersController : Controller
    {
        private readonly CarsContext _context;

        public OwnersController(CarsContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var owners = _context.Owners.ToList();
            return View(new OwnersViewModel()
            {
                Owners = owners,
            });
        }

        [HttpPost]
        public IActionResult Edit(int? id, string? newNumber, string? secondName, string? name, string? surname, string? login)
        {
            string? error = null;
            if (id == null)
                error = "Не указан id";
            else
            {
                var owner = _context.Owners.FirstOrDefault(e => e.Id == id);
                if (owner == null)
                    error = "Выбранного элемента не существует";
                else
                {
                    if (newNumber != null) owner.Number = newNumber;
                    if (secondName != null) owner.SecondName = secondName;
                    if (name != null) owner.Name = name;
                    if (surname != null) owner.Surname = surname;
                    if (login != null) owner.Login = login;

                    try
                    {
                        _context.SaveChanges();
                    }
                    catch (DbUpdateException ex)
                    {
                        if (ex.InnerException is PostgresException e)
                        {
                            error = e.SqlState switch
                            {
                                "23503" => "Нельзя добавить запись с номером машины, отсутствующем в таблице машин",
                                "23514" or "22001" => "Неправильный формат номера машины",
                                _ => $"Ошибка базы данных: {e.MessageText}"
                            };
                        }
                        else
                        {
                            error = "Произошла непредвиденная ошибка";
                        }
                        Console.WriteLine(ex);
                    }
                    catch (Exception ex)
                    {
                        error = $"Произошла непредвиденная ошибка: {ex.Message}";
                        Console.WriteLine(ex);
                    }
                }
            }

            return View("Index", new OwnersViewModel()
            {
                Owners = _context.Owners.ToList(),
                Error = error
            });
        }

        [HttpPost]
        public IActionResult Delete(int? id)
        {
            string? error = null;
            if (id == null)
                error = "Элемент не выбран";
            else
            {
                var owner = _context.Owners.FirstOrDefault(e => e.Id == id);
                if (owner == null)
                    error = "Выбранного элемента не существует";
                else
                {
                    try
                    {
                        _context.Owners.Remove(owner);
                        _context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        error = $"Произошла непредвиденная ошибка: {ex.Message}";
                        Console.WriteLine(ex);
                    }
                }
            }

            return View("Index", new OwnersViewModel()
            {
                Owners = _context.Owners.ToList(),
                Error = error
            });
        }

        [HttpGet]
        public IActionResult Filter(string? searchNumber, string? searchSecondName, string? searchName, string? searchSurname, string? searchLogin)
        {
            var items = _context.Owners.AsQueryable();

            if (searchNumber != null)
                items = items.Where(i => i.Number.Contains(searchNumber));
            if (searchSecondName != null)
                items = items.Where(i => i.SecondName.Contains(searchSecondName));
            if (searchName != null)
                items = items.Where(i => i.Name.Contains(searchName));
            if (searchSurname != null) //Поле Surname может быть нулевым
                items = items.Where(i => i.Surname.Contains(searchSurname));
            if (searchLogin != null)
                items = items.Where(i => i.Login.Contains(searchLogin));

            return View("Index", new OwnersViewModel()
            {
                Owners = items.ToList(),
            });
        }
        private string Transliterate(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var transliterationMap = new Dictionary<string, string>
            {
                {"а", "a"}, {"б", "b"}, {"в", "v"}, {"г", "g"}, {"д", "d"},
                {"е", "e"}, {"ё", "yo"}, {"ж", "zh"}, {"з", "z"}, {"и", "i"},
                {"й", "y"}, {"к", "k"}, {"л", "l"}, {"м", "m"}, {"н", "n"},
                {"о", "o"}, {"п", "p"}, {"р", "r"}, {"с", "s"}, {"т", "t"},
                {"у", "u"}, {"ф", "f"}, {"х", "kh"}, {"ц", "ts"}, {"ч", "ch"},
                {"ш", "sh"}, {"щ", "shch"}, {"ъ", ""}, {"ы", "y"}, {"ь", ""},
                {"э", "e"}, {"ю", "yu"}, {"я", "ya"},
                {"А", "A"}, {"Б", "B"}, {"В", "V"}, {"Г", "G"}, {"Д", "D"},
                {"Е", "E"}, {"Ё", "Yo"}, {"Ж", "Zh"}, {"З", "Z"}, {"И", "I"},
                {"Й", "Y"}, {"К", "K"}, {"Л", "L"}, {"М", "M"}, {"Н", "N"},
                {"О", "O"}, {"П", "P"}, {"Р", "R"}, {"С", "S"}, {"Т", "T"},
                {"У", "U"}, {"Ф", "F"}, {"Х", "Kh"}, {"Ц", "Ts"}, {"Ч", "Ch"},
                {"Ш", "Sh"}, {"Щ", "Shch"}, {"Ъ", ""}, {"Ы", "Y"}, {"Ь", ""},
                {"Э", "E"}, {"Ю", "Yu"}, {"Я", "Ya"}
            };

            foreach (var pair in transliterationMap)
            {
                text = text.Replace(pair.Key, pair.Value);
            }

            return text;
        }
        [HttpPost]
        public IActionResult Add(string? number, string? secondName, string? name, string? surname, string? login)
        {
            string? error = null;
            if (number == null || name == null || secondName == null)
            {
                error = "Не указан один из обязательных параметров (номер, имя или фамилия)";
            }
            else
            {
                try
                {
                    if (string.IsNullOrEmpty(login))
                    {
                        // Преобразуем имя и фамилию в латиницу
                        string transliteratedName = Transliterate(name);
                        string transliteratedSecondName = Transliterate(secondName);

                        // Базовый логин: первая буква имени + фамилия
                        login = $"{transliteratedName[0]}{transliteratedSecondName}";

                        // Проверка на уникальность логина
                        int suffixIndex = 1; // Индекс для добавления следующих букв имени
                        while (_context.Owners.Any(o => o.Login == login))
                        {
                            // Если логин уже существует, добавляем следующую букву имени
                            if (suffixIndex < transliteratedName.Length)
                            {
                                login = $"{transliteratedName.Substring(0, suffixIndex + 1)}{transliteratedSecondName}";
                                suffixIndex++;
                            }
                            else
                            {
                                // Если буквы имени закончились, берем следующую букву
                                int numericSuffix = 2;
                                

                                // Увеличиваем число, пока не найдем уникальный логин
                                while (_context.Owners.Any(o => o.Login == login))
                                {
                                    numericSuffix++;
                                    login = $"{transliteratedName}{transliteratedSecondName}{numericSuffix}";
                                }
                                break;
                            }
                        }
                    }

                    var owner = new Owner
                    {
                        Number = number,
                        SecondName = secondName,
                        Name = name,
                        Surname = surname, // Отчество (если есть)
                        Login = login
                    };

                    _context.Owners.Add(owner);
                    _context.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException is PostgresException e)
                    {
                        error = e.SqlState switch
                        {
                            "23503" => "Нельзя добавить запись с номером машины, отсутствующем в таблице машин",
                            _ => $"Ошибка базы данных: {e.MessageText}"
                        };
                    }
                    else
                    {
                        error = "Произошла непредвиденная ошибка";
                    }
                    Console.WriteLine(ex);
                }
                catch (Exception ex)
                {
                    error = $"Произошла непредвиденная ошибка: {ex.Message}";
                    Console.WriteLine(ex);
                }
            }

            return View("Index", new OwnersViewModel()
            {
                Owners = _context.Owners.ToList(),
                Error = error
            });
        }
    }
}