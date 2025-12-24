/* =========================================================
   Student Performance Tracking System - Schema (Simplified)
   - Only Sections (lớp học phần), no administrative Classes
   - Import students into sections via SectionStudents
   - Grades only allowed for students in SectionStudents
   SQL Server (T-SQL)
   ========================================================= */

/* =========================
   1) Users & roles
   ========================= */
CREATE TABLE dbo.Users (
    user_id         INT IDENTITY(1,1) CONSTRAINT PK_Users PRIMARY KEY,
    full_name       NVARCHAR(100) NOT NULL,
    email           NVARCHAR(255) NOT NULL CONSTRAINT UQ_Users_Email UNIQUE,
    password_hash   NVARCHAR(255) NOT NULL,
    role            VARCHAR(20) NOT NULL
                    CONSTRAINT CK_Users_Role CHECK (role IN ('STUDENT','TEACHER','ADVISOR','ADMIN')),
    status          VARCHAR(20) NOT NULL CONSTRAINT DF_Users_Status DEFAULT 'ACTIVE',
    created_at      DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME()
);

CREATE TABLE dbo.Students (
    student_id      INT CONSTRAINT PK_Students PRIMARY KEY,  -- FK -> Users
    student_code    VARCHAR(30) NOT NULL CONSTRAINT UQ_Students_Code UNIQUE,
    major           NVARCHAR(100) NULL,
    cohort_year     INT NULL,
    CONSTRAINT FK_Students_Users FOREIGN KEY (student_id) REFERENCES dbo.Users(user_id)
);

CREATE TABLE dbo.Teachers (
    teacher_id      INT CONSTRAINT PK_Teachers PRIMARY KEY,
    teacher_code    VARCHAR(30) NOT NULL CONSTRAINT UQ_Teachers_Code UNIQUE,
    CONSTRAINT FK_Teachers_Users FOREIGN KEY (teacher_id) REFERENCES dbo.Users(user_id)
);

CREATE TABLE dbo.Advisors (
    advisor_id      INT CONSTRAINT PK_Advisors PRIMARY KEY,
    advisor_code    VARCHAR(30) NOT NULL CONSTRAINT UQ_Advisors_Code UNIQUE,
    CONSTRAINT FK_Advisors_Users FOREIGN KEY (advisor_id) REFERENCES dbo.Users(user_id)
);

/* =========================
   2) Terms, Courses, Sections (lớp học phần)
   ========================= */
CREATE TABLE dbo.Terms (
    term_id      INT IDENTITY(1,1) CONSTRAINT PK_Terms PRIMARY KEY,
    term_name    NVARCHAR(50) NOT NULL CONSTRAINT UQ_Terms_Name UNIQUE,
    start_date   DATE NULL,
    end_date     DATE NULL
);

CREATE TABLE dbo.Courses (
    course_id    INT IDENTITY(1,1) CONSTRAINT PK_Courses PRIMARY KEY,
    course_code  VARCHAR(30) NOT NULL CONSTRAINT UQ_Courses_Code UNIQUE,
    course_name  NVARCHAR(200) NOT NULL,
    credits      INT NOT NULL CONSTRAINT CK_Courses_Credits CHECK (credits > 0)
);

CREATE TABLE dbo.Sections (
    section_id    INT IDENTITY(1,1) CONSTRAINT PK_Sections PRIMARY KEY,
    term_id       INT NOT NULL,
    course_id     INT NOT NULL,
    teacher_id    INT NOT NULL,
    section_code  VARCHAR(50) NULL,       -- VD: INT2201-01
    status        VARCHAR(20) NOT NULL CONSTRAINT DF_Sections_Status DEFAULT 'OPEN',
    CONSTRAINT FK_Sections_Terms    FOREIGN KEY (term_id) REFERENCES dbo.Terms(term_id),
    CONSTRAINT FK_Sections_Courses  FOREIGN KEY (course_id) REFERENCES dbo.Courses(course_id),
    CONSTRAINT FK_Sections_Teachers FOREIGN KEY (teacher_id) REFERENCES dbo.Teachers(teacher_id)
);

/* =========================
   3) SectionStudents: list students per section (import Excel)
   ========================= */
