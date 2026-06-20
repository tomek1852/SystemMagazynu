using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SystemMagazynu.Data;
using SystemMagazynu.Models;
using SystemMagazynu.Services;
using SystemMagazynu.ViewModels;

namespace SystemMagazynu.Controllers
{
    [Authorize]
    public class WarehouseStocksController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWarehouseStockService _stockService;
        private readonly IChangeHistoryService _history;
        private readonly UserManager<ApplicationUser> _userManager;

        public WarehouseStocksController(ApplicationDbContext db,
                                          IWarehouseStockService stockService,
                                          IChangeHistoryService history,
                                          UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _stockService = stockService;
            _history = history;
            _userManager = userManager;
        }

        // GET: /WarehouseStocks
        public async Task<IActionResult> Index(int? warehouseId, int? productId, bool? lowStock, string? sort)
        {
            var query = _db.WarehouseStocks
                .Include(s => s.Product)
                    .ThenInclude(p => p!.Category)
                .Include(s => s.Warehouse)
                .AsQueryable();

            if (warehouseId.HasValue)
                query = query.Where(s => s.WarehouseId == warehouseId);

            if (productId.HasValue)
                query = query.Where(s => s.ProductId == productId);

            if (lowStock == true)
                query = query.Where(s => s.Quantity <= s.Product!.MinimumStock);

            query = sort switch
            {
                "qty_asc" => query.OrderBy(s => s.Quantity),
                "qty_desc" => query.OrderByDescending(s => s.Quantity),
                "product" => query.OrderBy(s => s.Product!.Name),
                _ => query.OrderBy(s => s.Warehouse!.Name).ThenBy(s => s.Product!.Name)
            };

            var stocks = await query.ToListAsync();

            // Mini-dashboard
            ViewBag.TotalPositions = stocks.Count;
            ViewBag.LowStockCount = stocks.Count(s => s.Product != null && s.Quantity <= s.Product.MinimumStock);
            ViewBag.TotalQuantity = stocks.Sum(s => s.Quantity);

            ViewBag.Warehouses = new SelectList(await _db.Warehouse.OrderBy(w => w.Name).ToListAsync(), "Id", "Name", warehouseId);
            ViewBag.Products = new SelectList(await _db.Products.OrderBy(p => p.Name).ToListAsync(), "Id", "Name", productId);
            ViewBag.WarehouseId = warehouseId;
            ViewBag.ProductId = productId;
            ViewBag.LowStock = lowStock;
            ViewBag.Sort = sort;

            return View(stocks);
        }

        // GET: /WarehouseStocks/Correct?productId=1&warehouseId=2
        [Authorize(Roles = "Administrator,KierownikMagazynu")]
        public async Task<IActionResult> Correct(int productId, int warehouseId)
        {
            var stock = await _db.WarehouseStocks
                .Include(s => s.Product)
                .Include(s => s.Warehouse)
                .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == warehouseId);

            if (stock == null)
                return NotFound();

            var model = new StockCorrectionViewModel
            {
                ProductId = stock.ProductId,
                WarehouseId = stock.WarehouseId,
                ProductName = stock.Product?.Name ?? string.Empty,
                WarehouseName = stock.Warehouse?.Name ?? string.Empty,
                CurrentQuantity = stock.Quantity,
                NewQuantity = stock.Quantity
            };

            return View(model);
        }

        // POST: /WarehouseStocks/Correct
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,KierownikMagazynu")]
        public async Task<IActionResult> Correct(StockCorrectionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await RehydrateDisplayAsync(model);
                return View(model);
            }

            var stock = await _stockService.GetStockAsync(model.ProductId, model.WarehouseId);
            if (stock == null)
                return NotFound();

            var userId = _userManager.GetUserId(User)!;
            int oldQuantity = stock.Quantity;

            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // CorrectStockAsync ustawia nowy stan, dopisuje ruch typu Correction i sprawdza alerty.
                await _stockService.CorrectStockAsync(
                    model.ProductId,
                    model.WarehouseId,
                    model.NewQuantity,
                    userId,
                    model.Notes);

                await _history.LogAsync("WarehouseStocks", stock.Id, "Korekta stanu",
                    new { Quantity = oldQuantity },
                    new { Quantity = model.NewQuantity, model.Notes },
                    userId);

                await transaction.CommitAsync();

                TempData["Success"] = $"Stan został skorygowany: {oldQuantity} → {model.NewQuantity}.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Wystąpił błąd podczas korekty stanu. Zmiany zostały wycofane.");
                await RehydrateDisplayAsync(model);
                return View(model);
            }
        }

        // Uzupełnia pola wyświetlane po nieudanej walidacji
        private async Task RehydrateDisplayAsync(StockCorrectionViewModel model)
        {
            var stock = await _db.WarehouseStocks
                .Include(s => s.Product)
                .Include(s => s.Warehouse)
                .FirstOrDefaultAsync(s => s.ProductId == model.ProductId && s.WarehouseId == model.WarehouseId);

            if (stock != null)
            {
                model.ProductName = stock.Product?.Name ?? string.Empty;
                model.WarehouseName = stock.Warehouse?.Name ?? string.Empty;
                model.CurrentQuantity = stock.Quantity;
            }
        }
    }
}
