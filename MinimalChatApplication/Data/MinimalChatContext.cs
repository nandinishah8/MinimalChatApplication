using Microsoft.EntityFrameworkCore;
using MinimalChatApplication.Models;
using System.Collections.Generic;

namespace MinimalChatApplication.Data
{
    // DataContext.cs
    public class MinimalChatContext : DbContext
    {
        public MinimalChatContext(DbContextOptions<MinimalChatContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User");
        }
        public DbSet<User> Users { get; set; }
       

    }

}
