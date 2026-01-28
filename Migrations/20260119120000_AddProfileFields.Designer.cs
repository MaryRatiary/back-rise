using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Rise.API.Data;

namespace Rise.API.Migrations
{
    [DbContext(typeof(RiseDbContext))]
    [Migration("20260119120000_AddProfileFields")]
    partial class AddProfileFields
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity("Rise.API.Models.User", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("TEXT");

                b.Property<string>("FirstName")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<string>("LastName")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<string>("Email")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<string>("PasswordHash")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<string>("MatriculeNumber")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<string>("Filiere")
                    .HasColumnType("TEXT");

                b.Property<string>("Classe")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<string>("Role")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("TEXT");

                b.Property<DateTime>("UpdatedAt")
                    .HasColumnType("TEXT");

                b.Property<bool>("IsActive")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER")
                    .HasDefaultValue(true);

                b.Property<string>("ProfileImageUrl")
                    .HasColumnType("TEXT");

                b.Property<string>("CoverImageUrl")
                    .HasColumnType("TEXT");

                b.Property<string>("JobTitle")
                    .HasColumnType("TEXT");

                b.Property<string>("Company")
                    .HasColumnType("TEXT");

                b.Property<string>("Location")
                    .HasColumnType("TEXT");

                b.Property<string>("Phone")
                    .HasColumnType("TEXT");

                b.Property<string>("Bio")
                    .HasColumnType("TEXT");

                b.Property<string>("Specialization")
                    .HasColumnType("TEXT");

                b.Property<string>("InterestCategories")
                    .HasColumnType("TEXT");

                b.Property<string>("Associations")
                    .HasColumnType("TEXT");

                b.Property<string>("SharedExpertise")
                    .HasColumnType("TEXT");

                b.Property<string>("Languages")
                    .HasColumnType("TEXT");

                b.Property<string>("LinkedinUrl")
                    .HasColumnType("TEXT");

                b.Property<string>("InstagramUrl")
                    .HasColumnType("TEXT");

                b.Property<string>("TwitterUrl")
                    .HasColumnType("TEXT");

                b.Property<string>("GithubUrl")
                    .HasColumnType("TEXT");

                b.Property<string>("QrCodeUrl")
                    .HasColumnType("TEXT");

                b.Property<string>("NotificationPreferences")
                    .HasColumnType("TEXT");

                b.Property<string>("ProfileVisibility")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("TEXT")
                    .HasDefaultValue("public");

                b.Property<int>("EventsJoined")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER")
                    .HasDefaultValue(0);

                b.Property<int>("EventsAttended")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER")
                    .HasDefaultValue(0);

                b.Property<string>("Badges")
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.ToTable("Users");
            });

#pragma warning restore 612, 618
        }
    }
}
