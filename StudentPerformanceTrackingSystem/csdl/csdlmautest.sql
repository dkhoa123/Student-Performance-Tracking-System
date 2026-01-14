USE SPTS;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

PRINT '========================================';
PRINT 'SPTS DATABASE SEEDING - COMPLETE SYSTEM';
PRINT 'Current Date: 2026-01-14';
PRINT '========================================';
PRINT '';

BEGIN TRANSACTION;

BEGIN TRY

-- =============================================
-- 0.  SEED DEPARTMENTS (10 ngành)
-- =============================================
PRINT '0. Seeding Departments...';

IF NOT EXISTS (SELECT 1 FROM Departments)
BEGIN
    SET IDENTITY_INSERT Departments ON;
    INSERT INTO Departments (department_id, department_name, department_code, head_teacher_id)
    VALUES
        (1, N'Công nghệ Thông tin', 'CNTT', NULL),
        (2, N'Điện tử Viễn thông', 'DTVT', NULL),
        (3, N'Cơ khí Động lực', 'CKDL', NULL),
        (4, N'Quản trị Kinh doanh', 'QTKD', NULL),
        (5, N'Kinh tế', 'KT', NULL),
        (6, N'Ngoại ngữ', 'NN', NULL),
        (7, N'Y Dược', 'YD', NULL),
        (8, N'Xây dựng', 'XD', NULL),
        (9, N'Hóa học', 'HH', NULL),
        (10, N'Môi trường', 'MT', NULL);
    SET IDENTITY_INSERT Departments OFF;
    
    PRINT '  ✓ Inserted 10 departments';
END
ELSE
    PRINT '  ⊗ Departments already exist, skipping...';

PRINT '';

-- 1. SEED ACADEMIC YEARS (Fix: include start_year/end_year)
PRINT '1. Seeding AcademicYears...';

IF NOT EXISTS (SELECT 1 FROM dbo.AcademicYears)
BEGIN
    INSERT INTO dbo.AcademicYears (year_name, start_year, end_year, start_date, end_date)
    VALUES
        (N'2022-2023', 2022, 2023, '2022-09-01', '2023-06-30'),
        (N'2023-2024', 2023, 2024, '2023-09-01', '2024-06-30'),
        (N'2024-2025', 2024, 2025, '2024-09-01', '2025-06-30'),
        (N'2025-2026', 2025, 2026, '2025-09-01', '2026-06-30');

    PRINT '  ✓ Inserted 4 academic years';
END
ELSE
    PRINT '  ⊗ AcademicYears already exist, skipping...';

PRINT '';

-- =============================================
-- 2. SEED USERS (5 Admins + 20 Teachers + 1000 Students)
-- =============================================
PRINT '2. Seeding Users...';

-- 2.1 Admins
INSERT INTO dbo.Users (full_name, email, password_hash, role, status)
VALUES 
    (N'Nguyễn Văn An', 'admin. nva@university.edu. vn', '$2a$10$hashedpassword1', 'ADMIN', 'ACTIVE'),
    (N'Trần Thị Bình', 'admin. ttb@university.edu.vn', '$2a$10$hashedpassword2', 'ADMIN', 'ACTIVE'),
    (N'Lê Văn Cường', 'admin.lvc@university.edu.vn', '$2a$10$hashedpassword3', 'ADMIN', 'ACTIVE'),
    (N'Phạm Thị Dung', 'admin.ptd@university.edu.vn', '$2a$10$hashedpassword4', 'ADMIN', 'ACTIVE'),
    (N'Hoàng Văn Em', 'admin.hve@university.edu.vn', '$2a$10$hashedpassword5', 'ADMIN', 'ACTIVE');

PRINT '  ✓ Inserted 5 Admins';

-- 2.2 Teachers
DECLARE @TeacherCounter INT = 1;
DECLARE @TeacherNames TABLE (name NVARCHAR(100));
INSERT INTO @TeacherNames VALUES
    (N'Nguyễn Thị Hoa'), (N'Trần Văn Hùng'), (N'Lê Thị Lan'), (N'Phạm Văn Minh'),
    (N'Hoàng Thị Nga'), (N'Đặng Văn Phúc'), (N'Vũ Thị Quỳnh'), (N'Bùi Văn Sơn'),
    (N'Đỗ Thị Tâm'), (N'Ngô Văn Tú'), (N'Mai Thị Uyên'), (N'Phan Văn Vinh'),
    (N'Chu Thị Xuân'), (N'Lưu Văn Yên'), (N'Đinh Thị Ánh'), (N'Trịnh Văn Bảo'),
    (N'Võ Thị Cẩm'), (N'Dương Văn Đức'), (N'Hồ Thị Hằng'), (N'Lý Văn Khoa');

WHILE @TeacherCounter <= 20
BEGIN
    DECLARE @TeacherName NVARCHAR(100);
    SELECT TOP 1 @TeacherName = name FROM @TeacherNames ORDER BY (SELECT NULL);
    DELETE TOP (1) FROM @TeacherNames;
    
    INSERT INTO dbo.Users (full_name, email, password_hash, role, status)
    VALUES (@TeacherName, 
            'teacher' + RIGHT('00' + CAST(@TeacherCounter AS VARCHAR), 2) + '@university.edu.vn',
            '$2a$10$hashedpassword' + CAST(@TeacherCounter AS VARCHAR),
            'TEACHER', 
            'ACTIVE');
    
    SET @TeacherCounter = @TeacherCounter + 1;
