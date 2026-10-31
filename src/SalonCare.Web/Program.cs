using Microsoft.EntityFrameworkCore;
using SalonCare.Core.Interfaces;
using SalonCare.Core.Services;
using SalonCare.Infrastructure.Data;
using SalonCare.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình Controllers with Views (MVC)
builder.Services.AddControllersWithViews();

// Cấu hình PostgreSQL Database với Entity Framework Core 8.0
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Đăng ký DbContext: Ưu tiên PostgreSQL Npgsql
builder.Services.AddDbContext<SalonCareDbContext>(options =>
{
    if (!string.IsNullOrEmpty(connectionString))
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
        options.UseInMemoryDatabase("SalonCare_Demo_Db");
    }
});

// Đăng ký Dependency Injection (DI) cho Repository và Service Layers
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ITechnicianRepository, TechnicianRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IRfmService, RfmService>();

var app = builder.Build();

// Tự động kiểm tra và khởi tạo Database Schema & Seed Data ban đầu
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = services.GetRequiredService<SalonCareDbContext>();
        logger.LogInformation("Đang kiểm tra kết nối CSDL PostgreSQL...");
        db.Database.EnsureCreated();
        logger.LogInformation("Khởi tạo cấu trúc CSDL và Seed Data thành công!");
    }
    catch (Exception ex)
    {
        logger.LogWarning("Không thể kết nối trực tiếp đến PostgreSQL ({Message}). Ứng dụng sẽ hoạt động ở chế độ Demo Memory để đảm bảo chạy mượt mà.", ex.Message);
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
