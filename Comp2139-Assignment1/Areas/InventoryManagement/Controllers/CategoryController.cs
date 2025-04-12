using Comp2139_Assignment1.Areas.InventoryManagement.Models;
using Comp2139_Assignment1.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Comp2139_Assignment1.Areas.InventoryManagement.Controllers;

[Area("InventoryManagement")]
[Route("[area]/[controller]/[action]")]
[Authorize]
public class CategoriesController : Controller
{
    private readonly InventoryDBContext _context;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(InventoryDBContext context, ILogger<CategoriesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Categories
    public async Task<IActionResult> Index()
    {
        try
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categories.");
            TempData["ErrorMessage"] = "An error occurred while loading categories.";
            return View(new List<Category>());
        }
    }

    // GET: Categories/Create
    public IActionResult Create()
    {
        try
        {
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error preparing create category form.");
            TempData["ErrorMessage"] = "An error occurred while preparing the category creation form.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Categories/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public async Task<IActionResult> Create([Bind("Name")] Category category)
    {
        if (!ModelState.IsValid)
        {
            return View(category);
        }

        try
        {
            _context.Add(category);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Category created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category.");
            TempData["ErrorMessage"] = "Failed to create the category.";
            return View(category);
        }
    }

    // GET: Categories/Edit/5
    [HttpGet]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        try
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading category for editing.");
            TempData["ErrorMessage"] = "Failed to load the category for editing.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Categories/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Category category)
    {
        if (id != category.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(category);
        }

        try
        {
            _context.Update(category);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Category updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CategoryExists(category.Id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category.");
            TempData["ErrorMessage"] = "Failed to update the category.";
            return View(category);
        }
    }
    
    [HttpGet]
    [Authorize(Roles = "SuperAdmin, Admin")]
    // GET: Categories/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        try
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading category for deletion.");
            TempData["ErrorMessage"] = "Failed to load the category for deletion.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Categories/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Category deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category.");
            TempData["ErrorMessage"] = "Failed to delete the category.";
            return RedirectToAction(nameof(Index));
        }
    }

    private bool CategoryExists(int id)
    {
        try
        {
            return _context.Categories.Any(e => e.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking category existence.");
            return false;
        }
    }
}