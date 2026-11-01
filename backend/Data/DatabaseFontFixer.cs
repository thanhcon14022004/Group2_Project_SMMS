using Microsoft.EntityFrameworkCore;
using project_PRN232.Models.Entities;

namespace project_PRN232.Data
{
    public static class DatabaseFontFixer
    {
        public static async Task FixMojibakeAsync(AppDbContext db)
        {
            try
            {
                // 1. Chuẩn hóa tên 41 môn học chuyên ngành hẹp và môn cốt lõi
                var subjectNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["PRJ302"] = "Lập trình Java Spring Boot & Microservices",
                    ["PRJ321"] = "Kiến trúc phần mềm phân tán Cloud Native Java",
                    ["NWN301"] = "Lập trình Web Backend với NodeJS & NestJS",
                    ["NWN302"] = "Ứng dụng Realtime, GraphQL & Microservices với NodeJS",
                    ["GAM201"] = "Thiết kế & Lập trình Game 2D với Unity",
                    ["GAM301"] = "Lập trình Game 3D với Unreal Engine & C++",
                    ["SAP201"] = "Lập trình ABAP & Kiến trúc hệ thống SAP ERP",
                    ["SAP301"] = "Tích hợp giải pháp doanh nghiệp với SAP S/4HANA",
                    ["BSE201"] = "Tiếng Nhật chuyên ngành CNTT (IT Japanese N3)",
                    ["BSE301"] = "Đặc tả thiết kế phần mềm chuẩn Nhật Bản & Quản lý BrSE",
                    ["BSE202"] = "Tiếng Hàn chuyên ngành CNTT & Kỹ sư Cầu nối Hàn Quốc",
                    ["BSE302"] = "Quản trị dự án CNTT với doanh nghiệp Hàn Quốc",
                    ["AIL303"] = "Học sâu ứng dụng & Mạng nơ-ron tích chập (Deep Learning)",
                    ["AIL304"] = "Học tăng cường & Triển khai mô hình MLOps",
                    ["AIC201"] = "Thị giác máy tính & Xử lý hình ảnh kỹ thuật số (Computer Vision)",
                    ["AIN201"] = "Xử lý ngôn ngữ tự nhiên & Mô hình ngôn ngữ lớn (NLP & LLMs)",
                    ["SEC302"] = "Kỹ thuật tấn công & Đánh giá an ninh mạng (Penetration Testing)",
                    ["SEC303"] = "An toàn điện toán đám mây & DevSecOps",
                    ["FOR201"] = "Điều tra chứng cứ số & Phân tích pháp y số (Digital Forensics)",
                    ["FOR301"] = "Ứng phó sự cố an ninh & Phân tích mã độc (Malware Analysis)",
                    ["BDA301"] = "Phân tích dữ liệu kinh doanh dự báo (Predictive Analytics)",
                    ["BDE201"] = "Xây dựng đường ống dữ liệu với Spark & Kafka",
                    ["BDE301"] = "Hồ dữ liệu & Kho dữ liệu đám mây (Cloud Data Lake)",
                    ["ITW301"] = "Phát triển ứng dụng Web Full-stack nâng cao",
                    ["CSA301"] = "Kiến trúc hạ tầng đám mây AWS/Azure & Kubernetes",
                    ["ANM301"] = "Hoạt họa nhân vật 3D & Kỹ xảo chuyển động",
                    ["UIX301"] = "Hệ thống thiết kế (Design System) & Nghiên cứu UX",
                    ["MKD301"] = "Tiếp thị số đa kênh & Thương mại điện tử",
                    ["FNT301"] = "Hệ sinh thái Công nghệ tài chính FinTech & Blockchain",
                    ["HOS301"] = "Quản trị Resort, Du lịch & Dịch vụ cao cấp",
                    ["LOG301"] = "Quản trị Logistics & Chuỗi cung ứng toàn cầu",
                    ["PRE301"] = "Chiến lược Quan hệ công chúng PR & Quản trị sự kiện",
                    ["JOU301"] = "Báo chí đa nền tảng & Sản xuất truyền hình số",
                    ["ENB201"] = "Đàm phán thương mại & Thư tín tiếng Anh chuyên nghiệp",
                    ["ENT301"] = "Biên phiên dịch hội thảo & Dịch thuật chuyên sâu",
                    ["JIT201"] = "Tiếng Nhật chuyên ngành Công nghệ thông tin",
                    ["JBC201"] = "Tiếng Nhật thương mại & Nghi thức công sở Nhật Bản",
                    ["KRB201"] = "Tiếng Hàn thương mại & Đàm phán kinh doanh",
                    ["KRT301"] = "Biên phiên dịch tiếng Hàn cao cấp",
                    ["MKT301"] = "Quản trị thương hiệu & Chiến lược Marketing số",
                    ["IBT301"] = "Thanh toán quốc tế & Thủ tục Hải quan xuất nhập khẩu"
                };

                var subjects = await db.Subjects.ToListAsync();
                bool subjectsChanged = false;
                foreach (var s in subjects)
                {
                    if (subjectNames.TryGetValue(s.Code, out var correctName))
                    {
                        if (s.Name != correctName)
                        {
                            s.Name = correctName;
                            subjectsChanged = true;
                        }
                    }
                    else if (s.Name.Contains("Ã") || s.Name.Contains("á»") || s.Name.Contains("áº") || s.Name.Contains("Ä"))
                    {
                        // Sửa lỗi mojibake cơ bản nếu còn sót
                        s.Name = CleanMojibake(s.Name);
                        subjectsChanged = true;
                    }
                }
                if (subjectsChanged)
                {
                    await db.SaveChangesAsync();
                }

                // 2. Chuẩn hóa tên toàn bộ CurriculumConfigs
                var configs = await db.CurriculumConfigs
                    .Include(c => c.Major)
                    .Include(c => c.Specialization)
                    .ToListAsync();

                bool configsChanged = false;
                foreach (var cfg in configs)
                {
                    string targetName;
                    string targetDesc;

                    if (cfg.MajorId == null && cfg.SemesterNumber == 6)
                    {
                        targetName = "Chương trình Thực tập doanh nghiệp (OJT)";
                        targetDesc = "Chương trình OJT dùng chung cho tất cả các ngành";
                    }
                    else if (cfg.Specialization != null)
                    {
                        var specName = cfg.Specialization.Name;
                        if (specName.Contains("Ã") || specName.Contains("á»")) specName = CleanMojibake(specName);
                        targetName = $"Ngành {cfg.Major?.Code ?? ""} - {specName} (Kỳ {cfg.SemesterNumber})";
                        targetDesc = $"Chương trình đào tạo chuyên ngành hẹp Kỳ {cfg.SemesterNumber}";
                    }
                    else
                    {
                        targetName = $"Chương trình {cfg.Major?.Code ?? ""} - Kỳ {cfg.SemesterNumber}";
                        targetDesc = $"Chương trình đào tạo cơ sở ngành Kỳ {cfg.SemesterNumber}";
                    }

                    if (cfg.Name != targetName || cfg.Description != targetDesc)
                    {
                        cfg.Name = targetName;
                        cfg.Description = targetDesc;
                        configsChanged = true;
                    }
                }
                if (configsChanged)
                {
                    await db.SaveChangesAsync();
                }

                // 3. Chuẩn hóa tên CourseSections (Môn học - Lớp)
                var sections = await db.CourseSections
                    .Include(cs => cs.Subject)
                    .Include(cs => cs.AdministrativeClass)
                    .ToListAsync();

                bool sectionsChanged = false;
                foreach (var cs in sections)
                {
                    if (cs.Subject != null)
                    {
                        var classCode = cs.AdministrativeClass?.Code;
                        if (string.IsNullOrEmpty(classCode))
                        {
                            var parts = cs.Code.Split('_');
                            if (parts.Length >= 3) classCode = parts[2];
                        }

                        var correctSectionName = !string.IsNullOrEmpty(classCode)
                            ? $"{cs.Subject.Name} - {classCode}"
                            : cs.Subject.Name;

                        if (cs.Name != correctSectionName)
                        {
                            cs.Name = correctSectionName;
                            sectionsChanged = true;
                        }
                    }
                }
                if (sectionsChanged)
                {
                    await db.SaveChangesAsync();
                }

                // 4. Chuẩn hóa font chữ và bảng mã tiếng Việt trong file SMMS_Database.sql
                CleanSqlScriptFile();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi tự động sửa lỗi phông chữ / bảng mã: " + ex.Message);
            }
        }

