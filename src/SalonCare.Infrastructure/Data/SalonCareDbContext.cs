using Microsoft.EntityFrameworkCore;
using SalonCare.Core.Entities;
using SalonCare.Core.Enums;

namespace SalonCare.Infrastructure.Data;

/// <summary>
/// Entity Framework Core DbContext cho hệ thống SalonCare CRM.
/// Hỗ trợ PostgreSQL 16 và Unique Constraints chống trùng lịch ở cấp độ cơ sở dữ liệu.
/// </summary>
public class SalonCareDbContext : DbContext
{
    public SalonCareDbContext(DbContextOptions<SalonCareDbContext> options) : base(options)
    {
    }

    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Technician> Technicians => Set<Technician>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RfmScore> RfmScores => Set<RfmScore>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Cấu hình bảng APPOINTMENTS
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("Appointments");
            entity.HasKey(a => a.AppointmentId);

            entity.Property(a => a.ServiceName)
                  .IsRequired()
                  .HasMaxLength(150);

            entity.Property(a => a.Price)
                  .HasColumnType("decimal(18,2)")
                  .IsRequired();

            entity.Property(a => a.Status)
                  .HasConversion<int>()
                  .IsRequired();

            entity.Property(a => a.Notes)
                  .HasMaxLength(500);

            // QUAN TRỌNG: Unique Index ở cấp CSDL để chống Race Condition và đảm bảo KTV không trùng giờ bắt đầu
            entity.HasIndex(a => new { a.TechnicianId, a.AppointmentDate, a.StartTime })
                  .IsUnique()
                  .HasDatabaseName("IX_Appointments_Technician_Date_StartTime_Unique");

