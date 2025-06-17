using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebAppTest.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;


//Http методы должны быть асинхронными
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