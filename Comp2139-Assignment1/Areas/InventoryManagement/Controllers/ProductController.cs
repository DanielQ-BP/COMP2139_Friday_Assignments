using Comp2139_Assignment1.Areas.InventoryManagement.Models;
using Comp2139_Assignment1.Data;
using Comp2139_Assignment1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Comp2139_Assignment1.Areas.InventoryManagement.Controllers;

[Area("InventoryManagement")]
[Route("[area]/[controller]/[action]")]
[Authorize]
public class ProductController : Controller
{
    private readonly InventoryDBContext _context;
    private readonly ILogger<ProductController> _logger;

    public ProductController(InventoryDBContext context, ILogger<ProductController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string searchQuery, int? categoryId, decimal? minPrice, decimal? maxPrice, bool? lowStock, string sortBy)
    {
        try
        {
            var categories = await _context.Categories.ToListAsync();
            ViewData["Categories"] = categories.Any()
                ? new SelectList(categories, "Id", "Name")
                : new SelectList(new List<Category>(), "Id", "Name");

            var productsQuery = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
                productsQuery = productsQuery.Where(p => p.Name.Contains(searchQuery));

            if (categoryId.HasValue)
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId);

            if (minPrice.HasValue)
                productsQuery = productsQuery.Where(p => p.Price >= minPrice);

            if (maxPrice.HasValue)
                productsQuery = productsQuery.Where(p => p.Price <= maxPrice);

            if (lowStock.HasValue && lowStock.Value)
                productsQuery = productsQuery.Where(p => p.Quantity < p.Lowthreshhold);

            productsQuery = sortBy switch
            {
                "price_asc" => productsQuery.OrderBy(p => p.Price),
                "price_desc" => productsQuery.OrderByDescending(p => p.Price),
                "quantity_asc" => productsQuery.OrderBy(p => p.Quantity),
                "quantity_desc" => productsQuery.OrderByDescending(p => p.Quantity),
                "name_desc" => productsQuery.OrderByDescending(p => p.Name),
                _ => productsQuery.OrderBy(p => p.Name)
            };

            var products = await productsQuery.ToListAsync();

            return View(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading product list.");
            TempData["ErrorMessage"] = "An error occurred while loading products.";
            return View(new List<Product>());
        }
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public async Task<IActionResult> Create()
    {
        try
        {
            var categories = await _context.Categories.ToListAsync();

            ViewData["Categories"] = categories.Any()
                ? new SelectList(categories, "Id", "Name")
                : new SelectList(new List<Category>(), "Id", "Name");

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create form.");
            TempData["ErrorMessage"] = "An error occurred while preparing the product creation form.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public async Task<IActionResult> Create(Product product)
    {
        if (!ModelState.IsValid)
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        try
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Product created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product.");
            TempData["ErrorMessage"] = "Failed to create the product.";
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        try
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading product for edit.");
            TempData["ErrorMessage"] = "Unable to load product for editing.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        try
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Product updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Products.Any(p => p.Id == product.Id)) return NotFound();
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product.");
            TempData["ErrorMessage"] = "Failed to update product.";
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }
    }
    
    [HttpGet]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            return View(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading product for deletion.");
            TempData["ErrorMessage"] = "Unable to load product for deletion.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product.");
            TempData["ErrorMessage"] = "Failed to delete the product.";
            return RedirectToAction(nameof(Index));
        }
    }

    public async Task<IActionResult> InventoryOverview()
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading inventory overview.");
            TempData["ErrorMessage"] = "Could not load inventory overview.";
            return RedirectToAction(nameof(Index));
        }
    }
}
