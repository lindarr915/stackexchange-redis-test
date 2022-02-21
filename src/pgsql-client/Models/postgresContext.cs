using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace pgsql_client.Models
{

    public class Product
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public int QuantityInStock { get; set; }
        public int Price { get; set; }
    }
    public partial class postgresContext : DbContext
    {
        private static string? Host = System.Environment.GetEnvironmentVariable("DB_HOST") ?? "database-1.cluster-cyeqog6cufmf.us-west-2.rds.amazonaws.com";
        private static string? User = System.Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
        private static string? DBname = "postgres";
        private static string? Password = System.Environment.GetEnvironmentVariable("DB_PASSWORD");
        private static string? Port = "5432";

        public DbSet<Product> Products { get; set; }
        public postgresContext()
        {

        }

        public postgresContext(DbContextOptions<postgresContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connString = String.Format(
                "Server={0};Username={1};Database={2};Port={3};Password={4};SSLMode=Prefer",
                Host,
                User,
                DBname,
                Port,
                Password);

            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(connString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
