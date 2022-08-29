using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace GeekShopping.OderAPI.Model.Context
{
    public class MySQLContext : DbContext
    {
        public MySQLContext(DbContextOptions<MySQLContext> options) : base(options) { }

        public DbSet<OrderDetail> Details { get; set; }
        public DbSet<OrderHeader> Headers { get; set; }
    }
}