END

PRINT '  ✓ Inserted 20 Teachers';

-- 2.3 Students
DECLARE @StudentCounter INT = 1;
DECLARE @FirstNames TABLE (name NVARCHAR(50));
DECLARE @LastNames TABLE (name NVARCHAR(50));

INSERT INTO @FirstNames VALUES
    (N'An'), (N'B��nh'), (N'Cường'), (N'Dũng'), (N'Đạt'), (N'Giang'), (N'Hà'), (N'Hùng'),
    (N'Khánh'), (N'Linh'), (N'Minh'), (N'Nam'), (N'Phong'), (N'Quân'), (N'Sơn'), (N'Tâm'),
    (N'Thảo'), (N'Tuấn'), (N'Uyên'), (N'Vinh'), (N'Xuân'), (N'Yến'), (N'Ánh'), (N'Đức');

INSERT INTO @LastNames VALUES
    (N'Nguyễn'), (N'Trần'), (N'Lê'), (N'Phạm'), (N'Hoàng'), (N'Huỳnh'), (N'Phan'), (N'Vũ'),
    (N'Võ'), (N'Đặng'), (N'Bùi'), (N'Đỗ'), (N'Hồ'), (N'Ngô'), (N'Dương'), (N'Lý');

WHILE @StudentCounter <= 1000
BEGIN
    DECLARE @LastName NVARCHAR(50) = (SELECT TOP 1 name FROM @LastNames ORDER BY NEWID());
    DECLARE @FirstName NVARCHAR(50) = (SELECT TOP 1 name FROM @FirstNames ORDER BY NEWID());
    DECLARE @MiddleName NVARCHAR(50) = CASE WHEN @StudentCounter % 3 = 0 THEN N'Văn' 
                                             WHEN @StudentCounter % 3 = 1 THEN N'Thị' 
                                             ELSE N'Đức' END;
    DECLARE @StudentFullName NVARCHAR(100) = @LastName + N' ' + @MiddleName + N' ' + @FirstName;
    
    INSERT INTO dbo.Users (full_name, email, password_hash, role, status)
    VALUES (@StudentFullName,
            'student' + RIGHT('0000' + CAST(@StudentCounter AS VARCHAR), 4) + '@university.edu.vn',
            '$2a$10$hashedpassword' + CAST(@StudentCounter AS VARCHAR),
            'STUDENT',
            'ACTIVE');
    
    SET @StudentCounter = @StudentCounter + 1;
END

PRINT '  ✓ Inserted 1000 Students';
PRINT '';

-- =============================================
-- 3. SEED TEACHERS TABLE
-- =============================================
PRINT '3. Seeding Teachers... ';

INSERT INTO dbo.Teachers (teacher_id, teacher_code)
SELECT user_id, 'GV' + RIGHT('0000' + CAST(ROW_NUMBER() OVER (ORDER BY user_id) AS VARCHAR), 4)
FROM dbo.Users
WHERE role = 'TEACHER';

PRINT '  ✓ Inserted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' teachers';
PRINT '';

-- =============================================
-- 4. SEED STUDENTS TABLE
-- =============================================
PRINT '4. Seeding Students...';

INSERT INTO dbo. Students (student_id, student_code, major, cohort_year, department_id, DateOfBirth, Gender, Phone, Address)
SELECT 
    user_id,
    'SV' + RIGHT('0000' + CAST(ROW_NUMBER() OVER (ORDER BY user_id) AS VARCHAR), 4),
    CASE (ROW_NUMBER() OVER (ORDER BY user_id) % 10) + 1
        WHEN 1 THEN N'Công nghệ Thông tin'
        WHEN 2 THEN N'Điện tử Viễn thông'
        WHEN 3 THEN N'Cơ khí Động lực'
        WHEN 4 THEN N'Quản trị Kinh doanh'
        WHEN 5 THEN N'Kinh tế'
        WHEN 6 THEN N'Ngoại ngữ'
        WHEN 7 THEN N'Y Dược'
        WHEN 8 THEN N'Xây dựng'
        WHEN 9 THEN N'Hóa học'
        ELSE N'Môi trường'
    END,
    CASE 
        WHEN ROW_NUMBER() OVER (ORDER BY user_id) <= 250 THEN 2022
        WHEN ROW_NUMBER() OVER (ORDER BY user_id) <= 500 THEN 2023
        WHEN ROW_NUMBER() OVER (ORDER BY user_id) <= 750 THEN 2024
        ELSE 2025
    END,
    (ROW_NUMBER() OVER (ORDER BY user_id) % 10) + 1,
    DATEADD(YEAR, -18 - (ABS(CHECKSUM(NEWID())) % 7), 
            DATEADD(DAY, -(ABS(CHECKSUM(NEWID())) % 365), '2026-01-14')),
    CASE (ROW_NUMBER() OVER (ORDER BY user_id) % 2) WHEN 0 THEN N'Nam' ELSE N'Nữ' END,
    '09' + RIGHT('00000000' + CAST(ABS(CHECKSUM(NEWID())) % 100000000 AS VARCHAR), 8),
    N'Địa chỉ ' + CAST(ROW_NUMBER() OVER (ORDER BY user_id) AS VARCHAR) + N', Hà Nội'
