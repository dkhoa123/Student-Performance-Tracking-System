USE SPTS
CREATE TABLE dbo.SectionSchedules (
    schedule_id   INT IDENTITY(1,1) PRIMARY KEY,
    section_id    INT NOT NULL,
    day_of_week   VARCHAR(20) NOT NULL,      -- 'MONDAY', 'TUESDAY',... 
    start_period  INT NOT NULL,              -- Tiết bắt đầu (1-12)
    end_period    INT NOT NULL,              -- Tiết kết thúc
    start_time    TIME NULL,                 -- 07:00:00
    end_time      TIME NULL,                 -- 09:30:00
    room          NVARCHAR(50) NULL,
    
    CONSTRAINT FK_SectionSchedules_Sections 
        FOREIGN KEY (section_id) REFERENCES dbo.Sections(section_id),
    CONSTRAINT CK_SectionSchedules_Periods 
        CHECK (start_period <= end_period AND start_period BETWEEN 1 AND 12)
);