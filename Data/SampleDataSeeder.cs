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
                        UserName = email, Email = email, FirstName = first, LastName = last,
                        EmailConfirmed = true, IsActive = true
                    };
                    var result = await userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                        await userManager.AddToRoleAsync(user, role);
                }
                return user;
            }

            await EnsureUserAsync("kierownik@warehouse.pl", "Jan", "Kowalski", "Kierownik1234!", "KierownikMagazynu");
            var magazynier = await EnsureUserAsync("magazynier@warehouse.pl", "Anna", "Nowak", "Magazyn1234!", "Magazynier");
            await EnsureUserAsync("zakupy@warehouse.pl", "Piotr", "Wiśniewski", "Zakupy1234!", "PracownikZakupow");
            await EnsureUserAsync("odczyt@warehouse.pl", "Maria", "Wójcik", "Odczyt1234!", "UzytkownikOdczytu");

            var operatorId = magazynier.Id;

            // ---------- KATEGORIE ----------
            var catNarzedzia = new Category { Name = "Narzędzia ręczne", Description = "Narzędzia obsługiwane ręcznie" };
            var catElektro = new Category { Name = "Elektronarzędzia", Description = "Narzędzia zasilane elektrycznie" };
            var catBudowlane = new Category { Name = "Materiały budowlane", Description = "Materiały do prac budowlanych" };
            var catChemia = new Category { Name = "Chemia budowlana", Description = "Kleje, pianki, grunty" };
            var catHydraulika = new Category { Name = "Hydraulika", Description = "Rury, zawory, armatura" };
            var catElektryka = new Category { Name = "Elektryka", Description = "Przewody, osprzęt elektryczny" };
            var catStolarka = new Category { Name = "Stolarka", Description = "Drewno i materiały drewnopochodne" };
            var catOgrod = new Category { Name = "Ogród", Description = "Narzędzia i akcesoria ogrodowe" };
            var catFarby = new Category { Name = "Farby i lakiery", Description = "Farby, lakiery, emalie" };
            var catMetal = new Category { Name = "Artykuły metalowe", Description = "Śruby, gwoździe, łączniki" };
            var catSpaw = new Category { Name = "Spawalnictwo", Description = "Akcesoria spawalnicze" };
            var catBhp = new Category { Name = "BHP", Description = "Środki ochrony osobistej" };

            db.Categories.AddRange(catNarzedzia, catElektro, catBudowlane, catChemia, catHydraulika,
                catElektryka, catStolarka, catOgrod, catFarby, catMetal, catSpaw, catBhp);

            // ---------- MAGAZYNY ----------
            Warehouse Wh(string name, string street, string nr, string zip, string city) => new Warehouse
            {
                Name = name, Street = street, BuildingNumber = nr, PostalCode = zip,
                City = city, Country = "Polska", IsActive = true
            };

            var whGlowny = Wh("Magazyn Główny", "Przemysłowa", "12", "30-001", "Kraków");
            var whPolnoc = Wh("Magazyn Północ", "Portowa", "5", "80-001", "Gdańsk");
            var whPoludnie = Wh("Magazyn Południe", "Hutnicza", "18", "40-001", "Katowice");
            var whZachod = Wh("Magazyn Zachód", "Fabryczna", "7", "50-001", "Wrocław");
            var whWschod = Wh("Magazyn Wschód", "Składowa", "3", "20-001", "Lublin");
            var whCentralny = Wh("Magazyn Centralny", "Logistyczna", "22", "00-001", "Warszawa");
            var whMazowiecki = Wh("Magazyn Mazowiecki", "Spedycyjna", "9", "26-600", "Radom");
            var whPomorski = Wh("Magazyn Pomorski", "Nabrzeże", "14", "81-001", "Gdynia");

            db.Warehouse.AddRange(whGlowny, whPolnoc, whPoludnie, whZachod, whWschod,
                whCentralny, whMazowiecki, whPomorski);

            // ---------- DOSTAWCY ----------
            Supplier Sup(string name, string nip, string city) => new Supplier
            {
                Name = name, NIP = nip, Email = "biuro@" + name.Split(' ')[0].ToLower().Replace("ł", "l") + ".pl",
                Phone = "000000000", Street = "Hurtowa", BuildingNumber = "1", PostalCode = "00-001",
                City = city, Country = "Polska", IsActive = true
            };

            var suppliers = new[]
            {
                Sup("BudMax Sp. z o.o.", "5270206151", "Warszawa"),
                Sup("ElektroHurt S.A.", "1182006137", "Kraków"),
                Sup("ChemPol", "5250000883", "Gdańsk"),
                Sup("StalServ Sp. z o.o.", "6312544003", "Katowice"),
                Sup("Drewno-Trans", "9990001108", "Poznań"),
                Sup("HydroSystem", "8523214568", "Wrocław"),
                Sup("NarzedziaPro", "1133345678", "Łódź"),
                Sup("OgrodMax", "6762198340", "Lublin"),
                Sup("FarbyKolor S.A.", "5941200716", "Bydgoszcz"),
                Sup("MetalTech", "1482096311", "Częstochowa"),
                Sup("SpawMet", "7123456014", "Gliwice"),
                Sup("BHP-Serwis", "5912345607", "Szczecin"),
                Sup("InstalPol", "2468135707", "Rzeszów"),
                Sup("ProBud", "1357902468", "Białystok"),
                Sup("HurtElektro", "8020406011", "Opole")
            };
            db.Suppliers.AddRange(suppliers);
            var supBudMax = suppliers[0];
            var supElektro = suppliers[1];
            var supHydro = suppliers[5];

            await db.SaveChangesAsync();

            // ---------- PRODUKTY (generowane z list) ----------
            var catalog = new (Category cat, string[] names)[]
            {
                (catNarzedzia, new[] { "Młotek ślusarski 500g", "Śrubokręt płaski 6mm", "Śrubokręt krzyżakowy PH2",
                    "Klucz nastawny 250mm", "Szczypce uniwersalne", "Poziomica 60cm", "Miarka zwijana 5m",
                    "Nóż monterski", "Piła ręczna 450mm", "Klucz oczkowy 13mm", "Klucz płaski 17mm",
                    "Imadło ślusarskie 100mm", "Zestaw kluczy imbusowych", "Kątownik stolarski" }),

                (catElektro, new[] { "Wkrętarka akumulatorowa 18V", "Wiertarka udarowa 750W", "Szlifierka kątowa 125mm",
                    "Wyrzynarka 600W", "Młot udarowo-obrotowy SDS", "Szlifierka oscylacyjna", "Pilarka tarczowa 1200W",
                    "Strugarka elektryczna", "Opalarka 2000W", "Frezarka górnowrzecionowa", "Polerka 180mm",
                    "Klucz udarowy akumulatorowy" }),

                (catBudowlane, new[] { "Pustak ceramiczny", "Cement portlandzki 25kg", "Zaprawa murarska 25kg",
                    "Styropian EPS 100 (paczka)", "Płyta gipsowo-kartonowa", "Wełna mineralna (rolka)",
                    "Bloczek betonowy", "Cegła klinkierowa", "Gips szpachlowy 20kg", "Klej do płytek 25kg",
                    "Folia paroizolacyjna", "Siatka zbrojąca" }),

                (catChemia, new[] { "Klej montażowy", "Silikon sanitarny", "Pianka montażowa 750ml",
                    "Grunt głęboko penetrujący 5L", "Akryl malarski", "Masa szpachlowa 5kg",
                    "Środek grzybobójczy 5L", "Impregnat do drewna 5L", "Preparat antykorozyjny",
                    "Rozpuszczalnik uniwersalny 5L" }),

                (catHydraulika, new[] { "Rura PEX 16mm (zwój)", "Kolano PCV 50mm", "Zawór kulowy 1/2\"",
                    "Syfon umywalkowy", "Trójnik PCV 110mm", "Bateria umywalkowa", "Spłuczka WC",
                    "Wąż przyłączeniowy 50cm", "Taśma teflonowa", "Redukcja mosiężna", "Korek spustowy" }),

                (catElektryka, new[] { "Przewód YDYp 3x1.5 (100m)", "Gniazdko podtynkowe", "Wyłącznik nadprądowy B16",
                    "Puszka instalacyjna", "Przewód YDYp 3x2.5 (100m)", "Włącznik światła pojedynczy",
                    "Listwa zasilająca 5gn", "Taśma izolacyjna", "Oprawa LED 18W", "Rozdzielnica 12 modułów",
                    "Czujnik ruchu" }),

                (catStolarka, new[] { "Deska sosnowa 2m", "Płyta OSB 18mm", "Wkręty do drewna 4x40 (200szt)",
                    "Sklejka brzozowa 12mm", "Listwa przypodłogowa 2.5m", "Kątownik meblowy", "Zawias puszkowy",
                    "Klej do drewna 0.5kg", "Płyta MDF 16mm", "Kołek drewniany 8mm (50szt)" }),

                (catOgrod, new[] { "Sekator ogrodowy", "Wąż ogrodowy 20m", "Taczka budowlana 90L", "Grabie ogrodowe",
                    "Szpadel ogrodowy", "Konewka 10L", "Nożyce do żywopłotu", "Kosiarka spalinowa",
                    "Rękawice ogrodowe", "Opryskiwacz 5L" }),

                (catFarby, new[] { "Farba akrylowa biała 10L", "Lakier bezbarwny 2.5L", "Emalia ftalowa szara 1L",
                    "Farba elewacyjna 15L", "Podkład akrylowy 5L", "Farba do drewna 0.75L", "Spray czarny mat",
                    "Wałek malarski 25cm", "Pędzel płaski 50mm", "Taśma malarska 48mm" }),

                (catMetal, new[] { "Śruby M8 x60 (100szt)", "Gwoździe budowlane 100mm (5kg)", "Kątownik montażowy",
                    "Nakrętki M8 (100szt)", "Podkładki M8 (100szt)", "Kołek rozporowy 10mm (50szt)",
                    "Łańcuch ocynkowany 6mm", "Linka stalowa 4mm", "Uchwyt do rur 20mm", "Wkręt samowiercący (200szt)" }),

                (catSpaw, new[] { "Elektrody spawalnicze 3.2mm", "Tarcza do cięcia metalu 125mm", "Maska spawalnicza",
                    "Drut spawalniczy 0.8mm", "Rękawice spawalnicze", "Szczotka druciana" }),

                (catBhp, new[] { "Kask ochronny", "Okulary ochronne", "Rękawice robocze", "Buty robocze S3",
                    "Kamizelka odblaskowa", "Nauszniki przeciwhałasowe", "Maska przeciwpyłowa FFP2",
                    "Apteczka pierwszej pomocy" })
            };

            int seq = 1;
            var products = new List<Product>();
            foreach (var (cat, names) in catalog)
            {
                foreach (var name in names)
                {
                    products.Add(new Product
                    {
                        Name = name,
                        CategoryId = cat.Id,
                        CatalogNumber = $"KAT-{seq:D3}",
                        MinimumStock = 5 + (seq % 6) * 5,   // 5..30
                        IsActive = true
                    });
                    seq++;
                }
            }
            db.Products.AddRange(products);
            await db.SaveChangesAsync();

            Product Find(string name) => products.First(p => p.Name == name);

            // ---------- DOSTAWY + PRZYJĘCIA NA STAN ----------
            async Task RegisterDeliveryAsync(Supplier supplier, Warehouse warehouse, string number,
                                             (string product, int qty, decimal price)[] items)
            {
                var delivery = new Delivery
                {
                    SupplierId = supplier.Id, WarehouseId = warehouse.Id, DeliveryNumber = number,
                    DeliveryDate = DateTime.Today, UserId = operatorId,
                    DeliveryItems = items.Select(i => new DeliveryItem
                    {
                        ProductId = Find(i.product).Id, Quantity = i.qty, UnitPrice = i.price
                    }).ToList()
                };
                db.Deliveries.Add(delivery);
                await db.SaveChangesAsync();

                foreach (var i in items)
                    await stockService.IncreaseStockAsync(Find(i.product).Id, warehouse.Id, i.qty, operatorId, number);
            }

            await RegisterDeliveryAsync(supBudMax, whGlowny, "DOS/2026/0001", new[]
            {
                ("Młotek ślusarski 500g", 50, 24.99m),
                ("Pustak ceramiczny", 500, 3.20m),
                ("Cement portlandzki 25kg", 80, 18.50m),
                ("Farba akrylowa biała 10L", 3, 89.00m),
                ("Klej montażowy", 50, 12.50m),
                ("Gips szpachlowy 20kg", 60, 21.00m)
            });

            await RegisterDeliveryAsync(supElektro, whGlowny, "DOS/2026/0002", new[]
            {
                ("Wkrętarka akumulatorowa 18V", 20, 349.00m),
                ("Wiertarka udarowa 750W", 15, 279.00m),
                ("Szlifierka kątowa 125mm", 12, 199.00m),
                ("Wyrzynarka 600W", 10, 229.00m)
            });

            await RegisterDeliveryAsync(supHydro, whPolnoc, "DOS/2026/0003", new[]
            {
                ("Rura PEX 16mm (zwój)", 200, 4.50m),
                ("Zawór kulowy 1/2\"", 40, 15.00m),
                ("Syfon umywalkowy", 25, 22.00m),
                ("Bateria umywalkowa", 18, 149.00m)
            });

            // ---------- PRZYKŁADOWE WYDANIA ----------
            await stockService.DecreaseStockAsync(Find("Młotek ślusarski 500g").Id, whGlowny.Id, 5, operatorId, "WZ/2026/0001", "Wydanie na budowę A");
            await stockService.DecreaseStockAsync(Find("Wkrętarka akumulatorowa 18V").Id, whGlowny.Id, 18, operatorId, "WZ/2026/0002", "Wydanie na budowę B");
        }
    }
}
