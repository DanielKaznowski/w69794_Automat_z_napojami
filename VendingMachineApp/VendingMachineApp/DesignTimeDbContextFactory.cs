using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using VendingMachineApp.Data;

namespace VendingMachineApp
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<VendingMachineDbContext>
    {
        public VendingMachineDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<VendingMachineDbContext>();
            optionsBuilder.UseSqlServer("Server=MR_ROBOT\\SQLEXPRESS;Database=VendingMachineDb;Trusted_Connection=True;TrustServerCertificate=true");

            return new VendingMachineDbContext(optionsBuilder.Options);
        }
    }
}
