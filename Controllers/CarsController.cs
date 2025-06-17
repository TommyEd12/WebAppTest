using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Xml.Linq;
using WebAppTest.ViewModels;

namespace WebAppTest.Controllers
{
    public class CarsController : Controller
    {
        private CarsContext _context;

        public CarsController(CarsContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(new CarsViewModel()
            {
                Cars = _context.Cars.ToList(),
                Brands = _context.Brands.ToList()
            });
        }

        [HttpPost]
        public IActionResult Edit(string? id, string? newNumber, string? brand, string? model, string? color)
        {
            string? error = null;
            if (id == null)
                error = "Элемент не выбран";
            else
            {
                Car? e = _context.Cars.Where(e => e.Number == id).FirstOrDefault();
                if(e == null)
                    error = $"Выбранного элемента не существует";
                else
                {
                    if (newNumber != null) { e.Number = newNumber; }
                    else if (brand != null) { e.Brand = brand; }
                    else if (model != null) { e.Model = model; }
                    else if (color != null) { e.Color = color; }

                    try
                    {
                        _context.Cars.Update(e);
                        _context.SaveChanges();
                        
                    }
                    catch (DbUpdateException ex)
                    {
                        if (ex.InnerException != null)
                        {
                            PostgresException inner = (PostgresException)ex.InnerException;
                            if (inner.SqlState == "23503")
                                error = $"Нельзя добавить запись с брендом, " +
                                    $"отсуствующем в таблице Бренды";
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
                
            }

            return View("Index", new CarsViewModel()
            {
                Cars = _context.Cars.ToList(),
                Brands = _context.Brands.ToList(),
                Error = error
            });
        }

        [HttpPost]
        public IActionResult Delete(string? id)
        {
            string? error = null;
            if (id == null)
                error = "Элемент не выбран";
            else
            {
                Car? e = _context.Cars.Where(e => e.Number == id).FirstOrDefault();
                if (e == null)
                    error = $"Выбранного элемента не существует";
                else
                {
                    try
                    {
                        _context.Cars.Remove(e);
                        _context.SaveChanges();
                    }
                    catch (DbUpdateException ex)
                    {
                        if (ex.InnerException != null)
                        {
                            PostgresException inner = (PostgresException)ex.InnerException;
                            if (inner.SqlState == "23503")
                                error = $"Нельзя удалить запись, т.к она имеет зависимости в других таблицах";
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
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            return View("Index", new CarsViewModel()
            {
                Cars = _context.Cars.ToList(),
                Brands = _context.Brands.ToList(),
                Error = error
            });
        }

        [HttpGet]
        public IActionResult Filter(string? searchNumber, string? searchBrand, string? searchModel, string? searchColor)
        {
            var items = _context.Cars.AsQueryable(); 

            if (searchNumber != null)
                items = items.Where(car => car.Number.Contains(searchNumber)); // Частичное совпадение для номера
            if (searchBrand != null)
                items = items.Where(car => car.Brand.Contains(searchBrand)); // Частичное совпадение для марки
            if (searchModel != null)
                items = items.Where(car => car.Model.Contains(searchModel)); // Частичное совпадение для модели
            if (searchColor != null)
                items = items.Where(car => car.Color.Contains(searchColor)); // Частичное совпадение для цвета

            return View("Index", new CarsViewModel()
            {
                Cars = items.ToList(),
                Brands = _context.Brands.ToList()
            });
        }

        [HttpPost]
        public IActionResult Add(string? number, string? brand, string? model, string? color)
        {
            string? error = null;

            if (number == null || brand == null || model == null || color == null)
            {
                error = "Не уаказан один из параметров";
            }
            else
            {
                try
                {
                    var car = from Car c in _context.Cars
                              where c.Number == number
                              select c;
                    if (car.Count() == 1)
                    {
                        error = "Машина с данным номером уже существет";
                        throw new ArgumentException("Запись с данным номером уже существет");
                    }

                    _context.Cars.Add(new Car
                    {
                        Number = number,
                        Brand = brand,
                        Model = model,
                        Color = color

                    });
                    _context.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException != null)
                    {
                        PostgresException e = (PostgresException)ex.InnerException;
                        if (e.SqlState == "23503")
                            error = $"Нельзя добавить запись с брендом, " +
                                $"отсуствующем в таблице Бренды";
                    }
                    else
                    {
                        error = "Произошла непредвиденная ошибка";
                        Console.WriteLine(ex);
                    }
                }
                catch (ArgumentException ex)
                {
                    error = ex.Message;
                }
                catch (Exception ex)
                {
                    error = $"Произошла непредвиденная ошибка";
                    Console.WriteLine(ex);
                }
            }
            return View("Index", new CarsViewModel()
            {
                Cars = _context.Cars.ToList(),
                Brands = _context.Brands.ToList(),
                Error = error
            });
        }


    }
}
