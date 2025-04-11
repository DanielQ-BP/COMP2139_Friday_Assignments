using Comp2139_Assignment1.Areas.InventoryManagement.Models;
using Comp2139_Assignment1.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Comp2139_Assignment1.Areas.InventoryManagement.Controllers
{
    [Area("InventoryManagement")]
    [Route("[area]/[controller]/[action]")]
    public class OrdersController : Controller
    {
        private readonly InventoryDBContext _context;

        public OrdersController(InventoryDBContext context)
        {
            _context = context;
        }

        // Index: List orders
        public  IActionResult Index()
        {
            var orders =  _context.Orders.ToList();
            return View(orders);
        }

        // Create: Show form to create a new order
        public async Task<IActionResult> Create()
        {
            // Fetch categories and products for dropdowns
            var categories = await _context.Categories.ToListAsync();
            var products = await _context.Products.ToListAsync();

            if (!categories.Any() || !products.Any())  // Ensure both lists have data
            {
                ModelState.AddModelError("", "No categories or products available.");
                ViewData["Categories"] = new List<SelectListItem>();
                ViewData["Products"] = new List<SelectListItem>();
                return View(new Orders());
            }

            // Convert to SelectList for dropdowns
            ViewData["Categories"] = new SelectList(categories, "Id", "Name");
            ViewData["Products"] = new SelectList(products, "Id", "Name");

            return View(new Orders());
        }
        

        // Create: Process the form submission to create an order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Orders orders, List<int> productIds, List<int> quantities)
        {
            if (productIds.Count != quantities.Count)
            {
                ModelState.AddModelError("", "Products and quantities must match.");
                return View(orders);
            }

            if (ModelState.IsValid)
            {
                // Convert OrderDate to UTC before saving
                orders.OrderDate = DateTime.UtcNow;

                // Save the order
                _context.Orders.Add(orders);
                await _context.SaveChangesAsync();

                // Add order items
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

                // Redirect to the order confirmation page
                return RedirectToAction("OrderConfirmation", new { id = orders.Id });
            }

            // If the model state is invalid, reload the form with existing data
            ViewData["Products"] = new SelectList(_context.Products, "Id", "Name");
            return View(orders);
        }
        
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Calculate the total price
            decimal totalPrice = order.OrderItems.Sum(oi => oi.Product.Price * oi.Quantity);
            ViewData["TotalPrice"] = totalPrice;

            return View(order);
        }

        // Edit: Show form to edit an existing order
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _context.Orders.Include(o => o.OrderItems)
                                              .ThenInclude(oi => oi.Product)
                                              .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            ViewData["Products"] = new SelectList(_context.Products, "Id", "Name");
            return View(order);
        }

        // Edit: Process the form submission to update an order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Orders orders, List<int> productIds, List<int> quantities)
        {
            if (productIds.Count != quantities.Count)
            {
                ModelState.AddModelError("", "Products and quantities must match.");
                return View(orders);
            }

            if (id != orders.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure OrderDate is stored as UTC
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
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Orders.Any(o => o.Id == orders.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["Products"] = new SelectList(_context.Products, "Id", "Name");
            return View(orders);
        }

        // Delete: Show confirmation page to delete an order
        public async Task<IActionResult> Delete(int id)
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

        // Delete: Process the deletion of an order
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