CREATE TABLE dbo.SectionStudents (
    section_id   INT NOT NULL,
    student_id   INT NOT NULL,
    added_by     INT NULL, -- user_id of admin/teacher who imported
    added_at     DATETIME2 NOT NULL CONSTRAINT DF_SS_AddedAt DEFAULT SYSUTCDATETIME(),
    source       VARCHAR(20) NOT NULL CONSTRAINT DF_SS_Source DEFAULT 'EXCEL'
                 CONSTRAINT CK_SS_Source CHECK (source IN ('EXCEL','MANUAL')),
    status       VARCHAR(20) NOT NULL CONSTRAINT DF_SS_Status DEFAULT 'ACTIVE',
    CONSTRAINT PK_SectionStudents PRIMARY KEY (section_id, student_id),
    CONSTRAINT FK_SS_Sections FOREIGN KEY (section_id) REFERENCES dbo.Sections(section_id),
    CONSTRAINT FK_SS_Students FOREIGN KEY (student_id) REFERENCES dbo.Students(student_id),
    CONSTRAINT FK_SS_Users    FOREIGN KEY (added_by) REFERENCES dbo.Users(user_id)
);

/* =========================
   4) Grade rules & GPA scale
   ========================= */
CREATE TABLE dbo.GradeRules (
    rule_id        INT IDENTITY(1,1) CONSTRAINT PK_GradeRules PRIMARY KEY,
    course_id      INT NOT NULL,
    process_weight DECIMAL(5,4) NOT NULL,  -- VD: 0.4000
    final_weight   DECIMAL(5,4) NOT NULL,  -- VD: 0.6000
    rounding_scale INT NOT NULL CONSTRAINT DF_GradeRules_Round DEFAULT 1, -- 1 => round 1 decimal
    active         BIT NOT NULL CONSTRAINT DF_GradeRules_Active DEFAULT 1,
    CONSTRAINT FK_GradeRules_Courses FOREIGN KEY (course_id) REFERENCES dbo.Courses(course_id),
    CONSTRAINT CK_GradeRules_Weights CHECK (
        process_weight >= 0 AND final_weight >= 0 AND
        process_weight + final_weight = 1
    )
);

CREATE TABLE dbo.GpaScales (
    scale_id   INT IDENTITY(1,1) CONSTRAINT PK_GpaScales PRIMARY KEY,
    min_score  DECIMAL(4,2) NOT NULL,
    max_score  DECIMAL(4,2) NOT NULL,
    gpa_point  DECIMAL(3,2) NOT NULL CONSTRAINT CK_GpaScales_Point CHECK (gpa_point BETWEEN 0 AND 4),
    letter     VARCHAR(2) NULL,
    CONSTRAINT CK_GpaScales_Range CHECK (min_score <= max_score)
);

/* =========================
   5) Grades (QT, CK, total, gpa_point)
   IMPORTANT: enforce only students in SectionStudents can have grades
   ========================= */
CREATE TABLE dbo.Grades (
    grade_id      INT IDENTITY(1,1) CONSTRAINT PK_Grades PRIMARY KEY,
    section_id    INT NOT NULL,
    student_id    INT NOT NULL,
    process_score DECIMAL(4,2) NULL CONSTRAINT CK_Grades_Process CHECK (process_score BETWEEN 0 AND 10),
    final_score   DECIMAL(4,2) NULL CONSTRAINT CK_Grades_Final   CHECK (final_score BETWEEN 0 AND 10),
    total_score   DECIMAL(4,2) NULL CONSTRAINT CK_Grades_Total   CHECK (total_score BETWEEN 0 AND 10),
    gpa_point     DECIMAL(3,2) NULL CONSTRAINT CK_Grades_Gpa     CHECK (gpa_point BETWEEN 0 AND 4),
    updated_by    INT NULL,
    updated_at    DATETIME2 NOT NULL CONSTRAINT DF_Grades_UpdatedAt DEFAULT SYSUTCDATETIME(),

    -- One row per student per section
    CONSTRAINT UQ_Grades_SectionStudent UNIQUE (section_id, student_id),

    -- Basic FKs
    CONSTRAINT FK_Grades_Sections FOREIGN KEY (section_id) REFERENCES dbo.Sections(section_id),
    CONSTRAINT FK_Grades_Students FOREIGN KEY (student_id) REFERENCES dbo.Students(student_id),
    CONSTRAINT FK_Grades_Teachers FOREIGN KEY (updated_by) REFERENCES dbo.Teachers(teacher_id),

    -- Key constraint: grade must correspond to a student enrolled in that section
    CONSTRAINT FK_Grades_SectionStudents FOREIGN KEY (section_id, student_id)
        REFERENCES dbo.SectionStudents(section_id, student_id)
);

