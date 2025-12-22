/* =========================
   Student Performance Tracking System (SQL Server)
   ========================= */

-- 1) USERS (tài khoản chung)
CREATE TABLE dbo.Users (
    user_id         INT IDENTITY(1,1) PRIMARY KEY,
    full_name       NVARCHAR(100) NOT NULL,
    email           NVARCHAR(255) NOT NULL UNIQUE,
    password_hash   NVARCHAR(255) NOT NULL,
    role            VARCHAR(20) NOT NULL CHECK (role IN ('STUDENT','TEACHER','ADVISOR','ADMIN')),
    status          VARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    created_at      DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

-- 2) ADVISORS, TEACHERS, STUDENTS (1-1 với Users)
CREATE TABLE dbo.Advisors (
    advisor_id      INT PRIMARY KEY, -- FK to Users
    advisor_code    VARCHAR(30) NOT NULL UNIQUE,
    CONSTRAINT FK_Advisors_Users FOREIGN KEY (advisor_id) REFERENCES dbo.Users(user_id)
);

CREATE TABLE dbo.Teachers (
    teacher_id      INT PRIMARY KEY,
    teacher_code    VARCHAR(30) NOT NULL UNIQUE,
    CONSTRAINT FK_Teachers_Users FOREIGN KEY (teacher_id) REFERENCES dbo.Users(user_id)
);

-- 3) CLASSES (lớp hành chính)
CREATE TABLE dbo.Classes (
    class_id     INT IDENTITY(1,1) PRIMARY KEY,
    class_code   VARCHAR(30) NOT NULL UNIQUE,
    class_name   NVARCHAR(100) NOT NULL,
    advisor_id   INT NULL,
    CONSTRAINT FK_Classes_Advisors FOREIGN KEY (advisor_id) REFERENCES dbo.Advisors(advisor_id)
);

CREATE TABLE dbo.Students (
    student_id      INT PRIMARY KEY,
    student_code    VARCHAR(30) NOT NULL UNIQUE,
    class_id        INT NULL,
    major           NVARCHAR(100) NULL,
    cohort_year     INT NULL,
    CONSTRAINT FK_Students_Users   FOREIGN KEY (student_id) REFERENCES dbo.Users(user_id),
    CONSTRAINT FK_Students_Classes FOREIGN KEY (class_id) REFERENCES dbo.Classes(class_id)
);

-- 4) TERMS (học kỳ)
CREATE TABLE dbo.Terms (
    term_id      INT IDENTITY(1,1) PRIMARY KEY,
    term_name    NVARCHAR(50) NOT NULL UNIQUE,
    start_date   DATE NULL,
    end_date     DATE NULL
);

-- 5) COURSES (môn)
CREATE TABLE dbo.Courses (
    course_id    INT IDENTITY(1,1) PRIMARY KEY,
    course_code  VARCHAR(30) NOT NULL UNIQUE,
    course_name  NVARCHAR(200) NOT NULL,
    credits      INT NOT NULL CHECK (credits > 0)
);

-- 6) SECTIONS (lớp học phần)
CREATE TABLE dbo.Sections (
    section_id    INT IDENTITY(1,1) PRIMARY KEY,
    term_id       INT NOT NULL,
    course_id     INT NOT NULL,
    teacher_id    INT NOT NULL,
    section_code  VARCHAR(50) NULL,
    CONSTRAINT FK_Sections_Terms    FOREIGN KEY (term_id) REFERENCES dbo.Terms(term_id),
    CONSTRAINT FK_Sections_Courses  FOREIGN KEY (course_id) REFERENCES dbo.Courses(course_id),
    CONSTRAINT FK_Sections_Teachers FOREIGN KEY (teacher_id) REFERENCES dbo.Teachers(teacher_id)
);

-- 7) SECTION_STUDENTS (DS sinh viên của lớp học phần) - dùng cho import Excel
CREATE TABLE dbo.SectionStudents (
    section_id   INT NOT NULL,
    student_id   INT NOT NULL,
    added_by     INT NULL, -- user (admin/teacher)
    added_at     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    source       VARCHAR(20) NOT NULL DEFAULT 'EXCEL' CHECK (source IN ('EXCEL','MANUAL')),
    status       VARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    CONSTRAINT PK_SectionStudents PRIMARY KEY (section_id, student_id),
    CONSTRAINT FK_SS_Sections FOREIGN KEY (section_id) REFERENCES dbo.Sections(section_id),
    CONSTRAINT FK_SS_Students FOREIGN KEY (student_id) REFERENCES dbo.Students(student_id),
    CONSTRAINT FK_SS_Users    FOREIGN KEY (added_by) REFERENCES dbo.Users(user_id)
);

-- 8) GRADE_RULES (quy tắc tính điểm theo môn)
CREATE TABLE dbo.GradeRules (
    rule_id        INT IDENTITY(1,1) PRIMARY KEY,
    course_id      INT NOT NULL,
    process_weight DECIMAL(5,4) NOT NULL, -- ví dụ 0.4000
    final_weight   DECIMAL(5,4) NOT NULL, -- ví dụ 0.6000
    rounding_scale INT NOT NULL DEFAULT 1, -- 1: làm tròn 1 chữ số thập phân (0.1)
    active         BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_GradeRules_Courses FOREIGN KEY (course_id) REFERENCES dbo.Courses(course_id),
    CONSTRAINT CK_GradeRules_Weights CHECK (
        process_weight >= 0 AND final_weight >= 0 AND
        process_weight + final_weight = 1
    )
);