FROM dbo.Users
WHERE role = 'STUDENT';

PRINT '  ✓ Inserted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' students';
PRINT '';

-- =============================================
-- 5. SEED ADVISORS
-- =============================================
PRINT '5. Seeding Advisors...';

INSERT INTO dbo. Advisors (advisor_id, advisor_code)
SELECT TOP 10 teacher_id, 'CV' + RIGHT('0000' + CAST(ROW_NUMBER() OVER (ORDER BY teacher_id) AS VARCHAR), 4)
FROM dbo.Teachers
ORDER BY teacher_id;

PRINT '  ✓ Inserted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' advisors';
PRINT '';

-- =============================================
-- 6. UPDATE DEPARTMENTS WITH HEAD TEACHERS
-- =============================================
PRINT '6. Updating Departments with Head Teachers...';

UPDATE d
SET head_teacher_id = t.teacher_id
FROM Departments d
CROSS APPLY (
    SELECT TOP 1 teacher_id 
    FROM Teachers 
    WHERE teacher_id NOT IN (SELECT advisor_id FROM Advisors)
    ORDER BY NEWID()
) t
WHERE d.department_id <= 10;

PRINT '  ✓ Updated ' + CAST(@@ROWCOUNT AS VARCHAR) + ' departments';
PRINT '';

-- =============================================
-- 7. SEED TERMS (Fix FK AcademicYears)
-- =============================================
PRINT '7. Seeding Terms...';

DECLARE @AY_2022_2023 INT = (SELECT academic_year_id FROM dbo.AcademicYears WHERE year_name = N'2022-2023');
DECLARE @AY_2023_2024 INT = (SELECT academic_year_id FROM dbo.AcademicYears WHERE year_name = N'2023-2024');
DECLARE @AY_2024_2025 INT = (SELECT academic_year_id FROM dbo.AcademicYears WHERE year_name = N'2024-2025');
DECLARE @AY_2025_2026 INT = (SELECT academic_year_id FROM dbo.AcademicYears WHERE year_name = N'2025-2026');

IF @AY_2022_2023 IS NULL OR @AY_2023_2024 IS NULL OR @AY_2024_2025 IS NULL OR @AY_2025_2026 IS NULL
BEGIN
    THROW 50001, 'Missing AcademicYears rows. Please seed AcademicYears first (year_name 2022-2023..2025-2026).', 1;
END

INSERT INTO dbo.Terms (term_name, start_date, end_date, academic_year_id)
VALUES
    (N'HK1 2022-2023', '2022-09-01', '2023-01-15', @AY_2022_2023),
    (N'HK2 2022-2023', '2023-02-01', '2023-06-30', @AY_2022_2023),
    (N'HK1 2023-2024', '2023-09-01', '2024-01-15', @AY_2023_2024),
    (N'HK2 2023-2024', '2024-02-01', '2024-06-30', @AY_2023_2024),
    (N'HK1 2024-2025', '2024-09-01', '2025-01-15', @AY_2024_2025),
    (N'HK2 2024-2025', '2025-02-01', '2025-06-30', @AY_2024_2025),
    (N'HK1 2025-2026', '2025-09-01', '2026-01-15', @AY_2025_2026);

PRINT '  ✓ Inserted 7 terms';
PRINT '';

-- =============================================
-- 8. SEED COURSES (10 ngành × ~23 môn = 230 courses)
-- =============================================
PRINT '8. Seeding Courses...';

-- CNTT (25 courses)
INSERT INTO dbo.Courses (course_code, course_name, credits) VALUES
('INT2201', N'Lập trình hướng đối tượng', 3),
('INT2202', N'Cấu trúc dữ liệu và giải thuật', 4),
('INT2203', N'Cơ sở dữ liệu', 3),
('INT2204', N'Mạng máy tính', 3),
('INT2205', N'Hệ điều hành', 3),
('INT2206', N'Kỹ thuật phần mềm', 3),
('INT2207', N'Trí tuệ nhân tạo', 3),
('INT2208', N'Machine Learning', 3),
('INT2209', N'An toàn thông tin', 3),
('INT2210', N'Lập trình Web', 3),
('INT2211', N'Lập trình Mobile', 3),
('INT2212', N'Cloud Computing', 3),
('INT2213', N'DevOps', 2),
('INT2214', N'Blockchain', 2),
('INT2215', N'IoT Applications', 3),
('INT2216', N'Big Data', 3),
('INT2217', N'Computer Vision', 3),
('INT2218', N'Natural Language Processing', 3),
('INT2219', N'Phân tích thiết kế hệ thống', 3),
('INT2220', N'Quản lý dự án phần mềm', 2),
('INT2221', N'Kiểm thử phần mềm', 2),
('INT2222', N'UI/UX Design', 2),
('INT2223', N'Game Development', 3),
('INT2224', N'Cryptography', 3),
('INT2225', N'Distributed Systems', 3);

