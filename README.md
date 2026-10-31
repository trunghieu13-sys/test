# SalonCare CRM - Hệ Thống Quản Lý Spa/Salon & Giảm Tỉ Lệ No-Show (W02/W03)

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0%20LTS-purple.svg)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-blue.svg)](https://www.postgresql.org/)
[![EF Core](https://img.shields.io/badge/EF%20Core-8.0-darkgreen.svg)](https://learn.microsoft.com/ef/core/)
[![Tests](https://img.shields.io/badge/xUnit-11%20Passed-brightgreen.svg)]()
[![Architecture](https://img.shields.io/badge/Architecture-C4%20%7C%20Layered-orange.svg)]()

> **Đồ án Chuyên ngành Công nghệ Thông tin (Học kỳ 1 / 2026)**  
> **Sinh viên thực hiện:** Hoàng Khắc Trung Hiếu  
> **MSSV:** 2380600645 | **Lớp:** 23DTHC7  
> **Giảng viên hướng dẫn:** TS. Nguyễn Quang Trung  

---

## 1. TỔNG QUAN DỰ ÁN

**SalonCare** là giải pháp CRM chuyên sâu dành cho các cơ sở Spa và Salon vừa và nhỏ tại Việt Nam. Mục tiêu cốt lõi của hệ thống:
1. **Quản lý lịch hẹn dịch vụ thông minh:** Kiểm soát chặt chẽ khung giờ làm việc của kỹ thuật viên (thợ), ngăn ngừa triệt để tình trạng trùng giờ (Overlap / Double-booking) ở cả 2 cấp độ: Ứng dụng (Application logic) và Cơ sở dữ liệu (PostgreSQL Unique Composite Constraint).
2. **Giảm thiểu tỷ lệ bỏ hẹn (No-show):** Tự động phân khúc khách hàng theo mô hình **RFM (Recency - Frequency - Monetary)**, sớm nhận diện nhóm khách hàng có nguy cơ rời bỏ (`AtRisk` - ngưng hoạt động > 60 ngày) để gửi tin nhắn chăm sóc và nhắc lịch hai chiều qua **Zalo Notification Service (ZNS)**.

---

## 2. YÊU CẦU MÔI TRƯỜNG & HẠ TẦNG (PREREQUISITES)

- **Hệ điều hành:** Windows 10/11, macOS hoặc Linux.
- **.NET SDK:** Phiên bản `.NET 8.0 SDK` (hoặc .NET 10 SDK hỗ trợ target `net8.0`).
- **Cơ sở dữ liệu:** `PostgreSQL 16` (Cổng mặc định: `5432`).
- **Trình soạn thảo khuyến nghị:** Visual Studio 2022 (v17.8+), VS Code, hoặc JetBrains Rider.

---

## 3. CẤU TRÚC DỰ ÁN (LAYERED ARCHITECTURE)

```
Test chuc nang/
├── SalonCare.sln                          # File Solution quản lý toàn bộ dự án
├── ARCHITECTURE.md                        # Tài liệu C4 Diagrams, ADR-01..03, ERD
├── README.md                              # Hướng dẫn cài đặt và kịch bản demo video
│
├── src/
│   ├── SalonCare.Core/                    # Domain Layer: Entities, Enums, Interfaces, Services
│   │   ├── Entities/                      # Appointment, Customer, Technician, RfmScore, AuditLog, User, Role
│   │   ├── Enums/                         # AppointmentStatus, RfmSegment
│   │   ├── Interfaces/                    # IAppointmentRepository, ICustomerRepository, IAppointmentService...
│   │   └── Services/                      # AppointmentService (chống trùng lịch), RfmService (phân nhóm RFM)
│   │
│   ├── SalonCare.Infrastructure/          # Data Layer: EF Core 8.0 & PostgreSQL
│   │   ├── Data/                          # SalonCareDbContext (Unique Composite Index, Seed Data)
│   │   └── Repositories/                  # AppointmentRepository (Overlap checking LINQ), CustomerRepository...
│   │
│   └── SalonCare.Web/                     # Presentation Layer: ASP.NET Core 8.0 MVC
│       ├── Controllers/                   # AppointmentsController (Index, Create with Overlap detection)
│       ├── Models/                        # AppointmentIndexViewModel, CreateAppointmentViewModel
│       ├── Views/                         # Razor Views (Bootstrap 5, Badges, KPI Dashboard, Error Alerts)
│       ├── appsettings.json               # Cấu hình ConnectionString PostgreSQL
│       └── Program.cs                     # Cấu hình DI, DbContext, Auto Seed Data
│
└── tests/
    └── SalonCare.Tests/                   # Testing Layer: xUnit, Moq, FluentAssertions
        ├── AppointmentServiceTests.cs     # 5 Unit Tests: Overlap logic, Successful booking, RFM inactive > 60 days
        └── AppointmentRepositoryTests.cs  # 6 Interval Overlap Cases với DbContext In-Memory
```

---

## 4. HƯỚNG DẪN CÀI ĐẶT & CHẠY DỰ ÁN (GETTING STARTED)

### Bước 4.1: Cấu hình chuỗi kết nối PostgreSQL
Mở file `src/SalonCare.Web/appsettings.json` và điều chỉnh thông số tài khoản PostgreSQL nếu cần:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=saloncare_db;Username=postgres;Password=postgres"
  }
}
```
*(Ghi chú: Nếu máy tính chưa cài PostgreSQL, hệ thống tự động khởi chạy ở chế độ In-Memory an toàn để phục vụ trải nghiệm chấm đồ án).*

### Bước 4.2: Khởi tạo Lược đồ CSDL (EF Core Database Update)
Chạy lệnh sau tại thư mục gốc của dự án:
```powershell
dotnet ef database update --project src/SalonCare.Infrastructure --startup-project src/SalonCare.Web
```

### Bước 4.3: Khởi chạy Ứng dụng Web
Thực thi lệnh sau:
```powershell
dotnet run --project src/SalonCare.Web
```
Sau khi khởi chạy thành công, mở trình duyệt truy cập:
👉 **`http://localhost:5000`** hoặc **`https://localhost:5001`** (hoặc port hiển thị trên console).

---

## 5. HƯỚNG DẪN CHẠY BỘ KIỂM THỬ TỰ ĐỘNG (UNIT TESTS)

Để chạy toàn bộ 11 bài kiểm thử tự động (Unit Tests & Integration Overlap Tests) với kết quả chi tiết:
```powershell
dotnet test
```

### Kết quả kiểm thử mẫu:
```
Passed!  - Failed: 0, Passed: 11, Skipped: 0, Total: 11, Duration: 400 ms - SalonCare.Tests.dll (net8.0)
```
- ✅ `CreateAppointment_ShouldFail_WhenTechnicianIsOverlapped`: Phát hiện trùng lịch KTV -> Báo lỗi.
- ✅ `CreateAppointment_ShouldSucceed_WhenTimeSlotIsAvailable`: Đặt lịch thành công khi khung giờ rảnh.
- ✅ `CalculateRFM_ShouldReturnAtRisk_WhenCustomerInactiveOver60Days`: Phân nhóm `AtRisk` cho khách không ghé > 60 ngày.
- ✅ `CreateAppointment_ShouldFail_WhenEndTimeIsBeforeOrEqualStartTime`: Bắt lỗi giờ kết thúc sớm hơn giờ bắt đầu.
- ✅ `CalculateRFM_ShouldReturnChampions_WhenCustomerVisitsFrequentlyWithHighSpending`: Nhận diện khách VIP.
- ✅ 6 Test cases lý thuyết bao phủ mọi tình huống chồng lấn thời gian (Giao nhau đầu, giao nhau đuôi, nằm trong, bao trùm, chạm mép).

---

## 6. KỊCH BẢN QUAY VIDEO DEMO 30-60 GIÂY (DEMO SCRIPT)

Dành cho sinh viên thực hiện quay video màn hình minh chứng tiến độ Tuần 03 (W02/W03):

| Thời lượng | Phân cảnh | Thao tác thực hiện trên màn hình | Lời thuyết minh gợi ý (Voiceover) |
| :--- | :--- | :--- | :--- |
| **00:00 - 00:10** | Mở đầu & Giới thiệu giao diện | - Mở trình duyệt tại trang chủ `http://localhost:5000`.<br>- Lướt qua Banner thông tin đồ án, KPI Cards và Bảng lịch hẹn đã có sẵn 2 lịch mẫu. | *"Kính chào Thầy và các bạn, đây là hệ thống SalonCare CRM do em Hoàng Khắc Trung Hiếu thực hiện cho Tuần 03. Giao diện hiển thị danh sách lịch hẹn và form đặt lịch."* |
| **00:10 - 00:30** | **Demo 1: Kiểm thử chặn trùng lịch (Overlap Test)** | - Tại form bên trái, chọn:<br>  + Khách hàng: `Phạm Minh Thư`<br>  + KTV: **`Nguyễn Văn Hùng`** (Đang có lịch sẵn lúc 09:00 - 10:00)<br>  + Ngày hẹn: **`02/11/2026`**<br>  + Khung giờ: **`09:15` đến `09:45`**<br>- Bấm nút **[Xác nhận Đặt Lịch]**.<br>- Hệ thống hiển thị Alert đỏ: *"RẤT TIẾC: Kỹ thuật viên đã có lịch hẹn trùng trong khung giờ này!"*. | *"Bây giờ em thử xếp lịch cho KTV Hùng vào khung giờ 9h15 đến 9h45, trùng với lịch đang có từ 9h đến 10h. Khi bấm Đặt lịch, hệ thống phát hiện xung đột và chặn ngay lập tức."* |
| **00:30 - 00:45** | **Demo 2: Đặt lịch thành công khung giờ rảnh** | - Giữ nguyên KTV `Nguyễn Văn Hùng`, đổi khung giờ thành **`14:00` đến `15:00`**.<br>- Bấm nút **[Xác nhận Đặt Lịch]**.<br>- Hệ thống hiển thị Alert xanh: *"Đặt lịch hẹn thành công!"*.<br>- Bảng bên phải lập tức xuất hiện dòng lịch hẹn mới với Badge trạng thái màu xanh lá `Đã xác nhận`. | *"Tiếp theo, em đổi sang khung giờ chiều từ 14h đến 15h. Lịch hẹn được tạo thành công và lập tức cập nhật vào danh sách lịch hẹn của salon."* |
| **00:45 - 00:60** | **Demo 3: Chạy Unit Test tự động trên Terminal** | - Mở Terminal/PowerShell tại thư mục dự án.<br>- Gõ lệnh `dotnet test`.<br>- Hiển thị kết quả **11/11 tests Passed** màu xanh. | *"Cuối cùng, em chạy lệnh `dotnet test`. Toàn bộ 11 bài kiểm thử xUnit về chống trùng lịch và thuật toán RFM đều vượt qua thành công. Em xin cảm ơn Thầy!"* |

---

## 7. LIÊN HỆ & BẢN QUYỀN

- **Tác giả:** Hoàng Khắc Trung Hiếu (MSSV: 2380600645)
- **Lớp:** 23DTHC7 - Khoa Công nghệ Thông tin
- **Giảng viên hướng dẫn:** TS. Nguyễn Quang Trung
- **Đồ án:** Chuyên ngành Công nghệ Thông tin - Đại học Công nghệ TP.HCM (HUTECH)