        public static void CleanSqlScriptFile()
        {
            try
            {
                var cur = new DirectoryInfo(Directory.GetCurrentDirectory());
                FileInfo? sqlFile = null;
                while (cur != null)
                {
                    var path = Path.Combine(cur.FullName, "SMMS_Database.sql");
                    if (File.Exists(path))
                    {
                        sqlFile = new FileInfo(path);
                        break;
                    }
                    cur = cur.Parent;
                }

                if (sqlFile == null || !sqlFile.Exists) return;

                var text = File.ReadAllText(sqlFile.FullName, System.Text.Encoding.UTF8);

                var map = new Dictionary<string, string>
                {
                    ["Há»† THá» NG QUáº¢N LĂ  Ä Iá»‚M SINH VIĂŠN"] = "HỆ THỐNG QUẢN LÝ ĐIỂM SINH VIÊN",
                    ["MĂ”N Há»ŒC: PRN232"] = "MÔN HỌC: PRN232",
                    ["Táº O CÆ  Sá»ž Dá»® LIá»†U & Náº P Dá»® LIá»†U MáºªU CHUáº¨N"] = "TẠO CƠ SỞ DỮ LIỆU & NẠP DỮ LIỆU MẪU CHUẨN",
                    ["Há»— trá»£:"] = "Hỗ trợ:",
                    ["TĂªn CSDL:"] = "Tên CSDL:",
                    ["CĂ¡ch cháº¡y:"] = "Cách chạy:",
                    ["CĂ¡ch 1: Má»Ÿ SQL Server Management Studio (SSMS) hoáº·c Azure Data Studio -> Nháº¥n F5 (Execute)."] = "Cách 1: Mở SQL Server Management Studio (SSMS) hoặc Azure Data Studio -> Nhấn F5 (Execute).",
                    ["CĂ¡ch 2: Cháº¡y lá»‡nh terminal:"] = "Cách 2: Chạy lệnh terminal:",
                    ["DANH SĂ CH TĂ€I KHOáº¢N MáºªU KHá»žI Táº O (Máº­t kháº©u chung cho táº¥t cáº£: Admin@123):"] = "DANH SÁCH TÀI KHOẢN MẪU KHỞI TẠO (Mật khẩu chung cho tất cả: Admin@123):",
                    ["1. Quáº£n trá»‹ viĂªn (Admin):"] = "1. Quản trị viên (Admin):",
                    ["Quáº£n trá»‹ toĂ n há»‡ thá»‘ng"] = "Quản trị toàn hệ thống",
                    ["2. Giáº£ng viĂªn (Lecturer):"] = "2. Giảng viên (Lecturer):",
                    ["ThS. Nguyá»…n VÄƒn HĂ¹ng - Giáº£ng viĂªn PRN232"] = "ThS. Nguyễn Văn Hùng - Giảng viên PRN232",
                    ["TS. LĂª Thá»‹ HÆ°Æ¡ng - Giáº£ng viĂªn SWP391"] = "TS. Lê Thị Hương - Giảng viên SWP391",
                    ["3. Sinh viĂªn (Student):"] = "3. Sinh viên (Student):",
                    ["Tráº§n Quang Minh - Lá»›p SE1701, Ká»³ 5 SE-IS"] = "Trần Quang Minh - Lớp SE1701, Kỳ 5 SE-IS",
                    ["LĂª HoĂ ng Nam    - Lá»›p SE1701, Ká»³ 5 SE-JS"] = "Lê Hoàng Nam    - Lớp SE1701, Kỳ 5 SE-JS",
                    ["Pháº¡m Mai Anh   - Lá»›p SE1702, Ká»³ 4 Ä ang chá» n CN"] = "Phạm Mai Anh   - Lớp SE1702, Kỳ 4 Đang chọn CN",
                    ["LÆ¯U Ă  LĂ€M Má»šI (RESET DATABASE):"] = "LƯU Ý LÀM MỚI (RESET DATABASE):",
                    ["Náº¿u báº¡n muá»‘n xĂ³a sáº¡ch cÆ¡ sá»Ÿ dá»¯ liá»‡u cÅ© Ä‘á»ƒ táº¡o láº¡i tá»« Ä‘áº§u, hĂ£y bá»  comment 2 dĂ²ng sau:"] = "Nếu bạn muốn xóa sạch cơ sở dữ liệu cũ để tạo lại từ đầu, hãy bỏ comment 2 dòng sau:",
                    ["==> Táº¡o má»›i cÆ¡ sá»Ÿ dá»¯ liá»‡u SMMS_PRN232..."] = "==> Tạo mới cơ sở dữ liệu SMMS_PRN232...",
                    ["==> Ä ang sá»­ dá»¥ng cÆ¡ sá»Ÿ dá»¯ liá»‡u SMMS_PRN232..."] = "==> Đang sử dụng cơ sở dữ liệu SMMS_PRN232...",
                    ["Ká»¹ thuáº­t pháº§n má» m (Software Engineering)"] = "Kỹ thuật phần mềm (Software Engineering)",
                    ["Ä Ă o táº¡o ká»¹ sÆ° pháº§n má» m cháº¥t lÆ°á»£ng cao"] = "Đào tạo kỹ sư phần mềm chất lượng cao",
                    ["An toĂ n thĂ´ng tin (Information Assurance)"] = "An toàn thông tin (Information Assurance)",
                    ["Ä Ă o táº¡o chuyĂªn gia báº£o máº­t vĂ  phĂ²ng thá»§ khĂ´ng gian máº¡ng"] = "Đào tạo chuyên gia bảo mật và phòng thủ không gian mạng",
                    ["==> Ä Ă£ náº¡p 2 báº£n ghi máº«u vĂ o báº£ng Majors"] = "==> Đã nạp 2 bản ghi mẫu vào bảng Majors",
                    ["==> Báº£ng Majors Ä‘Ă£ cĂ³ dá»¯ liá»‡u, bá»  qua bÆ°á»›c náº¡p máº«u"] = "==> Bảng Majors đã có dữ liệu, bỏ qua bước nạp mẫu",
                    ["Náº¡p dá»¯ liá»‡u máº«u cho báº£ng: Specializations"] = "Nạp dữ liệu mẫu cho bảng: Specializations",
                    ["Há»‡ thá»‘ng thĂ´ng tin doanh nghiá»‡p (Information Systems)"] = "Hệ thống thông tin doanh nghiệp (Information Systems)",
                    ["ChuyĂªn sĂ¢u vá»  kiáº¿n trĂºc há»‡ thá»‘ng vĂ  ERP"] = "Chuyên sâu về kiến trúc hệ thống và ERP",
                    ["Ká»¹ thuáº­t pháº§n má» m Nháº­t Báº£n (Japanese Software)"] = "Kỹ thuật phần mềm Nhật Bản (Japanese Software)",
                    ["ChuyĂªn sĂ¢u vá»  cáº§u ná»‘i phĂ¡t triá»ƒn pháº§n má» m cho thá»‹ trÆ°á» ng Nháº­t"] = "Chuyên sâu về cầu nối phát triển phần mềm cho thị trường Nhật",
                    ["==> Ä Ă£ náº¡p 2 báº£n ghi máº«u vĂ o báº£ng Specializations"] = "==> Đã nạp 2 bản ghi mẫu vào bảng Specializations",
                    ["==> Báº£ng Specializations Ä‘Ă£ cĂ³ dá»¯ liá»‡u, bá»  qua bÆ°á»›c náº¡p máº«u"] = "==> Bảng Specializations đã có dữ liệu, bỏ qua bước nạp mẫu",
                    ["Náº¡p dá»¯ liá»‡u máº«u cho báº£ng: Subjects"] = "Nạp dữ liệu mẫu cho bảng: Subjects",
                    ["Nháº­p mĂ´n láº­p trĂ¬nh C"] = "Nhập môn lập trình C",
                    ["Láº­p trĂ¬nh hÆ°á»›ng Ä‘á»‘i tÆ°á»£ng Java"] = "Lập trình hướng đối tượng Java",
                    ["Láº­p trĂ¬nh C# cÆ¡ báº£n & Windows Apps"] = "Lập trình C# cơ bản & Windows Apps",
                    ["XĂ¢y dá»±ng á»©ng dá»¥ng Web vá»›i ASP.NET Core"] = "Xây dựng ứng dụng Web với ASP.NET Core",
                    ["Dá»± Ă¡n phĂ¡t triá»ƒn pháº§n má» m thá»±c chiáº¿n"] = "Dự án phát triển phần mềm thực chiến",
                    ["==> Ä Ă£ náº¡p 5 báº£n ghi máº«u vĂ o báº£ng Subjects"] = "==> Đã nạp 5 bản ghi mẫu vào bảng Subjects",
                    ["==> Báº£ng Subjects Ä‘Ă£ cĂ³ dá»¯ liá»‡u, bá»  qua bÆ°á»›c náº¡p máº«u"] = "==> Bảng Subjects đã có dữ liệu, bỏ qua bước nạp mẫu",
                    ["Náº¡p dá»¯ liá»‡u máº«u cho báº£ng: SubjectGradeComponents"] = "Nạp dữ liệu mẫu cho bảng: SubjectGradeComponents",
                    ["==> Ä Ă£ náº¡p 16 Ä‘áº§u Ä‘iá»ƒm máº«u cho 5 mĂ´n há» c"] = "==> Đã nạp 16 đầu điểm mẫu cho 5 môn học",
                    ["==> Báº£ng SubjectGradeComponents Ä‘Ă£ cĂ³ dá»¯ liá»‡u, bá»  qua"] = "==> Bảng SubjectGradeComponents đã có dữ liệu, bỏ qua",
                    ["Náº¡p dá»¯ liá»‡u máº«u cho báº£ng: PrerequisiteSubjects"] = "Nạp dữ liệu mẫu cho bảng: PrerequisiteSubjects",
                    ["==> Ä Ă£ náº¡p 3 Ä‘iá» u kiá»‡n tiĂªn quyáº¿t máº«u"] = "==> Đã nạp 3 điều kiện tiên quyết mẫu",
                    ["==> Báº£ng PrerequisiteSubjects Ä‘Ă£ cĂ³ dá»¯ liá»‡u, bá»  qua"] = "==> Bảng PrerequisiteSubjects đã có dữ liệu, bỏ qua",
                    ["Náº¡p dá»¯ liá»‡u máº«u cho báº£ng: Users"] = "Nạp dữ liệu mẫu cho bảng: Users",
                    ["Quáº£n trá»‹ viĂªn Há»‡ thá»‘ng SMMS"] = "Quản trị viên Hệ thống SMMS",
                    ["ThS. Nguyá»…n VÄƒn HĂ¹ng"] = "ThS. Nguyễn Văn Hùng",
                    ["TS. LĂª Thá»‹ HÆ°Æ¡ng"] = "TS. Lê Thị Hương",
                    ["Tráº§n Quang Minh"] = "Trần Quang Minh",
                    ["LĂª HoĂ ng Nam"] = "Lê Hoàng Nam",
                    ["Pháº¡m Mai Anh"] = "Phạm Mai Anh",
                    ["==> Ä Ă£ náº¡p 6 tĂ i khoáº£n máº«u (1 Admin, 2 Lecturer, 3 Student)"] = "==> Đã nạp 6 tài khoản mẫu (1 Admin, 2 Lecturer, 3 Student)",
                    ["==> Báº£ng Users Ä‘Ă£ cĂ³ dá»¯ liá»‡u, bá»  qua"] = "==> Bảng Users đã có dữ liệu, bỏ qua",
                    ["Náº¡p dá»¯ liá»‡u máº«u cho báº£ng: Lecturers"] = "Nạp dữ liệu mẫu cho bảng: Lecturers",
                    ["Khoa Ká»¹ thuáº­t Pháº§n má» m"] = "Khoa Kỹ thuật Phần mềm",
                    ["==> Ä Ă£ náº¡p thĂ´ng tin 2 giáº£ng viĂªn máº«u"] = "==> Đã nạp thông tin 2 giảng viên mẫu",
                    ["==> Báº£ng Lecturers Ä‘Ă£ cĂ³ dá»¯ liá»‡u, bá»  qua"] = "==> Bảng Lecturers đã có dữ liệu, bỏ qua",
                    ["Náº¡p dá»¯ liá»‡u máº«u cho báº£ng: AdministrativeClasses"] = "Nạp dữ liệu mẫu cho bảng: AdministrativeClasses",
                    ["Ká»¹ thuáº­t pháº§n má» m K17 - Lá»›p 01"] = "Kỹ thuật phần mềm K17 - Lớp 01",
                    ["Ká»¹ thuáº­t pháº§n má» m K17 - Lá»›p 02"] = "Kỹ thuật phần mềm K17 - Lớp 02",
                    ["==> Ä Ă£ náº¡p 2 lá»›p hĂ nh chĂ­nh máº«u"] = "==> Đã nạp 2 lớp hành chính mẫu",
                    ["==> Báº£ng AdministrativeClasses Ä‘Ă£ cĂ³ dá»¯ liá»‡u, bá»  qua"] = "==> Bảng AdministrativeClasses đã có dữ liệu, bỏ qua",
                    ["Náº¡p dá»¯ liá»‡u máº«u cho báº£ng: Students"] = "Nạp dữ liệu mẫu cho bảng: Students",
                    ["==> Ä Ă£ náº¡p há»“ sÆ¡ 3 sinh viĂªn máº«u"] = "==> Đã nạp hồ sơ 3 sinh viên mẫu",
                    ["==> Báº£ng Students Ä‘Ă£ cĂ³ dá»¯ liá»‡u, bá»  qua"] = "==> Bảng Students đã có dữ liệu, bỏ qua",
                    ["Náº¡p dá»¯ liá»‡u máº«u cho báº£ng: Semesters"] = "Nạp dữ liệu mẫu cho bảng: Semesters",
                    ["Há» c ká»³ Fall 2024"] = "Học kỳ Fall 2024",
                    ["Há» c ká»³ Summer 2024"] = "Học kỳ Summer 2024",
                    ["Há» c ká»³ Spring 2025"] = "Học kỳ Spring 2025",
                    ["==> Ä Ă£ náº¡p 3 há» c ká»³ máº«u"] = "==> Đã nạp 3 học kỳ mẫu",
                    ["==> Báº£ng Semesters Ä‘Ă£ cĂ³ dá»¯ liá»‡u, bá»  qua"] = "==> Bảng Semesters đã có dữ liệu, bỏ qua",
                    ["Náº¡p dá»¯ liá»‡u máº«u cho báº£ng: CourseSections"] = "Nạp dữ liệu mẫu cho bảng: CourseSections",
                    ["PRN232 - Lá»›p 01"] = "PRN232 - Lớp 01",
                    ["SWP391 - Lá»›p 01"] = "SWP391 - Lớp 01",
                    ["==> Ä Ă£ náº¡p 2 lá»›p há» c pháº§n máº«u"] = "==> Đã nạp 2 lớp học phần mẫu",
                    ["==> Báº£ng CourseSections Ä‘Ă£ cĂ³ dá»¯ liá»‡u, bá»  qua"] = "==> Bảng CourseSections đã có dữ liệu, bỏ qua",
                    ["Náº¡p dá»¯ liá»‡u máº«u cho báº£ng: CourseSectionSchedules"] = "Nạp dữ liệu mẫu cho bảng: CourseSectionSchedules",
                    ["==> Ä Ă£ náº¡p lá»‹ch há» c cho cĂ¡c lá»›p há» c pháº§n"] = "==> Đã nạp lịch học cho các lớp học phần",
                    ["==> Báº£ng CourseSectionSchedules Ä‘Ă£ cĂ³ dá»¯ liá»‡u, bá»  qua"] = "==> Bảng CourseSectionSchedules đã có dữ liệu, bỏ qua",
                    ["Náº¡p dá»¯ liá»‡u máº«u cho báº£ng: CourseSectionStudents"] = "Nạp dữ liệu mẫu cho bảng: CourseSectionStudents",
                    ["==> Ä Ă£ ghi danh sinh viĂªn vĂ o cĂ¡c lá»›p há» c pháº§n máº«u"] = "==> Đã ghi danh sinh viên vào các lớp học phần mẫu",
                    ["==> Báº£ng CourseSectionStudents Ä‘Ă£ cĂ³ dá»¯ liá»‡u, bá»  qua"] = "==> Bảng CourseSectionStudents đã có dữ liệu, bỏ qua",
                    ["Náº¡p dá»¯ liá»‡u máº«u cho báº£ng: StudentGrades"] = "Nạp dữ liệu mẫu cho bảng: StudentGrades",
                    ["Ä iá»ƒm kiá»ƒm tra tiáº¿n Ä‘á»™ dá»± Ă¡n giai Ä‘oáº¡n 1"] = "Điểm kiểm tra tiến độ dự án giai đoạn 1",
                    ["Ä iá»ƒm Ä‘Ă¡nh giĂ¡ quĂ¡ trĂ¬nh"] = "Điểm đánh giá quá trình",
                    ["Ä iá»ƒm tiáº¿n Ä‘á»™ dá»± Ă¡n SWP391"] = "Điểm tiến độ dự án SWP391",
                    ["==> Ä Ă£ náº¡p cĂ¡c Ä‘áº§u Ä‘iá»ƒm khá»Ÿi táº¡o máº«u"] = "==> Đã nạp các đầu điểm khởi tạo mẫu",
                    ["==> Báº£ng StudentGrades Ä‘Ă£ cĂ³ dá»¯ liá»‡u, bá»  qua"] = "==> Bảng StudentGrades đã có dữ liệu, bỏ qua",
                    ["Náº¡p dá»¯ liá»‡u máº«u cho báº£ng: AuditLogs"] = "Nạp dữ liệu mẫu cho bảng: AuditLogs",
                    ["Khá»Ÿi táº¡o cÆ¡ sá»Ÿ dá»¯ liá»‡u vĂ  náº¡p dá»¯ liá»‡u máº«u thĂ nh cĂ´ng"] = "Khởi tạo cơ sở dữ liệu và nạp dữ liệu mẫu thành công",
                    ["==> Ä Ă£ náº¡p 1 báº£n ghi nháº­t kĂ½ há»‡ thá»‘ng khá»Ÿi táº¡o"] = "==> Đã nạp 1 bản ghi nhật ký hệ thống khởi tạo",
                    ["==> Báº£ng AuditLogs Ä‘Ă£ cĂ³ dá»¯ liá»‡u, bá»  qua"] = "==> Bảng AuditLogs đã có dữ liệu, bỏ qua",
                    ["==> Báº®T Ä áº¦U Náº P Dá»® LIá»†U Ä áº¦Y Ä á»¦ CHÆ¯Æ NG TRĂŒNH Ä Ă€O Táº O Ä áº I Há»ŒC FPT..."] = "==> BẮT ĐẦU NẠP DỮ LIỆU ĐẦY ĐỦ CHƯƠNG TRÌNH ĐÀO TẠO ĐẠI HỌC FPT...",
                    ["1. NGĂ€NH Ä Ă€O Táº O (MAJORS)"] = "1. NGÀNH ĐÀO TẠO (MAJORS)",
                    ["TrĂ­ tuá»‡ nhĂ¢n táº¡o (Artificial Intelligence)"] = "Trí tuệ nhân tạo (Artificial Intelligence)",
                    ["Ä Ă o táº¡o chuyĂªn gia AI, Machine Learning vĂ  Khoa há» c dá»¯ liá»‡u"] = "Đào tạo chuyên gia AI, Machine Learning và Khoa học dữ liệu",
                    ["Khoa há» c dá»¯ liá»‡u (Data Science)"] = "Khoa học dữ liệu (Data Science)",
                    ["Ä Ă o táº¡o ká»¹ sÆ° phĂ¢n tĂ­ch dá»¯ liá»‡u lá»›n vĂ  trá»±c quan hĂ³a kinh doanh"] = "Đào tạo kỹ sư phân tích dữ liệu lớn và trực quan hóa kinh doanh",
                    ["CĂ´ng nghá»‡ thĂ´ng tin (Information Technology)"] = "Công nghệ thông tin (Information Technology)",
                    ["Ä Ă o táº¡o ká»¹ sÆ° CNTT tá»•ng há»£p, Cloud vĂ  Quáº£n trá»‹ há»‡ thá»‘ng"] = "Đào tạo kỹ sư CNTT tổng hợp, Cloud và Quản trị hệ thống",
                    ["Thiáº¿t káº¿ Má»¹ thuáº­t sá»‘ (Digital Art & Graphic Design)"] = "Thiết kế Mỹ thuật số (Digital Art & Graphic Design)",
                    ["Ä Ă o táº¡o thiáº¿t káº¿ Ä‘á»“ há» a 2D/3D, UI/UX vĂ  truyá» n thĂ´ng hĂ¬nh áº£nh"] = "Đào tạo thiết kế đồ họa 2D/3D, UI/UX và truyền thông hình ảnh",
                    ["Quáº£n trá»‹ kinh doanh (Business Administration)"] = "Quản trị kinh doanh (Business Administration)",
                    ["Ä Ă o táº¡o cá»­ nhĂ¢n QTKD, Digital Marketing vĂ  Kinh doanh quá»‘c táº¿"] = "Đào tạo cử nhân QTKD, Digital Marketing và Kinh doanh quốc tế",
                    ["Truyá» n thĂ´ng Ä‘a phÆ°Æ¡ng tiá»‡n (Multimedia Communication)"] = "Truyền thông đa phương tiện (Multimedia Communication)",
                    ["Ä Ă o táº¡o chuyĂªn viĂªn truyá» n thĂ´ng sá»‘, bĂ¡o chĂ­ vĂ  tá»• chá»©c sá»± kiá»‡n"] = "Đào tạo chuyên viên truyền thông số, báo chí và tổ chức sự kiện",
                    ["NgĂ´n ngá»¯ Anh (English Language)"] = "Ngôn ngữ Anh (English Language)",
                    ["Ä Ă o táº¡o chuyĂªn gia ngĂ´n ngá»¯ Anh thÆ°Æ¡ng máº¡i vĂ  biĂªn phiĂªn dá»‹ch"] = "Đào tạo chuyên gia ngôn ngữ Anh thương mại và biên phiên dịch",
                    ["NgĂ´n ngá»¯ Nháº­t (Japanese Language)"] = "Ngôn ngữ Nhật (Japanese Language)",
                    ["Ä Ă o táº¡o cá»­ nhĂ¢n tiáº¿ng Nháº­t CNTT vĂ  tiáº¿ng Nháº­t thÆ°Æ¡ng máº¡i"] = "Đào tạo cử nhân tiếng Nhật CNTT và tiếng Nhật thương mại",
                    ["NgĂ´n ngá»¯ HĂ n (Korean Language)"] = "Ngôn ngữ Hàn (Korean Language)",
                    ["Ä Ă o táº¡o cá»­ nhĂ¢n tiáº¿ng HĂ n thÆ°Æ¡ng máº¡i vĂ  truyá» n thĂ´ng"] = "Đào tạo cử nhân tiếng Hàn thương mại và truyền thông",
                    ["==> Ä Ă£ cáº­p nháº­t 11 ngĂ nh Ä‘Ă o táº¡o FPT."] = "==> Đã cập nhật 11 ngành đào tạo FPT.",
                    ["Láº¥y ID cĂ¡c ngĂ nh"] = "Lấy ID các ngành",
                    ["2. CHUYĂŠN NGĂ€NH Háº¸P (SPECIALIZATIONS)"] = "2. CHUYÊN NGÀNH HẸP (SPECIALIZATIONS)",
                    ["Há»‡ thá»‘ng nhĂºng & IoT (Embedded Systems & IoT)"] = "Hệ thống nhúng & IoT (Embedded Systems & IoT)",
                    ["Láº­p trĂ¬nh vi Ä‘iá» u khiá»ƒn, há»‡ thá»‘ng nhĂºng vĂ  thiáº¿t bá»‹ thĂ´ng minh"] = "Lập trình vi điều khiển, hệ thống nhúng và thiết bị thông minh",
                    ["Ká»¹ thuáº­t pháº§n má» m Ă´ tĂ´ (Automotive Software)"] = "Kỹ thuật phần mềm ô tô (Automotive Software)",
                    ["PhĂ¡t triá»ƒn pháº§n má» m Ä‘iá» u khiá»ƒn vĂ  xe tá»± hĂ nh"] = "Phát triển phần mềm điều khiển và xe tự hành",
                    ["An ninh máº¡ng & Ä iá»‡n toĂ¡n Ä‘Ă¡m mĂ¢y (Cyber & Cloud Security)"] = "An ninh mạng & Điện toán đám mây (Cyber & Cloud Security)",
                    ["Báº£o máº­t háº¡ táº§ng máº¡ng, phĂ²ng chá»‘ng táº¥n cĂ´ng DDoS"] = "Bảo mật hạ tầng mạng, phòng chống tấn công DDoS",
                    ["PhĂ¡p y ká»¹ thuáº­t sá»‘ & Ä iá» u tra tá»™i pháº¡m máº¡ng (Digital Forensics)"] = "Pháp y kỹ thuật số & Điều tra tội phạm mạng (Digital Forensics)",
                    ["Ä iá» u tra sá»± cá»‘, thu tháº­p chá»©ng cá»© sá»‘"] = "Điều tra sự cố, thu thập chứng cứ số",
                    ["Ká»¹ thuáº­t Dá»¯ liá»‡u lá»›n (Data Engineering)"] = "Kỹ thuật Dữ liệu lớn (Data Engineering)",
                    ["XĂ¢y dá»±ng Data Pipeline, ETL, Big Data Platform"] = "Xây dựng Data Pipeline, ETL, Big Data Platform",
                    ["PhĂ¢n tĂ­ch Dá»¯ liá»‡u Kinh doanh (Business Analytics)"] = "Phân tích Dữ liệu Kinh doanh (Business Analytics)",
                    ["PhĂ¢n tĂ­ch dá»± bĂ¡o, BI & visualization"] = "Phân tích dự báo, BI & visualization",
                    ["PhĂ¡t triá»ƒn á»¨ng dá»¥ng Web (Web Development)"] = "Phát triển Ứng dụng Web (Web Development)",
                    ["Full-stack web engineering vá»›i Cloud Native"] = "Full-stack web engineering với Cloud Native",
                    ["Kiáº¿n trĂºc & Váº­n hĂ nh Ä Ă¡m mĂ¢y (Cloud & DevOps)"] = "Kiến trúc & Vận hành Đám mây (Cloud & DevOps)",
                    ["Háº¡ táº§ng AWS/Azure, CI/CD vĂ  Kubernetes"] = "Hạ tầng AWS/Azure, CI/CD và Kubernetes",
                    ["Hoáº¡t hĂ¬nh 3D & Ká»¹ xáº£o VFX (3D Animation & VFX)"] = "Hoạt hình 3D & Kỹ xảo VFX (3D Animation & VFX)",
                    ["Thiáº¿t káº¿ Tráº£i nghiá»‡m ngÆ°á» i dĂ¹ng (UI/UX Design)"] = "Thiết kế Trải nghiệm người dùng (UI/UX Design)",
                    ["Digital Marketing & ThÆ°Æ¡ng máº¡i Ä‘iá»‡n tá»­ (Digital Marketing & E-Commerce)"] = "Digital Marketing & Thương mại điện tử (Digital Marketing & E-Commerce)",
                    ["CĂ´ng nghá»‡ TĂ i chĂ­nh (FinTech)"] = "Công nghệ Tài chính (FinTech)",
                    ["Quáº£n trá»‹ KhĂ¡ch sáº¡n & Du lá»‹ch (Hospitality Management)"] = "Quản trị Khách sạn & Du lịch (Hospitality Management)",
                    ["Logistics & Chuá»—i cung á»©ng toĂ n cáº§u (Logistics & SCM)"] = "Logistics & Chuỗi cung ứng toàn cầu (Logistics & SCM)",
                    ["Quan há»‡ cĂ´ng chĂºng (Public Relations - PR)"] = "Quan hệ công chúng (Public Relations - PR)",
                    ["BĂ¡o chĂ­ Ä‘a phÆ°Æ¡ng tiá»‡n (Multimedia Journalism)"] = "Báo chí đa phương tiện (Multimedia Journalism)",
                    ["Tiáº¿ng Anh ThÆ°Æ¡ng máº¡i (Business English)"] = "Tiếng Anh Thương mại (Business English)",
                    ["BiĂªn phiĂªn dá»‹ch Tiáº¿ng Anh (English Translation & Interpreting)"] = "Biên phiên dịch Tiếng Anh (English Translation & Interpreting)",
                    ["Tiáº¿ng Nháº­t CNTT (IT Japanese)"] = "Tiếng Nhật CNTT (IT Japanese)",
                    ["Tiáº¿ng Nháº­t ThÆ°Æ¡ng máº¡i (Business Japanese)"] = "Tiếng Nhật Thương mại (Business Japanese)",
                    ["Tiáº¿ng HĂ n ThÆ°Æ¡ng máº¡i (Business Korean)"] = "Tiếng Hàn Thương mại (Business Korean)",
                    ["BiĂªn phiĂªn dá»‹ch Tiáº¿ng HĂ n (Korean Translation & Interpreting)"] = "Biên phiên dịch Tiếng Hàn (Korean Translation & Interpreting)",
                    ["==> Ä Ă£ cáº­p nháº­t 20 chuyĂªn ngĂ nh háº¹p FPT."] = "==> Đã cập nhật 20 chuyên ngành hẹp FPT.",
                    ["3. MĂ”N Há»ŒC Má»šI (SUBJECTS)"] = "3. MÔN HỌC MỚI (SUBJECTS)",
                    ["Cáº­p nháº­t cĂ¡c mĂ´n há» c chuyĂªn ngĂ nh vĂ  Ä‘á»“ Ă¡n tá»‘t nghiá»‡p FPT"] = "Cập nhật các môn học chuyên ngành và đồ án tốt nghiệp FPT",
                    ["Ká»¹ thuáº­t láº­p trĂ¬nh nĂ¢ng cao vá»›i Java"] = "Kỹ thuật lập trình nâng cao với Java",
                    ["Cáº¥u trĂºc dá»¯ liá»‡u & Giáº£i thuáº­t vá»›i Java"] = "Cấu trúc dữ liệu & Giải thuật với Java",
                    ["XĂ¢y dá»±ng á»©ng dá»¥ng Web Java vá»›i Spring Boot"] = "Xây dựng ứng dụng Web Java với Spring Boot",
                    ["CÆ¡ sá»Ÿ dá»¯ liá»‡u SQL Server"] = "Cơ sở dữ liệu SQL Server",
                    ["Há»‡ thá»‘ng cÆ¡ sá»Ÿ dá»¯ liá»‡u nĂ¢ng cao"] = "Hệ thống cơ sở dữ liệu nâng cao",
                    ["Dá»± Ă¡n phĂ¡t triá»ƒn pháº§n má» m - Lá»›p 01"] = "Dự án phát triển phần mềm - Lớp 01",
                    ["TrĂ­ tuá»‡ nhĂ¢n táº¡o & Machine Learning - Lá»›p 01"] = "Trí tuệ nhân tạo & Machine Learning - Lớp 01",
                    ["NguyĂªn lĂ½ Marketing - Lá»›p 01"] = "Nguyên lý Marketing - Lớp 01",
                    ["Thiáº¿t káº¿ giao diá»‡n UI/UX - Lá»›p 01"] = "Thiết kế giao diện UI/UX - Lớp 01",
                    ["Tiáº¿ng Nháº­t sÆ¡ cáº¥p 1 - Lá»›p 01"] = "Tiếng Nhật sơ cấp 1 - Lớp 01",
                    ["==> Ä Ă£ má»Ÿ cĂ¡c lá»›p há» c pháº§n ká»³ FA24."] = "==> Đã mở các lớp học phần kỳ FA24.",
                    ["PhĂ¢n bá»• tá»± Ä‘á»™ng sinh viĂªn vĂ o cĂ¡c lá»›p HP má»›i má»Ÿ vĂ  sinh Ä‘iá»ƒm máº«u"] = "Phân bổ tự động sinh viên vào các lớp HP mới mở và sinh điểm mẫu",
                    ["Láº¥y cĂ¡c Ä‘áº§u Ä‘iá»ƒm cá»§a PRN232"] = "Lấy các đầu điểm của PRN232",
                    ["==> Náº P Dá»® LIá»†U Ä áº I Há»ŒC FPT THĂ€NH CĂ”NG VĂ€ HOĂ€N Táº¤T!"] = "==> NẠP DỮ LIỆU ĐẠI HỌC FPT THÀNH CÔNG VÀ HOÀN TẤT!",
                    ["KIá»‚M TRA Tá»”NG Káº¾T Báº¢NG & Sá»  DĂ’NG Dá»® LIá»†U Ä Ăƒ Náº P"] = "KIỂM TRA TỔNG KẾT BẢNG & SỐ DÒNG DỮ LIỆU ĐÃ NẠP",
                    ["KHá»žI Táº O CÆ  Sá»ž Dá»® LIá»†U SMMS_PRN232 THĂ€NH CĂ”NG!"] = "KHỞI TẠO CƠ SỞ DỮ LIỆU SMMS_PRN232 THÀNH CÔNG!",
                    ["Báº£ng Dá»¯ Liá»‡u"] = "Bảng Dữ Liệu",
                    ["Sá»‘ Báº£n Ghi Máº«u"] = "Số Bản Ghi Mẫu",
                    ["DANH SĂ CH TĂ€I KHOáº¢N Ä Ä‚NG NHáº¬P MáºªU TRONG Há»† THá» NG (Pass: Admin@123)"] = "DANH SÁCH TÀI KHOẢN ĐĂNG NHẬP MẪU TRONG HỆ THỐNG (Pass: Admin@123)"
                };

                foreach (var kv in map)
                {
                    text = text.Replace(kv.Key, kv.Value);
                }

                // Bổ sung các cụm từ chuyên ngành và mô tả chi tiết
                var extendedMap = new Dictionary<string, string>
                {
                    ["An ninh máº¡ng & Ä iá»‡n toĂ¡n Ä‘Ă¡m mĂ¢y (Cyber & Cloud Security)"] = "An ninh mạng & Điện toán đám mây (Cyber & Cloud Security)",
                    ["Ä iá» u tra sá»‘ & á»¨ng phĂ³ sá»± cá»‘ an ninh (Digital Forensics)"] = "Điều tra số & Ứng phó sự cố an ninh (Digital Forensics)",
                    ["KhĂ´i phá»¥c chá»©ng cá»© sá»‘ vĂ  giĂ¡m Ä‘á»‹nh báº£o máº­t"] = "Khôi phục chứng cứ số và giám định bảo mật",
                    ["Há» c mĂ¡y & Há» c sĂ¢u á»©ng dá»¥ng (Machine Learning & Deep Learning)"] = "Học máy & Học sâu ứng dụng (Machine Learning & Deep Learning)",
                    ["XĂ¢y dá»±ng cĂ¡c mĂ´ hĂ¬nh AI dá»± bĂ¡o vĂ  huáº¥n luyá»‡n máº¡ng nÆ¡-ron"] = "Xây dựng các mô hình AI dự báo và huấn luyện mạng nơ-ron",
                    ["Thá»‹ giĂ¡c mĂ¡y tĂ­nh & Xá»­ lĂ½ ngĂ´n ngá»¯ tá»± nhiĂªn (Computer Vision & NLP)"] = "Thị giác máy tính & Xử lý ngôn ngữ tự nhiên (Computer Vision & NLP)",
                    ["Nháº­n diá»‡n khuĂ´n máº·t, chatbot vĂ  xá»­ lĂ½ vÄƒn báº£n"] = "Nhận diện khuôn mặt, chatbot và xử lý văn bản",
                    ["PhĂ¢n tĂ­ch dá»¯ liá»‡u kinh doanh (Business Analytics & BI)"] = "Phân tích dữ liệu kinh doanh (Business Analytics & BI)",
                    ["PhĂ¢n tĂ­ch insight khĂ¡ch hĂ ng vĂ  tá»‘i Æ°u chiáº¿n lÆ°á»£c kinh doanh"] = "Phân tích insight khách hàng và tối ưu chiến lược kinh doanh",
                    ["Ká»¹ thuáº­t dá»¯ liá»‡u lá»›n (Big Data Engineering)"] = "Kỹ thuật dữ liệu lớn (Big Data Engineering)",
                    ["XĂ¢y dá»±ng Data Pipeline, Spark, Hadoop vĂ  há»“ dá»¯ liá»‡u"] = "Xây dựng Data Pipeline, Spark, Hadoop và hồ dữ liệu",
                    ["PhĂ¡t triá»ƒn á»©ng dá»¥ng Web Full-stack (Full-stack Web Development)"] = "Phát triển ứng dụng Web Full-stack (Full-stack Web Development)",
                    ["Thiáº¿t káº¿ vĂ  phĂ¡t triá»ƒn web hiá»‡n Ä‘áº¡i vá»›i Frontend & Backend"] = "Thiết kế và phát triển web hiện đại với Frontend & Backend",
                    ["Quáº£n trá»‹ há»‡ thá»‘ng & Ä iá»‡n toĂ¡n Ä‘Ă¡m mĂ¢y (Cloud & Systems)"] = "Quản trị hệ thống & Điện toán đám mây (Cloud & Systems)",
                    ["Triá»ƒn khai AWS/Azure, CI/CD vĂ  DevOps"] = "Triển khai AWS/Azure, CI/CD và DevOps",
                    ["Thiáº¿t káº¿ Ä‘á»“ há» a 3D & Ká»¹ xáº£o hĂ¬nh áº£nh (3D Animation & VFX)"] = "Thiết kế đồ họa 3D & Kỹ xảo hình ảnh (3D Animation & VFX)",
                    ["Dá»±ng hĂ¬nh nhĂ¢n váº­t 3D, lĂ m hoáº¡t hĂ¬nh vĂ  ká»¹ xáº£o Ä‘iá»‡n áº£nh"] = "Dựng hình nhân vật 3D, làm hoạt hình và kỹ xảo điện ảnh",
                    ["Thiáº¿t káº¿ giao diá»‡n & Tráº£i nghiá»‡m ngÆ°á» i dĂ¹ng (UI/UX Design)"] = "Thiết kế giao diện & Trải nghiệm người dùng (UI/UX Design)",
                    ["NghiĂªn cá»©u hĂ nh vi ngÆ°á» i dĂ¹ng, thiáº¿t káº¿ wireframe vĂ  prototype"] = "Nghiên cứu hành vi người dùng, thiết kế wireframe và prototype",
                    ["Digital Marketing (Marketing sá»‘)"] = "Digital Marketing (Marketing số)",
                    ["SEO, Google/Facebook Ads, Content Strategy vĂ  Social Media"] = "SEO, Google/Facebook Ads, Content Strategy và Social Media",
                    ["Kinh doanh quá»‘c táº¿ (International Business)"] = "Kinh doanh quốc tế (International Business)",
                    ["ThÆ°Æ¡ng máº¡i toĂ n cáº§u, xuáº¥t nháº­p kháº©u vĂ  chuá»—i cung á»©ng logistics"] = "Thương mại toàn cầu, xuất nhập khẩu và chuỗi cung ứng logistics",
                    ["Váº­n hĂ nh khĂ¡ch sáº¡n, resort vĂ  dá»‹ch vá»¥ du lá»‹ch quá»‘c táº¿"] = "Vận hành khách sạn, resort và dịch vụ du lịch quốc tế",
                    ["CĂ´ng nghá»‡ tĂ i chĂ­nh (FinTech)"] = "Công nghệ tài chính (FinTech)",
                    ["NgĂ¢n hĂ ng sá»‘, thanh toĂ¡n Ä‘iá»‡n tá»­ vĂ  phĂ¢n tĂ­ch Ä‘áº§u tÆ°"] = "Ngân hàng số, thanh toán điện tử và phân tích đầu tư",
                    ["Quan há»‡ cĂ´ng chĂºng & Quáº£n trá»‹ sá»± kiá»‡n (PR & Event)"] = "Quan hệ công chúng & Quản trị sự kiện (PR & Event)",
                    ["Truyá» n thĂ´ng thÆ°Æ¡ng hiá»‡u, xá»­ lĂ½ khá»§ng hoáº£ng vĂ  tá»• chá»©c sá»± kiá»‡n"] = "Truyền thông thương hiệu, xử lý khủng hoảng và tổ chức sự kiện",
                    ["BĂ¡o chĂ­ sá»‘ & SĂ¡ng táº¡o ná»™i dung (Digital Journalism)"] = "Báo chí số & Sáng tạo nội dung (Digital Journalism)",
                    ["Sáº£n xuáº¥t video, podcast vĂ  bĂ¡o chĂ­ truyá» n thĂ´ng sá»‘"] = "Sản xuất video, podcast và báo chí truyền thông số",
                    ["Tiáº¿ng Anh thÆ°Æ¡ng máº¡i (Business Communication)"] = "Tiếng Anh thương mại (Business Communication)",
                    ["Giao tiáº¿p Ä‘Ă m phĂ¡n quá»‘c táº¿ vĂ  há»£p Ä‘á»“ng thÆ°Æ¡ng máº¡i"] = "Giao tiếp đàm phán quốc tế và hợp đồng thương mại",
                    ["Tiáº¿ng Anh BiĂªn phiĂªn dá»‹ch (Translation & Interpretation)"] = "Tiếng Anh Biên phiên dịch (Translation & Interpretation)",
                    ["BiĂªn dá»‹ch tĂ i liá»‡u chuyĂªn ngĂ nh vĂ  phiĂªn dá»‹ch há»™i tháº£o"] = "Biên dịch tài liệu chuyên ngành và phiên dịch hội thảo",
                    ["Tiáº¿ng Nháº­t CĂ´ng nghá»‡ thĂ´ng tin (IT Japanese)"] = "Tiếng Nhật Công nghệ thông tin (IT Japanese)",
                    ["Cáº§u ná»‘i phĂ¡t triá»ƒn pháº§n má» m Bridge SE cho doanh nghiá»‡p Nháº­t"] = "Cầu nối phát triển phần mềm Bridge SE cho doanh nghiệp Nhật",
                    ["Giao tiáº¿p vÄƒn phĂ²ng vĂ  thÆ°Æ¡ng máº¡i vá»›i Ä‘á»‘i tĂ¡c Nháº­t Báº£n"] = "Giao tiếp văn phòng và thương mại với đối tác Nhật Bản",
                    ["LĂ m viá»‡c táº¡i cĂ¡c táº­p Ä‘oĂ n Ä‘a quá»‘c gia HĂ n Quá»‘c (Samsung, LG...)"] = "Làm việc tại các tập đoàn đa quốc gia Hàn Quốc (Samsung, LG...)",
                    ["Tiáº¿ng HĂ n BiĂªn phiĂªn dá»‹ch (Korean Translation)"] = "Tiếng Hàn Biên phiên dịch (Korean Translation)",
                    ["Dá»‹ch thuáº­t cabin, há»™i nghá»‹ vĂ  tĂ i liá»‡u kinh táº¿"] = "Dịch thuật cabin, hội nghị và tài liệu kinh tế",
                    ["Láº­p trĂ¬nh vi Ä‘iá» u khiá»ƒn, há»‡ thá»‘ng nhĂºng vĂ  thiáº¿t bá»‹ thĂ´ng minh"] = "Lập trình vi điều khiển, hệ thống nhúng và thiết bị thông minh",
                    ["PhĂ¡t triá»ƒn pháº§n má» m Ä‘iá» u khiá»ƒn vĂ  xe tá»± hĂ nh"] = "Phát triển phần mềm điều khiển và xe tự hành",
                    ["Ä Ă o táº¡o"] = "Đào tạo",
                    ["chuyĂªn gia"] = "chuyên gia",
                    ["vĂ "] = "và",
                    ["Khoa há» c"] = "Khoa học",
                    ["dá»¯ liá»‡u"] = "dữ liệu",
                    ["ká»¹ sÆ°"] = "kỹ sư",
                    ["phĂ¢n tĂ­ch"] = "phân tích",
                    ["lá»›n"] = "lớn",
                    ["trá»±c quan hĂ³a"] = "trực quan hóa",
                    ["thĂ´ng tin"] = "thông tin",
                    ["tá»•ng há»£p"] = "tổng hợp",
                    ["Quáº£n trá»‹"] = "Quản trị",
                    ["há»‡ thá»‘ng"] = "hệ thống",
                    ["Thiáº¿t káº¿"] = "Thiết kế",
                    ["thiáº¿t káº¿"] = "thiết kế",
                    ["Ä‘á»“ há» a"] = "đồ họa",
                    ["truyá» n thĂ´ng"] = "truyền thông",
                    ["hĂ¬nh áº£nh"] = "hình ảnh",
                    ["cá»­ nhĂ¢n"] = "cử nhân",
                    ["quá»‘c táº¿"] = "quốc tế",
                    ["chuyĂªn viĂªn"] = "chuyên viên",
                    ["bĂ¡o chĂ­"] = "báo chí",
                    ["tá»• chá»©c"] = "tổ chức",
                    ["sá»± kiá»‡n"] = "sự kiện",
                    ["ngĂ´n ngá»¯"] = "ngôn ngữ",
                    ["thÆ°Æ¡ng máº¡i"] = "thương mại",
                    ["biĂªn phiĂªn dá»‹ch"] = "biên phiên dịch",
                    ["tiáº¿ng Nháº­t"] = "tiếng Nhật",
                    ["tiáº¿ng HĂ n"] = "tiếng Hàn",
                    ["Láº­p trĂ¬nh"] = "Lập trình",
                    ["hÆ°á»›ng Ä‘á»‘i tÆ°á»£ng"] = "hướng đối tượng",
                    ["cÆ¡ báº£n"] = "cơ bản",
                    ["XĂ¢y dá»±ng"] = "Xây dựng",
                    ["á»©ng dá»¥ng"] = "ứng dụng",
                    ["vá»›i"] = "với",
                    ["Dá»± Ă¡n"] = "Dự án",
                    ["phĂ¡t triá»ƒn"] = "phát triển",
                    ["pháº§n má» m"] = "phần mềm",
                    ["thá»±c chiáº¿n"] = "thực chiến",
                    ["Ä iá»ƒm"] = "Điểm",
                    ["quĂ¡ trĂ¬nh"] = "quá trình",
                    ["tiáº¿n Ä‘á»™"] = "tiến độ",
                    ["dá»± Ă¡n"] = "dự án",
                    ["lá»›p há» c pháº§n"] = "lớp học phần",
                    ["lá»‹ch há» c"] = "lịch học",
                    ["sinh viĂªn"] = "sinh viên",
                    ["giáº£ng viĂªn"] = "giảng viên",
                    ["Lá»›p"] = "Lớp",
                    ["Ká»³"] = "Kỳ",
                    ["mĂ´n há» c"] = "môn học",
                    ["Ä‘á»“ Ă¡n"] = "đồ án",
                    ["tá»‘t nghiá»‡p"] = "tốt nghiệp",
                    ["nĂ¢ng cao"] = "nâng cao",
                    ["Cáº¥u trĂºc"] = "Cấu trúc",
                    ["Giáº£i thuáº­t"] = "Giải thuật",
                    ["CÆ¡ sá»Ÿ"] = "Cơ sở",
                    ["chuyĂªn ngĂ nh"] = "chuyên ngành",
                    ["ngĂ nh"] = "ngành",
                    ["NgĂ nh"] = "Ngành",
                    ["Há» c ká»³"] = "Học kỳ",
                    ["CHUYĂŠN NGĂ€NH Háº¸P"] = "CHUYÊN NGÀNH HẸP",
                    ["Ä‘iá» u khiá»ƒn"] = "điều khiển",
                    ["nhĂºng"] = "nhúng",
                    ["thiáº¿t bá»‹ thĂ´ng minh"] = "thiết bị thông minh",
                    ["PhĂ¡t triá»ƒn"] = "Phát triển",
                    ["Ká»¹ thuáº­t pháº§n má» m Ă´ tĂ´"] = "Kỹ thuật phần mềm ô tô",
                    ["xe tá»± hĂ nh"] = "xe tự hành",
                    ["Ä iá»‡n toĂ¡n Ä‘Ă¡m mĂ¢y"] = "Điện toán đám mây",
                    ["Ä iá» u tra sá»‘ & á»¨ng phĂ³ sá»± cá»‘ an ninh"] = "Điều tra số & Ứng phó sự cố an ninh",
                    ["Há» c mĂ¡y & Há» c sĂ¢u"] = "Học máy & Học sâu",
                    ["Ká»¹ xáº£o"] = "Kỹ xảo",
                    ["giao diá»‡n & Tráº£i nghiá»‡m ngÆ°á» i dĂ¹ng"] = "giao diện & Trải nghiệm người dùng",
                    ["NghiĂªn cá»©u hĂ nh vi ngÆ°á» i dĂ¹ng"] = "Nghiên cứu hành vi người dùng",
                    ["Truyá» n thĂ´ng thÆ°Æ¡ng hiá»‡u, xá»­ lĂ½ khá»§ng hoáº£ng"] = "Truyền thông thương hiệu, xử lý khủng hoảng",
                    ["Giao tiáº¿p"] = "Giao tiếp",
                    ["vÄƒn phĂ²ng"] = "văn phòng",
                    ["Ä‘á»‘i tĂ¡c Nháº­t Báº£n"] = "đối tác Nhật Bản",
                    ["LĂ m viá»‡c táº¡i cĂ¡c táº­p Ä‘oĂ n Ä‘a quá»‘c gia HĂ n Quá»‘c"] = "Làm việc tại các tập đoàn đa quốc gia Hàn Quốc",
                    ["Dá»‹ch thuáº­t cabin, há»™i nghá»‹ vĂ  tĂ i liá»‡u kinh táº¿"] = "Dịch thuật cabin, hội nghị và tài liệu kinh tế",
                    ["BiĂªn dá»‹ch tĂ i liá»‡u"] = "Biên dịch tài liệu",
                    ["phiĂªn dá»‹ch há»™i tháº£o"] = "phiên dịch hội thảo",
                    ["Cáº§u ná»‘i phĂ¡t triá»ƒn pháº§n má» m"] = "Cầu nối phát triển phần mềm",
                    ["Ä‘Ă m phĂ¡n quá»‘c táº¿ vĂ  há»£p Ä‘á»“ng thÆ°Æ¡ng máº¡i"] = "đàm phán quốc tế và hợp đồng thương mại",
                    ["Sáº£n xuáº¥t video, podcast vĂ  bĂ¡o chĂ­"] = "Sản xuất video, podcast và báo chí",
                    ["Dá»±ng hĂ¬nh nhĂ¢n váº­t 3D, lĂ m hoáº¡t hĂ¬nh"] = "Dựng hình nhân vật 3D, làm hoạt hình",
                    ["Váº­n hĂ nh khĂ¡ch sáº¡n, resort"] = "Vận hành khách sạn, resort",
                    ["NgĂ¢n hĂ ng sá»‘, thanh toĂ¡n Ä‘iá»‡n tá»­"] = "Ngân hàng số, thanh toán điện tử",
                    ["phĂ¢n tĂ­ch Ä‘áº§u tÆ°"] = "phân tích đầu tư"
                };

                foreach (var kv in extendedMap)
                {
                    text = text.Replace(kv.Key, kv.Value);
                }

                // Write with UTF-8 BOM so SSMS / VS recognize UTF-8
                File.WriteAllText(sqlFile.FullName, text, new System.Text.UTF8Encoding(true));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi dọn dẹp file SMMS_Database.sql: " + ex.Message);
            }
        }

        private static string CleanMojibake(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            try
            {
                var bytes = System.Text.Encoding.GetEncoding("iso-8859-1").GetBytes(input);
                var fixedStr = System.Text.Encoding.UTF8.GetString(bytes);
                return !string.IsNullOrEmpty(fixedStr) && !fixedStr.Contains("") ? fixedStr : input;
            }
            catch
            {
                return input;
            }
        }
    }
}