-- 9) GPA_SCALES (quy đổi điểm tổng -> thang 4)
CREATE TABLE dbo.GpaScales (
    scale_id   INT IDENTITY(1,1) PRIMARY KEY,
    min_score  DECIMAL(4,2) NOT NULL,
    max_score  DECIMAL(4,2) NOT NULL,
    gpa_point  DECIMAL(3,2) NOT NULL CHECK (gpa_point BETWEEN 0 AND 4),
    letter     VARCHAR(2) NULL,
    CONSTRAINT CK_GpaScales_Range CHECK (min_score <= max_score)
);

-- 10) GRADES (điểm QT, CK, tổng tự tính, GPA point)
CREATE TABLE dbo.Grades (
    grade_id      INT IDENTITY(1,1) PRIMARY KEY,
    section_id    INT NOT NULL,
    student_id    INT NOT NULL,
    process_score DECIMAL(4,2) NULL CHECK (process_score BETWEEN 0 AND 10),
    final_score   DECIMAL(4,2) NULL CHECK (final_score BETWEEN 0 AND 10),
    total_score   DECIMAL(4,2) NULL CHECK (total_score BETWEEN 0 AND 10),
    gpa_point     DECIMAL(3,2) NULL CHECK (gpa_point BETWEEN 0 AND 4),
    updated_by    INT NULL, -- teacher_id
    updated_at    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Grades UNIQUE (section_id, student_id),
    CONSTRAINT FK_Grades_Sections FOREIGN KEY (section_id) REFERENCES dbo.Sections(section_id),
    CONSTRAINT FK_Grades_Students FOREIGN KEY (student_id) REFERENCES dbo.Students(student_id),
    CONSTRAINT FK_Grades_Teachers FOREIGN KEY (updated_by) REFERENCES dbo.Teachers(teacher_id)
);

-- 11) TERM_GPA (GPA theo kỳ, tính theo trọng số tín chỉ)
CREATE TABLE dbo.TermGpa (
    term_id           INT NOT NULL,
    student_id        INT NOT NULL,
    gpa_value         DECIMAL(3,2) NULL CHECK (gpa_value BETWEEN 0 AND 4),
    credits_attempted INT NULL,
    credits_earned    INT NULL,
    updated_at        DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_TermGpa PRIMARY KEY (term_id, student_id),
    CONSTRAINT FK_TermGpa_Terms    FOREIGN KEY (term_id) REFERENCES dbo.Terms(term_id),
    CONSTRAINT FK_TermGpa_Students FOREIGN KEY (student_id) REFERENCES dbo.Students(student_id)
);

-- 12) ALERTS (cảnh báo theo môn hoặc theo GPA kỳ)
CREATE TABLE dbo.Alerts (
    alert_id         INT IDENTITY(1,1) PRIMARY KEY,
    student_id       INT NOT NULL,
    term_id          INT NULL,
    section_id       INT NULL,
    alert_type       VARCHAR(30) NOT NULL, -- LOW_TOTAL, LOW_FINAL, LOW_GPA...
    severity         VARCHAR(10) NOT NULL CHECK (severity IN ('LOW','MEDIUM','HIGH')),
    threshold_value  DECIMAL(6,2) NULL,
    actual_value     DECIMAL(6,2) NULL,
    reason           NVARCHAR(500) NULL,
    status           VARCHAR(20) NOT NULL DEFAULT 'NEW' CHECK (status IN ('NEW','SENT','VIEWED','IN_PROGRESS','CLOSED')),
    created_at       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Alerts_Students FOREIGN KEY (student_id) REFERENCES dbo.Students(student_id),
    CONSTRAINT FK_Alerts_Terms    FOREIGN KEY (term_id) REFERENCES dbo.Terms(term_id),
    CONSTRAINT FK_Alerts_Sections FOREIGN KEY (section_id) REFERENCES dbo.Sections(section_id),
    CONSTRAINT CK_Alerts_Target CHECK (
        (term_id IS NOT NULL OR section_id IS NOT NULL) -- ít nhất một loại cảnh báo
    )
);

-- 13) REMINDERS (cố vấn gửi nhắc nhở)
CREATE TABLE dbo.Reminders (
    reminder_id  INT IDENTITY(1,1) PRIMARY KEY,
    alert_id     INT NOT NULL,
    advisor_id   INT NOT NULL,
    message      NVARCHAR(1000) NOT NULL,
    sent_at      DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Reminders_Alerts   FOREIGN KEY (alert_id) REFERENCES dbo.Alerts(alert_id),
    CONSTRAINT FK_Reminders_Advisors FOREIGN KEY (advisor_id) REFERENCES dbo.Advisors(advisor_id)
);

-- 14) NOTIFICATIONS (thông báo)
CREATE TABLE dbo.Notifications (
    notification_id  INT IDENTITY(1,1) PRIMARY KEY,
    user_id          INT NOT NULL,
    title            NVARCHAR(200) NOT NULL,
    content          NVARCHAR(2000) NOT NULL,
    related_alert_id INT NULL,
    is_read          BIT NOT NULL DEFAULT 0,
    created_at       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Notifications_Users  FOREIGN KEY (user_id) REFERENCES dbo.Users(user_id),
    CONSTRAINT FK_Notifications_Alerts FOREIGN KEY (related_alert_id) REFERENCES dbo.Alerts(alert_id)
);
GO