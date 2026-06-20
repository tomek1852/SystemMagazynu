# System Zarządzania Magazynem

Aplikacja webowa ASP.NET Core MVC wspomagająca ewidencję i kontrolę stanów magazynowych: zarządzanie produktami, magazynami, dostawcami, rejestrację dostaw i wydań, korekty stanów, alerty o niskim stanie, raporty oraz pełny dziennik zmian.

## Technologie

- **ASP.NET Core MVC** (.NET 10)
- **Entity Framework Core** (Code First, migracje)
- **MS SQL Server** (LocalDB)
- **ASP.NET Core Identity** (uwierzytelnianie, role, polityka haseł)
- **Bootstrap 5** (warstwa widoku)
- **xUnit + Moq + EF Core InMemory** (testy jednostkowe)

## Uruchomienie

1. Skonfiguruj połączenie z bazą w `appsettings.json` (`ConnectionStrings:DefaultConnection`).
2. Zastosuj migracje, które utworzą bazę:
   ```
   dotnet ef database update
   ```
3. Uruchom aplikację:
   ```
   dotnet run
   ```
4. Przy pierwszym starcie seeder tworzy role oraz konto administratora.

### Konto startowe

| Pole  | Wartość               |
|-------|-----------------------|
| Login | `admin@warehouse.pl`  |
| Hasło | `Admin1234!`          |
| Rola  | Administrator         |

Pozostałe konta (Magazynier, Kierownik magazynu, Pracownik zakupów, Użytkownik odczytu) zakłada się w panelu administratora.

## Role i uprawnienia

| Rola                  | Zakres |
|-----------------------|--------|
| **Administrator**     | Pełny dostęp, zarządzanie użytkownikami i rolami |
| **Magazynier**        | Produkty (dodawanie/edycja), dostawy, wydania, podgląd stanów |
| **KierownikMagazynu** | Magazyny, kategorie, korekty stanów, alerty, raporty, historia |
| **PracownikZakupow**  | Dostawcy, dostawy, podgląd alertów |
| **UzytkownikOdczytu** | Dostęp tylko do odczytu |

## Funkcjonalności

### Produkty i kategorie
CRUD produktów z unikalnym kodem katalogowym, przypisaniem do kategorii i minimalnym stanem. CRUD kategorii z blokadą usunięcia kategorii zawierającej produkty.

### Magazyny i dostawcy
CRUD magazynów z podglądem stanów i flagą aktywności. CRUD dostawców z walidacją NIP (format 10 cyfr + suma kontrolna), dezaktywacją oraz historią współpracy (lista dostaw).

### Dostawy
Rejestracja dostawy z wieloma pozycjami (dynamiczne wiersze). Operacja wykonywana w transakcji EF Core: zapis dostawy oraz automatyczne zwiększenie stanów magazynowych z zapisem ruchów typu *Przyjęcie*.

### Wydania
Wydanie towaru z magazynu z walidacją dostępności (podgląd dostępnej ilości na żywo). Blokada wydania ponad stan. Zapis ruchu typu *Wydanie* w transakcji.

### Stany magazynowe
Lista stanów z filtrami, sortowaniem, mini-dashboardem i podświetlaniem pozycji poniżej minimum. Korekta stanu z zapisem ruchu typu *Korekta* (różnica ilości) oraz wpisem do historii.

### Alerty
Automatyczne generowanie alertu, gdy stan spadnie do/poniżej minimum, oraz automatyczne zamykanie po uzupełnieniu. Lista z filtrami, oznaczanie jako rozwiązane i ręczne przeskanowanie wszystkich stanów.

### Raporty
Raporty stanów magazynowych, ruchów (przyjęcia/wydania/korekty w okresie) oraz dostaw, z filtrowaniem i eksportem do CSV (separator `;`, BOM UTF-8 dla poprawnych polskich znaków).

### Historia operacji
Dziennik audytowy wszystkich operacji (dodanie, edycja, usunięcie, wydanie, korekta) z wartościami przed/po w formacie JSON. Filtry (tabela, operacja, użytkownik, zakres dat) i stronicowanie. Dostęp dla Administratora i Kierownika.

### Panel administratora
Zarządzanie użytkownikami: zakładanie kont z przypisaniem roli, edycja, reset hasła, aktywacja/dezaktywacja (nieaktywne konto nie może się zalogować). Zabezpieczenie przed odebraniem uprawnień własnemu kontu.

### Pulpit
Strona główna z podsumowaniem: liczby produktów, magazynów, dostawców, aktywnych alertów, pozycji poniżej minimum, dostaw i wydań w bieżącym miesiącu oraz skróty do ostatnich dostaw i alertów.

## Architektura

Wzorzec **MVC** z wydzieloną warstwą serwisów dla logiki biznesowej:

- `WarehouseStockService` — zwiększanie/zmniejszanie/korekta stanu, zapis ruchów, wyzwalanie alertów.
- `StockAlertService` — tworzenie i zamykanie alertów.
- `ChangeHistoryService` — zapis dziennika zmian (serializacja JSON).

Operacje zmieniające stany (dostawy, wydania, korekty) wykonywane są w transakcjach EF Core, co gwarantuje spójność danych przy błędzie.

### Struktura katalogów

```
SystemMagazynu/
├── Controllers/      # Kontrolery MVC
├── Models/           # Encje domenowe
├── ViewModels/       # Modele widoków z walidacją
├── Views/            # Widoki Razor
├── Services/         # Logika biznesowa
├── Data/             # DbContext, migracje, seeder
└── wwwroot/          # Zasoby statyczne

SystemMagazynu.Tests/ # Testy jednostkowe (xUnit)
```

## Testy

Projekt `SystemMagazynu.Tests` zawiera testy jednostkowe warstwy serwisów na bazie InMemory (każdy test na izolowanej bazie):

- **WarehouseStockServiceTests** — przyjęcia, wydania (w tym blokada poniżej stanu), korekty, wyzwalanie alertów.
- **StockAlertServiceTests** — tworzenie, brak duplikatów, automatyczne zamykanie alertów.
- **ChangeHistoryServiceTests** — poprawny zapis wartości przed/po.

Uruchomienie:
```
dotnet test
```

## Bezpieczeństwo

- Uwierzytelnianie cookie z Identity, polityka haseł (min. 8 znaków, cyfra, mała i wielka litera).
- Blokada konta po 5 nieudanych próbach na 15 minut.
- Autoryzacja oparta na rolach na poziomie akcji kontrolerów.
- Ochrona CSRF (token antyforgery przy operacjach POST).
- Walidacja danych po stronie modelu i logiki biznesowej.