-- DTVT (23 courses)
INSERT INTO dbo.Courses (course_code, course_name, credits) VALUES
('ELE2201', N'Mạch điện tử', 3),
('ELE2202', N'Vi xử lý', 3),
('ELE2203', N'Truyền thông số', 3),
('ELE2204', N'Xử lý tín hiệu số', 3),
('ELE2205', N'Điện tử công suất', 3),
('ELE2206', N'Viễn thông di động', 3),
('ELE2207', N'Mạng viễn thông', 3),
('ELE2208', N'Anten và truyền sóng', 3),
('ELE2209', N'IoT và Embedded Systems', 3),
('ELE2210', N'FPGA Design', 3),
('ELE2211', N'RF Circuit Design', 3),
('ELE2212', N'Optical Communication', 3),
('ELE2213', N'Satellite Communication', 3),
('ELE2214', N'Wireless Networks', 3),
('ELE2215', N'5G Technology', 3),
('ELE2216', N'Digital Circuit Design', 3),
('ELE2217', N'Analog Circuit Design', 3),
('ELE2218', N'Power Electronics', 3),
('ELE2219', N'Control Systems', 3),
('ELE2220', N'Robotics', 3),
('ELE2221', N'Automation', 3),
('ELE2222', N'Sensor Technology', 2),
('ELE2223', N'Smart Grid', 3);

-- CKDL (21 courses)
INSERT INTO dbo.Courses (course_code, course_name, credits) VALUES
('MEC2201', N'Cơ học kỹ thuật', 4),
('MEC2202', N'Vật liệu cơ khí', 3),
('MEC2203', N'CAD/CAM', 3),
('MEC2204', N'Nhiệt động học', 3),
('MEC2205', N'Truyền nhiệt', 3),
('MEC2206', N'Cơ học chất lỏng', 3),
('MEC2207', N'Máy động lực', 3),
('MEC2208', N'Kỹ thuật chế tạo máy', 4),
('MEC2209', N'Dung sai và lắp ghép', 2),
('MEC2210', N'Hệ thống thủy khí', 3),
('MEC2211', N'Robotics công nghiệp', 3),
('MEC2212', N'CNC Programming', 3),
('MEC2213', N'Manufacturing Processes', 3),
('MEC2214', N'Quality Control', 2),
('MEC2215', N'Production Management', 3),
('MEC2216', N'Automotive Engineering', 3),
('MEC2217', N'Aerospace Engineering', 3),
('MEC2218', N'Mechatronics', 3),
('MEC2219', N'Finite Element Analysis', 3),
('MEC2220', N'Vibration Analysis', 3),
('MEC2221', N'Machine Design', 4);

-- QTKD (21 courses)
INSERT INTO dbo.Courses (course_code, course_name, credits) VALUES
('BUS2201', N'Quản trị học', 3),
('BUS2202', N'Marketing căn bản', 3),
('BUS2203', N'Quản trị nhân sự', 3),
('BUS2204', N'Quản trị sản xuất', 3),
('BUS2205', N'Quản trị chiến lược', 3),
('BUS2206', N'Hành vi tổ chức', 3),
('BUS2207', N'Quản trị dự án', 3),
('BUS2208', N'Logistics và chuỗi cung ứng', 3),
('BUS2209', N'E-Commerce', 3),
('BUS2210', N'Digital Marketing', 3),
('BUS2211', N'Brand Management', 3),
('BUS2212', N'Consumer Behavior', 3),
('BUS2213', N'Sales Management', 3),
('BUS2214', N'International Business', 3),
('BUS2215', N'Entrepreneurship', 3),
('BUS2216', N'Business Analytics', 3),
('BUS2217', N'Operations Research', 3),
('BUS2218', N'Quality Management', 2),
('BUS2219', N'Business Law', 2),
('BUS2220', N'Business Ethics', 2),
('BUS2221', N'Negotiation Skills', 2);

-- KT (20 courses)
INSERT INTO dbo.Courses (course_code, course_name, credits) VALUES
('ECO2201', N'Kinh tế vi mô', 3),
('ECO2202', N'Kinh tế vĩ mô', 3),
('ECO2203', N'Kinh tế lượng', 3),
('ECO2204', N'Tài chính doanh nghiệp', 3),
('ECO2205', N'Tài chính tiền tệ', 3),
('ECO2206', N'Ngân hàng thương mại', 3),
('ECO2207', N'Thị trường chứng khoán', 3),
('ECO2208', N'Kế toán tài chính', 3),
('ECO2209', N'Kế toán quản trị', 3),
('ECO2210', N'Kiểm toán', 3),
('ECO2211', N'Thuế', 2),
('ECO2212', N'Tài chính quốc tế', 3),
('ECO2213', N'Đầu tư tài chính', 3),
('ECO2214', N'Quản trị rủi ro', 3),
('ECO2215', N'Phân tích báo cáo tài chính', 3),
('ECO2216', N'Financial Modeling', 3),
('ECO2217', N'Corporate Finance', 3),
('ECO2218', N'Investment Banking', 3),
('ECO2219', N'Financial Markets', 3),
('ECO2220', N'Behavioral Finance', 3);

