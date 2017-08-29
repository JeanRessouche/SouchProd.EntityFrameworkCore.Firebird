using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ScaffoldingConsoleApp.Entities
{
    public partial class SampleDbContext : DbContext
    {
        public virtual DbSet<Sampletable1> Sampletable1 { get; set; }
        public virtual DbSet<Sampletable2> Sampletable2 { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseFirebird("User=SYSDBA;Password=masterkey;Database=D:/ScaffoldTestDb.fdb;DataSource=127.0.0.1;Port=3050;Dialect=3;Charset=UTF8;Role=;Connection lifetime=15;Pooling=true;MinPoolSize=0;MaxPoolSize=50;Packet Size=8192;ServerType=0;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Sampletable1>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();
            });

            modelBuilder.Entity<Sampletable2>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Fieldbigint).HasDefaultValueSql("DEFAULT 42");

                entity.Property(e => e.Typetimestamp).HasDefaultValueSql("DEFAULT CURRENT_TIMESTAMP ");
            });
        }
    }
}