/* =========================
   6) TermGpa (GPA per term, weighted by credits)
   ========================= */
CREATE TABLE dbo.TermGpa (
    term_id           INT NOT NULL,
    student_id        INT NOT NULL,
    gpa_value         DECIMAL(3,2) NULL CONSTRAINT CK_TermGpa_Value CHECK (gpa_value BETWEEN 0 AND 4),
    credits_attempted INT NULL,
    credits_earned    INT NULL,
    updated_at        DATETIME2 NOT NULL CONSTRAINT DF_TermGpa_UpdatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_TermGpa PRIMARY KEY (term_id, student_id),
    CONSTRAINT FK_TermGpa_Terms    FOREIGN KEY (term_id) REFERENCES dbo.Terms(term_id),
    CONSTRAINT FK_TermGpa_Students FOREIGN KEY (student_id) REFERENCES dbo.Students(student_id)
);

/* =========================
   7) Alerts, Reminders, Notifications
   ========================= */
CREATE TABLE dbo.Alerts (
    alert_id         INT IDENTITY(1,1) CONSTRAINT PK_Alerts PRIMARY KEY,
    student_id       INT NOT NULL,
    term_id          INT NULL,
    section_id       INT NULL,
    alert_type       VARCHAR(30) NOT NULL, -- VD: LOW_TOTAL, LOW_GPA, LOW_FINAL...
    severity         VARCHAR(10) NOT NULL CONSTRAINT CK_Alerts_Severity CHECK (severity IN ('LOW','MEDIUM','HIGH')),
    threshold_value  DECIMAL(6,2) NULL,
    actual_value     DECIMAL(6,2) NULL,
    reason           NVARCHAR(500) NULL,
    status           VARCHAR(20) NOT NULL CONSTRAINT DF_Alerts_Status DEFAULT 'NEW'
                     CONSTRAINT CK_Alerts_Status CHECK (status IN ('NEW','SENT','VIEWED','IN_PROGRESS','CLOSED')),
    created_at       DATETIME2 NOT NULL CONSTRAINT DF_Alerts_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Alerts_Students FOREIGN KEY (student_id) REFERENCES dbo.Students(student_id),
    CONSTRAINT FK_Alerts_Terms    FOREIGN KEY (term_id) REFERENCES dbo.Terms(term_id),
    CONSTRAINT FK_Alerts_Sections FOREIGN KEY (section_id) REFERENCES dbo.Sections(section_id),
    CONSTRAINT CK_Alerts_Target CHECK (term_id IS NOT NULL OR section_id IS NOT NULL)
);

CREATE TABLE dbo.Reminders (
    reminder_id  INT IDENTITY(1,1) CONSTRAINT PK_Reminders PRIMARY KEY,
    alert_id     INT NOT NULL,
    advisor_id   INT NOT NULL,
    message      NVARCHAR(1000) NOT NULL,
    sent_at      DATETIME2 NOT NULL CONSTRAINT DF_Reminders_SentAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Reminders_Alerts   FOREIGN KEY (alert_id) REFERENCES dbo.Alerts(alert_id),
    CONSTRAINT FK_Reminders_Advisors FOREIGN KEY (advisor_id) REFERENCES dbo.Advisors(advisor_id)
);

CREATE TABLE dbo.Notifications (
    notification_id  INT IDENTITY(1,1) CONSTRAINT PK_Notifications PRIMARY KEY,
    user_id          INT NOT NULL,
    title            NVARCHAR(200) NOT NULL,
    content          NVARCHAR(2000) NOT NULL,
    related_alert_id INT NULL,
    is_read          BIT NOT NULL CONSTRAINT DF_Notifications_IsRead DEFAULT 0,
    created_at       DATETIME2 NOT NULL CONSTRAINT DF_Notifications_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Notifications_Users  FOREIGN KEY (user_id) REFERENCES dbo.Users(user_id),
    CONSTRAINT FK_Notifications_Alerts FOREIGN KEY (related_alert_id) REFERENCES dbo.Alerts(alert_id)
);
GO

/* Helpful indexes (optional) */
CREATE INDEX IX_Sections_Teacher ON dbo.Sections(teacher_id, term_id);
CREATE INDEX IX_Grades_Student   ON dbo.Grades(student_id, section_id);
CREATE INDEX IX_Alerts_Student   ON dbo.Alerts(student_id, status, created_at);
GO