using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SystemMagazynu.Data;
using SystemMagazynu.Models;
using SystemMagazynu.Services;

namespace SystemMagazynu.Controllers
{
    [Authorize]
    public class StockAlertsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IStockAlertService _alertService;
        private readonly IChangeHistoryService _history;
        private readonly UserManager<ApplicationUser> _userManager;

        public StockAlertsController(ApplicationDbContext db,
                                      IStockAlertService alertService,
                                      IChangeHistoryService history,
                                      UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _alertService = alertService;
            _history = history;
            _userManager = userManager;
        }

        // GET: /StockAlerts
        public async Task<IActionResult> Index(string? status, int? warehouseId, int? productId)
        {
            // Domyślnie pokazujemy aktywne alerty
            status ??= "active";

            var query = _db.StockAlerts
                .Include(a => a.Product)
                .Include(a => a.Warehouse)
                .AsQueryable();

            if (status == "active")
                query = query.Where(a => a.Status == AlertStatus.Active);
            else if (status == "resolved")
                query = query.Where(a => a.Status == AlertStatus.Resolved);

            if (warehouseId.HasValue)
                query = query.Where(a => a.WarehouseId == warehouseId);

            if (productId.HasValue)
                query = query.Where(a => a.ProductId == productId);

            var alerts = await query
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            // Mini-dashboard (liczone niezależnie od filtra statusu)
            ViewBag.ActiveCount = await _db.StockAlerts.CountAsync(a => a.Status == AlertStatus.Active);
            ViewBag.OutOfStockCount = await _db.StockAlerts.CountAsync(a => a.Status == AlertStatus.Active && a.CurrentQuantity == 0);
            ViewBag.ResolvedCount = await _db.StockAlerts.CountAsync(a => a.Status == AlertStatus.Resolved);

            ViewBag.Warehouses = new SelectList(await _db.Warehouse.OrderBy(w => w.Name).ToListAsync(), "Id", "Name", warehouseId);
            ViewBag.Products = new SelectList(await _db.Products.OrderBy(p => p.Name).ToListAsync(), "Id", "Name", productId);
            ViewBag.Status = status;
            ViewBag.WarehouseId = warehouseId;
            ViewBag.ProductId = productId;

            return View(alerts);
        }

        // POST: /StockAlerts/Resolve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,KierownikMagazynu")]
        public async Task<IActionResult> Resolve(int id)
        {
            var alert = await _db.StockAlerts.FindAsync(id);
            if (alert == null)
                return NotFound();

            await _alertService.ResolveAlertAsync(id);

            var userId = _userManager.GetUserId(User)!;
            await _history.LogAsync("StockAlerts", id, "Rozwiązanie alertu", null, new
            {
                alert.ProductId,
                alert.WarehouseId,
                alert.CurrentQuantity
            }, userId);

            TempData["Success"] = "Alert został oznaczony jako rozwiązany.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /StockAlerts/Rescan
        // Ręczne przeskanowanie wszystkich stanów i aktualizacja alertów.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,KierownikMagazynu")]
        public async Task<IActionResult> Rescan()
        {
            var stockKeys = await _db.WarehouseStocks
                .Select(s => new { s.ProductId, s.WarehouseId })
                .ToListAsync();

            foreach (var key in stockKeys)
            {
                await _alertService.CheckAndCreateAlertsAsync(key.ProductId, key.WarehouseId);
            }

            TempData["Success"] = $"Przeskanowano {stockKeys.Count} pozycji magazynowych.";
            return RedirectToAction(nameof(Index));
        }
    }
}
