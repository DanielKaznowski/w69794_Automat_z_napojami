using System.Collections.Generic;
using System.Threading.Tasks;
using VendingMachineApp.Models;

namespace VendingMachineApp.Interfaces
{
    public interface IProductService
    {
        Task AddProductAsync(string name, decimal price, int quantity);

        Task<List<Product>> GetAllProductsAsync();

        Task UpdateProductAsync(int productId, decimal price, int quantity);

        Task DeleteProductAsync(int productId);

        Task BuyProductAsync(int productId, decimal insertedAmount);

        Task<List<Transaction>> GetAllTransactionsAsync();

        Task ShowTransactionsAsync();
    }
}
