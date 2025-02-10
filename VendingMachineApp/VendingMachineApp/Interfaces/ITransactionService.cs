using System.Collections.Generic;
using System.Threading.Tasks;
using VendingMachineApp.Models;

namespace VendingMachineApp.Interfaces
{
    public interface ITransactionService
    {
        Task<List<Transaction>> GetAllTransactionsAsync();
        Task AddTransactionAsync(int productId, decimal price);
        Task ShowTransactionsAsync();
    }
}
