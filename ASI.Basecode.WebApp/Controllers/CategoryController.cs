using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IExpenseService _expenseService; // Reference to ExpenseService

        public CategoryController(ICategoryService categoryService, IExpenseService expenseService)
        {
            _categoryService = categoryService;
            _expenseService = expenseService;
        }

        //public IActionResult Index()
        //{
        //    var categories = _categoryService.GetCategories();
        //    return View(categories);
        //}

        public IActionResult Index( int? categoryFilter, int pageNumber = 1, int pageSize = 10)
        {
            var categories = _categoryService.GetCategories();

            // Pass categories to the ViewBag for the dropdown
            ViewBag.Categories = categories;

            // Filter categories if a filter is applied
            if (categoryFilter.HasValue && categoryFilter.Value > 0)
            {
                categories = categories.Where(c => c.CategoryID == categoryFilter.Value).ToList();
            }
            var totalCategories = categories.Count();
            var totalPages = (int)Math.Ceiling((double)totalCategories / pageSize);

            categories = categories              
                .Skip((pageNumber - 1) * pageSize) 
                .Take(pageSize) 
                .ToList();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = pageNumber;

            return View(categories);
        }

        public IActionResult View(int id)
        {
            var expenses = _expenseService.GetExpenses()
                .Where(e => e.CategoryID == id)
                .ToList();

            ViewBag.CategoryName = _categoryService.GetCategories()
                .FirstOrDefault(c => c.CategoryID == id)?.CategoryName;

            return View(expenses);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _categoryService.AddCategory(category);
                return RedirectToAction("Index", "Home");
            }
            //return View(category);
            return PartialView("Create", category);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _categoryService.GetCategories().FirstOrDefault(c => c.CategoryID == id);
            if (category == null)
            {
                return NotFound();
            }
            TempData["PreviousPage"] = Request.Headers["Referer"].ToString();
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _categoryService.UpdateCategory(category);

                var previousPage = TempData["PreviousPage"]?.ToString();
                if (!string.IsNullOrEmpty(previousPage))
                {
                    return Redirect(previousPage); // Redirect to the page the user came from (Category Management)
                }

                //return RedirectToAction("Index","Home");
            }
            return View(category);
        }

        public IActionResult Delete(int id)
        {
            var category = _categoryService.GetCategories().FirstOrDefault(c => c.CategoryID == id);
            if (category != null)
            {
                _categoryService.DeleteCategory(category);
            }
            return RedirectToAction("Index","Home");
        }
    }
}