-- NN (20 courses)
INSERT INTO dbo.Courses (course_code, course_name, credits) VALUES
('LAN2201', N'Tiếng Anh 1', 3),
('LAN2202', N'Tiếng Anh 2', 3),
('LAN2203', N'Tiếng Anh 3', 3),
('LAN2204', N'Tiếng Anh 4', 3),
('LAN2205', N'Tiếng Anh chuyên ngành', 3),
('LAN2206', N'TOEIC Preparation', 3),
('LAN2207', N'IELTS Preparation', 3),
('LAN2208', N'Business English', 3),
('LAN2209', N'Academic Writing', 3),
('LAN2210', N'Public Speaking', 2),
('LAN2211', N'Tiếng Nhật 1', 3),
('LAN2212', N'Tiếng Nhật 2', 3),
('LAN2213', N'Tiếng Nhật 3', 3),
('LAN2214', N'Tiếng Trung 1', 3),
('LAN2215', N'Tiếng Trung 2', 3),
('LAN2216', N'Tiếng Trung 3', 3),
('LAN2217', N'Translation', 3),
('LAN2218', N'Interpretation', 3),
('LAN2219', N'Cross-cultural Communication', 2),
('LAN2220', N'Linguistics', 3);

-- YD (20 courses)
INSERT INTO dbo.Courses (course_code, course_name, credits) VALUES
('MED2201', N'Giải phẫu học', 4),
('MED2202', N'Sinh lý học', 4),
('MED2203', N'Dược lý học', 3),
('MED2204', N'Bệnh lý học', 3),
('MED2205', N'Vi sinh vật y học', 3),
('MED2206', N'Nội khoa', 4),
('MED2207', N'Ngoại khoa', 4),
('MED2208', N'Sản phụ khoa', 3),
('MED2209', N'Nhi khoa', 3),
('MED2210', N'Chẩn đoán hình ảnh', 3),
('MED2211', N'Xét nghiệm y học', 3),
('MED2212', N'Dược học lâm sàng', 3),
('MED2213', N'Hóa dược', 3),
('MED2214', N'Điều dưỡng nội khoa', 3),
('MED2215', N'Điều dưỡng ngoại khoa', 3),
('MED2216', N'Điều dưỡng cấp cứu', 3),
('MED2217', N'Y đức', 2),
('MED2218', N'Y tế công cộng', 3),
('MED2219', N'Dinh dưỡng', 2),
('MED2220', N'Sức khỏe tâm thần', 3);

-- XD (20 courses)
INSERT INTO dbo.Courses (course_code, course_name, credits) VALUES
('CIV2201', N'Vẽ kỹ thuật xây dựng', 3),
('CIV2202', N'Cơ học kết cấu', 4),
('CIV2203', N'Sức bền vật liệu', 4),
('CIV2204', N'Vật liệu xây dựng', 3),
('CIV2205', N'Địa chất công trình', 3),
('CIV2206', N'Kết cấu bê tông cốt thép', 4),
('CIV2207', N'Kết cấu thép', 3),
('CIV2208', N'Thiết kế nhà dân dụng', 3),
('CIV2209', N'Thiết kế nhà công nghiệp', 3),
('CIV2210', N'Cầu đường', 4),
('CIV2211', N'Thủy lực', 3),
('CIV2212', N'Kỹ thuật thi công', 3),
('CIV2213', N'Quản lý dự án xây dựng', 3),
('CIV2214', N'Dự toán xây dựng', 3),
('CIV2215', N'CAD Xây dựng', 3),
('CIV2216', N'BIM Technology', 3),
('CIV2217', N'Kiến trúc', 3),
('CIV2218', N'Quy hoạch đô thị', 3),
('CIV2219', N'Môi trường xây dựng', 2),
('CIV2220', N'Xây dựng xanh', 2);

-- HH (20 courses)
INSERT INTO dbo.Courses (course_code, course_name, credits) VALUES
('CHE2201', N'Hóa học đại cương', 4),
('CHE2202', N'Hóa học vô cơ', 3),
('CHE2203', N'Hóa học hữu cơ', 4),
('CHE2204', N'Hóa học phân tích', 3),
('CHE2205', N'Hóa lý', 3),
('CHE2206', N'Hóa sinh', 3),
('CHE2207', N'Công nghệ hóa học', 3),
('CHE2208', N'Hóa dầu', 3),
('CHE2209', N'Hóa polymer', 3),
('CHE2210', N'Công nghệ sinh học', 3),
('CHE2211', N'Hóa môi trường', 3),
('CHE2212', N'An toàn hóa chất', 2),
('CHE2213', N'Phân tích công cụ', 3),
('CHE2214', N'Xúc tác', 3),
('CHE2215', N'Điện hóa', 3),
('CHE2216', N'Hóa lượng tử', 3),
('CHE2217', N'Thiết kế quy trình hóa học', 3),
('CHE2218', N'Thực hành hóa học', 2),
('CHE2219', N'Kiểm soát chất lượng', 2),
('CHE2220', N'Hóa dược phẩm', 3);

