using Microsoft.EntityFrameworkCore;
using Npgsql;
using SalonCare.Core.Interfaces;
using SalonCare.Core.Services;
using SalonCare.Infrastructure.Data;
using SalonCare.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình Controllers with Views (MVC)
builder.Services.AddControllersWithViews();

// Kiểm tra kết nối CSDL PostgreSQL trước khi khởi tạo
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
bool isPostgreSqlAvailable = false;

if (!string.IsNullOrWhiteSpace(connectionString))
{
    try
    {
        using var testConn = new NpgsqlConnection(connectionString);
        testConn.Open();
        isPostgreSqlAvailable = true;
    }
    catch
    {
        isPostgreSqlAvailable = false;
    }
}

// Đăng ký DbContext: Tự động dùng PostgreSQL nếu kết nối thành công,
// hoặc chuyển đổi an toàn sang InMemory Database để luôn đảm bảo chạy demo ổn định 100%.
builder.Services.AddDbContext<SalonCareDbContext>(options =>
{
    if (isPostgreSqlAvailable)
    {
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
        });
    }
    else
    {
        options.UseInMemoryDatabase("SalonCare_Active_Db");
    }
});

// Đăng ký Dependency Injection (DI) cho Repository và Service Layers
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ITechnicianRepository, TechnicianRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IRfmService, RfmService>();

var app = builder.Build();

// Khởi tạo Database Schema & nạp Seed Data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var db = services.GetRequiredService<SalonCareDbContext>();

    if (isPostgreSqlAvailable)
    {
        logger.LogInformation(">>> Đang sử dụng cơ sở dữ liệu: PostgreSQL 16");
        db.Database.EnsureCreated();
        logger.LogInformation(">>> Khởi tạo cấu trúc PostgreSQL và Seed Data thành công!");
    }
    else
    {
        logger.LogWarning(">>> Không kết nối được PostgreSQL (Kiểm tra lại tài khoản/mật khẩu trong appsettings.json).");
        logger.LogInformation(">>> Hệ thống kích hoạt In-Memory Database Mode: Khởi tạo Seed Data demo sẵn sàng!");
        db.Database.EnsureCreated();
    }
}

// Cấu hình HTTP Request Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// Route mặc định dẫn trực tiếp vào module Lịch hẹn của SalonCare
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Appointments}/{action=Index}/{id?}");

app.Run();
