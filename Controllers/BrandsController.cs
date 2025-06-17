using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebAppTest.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebAppTest.Controllers
{
    /// <summary>
    /// Контроллер для управления брендами автомобилей.
    /// </summary>
    public class BrandsController : Controller
    {
        private CarsContext _context;

        /// <summary>
        /// Инициализирует новый экземпляр контроллера брендов.
        /// </summary>
        /// <param name="context">Контекст базы данных для работы с брендами.</param>
        public BrandsController(CarsContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Отображает список всех брендов.
        /// </summary>
        /// <returns>Представление со списком брендов.</returns>
        public IActionResult Index()
        {
            return View(new BrandsViewModel
            {
                Brands = _context.Brands.ToList()
            });
        }

        /// <summary>
        /// Редактирует существующий бренд.
        /// </summary>
        /// <param name="id">Идентификатор бренда (название).</param>
        /// <param name="title">Краткое название бренда.</param>
        /// <param name="fullTitle">Полное название бренда.</param>
        /// <param name="country">Страна производителя.</param>
        /// <returns>Обновленное представление со списком брендов.</returns>
        [HttpPost]
        public IActionResult Edit(string? id, string? title, string? fullTitle, string? country)
        {
            string? error = null;
            if (id == null)
                error = "Элемент не выбран";
            else
            {
                Brand? brand = _context.Brands.Where(brand => brand.Title == id).FirstOrDefault();
                if (brand == null)
                    error = $"Выбранного элемента не существует";
                else
                {
                    if (title != null) { brand.Title = title; }
                    else if (fullTitle != null) { brand.FullTitle = fullTitle; }
                    else if (country != null) { brand.Country = country; }

                    try
                    {
                        _context.Brands.Update(brand);
                        _context.SaveChanges();
                    }
                    catch (Exception editException)
                    {
                        error = $"Произошла непредвиденная ошибка";
                        Console.WriteLine(editException.Message);
                    }
                }
            }
            return View("Index", new BrandsViewModel
            {
                Brands = _context.Brands.ToList()
            });
        }

        /// <summary>
        /// Удаляет бренд из системы.
        /// </summary>
        /// <param name="id">Идентификатор бренда (название) для удаления.</param>
        /// <returns>Обновленное представление со списком брендов и сообщением об ошибке (если есть).</returns>
        [HttpPost]
        public IActionResult Delete(string? id)
        {
            string? error = null;
            if (id == null)
                error = "Элемент не выбран";
            else
            {
                Brand? brand = _context.Brands.Where(brand => brand.Title == id).FirstOrDefault();
                if (brand == null)
                    error = $"Выбранного элемента не существует";
                else
                {
                    try
                    {
                        _context.Brands.Remove(brand);
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
            return View("Index", new BrandsViewModel
            {
                Brands = _context.Brands.ToList(),
                Error = error
            });
        }

        /// <summary>
        /// Добавляет новый бренд в систему.
        /// </summary>
        /// <param name="title">Краткое название бренда.</param>
        /// <param name="fullTitle">Полное название бренда.</param>
        /// <param name="country">Страна производителя.</param>
        /// <returns>Обновленное представление со списком брендов и сообщением об ошибке (если есть).</returns>
        [HttpPost]
        public IActionResult Add(string? title, string? fullTitle, string? country)
        {
            string? error = null;
            if (title == null || fullTitle == null || country == null)
                error = "Не уаказан один из параметров";
            else
            {
                try
                {
                    var car = from Brand currentCar in _context.Brands
                              where currentCar.Title == title
                              select currentCar;
                    if (car.Count() == 1)
                    {
                        throw new ArgumentException("Запись с данным номером уже существет");
                    }
                    _context.Brands.Add(new Brand
                    {
                        Title = title,
                        FullTitle = fullTitle,
                        Country = country
                    });
                    _context.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException != null)
                    {
                        PostgresException brand = (PostgresException)ex.InnerException;
                        if (brand.SqlState == "23503")
                            error = $"Нельзя добавить запись с номером машины, " +
                                $"отсуствующем в таблице машины";
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

            return View("Index", new BrandsViewModel
            {
                Brands = _context.Brands.ToList(),
                Error = error
            });
        }

        /// <summary>
        /// Фильтрует бренды по указанным критериям.
        /// </summary>
        /// <param name="title">Краткое название для фильтрации.</param>
        /// <param name="fullTitle">Полное название для фильтрации.</param>
        /// <param name="country">Страна производителя для фильтрации.</param>
        /// <returns>Представление с отфильтрованным списком брендов.</returns>
        [HttpGet]
        public IActionResult Filter(string? title, string? fullTitle, string? country)
        {
            var brands = _context.Brands.AsQueryable();

            if (title != null)
                brands = brands.Where(brand => brand.Title.Contains(title));
            if (fullTitle != null)
                brands = brands.Where(brand => brand.FullTitle.Contains(fullTitle));
            if (country != null)
                brands = brands.Where(brand => brand.Country.Contains(country));

            return View("Index", new BrandsViewModel
            {
                Brands = brands.ToList()
            });
        }
    }
}