-- MT (20 courses)
INSERT INTO dbo.Courses (course_code, course_name, credits) VALUES
('ENV2201', N'Khoa học môi trường', 3),
('ENV2202', N'Sinh thái học', 3),
('ENV2203', N'Quản lý tài nguyên thiên nhiên', 3),
('ENV2204', N'Ô nhiễm môi trường', 3),
('ENV2205', N'Xử lý nước thải', 3),
('ENV2206', N'Xử lý khí thải', 3),
('ENV2207', N'Quản lý chất thải rắn', 3),
('ENV2208', N'Đánh giá tác động môi trường', 3),
('ENV2209', N'Giám sát môi trường', 3),
('ENV2210', N'Biến đổi khí hậu', 3),
('ENV2211', N'Năng lượng tái tạo', 3),
('ENV2212', N'Phát triển bền vững', 3),
('ENV2213', N'GIS và viễn thám', 3),
('ENV2214', N'Quản lý chất lượng không khí', 3),
('ENV2215', N'Quản lý nguồn nước', 3),
('ENV2216', N'Bảo tồn đa dạng sinh học', 3),
('ENV2217', N'Kinh tế môi trường', 3),
('ENV2218', N'Luật môi trường', 2),
('ENV2219', N'Công nghệ môi trường', 3),
('ENV2220', N'Hệ thống quản lý môi trường', 2);

PRINT '  ✓ Inserted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' courses';
PRINT '';

-- =============================================
-- 9. SEED GRADE RULES (quy tắc tính điểm tổng cho từng môn)
-- =============================================
PRINT '9. Seeding Grade Rules...';

INSERT INTO dbo.GradeRules (course_id, process_weight, final_weight, rounding_scale, active)
SELECT 
    course_id,
    CASE (course_id % 3)
        WHEN 0 THEN 0.3000 -- 30/70
        WHEN 1 THEN 0.4000 -- 40/60
        ELSE 0.5000        -- 50/50
    END AS process_weight,
    CASE (course_id % 3)
        WHEN 0 THEN 0.7000
        WHEN 1 THEN 0.6000
        ELSE 0.5000
    END AS final_weight,
    1,
    1
FROM dbo. Courses;

PRINT '  ✓ Inserted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' grade rules (1 per course)';
PRINT '';

-- =============================================
-- 10. SEED SECTIONS
-- =============================================
PRINT '10. Seeding Sections...';

DECLARE @CourseID INT;
DECLARE @TermID INT = (SELECT TOP 1 term_id FROM Terms ORDER BY start_date DESC);
DECLARE @TeacherID INT;
DECLARE @SectionCount INT;

DECLARE course_cursor CURSOR FOR 
SELECT course_id FROM Courses;

OPEN course_cursor;
FETCH NEXT FROM course_cursor INTO @CourseID;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @SectionCount = 1 + (ABS(CHECKSUM(NEWID())) % 3); -- 1-3 sections per course
    
    DECLARE @i INT = 1;
    WHILE @i <= @SectionCount
    BEGIN
        SELECT TOP 1 @TeacherID = teacher_id FROM Teachers ORDER BY NEWID();
        
        INSERT INTO dbo.Sections (term_id, course_id, teacher_id, section_code, status)
        VALUES (
            @TermID,
            @CourseID,
            @TeacherID,
            RIGHT('000' + CAST(@CourseID AS VARCHAR), 3) + '-' + CAST(@i AS VARCHAR(2)), -- e.g., "001-1"
            'OPEN'
        );
        
        SET @i = @i + 1;
    END
    
    FETCH NEXT FROM course_cursor INTO @CourseID;
END

CLOSE course_cursor;
DEALLOCATE course_cursor;

PRINT '  ✓ Inserted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' sections';
PRINT '';

-- =============================================
-- 11. SEED SECTION SCHEDULES
-- =============================================
PRINT '11. Seeding Section Schedules...';

DECLARE @SectionID INT;
DECLARE @DayOfWeek VARCHAR(20);
DECLARE @StartPeriod INT;
DECLARE @EndPeriod INT;
DECLARE @Room NVARCHAR(50);

DECLARE section_cursor CURSOR FOR 
SELECT section_id FROM Sections;

OPEN section_cursor;
FETCH NEXT FROM section_cursor INTO @SectionID;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @Meeting INT = 1;
    WHILE @Meeting <= 2
    BEGIN
        SET @DayOfWeek = CASE (ABS(CHECKSUM(NEWID())) % 5) + 1
            WHEN 1 THEN 'MONDAY'
            WHEN 2 THEN 'TUESDAY'
            WHEN 3 THEN 'WEDNESDAY'
            WHEN 4 THEN 'THURSDAY'
            ELSE 'FRIDAY'
        END;
        
        SET @StartPeriod = 1 + (ABS(CHECKSUM(NEWID())) % 9);
        SET @EndPeriod = @StartPeriod + 2;
        
        SET @Room = N'P' + CAST((ABS(CHECKSUM(NEWID())) % 5) + 1 AS NVARCHAR(2)) 
                    + '0' + CAST((ABS(CHECKSUM(NEWID())) % 10) + 1 AS NVARCHAR(2));
        
        INSERT INTO dbo.SectionSchedules (
            section_id, 
            day_of_week, 
            start_period, 
            end_period,
            start_time,
            end_time,
            room
        )
        VALUES (
            @SectionID,
            @DayOfWeek,
            @StartPeriod,
            @EndPeriod,
            DATEADD(MINUTE, (@StartPeriod - 1) * 50, '07:00:00'),
            DATEADD(MINUTE, @EndPeriod * 50, '07:00:00'),
            @Room
        );
        
        SET @Meeting = @Meeting + 1;
    END
    
    FETCH NEXT FROM section_cursor INTO @SectionID;
