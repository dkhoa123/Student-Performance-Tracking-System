Use SPTS;
ALTER TABLE Students
ADD DateOfBirth date NULL,
    Gender nvarchar(10) NULL,
    Phone nvarchar(20) NULL,
    Address nvarchar(255) NULL;