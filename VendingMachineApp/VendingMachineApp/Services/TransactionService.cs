using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendingMachineApp.Data;
using VendingMachineApp.Models;
using VendingMachineApp.Interfaces;


namespace VendingMachineApp.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly VendingMachineDbContext _context;

        public TransactionService(VendingMachineDbContext context)
        {
            _context = context;
        }

        // Pobranie wszystkich transakcji
        public async Task<List<Transaction>> GetAllTransactionsAsync()
        {
            return await _context.Transactions.Include(t => t.Product).ToListAsync();
        }

        // Dodanie nowej transakcji
        public async Task AddTransactionAsync(int productId, decimal price)
        {
            var transaction = new Transaction
            {
                ProductId = productId,
                Price = price,
                TransactionDate = DateTime.Now
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        // Wyświetlanie transakcji w konsoli
        public async Task ShowTransactionsAsync()
        {
            var transactions = await GetAllTransactionsAsync();
            if (transactions.Any())
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
