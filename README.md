# SMMS (Student Mark Management System) — Hệ Thống Quản Lý Điểm Sinh Viên
> Dự án môn học PRN232 — Kiến trúc tách biệt: **Backend** (ASP.NET Core Web API) & **Frontend** (Vue 3 SPA).

---

## 1. CẤU TRÚC SOLUTION & 2 PROJECT TRONG VISUAL STUDIO

Giải pháp (Solution) hiển thị đầy đủ 2 project chính trong Visual Studio Solution Explorer:

```
project_PRN232.slnx / project_PRN232.sln
├── project_PRN232.Backend/              # [PROJECT 1] ASP.NET Core 8 Web API
│   ├── Controllers/                     # API Controllers (Auth, Majors, Subjects, Curriculums...)
│   ├── Data/                            # AppDbContext & DbInitializer (Seed Data)
│   ├── DTOs/                            # Data Transfer Objects
│   ├── Migrations/                      # EF Core Code-First Migrations
│   ├── Models/Entities/                 # Các thực thể dữ liệu nghiệp vụ
│   ├── Services/                        # Xử lý nghiệp vụ tập trung (Allocation, Grade, Curriculum...)
│   ├── Properties/launchSettings.json   # Cấu hình cổng chạy backend (http://localhost:5050)
│   ├── appsettings.json                 # Cấu hình ConnectionString và JWT
│   ├── Program.cs                       # Pipeline, DI, JWT Auth, CORS, Swagger
│   ├── project_PRN232.Backend.csproj    # File Project Backend
│   └── report.docx                      # Tài liệu đặc tả nghiệp vụ gốc
│
├── project_PRN232.Frontend/             # [PROJECT 2] ASP.NET Core Web Client & SPA
│   ├── wwwroot/                         # Chứa toàn bộ tài nguyên giao diện
│   │   ├── css/app.css                  # Giao diện quản lý giáo dục hiện đại
│   │   ├── js/app.js                    # Logic Vue 3 SPA, gọi API backend qua JWT
│   │   └── index.html                   # 21 màn hình chức năng theo yêu cầu
│   ├── Properties/launchSettings.json   # Cấu hình cổng chạy frontend (http://localhost:3000)
│   ├── Program.cs                       # Web server phục vụ SPA tĩnh
│   ├── project_PRN232.Frontend.csproj   # File Project Frontend
│   ├── package.json                     # Hỗ trợ chạy cả với npm / node
│   └── server.js                        # Node.js dev server tích hợp
│
└── project_PRN232.Tests/                # [PROJECT 3] Dự án kiểm thử xUnit
    ├── SMMSBusinessTests.cs             # 5 bộ kiểm thử nghiệp vụ then chốt
    └── project_PRN232.Tests.csproj      # File Project Tests
```

---

## 2. HƯỚNG DẪN KHỞI CHẠY TRÊN VISUAL STUDIO (F5)

Trong **Visual Studio Solution Explorer**, bạn sẽ thấy 3 project:
- **`project_PRN232.Backend`**
- **`project_PRN232.Frontend`**
- **`project_PRN232.Tests`**

### Cách 1: Chạy đồng thời cả 2 project bằng 1 phím F5 (Multiple Startup Projects)
1. Trong Visual Studio, click chuột phải vào **Solution 'project_PRN232'** -> chọn **Configure Startup Projects...**
2. Chọn **Multiple startup projects**:
   - Đối với `project_PRN232.Backend`: Chọn Action = **Start**
   - Đối với `project_PRN232.Frontend`: Chọn Action = **Start**
3. Nhấn **OK**, sau đó nhấn **F5** (hoặc nút **Start** trên thanh công cụ).
4. Visual Studio sẽ tự động mở đồng thời cả 2 trang:
   - **Backend API (Swagger):** `http://localhost:5050/swagger`
   - **Frontend UI (SMMS):** `http://localhost:3000`

### Cách 2: Khởi chạy từ Terminal / PowerShell

**Terminal 1 - Chạy Backend:**
```powershell
dotnet run --project backend/project_PRN232.Backend.csproj
```
*(Backend chạy tại `http://localhost:5050`, tự động khởi tạo database LocalDB và Seed Data).*

