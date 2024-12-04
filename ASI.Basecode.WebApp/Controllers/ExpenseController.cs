using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Drawing.Printing;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    public class ExpenseController : Controller
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly ICategoryService _categoryService; // Inject ICategoryService

        public ExpenseController(IExpenseRepository expenseRepository, ICategoryService categoryService)
        {
            _expenseRepository = expenseRepository;
            _categoryService = categoryService; // Initialize the category service
        }

        public IActionResult Index(int pageNumber = 1, int pageSize = 10, string startDate = null, string endDate = null, int? category = null)
        {
            var expenses = _expenseRepository.ViewExpenses();

            // Apply filters if any
            if (!string.IsNullOrWhiteSpace(startDate))
            {
                DateTime start = DateTime.Parse(startDate);
                expenses = expenses.Where(e => e.Date >= start).ToList();
            }

            if (!string.IsNullOrWhiteSpace(endDate))
            {
                DateTime end = DateTime.Parse(endDate);
                expenses = expenses.Where(e => e.Date <= end).ToList();
            }

            if (category.HasValue)
            {
                expenses = expenses.Where(e => e.CategoryID == category.Value).ToList();
            }

            // Pagination logic: Calculate total pages
            var totalExpenses = expenses.Count();
            var totalPages = (int)Math.Ceiling((double)totalExpenses /pageSize);

            // Get the current page's expenses
            expenses = expenses
                .OrderByDescending(e => e.Date) // Order by date (most recent first)
                .Skip((pageNumber - 1) * pageSize) // Skip items for previous pages
                .Take(pageSize) // Take only the current page's items
                .ToList();

            // Pass pagination data to the view
            ViewBag.Categories = _categoryService.GetCategories(); // Categories for the dropdown
            ViewBag.TotalPages = totalPages; // Total number of pages
            ViewBag.CurrentPage = pageNumber; // Current page number

            return View("Index", expenses);
        }


        public IActionResult Create()
        {
            ViewBag.Categories = _categoryService.GetCategories(); // Ensure categories are set
            return View();
        }

        [HttpPost]
        public IActionResult Create(Expense expense)
        {
            if (ModelState.IsValid)
            {
                _expenseRepository.AddExpense(expense);


                return RedirectToAction("Index","Home");
            }

            // If there’s an error, return the validation error view data
            ViewBag.Categories = _categoryService.GetCategories();
            return PartialView("Create", expense);
        }

        public IActionResult Edit(int id)
        {
            var expense = _expenseRepository.ViewExpenses().FirstOrDefault(e => e.ExpenseID == id);
            if (expense == null)
            {
                return NotFound();
            }

            ViewBag.Categories = _categoryService.GetCategories(); // Load categories for the dropdown
            return View(expense);
        }

        
        [HttpPost]
        public IActionResult Edit(Expense expense)
        {
            if (ModelState.IsValid)
            {
                _expenseRepository.UpdateExpense(expense);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Categories = _categoryService.GetCategories(); // Reload categories in case of validation error
            return View(expense); // Return the view with the invalid model to display errors
        }


        public IActionResult Delete(int id)
        {
            var expense = _expenseRepository.ViewExpenses().FirstOrDefault(e => e.ExpenseID == id);
            if (expense != null)
            {
                _expenseRepository.DeleteExpense(expense);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}