END

CLOSE section_cursor;
DEALLOCATE section_cursor;

PRINT '  ✓ Inserted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' section schedules';
PRINT '';

-- =============================================
-- 12. SEED SECTION_STUDENTS (Students added to sections)
-- =============================================
PRINT '12. Seeding SectionStudents...';

DECLARE @StudentID INT;
DECLARE @StudentDeptID INT;
DECLARE @EnrollCount INT;

DECLARE student_cursor CURSOR FOR 
SELECT student_id, department_id FROM Students;

OPEN student_cursor;
FETCH NEXT FROM student_cursor INTO @StudentID, @StudentDeptID;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @EnrollCount = 5 + (ABS(CHECKSUM(NEWID())) % 3); -- 5-7 môn/SV
    
    -- Add student to random sections from their department
    INSERT INTO dbo.SectionStudents (section_id, student_id, added_by, added_at, source, status)
    SELECT TOP (@EnrollCount)
        s.section_id,
        @StudentID,
        (SELECT TOP 1 user_id FROM Users WHERE role='ADMIN'), -- admin added
        DATEADD(DAY, -(ABS(CHECKSUM(NEWID())) % 30), '2026-01-14'),
        'MANUAL',
        'ACTIVE'
    FROM Sections s
    INNER JOIN Courses c ON s.course_id = c.course_id
    WHERE c.course_code LIKE 
        CASE @StudentDeptID
            WHEN 1 THEN 'INT%'
            WHEN 2 THEN 'ELE%'
            WHEN 3 THEN 'MEC%'
            WHEN 4 THEN 'BUS%'
            WHEN 5 THEN 'ECO%'
            WHEN 6 THEN 'LAN%'
            WHEN 7 THEN 'MED%'
            WHEN 8 THEN 'CIV%'
            WHEN 9 THEN 'CHE%'
            ELSE 'ENV%'
        END
    AND NOT EXISTS (
        SELECT 1 FROM SectionStudents ss 
        WHERE ss.section_id = s.section_id AND ss.student_id = @StudentID
    )
    ORDER BY NEWID();
    
    FETCH NEXT FROM student_cursor INTO @StudentID, @StudentDeptID;
END

CLOSE student_cursor;
DEALLOCATE student_cursor;

PRINT '  ✓ Inserted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' section-student records';
PRINT '';

-- =============================================
-- 13. SEED GPA SCALES (for converting total score to GPA 0-4)
-- =============================================
PRINT '13. Seeding GPA Scales...';

INSERT INTO dbo.GpaScales (min_score, max_score, gpa_point, letter)
VALUES
    (0.00, 3.99, 0.00, 'F'),
    (4.00, 4.99, 1.00, 'D'),
    (5.00, 6.49, 2.00, 'C'),
    (6.50, 7.99, 3.00, 'B'),
    (8.00, 10.00, 4.00, 'A');

PRINT '  ✓ Inserted 5 GPA scale records';
PRINT '';

-- =============================================
-- 14. SEED SAMPLE GRADES (for ~30% of section-student pairs)
-- =============================================
PRINT '14. Seeding Sample Grades...';

-- Insert process_score and final_score for 30% of section_students
INSERT INTO dbo. Grades (section_id, student_id, process_score, final_score, updated_by, updated_at)
SELECT TOP (SELECT COUNT(*) * 30 / 100 FROM SectionStudents)
    ss.section_id,
    ss.student_id,
    -- Random process score 4. 0-10.0
    4.0 + (ABS(CHECKSUM(NEWID())) % 61) / 10.0,
    -- Random final score 4.0-10.0
    4.0 + (ABS(CHECKSUM(NEWID())) % 61) / 10.0,
    (SELECT TOP 1 teacher_id FROM Sections WHERE section_id = ss.section_id), -- teacher who teaches
    DATEADD(DAY, -(ABS(CHECKSUM(NEWID())) % 60), '2026-01-14')
FROM SectionStudents ss
WHERE ss.status = 'ACTIVE'
ORDER BY NEWID();

PRINT '  ✓ Inserted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' grade records';
PRINT '';

-- =============================================
-- 15. AUTO-CALCULATE TOTAL_SCORE & GPA_POINT
-- =============================================
PRINT '15. Calculating total_score and gpa_point...';

-- Calculate total_score from process + final using GradeRules
UPDATE g
SET total_score = ROUND(
    (ISNULL(g.process_score, 0) * gr.process_weight) +
    (ISNULL(g.final_score, 0) * gr.final_weight),
    1
)
FROM dbo. Grades g
INNER JOIN dbo. Sections s ON s.section_id = g.section_id
INNER JOIN dbo.GradeRules gr ON gr.course_id = s.course_id AND gr.active = 1;

