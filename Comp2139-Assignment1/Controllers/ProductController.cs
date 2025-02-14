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

    public async Task<IActionResult> Index(string searchQuery)
    {
        var products = await _context.Products.Include(p => p.Category).ToListAsync();
        return View(products);
    }

    public IActionResult Create()
    {
        ViewData["Category"] = new SelectList(_context.Categories, "Id", "Name");
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

        ViewData["CategoryId"] = new SelectList(_context.Categories, "id", "Name", product.CategoryId);

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
}