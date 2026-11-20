using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Web1.Models;

namespace Web1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<Document> Documents { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Document>()
                .HasIndex(d => d.ContentHash)
                .IsUnique();
        }
    }
}