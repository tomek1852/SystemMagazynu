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
    public class DeliveriesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWarehouseStockService _stockService;
        private readonly IChangeHistoryService _history;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeliveriesController(ApplicationDbContext db,
                                     IWarehouseStockService stockService,
                                     IChangeHistoryService history,
                                     UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _stockService = stockService;
            _history = history;
            _userManager = userManager;
        }

        // GET: /Deliveries
        public async Task<IActionResult> Index(int? supplierId, int? warehouseId)
        {
            var query = _db.Deliveries
                .Include(d => d.Supplier)
                .Include(d => d.Warehouse)
                .Include(d => d.DeliveryItems)
                .AsQueryable();

            if (supplierId.HasValue)
                query = query.Where(d => d.SupplierId == supplierId);

            if (warehouseId.HasValue)
                query = query.Where(d => d.WarehouseId == warehouseId);

            var deliveries = await query
                .OrderByDescending(d => d.DeliveryDate)
                .ToListAsync();

            ViewBag.Suppliers = new SelectList(await _db.Suppliers.OrderBy(s => s.Name).ToListAsync(), "Id", "Name", supplierId);
            ViewBag.Warehouses = new SelectList(await _db.Warehouse.OrderBy(w => w.Name).ToListAsync(), "Id", "Name", warehouseId);
            ViewBag.SupplierId = supplierId;
            ViewBag.WarehouseId = warehouseId;

            return View(deliveries);
        }

        // GET: /Deliveries/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var delivery = await _db.Deliveries
                .Include(d => d.Supplier)
                .Include(d => d.Warehouse)
                .Include(d => d.User)
                .Include(d => d.DeliveryItems)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (delivery == null)
                return NotFound();

            return View(delivery);
        }

        // GET: /Deliveries/Create
        [Authorize(Roles = "Administrator,Magazynier,KierownikMagazynu,PracownikZakupow")]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();

            var model = new DeliveryViewModel
            {
                DeliveryDate = DateTime.Today,
                DeliveryNumber = await GenerateDeliveryNumberAsync(),
                Items = new List<DeliveryItemViewModel> { new DeliveryItemViewModel() }
            };

            return View(model);
        }

        // POST: /Deliveries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Magazynier,KierownikMagazynu,PracownikZakupow")]
        public async Task<IActionResult> Create(DeliveryViewModel model)
        {
            // Odrzuć puste wiersze (np. niewypełniony szablon)
            model.Items = model.Items
                .Where(i => i.ProductId > 0)
                .ToList();

            if (!model.Items.Any())
                ModelState.AddModelError(string.Empty, "Dostawa musi zawierać przynajmniej jedną pozycję.");

            if (await _db.Deliveries.AnyAsync(d => d.DeliveryNumber == model.DeliveryNumber))
                ModelState.AddModelError("DeliveryNumber", "Dostawa o takim numerze już istnieje.");

            // Walidacja istnienia powiązań
            if (!await _db.Suppliers.AnyAsync(s => s.Id == model.SupplierId))
                ModelState.AddModelError("SupplierId", "Wybrany dostawca nie istnieje.");

            if (!await _db.Warehouse.AnyAsync(w => w.Id == model.WarehouseId))
                ModelState.AddModelError("WarehouseId", "Wybrany magazyn nie istnieje.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(model);
            }

            var userId = _userManager.GetUserId(User)!;

            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var delivery = new Delivery
                {
                    SupplierId = model.SupplierId,
                    WarehouseId = model.WarehouseId,
                    DeliveryNumber = model.DeliveryNumber,
                    DeliveryDate = model.DeliveryDate,
                    Notes = model.Notes,
                    UserId = userId,
                    DeliveryItems = model.Items.Select(i => new DeliveryItem
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList()
                };

                _db.Deliveries.Add(delivery);
                await _db.SaveChangesAsync();

                // Automatyczne zwiększenie stanów magazynowych dla każdej pozycji.
                // IncreaseStockAsync dopisuje ruch magazynowy (Receipt) i sprawdza alerty.
                foreach (var item in delivery.DeliveryItems)
                {
                    await _stockService.IncreaseStockAsync(
                        item.ProductId,
                        delivery.WarehouseId,
                        item.Quantity,
                        userId,
                        delivery.DeliveryNumber);
                }

                await _history.LogAsync("Deliveries", delivery.Id, "Dodanie", null, new
                {
                    delivery.DeliveryNumber,
                    delivery.SupplierId,
                    delivery.WarehouseId,
                    delivery.DeliveryDate,
                    ItemCount = delivery.DeliveryItems.Count
                }, userId);

                await transaction.CommitAsync();

                TempData["Success"] = "Dostawa została zarejestrowana, a stany magazynowe zostały zaktualizowane.";
                return RedirectToAction(nameof(Details), new { id = delivery.Id });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Wystąpił błąd podczas zapisywania dostawy. Zmiany zostały wycofane.");
                await PopulateDropdownsAsync();
                return View(model);
            }
        }

        private async Task PopulateDropdownsAsync()
        {
            ViewBag.Suppliers = new SelectList(
                await _db.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync(),
                "Id", "Name");

            ViewBag.Warehouses = new SelectList(
                await _db.Warehouse.Where(w => w.IsActive).OrderBy(w => w.Name).ToListAsync(),
                "Id", "Name");

            ViewBag.Products = new SelectList(
                await _db.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync(),
                "Id", "Name");
        }

        private async Task<string> GenerateDeliveryNumberAsync()
        {
            var year = DateTime.Today.Year;
            var countThisYear = await _db.Deliveries
                .CountAsync(d => d.DeliveryDate.Year == year);

            return $"DOS/{year}/{(countThisYear + 1):D4}";
        }
    }
}
