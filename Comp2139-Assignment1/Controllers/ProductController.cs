using Comp2139_Assignment1.Data;
using Comp2139_Assignment1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Comp2139_Assignment1.Controllers;

public class ProductController : Controller
{
    private readonly InventoryDBContext _context;

    public ProductController(InventoryDBContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string searchQuery, int? categoryId, decimal? minPrice, decimal? maxPrice, bool? lowStock, string sortBy)
{
    var categories = await _context.Categories.ToListAsync();
    ViewData["Categories"] = categories; // Prevents null reference
    
    if (!categories.Any())
    {
        ViewData["Categories"] = new List<Category>(); // Prevents null reference
    }
    else
    {
        ViewData["Categories"] = new SelectList(categories, "Id", "Name");
    }

    
    // Start with all products
    var productsQuery = _context.Products.Include(p => p.Category).AsQueryable();

    // Apply search filter
    if (!string.IsNullOrEmpty(searchQuery))
    {
        productsQuery = productsQuery.Where(p => p.Name.Contains(searchQuery));
    }

    // Apply category filter
    if (categoryId.HasValue)
    {
        productsQuery = productsQuery.Where(p => p.CategoryId == categoryId);
    }

    // Apply price range filter
    if (minPrice.HasValue)
    {
        productsQuery = productsQuery.Where(p => p.Price >= minPrice);
    }
    if (maxPrice.HasValue)
    {
        productsQuery = productsQuery.Where(p => p.Price <= maxPrice);
    }

    // Apply low-stock filter
    if (lowStock.HasValue && lowStock.Value)
    {
        productsQuery = productsQuery.Where(p => p.Quantity < p.Lowthreshhold);
    }

    // Apply sorting
    switch (sortBy)
    {
        case "price_asc":
            productsQuery = productsQuery.OrderBy(p => p.Price);
            break;
        case "price_desc":
            productsQuery = productsQuery.OrderByDescending(p => p.Price);
            break;
        case "quantity_asc":
            productsQuery = productsQuery.OrderBy(p => p.Quantity);
            break;
        case "quantity_desc":
            productsQuery = productsQuery.OrderByDescending(p => p.Quantity);
            break;
        case "name_asc":
            productsQuery = productsQuery.OrderBy(p => p.Name);
            break;
        case "name_desc":
            productsQuery = productsQuery.OrderByDescending(p => p.Name);
            break;
        default:
            productsQuery = productsQuery.OrderBy(p => p.Name); // Default sorting by name
            break;
    }

    // Execute the query and pass the results to the view
    var products = await productsQuery.ToListAsync();

    // Pass categories to the view for the category filter dropdown
    ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");

    return View(products);
}

    public async Task<IActionResult> Create()
    {
        var categories = await _context.Categories.ToListAsync();

        if (!categories.Any())
        {
            TempData["ErrorMessage"] = "No categories available.";
            ViewData["Categories"] = new SelectList(new List<Category>(), "Id", "Name");
        }
        else
        {
            ViewData["Categories"] = new SelectList(categories, "Id", "Name");
        }

        return View();
    }

    

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (ModelState.IsValid)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
        return View(product);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
        return View(product);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Products.Any(p => p.Id == product.Id))
                {
                    return NotFound();
                }
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
        return View(product);
    }

    // Delete: Show confirmation page to delete a product
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    // Delete: Process the deletion of a product
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
    
    public async Task<IActionResult> InventoryOverview()
    {
        var totalProducts = await _context.Products.CountAsync();
        var totalCategories = await _context.Categories.CountAsync();
        var lowStockProducts = await _context.Products
            .Where(p => p.Quantity < p.Lowthreshhold)
            .Include(p => p.Category)
            .ToListAsync();

        var model = new InventoryOverviewViewModel
        {
            TotalProducts = totalProducts,
            TotalCategories = totalCategories,
            LowStockProducts = lowStockProducts
        };

        return View(model);
    }

}
