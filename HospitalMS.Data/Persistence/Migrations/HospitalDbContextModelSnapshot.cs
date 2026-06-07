using System;
using HospitalMS.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace HospitalMS.Data.Persistence.Migrations;

[DbContext(typeof(HospitalDbContext))]
partial class HospitalDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity("HospitalMS.Data.Persistence.Entities.Patient", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid");

            b.Property<DateTime>("CreatedAtUtc")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTime>("DateOfBirth")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("Email")
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnType("character varying(256)");

            b.Property<string>("FirstName")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("character varying(100)");

            b.Property<string>("LastName")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("character varying(100)");

            b.Property<string>("MedicalRecordNumber")
                .IsRequired()
                .HasMaxLength(32)
                .HasColumnType("character varying(32)");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.HasKey("Id");

            b.HasIndex("TenantId", "MedicalRecordNumber")
                .IsUnique();

            b.ToTable("Patients");
        });

        modelBuilder.Entity("HospitalMS.Data.Persistence.Entities.Tenant", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("character varying(200)");

            b.HasKey("Id");

            b.HasIndex("Name")
                .IsUnique();

            b.ToTable("Tenants");
        });

        modelBuilder.Entity("HospitalMS.Data.Persistence.Entities.User", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid");

            b.Property<DateTime>("CreatedAtUtc")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("Email")
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnType("character varying(256)");

            b.Property<bool>("IsActive")
                .HasColumnType("boolean");

            b.Property<string>("PasswordHash")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("character varying(100)");

            b.Property<string>("Role")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("character varying(100)");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<string>("Username")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("character varying(100)");

            b.HasKey("Id");

            b.HasIndex("TenantId", "Username")
                .IsUnique();

            b.ToTable("Users");
        });

        modelBuilder.Entity("HospitalMS.Data.Persistence.Entities.Patient", b =>
        {
            b.HasOne("HospitalMS.Data.Persistence.Entities.Tenant", "Tenant")
                .WithMany("Patients")
                .HasForeignKey("TenantId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.Navigation("Tenant");
        });

        modelBuilder.Entity("HospitalMS.Data.Persistence.Entities.User", b =>
        {
            b.HasOne("HospitalMS.Data.Persistence.Entities.Tenant", "Tenant")
                .WithMany("Users")
                .HasForeignKey("TenantId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.Navigation("Tenant");
        });

        modelBuilder.Entity("HospitalMS.Data.Persistence.Entities.Tenant", b =>
        {
            b.Navigation("Patients");

            b.Navigation("Users");
        });
    }
}
