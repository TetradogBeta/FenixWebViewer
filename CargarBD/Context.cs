using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CheckFenix.CargarBD
{
    public class Context:DbContext
    {
        public static string NameFile = "SeriesFenix.db";
        static bool Created = System.IO.File.Exists(NameFile);
        public Context():base() {

            if (!Created)
            {
                Created = true;
                Database.EnsureDeleted();
                Database.EnsureCreated();
            }
        }

        public DbSet<SerieBD> Series { get; set; }
        public DbSet<Update> Updates { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={NameFile};");
        }
    }
}
