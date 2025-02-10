using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendingMachineApp.Interfaces
{
    public interface IAdminService
    {
        Task ShowTransactionsAsync();
        Task AddNewProductAsync(string name, decimal price, int quantity);
        Task EditProductAsync(int productId, decimal newPrice, int newQuantity);
        Task DeleteProductAsync(int productId);
        Task ShowBalanceAsync();
        Task WithdrawMoneyAsync(decimal amount);
    }

}