**Terminal 2 - Chạy Frontend:**
```powershell
dotnet run --project frontend/project_PRN232.Frontend.csproj
```
*(Frontend chạy tại `http://localhost:3000`, tự động mở trình duyệt giao diện SMMS).*

```powershell
cd frontend
npm start
```
*(hoặc `npm run dev`)*

- **Frontend App URL:** `http://localhost:3000`
- *Giao diện Frontend tự động kết nối với Backend API tại `http://localhost:5050/api`.*
- *Ngoài ra, bạn cũng có thể mở trực tiếp file `frontend/index.html` trên trình duyệt hoặc qua Live Server trong VS Code.*

---

## 3. FILE DATABASE SCRIPT (SQL SERVER)

File script CSDL được tạo sẵn tại:
- **[SMMS_Database.sql](file:///d:/Ki8/PRN232/project_PRN232/SMMS_Database.sql)** (thư mục gốc dự án)
- **[backend/Data/SMMS_Database.sql](file:///d:/Ki8/PRN232/project_PRN232/backend/Data/SMMS_Database.sql)**

### Cách thực thi file database:
- **Cách 1 (SSMS):** Mở SQL Server Management Studio hoặc Azure Data Studio -> Mở file `SMMS_Database.sql` -> Nhấn **Execute (F5)**.
- **Cách 2 (Terminal):**
  ```powershell
  sqlcmd -S "(localdb)\mssqllocaldb" -E -i SMMS_Database.sql
  ```
- *Đặc điểm:* Script tự động tạo database `SMMS_PRN232`, tạo toàn bộ 19 bảng kèm khóa chính, khóa ngoại, chỉ mục và nạp toàn bộ dữ liệu mẫu (Seed Data) chuẩn mực. Đã có kiểm tra idempotent an toàn, không lo lỗi trùng khóa khi chạy lại.

---

## 4. CHẠY BỘ KIỂM THỬ NGHIỆP VỤ (UNIT TESTS)

Tại thư mục gốc, chạy lệnh:

```powershell
dotnet test
```

Tất cả 5/5 test case nghiệp vụ then chốt đều đạt 100%:
1. `Test_GradeCalculation_RoundsToOneDecimalPlace_And_DeterminesPass`: Làm tròn điểm tổng kết 1 chữ số thập phân (7.26 $\to$ 7.3) và đạt môn ($\ge 5.0$).
2. `Test_GpaCalculation_IncludesFailedCourses_OnTenScale`: Tính GPA thang 10 theo tín chỉ và **tính cả môn trượt**.
3. `Test_Curriculum_RequiresSpecialization_FromSemester5`: Bắt buộc ngành hẹp từ học kỳ 5+ nếu ngành có phân nhánh.
4. `Test_Allocation_BlocksStudent_WhenPrerequisiteNotMet`: Chặn xếp lớp khi chưa đạt môn tiên quyết ($< 5.0$).
5. `Test_DistinguishZeroFromUnentered`: Phân biệt điểm 0.0 với điểm chưa nhập (`null`).

---

## 4. TÀI KHOẢN THỬ NGHIỆM SẴN CÓ

Dữ liệu mẫu chuẩn hóa đã được cài đặt sẵn:

| Vai trò | Tên đăng nhập | Mật khẩu | Chức danh / Thông tin thử nghiệm |
| ------- | ------------- | -------- | --------------------------------- |
| **Admin** | `admin` | `Admin@123` | Quản trị viên toàn quyền hệ thống |
| **Giảng viên** | `gv_hungnv` | `Admin@123` | ThS. Nguyễn Văn Hùng (Kỹ thuật phần mềm) |
| **Giảng viên** | `gv_huonglt` | `Admin@123` | TS. Lê Thị Hương (Hệ thống thông tin) |
| **Sinh viên** | `he170001` | `Admin@123` | Trần Quang Minh (Lớp SE1701, Kỳ 5, Ngành hẹp SE-IS) |
| **Sinh viên** | `he170002` | `Admin@123` | Lê Hoàng Nam (Lớp SE1701, Kỳ 5, Ngành hẹp SE-JS) |
| **Sinh viên** | `he170003` | `Admin@123` | Phạm Mai Anh (Lớp SE1702, Kỳ 4) |

*(Trên màn hình đăng nhập của Frontend có sẵn 3 nút **Đăng nhập nhanh 1-Click** cho từng vai trò).*

---

## 5. DANH SÁCH 21 MÀN HÌNH CHỨC NĂNG (FRONTEND)

1. **Đăng nhập (Login):** JWT Token, kiểm tra tài khoản Active, đăng nhập nhanh 1-click.
2. **Dashboard:** Thống kê tổng quan sinh viên, giảng viên, lớp HP, GPA trung bình, cảnh báo lớp thiếu điểm/thiếu GV, biểu đồ cột phân phối học lực.
3. **Quản lý Tài khoản:** Tìm kiếm, lọc theo vai trò, kích hoạt/vô hiệu hóa an toàn, thêm tài khoản.
4. **Quản lý Ngành đào tạo:** Danh sách ngành, thêm/sửa ngành (SE, IA...).
5. **Quản lý Ngành hẹp:** Danh sách ngành hẹp theo ngành (SE-IS, SE-JS...), thêm ngành hẹp.
6. **Quản lý Môn học:** Danh mục môn học, số tín chỉ, mô tả, trạng thái hoạt động.
7. **Quản lý Đầu điểm của Môn:** Cấu hình đầu điểm động, tự động tính tổng tỷ trọng, **chặn lưu nếu khác 100%**, đánh dấu cờ Admin-only.
8. **Điều kiện Tiên quyết:** Gán môn tiên quyết cho môn học và điểm tối thiểu yêu cầu ($\ge 5.0$).
9. **Cấu hình Chương trình Đào tạo:** Tạo và quản lý phiên bản CTTĐT, chọn môn học sẵn có.
10. **Tra cứu Cấu hình & Mở lớp HP tự động:** Nhập lớp hành chính và học kỳ, bấm "Tra cứu cấu hình", hệ thống hiển thị toàn bộ môn học, tín chỉ, đầu điểm và cho phép sinh hàng loạt lớp học phần tự động.
11. **Quản lý Lớp Hành chính:** Danh sách lớp niên chế, ngành, khóa, sĩ số hiện tại, xem danh sách sinh viên.
12. **Danh sách Lớp Học phần:** Quản lý sức chứa, sĩ số thực tế, lịch học và phòng học.
13. **Phân công Giảng viên:** Gán hoặc đổi giảng viên phụ trách với cơ chế chuyển quyền tức thì, bảo lưu điểm số.
14. **Phân bổ Sinh viên:** Giao diện chạy thuật toán xếp lớp tự động; hiển thị chi tiết số sinh viên được xếp và danh sách ngoại lệ kèm nguyên nhân (thiếu tiên quyết, lớp đầy, trùng lịch).
15. **Bảng điểm Lớp Học phần (Ma trận):** Dành cho Giảng viên/Admin nhập điểm từng thành phần cho sinh viên; tự động tính điểm tổng kết và kết quả Đạt/Trượt theo thời gian thực.
16. **Khóa & Mở khóa Điểm:** Quyền quản trị cho Admin khóa/mở khóa bảng điểm kèm ghi nhật ký.
17. **Xem Điểm & GPA Sinh viên:** Bảng điểm học kỳ, tổng số tín chỉ tích lũy, GPA thang 10, xếp loại học lực.
18. **Yêu cầu Chuyển Lớp Hành chính:** Sinh viên kiểm tra tính hợp lệ và gửi đơn; Admin duyệt hoặc từ chối có kèm lý do.
19. **Kiểm tra & Kết thúc Học kỳ:** Checklist tự động quét lỗi (lớp thiếu GV, thiếu điểm, điểm chưa khóa); cho phép kết thúc ngoại lệ có lý do bắt buộc.
20. **Báo cáo & Thống kê:** Bảng điểm GPA toàn trường, xuất dữ liệu ra file CSV hỗ trợ Excel.
21. **Nhật ký & Sao lưu / Khôi phục:** Xem toàn bộ Audit Log hệ thống; Tạo bản sao lưu định kỳ; Khôi phục database với xác nhận bảo mật `"RESTORE"`.
