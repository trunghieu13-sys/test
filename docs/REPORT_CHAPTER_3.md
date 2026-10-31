# CHƯƠNG 3: THIẾT KẾ KIẾN TRÚC VÀ HIỆN THỰC HỆ THỐNG

> **Nội dung trích xuất dành cho Báo cáo Tiến độ Đồ án Chuyên ngành CNTT**  
> **Tên đề tài:** Hệ thống CRM quản lý Spa/Salon nhỏ giảm tỉ lệ bỏ hẹn (No-show) dựa trên phân khúc RFM (SalonCare)  
> **Sinh viên thực hiện:** Hoàng Khắc Trung Hiếu | **MSSV:** 2380600645 | **Lớp:** 23DTHC7  
> **Giảng viên hướng dẫn:** TS. Nguyễn Quang Trung  

---

## 3.1. PHÂN TÍCH VÀ THIẾT KẾ KIẾN TRÚC HỆ THỐNG (SYSTEM ARCHITECTURE DESIGN)

### 3.1.1. Bối cảnh bài toán và Yêu cầu phi chức năng
Trong mô hình kinh doanh dịch vụ làm đẹp, Spa và Salon vừa và nhỏ tại Việt Nam, doanh thu và năng suất hoạt động phụ thuộc trực tiếp vào thời gian làm việc hiệu dụng của đội ngũ kỹ thuật viên (thợ làm tóc, chuyên viên chăm sóc da, nghệ nhân nail). Qua khảo sát thực tế, hai vấn đề nhức nhối nhất mà các chủ cơ sở đối mặt là:
1. **Xung đột lịch hẹn (Double-booking / Overlap):** Khi tiếp nhận lịch hẹn qua nhiều kênh rời rạc (gọi điện thoại, nhắn tin fanpage, khách đến trực tiếp), việc thiếu một cơ chế khóa tương quan thời gian theo thời gian thực dễ dẫn đến tình trạng hai khách hàng cùng được xếp vào một khung giờ cho cùng một kỹ thuật viên, gây quá tải, suy giảm chất lượng phục vụ và làm mất uy tín thương hiệu.
2. **Khách hàng bỏ hẹn không báo trước (No-show):** Tỷ lệ No-show tại các salon truyền thống thường chiếm từ 15% đến 30%. Nếu chỉ nhắc lịch thụ động qua điện thoại, chi phí nhân sự rất cao, trong khi tin nhắn SMS Brandname một chiều có chi phí đắt đỏ (500 - 800 VNĐ/tin) và tỷ lệ tương tác thấp.

Từ bối cảnh trên, hệ thống **SalonCare CRM** được thiết kế với các yêu cầu kiến trúc trọng yếu:
- **Tính toàn vẹn và nhất quán dữ liệu (Data Integrity):** Ngăn chặn tuyệt đối việc trùng lịch của KTV ở cả 2 lớp phòng thủ: Lớp Nghiệp vụ Ứng dụng (Business Logic) và Lớp Ràng buộc CSDL (Database Unique Constraints).
- **Khả năng mở rộng theo module (Modularity & Maintainability):** Áp dụng kiến trúc phân tầng chuẩn mực của Microsoft .NET 8 (Clean/Layered Architecture), tách bạch rõ Domain Core, Infrastructure, Presentation Web và Test Layer.
- **Tự động hóa chăm sóc khách hàng:** Tích hợp mô hình phân tích RFM (Recency - Frequency - Monetary) nhằm tự động phát hiện nhóm khách hàng có nguy cơ rời bỏ (`AtRisk` - ngưng hoạt động > 60 ngày) và kích hoạt kênh nhắc lịch hai chiều qua Zalo Official Account (ZNS).

---

### 3.1.2. Mô hình Kiến trúc C4

#### A. Sơ đồ C4 Mức 1: Bối cảnh hệ thống (System Context)
Sơ đồ C4 Mức 1 định hình vị trí trung tâm của hệ thống SalonCare CRM trong hệ sinh thái vận hành của một cơ sở làm đẹp:

