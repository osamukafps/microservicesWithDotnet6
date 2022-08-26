﻿using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace GeekShopping.CartAPI.Model.Context
{
    public class MySQLContext : DbContext
    {
        public MySQLContext() { }

        public MySQLContext(DbContextOptions<MySQLContext> options) : base(options) {  }

        public DbSet<Product> Products { get; set; }       
    }
}