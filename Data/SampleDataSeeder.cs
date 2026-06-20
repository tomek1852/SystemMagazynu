using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SystemMagazynu.Models;
using SystemMagazynu.Services;

namespace SystemMagazynu.Data
{
    // Wypełnia bazę przykładowymi danymi do prezentacji.
    // Uruchamia się tylko gdy baza jest pusta (brak kategorii).
    public static class SampleDataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var db = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var stockService = services.GetRequiredService<IWarehouseStockService>();

            // Jeśli są już jakieś kategorie, zakładamy że dane przykładowe istnieją.
            if (await db.Categories.AnyAsync())
                return;

            // ---------- UŻYTKOWNICY ----------
            async Task<ApplicationUser> EnsureUserAsync(string email, string first, string last,
                                                        string password, string role)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FirstName = first,
                        LastName = last,
                        EmailConfirmed = true,
                        IsActive = true
                    };
                    var result = await userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                        await userManager.AddToRoleAsync(user, role);
                }
                return user;
            }

            var kierownik = await EnsureUserAsync("kierownik@warehouse.pl", "Jan", "Kowalski", "Kierownik1234!", "KierownikMagazynu");
            var magazynier = await EnsureUserAsync("magazynier@warehouse.pl", "Anna", "Nowak", "Magazyn1234!", "Magazynier");
            await EnsureUserAsync("zakupy@warehouse.pl", "Piotr", "Wiśniewski", "Zakupy1234!", "PracownikZakupow");
            await EnsureUserAsync("odczyt@warehouse.pl", "Maria", "Wójcik", "Odczyt1234!", "UzytkownikOdczytu");

            var operatorId = magazynier.Id;

            // ---------- KATEGORIE ----------
            var catNarzedzia = new Category { Name = "Narzędzia ręczne", Description = "Narzędzia obsługiwane ręcznie" };
            var catElektro = new Category { Name = "Elektronarzędzia", Description = "Narzędzia zasilane elektrycznie" };
            var catBudowlane = new Category { Name = "Materiały budowlane", Description = "Materiały do prac budowlanych" };
            var catChemia = new Category { Name = "Chemia budowlana", Description = "Farby, kleje, zaprawy" };

            db.Categories.AddRange(catNarzedzia, catElektro, catBudowlane, catChemia);

            // ---------- MAGAZYNY ----------
            var whGlowny = new Warehouse
            {
                Name = "Magazyn Główny",
                Street = "Przemysłowa",
                BuildingNumber = "12",
                PostalCode = "30-001",
                City = "Kraków",
                Country = "Polska",
                Description = "Magazyn centralny",
                IsActive = true
            };
            var whPolnoc = new Warehouse
            {
                Name = "Magazyn Północ",
                Street = "Portowa",
                BuildingNumber = "5",
                PostalCode = "80-001",
                City = "Gdańsk",
                Country = "Polska",
                Description = "Magazyn regionalny",
                IsActive = true
            };

            db.Warehouse.AddRange(whGlowny, whPolnoc);

            // ---------- DOSTAWCY ----------
            var supBudMax = new Supplier
            {
                Name = "BudMax Sp. z o.o.",
                NIP = "5270206151",
                Email = "kontakt@budmax.pl",
                Phone = "221234567",
                Street = "Magazynowa",
                BuildingNumber = "8",
                PostalCode = "00-001",
                City = "Warszawa",
                Country = "Polska",
                IsActive = true
            };
            var supElektro = new Supplier
            {
                Name = "ElektroHurt S.A.",
                NIP = "1182006137",
                Email = "biuro@elektrohurt.pl",
                Phone = "126543210",
                Street = "Hurtowa",
                BuildingNumber = "20",
                PostalCode = "31-001",
                City = "Kraków",
                Country = "Polska",
                IsActive = true
            };
            var supChemPol = new Supplier
            {
                Name = "ChemPol",
                NIP = "5250000883",
                Email = "zamowienia@chempol.pl",
                Phone = "583334455",
                Street = "Chemiczna",
                BuildingNumber = "3",
                PostalCode = "80-002",
                City = "Gdańsk",
                Country = "Polska",
                IsActive = true
            };

            db.Suppliers.AddRange(supBudMax, supElektro, supChemPol);

            // Zapis, aby kategorie/magazyny/dostawcy dostały Id
            await db.SaveChangesAsync();

            // ---------- PRODUKTY ----------
            var pMlotek = new Product { Name = "Młotek ślusarski 500g", CatalogNumber = "KAT-001", CategoryId = catNarzedzia.Id, MinimumStock = 10, IsActive = true };
            var pWkretarka = new Product { Name = "Wkrętarka akumulatorowa 18V", CatalogNumber = "KAT-002", CategoryId = catElektro.Id, MinimumStock = 5, IsActive = true };
            var pWiertarka = new Product { Name = "Wiertarka udarowa 750W", CatalogNumber = "KAT-003", CategoryId = catElektro.Id, MinimumStock = 4, IsActive = true };
            var pPustak = new Product { Name = "Pustak ceramiczny", CatalogNumber = "KAT-004", CategoryId = catBudowlane.Id, MinimumStock = 100, IsActive = true };
            var pFarba = new Product { Name = "Farba akrylowa biała 10L", CatalogNumber = "KAT-005", CategoryId = catChemia.Id, MinimumStock = 8, IsActive = true };
            var pKlej = new Product { Name = "Klej montażowy", CatalogNumber = "KAT-006", CategoryId = catChemia.Id, MinimumStock = 15, IsActive = true };

            db.Products.AddRange(pMlotek, pWkretarka, pWiertarka, pPustak, pFarba, pKlej);
            await db.SaveChangesAsync();

            // ---------- DOSTAWY + PRZYJĘCIA NA STAN ----------
            async Task RegisterDeliveryAsync(Supplier supplier, Warehouse warehouse, string number,
                                             (Product product, int qty, decimal price)[] items)
            {
                var delivery = new Delivery
                {
                    SupplierId = supplier.Id,
                    WarehouseId = warehouse.Id,
                    DeliveryNumber = number,
                    DeliveryDate = DateTime.Today,
                    UserId = operatorId,
                    DeliveryItems = items.Select(i => new DeliveryItem
                    {
                        ProductId = i.product.Id,
                        Quantity = i.qty,
                        UnitPrice = i.price
                    }).ToList()
                };

                db.Deliveries.Add(delivery);
                await db.SaveChangesAsync();

                foreach (var i in items)
                {
                    await stockService.IncreaseStockAsync(i.product.Id, warehouse.Id, i.qty, operatorId, number);
                }
            }

            await RegisterDeliveryAsync(supBudMax, whGlowny, "DOS/2026/0001", new[]
            {
                (pMlotek, 50, 24.99m),
                (pPustak, 500, 3.20m),
                (pFarba, 3, 89.00m),   // poniżej minimum (8) -> wygeneruje alert
                (pKlej, 50, 12.50m)
            });

            await RegisterDeliveryAsync(supElektro, whGlowny, "DOS/2026/0002", new[]
            {
                (pWkretarka, 20, 349.00m),
                (pWiertarka, 15, 279.00m)
            });

            // ---------- PRZYKŁADOWE WYDANIA ----------
            await stockService.DecreaseStockAsync(pMlotek.Id, whGlowny.Id, 5, operatorId, "WZ/2026/0001", "Wydanie na budowę A");
            await stockService.DecreaseStockAsync(pWkretarka.Id, whGlowny.Id, 18, operatorId, "WZ/2026/0002", "Wydanie na budowę B"); // zostaje 2 -> alert (min 5)
        }
    }
}
