using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Xml.Linq;
using WebAppTest.ViewModels;

namespace WebAppTest.Controllers
{
    /// <summary>
    /// Контроллер для управления автомобилями.
    /// Обеспечивает CRUD-операции и фильтрацию записей об автомобилях.
    /// </summary>
    public class CarsController : Controller
    {
        private CarsContext _context;

        /// <summary>
        /// Инициализирует новый экземпляр контроллера автомобилей.
        /// </summary>
        /// <param name="context">Контекст базы данных для работы с автомобилями.</param>
        public CarsController(CarsContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Отображает список всех автомобилей.
        /// </summary>
        /// <returns>Представление со списком автомобилей и брендов.</returns>
        public IActionResult Index()
        {
            return View(new CarsViewModel()
            {
                Cars = _context.Cars.ToList(),
                Brands = _context.Brands.ToList()
            });
        }

        /// <summary>
        /// Редактирует существующий автомобиль.
        /// </summary>
        /// <param name="id">Номер автомобиля для редактирования.</param>
        /// <param name="newNumber">Новый номер автомобиля.</param>
        /// <param name="brand">Новый бренд автомобиля.</param>
        /// <param name="model">Новая модель автомобиля.</param>
        /// <param name="color">Новый цвет автомобиля.</param>
        /// <returns>Обновленное представление со списком автомобилей и сообщением об ошибке (если есть).</returns>
        [HttpPost]
        public IActionResult Edit(string? id, string? newNumber, string? brand, string? model, string? color)
        {
            string? error = null;
            if (id == null)
                error = "Элемент не выбран";
            else
            {
                Car? car = _context.Cars.Where(car => car.Number == id).FirstOrDefault();
                if(car == null)
                    error = $"Выбранного элемента не существует";
                else
                {
                    if (newNumber != null) { car.Number = newNumber; }
                    else if (brand != null) { car.Brand = brand; }
                    else if (model != null) { car.Model = model; }
                    else if (color != null) { car.Color = color; }

                    try
                    {
                        _context.Cars.Update(car);
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

        /// <summary>
        /// Удаляет автомобиль из системы.
        /// </summary>
        /// <param name="id">Номер автомобиля для удаления.</param>
        /// <returns>Обновленное представление со списком автомобилей и сообщением об ошибке (если есть).</returns>
        [HttpPost]
        public IActionResult Delete(string? id)
        {
            string? error = null;
            if (id == null)
                error = "Элемент не выбран";
            else
            {
                Car? car = _context.Cars.Where(car => car.Number == id).FirstOrDefault();
                if (car == null)
                    error = $"Выбранного элемента не существует";
                else
                {
                    try
                    {
                        _context.Cars.Remove(car);
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

        /// <summary>
        /// Фильтрует автомобили по указанным критериям.
        /// </summary>
        /// <param name="searchNumber">Номер автомобиля для фильтрации.</param>
        /// <param name="searchBrand">Бренд для фильтрации.</param>
        /// <param name="searchModel">Модель для фильтрации.</param>
        /// <param name="searchColor">Цвет для фильтрации.</param>
        /// <returns>Представление с отфильтрованным списком автомобилей.</returns>
        [HttpGet]
        public IActionResult Filter(string? searchNumber, string? searchBrand, string? searchModel, string? searchColor)
        {
            var items = _context.Cars.AsQueryable(); 

            if (searchNumber != null)
                items = items.Where(car => car.Number.Contains(searchNumber)); 
            if (searchBrand != null)
                items = items.Where(car => car.Brand.Contains(searchBrand));
            if (searchModel != null)
                items = items.Where(car => car.Model.Contains(searchModel));
            if (searchColor != null)
                items = items.Where(car => car.Color.Contains(searchColor));

            return View("Index", new CarsViewModel()
            {
                Cars = items.ToList(),
                Brands = _context.Brands.ToList()
            });
        }

        /// <summary>
        /// Добавляет новый автомобиль в систему.
        /// </summary>
        /// <param name="number">Номер автомобиля.</param>
        /// <param name="brand">Бренд автомобиля.</param>
        /// <param name="model">Модель автомобиля.</param>
        /// <param name="color">Цвет автомобиля.</param>
        /// <returns>Обновленное представление со списком автомобилей и сообщением об ошибке (если есть).</returns>
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
                        PostgresException car = (PostgresException)ex.InnerException;
                        if (car.SqlState == "23503")
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