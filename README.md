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
1. **Quản lý lịch hẹn dịch vụ thông minh:** Kiểm soát chặt chẽ khung giờ làm việc của kỹ thuật viên (thợ) trong ca làm việc **06:00 sáng đến 16:00 (4h chiều)**, ngăn ngừa triệt để tình trạng trùng giờ (Overlap / Double-booking) ở cả 2 cấp độ: Ứng dụng (Application logic) và Cơ sở dữ liệu (PostgreSQL Unique Composite Constraint).
2. **Hệ thống Slot Thời gian Thực (Nút Đỏ & Nút Xanh):**
   - 🔴 **Khu vực Nút Đỏ (Khung giờ ĐÃ ĐẶT):** Tự động hiển thị toàn bộ các khung giờ KTV đã có khách đặt (1 hoặc nhiều nút đỏ tương ứng). Click vào nút đỏ để điền giờ và test chặn trùng lịch.
   - 🟢 **Khu vực Nút Xanh (Khung giờ CÒN RẢNH):** Tự động tính toán trong ca 06:00 - 16:00, **trừ đi tất cả các khung giờ đã đặt**, hiển thị các slot còn rảnh. Click vào nút xanh để điền giờ và đặt lịch thành công tức thì.
   - 🗑️ **Quản lý Hủy/Xóa lịch:** Cho phép xóa lịch hẹn trực tiếp trên bảng danh sách để kiểm thử linh hoạt.
3. **Giảm thiểu tỷ lệ bỏ hẹn (No-show):** Tự động phân khúc khách hàng theo mô hình **RFM (Recency - Frequency - Monetary)**, sớm nhận diện nhóm khách hàng có nguy cơ rời bỏ (`AtRisk` - ngưng hoạt động > 60 ngày) để gửi tin nhắn chăm sóc và nhắc lịch hai chiều qua **Zalo Notification Service (ZNS)**.

---

## 2. YÊU CẦU MÔI TRƯỜNG & HẠ TẦNG (PREREQUISITES)

- **Hệ điều hành:** Windows 10/11, macOS hoặc Linux.
- **.NET SDK:** Phiên bản `.NET 8.0 SDK` (hoặc .NET 10 SDK hỗ trợ target `net8.0`).
- **Cơ sở dữ liệu:** `PostgreSQL 16` (Cổng mặc định: `5432`). Hệ thống hỗ trợ chế độ Zero-Config In-Memory fallback tự động để trải nghiệm demo mượt mà ngay cả khi chưa bật PostgreSQL.
- **Trình duyệt khuyến nghị:** Chrome, Edge, Firefox.

---

## 3. CẤU TRÚC DỰ ÁN (LAYERED ARCHITECTURE)

```
Test chuc nang/
├── SalonCare.sln                          # File Solution quản lý toàn bộ dự án
├── ARCHITECTURE.md                        # Tài liệu C4 Diagrams, ADR-01..03, ERD
├── README.md                              # Hướng dẫn cài đặt và kịch bản demo video
├── docs/
│   └── REPORT_CHAPTER_3.md                # Nội dung Mục 3.1 & 3.2 sẵn sàng dán vào Word
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
│   │   └── Repositories/                  # AppointmentRepository (Overlap checking LINQ, GetByTechnicianAndDate)
│   │
│   └── SalonCare.Web/                     # Presentation Layer: ASP.NET Core 8.0 MVC
│       ├── Controllers/                   # AppointmentsController (Index, Create, Delete, GetSchedule API)
│       ├── Models/                        # AppointmentIndexViewModel, CreateAppointmentViewModel
│       ├── Views/                         # Razor Views (Bootstrap 5, Badges, Nút Đỏ/Xanh, Error Alerts)
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

### Bước 4.2: Khởi tạo Lược đồ CSDL (EF Core Database Update)
Chạy lệnh sau tại thư mục gốc:
```powershell
dotnet ef database update --project src/SalonCare.Infrastructure --startup-project src/SalonCare.Web
```

### Bước 4.3: Khởi chạy Ứng dụng Web
Thực thi lệnh sau:
```powershell
dotnet run --project src/SalonCare.Web
```
Sau khi khởi chạy thành công, mở trình duyệt truy cập:
👉 **`http://localhost:5259`**

---

## 5. HƯỚNG DẪN CHẠY BỘ KIỂM THỬ TỰ ĐỘNG (UNIT TESTS)

Để chạy toàn bộ 11 bài kiểm thử tự động (Unit Tests & Integration Overlap Tests) với kết quả chi tiết:
```powershell
dotnet test
```