            // Quan hệ với Customer
            entity.HasOne(a => a.Customer)
                  .WithMany(c => c.Appointments)
                  .HasForeignKey(a => a.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ với Technician
            entity.HasOne(a => a.Technician)
                  .WithMany(t => t.Appointments)
                  .HasForeignKey(a => a.TechnicianId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 2. Cấu hình bảng CUSTOMERS
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(c => c.CustomerId);

            entity.Property(c => c.FullName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(c => c.PhoneNumber)
                  .IsRequired()
                  .HasMaxLength(20);

            entity.HasIndex(c => c.PhoneNumber)
                  .IsUnique()
                  .HasDatabaseName("IX_Customers_PhoneNumber_Unique");

            entity.Property(c => c.Email)
                  .HasMaxLength(100);

            entity.Property(c => c.Address)
                  .HasMaxLength(255);

            entity.Property(c => c.TotalSpent)
                  .HasColumnType("decimal(18,2)");
        });

        // 3. Cấu hình bảng TECHNICIANS
        modelBuilder.Entity<Technician>(entity =>
        {
            entity.ToTable("Technicians");
            entity.HasKey(t => t.TechnicianId);

            entity.Property(t => t.FullName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(t => t.PhoneNumber)
                  .IsRequired()
                  .HasMaxLength(20);

            entity.Property(t => t.Specialization)
                  .IsRequired()
                  .HasMaxLength(100);
        });

        // 4. Cấu hình bảng RFM_SCORES
        modelBuilder.Entity<RfmScore>(entity =>
        {
            entity.ToTable("RfmScores");
            entity.HasKey(r => r.RfmScoreId);

            entity.Property(r => r.Monetary)
                  .HasColumnType("decimal(18,2)");

            entity.Property(r => r.RfmSegment)
                  .HasConversion<int>();

            entity.HasOne(r => r.Customer)
                  .WithMany(c => c.RfmScores)
                  .HasForeignKey(r => r.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 5. Cấu hình bảng AUDIT_LOGS
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(l => l.AuditLogId);

            entity.Property(l => l.EntityName).HasMaxLength(50).IsRequired();
            entity.Property(l => l.Action).HasMaxLength(50).IsRequired();
            entity.Property(l => l.PerformedBy).HasMaxLength(100).IsRequired();
        });

        // 6. Cấu hình bảng ROLES & USERS
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(r => r.RoleId);
            entity.Property(r => r.RoleName).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.UserId);
            entity.Property(u => u.Username).HasMaxLength(50).IsRequired();
            entity.HasIndex(u => u.Username).IsUnique();

            entity.HasOne(u => u.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(u => u.RoleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // SEED DATA MẪU CHO DEMO VÀ BÁO CÁO
        SeedInitialData(modelBuilder);
    }

    private static void SeedInitialData(ModelBuilder modelBuilder)
    {
        // 1. Roles
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, RoleName = "SalonOwner", Description = "Chủ salon, toàn quyền quản trị" },
            new Role { RoleId = 2, RoleName = "Receptionist", Description = "Lễ tân tiếp đón và điều phối lịch hẹn" }
        );

        // 2. Technicians
        modelBuilder.Entity<Technician>().HasData(
            new Technician
            {
                TechnicianId = 1,
                FullName = "Nguyễn Văn Hùng",
                PhoneNumber = "0908111222",
                Specialization = "Chuyên gia Cắt & Tạo kiểu Tóc",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Technician
            {
                TechnicianId = 2,
                FullName = "Trần Thị Mai",
                PhoneNumber = "0918333444",
                Specialization = "Kỹ thuật viên Chăm sóc da & Massage trị liệu",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Technician
            {
                TechnicianId = 3,
                FullName = "Lê Hoàng Yến",
                PhoneNumber = "0938555666",
                Specialization = "Nghệ nhân Chăm sóc & Tạo mẫu Nail",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // 3. Customers
        modelBuilder.Entity<Customer>().HasData(
            new Customer
            {
                CustomerId = 1,
                FullName = "Hoàng Khắc Trung Hiếu",
                PhoneNumber = "0909123456",
                Email = "trunghieu.hcm@gmail.com",
                Address = "Quận 1, TP. Hồ Chí Minh",
                TotalSpent = 3500000,
                LastVisitDate = new DateTime(2026, 10, 20, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc)
            },
            new Customer
            {
                CustomerId = 2,
                FullName = "Phạm Minh Thư",
                PhoneNumber = "0987654321",
                Email = "minhthu.pham@yahoo.com",
                Address = "Bình Thạnh, TP. Hồ Chí Minh",
                TotalSpent = 1200000,
                LastVisitDate = new DateTime(2026, 8, 15, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = new DateTime(2026, 2, 5, 0, 0, 0, DateTimeKind.Utc)
            },
            new Customer
            {
                CustomerId = 3,
                FullName = "Đỗ Gia Hưng",
                PhoneNumber = "0977889900",
                Email = "giahung.do@outlook.com",
                Address = "Phú Nhuận, TP. Hồ Chí Minh",
                TotalSpent = 550000,
                LastVisitDate = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = new DateTime(2026, 3, 12, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // 4. Appointments mẫu (Ngày hôm nay hoặc gần nhất)
        modelBuilder.Entity<Appointment>().HasData(
            new Appointment
            {
                AppointmentId = 1,
                CustomerId = 1,
                TechnicianId = 1, // KTV Nguyễn Văn Hùng
                AppointmentDate = new DateOnly(2026, 11, 2),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0),
                ServiceName = "Combo Cắt Gội & Tạo kiểu Quý ông",
                Price = 250000,
                Status = AppointmentStatus.Confirmed,
                Notes = "Khách yêu cầu kỹ thuật viên Hùng tạo kiểu",
                CreatedAt = new DateTime(2026, 10, 31, 8, 0, 0, DateTimeKind.Utc)
            },
            new Appointment
            {
                AppointmentId = 2,
                CustomerId = 2,
                TechnicianId = 2, // KTV Trần Thị Mai
                AppointmentDate = new DateOnly(2026, 11, 2),
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(11, 30),
                ServiceName = "Liệu trình Chăm sóc da Chuyên sâu Căng bóng",
                Price = 650000,
                Status = AppointmentStatus.Pending,
                Notes = "Khách mới đặt qua Web cần gọi điện xác nhận",
                CreatedAt = new DateTime(2026, 10, 31, 9, 30, 0, DateTimeKind.Utc)
            }
        );
    }
}
