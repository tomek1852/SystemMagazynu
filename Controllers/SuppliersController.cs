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
    public class SuppliersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IChangeHistoryService _history;
        private readonly UserManager<ApplicationUser> _userManager;

        public SuppliersController(ApplicationDbContext db,
                                    IChangeHistoryService history,
                                    UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _history = history;
            _userManager = userManager;
        }

        // GET: /Suppliers
        public async Task<IActionResult> Index(string? searchName, bool? onlyActive)
        {
            var query = _db.Suppliers.AsQueryable();

            if (!string.IsNullOrEmpty(searchName))
                query = query.Where(s => s.Name.Contains(searchName));

            if (onlyActive == true)
                query = query.Where(s => s.IsActive);

            var suppliers = await query.OrderBy(s => s.Name).ToListAsync();

            ViewBag.SearchName = searchName;
            ViewBag.OnlyActive = onlyActive;

            return View(suppliers);
        }

        // GET: /Suppliers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var supplier = await _db.Suppliers
                .Include(s => s.Deliveries)
                    .ThenInclude(d => d.Warehouse)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (supplier == null)
                return NotFound();

            return View(supplier);
        }

        // GET: /Suppliers/Create
        [Authorize(Roles = "Administrator,PracownikZakupow")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Suppliers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,PracownikZakupow")]
        public async Task<IActionResult> Create(SupplierViewModel model)
        {
            await ValidateSupplierAsync(model, null);

            if (!ModelState.IsValid)
                return View(model);

            var supplier = new Supplier
            {
                Name = model.Name,
                NIP = model.NIP,
                Email = model.Email,
                Phone = model.Phone,
                Street = model.Street,
                BuildingNumber = model.BuildingNumber,
                PostalCode = model.PostalCode,
                City = model.City,
                Country = model.Country,
                IsActive = true
            };

            _db.Suppliers.Add(supplier);
            await _db.SaveChangesAsync();

            var userId = _userManager.GetUserId(User)!;
            await _history.LogAsync("Suppliers", supplier.Id, "Dodanie", null, supplier, userId);

            TempData["Success"] = "Dostawca został dodany.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Suppliers/Edit/5
        [Authorize(Roles = "Administrator,PracownikZakupow")]
        public async Task<IActionResult> Edit(int id)
        {
            var supplier = await _db.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound();

            var model = new SupplierViewModel
            {
                Id = supplier.Id,
                Name = supplier.Name,
                NIP = supplier.NIP,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Street = supplier.Street,
                BuildingNumber = supplier.BuildingNumber,
                PostalCode = supplier.PostalCode,
                City = supplier.City,
                Country = supplier.Country,
                IsActive = supplier.IsActive
            };

            return View(model);
        }

        // POST: /Suppliers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,PracownikZakupow")]
        public async Task<IActionResult> Edit(int id, SupplierViewModel model)
        {
            await ValidateSupplierAsync(model, id);

            if (!ModelState.IsValid)
                return View(model);

            var supplier = await _db.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound();

            var oldSupplier = new
            {
                supplier.Name,
                supplier.NIP,
                supplier.Email,
                supplier.Phone,
                supplier.Street,
                supplier.BuildingNumber,
                supplier.PostalCode,
                supplier.City,
                supplier.Country,
                supplier.IsActive
            };

            supplier.Name = model.Name;
            supplier.NIP = model.NIP;
            supplier.Email = model.Email;
            supplier.Phone = model.Phone;
            supplier.Street = model.Street;
            supplier.BuildingNumber = model.BuildingNumber;
            supplier.PostalCode = model.PostalCode;
            supplier.City = model.City;
            supplier.Country = model.Country;
            supplier.IsActive = model.IsActive;

            await _db.SaveChangesAsync();

            var userId = _userManager.GetUserId(User)!;
            await _history.LogAsync("Suppliers", supplier.Id, "Edycja", oldSupplier, supplier, userId);

            TempData["Success"] = "Dane dostawcy zostały zaktualizowane.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Suppliers/Deactivate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,PracownikZakupow")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var supplier = await _db.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound();

            supplier.IsActive = false;
            await _db.SaveChangesAsync();

            var userId = _userManager.GetUserId(User)!;
            await _history.LogAsync("Suppliers", supplier.Id, "Dezaktywacja", null, supplier, userId);

            TempData["Success"] = "Dostawca został oznaczony jako nieaktywny.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Suppliers/Delete/5
        [Authorize(Roles = "Administrator,PracownikZakupow")]
        public async Task<IActionResult> Delete(int id)
        {
            var supplier = await _db.Suppliers
                .Include(s => s.Deliveries)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (supplier == null)
                return NotFound();

            return View(supplier);
        }

        // POST: /Suppliers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,PracownikZakupow")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var supplier = await _db.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound();

            var hasDeliveries = await _db.Deliveries.AnyAsync(d => d.SupplierId == id);
            if (hasDeliveries)
            {
                TempData["Error"] = "Nie można usunąć dostawcy, który ma powiązane dostawy. " +
                                    "Zamiast tego oznacz go jako nieaktywnego.";
                return RedirectToAction(nameof(Index));
            }

            var userId = _userManager.GetUserId(User)!;
            await _history.LogAsync("Suppliers", supplier.Id, "Usunięcie", supplier, null, userId);

            _db.Suppliers.Remove(supplier);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Dostawca został usunięty.";
            return RedirectToAction(nameof(Index));
        }

        // Walidacja biznesowa: unikalność i suma kontrolna NIP
        private async Task ValidateSupplierAsync(SupplierViewModel model, int? currentId)
        {
            if (!string.IsNullOrEmpty(model.NIP))
            {
                if (!IsValidNip(model.NIP))
                {
                    ModelState.AddModelError("NIP", "Podany NIP jest nieprawidłowy (błędna suma kontrolna).");
                }
                else
                {
                    bool nipExists = await _db.Suppliers
                        .AnyAsync(s => s.NIP == model.NIP && (currentId == null || s.Id != currentId));

                    if (nipExists)
                        ModelState.AddModelError("NIP", "Dostawca z takim numerem NIP już istnieje.");
                }
            }
        }

        // Walidacja sumy kontrolnej polskiego NIP (10 cyfr)
        private static bool IsValidNip(string nip)
        {
            if (nip.Length != 10 || !nip.All(char.IsDigit))
                return false;

            int[] weights = { 6, 5, 7, 2, 3, 4, 5, 6, 7 };
            int sum = 0;

            for (int i = 0; i < 9; i++)
                sum += (nip[i] - '0') * weights[i];

            int checkDigit = sum % 11;

            if (checkDigit == 10)
                return false;

            return checkDigit == (nip[9] - '0');
        }
    }
}
