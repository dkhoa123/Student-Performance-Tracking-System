
USE SPTS;
GO

-- =============================================
-- 1. CREATE NEW TABLES
-- =============================================

-- Table: AcademicYears
IF NOT EXISTS (SELECT * FROM sys. tables WHERE name = 'AcademicYears')
BEGIN
    CREATE TABLE AcademicYears (
        academic_year_id INT IDENTITY(1,1) PRIMARY KEY,
        year_name NVARCHAR(20) NOT NULL UNIQUE,
        start_year INT NOT NULL,
        end_year INT NOT NULL,
        start_date DATE NOT NULL,
        end_date DATE NOT NULL,
        is_current BIT NOT NULL DEFAULT 0,
        status VARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
        created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        
        CONSTRAINT CK_AcademicYears_Years CHECK (end_year = start_year + 1),
        CONSTRAINT CK_AcademicYears_Dates CHECK (end_date > start_date),
        CONSTRAINT CK_AcademicYears_Status CHECK (status IN ('ACTIVE', 'INACTIVE', 'ARCHIVED'))
    );
    
    CREATE INDEX IX_AcademicYears_Current ON AcademicYears(is_current) WHERE is_current = 1;
    CREATE INDEX IX_AcademicYears_Year ON AcademicYears(start_year, end_year);
    
    PRINT '✓ Table AcademicYears created successfully';
END
GO

-- Table:  Departments
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Departments')
BEGIN
    CREATE TABLE Departments (
        department_id INT IDENTITY(1,1) PRIMARY KEY,
        department_code VARCHAR(20) NOT NULL UNIQUE,
        department_name NVARCHAR(200) NOT NULL,
        description NVARCHAR(1000) NULL,
        head_teacher_id INT NULL,
        phone VARCHAR(20) NULL,
        email VARCHAR(255) NULL,
        status VARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
        created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        
        CONSTRAINT FK_Departments_Teachers FOREIGN KEY (head_teacher_id) 
            REFERENCES Teachers(teacher_id),
        CONSTRAINT CK_Departments_Status CHECK (status IN ('ACTIVE', 'INACTIVE', 'MERGED')),
        CONSTRAINT CK_Departments_Email CHECK (email LIKE '%@%' OR email IS NULL)
    );
    
    CREATE INDEX IX_Departments_Status ON Departments(status);
    CREATE INDEX IX_Departments_HeadTeacher ON Departments(head_teacher_id);
    
    PRINT '✓ Table Departments created successfully';
END
GO

-- =============================================
-- 2. ALTER EXISTING TABLES
-- =============================================

-- Add academic_year_id to Terms
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID('Terms') AND name = 'academic_year_id'
)
BEGIN
    ALTER TABLE Terms
    ADD academic_year_id INT NULL,
        CONSTRAINT FK_Terms_AcademicYears FOREIGN KEY (academic_year_id)
            REFERENCES AcademicYears(academic_year_id);
    
    CREATE INDEX IX_Terms_AcademicYear ON Terms(academic_year_id);
    
    PRINT '✓ Added academic_year_id to Terms table';
END
GO

-- Add department_id to Students
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID('Students') AND name = 'department_id'
)
BEGIN
    ALTER TABLE Students
    ADD department_id INT NULL,
        CONSTRAINT FK_Students_Departments FOREIGN KEY (department_id)
            REFERENCES Departments(department_id);
    
    CREATE INDEX IX_Students_Department ON Students(department_id);
    
    PRINT '✓ Added department_id to Students table';
END
GO

-- =============================================
-- 3. INSERT SAMPLE DATA - DEPARTMENTS
-- =============================================

SET IDENTITY_INSERT Departments ON;

