using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using TwsSharpApp.Data;

namespace TwsSharpApp.Migrations
{
    [DbContext(typeof(DB_ModelContainer))]
    partial class DB_ModelContainerModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.6");

            modelBuilder.Entity("TwsSharpApp.Data.ContractData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Currency");

                    b.Property<string>("Exchange");

                    b.Property<string>("PrimaryExch");

                    b.Property<string>("SecType");

                    b.Property<string>("Symbol");

                    b.HasKey("Id");

                    b.ToTable("DisplayedContracts");
                });
        }
    }
}
