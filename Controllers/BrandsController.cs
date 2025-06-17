using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebAppTest.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebAppTest.Controllers
{
    public class BrandsController : Controller
    {
        private CarsContext _context;

        public BrandsController(CarsContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(new BrandsViewModel
            {
                Brands = _context.Brands.ToList()
            });
        }

        [HttpPost]
        public IActionResult Edit(string? id, string? title, string? fullTitle, string? country)
        {
            string? error = null;
            if (id == null)
                error = "Элемент не выбран";
            else
            {
                Brand? e = _context.Brands.Where(e => e.Title == id).FirstOrDefault();
                if (e == null)
                    error = $"Выбранного элемента не существует";
                else
                {
                    if (title != null) { e.Title = title; }
                    else if (fullTitle != null) { e.FullTitle = fullTitle; }
                    else if (country != null) { e.Country = country; }

                    try
                    {
                        _context.Brands.Update(e);
                        _context.SaveChanges();
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
                Brands = _context.Brands.ToList()
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
                Brand? e = _context.Brands.Where(e => e.Title == id).FirstOrDefault();
                if (e == null)
                    error = $"Выбранного элемента не существует";
                else
                {
                    try
                    {
                        _context.Brands.Remove(e);
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
                    var car = from Brand c in _context.Brands
                              where c.Title == title
                              select c;
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

        [HttpGet]
        public IActionResult Filter(string? title, string? fullTitle, string? country)
        {
            var items = _context.Brands.AsQueryable(); // Используем AsQueryable для дальнейшей фильтрации

            if (title != null)
                items = items.Where(brand => brand.Title.Contains(title)); // Частичное совпадение для названия
            if (fullTitle != null)
                items = items.Where(brand => brand.FullTitle.Contains(fullTitle)); // Частичное совпадение для полного названия
            if (country != null)
                items = items.Where(brand => brand.Country.Contains(country)); // Частичное совпадение для страны

            return View("Index", new BrandsViewModel
            {
                Brands = items.ToList()
            });
        }
    }
}
