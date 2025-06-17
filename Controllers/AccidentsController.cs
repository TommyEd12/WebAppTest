using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebAppTest.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebAppTest.Controllers
{
    public class AccidentsController : Controller
    {
        private CarsContext _context;

        public AccidentsController(CarsContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(new AccidentsViewModel
            {
                Accidents = _context.Accidents.ToList(),
            });
        }

        [HttpPost]
        public IActionResult Edit(int? id,
    string? number, DateOnly? date, string? login,
    string? departureaddress, string? destinationaddress, decimal? sum)
        {
            string? error = null;
            if (id == null)
            {
                error = $"Не укзан id";
            }
            else
            {
                Accident? e = _context.Accidents.Where(e => e.Id == id).FirstOrDefault();
                if (e == null)
                {
                    error = $"Выбранного элемента не существует";
                }
                else
                {
                    // Обновляем каждое поле, если оно передано
                    if (number != null) e.Number = number;
                    if (date != null) e.Date = (DateOnly)date;
                    if (login != null) e.Login = login;
                    if (departureaddress != null) e.Departureaddress = departureaddress;
                    if (destinationaddress != null) e.Destinationaddress = destinationaddress;
                    if (sum != null) e.Sum = (decimal)sum;

                    try
                    {
                        _context.Accidents.Update(e);
                        _context.SaveChanges();
                    }
                    catch (DbUpdateException ex)
                    {
                        if (ex.InnerException != null)
                        {
                            PostgresException inner = (PostgresException)ex.InnerException;
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
                            Console.WriteLine(ex);
                        }
                    }
                    catch (Exception ex)
                    {
                        error = $"{ex.Message}";
                    }
                }
            }
            return View("Index", new AccidentsViewModel
            {
                Accidents = _context.Accidents.ToList(),
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
                Accident? e = _context.Accidents.Where(e => e.Id == id).FirstOrDefault();
                if (e == null)
                    error = $"Выбранного элемента не существует";
                else
                {
                    try
                    {
                        _context.Accidents.Remove(e);
                        _context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        error = $"Произошла непредвиденная ошибка";
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            return View("Index", new AccidentsViewModel
            {
                Accidents = _context.Accidents.ToList(),
                Error = error
            });
        }

        [HttpPost]
        public IActionResult Add(string? number, DateOnly? date, string? login,
            string? departureaddress, string? destinationaddress, decimal? sum)
        {
            string? error = null;
            if (number == null || date == null || login == null ||
                departureaddress == null || destinationaddress == null || sum == null)
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
                        Departureaddress = departureaddress,
                        Destinationaddress = destinationaddress,
                        Sum = (decimal)sum
                    });
                    _context.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException != null)
                    {
                        PostgresException e = (PostgresException)ex.InnerException;
                        if (e.SqlState == "23503")
                            error = $"Нельзя добавить запись с номером машины, " +
                                $"отсуствующем в таблице машины";
                    }
                    else
                    {
                        error = "Произошла непредвиденная ошибка";
                        Console.WriteLine(ex);
                    }
                }
                catch (Exception ex)
                {
                    error = $"Произошла непредвиденная ошибка";
                    Console.WriteLine(ex);
                }
            }
            return View("Index", new AccidentsViewModel
            {
                Accidents = _context.Accidents.ToList(),
                Error = error
            });
        }

        [HttpGet]
        public IActionResult Filter(string? searchNumber, DateOnly? searchDate, string? searchLogin,
    string? searchDepartureAddress, string? searchDestinationAddress, decimal? searchSum)
        {
            var items = _context.Accidents.AsQueryable(); // Фильтруем данные из таблицы Accidents

            if (searchNumber != null)
                items = items.Where(a => a.Number.Contains(searchNumber)); // Фильтрация по номеру
            if (searchDate != null)
                items = items.Where(a => a.Date == searchDate); // Фильтрация по дате
            if (searchLogin != null)
                items = items.Where(a => a.Login.Contains(searchLogin)); // Фильтрация по логину
            if (searchDepartureAddress != null)
                items = items.Where(a => a.Departureaddress.Contains(searchDepartureAddress)); // Фильтрация по адресу отправления
            if (searchDestinationAddress != null)
                items = items.Where(a => a.Destinationaddress.Contains(searchDestinationAddress)); // Фильтрация по адресу назначения
            if (searchSum != null)
                items = items.Where(a => a.Sum == searchSum); // Фильтрация по сумме

            return View("Index", new AccidentsViewModel
            {
                Accidents = items.ToList(),
            });
        }
    }
}