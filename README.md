
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

