using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using TwsSharpApp.Data;

namespace TwsSharpApp.Migrations
{
    [DbContext(typeof(DB_ModelContainer))]
    [Migration("20191110071127_Added Settings table")]
    partial class AddedSettingstable
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
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

            modelBuilder.Entity("TwsSharpApp.Data.Setting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Key");

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.ToTable("Settings");
                });
        }
    }
}
