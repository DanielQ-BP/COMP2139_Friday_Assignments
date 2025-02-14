using Comp2139_Assignment1.Data;
using Comp2139_Assignment1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Comp2139_Assignment1.Controllers
{
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
            var categories = await _context.Categories.ToListAsync(); // Fetch categories
            var products = await _context.Products.ToListAsync(); // Fetch products

            if (!categories.Any())
            {
                ModelState.AddModelError("", "No categories available.");
                ViewData["Categories"] = new List<Category>();
                ViewData["Products"] = new List<Product>();
                return View(new Orders());
            }

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
                return RedirectToAction(nameof(Index));
            }

            ViewData["Products"] = new SelectList(_context.Products, "Id", "Name");
            return View(orders);
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
