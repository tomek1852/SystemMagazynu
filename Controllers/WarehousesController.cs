using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemMagazynu.Data;
using SystemMagazynu.Models;
using SystemMagazynu.Services;
using SystemMagazynu.ViewModels;

namespace SystemMagazynu.Controllers
{
    [Authorize]
    public class WarehousesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IChangeHistoryService _history;
        private readonly UserManager<ApplicationUser> _userManager;

        public WarehousesController(ApplicationDbContext db,
                                     IChangeHistoryService history,
                                     UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _history = history;
            _userManager = userManager;
        }

        // GET: /Warehouses
        public async Task<IActionResult> Index(string? searchName, string? searchCity)
        {
            var query = _db.Warehouse.AsQueryable();

            if (!string.IsNullOrEmpty(searchName))
                query = query.Where(w => w.Name.Contains(searchName));

            if (!string.IsNullOrEmpty(searchCity))
                query = query.Where(w => w.City.Contains(searchCity));

            var warehouses = await query.OrderBy(w => w.Name).ToListAsync();

            ViewBag.SearchName = searchName;
            ViewBag.SearchCity = searchCity;

            return View(warehouses);
        }

        // GET: /Warehouses/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var warehouse = await _db.Warehouse
                .Include(w => w.WarehouseStocks)
                    .ThenInclude(s => s.Product)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (warehouse == null)
                return NotFound();

            return View(warehouse);
        }

        // GET: /Warehouses/Create
        [Authorize(Roles = "Administrator,KierownikMagazynu")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Warehouses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,KierownikMagazynu")]
        public async Task<IActionResult> Create(WarehouseViewModel model)
        {
            if (await _db.Warehouse.AnyAsync(w => w.Name == model.Name))
                ModelState.AddModelError("Name", "Magazyn o takiej nazwie już istnieje.");

            if (!ModelState.IsValid)
                return View(model);

            var warehouse = new Warehouse
            {
                Name = model.Name,
                Street = model.Street,
                BuildingNumber = model.BuildingNumber,
                PostalCode = model.PostalCode,
                City = model.City,
                Country = model.Country,
                Description = model.Description,
                IsActive = true
            };

            _db.Warehouse.Add(warehouse);
            await _db.SaveChangesAsync();

            var userId = _userManager.GetUserId(User)!;
            await _history.LogAsync("Warehouses", warehouse.Id, "Dodanie", null, warehouse, userId);

            TempData["Success"] = "Magazyn został dodany.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Warehouses/Edit/5
        [Authorize(Roles = "Administrator,KierownikMagazynu")]
        public async Task<IActionResult> Edit(int id)
        {
            var warehouse = await _db.Warehouse.FindAsync(id);
            if (warehouse == null)
                return NotFound();

            var model = new WarehouseViewModel
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Street = warehouse.Street,
                BuildingNumber = warehouse.BuildingNumber,
                PostalCode = warehouse.PostalCode,
                City = warehouse.City,
                Country = warehouse.Country,
                Description = warehouse.Description,
                IsActive = warehouse.IsActive
            };

            return View(model);
        }

        // POST: /Warehouses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,KierownikMagazynu")]
        public async Task<IActionResult> Edit(int id, WarehouseViewModel model)
        {
            if (await _db.Warehouse.AnyAsync(w => w.Name == model.Name && w.Id != id))
                ModelState.AddModelError("Name", "Magazyn o takiej nazwie już istnieje.");

            if (!ModelState.IsValid)
                return View(model);

            var warehouse = await _db.Warehouse.FindAsync(id);
            if (warehouse == null)
                return NotFound();

            var oldWarehouse = new
            {
                warehouse.Name,
                warehouse.Street,
                warehouse.BuildingNumber,
                warehouse.PostalCode,
                warehouse.City,
                warehouse.Country,
                warehouse.Description,
                warehouse.IsActive
            };

            warehouse.Name = model.Name;
            warehouse.Street = model.Street;
            warehouse.BuildingNumber = model.BuildingNumber;
            warehouse.PostalCode = model.PostalCode;
            warehouse.City = model.City;
            warehouse.Country = model.Country;
            warehouse.Description = model.Description;
            warehouse.IsActive = model.IsActive;

            await _db.SaveChangesAsync();

            var userId = _userManager.GetUserId(User)!;
            await _history.LogAsync("Warehouses", warehouse.Id, "Edycja", oldWarehouse, warehouse, userId);

            TempData["Success"] = "Magazyn został zaktualizowany.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Warehouses/Delete/5
        [Authorize(Roles = "Administrator,KierownikMagazynu")]
        public async Task<IActionResult> Delete(int id)
        {
            var warehouse = await _db.Warehouse
                .Include(w => w.WarehouseStocks)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (warehouse == null)
                return NotFound();

            return View(warehouse);
        }

        // POST: /Warehouses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,KierownikMagazynu")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var warehouse = await _db.Warehouse.FindAsync(id);
            if (warehouse == null)
                return NotFound();

            // Blokada usunięcia gdy istnieją powiązane rekordy (klucze obce z DeleteBehavior.Restrict)
            bool hasStocks = await _db.WarehouseStocks.AnyAsync(s => s.WarehouseId == id);
            bool hasDeliveries = await _db.Deliveries.AnyAsync(d => d.WarehouseId == id);
            bool hasMovements = await _db.WarehouseMovements.AnyAsync(m => m.WarehouseId == id);

            if (hasStocks || hasDeliveries || hasMovements)
            {
                TempData["Error"] = "Nie można usunąć magazynu, który ma powiązane stany, dostawy lub ruchy magazynowe. " +
                                    "Zamiast tego oznacz go jako nieaktywny.";
                return RedirectToAction(nameof(Index));
            }

            var userId = _userManager.GetUserId(User)!;
            await _history.LogAsync("Warehouses", warehouse.Id, "Usunięcie", warehouse, null, userId);

            _db.Warehouse.Remove(warehouse);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Magazyn został usunięty.";
            return RedirectToAction(nameof(Index));
        }
    }
}