```
                                +---------------------------+
                                |  Chủ Salon (Salon Owner)  |
                                +---------------------------+
                                              |
                             [HTTPS] Xem báo cáo RFM, Doanh thu
                                              v
+------------------------+      +---------------------------+      +-------------------------+
| Lễ tân (Receptionist)  | ---> |   HỆ THỐNG SALONCARE CRM  | ---> | Zalo OA / ZNS Service   |
+------------------------+      +---------------------------+      +-------------------------+
   Xếp lịch, kiểm tra              ^                     |             Bắn tin ZNS nhắc hẹn
   trùng giờ KTV [HTTPS]           |  Nhận Webhook       |             (T-24h, T-2h) [HTTPS]
                                   |  phản hồi [HTTPS]   |                        |
                                   +---------------------+                        | Zalo App
                                                                                  v
                                                                   +-------------------------+
                                                                   |  Khách hàng (Customer)  |
                                                                   +-------------------------+
```

*Thuyết minh các tác nhân và hệ thống liên kết:*
- **Chủ Salon (Salon Owner):** Theo dõi Dashboard kinh doanh, chỉ số No-show, báo cáo phân nhóm khách hàng RFM và phân bổ nhân sự KTV.
- **Lễ tân (Receptionist):** Sử dụng giao diện Web Responsive để tiếp nhận yêu cầu đặt hẹn, theo dõi trạng thái lịch và xử lý tức thời các cảnh báo xung đột lịch KTV.
- **Khách hàng (Customer):** Nhận tin nhắn thông báo xác nhận lịch hẹn và tin nhắc hẹn tự động trên ứng dụng Zalo, có thể bấm nút tương tác "Xác nhận đến" hoặc "Đổi lịch" trực tiếp trên màn hình tin nhắn.
- **Hệ thống Zalo OA / ZNS (External Service):** Đóng vai trò cầu nối truyền tin OTT tin cậy, cung cấp API gửi tin nhắn ZNS chăm sóc khách hàng và cơ chế Webhook trả kết quả tương tác về SalonCare.

