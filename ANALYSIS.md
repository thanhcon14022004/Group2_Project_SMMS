# BÁO CÁO PHÂN TÍCH HỆ THỐNG VÀ PHÂN CHIA CÔNG VIỆC DỰ ÁN SMMS
> **Dự án**: SMMS (Student Mark Management System) — Hệ thống Quản lý Điểm & Phân bổ Lớp học Sinh viên  
> **Môn học**: PRN232 — Lập trình Web với ASP.NET Core & C#  
> **Kiến trúc**: N-Tier Architecture (ASP.NET Core 8 Web API + ASP.NET Core MVC Razor Views)  

---

## MỤC LỤC
1. [Tổng quan Kiến trúc Dự án](#1-tổng-quan-kiến-trúc-dự-án)
2. [Phân tích Các Thành phần Cốt lõi & Phân hệ Nghiệp vụ](#2-phân-tích-các-thành-phần-cốt-lõi--phân-hệ-nghiệp-vụ)
3. [Phân chia Công việc Cho Nhóm 6 Thành viên](#3-phân-chia-công-việc-cho-nhóm-6-thành-viên)
4. [Phân tích Vai trò & Quyền hạn của các Role (Admin, Lecturer, Student)](#4-phân-tích-vai-trò--quyền-hạn-của-các-role)
5. [Bảng Ma trận Phân quyền (Permission Matrix)](#5-bảng-ma-trận-phân-quyền-permission-matrix)
6. [Kinh nghiệm & Hướng dẫn Báo cáo Bảo vệ (Defense Tips)](#6-kinh-nghiệm--hướng-dẫn-báo-cáo-bảo-vệ-defense-tips)

---

## 1. TỔNG QUAN KIẾN TRÚC DỰ ÁN

Hệ thống được thiết kế theo mô hình tách biệt 2 project chính chạy độc lập và giao tiếp qua giao thức RESTful API:

```
project_PRN232.sln
├── project_PRN232.Backend/        # [PROJECT 1] ASP.NET Core 8 Web API
│   ├── Controllers/               # 17 API Controllers
│   ├── Services/                  # 9 Business Services xử lý logic tập trung
│   ├── Data/                      # AppDbContext, DbInitializer, SQL Script
│   ├── Models/Entities/           # 19 Entity Tables
│   ├── DTOs/                      # Data Transfer Objects
│   └── Program.cs                 # Pipeline, DI, JWT Auth, Swagger
│
├── project_PRN232.Frontend/       # [PROJECT 2] ASP.NET Core MVC (Razor Views)
│   ├── Controllers/               # 12 MVC Controllers
│   ├── Views/                     # Razor Views (Bootstrap 5, FontAwesome)
│   ├── Services/ApiClientService  # HTTP Client truyền Bearer JWT Token
│   └── Models/ViewModels          # Các ViewModel hiển thị giao diện
│
└── project_PRN232.Tests/          # [PROJECT 3] Dự án kiểm thử tự động xUnit
    └── SMMSBusinessTests.cs       # 5 bộ Unit Test nghiệp vụ cốt lõi
```

---

## 2. PHÂN TÍCH CÁC THÀNH PHẦN CỐT LÕI & PHÂN HỆ NGHIỆP VỤ

### 2.1. Backend Web API (Cổng 5050)
- **Công nghệ**: .NET 8 Web API, Entity Framework Core 8 Code-First, SQL Server.
- **Xác thực**: JWT Bearer Token, mã hóa mật khẩu bằng BCrypt.
- **9 Services nghiệp vụ chính**:
  1. `StudentAllocationService`: Thuật toán tự động xếp sinh viên vào lớp học phần theo lớp hành chính, kiểm tra chuẩn 28–31 SV/lớp, môn tiên quyết, xử lý lớp đầy.
  2. `GradeService`: Nhập điểm thành phần, tính điểm tổng kết `Σ(điểm × trọng số)`, làm tròn 1 chữ số thập phân, tính GPA thang 10 theo tín chỉ.
  3. `CurriculumService`: Cấu hình chương trình đào tạo theo ngành, kỳ học, ngành hẹp và phiên bản.
  4. `SemesterClosingService`: Kiểm tra các điều kiện tiên quyết trước khi kết thúc học kỳ.
  5. `ClassTransferService`: Xử lý sinh viên gửi đơn xin chuyển lớp hành chính.
  6. `ReportService`: Tổng hợp báo cáo kết quả học tập, GPA, thống kê sĩ số.
  7. `AuthService`: Đăng nhập, cấp phát JWT Token và phân quyền theo Role.
  8. `AuditLogService`: Ghi nhận lịch sử thao tác quan trọng (sửa điểm, mở/khóa điểm, chuyển lớp).
  9. `BackupRestoreService`: Sao lưu và phục hồi dữ liệu hệ thống.

### 2.2. Frontend Web Client (Cổng 3000)
- **Công nghệ**: ASP.NET Core MVC, Razor Views, Bootstrap 5, FontAwesome.
- **Cơ chế**: Toàn bộ thao tác nghiệp vụ được Frontend Controller gửi qua `ApiClientService` (chứa Header `Authorization: Bearer <token>`) sang Backend Web API để lấy dữ liệu.

### 2.3. Cơ sở dữ liệu (Database - 19 Bảng)
1. **Người dùng**: `Users`, `Students`, `Lecturers`.
2. **Đào tạo**: `Majors`, `Specializations`, `Subjects`, `SubjectGradeComponents`, `CurriculumConfigs`, `CurriculumSubjects`, `PrerequisiteSubjects`.
3. **Lớp học & Học kỳ**: `Semesters`, `AdministrativeClasses`, `CourseSections`, `CourseSectionSchedules`.
4. **Xếp lớp & Điểm**: `CourseSectionStudents`, `StudentGrades`.
5. **Vận hành**: `ClassTransferRequests`, `SpecializationSelections`, `AuditLogs`, `BackupRecords`.

---

## 3. PHÂN CHIA CÔNG VIỆC CHO NHÓM 6 THÀNH VIÊN

Mỗi thành viên phụ trách **trọn vẹn một module Fullstack (Entity -> Service -> Web API -> Controller & Razor View Frontend)**:

```
┌────────────────────────────────────────────────────────────────────────┐
│                        PHÂN CHIA 6 THÀNH VIÊN                          │
├──────────┬─────────────────────────────────────┬───────────────────────┤
│ TV 1     │ Leader, Auth, RBAC & Dashboard      │ Khung kiến trúc       │
│ TV 2     │ Module Phân Bổ Sinh Viên & Lớp Học  │ Module thuật toán lõi │
│ TV 3     │ Module Quản Lý Điểm & Tính GPA      │ Module nghiệp vụ điểm │
│ TV 4     │ Module CTĐT, Môn Học & Tiên Quyết   │ Module cấu hình dữ liệu│
│ TV 5     │ Module Học Kỳ, Giảng Viên & Chuyển Lớp│ Module vận hành kỳ │
│ TV 6     │ Đóng Kỳ, Báo Cáo, Test & Backup     │ Module QA & Tổng hợp  │
└──────────┴─────────────────────────────────────┴───────────────────────┘
```

### 👤 Thành viên 1: Trưởng nhóm (Architecture, Auth, User & Dashboard)
- **Nội dung phụ trách**:
  - Khởi tạo kiến trúc Solution, cấu hình JWT Middleware, CORS, Swagger.
  - Phân quyền theo Role (Admin, Lecturer, Student).
  - Quản lý tài khoản người dùng (kích hoạt / vô hiệu hóa).
  - Giao diện Dashboard tổng quan và Navigation Layout.
- **File Backend**:
  - `Program.cs`, `Controllers/AuthController.cs`, `Controllers/UsersController.cs`
  - `Services/AuthService.cs`, `Models/Entities/User.cs`
- **File Frontend**:
  - `Controllers/AccountController.cs`, `Controllers/DashboardController.cs`, `Controllers/BaseController.cs`
  - `Views/Account/Login.cshtml`, `Views/Dashboard/Index.cshtml`, `Views/Shared/_Layout.cshtml`

---

### 👤 Thành viên 2: Phân bổ Sinh viên & Quản lý Lớp học (nhánh `hunglt`)
- **Nội dung phụ trách**:
  - Quản lý Lớp hành chính (`AdministrativeClass`) và Lớp học phần (`CourseSection`).
  - Xây dựng thuật toán phân bổ sinh viên vào các lớp học phần tương ứng theo lớp hành chính của từng mùa.
  - Xử lý ràng buộc sĩ số chuẩn (28–31 SV/lớp), tự động sinh sinh viên nếu thiếu, xử lý lớp đầy và xung đột lịch.
  - Can thiệp thủ công: Xếp lớp hoặc hủy sinh viên khỏi lớp học phần.
- **File Backend**:
  - `Controllers/AllocationsController.cs`, `Controllers/AdministrativeClassesController.cs`, `Controllers/CourseSectionsController.cs`
  - `Services/StudentAllocationService.cs`, `Models/Entities/CourseAndSemester.cs`, `Models/Entities/PeopleAndClass.cs`
- **File Frontend**:
  - `Controllers/ClassesController.cs`, `Controllers/CourseSectionsController.cs`
  - `Views/Classes/Allocations.cshtml`, `Views/Classes/Index.cshtml`, `Views/CourseSections/Index.cshtml`

---

### 👤 Thành viên 3: Quản lý Điểm số, Đánh giá & Tính GPA
- **Nội dung phụ trách**:
  - Quản lý quy trình nhập điểm thành phần của giảng viên cho từng lớp học phần.
  - Thuật toán tính điểm tổng kết môn học: `Σ (Điểm thành phần × Tỷ trọng)`.
  - Quy tắc làm tròn điểm 1 chữ số thập phân (VD: 7.26 -> 7.3) để xét đạt môn (≥ 5.0).
  - Thuật toán tính điểm trung bình tích lũy **GPA** thang điểm 10 theo số tín chỉ.
  - Tính năng khóa điểm / mở khóa điểm của Admin.
- **File Backend**:
  - `Controllers/GradesController.cs`, `Services/GradeService.cs`, `Models/Entities/GradeAndOperational.cs`
- **File Frontend**:
  - `Controllers/GradesController.cs`
  - `Views/Grades/Index.cshtml`, `Views/Grades/Transcript.cshtml`

---

### 👤 Thành viên 4: Chương trình Đào tạo (CTĐT), Môn học & Tiên quyết
- **Nội dung phụ trách**:
  - Quản lý danh mục Ngành đào tạo (`Majors`) và Ngành hẹp (`Specializations`).
  - Quản lý Môn học (`Subjects`) và Cấu hình đầu điểm (`SubjectGradeComponents`): kiểm tra tổng trọng số bắt buộc đúng 100%.
  - Cấu hình Chương trình đào tạo linh hoạt theo dữ liệu (không hard-code), quản lý phiên bản CTĐT.
  - Cấu hình môn học tiên quyết (`PrerequisiteSubjects`).
- **File Backend**:
  - `Controllers/MajorsController.cs`, `Controllers/SubjectsController.cs`, `Controllers/CurriculumsController.cs`
  - `Services/CurriculumService.cs`, `Models/Entities/Curriculum.cs`, `Models/Entities/Subject.cs`, `Models/Entities/Major.cs`
- **File Frontend**:
  - `Controllers/MajorsController.cs`, `Controllers/SubjectsController.cs`, `Controllers/CurriculumsController.cs`
  - `Views/Majors/Index.cshtml`, `Views/Subjects/Index.cshtml`, `Views/Curriculums/Index.cshtml`

---

### 👤 Thành viên 5: Quản lý Học kỳ, Giảng viên, Chuyển lớp & Ngành hẹp
- **Nội dung phụ trách**:
  - Quản lý Học kỳ (`Semesters`): ngày bắt đầu, ngày kết thúc, thời hạn nhập điểm, đợt chuyển lớp.
  - Quản lý hồ sơ Giảng viên (`Lecturers`) và phân công giảng viên cho từng lớp học phần.
  - Xử lý quy trình Sinh viên gửi đơn xin chuyển lớp hành chính (`ClassTransferRequests`), kiểm tra thời hạn và điều kiện lớp đích.
  - Xử lý đợt Sinh viên chọn ngành hẹp (`Specializations`) khi kết thúc học kỳ 4.
- **File Backend**:
  - `Controllers/SemestersController.cs`, `Controllers/LecturersController.cs`, `Controllers/StudentsController.cs`, `Controllers/ClassTransfersController.cs`
  - `Services/ClassTransferService.cs`
- **File Frontend**:
  - `Controllers/SemestersController.cs`, `Controllers/OperationsController.cs`
  - `Views/Semesters/Index.cshtml`, `Views/Classes/Transfers.cshtml`

---

### 👤 Thành viên 6: Đóng kỳ, Báo cáo, Nhật ký, Sao lưu & Unit Tests
- **Nội dung phụ trách**:
  - Quản lý logic Đóng học kỳ (`Semester Closing`): kiểm tra điều kiện đóng kỳ (thiếu điểm, chưa phân công GV, thiếu phân bổ).
  - Báo cáo thống kê kết quả học tập, GPA, danh sách sinh viên ngoại lệ chưa được xếp lớp.
  - Nhật ký kiểm toán hệ thống (`AuditLogs`), sao lưu và khôi phục CSDL (`Backup & Restore`).
  - Quản lý và tối ưu hóa Script CSDL SQL Server (`SMMS_Database.sql`, `DatabaseFontFixer.cs`).
  - Xây dựng bộ kiểm thử tự động xUnit (`project_PRN232.Tests`).
- **File Backend**:
  - `Controllers/SemesterClosingController.cs`, `Controllers/ReportsController.cs`, `Controllers/OperationsController.cs`
  - `Services/SemesterClosingService.cs`, `Services/ReportService.cs`, `Services/AuditLogService.cs`, `Services/BackupRestoreService.cs`
  - `project_PRN232.Tests/SMMSBusinessTests.cs`, `SMMS_Database.sql`
- **File Frontend**:
  - `Controllers/ReportsController.cs`, `Controllers/OperationsController.cs`
  - `Views/Reports/Index.cshtml`, `Views/Operations/AuditLogs.cshtml`, `Views/Operations/Backup.cshtml`

---

## 4. PHÂN TÍCH VAI TRÒ & QUYỀN HẠN CỦA CÁC ROLE

### 4.1. 🛡️ ADMIN (Quản trị viên / Đào tạo)
- **Toàn quyền thiết lập hệ thống**:
  - Quản lý ngành, môn học, đầu điểm và cấu hình CTĐT.
  - Mở lớp hành chính, mở lớp học phần theo mùa học.
  - Chạy thuật toán phân bổ sinh viên vào lớp học phần tự động.
  - Phân công giảng viên giảng dạy cho từng lớp học phần.
  - Duyệt đơn xin chuyển lớp hành chính của sinh viên.
  - Kiểm soát kỳ thi: Khóa / mở khóa đợt nhập điểm.
  - Kiểm tra điều kiện và thực hiện đóng học kỳ.
  - Quản lý tài khoản (kích hoạt/khóa), xem nhật ký thao tác và sao lưu CSDL.

### 4.2. 👨‍🏫 TEACHER / LECTURER (Giảng viên)
- **Phạm vi quyền hạn giới hạn theo lớp phụ trách**:
  - Xem danh sách các lớp học phần được phân công giảng dạy trong học kỳ (không xem được lớp của GV khác).
  - Xem danh sách sinh viên thuộc lớp học phần phụ trách.
  - Xem cấu hình tỷ trọng đầu điểm của môn học áp dụng cho lớp.
  - Nhập điểm thành phần (Assignment, Lab, Quiz, PE,...) trong thời hạn cho phép.
  - **Giới hạn**: Không thể sửa điểm khi Admin đã khóa điểm hoặc học kỳ đã đóng; không thể tự ý sửa CTĐT hay đổi phân công giảng viên.

### 4.3. 🎓 STUDENT (Sinh viên)
- **Quyền hạn tra cứu và gửi yêu cầu học vụ**:
  - Xem hồ sơ cá nhân, CTĐT toàn khóa của ngành mình.
  - Xem thời khóa biểu và danh sách lớp học phần đã được phân bổ trong học kỳ.
  - Xem bảng điểm cá nhân chi tiết (điểm thành phần, điểm tổng kết, kết quả Passed/Failed, GPA thang 10).
  - Chọn chuyên ngành hẹp khi hoàn thành kỳ 4 để xếp lớp kỳ 5+.
  - Gửi đơn xin chuyển lớp hành chính trong đợt quy định.
  - **Giới hạn**: Không tự ý đăng ký/hủy lớp học phần (việc xếp lớp do hệ thống phân bổ theo CTĐT).

---

## 5. BẢNG MA TRẬN PHÂN QUYỀN (PERMISSION MATRIX)

| Chức năng hệ thống | 🛡️ Admin | 👨‍🏫 Lecturer | 🎓 Student |
|---|:---:|:---:|:---:|
| **Quản lý Ngành, CTĐT, Môn học, Đầu điểm** |  Toàn quyền |  Không |  Chỉ xem CTĐT |
| **Tạo Lớp hành chính & Lớp học phần** |  Toàn quyền |  Không |  Không |
| **Chạy thuật toán phân bổ sinh viên** |  Toàn quyền |  Không |  Không |
| **Xem danh sách lớp học phần** |  Tất cả các lớp |  Chỉ lớp được phân công |  Chỉ lớp mình học |
| **Nhập điểm thành phần** |  Toàn quyền can thiệp |  Chỉ lớp phụ trách |  Không |
| **Khóa / Mở khóa đợt nhập điểm** |  Duy nhất Admin |  Không |  Không |
| **Xem bảng điểm & GPA** |  Xem toàn trường |  Xem lớp mình dạy |  Chỉ xem của bản thân |
| **Gửi đơn xin chuyển lớp hành chính** |  Duyệt / Từ chối đơn |  Không |  Gửi yêu cầu |
| **Chọn chuyên ngành hẹp (Kỳ 4 -> 5)** |  Mở đợt & Cấu hình |  Không |  Thực hiện chọn |
| **Đóng học kỳ & Xử lý ngoại lệ** |  Thực hiện |  Không |  Không |
| **Quản lý Tài khoản, Audit Log, Backup** |  Toàn quyền |  Không |  Không |

---

## 6. KINH NGHIỆM & HƯỚNG DẪN BÁO CÁO BẢO VỆ (DEFENSE TIPS)

1. **Chuẩn bị Database**:
   - Chạy script `SMMS_Database.sql` để tạo CSDL LocalDB với 19 bảng và đầy đủ seed data.
   - Trình bày rõ ràng cấu hình connection string hỗ trợ `MyCNN` / `DefaultConnection`.
2. **Kịch bản Demo ấn tượng (Demo Flow)**:
   - **Bước 1**: Đăng nhập Admin -> Xem Dashboard thống kê -> Xem cấu hình Môn học & CTĐT (chứng minh không hard-code).
   - **Bước 2**: Vào Quản lý lớp -> Chạy **Phân bổ sinh viên vào lớp học phần** (nhấn mạnh chuẩn sĩ số 28–31 SV/lớp).
   - **Bước 3**: Đăng nhập tài khoản Giảng viên (`he170001`) -> Mở lớp được phân công -> Tiến hành nhập điểm thành phần -> Hệ thống tự tính điểm tổng kết.
   - **Bước 4**: Đăng nhập tài khoản Sinh viên (`he170002`) -> Xem bảng điểm cá nhân và GPA cập nhật tức thì.
   - **Bước 5**: Quay lại Admin -> Khóa điểm -> Thử đăng nhập lại Giảng viên để chứng minh hệ thống ngăn chặn sửa điểm thành công.
3. **Phân chia câu hỏi**:
   - Mỗi thành viên bám sát bảng phân công ở Mục 3, tự tin giải thích từ Database Entity -> Service xử lý logic -> API Endpoint -> View Razor.