### Kết quả kiểm thử:
```
Passed!  - Failed: 0, Passed: 11, Skipped: 0, Total: 11, Duration: 400 ms - SalonCare.Tests.dll (net8.0)
```
- ✅ `CreateAppointment_ShouldFail_WhenTechnicianIsOverlapped`: Phát hiện trùng lịch KTV -> Báo lỗi.
- ✅ `CreateAppointment_ShouldSucceed_WhenTimeSlotIsAvailable`: Đặt lịch thành công khi khung giờ rảnh.
- ✅ `CalculateRFM_ShouldReturnAtRisk_WhenCustomerInactiveOver60Days`: Phân nhóm `AtRisk` cho khách không ghé > 60 ngày.
- ✅ `CreateAppointment_ShouldFail_WhenEndTimeIsBeforeOrEqualStartTime`: Bắt lỗi giờ kết thúc sớm hơn giờ bắt đầu.
- ✅ `CalculateRFM_ShouldReturnChampions_WhenCustomerVisitsFrequentlyWithHighSpending`: Nhận diện khách VIP.
- ✅ 6 Test cases lý thuyết bao phủ mọi tình huống chồng lấn thời gian.

---

## 6. KỊCH BẢN QUAY VIDEO DEMO 30-60 GIÂY (DEMO SCRIPT)

| Thời lượng | Phân cảnh | Thao tác thực hiện trên màn hình | Lời thuyết minh gợi ý (Voiceover) |
| :--- | :--- | :--- | :--- |
| **00:00 - 00:10** | Mở đầu & Giới thiệu giao diện | - Mở trình duyệt tại `http://localhost:5259`.<br>- Giới thiệu ca làm việc 06:00 - 16:00, 2 vùng Nút Đỏ và Nút Xanh. | *"Kính chào Thầy, đây là giao diện SalonCare CRM. Hệ thống áp dụng khung giờ làm việc từ 6h sáng đến 4h chiều với cơ chế tính slot thông minh."* |
| **00:10 - 00:25** | **Demo 1: Nút Đỏ (Khung giờ đã đặt)** | - Bấm vào **nút màu đỏ** (ví dụ `09:00 - 10:00`).<br>- Giờ bắt đầu và kết thúc tự động điền.<br>- Bấm **[Xác nhận Đặt Lịch]**.<br>- Hệ thống hiển thị cảnh báo đỏ chặn trùng lịch. | *"Bên trên là danh sách nút đỏ hiển thị các khung giờ KTV đã có người đặt. Khi em bấm vào nút đỏ và xác nhận đặt lịch, hệ thống sẽ lập tức chặn lại và báo lỗi trùng lịch."* |
| **00:25 - 00:45** | **Demo 2: Nút Xanh (Khung giờ còn rảnh)** | - Bấm vào một **nút màu xanh** (ví dụ `07:00 - 08:00`).<br>- Bấm **[Xác nhận Đặt Lịch]**.<br>- Hệ thống hiển thị Alert xanh thành công.<br>- Khung giờ `07:00 - 08:00` lập tức chuyển thành nút màu đỏ! | *"Bên dưới là các nút xanh hiển thị các khung giờ rảnh sau khi đã trừ các lịch đã đặt. Khi bấm vào nút xanh và xác nhận, lịch hẹn được tạo thành công và slot này chuyển sang màu đỏ ngay lập tức."* |
| **00:45 - 00:60** | **Demo 3: Chạy Unit Test tự động trên Terminal** | - Mở Terminal/PowerShell tại thư mục dự án.<br>- Gõ lệnh `dotnet test`.<br>- Hiển thị kết quả **11/11 tests Passed** màu xanh. | *"Cuối cùng, toàn bộ 11 bài kiểm thử tự động xUnit về chống trùng lịch và thuật toán RFM đều vượt qua thành công 100%."* |

---

## 7. LIÊN HỆ & BẢN QUYỀN

- **Tác giả:** Hoàng Khắc Trung Hiếu (MSSV: 2380600645)
- **Lớp:** 23DTHC7 - Khoa Công nghệ Thông tin
- **Giảng viên hướng dẫn:** TS. Nguyễn Quang Trung
- **Đồ án:** Chuyên ngành Công nghệ Thông tin - Đại học Công nghệ TP.HCM (HUTECH)
- **GitHub Repository:** [https://github.com/trunghieu13-sys/test](https://github.com/trunghieu13-sys/test)