#### B. Sơ đồ C4 Mức 2: Cấu trúc Container
Sơ đồ Container phân rã SalonCare CRM thành 4 khối thực thi độc lập:
1. **Web App UI Container (Razor Views & Bootstrap 5):** Chạy trên trình duyệt của người dùng, giao tiếp với Backend qua giao thức HTTPS (Form POST và JSON Request).
2. **Web API & MVC Backend Container (ASP.NET Core 8.0 / C#):** Đảm nhiệm xử lý định tuyến (Routing), kiểm tra xác thực, thực thi các quy tắc nghiệp vụ `IAppointmentService`, `IRfmService`.
3. **Database Container (PostgreSQL 16):** Lưu trữ toàn bộ dữ liệu nghiệp vụ, quản lý các giao dịch ACID và thực thi ràng buộc Unique Index chống trùng lịch. Giao tiếp qua giao thức TCP/SQL (Cổng 5432).
4. **Background Worker Container (Quartz.NET / .NET BackgroundService):** Tác vụ chạy ngầm định kỳ quét lịch hẹn cần gửi nhắc nhở ZNS (T-24h, T-2h) và job quét tính điểm RFM lúc 00:00 hàng ngày.

---

### 3.1.3. Hồ sơ Quyết định Kiến trúc (Architecture Decision Records - ADR)

Nhằm đảm bảo tính minh bạch và chuẩn mực kỹ thuật trong đồ án, nhóm nghiên cứu thiết lập 3 bản ghi quyết định kiến trúc theo tiêu chuẩn của Michael Nygard:

#### Bảng 3.1: Tổng hợp các Quyết định Kiến trúc (ADR) của dự án SalonCare

| Mã ADR | Tiêu đề quyết định | Các phương án so sánh | Phương án lựa chọn | Lý do cốt lõi & Giá trị mang lại |
| :--- | :--- | :--- | :--- | :--- |
| **ADR-01** | Lựa chọn CSDL Relational & Xử lý tương tranh | • **PostgreSQL 16**<br>• MongoDB (NoSQL) | **PostgreSQL 16** | Đảm bảo tính toàn vẹn giao dịch (ACID); hỗ trợ tạo Unique Index chống trùng lịch KTV đa cột; hỗ trợ tối ưu kiểu dữ liệu `date`, `time` native trong EF Core 8. |
| **ADR-02** | Lựa chọn Nền tảng Backend Framework | • **ASP.NET Core 8.0 MVC**<br>• Node.js (Express.js) | **ASP.NET Core 8.0** | Kiểu dữ liệu tĩnh (Strongly-typed) phát hiện lỗi tại compile-time; DI container và ORM (EF Core) tích hợp nguyên bản; hiệu năng hàng đầu và tính bảo mật cao (Anti-CSRF). |
| **ADR-03** | Lựa chọn Kênh Nhắc lịch & Giảm No-show | • **Zalo OA (ZNS)**<br>• SMS Brandname<br>• Email | **Zalo OA (ZNS)** | Tỷ lệ mở tin > 85% tại thị trường Việt Nam; chi phí rẻ hơn 60% so với SMS; hỗ trợ nút bấm tương tác CTA 2 chiều qua Webhook giúp khách xác nhận hoặc đổi lịch tức thì. |

---

### 3.1.4. Thiết kế Cơ sở dữ liệu và Sơ đồ Thực thể Liên kết (ERD)

Lược đồ cơ sở dữ liệu quan hệ của SalonCare được chuẩn hóa đến dạng chuẩn 3NF (Third Normal Form) nhằm hạn chế tối đa dư thừa dữ liệu:
- **`ROLES` (1) ─── (N) `USERS`:** Phân quyền quản trị hệ thống (`SalonOwner`, `Receptionist`).
- **`CUSTOMERS` (1) ─── (N) `APPOINTMENTS`:** Quản lý lịch sử đặt lịch dịch vụ của khách hàng.
- **`TECHNICIANS` (1) ─── (N) `APPOINTMENTS`:** Phân công kỹ thuật viên thực hiện dịch vụ.
- **`CUSTOMERS` (1) ─── (N) `RFM_SCORES`:** Lưu trữ lịch sử chấm điểm Recency, Frequency, Monetary qua các chu kỳ.
- **`AUDIT_LOGS`:** Bảng ghi vết nhật ký kiểm toán độc lập phục vụ giám sát và truy vết an ninh.

**Giải pháp kỹ thuật cốt lõi chống trùng lịch tại tầng CSDL:**  
Trong file cấu hình `SalonCareDbContext.cs`, nhóm nghiên cứu khai báo chỉ mục duy nhất kết hợp (Composite Unique Index):
```csharp
modelBuilder.Entity<Appointment>()
    .HasIndex(a => new { a.TechnicianId, a.AppointmentDate, a.StartTime })
    .IsUnique()
    .HasDatabaseName("IX_Appointments_Technician_Date_StartTime_Unique");
```
Chỉ mục này đóng vai trò rào chắn vật lý tầng thấp nhất: ngay cả khi xảy ra hiện tượng tranh chấp tương tranh dữ liệu (Race Condition) từ hai luồng HTTP đồng thời vượt qua tầng ứng dụng, hệ thống PostgreSQL sẽ ném ra ngoại lệ vi phạm khóa duy nhất `UniqueConstraintViolationException`, ngăn chặn tuyệt đối việc lưu trữ 2 bản ghi trùng lặp vào đĩa cứng.

---

## 3.2. HIỆN THỰC MÃ NGUỒN ("HELLO WORLD" ĐẦU-CUỐI) VÀ KIỂM THỬ TỰ ĐỘNG

### 3.2.1. Cấu trúc Giải pháp Phân tầng (Layered Architecture)
Dự án được khởi tạo theo mô hình phân tầng hướng đối tượng trên nền tảng .NET 8.0:
1. **`SalonCare.Core` (Domain Layer):**
   - Chứa các thực thể chính (`Customer`, `Technician`, `Appointment`, `RfmScore`, `AuditLog`, `User`, `Role`).
   - Chứa định nghĩa kiểu liệt kê (`AppointmentStatus`, `RfmSegment`).
   - Chứa các giao diện nghiệp vụ độc lập với hạ tầng (`IAppointmentRepository`, `IAppointmentService`, `IRfmService`).
   - Hiện thực nghiệp vụ cốt lõi trong `AppointmentService` và `RfmService`.
2. **`SalonCare.Infrastructure` (Data Access Layer):**
   - Hiện thực `SalonCareDbContext` với cấu hình ánh xạ Fluent API và nạp dữ liệu mẫu ban đầu (Data Seeding).
   - Hiện thực các kho dữ liệu `AppointmentRepository`, `CustomerRepository`, `TechnicianRepository` sử dụng Entity Framework Core 8.0 và Npgsql Driver.
3. **`SalonCare.Web` (Presentation Layer):**
   - Hiện thực `AppointmentsController` điều phối luồng thao tác người dùng.
   - Các ViewModels đóng gói dữ liệu truyền tải (`AppointmentIndexViewModel`, `CreateAppointmentViewModel`).
   - Giao diện Razor Views tích hợp Bootstrap 5.3, Bootstrap Icons và hệ thống bảng theo dõi trực quan.
4. **`SalonCare.Tests` (Automated Testing Layer):**
   - Bộ kiểm thử tự động sử dụng xUnit 2.9, Moq 4.20 và FluentAssertions 6.12.

---

### 3.2.2. Hiện thực Thuật toán Chống Trùng Giờ Kỹ Thuật Viên (Interval Overlap Algorithm)

#### A. Cơ sở lý thuyết toán học về giao thoa khoảng thời gian
Một lịch hẹn dịch vụ mới cần xếp có khoảng thời gian $A = [S_{new}, E_{new})$ và một lịch hẹn đã tồn tại trong CSDL của cùng một kỹ thuật viên trong cùng một ngày có khoảng thời gian $B = [S_{exist}, E_{exist})$.

Hai khoảng thời gian $A$ và $B$ giao nhau (xung đột thời gian) khi và chỉ khi thỏa mãn đồng thời hai bất đẳng thức:
$$\text{Overlap}(A, B) \iff (S_{new} < E_{exist}) \land (E_{new} > S_{exist})$$

#### B. Hiện thực truy vấn LINQ to Entities trong `AppointmentRepository.cs`
Thuật toán được dịch trực tiếp sang câu lệnh truy vấn LINQ bất đồng bộ, loại bỏ các lịch hẹn đã bị hủy (`Status != AppointmentStatus.Canceled`):

```csharp
public async Task<bool> HasOverlapAsync(
    int technicianId,
    DateOnly appointmentDate,
    TimeOnly startTime,
    TimeOnly endTime,
    int? excludeAppointmentId = null)
{
    return await _context.Appointments
        .AsNoTracking()
        .AnyAsync(a =>
            a.TechnicianId == technicianId &&
            a.AppointmentDate == appointmentDate &&
            a.Status != AppointmentStatus.Canceled &&
            (!excludeAppointmentId.HasValue || a.AppointmentId != excludeAppointmentId.Value) &&
            startTime < a.EndTime &&
            endTime > a.StartTime
        );
}
```

#### C. Xử lý phản hồi tại `AppointmentsController.cs`
Khi người dùng bấm submit form đặt lịch, action `Create` tiếp nhận dữ liệu và thực hiện kiểm tra trước khi ghi vào cơ sở dữ liệu:

```csharp
var hasOverlap = await _appointmentRepository.HasOverlapAsync(
    model.TechnicianId,
    model.AppointmentDate,
    model.StartTime,
    model.EndTime
);

if (hasOverlap)
{
    TempData["ErrorMessage"] = "RẤT TIẾC: Kỹ thuật viên đã có lịch hẹn trùng trong khung giờ này!";
    return RedirectToAction(nameof(Index));
}
```

---

### 3.2.3. Hiện thực Giao diện Người dùng và Tương tác Thực tế

Giao diện trang chủ (`/Appointments/Index`) được xây dựng theo ngôn ngữ thiết kế Dashboard hiện đại với bố cục 2 cột tiện dụng:
- **Cột bên trái:** Form tiếp nhận đặt lịch hẹn với các trường chọn Khách hàng, chọn Kỹ thuật viên, tên dịch vụ, đơn giá, ngày hẹn và hai trường chọn giờ `StartTime` - `EndTime`.
- **Cột bên phải:** Bảng hiển thị danh sách các lịch hẹn hiện có trong ngày kèm các Badge trạng thái sinh động (`Đã xác nhận` màu xanh lá, `Chờ xử lý` màu vàng hổ phách, `No-Show` màu đỏ cảnh báo).
- **Hệ thống Alert thông báo:** Khi phát hiện trùng giờ, hệ thống hiển thị ngay hộp thông báo màu đỏ nổi bật với nội dung cảnh báo rõ ràng, giữ lại dữ liệu và hướng dẫn người dùng điều chỉnh khung giờ.

---

### 3.2.4. Xây dựng Bộ Kiểm thử Tự động (Automated Unit Tests)

Để bảo đảm chất lượng phần mềm ngay từ giai đoạn W02/W03, nhóm nghiên cứu đã xây dựng bộ kiểm thử tự động toàn diện trong dự án `SalonCare.Tests`:

#### Bảng 3.2: Danh mục các ca kiểm thử tự động (Unit Test Suite)

| STT | Tên ca kiểm thử (Test Name) | Mục đích kiểm thử | Dữ liệu đầu vào (Input) | Kết quả kỳ vọng (Assertion) | Trạng thái |
| :---: | :--- | :--- | :--- | :--- | :---: |
| 1 | `CreateAppointment_ShouldFail_WhenTechnicianIsOverlapped` | Xác nhận chặn đặt lịch khi KTV đã có lịch trùng giờ | KTV 1, Ngày 02/11/2026, 09:15 - 10:15 (Trùng lịch 09:00 - 10:00) | `IsSuccess = false`, hiển thị thông báo lỗi, `AddAsync` không được gọi. | **PASSED** ✅ |
| 2 | `CreateAppointment_ShouldSucceed_WhenTimeSlotIsAvailable` | Xác nhận đặt lịch thành công khi khung giờ hoàn toàn rảnh | KTV 2, Ngày 02/11/2026, 14:00 - 15:30 (Khung giờ trống) | `IsSuccess = true`, thông báo thành công, `AddAsync` được gọi đúng 1 lần. | **PASSED** ✅ |
| 3 | `CalculateRFM_ShouldReturnAtRisk_WhenCustomerInactiveOver60Days` | Xác nhận thuật toán phân nhóm khách hàng nguy cơ bỏ hẹn | Khách hàng có `Recency = 75 ngày`, `Frequency = 2`, `Monetary = 650.000` | Trả về chính xác phân khúc `RfmSegment.AtRisk`. | **PASSED** ✅ |
| 4 | `CreateAppointment_ShouldFail_WhenEndTimeIsBeforeOrEqualStartTime` | Kiểm tra tính hợp lệ thứ tự thời gian | Khung giờ vô lý: Bắt đầu 10:00, Kết thúc 09:30 | `IsSuccess = false`, bắt lỗi trước khi truy vấn CSDL. | **PASSED** ✅ |
| 5 | `CalculateRFM_ShouldReturnChampions_WhenCustomerVisitsFrequently` | Xác nhận thuật toán xếp hạng khách hàng VIP | `Recency = 5 ngày`, `Frequency = 6 lần`, `Monetary = 3.500.000` | Trả về phân khúc `RfmSegment.Champions`. | **PASSED** ✅ |
| 6-11 | `HasOverlapAsync_ShouldCorrectlyDetectOverlaps` (Theory) | Kiểm thử tích hợp thuật toán giao thoa trên EF Core In-Memory | 6 trường hợp: Chồng đầu, chồng đuôi, nằm trong, bao trùm, chạm mép trước/sau | 100% các trường hợp trả về đúng `true`/`false`. | **PASSED** ✅ |

**Kết quả thực thi lệnh `dotnet test` trên Terminal:**
```
Test run for .../SalonCare.Tests/bin/Debug/net8.0/SalonCare.Tests.dll (.NETCoreApp,Version=v8.0)
Passed!  - Failed: 0, Passed: 11, Skipped: 0, Total: 11, Duration: 400 ms
```

---

### 3.2.5. Đánh giá Kết quả Tuần 03 và Kế hoạch Tiếp theo (Tuần 04)
- **Kết quả đạt được (W02/W03):**
  - Hoàn thành 100% kiến trúc C4 (Context, Container), 3 tài liệu ADR chuẩn Michael Nygard và lược đồ ERD.
  - Xây dựng thành công mã nguồn "Hello World" đầu-cuối chuẩn Clean Layered Architecture trên .NET 8.0 và PostgreSQL 16.
  - Hiện thực thành công thuật toán kiểm tra chống trùng lịch KTV ở cả 2 cấp độ (Application LINQ & Database Unique Index).
  - Hoàn thiện giao diện Razor View / Bootstrap 5 trực quan, có đầy đủ dữ liệu mẫu (Seed Data) và thông báo lỗi.
  - Xây dựng bộ kiểm thử 11 test cases xUnit / Moq đạt tỷ lệ thành công 100%.
- **Kế hoạch triển khai Tuần 04:**
  - Tích hợp Zalo Cloud OpenAPI SDK (gửi tin nhắn mẫu ZNS nhắc lịch T-24h và T-2h).
  - Xây dựng Endpoint Webhook tiếp nhận sự kiện khách hàng bấm nút "Xác nhận lịch hẹn" trên ứng dụng Zalo.
  - Hoàn thiện Background Service tính toán điểm số RFM tự động hàng đêm.
