using Comp2139_Assignment1.Areas.InventoryManagement.Models;
using Comp2139_Assignment1.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Comp2139_Assignment1.Areas.InventoryManagement.Controllers;

[Area("InventoryManagement")]
[Route("[area]/[controller]/[action]")]
[Authorize]
public class OrdersController : Controller
{
    private readonly InventoryDBContext _context;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(InventoryDBContext context, ILogger<OrdersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public IActionResult Index()
    {
        try
        {
            var orders = _context.Orders.ToList();
            return View(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading orders.");
            TempData["ErrorMessage"] = "An error occurred while loading orders.";
            return View(new List<Orders>());
        }
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public async Task<IActionResult> Create()
    {
        try
        {
            var categories = await _context.Categories.ToListAsync();
            var products = await _context.Products.ToListAsync();

            if (!categories.Any() || !products.Any())
            {
                ModelState.AddModelError("", "No categories or products available.");
                ViewData["Categories"] = new List<SelectListItem>();
                ViewData["Products"] = new List<SelectListItem>();
                return View(new Orders());
            }

            ViewData["Categories"] = new SelectList(categories, "Id", "Name");
            ViewData["Products"] = new SelectList(products, "Id", "Name");

            return View(new Orders());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error preparing create order form.");
            TempData["ErrorMessage"] = "An error occurred while preparing the order creation form.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public async Task<IActionResult> Create(Orders orders, List<int> productIds, List<int> quantities)
    {
        if (productIds.Count != quantities.Count)
        {
            ModelState.AddModelError("", "Products and quantities must match.");
            return View(orders);
        }

        try
        {
            if (ModelState.IsValid)
            {
                orders.OrderDate = DateTime.UtcNow;

                _context.Orders.Add(orders);
                await _context.SaveChangesAsync();

                for (int i = 0; i < productIds.Count; i++)
                {
                    var orderItem = new OrderItem
                    {
                        ProductId = productIds[i],
                        OrderId = orders.Id,
                        Quantity = quantities[i]
                    };
                    _context.OrderItems.Add(orderItem);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("OrderConfirmation", new { id = orders.Id });
            }
            
            ViewData["Categories"] = new SelectList(await _context.Categories.ToListAsync() , "Id", "Name");
            ViewData["Products"] = new SelectList(await _context.Products.ToListAsync() , "Id", "Name");
            return View(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order.");
            TempData["ErrorMessage"] = "Failed to create the order.";
            return RedirectToAction(nameof(Index));
        }
    }

    [Authorize(Roles = "SuperAdmin, Admin")]
    public async Task<IActionResult> OrderConfirmation(int id)
    {
        try
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            decimal totalPrice = order.OrderItems.Sum(oi => oi.Product.Price * oi.Quantity);
            ViewData["TotalPrice"] = totalPrice;

            return View(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error displaying order confirmation.");
            TempData["ErrorMessage"] = "Failed to display order confirmation.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var order = await _context.Orders.Include(o => o.OrderItems)
                                             .ThenInclude(oi => oi.Product)
                                             .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            ViewData["Products"] = new SelectList(await _context.Products.ToListAsync(), "Id", "Name");
            return View(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading order for editing.");
            TempData["ErrorMessage"] = "Failed to load the order for editing.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public async Task<IActionResult> Edit(int id, Orders orders, List<int> productIds, List<int> quantities)
    {
        if (productIds.Count != quantities.Count)
        {
            ModelState.AddModelError("", "Products and quantities must match.");
            return View(orders);
        }

        try
        {
            if (id != orders.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                orders.OrderDate = orders.OrderDate.ToUniversalTime();

                _context.Update(orders);
                await _context.SaveChangesAsync();

                var existingItems = _context.OrderItems.Where(oi => oi.OrderId == orders.Id).ToList();
                _context.OrderItems.RemoveRange(existingItems);

                for (int i = 0; i < productIds.Count; i++)
                {
                    var orderItem = new OrderItem
                    {
                        ProductId = productIds[i],
                        OrderId = orders.Id,
                        Quantity = quantities[i]
                    };
                    _context.OrderItems.Add(orderItem);
                }

                await _context.SaveChangesAsync();

                // Make sure to set the products for the ViewData
                ViewData["Products"] = new SelectList(_context.Products, "Id", "Name");

                return RedirectToAction(nameof(Index));
            }

            // Ensure ViewData["Products"] is set before returning the view
            ViewData["Products"] = new SelectList(_context.Products, "Id", "Name");
            return View(orders);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Orders.Any(o => o.Id == orders.Id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing order.");
            TempData["ErrorMessage"] = "Failed to edit the order.";
            return RedirectToAction(nameof(Index));
        }
    }



    [HttpGet]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var order = await _context.Orders.Include(o => o.OrderItems)
                                             .ThenInclude(oi => oi.Product)
                                             .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading order for deletion.");
            TempData["ErrorMessage"] = "Failed to load the order for deletion.";
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
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Order deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting order.");
            TempData["ErrorMessage"] = "Failed to delete the order.";
            return RedirectToAction(nameof(Index));
        }
    }
}