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
    public class WarehouseIssuesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWarehouseStockService _stockService;
        private readonly IChangeHistoryService _history;
        private readonly UserManager<ApplicationUser> _userManager;

        public WarehouseIssuesController(ApplicationDbContext db,
                                          IWarehouseStockService stockService,
                                          IChangeHistoryService history,
                                          UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _stockService = stockService;
            _history = history;
            _userManager = userManager;
        }

        // GET: /WarehouseIssues
        // Historia wydań = ruchy magazynowe typu Issue
        public async Task<IActionResult> Index(int? warehouseId, int? productId)
        {
            var query = _db.WarehouseMovements
                .Include(m => m.Product)
                .Include(m => m.Warehouse)
                .Include(m => m.User)
                .Where(m => m.MovementType == MovementType.Issue)
                .AsQueryable();

            if (warehouseId.HasValue)
                query = query.Where(m => m.WarehouseId == warehouseId);

            if (productId.HasValue)
                query = query.Where(m => m.ProductId == productId);

            var movements = await query
                .OrderByDescending(m => m.MovementDate)
                .ToListAsync();

            ViewBag.Warehouses = new SelectList(await _db.Warehouse.OrderBy(w => w.Name).ToListAsync(), "Id", "Name", warehouseId);
            ViewBag.Products = new SelectList(await _db.Products.OrderBy(p => p.Name).ToListAsync(), "Id", "Name", productId);
            ViewBag.WarehouseId = warehouseId;
            ViewBag.ProductId = productId;

            return View(movements);
        }

        // GET: /WarehouseIssues/Create
        [Authorize(Roles = "Administrator,Magazynier,KierownikMagazynu")]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View(new IssueViewModel());
        }

        // POST: /WarehouseIssues/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Magazynier,KierownikMagazynu")]
        public async Task<IActionResult> Create(IssueViewModel model)
        {
            if (!await _db.Warehouse.AnyAsync(w => w.Id == model.WarehouseId))
                ModelState.AddModelError("WarehouseId", "Wybrany magazyn nie istnieje.");

            if (!await _db.Products.AnyAsync(p => p.Id == model.ProductId))
                ModelState.AddModelError("ProductId", "Wybrany produkt nie istnieje.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(model);
            }

            // Wczesne sprawdzenie dostępności (czytelny komunikat dla użytkownika)
            var stock = await _stockService.GetStockAsync(model.ProductId, model.WarehouseId);
            int available = stock?.Quantity ?? 0;

            if (available < model.Quantity)
            {
                ModelState.AddModelError(string.Empty,
                    $"Niewystarczający stan magazynowy. Dostępne: {available}, żądane: {model.Quantity}.");
                await PopulateDropdownsAsync();
                return View(model);
            }

            var userId = _userManager.GetUserId(User)!;

            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // DecreaseStockAsync zmniejsza stan, dopisuje ruch typu Issue i sprawdza alerty.
                // Zwraca false, jeśli stan jest niewystarczający (podwójne zabezpieczenie).
                bool success = await _stockService.DecreaseStockAsync(
                    model.ProductId,
                    model.WarehouseId,
                    model.Quantity,
                    userId,
                    model.SourceDocument,
                    model.Notes);

                if (!success)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "Niewystarczający stan magazynowy do wykonania wydania.");
                    await PopulateDropdownsAsync();
                    return View(model);
                }

                await _history.LogAsync("WarehouseMovements", model.ProductId, "Wydanie", null, new
                {
                    model.WarehouseId,
                    model.ProductId,
                    model.Quantity,
                    model.SourceDocument,
                    model.Notes
                }, userId);

                await transaction.CommitAsync();

                TempData["Success"] = "Wydanie zostało zarejestrowane, a stan magazynowy zaktualizowany.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Wystąpił błąd podczas zapisywania wydania. Zmiany zostały wycofane.");
                await PopulateDropdownsAsync();
                return View(model);
            }
        }

        // GET: /WarehouseIssues/GetStock?productId=1&warehouseId=2
        // Zwraca aktualnie dostępną ilość (do podpowiedzi w formularzu)
        [HttpGet]
        public async Task<IActionResult> GetStock(int productId, int warehouseId)
        {
            var stock = await _stockService.GetStockAsync(productId, warehouseId);
            return Json(new { quantity = stock?.Quantity ?? 0 });
        }

        private async Task PopulateDropdownsAsync()
        {
            ViewBag.Warehouses = new SelectList(
                await _db.Warehouse.Where(w => w.IsActive).OrderBy(w => w.Name).ToListAsync(),
                "Id", "Name");

            ViewBag.Products = new SelectList(
                await _db.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync(),
                "Id", "Name");
        }
    }
}
