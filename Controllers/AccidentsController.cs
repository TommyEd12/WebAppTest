using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebAppTest.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;


/// <summary>
/// Контроллер для управления записями об авариях.
/// </summary>
namespace WebAppTest.Controllers
{
    /// <summary>
    /// Контроллер для обработки операций с авариями, включая CRUD и фильтрацию.
    /// </summary>
    public class AccidentsController : Controller
    {
        private CarsContext _context;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AccidentsController"/>.
        /// </summary>
        /// <param name="context">Контекст базы данных для информации об автомобилях и авариях.</param>
        public AccidentsController(CarsContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Отображает главное представление со списком всех аварий.
        /// </summary>
        /// <returns>Представление со всеми записями об авариях.</returns>
        public IActionResult Index()
        {
            return View(new AccidentsViewModel
            {
                Accidents = _context.Accidents.ToList(),
            });
        }

        /// <summary>
        /// Редактирует существующую запись об аварии.
        /// </summary>
        /// <param name="id">ID аварии для редактирования.</param>
        /// <param name="number">Номер автомобиля, связанного с аварией.</param>
        /// <param name="date">Дата аварии.</param>
        /// <param name="login">Логин, связанный с записью об аварии.</param>
        /// <param name="deparartureAddress">Адрес отправления.</param>
        /// <param name="destinationAddress">Адрес назначения.</param>
        /// <param name="sum">Сумма, связанная с аварией.</param>
        /// <returns>Обновленное представление с записями об авариях или сообщением об ошибке.</returns>
        [HttpPost]
        public IActionResult Edit(int? id,
            string? number, DateOnly? date, string? login,
            string? deparartureAddress, string? destinationAddress, decimal? sum)
        {
            string? error = null;
            if (id == null)
            {
                error = $"Не укзан id";
            }
            else
            {
                Accident? accident = _context.Accidents.Where(accident => accident.Id == id).FirstOrDefault();
                if (accident == null)
                {
                    error = $"Выбранного элемента не существует";
                }
                else
                {
                    if (number != null) accident.Number = number;
                    if (date != null) accident.Date = (DateOnly)date;
                    if (login != null) accident.Login = login;
                    if (deparartureAddress != null) accident.Departureaddress = deparartureAddress;
                    if (destinationAddress != null) accident.Destinationaddress = destinationAddress;
                    if (sum != null) accident.Sum = (decimal)sum;

                    try
                    {
                        _context.Accidents.Update(accident);
                        _context.SaveChanges();
                    }
                    catch (DbUpdateException updateException)
                    {
                        if (updateException.InnerException != null)
                        {
                            PostgresException inner = (PostgresException)updateException.InnerException;
                            if (inner.SqlState == "23503")
                                error = $"Нельзя добавить запись с номером машины, " +
                                    $"отсуствующем в таблице машины";
                            else if (inner.SqlState == "23514" || inner.SqlState == "22001")
                                error = $"Неправильный формат номера машины";
                            else
                                Console.WriteLine(inner.Message);
                        }
                        else
                        {
                            error = "Произошла непредвиденная ошибка";
                            Console.WriteLine(updateException);
                        }
                    }
                    catch (Exception updateException)
                    {
                        error = $"{updateException.Message}";
                    }
                }
            }
            return View("Index", new AccidentsViewModel
            {
                Accidents = _context.Accidents.ToList(),
                Error = error
            });
        }

        /// <summary>
        /// Удаляет запись об аварии.
        /// </summary>
        /// <param name="id">ID аварии для удаления.</param>
        /// <returns>Обновленное представление с оставшимися записями или сообщением об ошибке.</returns>
        [HttpPost]
        public IActionResult Delete(int? id)
        {
            string? error = null;
            if (id == null)
                error = "Элемент не выбран";
            else
            {
                Accident? accident = _context.Accidents.Where(accident => accident.Id == id).FirstOrDefault();
                if (accident == null)
                    error = $"Выбранного элемента не существует";
                else
                {
                    try
                    {
                        _context.Accidents.Remove(accident);
                        _context.SaveChanges();
                    }
                    catch (Exception deleteException)
                    {
                        error = $"Произошла непредвиденная ошибка";
                        Console.WriteLine(deleteException.Message);
                    }
                }
            }
            return View("Index", new AccidentsViewModel
            {
                Accidents = _context.Accidents.ToList(),
                Error = error
            });
        }

        /// <summary>
        /// Добавляет новую запись об аварии.
        /// </summary>
        /// <param name="number">Номер автомобиля, связанного с аварией.</param>
        /// <param name="date">Дата аварии.</param>
        /// <param name="login">Логин, связанный с записью об аварии.</param>
        /// <param name="deparartureAddress">Адрес отправления.</param>
        /// <param name="destinationAddress">Адрес назначения.</param>
        /// <param name="sum">Сумма, связанная с аварией.</param>
        /// <returns>Обновленное представление со всеми записями об авариях, включая новую, или сообщение об ошибке.</returns>
        [HttpPost]
        public IActionResult Add(string? number, DateOnly? date, string? login,
            string? deparartureAddress, string? destinationAddress, decimal? sum)
        {
            string? error = null;
            if (number == null || date == null || login == null ||
                deparartureAddress == null || destinationAddress == null || sum == null)
                error = "Не уаказан один из параметров";
            else
            {
                try
                {
                    _context.Accidents.Add(new Accident
                    {
                        Number = number,
                        Date = (DateOnly)date,
                        Login = login,
                        Departureaddress = deparartureAddress,
                        Destinationaddress = destinationAddress,
                        Sum = (decimal)sum
                    });
                    _context.SaveChanges();
                }
                catch (DbUpdateException updateException)
                {
                    if (updateException.InnerException != null)
                    {
                        PostgresException accident = (PostgresException)updateException.InnerException;
                        if (accident.SqlState == "23503")
                            error = $"Нельзя добавить запись с номером машины, " +
                                $"отсуствующем в таблице машины";
                    }
                    else
                    {
                        error = "Произошла непредвиденная ошибка";
                        Console.WriteLine(updateException);
                    }
                }
                catch (Exception updateException)
                {
                    error = $"Произошла непредвиденная ошибка";
                    Console.WriteLine(updateException);
                }
            }
            return View("Index", new AccidentsViewModel
            {
                Accidents = _context.Accidents.ToList(),
                Error = error
            });
        }

        /// <summary>
        /// Фильтрует записи об авариях по указанным критериям.
        /// </summary>
        /// <param name="searchNumber">Номер автомобиля для фильтрации.</param>
        /// <param name="searchDate">Дата аварии для фильтрации.</param>
        /// <param name="searchLogin">Логин для фильтрации.</param>
        /// <param name="searchDepartureAddress">Адрес отправления для фильтрации.</param>
        /// <param name="searchDestinationAddress">Адрес назначения для фильтрации.</param>
        /// <param name="searchSum">Сумма для фильтрации.</param>
        /// <returns>Представление с отфильтрованными записями об авариях.</returns>
        [HttpGet]
        public IActionResult Filter(string? searchNumber, DateOnly? searchDate, string? searchLogin,
            string? searchDepartureAddress, string? searchDestinationAddress, decimal? searchSum)
        {
            var items = _context.Accidents.AsQueryable();

            if (searchNumber != null)
                items = items.Where(a => a.Number.Contains(searchNumber));
            if (searchDate != null)
                items = items.Where(a => a.Date == searchDate);
            if (searchLogin != null)
                items = items.Where(a => a.Login.Contains(searchLogin));
            if (searchDepartureAddress != null)
                items = items.Where(a => a.Departureaddress.Contains(searchDepartureAddress));
            if (searchDestinationAddress != null)
                items = items.Where(a => a.Destinationaddress.Contains(searchDestinationAddress));
            if (searchSum != null)
                items = items.Where(a => a.Sum == searchSum);

            return View("Index", new AccidentsViewModel
            {
                Accidents = items.ToList(),
            });
        }
    }
}