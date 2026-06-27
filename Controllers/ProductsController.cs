using System.Linq;
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
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IChangeHistoryService _history;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductsController(ApplicationDbContext db,
                                   IChangeHistoryService history,
                                   UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _history = history;
            _userManager = userManager;
        }

        // GET: /Products
        // Przy żądaniu AJAX (nagłówek X-Requested-With) zwraca tylko tabelę (partial),
        // dzięki czemu wyszukiwanie i stronicowanie działają bez przeładowania strony.
        public async Task<IActionResult> Index(string? searchName, string? searchCatalog, int? categoryId, int page = 1, int pageSize = 5)
        {
            var allowedPageSizes = new[] { 5, 10, 25, 50 };
            if (!allowedPageSizes.Contains(pageSize))
                pageSize = 5;

            var query = _db.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchName))
                query = query.Where(p => p.Name.Contains(searchName));

            if (!string.IsNullOrEmpty(searchCatalog))
                query = query.Where(p => p.CatalogNumber.Contains(searchCatalog));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (page < 1) page = 1;
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var items = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new PagedResult<Product>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            ViewBag.Categories = new SelectList(await _db.Categories.ToListAsync(), "Id", "Name", categoryId);
            ViewBag.SearchName = searchName;
            ViewBag.SearchCatalog = searchCatalog;
            ViewBag.CategoryId = categoryId;
            ViewBag.PageSize = pageSize;

            // Żądanie AJAX -> tylko tabela
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_ProductsTable", model);

            return View(model);
        }

        // GET: /Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .Include(p => p.WarehouseStocks)
                    .ThenInclude(s => s.Warehouse)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // GET: /Products/Create
        [Authorize(Roles = "Administrator,Magazynier,KierownikMagazynu")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _db.Categories.ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: /Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Magazynier,KierownikMagazynu")]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (await _db.Products.AnyAsync(p => p.CatalogNumber == model.CatalogNumber))
                ModelState.AddModelError("CatalogNumber", "Produkt z takim kodem katalogowym już istnieje.");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(await _db.Categories.ToListAsync(), "Id", "Name");
                return View(model);
            }

            var product = new Product
            {
                Name = model.Name,
                CategoryId = model.CategoryId,
                CatalogNumber = model.CatalogNumber,
                Description = model.Description,
                MinimumStock = model.MinimumStock,
                IsActive = true
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            var userId = _userManager.GetUserId(User)!;
            await _history.LogAsync("Products", product.Id, "Dodanie", null, product, userId);

            TempData["Success"] = "Produkt został dodany.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Products/Edit/5
        [Authorize(Roles = "Administrator,Magazynier,KierownikMagazynu")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            var model = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                CategoryId = product.CategoryId,
                CatalogNumber = product.CatalogNumber,
                Description = product.Description,
                MinimumStock = product.MinimumStock,
                IsActive = product.IsActive
            };

            ViewBag.Categories = new SelectList(await _db.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
            return View(model);
        }

        // POST: /Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Magazynier,KierownikMagazynu")]
        public async Task<IActionResult> Edit(int id, ProductViewModel model)
        {
            if (await _db.Products.AnyAsync(p => p.CatalogNumber == model.CatalogNumber && p.Id != id))
                ModelState.AddModelError("CatalogNumber", "Produkt z takim kodem katalogowym już istnieje.");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(await _db.Categories.ToListAsync(), "Id", "Name");
                return View(model);
            }

            var product = await _db.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            var oldProduct = new
            {
                product.Name,
                product.CategoryId,
                product.CatalogNumber,
                product.Description,
                product.MinimumStock,
                product.IsActive
            };

            product.Name = model.Name;
            product.CategoryId = model.CategoryId;
            product.CatalogNumber = model.CatalogNumber;
            product.Description = model.Description;
            product.MinimumStock = model.MinimumStock;
            product.IsActive = model.IsActive;

            await _db.SaveChangesAsync();

            var userId = _userManager.GetUserId(User)!;
            await _history.LogAsync("Products", product.Id, "Edycja", oldProduct, product, userId);

            TempData["Success"] = "Produkt został zaktualizowany.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Products/Delete/5
        [Authorize(Roles = "Administrator,KierownikMagazynu")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: /Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,KierownikMagazynu")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            var hasStock = await _db.WarehouseStocks.AnyAsync(s => s.ProductId == id && s.Quantity > 0);
            if (hasStock)
            {
                TempData["Error"] = "Nie można usunąć produktu który ma stan magazynowy większy od zera.";
                return RedirectToAction(nameof(Index));
            }

            var userId = _userManager.GetUserId(User)!;
            await _history.LogAsync("Products", product.Id, "Usunięcie", product, null, userId);

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Produkt został usunięty.";
            return RedirectToAction(nameof(Index));
        }
    }
}