-- Calculate gpa_point from total_score using GpaScales
UPDATE g
SET gpa_point = sc.gpa_point
FROM dbo.Grades g
INNER JOIN dbo.GpaScales sc
  ON g.total_score BETWEEN sc.min_score AND sc.max_score
WHERE g.total_score IS NOT NULL;

PRINT '  ✓ Updated total_score and gpa_point for all grades';
PRINT '';

-- =============================================
-- 16. CALCULATE TERM_GPA (weighted by credits)
-- =============================================
PRINT '16. Calculating TermGpa...';

-- GPA = SUM(gpa_point * credits) / SUM(credits)
;WITH G AS (
    SELECT
        s.term_id,
        g.student_id,
        c.credits,
        g.gpa_point
    FROM dbo.Grades g
    INNER JOIN dbo.Sections s ON s.section_id = g.section_id
    INNER JOIN dbo.Courses c ON c.course_id = s.course_id
    WHERE g.gpa_point IS NOT NULL
),
Agg AS (
    SELECT
        term_id,
        student_id,
        CAST(SUM(gpa_point * credits) AS DECIMAL(10,4)) / NULLIF(SUM(credits), 0) AS gpa_value,
        SUM(credits) AS credits_attempted
    FROM G
    GROUP BY term_id, student_id
)
MERGE dbo.TermGpa AS tgt
USING Agg AS src
ON (tgt.term_id = src.term_id AND tgt. student_id = src.student_id)
WHEN MATCHED THEN
  UPDATE SET
    gpa_value = CAST(ROUND(src.gpa_value, 2) AS DECIMAL(3,2)),
    credits_attempted = src.credits_attempted,
    updated_at = SYSUTCDATETIME()
WHEN NOT MATCHED THEN
  INSERT (term_id, student_id, gpa_value, credits_attempted, updated_at)
  VALUES (src.term_id, src. student_id, CAST(ROUND(src.gpa_value, 2) AS DECIMAL(3,2)), src.credits_attempted, SYSUTCDATETIME());

PRINT '  ✓ Calculated TermGpa for ' + CAST(@@ROWCOUNT AS VARCHAR) + ' term-student pairs';
PRINT '';

-- =============================================
-- SUMMARY
-- =============================================
PRINT '';
PRINT '========================================';
PRINT 'SEEDING COMPLETED SUCCESSFULLY!';
PRINT '========================================';
PRINT '';

-- Declare variables to hold counts (đổi tên để tránh trùng)
DECLARE @CountDept INT, @CountUser INT, @CountStudent INT, @CountTeacher INT;
DECLARE @CountAdvisor INT, @CountCourse INT, @CountTerm INT, @CountSection INT;
DECLARE @CountSchedule INT, @CountSectionStudent INT, @CountGrade INT, @CountTermGpa INT;

-- Get counts
SELECT @CountDept = COUNT(*) FROM Departments;
SELECT @CountUser = COUNT(*) FROM Users;
SELECT @CountStudent = COUNT(*) FROM Students;
SELECT @CountTeacher = COUNT(*) FROM Teachers;
SELECT @CountAdvisor = COUNT(*) FROM Advisors;
SELECT @CountCourse = COUNT(*) FROM Courses;
SELECT @CountTerm = COUNT(*) FROM Terms;
SELECT @CountSection = COUNT(*) FROM Sections;
SELECT @CountSchedule = COUNT(*) FROM SectionSchedules;
SELECT @CountSectionStudent = COUNT(*) FROM SectionStudents;
SELECT @CountGrade = COUNT(*) FROM Grades;
SELECT @CountTermGpa = COUNT(*) FROM TermGpa;

-- Print summary
PRINT 'Summary:';
PRINT '  - Departments: ' + CAST(@CountDept AS VARCHAR);
PRINT '  - Users: ' + CAST(@CountUser AS VARCHAR);
PRINT '  - Students:  ' + CAST(@CountStudent AS VARCHAR);
PRINT '  - Teachers: ' + CAST(@CountTeacher AS VARCHAR);
PRINT '  - Advisors: ' + CAST(@CountAdvisor AS VARCHAR);
PRINT '  - Courses: ' + CAST(@CountCourse AS VARCHAR);
PRINT '  - Terms: ' + CAST(@CountTerm AS VARCHAR);
PRINT '  - Sections: ' + CAST(@CountSection AS VARCHAR);
PRINT '  - Section Schedules:  ' + CAST(@CountSchedule AS VARCHAR);
PRINT '  - SectionStudents: ' + CAST(@CountSectionStudent AS VARCHAR);
PRINT '  - Grades: ' + CAST(@CountGrade AS VARCHAR);
PRINT '  - TermGpa: ' + CAST(@CountTermGpa AS VARCHAR);
PRINT '';

    COMMIT TRANSACTION;
    PRINT 'Transaction committed successfully. ';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    
    PRINT '';
    PRINT '========================================';
    PRINT 'ERROR OCCURRED - TRANSACTION ROLLED BACK';
    PRINT '========================================';
    PRINT 'Error Message: ' + ERROR_MESSAGE();
    PRINT 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR);
    PRINT 'Error Number: ' + CAST(ERROR_NUMBER() AS VARCHAR);
    PRINT '';
    
    THROW;
END CATCH;
GO