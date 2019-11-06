using Microsoft.EntityFrameworkCore;
using System;

namespace TwsSharpApp.Data
{
    public partial class DB_ModelContainer : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=" + Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory ) + "\\TwsSharp.db");
        }

        public DB_ModelContainer() : base()
        {
        }

        public virtual DbSet<ContractData> DisplayedContracts { get; set; }
    }
}