MERGE INTO Departments AS target
USING (VALUES
    (1, 'CNTT', N'Khoa Công nghệ Thông tin', N'Đào tạo chuyên ngành CNTT, An toàn thông tin, Khoa học máy tính', NULL, '024-3869-4242', 'cntt@university.edu. vn', 'ACTIVE'),
    (2, 'DTVT', N'Khoa Điện tử Viễn thông', N'Đào tạo chuyên ngành Điện tử, Viễn thông, IoT', NULL, '024-3869-4243', 'dtvt@university.edu.vn', 'ACTIVE'),
    (3, 'CKDL', N'Khoa Cơ khí Động lực', N'Đào tạo chuyên ngành Cơ khí chế tạo, Cơ điện tử', NULL, '024-3869-4244', 'ckdl@university.edu.vn', 'ACTIVE'),
    (4, 'QTKD', N'Khoa Quản trị Kinh doanh', N'Đào tạo Quản trị kinh doanh, Marketing, Logistics', NULL, '024-3869-4245', 'qtkd@university.edu.vn', 'ACTIVE'),
    (5, 'KT', N'Khoa Kinh tế', N'Đào tạo Kinh tế, Tài chính, Ngân hàng', NULL, '024-3869-4246', 'kt@university.edu.vn', 'ACTIVE'),
    (6, 'NN', N'Khoa Ngoại ngữ', N'Đào tạo Tiếng Anh, Tiếng Nhật, Tiếng Trung', NULL, '024-3869-4247', 'nn@university.edu.vn', 'ACTIVE'),
    (7, 'YD', N'Khoa Y Dược', N'Đào tạo Y khoa, Dược, Điều dưỡng', NULL, '024-3869-4248', 'yd@university.edu.vn', 'ACTIVE'),
    (8, 'XD', N'Khoa Xây dựng', N'Đào tạo Kỹ thuật xây dựng, Kiến trúc', NULL, '024-3869-4249', 'xd@university.edu.vn', 'ACTIVE'),
    (9, 'HH', N'Khoa Hóa học', N'Đào tạo Hóa học, Công nghệ hóa học', NULL, '024-3869-4250', 'hh@university.edu.vn', 'ACTIVE'),
    (10, 'MT', N'Khoa Môi trường', N'Đào tạo Khoa học môi trường, Quản lý tài nguyên', NULL, '024-3869-4251', 'mt@university.edu. vn', 'ACTIVE')
) AS source (department_id, department_code, department_name, description, head_teacher_id, phone, email, status)
ON target.department_code = source.department_code
WHEN NOT MATCHED THEN
    INSERT (department_id, department_code, department_name, description, head_teacher_id, phone, email, status)
    VALUES (source.department_id, source.department_code, source.department_name, source.description, source.head_teacher_id, source.phone, source.email, source.status);

SET IDENTITY_INSERT Departments OFF;

PRINT '✓ Inserted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' departments';
GO

-- =============================================
-- 4. INSERT SAMPLE DATA - ACADEMIC YEARS
-- =============================================

SET IDENTITY_INSERT AcademicYears ON;

MERGE INTO AcademicYears AS target
USING (VALUES
    (1, N'2022-2023', 2022, 2023, '2022-09-01', '2023-06-30', 0, 'ARCHIVED'),
    (2, N'2023-2024', 2023, 2024, '2023-09-01', '2024-06-30', 0, 'ARCHIVED'),
    (3, N'2024-2025', 2024, 2025, '2024-09-01', '2025-06-30', 0, 'ACTIVE'),
    (4, N'2025-2026', 2025, 2026, '2025-09-01', '2026-06-30', 1, 'ACTIVE'),
    (5, N'2026-2027', 2026, 2027, '2026-09-01', '2027-06-30', 0, 'ACTIVE')
) AS source (academic_year_id, year_name, start_year, end_year, start_date, end_date, is_current, status)
ON target.year_name = source.year_name
WHEN NOT MATCHED THEN
    INSERT (academic_year_id, year_name, start_year, end_year, start_date, end_date, is_current, status)
    VALUES (source.academic_year_id, source.year_name, source. start_year, source.end_year, source.start_date, source.end_date, source. is_current, source.status);

SET IDENTITY_INSERT AcademicYears OFF;

PRINT '✓ Inserted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' academic years';
GO