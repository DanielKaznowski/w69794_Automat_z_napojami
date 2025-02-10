using VendingMachineApp.Interfaces;

namespace VendingMachineApp.Models
{
    public class VendingMachineState
    {
        private decimal _totalMoneyInMachine;

        //Saldo automatu
        public decimal GetBalance()
        {
            return _totalMoneyInMachine;
        }
        //Dodawanie pieniędzy do salda
        public void AddMoney(decimal amount)
        {
            _totalMoneyInMachine += amount;
        }

        //Wyciągnij pieniądze
        public void WithdrawMoney(decimal amount)
        {
            if (_totalMoneyInMachine < amount)
            {
                throw new InvalidOperationException("Wybrana kwota jest większa niż saldo automatu.");
            }
            _totalMoneyInMachine -= amount;
        }
        //Wpłać pieniądze
        public void DepositMoney(decimal amount)
        {
            _totalMoneyInMachine += amount;
        }
    }
}
