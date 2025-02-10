using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VendingMachineApp.Data;
using VendingMachineApp.Models;
using VendingMachineApp.Services;
using VendingMachineApp.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

class Program
{

    public static async Task Main(string[] args)
    {
        // Tworzenie kolekcji usług (Dependency Injection)
        var services = new ServiceCollection();

        // Konfiguracja Entity Framework z SQL Server
        services.AddDbContext<VendingMachineDbContext>(options =>
        {
            options.UseSqlServer("Server=MR_ROBOT\\SQLEXPRESS;Database=VendingMachineDb;Trusted_Connection=True;TrustServerCertificate=true");
        });

        // Rejestracja Serwisów
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IAdminService, AdminService>();

        // Rejestracja VendingMachineState w DI
        services.AddScoped<VendingMachineState>();

        // Zbudowanie kontenera usług
        var serviceProvider = services.BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
            var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();

            // Tworzymy instancję VendingMachineState
            var vendingMachineState = new VendingMachineState();

            // Menu
            bool exit = false;

            while (!exit)
            {
                Console.Clear();
                PrintHeader("Witaj w Automacie!");

                Console.WriteLine("Dostępne produkty:");
                var products = await productService.GetAllProductsAsync();
                foreach (var product in products)
                {
                    Console.WriteLine($"{product.Id} - {product.Name} - {product.Price:C} - {product.Quantity} dostępne");
                }

                Console.WriteLine("\nWpisz numer produktu do zakupu, lub wpisz 'admin', aby uzyskać dostęp do panelu administratora.");
                var input = Console.ReadLine();

                if (input.ToLower() == "admin")
                {
                    // Zapytanie o hasło
                    if (LoginAsAdmin())
                    {
                        // Jeśli hasło jest poprawne, uruchamiamy menu administratora
                        await AdminMenu(serviceProvider, productService, transactionService, vendingMachineState);
                    }
                    else
                    {
                        PrintError("Błędne hasło. Spróbuj ponownie.");
                    }
                }
                else if (int.TryParse(input, out int productId))
                {
                    var product = products.FirstOrDefault(p => p.Id == productId);

                    if (product != null && product.Quantity > 0)
                    {
                        Console.WriteLine($"Wybrałeś {product.Name}. Włóż odpowiednią ilość monet.");
                        decimal insertedAmount = 0m;

                        // Logika dodawania monet
                        while (insertedAmount < product.Price)
                        {
                            Console.WriteLine($"Włożono {insertedAmount:C}. Potrzebujesz {product.Price - insertedAmount:C} więcej.");
                            insertedAmount += decimal.Parse(Console.ReadLine());
                        }

                        try
                        {
                            // Wywołanie metody zakupu
                            await productService.BuyProductAsync(product.Id, insertedAmount);

                            // Dodanie pieniędzy do salda automatu
                            vendingMachineState.AddMoney(insertedAmount);

                            // Wyświetlenie komunikatu o sukcesie
                            PrintSuccess($"Zakupiono {product.Name}. Reszta: {insertedAmount - product.Price:C}");
                        }
                        catch (Exception ex)
                        {
                            // Wyświetlenie błędu
                            PrintError($"Błąd zakupu: {ex.Message}");
                        }
                    }
                    else
                    {
                        PrintError("Produkt niedostępny.");
                    }

                    // Pauza po zakupie produktu
                    Console.WriteLine("\nNaciśnij Enter, aby kontynuować.");
                    Console.ReadLine();
                }
                else
                {
                    PrintError("Niepoprawny wybór.");
                }
            }
        }
    }

    //Style wiadomości

    private static void PrintHeader(string message)
    {
        Console.WriteLine(new string('*', 50));
        Console.WriteLine($"* {message.PadRight(46)} *");
        Console.WriteLine(new string('*', 50));
    }

    private static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[BŁĄD] {message}");
        Console.ResetColor();
    }

    private static void PrintSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[SUKCES] {message}");
        Console.ResetColor();
    }

    private static bool LoginAsAdmin()
    {
        const string adminPassword = "admin123";
        string enteredPassword;

        int attempts = 0; // Liczba prób logowania

        while (attempts < 3) // Maksymalnie 3 próby logowania
        {
            Console.WriteLine("Podaj hasło administratora:");
            enteredPassword = Console.ReadLine();

            if (enteredPassword == adminPassword)
            {
                return true; // Zalogowano pomyślnie
            }
            else
            {
                attempts++; // Zwiększamy liczbę prób

                // Zmiana koloru tekstu na czerwony dla błędu
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Błędne hasło! Spróbuj ponownie.");
                Console.ResetColor(); // Przywrócenie domyślnego koloru
            }
        }

        // Po trzech nieudanych próbach
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Zbyt wiele nieudanych prób logowania.");
        Console.ResetColor(); // Przywrócenie domyślnego koloru

        // Program czeka na naciśnięcie dowolnego klawisza przed powrotem
        Console.WriteLine("Naciśnij dowolny klawisz, aby kontynuować...");
        Console.ReadKey(); // Czekanie na klawisz

        return false; // Zbyt wiele nieudanych prób


    }



    private static async Task AdminMenu(IServiceProvider serviceProvider, IProductService productService, ITransactionService transactionService, VendingMachineState vendingMachineState)

    {
        bool exitAdminMenu = false;

        while (!exitAdminMenu)
        {
            Console.Clear();
            PrintHeader("Panel Administratora");

            Console.WriteLine("1 - Dodaj nowy produkt");
            Console.WriteLine("2 - Edytuj produkt");
            Console.WriteLine("3 - Usuń produkt");
            Console.WriteLine("4 - Wyświetl transakcje");
            Console.WriteLine("5 - Sprawdź saldo automatu");
            Console.WriteLine("6 - Wpłać pieniądze do automatu");
            Console.WriteLine("7 - Wypłać pieniądze z automatu");
            Console.WriteLine("8 - Wyjście z panelu administratora");

            var adminChoice = Console.ReadLine();

            switch (adminChoice)
            {
                case "1":
                    await AddNewProductAsync(productService);
                    break;

                case "2":
                    await EditProductAsync(productService);
                    break;

                case "3":
                    await DeleteProductAsync(productService);
                    break;

                case "4":
                    await ShowTransactionsAsync(transactionService);
                    break;

                case "5":
                    ShowVendingMachineBalance(vendingMachineState);
                    break;

                case "6":
                    DepositMoneyToMachine(vendingMachineState);
                    break;

                case "7":
                    WithdrawMoneyFromMachine(vendingMachineState);
                    break;

                case "8":
                    exitAdminMenu = true;
                    break;

                default:
                    PrintError("Niepoprawny wybór. Spróbuj ponownie.");
                    break;
            }

            // Pauza przed kontynuowaniem
            if (!exitAdminMenu)
            {
                Console.WriteLine("\nNaciśnij Enter, aby kontynuować.");
                Console.ReadLine();
            }
        }
    }

    private static void DepositMoneyToMachine(VendingMachineState vendingMachineState)
    {
        Console.WriteLine("Podaj kwotę do wpłacenia do automatu:");
        if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
        {
            PrintError("Niepoprawny format kwoty.");
            return;
        }

        try
        {
            vendingMachineState.DepositMoney(amount);
            PrintSuccess($"Wpłacono {amount:C} do automatu.");
        }
        catch (Exception ex)
        {
            PrintError($"Błąd: {ex.Message}");
        }
    }

    private static void WithdrawMoneyFromMachine(VendingMachineState vendingMachineState)
    {
        Console.WriteLine("Aktualne saldo automatu: " + vendingMachineState.GetBalance().ToString("C"));
        Console.WriteLine("Podaj kwotę do wypłaty:");

        if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
        {
            PrintError("Niepoprawny format kwoty.");
            return;
        }

        try
        {
            vendingMachineState.WithdrawMoney(amount);
            PrintSuccess($"Wypłacono {amount:C} z automatu.");
        }
        catch (Exception ex)
        {
            PrintError($"Błąd: {ex.Message}");
        }
    }

    private static async Task ShowTransactionsAsync(ITransactionService transactionService)
    {
        var transactions = await transactionService.GetAllTransactionsAsync();

        if (transactions.Any())
        {
            foreach (var transaction in transactions)
            {
                Console.WriteLine($"Transakcja {transaction.Id} - Kwota: {transaction.Price:C} - Data: {transaction.TransactionDate}");
            }
        }
        else
        {
            PrintError("Brak transakcji.");
        }
    }

    private static void ShowVendingMachineBalance(VendingMachineState vendingMachineState)
    {
        PrintHeader($"Saldo automatu: {vendingMachineState.GetBalance():C}");
    }


    private static async Task AddNewProductAsync(IProductService productService)
    {
        Console.WriteLine("Podaj nazwę produktu:");
        string name = Console.ReadLine();

        Console.WriteLine("Podaj cenę produktu:");
        if (!decimal.TryParse(Console.ReadLine(), out decimal price))
        {
            PrintError("Niepoprawny format ceny.");
            return;
        }

        Console.WriteLine("Podaj ilość produktu:");
        if (!int.TryParse(Console.ReadLine(), out int quantity))
        {
            PrintError("Niepoprawny format ilości.");
            return;
        }

        try
        {
            await productService.AddProductAsync(name, price, quantity);
            PrintSuccess("Produkt został dodany.");
        }
        catch (Exception ex)
        {
            PrintError($"Wystąpił błąd podczas dodawania produktu: {ex.Message}");
        }
    }

    private static async Task EditProductAsync(IProductService productService)
    {
        Console.WriteLine("Lista produktów:");
        var products = await productService.GetAllProductsAsync();
        foreach (var product in products)
        {
            Console.WriteLine($"{product.Id} - {product.Name} - {product.Price:C} - {product.Quantity} dostępne");
        }

        Console.WriteLine("Podaj ID produktu do edycji:");
        if (!int.TryParse(Console.ReadLine(), out int productId))
        {
            PrintError("Niepoprawny format ID.");
            return;
        }

        Console.WriteLine("Podaj nową cenę produktu:");
        if (!decimal.TryParse(Console.ReadLine(), out decimal price))
        {
            PrintError("Niepoprawny format ceny.");
            return;
        }

        Console.WriteLine("Podaj nową ilość produktu:");
        if (!int.TryParse(Console.ReadLine(), out int quantity))
        {
            PrintError("Niepoprawny format ilości.");
            return;
        }

        try
        {
            await productService.UpdateProductAsync(productId, price, quantity);
        }
        catch (InvalidOperationException ex)
        {
            PrintError($"Wystąpił błąd: {ex.Message}");
        }
        catch (Exception ex)
        {
            PrintError($"Wystąpił nieoczekiwany błąd: {ex.Message}");
        }
    }


    private static async Task DeleteProductAsync(IProductService productService)
    {
        Console.WriteLine("Lista produktów:");
        var products = await productService.GetAllProductsAsync();
        foreach (var product in products)
        {
            Console.WriteLine($"{product.Id} - {product.Name} - {product.Price:C} - {product.Quantity} dostępne");
        }

        Console.WriteLine("Podaj ID produktu do usunięcia:");
        if (!int.TryParse(Console.ReadLine(), out int productId))
        {
            PrintError("Niepoprawny format ID.");
            return;
        }

        var productToDelete = products.FirstOrDefault(p => p.Id == productId);

        if (productToDelete == null)
        {
            // Jeśli produkt nie istnieje, wyświetlamy komunikat o błędzie
            PrintError("Produkt o podanym ID nie istnieje.");
            return;
        }

        try
        {
            // Produkt istnieje, usuwamy go
            await productService.DeleteProductAsync(productId);
            PrintSuccess("Produkt został usunięty.");
        }
        catch (Exception ex)
        {
            PrintError($"Wystąpił błąd podczas usuwania produktu: {ex.Message}");
        }
    }
}
