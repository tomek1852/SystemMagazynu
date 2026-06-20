using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SystemMagazynu.Data;
using SystemMagazynu.Models;

namespace SystemMagazynu.Controllers
{
    [Authorize(Roles = "Administrator,KierownikMagazynu")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ReportsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Reports
        public IActionResult Index()
        {
            return View();
        }

        // ---------- RAPORT: STANY MAGAZYNOWE ----------

        private IQueryable<WarehouseStock> StockQuery(int? warehouseId, bool? lowStock)
        {
            var query = _db.WarehouseStocks
                .Include(s => s.Product)
                    .ThenInclude(p => p!.Category)
                .Include(s => s.Warehouse)
                .AsQueryable();

            if (warehouseId.HasValue)
                query = query.Where(s => s.WarehouseId == warehouseId);

            if (lowStock == true)
                query = query.Where(s => s.Quantity <= s.Product!.MinimumStock);

            return query.OrderBy(s => s.Warehouse!.Name).ThenBy(s => s.Product!.Name);
        }

        public async Task<IActionResult> Stock(int? warehouseId, bool? lowStock)
        {
            var data = await StockQuery(warehouseId, lowStock).ToListAsync();

            ViewBag.Warehouses = new SelectList(await _db.Warehouse.OrderBy(w => w.Name).ToListAsync(), "Id", "Name", warehouseId);
            ViewBag.WarehouseId = warehouseId;
            ViewBag.LowStock = lowStock;

            return View(data);
        }

        public async Task<IActionResult> StockCsv(int? warehouseId, bool? lowStock)
        {
            var data = await StockQuery(warehouseId, lowStock).ToListAsync();

            var headers = new[] { "Magazyn", "Produkt", "Kod katalogowy", "Kategoria", "Ilość", "Min. stan", "Poniżej minimum" };
            var rows = data.Select(s => new[]
            {
                s.Warehouse?.Name ?? "",
                s.Product?.Name ?? "",
                s.Product?.CatalogNumber ?? "",
                s.Product?.Category?.Name ?? "",
                s.Quantity.ToString(),
                (s.Product?.MinimumStock ?? 0).ToString(),
                (s.Product != null && s.Quantity <= s.Product.MinimumStock) ? "TAK" : "NIE"
            });

            return CsvFile(headers, rows, "raport_stany");
        }

        // ---------- RAPORT: RUCHY MAGAZYNOWE ----------

        private IQueryable<WarehouseMovement> MovementsQuery(DateTime? from, DateTime? to,
            int? warehouseId, int? productId, MovementType? type)
        {
            var query = _db.WarehouseMovements
                .Include(m => m.Product)
                .Include(m => m.Warehouse)
                .Include(m => m.User)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(m => m.MovementDate >= from.Value);

            if (to.HasValue)
            {
                var toEnd = to.Value.Date.AddDays(1);
                query = query.Where(m => m.MovementDate < toEnd);
            }

            if (warehouseId.HasValue)
                query = query.Where(m => m.WarehouseId == warehouseId);

            if (productId.HasValue)
                query = query.Where(m => m.ProductId == productId);

            if (type.HasValue)
                query = query.Where(m => m.MovementType == type);

            return query.OrderByDescending(m => m.MovementDate);
        }

        public async Task<IActionResult> Movements(DateTime? from, DateTime? to,
            int? warehouseId, int? productId, MovementType? type)
        {
            // Domyślnie ostatnie 30 dni
            from ??= DateTime.Today.AddDays(-30);
            to ??= DateTime.Today;

            var data = await MovementsQuery(from, to, warehouseId, productId, type).ToListAsync();

            ViewBag.Warehouses = new SelectList(await _db.Warehouse.OrderBy(w => w.Name).ToListAsync(), "Id", "Name", warehouseId);
            ViewBag.Products = new SelectList(await _db.Products.OrderBy(p => p.Name).ToListAsync(), "Id", "Name", productId);
            ViewBag.From = from.Value.ToString("yyyy-MM-dd");
            ViewBag.To = to.Value.ToString("yyyy-MM-dd");
            ViewBag.WarehouseId = warehouseId;
            ViewBag.ProductId = productId;
            ViewBag.Type = type;

            return View(data);
        }

        public async Task<IActionResult> MovementsCsv(DateTime? from, DateTime? to,
            int? warehouseId, int? productId, MovementType? type)
        {
            var data = await MovementsQuery(from, to, warehouseId, productId, type).ToListAsync();

            var headers = new[] { "Data", "Typ", "Produkt", "Magazyn", "Ilość", "Dokument", "Uwagi", "Użytkownik" };
            var rows = data.Select(m => new[]
            {
                m.MovementDate.ToString("yyyy-MM-dd HH:mm"),
                MovementTypeLabel(m.MovementType),
                m.Product?.Name ?? "",
                m.Warehouse?.Name ?? "",
                m.Quantity.ToString(),
                m.SourceDocument ?? "",
                m.Notes ?? "",
                m.User?.FullName ?? ""
            });

            return CsvFile(headers, rows, "raport_ruchy");
        }

        // ---------- RAPORT: DOSTAWY ----------

        private IQueryable<Delivery> DeliveriesQuery(DateTime? from, DateTime? to,
            int? supplierId, int? warehouseId)
        {
            var query = _db.Deliveries
                .Include(d => d.Supplier)
                .Include(d => d.Warehouse)
                .Include(d => d.DeliveryItems)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(d => d.DeliveryDate >= from.Value);

            if (to.HasValue)
            {
                var toEnd = to.Value.Date.AddDays(1);
                query = query.Where(d => d.DeliveryDate < toEnd);
            }

            if (supplierId.HasValue)
                query = query.Where(d => d.SupplierId == supplierId);

            if (warehouseId.HasValue)
                query = query.Where(d => d.WarehouseId == warehouseId);

            return query.OrderByDescending(d => d.DeliveryDate);
        }

        public async Task<IActionResult> Deliveries(DateTime? from, DateTime? to,
            int? supplierId, int? warehouseId)
        {
            from ??= DateTime.Today.AddDays(-30);
            to ??= DateTime.Today;

            var data = await DeliveriesQuery(from, to, supplierId, warehouseId).ToListAsync();

            ViewBag.Suppliers = new SelectList(await _db.Suppliers.OrderBy(s => s.Name).ToListAsync(), "Id", "Name", supplierId);
            ViewBag.Warehouses = new SelectList(await _db.Warehouse.OrderBy(w => w.Name).ToListAsync(), "Id", "Name", warehouseId);
            ViewBag.From = from.Value.ToString("yyyy-MM-dd");
            ViewBag.To = to.Value.ToString("yyyy-MM-dd");
            ViewBag.SupplierId = supplierId;
            ViewBag.WarehouseId = warehouseId;

            return View(data);
        }

        public async Task<IActionResult> DeliveriesCsv(DateTime? from, DateTime? to,
            int? supplierId, int? warehouseId)
        {
            var data = await DeliveriesQuery(from, to, supplierId, warehouseId).ToListAsync();

            var headers = new[] { "Numer dostawy", "Data", "Dostawca", "Magazyn", "Liczba pozycji", "Wartość" };
            var rows = data.Select(d => new[]
            {
                d.DeliveryNumber,
                d.DeliveryDate.ToString("yyyy-MM-dd"),
                d.Supplier?.Name ?? "",
                d.Warehouse?.Name ?? "",
                d.DeliveryItems.Count.ToString(),
                d.DeliveryItems.Sum(i => i.Quantity * i.UnitPrice).ToString("0.00")
            });

            return CsvFile(headers, rows, "raport_dostawy");
        }

        // ---------- POMOCNICZE ----------

        private static string MovementTypeLabel(MovementType type) => type switch
        {
            MovementType.Receipt => "Przyjęcie",
            MovementType.Issue => "Wydanie",
            MovementType.Correction => "Korekta",
            _ => type.ToString()
        };

        private FileContentResult CsvFile(string[] headers, IEnumerable<string[]> rows, string baseName)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(";", headers.Select(EscapeCsv)));

            foreach (var row in rows)
                sb.AppendLine(string.Join(";", row.Select(EscapeCsv)));

            // BOM UTF-8, aby Excel poprawnie odczytał polskie znaki
            var preamble = Encoding.UTF8.GetPreamble();
            var body = Encoding.UTF8.GetBytes(sb.ToString());
            var bytes = preamble.Concat(body).ToArray();

            var fileName = $"{baseName}_{DateTime.Now:yyyyMMdd_HHmm}.csv";
            return File(bytes, "text/csv", fileName);
        }

        private static string EscapeCsv(string value)
        {
            value ??= string.Empty;
            if (value.Contains(';') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }
    }
}
