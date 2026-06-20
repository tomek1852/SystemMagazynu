using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SystemMagazynu.Data;

namespace SystemMagazynu.Controllers
{
    [Authorize(Roles = "Administrator,KierownikMagazynu")]
    public class ChangeHistoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        private const int PageSize = 25;

        public ChangeHistoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /ChangeHistory
        public async Task<IActionResult> Index(string? tableName, string? operationType,
                                               string? userId, DateTime? dateFrom, DateTime? dateTo,
                                               int page = 1)
        {
            var query = _db.ChangeHistories
                .Include(h => h.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(tableName))
                query = query.Where(h => h.TableName == tableName);

            if (!string.IsNullOrEmpty(operationType))
                query = query.Where(h => h.OperationType == operationType);

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(h => h.UserId == userId);

            if (dateFrom.HasValue)
                query = query.Where(h => h.ChangedAt >= dateFrom.Value);

            if (dateTo.HasValue)
            {
                // do końca wybranego dnia
                var to = dateTo.Value.Date.AddDays(1);
                query = query.Where(h => h.ChangedAt < to);
            }

            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
            if (page < 1) page = 1;
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var entries = await query
                .OrderByDescending(h => h.ChangedAt)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Wartości do dropdownów filtrów
            ViewBag.TableNames = new SelectList(
                await _db.ChangeHistories.Select(h => h.TableName).Distinct().OrderBy(t => t).ToListAsync(),
                tableName);
            ViewBag.OperationTypes = new SelectList(
                await _db.ChangeHistories.Select(h => h.OperationType).Distinct().OrderBy(o => o).ToListAsync(),
                operationType);
            ViewBag.Users = new SelectList(
                await _db.Users.OrderBy(u => u.Email).ToListAsync(),
                "Id", "Email", userId);

            ViewBag.TableName = tableName;
            ViewBag.OperationType = operationType;
            ViewBag.UserId = userId;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;

            return View(entries);
        }

        // GET: /ChangeHistory/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var entry = await _db.ChangeHistories
                .Include(h => h.User)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (entry == null)
                return NotFound();

            ViewBag.OldFormatted = PrettyJson(entry.OldValue);
            ViewBag.NewFormatted = PrettyJson(entry.NewValue);

            return View(entry);
        }

        // Formatuje surowy JSON do czytelnej, wciętej postaci
        private static string PrettyJson(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "—";

            try
            {
                using var doc = JsonDocument.Parse(raw);
                return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
            }
            catch
            {
                return raw;
            }
        }
    }
}
