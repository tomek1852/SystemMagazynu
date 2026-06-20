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
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IChangeHistoryService _history;
        private readonly UserManager<ApplicationUser> _userManager;

        public CategoriesController(ApplicationDbContext db,
                                     IChangeHistoryService history,
                                     UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _history = history;
            _userManager = userManager;
        }

        // GET: /Categories
        public async Task<IActionResult> Index(string? searchName)
        {
            var query = _db.Categories
                .Include(c => c.Products)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchName))
                query = query.Where(c => c.Name.Contains(searchName));

            var categories = await query.OrderBy(c => c.Name).ToListAsync();

            ViewBag.SearchName = searchName;

            return View(categories);
        }

        // GET: /Categories/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var category = await _db.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        // GET: /Categories/Create
        [Authorize(Roles = "Administrator,Magazynier,KierownikMagazynu")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Magazynier,KierownikMagazynu")]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (await _db.Categories.AnyAsync(c => c.Name == model.Name))
                ModelState.AddModelError("Name", "Kategoria o takiej nazwie już istnieje.");

            if (!ModelState.IsValid)
                return View(model);

            var category = new Category
            {
                Name = model.Name,
                Description = model.Description
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            var userId = _userManager.GetUserId(User)!;
            await _history.LogAsync("Categories", category.Id, "Dodanie", null, category, userId);

            TempData["Success"] = "Kategoria została dodana.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Categories/Edit/5
        [Authorize(Roles = "Administrator,Magazynier,KierownikMagazynu")]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            var model = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            return View(model);
        }

        // POST: /Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Magazynier,KierownikMagazynu")]
        public async Task<IActionResult> Edit(int id, CategoryViewModel model)
        {
            if (await _db.Categories.AnyAsync(c => c.Name == model.Name && c.Id != id))
                ModelState.AddModelError("Name", "Kategoria o takiej nazwie już istnieje.");

            if (!ModelState.IsValid)
                return View(model);

            var category = await _db.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            var oldCategory = new
            {
                category.Name,
                category.Description
            };

            category.Name = model.Name;
            category.Description = model.Description;

            await _db.SaveChangesAsync();

            var userId = _userManager.GetUserId(User)!;
            await _history.LogAsync("Categories", category.Id, "Edycja", oldCategory, category, userId);

            TempData["Success"] = "Kategoria została zaktualizowana.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Categories/Delete/5
        [Authorize(Roles = "Administrator,KierownikMagazynu")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _db.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: /Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,KierownikMagazynu")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            var hasProducts = await _db.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
            {
                TempData["Error"] = "Nie można usunąć kategorii, do której przypisane są produkty.";
                return RedirectToAction(nameof(Index));
            }

            var userId = _userManager.GetUserId(User)!;
            await _history.LogAsync("Categories", category.Id, "Usunięcie", category, null, userId);

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Kategoria została usunięta.";
            return RedirectToAction(nameof(Index));
        }
    }
}
