using VendingMachineApp.Data;
using VendingMachineApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VendingMachineApp.Interfaces;

namespace VendingMachineApp.Services
{
    public class ProductService : IProductService
    {
        private readonly VendingMachineDbContext _context;

        public ProductService(VendingMachineDbContext context)
        {
            _context = context;
        }

        // Dodanie nowego produktu
        public async Task AddProductAsync(string name, decimal price, int quantity)
        {
            var product = new Product
            {
                Name = name,
                Price = price,
                Quantity = quantity
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            Console.WriteLine("Produkt został dodany.");
        }

        // Pobranie wszystkich produktów
        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _context.Products.ToListAsync();
        }

        // Edytowanie produktu
        public async Task UpdateProductAsync(int productId, decimal price, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                throw new InvalidOperationException("Produkt o podanym ID nie istnieje.");
            }

            product.Price = price;
            product.Quantity = quantity;
            await _context.SaveChangesAsync();
            Console.WriteLine("Produkt zaktualizowany.");
        }

        // Usuwanie produktu
        public async Task DeleteProductAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                Console.WriteLine("Produkt usunięty.");
            }
            else
            {
                Console.WriteLine("Produkt nie znaleziony.");
            }
        }

        // Obsługa zakupu napoju
        public async Task BuyProductAsync(int productId, decimal insertedAmount)
        {
            var product = await _context.Products.FindAsync(productId);

            if (product == null || product.Quantity == 0)
            {
                Console.WriteLine("Produkt niedostępny.");
                throw new InvalidOperationException("Produkt niedostępny.");
            }

            if (insertedAmount < product.Price)
            {
                Console.WriteLine("Za mało monet.");
                throw new InvalidOperationException("Za mało monet.");
            }

            // Zmniejszamy ilość produktu
            product.Quantity--;

            // Rejestrujemy transakcję
            var transaction = new Transaction
            {
                ProductId = product.Id,
                Price = product.Price,
                TransactionDate = DateTime.Now
            };

            Console.WriteLine($"Dodaję transakcję: Produkt {product.Name}, Cena: {product.Price:C}, Data: {transaction.TransactionDate}");

            _context.Transactions.Add(transaction);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd zapisu transakcji: {ex.Message}");
            }

            // Obliczamy resztę
            decimal change = insertedAmount - product.Price;
        }




        // Pobranie wszystkich transakcji
        public async Task<List<Transaction>> GetAllTransactionsAsync()
        {
            return await _context.Transactions.Include(t => t.Product).ToListAsync();
        }

        // Wyświetlanie transakcji
        public async Task ShowTransactionsAsync()
        {
            var transactions = await GetAllTransactionsAsync();
            if (transactions != null && transactions.Any())
            {
                Console.WriteLine("Historia transakcji:");
                foreach (var transaction in transactions)
                {
                    Console.WriteLine($"{transaction.TransactionDate}: {transaction.Product.Name} - {transaction.Price:C}");
                }
            }
            else
            {
                Console.WriteLine("Brak transakcji w historii.");
            }
        }
    }
}

