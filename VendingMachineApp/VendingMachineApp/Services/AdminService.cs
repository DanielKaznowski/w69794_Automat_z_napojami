using VendingMachineApp.Interfaces;
using VendingMachineApp.Models;
using VendingMachineApp.Services;
using System;
using System.Threading.Tasks;

namespace VendingMachineApp.Services
{
    public class AdminService : IAdminService
    {
        private readonly ITransactionService _transactionService;
        private readonly ProductService _productService;
        private readonly VendingMachineState _vendingMachineState;

        public AdminService(ITransactionService transactionService, ProductService productService, VendingMachineState vendingMachineState)
        {
            _transactionService = transactionService;
            _productService = productService;
            _vendingMachineState = vendingMachineState;
        }

        public async Task ShowTransactionsAsync()
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();
            foreach (var transaction in transactions)
            {
                Console.WriteLine($"Transaction ID: {transaction.Id}, Product ID: {transaction.ProductId}, Price: {transaction.Price:C}, Date: {transaction.TransactionDate}");
            }
        }

        public async Task AddNewProductAsync(string name, decimal price, int quantity)
        {
            await _productService.AddProductAsync(name, price, quantity);
            Console.WriteLine("Produkt został dodany.");
        }

        public async Task EditProductAsync(int productId, decimal price, int quantity)
        {
            await _productService.UpdateProductAsync(productId, price, quantity);
            Console.WriteLine("Produkt został zaktualizowany.");
        }

        public async Task DeleteProductAsync(int productId)
        {
            await _productService.DeleteProductAsync(productId);
            Console.WriteLine("Produkt został usunięty.");
        }

        public async Task ShowBalanceAsync()
        {
            await Task.CompletedTask;
            Console.WriteLine($"Saldo automatu: {_vendingMachineState.GetBalance:C}");
        }

        public async Task WithdrawMoneyAsync(decimal amount)
        {
            try
            {
                await Task.CompletedTask;
                _vendingMachineState.WithdrawMoney(amount);
                Console.WriteLine($"Wypłacono {amount:C} z automatu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd przy wypłacie: {ex.Message}");
            }
        }
    }



}

