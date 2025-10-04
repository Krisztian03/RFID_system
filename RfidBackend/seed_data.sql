USE RFIDSystem;
GO

SET IDENTITY_INSERT Employees ON;

INSERT INTO Employees (Id, Name, RfidUid, HourlyRate, Role) VALUES
(1, 'Admin User', 'ADMIN-001', 0, 'Admin'),
(2, 'Kovács János', 'AA:BB:CC:DD', 2500, 'Worker'),
(3, 'Nagy Anna', '11:22:33:44', 2800, 'Worker');


SET IDENTITY_INSERT Employees OFF;
GO

SET IDENTITY_INSERT Users ON;

INSERT INTO Users (Id, Email, PasswordHash, Role, EmployeeId, CreatedAt) VALUES
(1, 'admin@rfid.com', '$2a$11$p8W45BSQVANsDL67CdgG7uRc80uA26EKclDa4pfJ9zc.fLQ1Dw3VO', 'Admin', 1, GETUTCDATE()),
(2, 'kovacs.janos@rfid.com', '$2a$11$uSXpy8PBH.dx8oLrSWxkOOcDIORjKdvhW7vR9TTVD6ggHEQrJQOvy', 'Worker', 2, GETUTCDATE()),
(3, 'nagy.anna@rfid.com', '$2a$11$/0DbkJ2X1xIbsLligkqxSO55PpqG8xHUq9XCUBu1iKKdvk7TaH2m6', 'Worker', 3, GETUTCDATE());


SET IDENTITY_INSERT Users OFF;
GO


-- ACCESS LOGS (Utolsó 15 munkanap)

DECLARE @StartDate DATE = DATEADD(DAY, -20, GETDATE());
DECLARE @CurrentDate DATE = @StartDate;

WHILE @CurrentDate <= GETDATE()
BEGIN
    IF DATEPART(WEEKDAY, @CurrentDate) BETWEEN 2 AND 6
    BEGIN
        -- Kovács János
        INSERT INTO AccessLogs (EmployeeId, TimestampUtc, Type, RawUid) VALUES
        (2, DATEADD(MINUTE, (ABS(CHECKSUM(NEWID())) % 60) - 30, DATEADD(HOUR, 8, CAST(@CurrentDate AS DATETIME))), 0, 'AA:BB:CC:DD'),
        (2, DATEADD(MINUTE, (ABS(CHECKSUM(NEWID())) % 60), DATEADD(HOUR, 16, CAST(@CurrentDate AS DATETIME))), 1, 'AA:BB:CC:DD');
        
        -- Nagy Anna
        INSERT INTO AccessLogs (EmployeeId, TimestampUtc, Type, RawUid) VALUES
        (3, DATEADD(MINUTE, (ABS(CHECKSUM(NEWID())) % 60) - 30, DATEADD(HOUR, 8, CAST(@CurrentDate AS DATETIME))), 0, '11:22:33:44'),
        (3, DATEADD(MINUTE, (ABS(CHECKSUM(NEWID())) % 60), DATEADD(HOUR, 16, CAST(@CurrentDate AS DATETIME))), 1, '11:22:33:44');
    END
    
    SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate);
END
GO

PRINT 'Adatok feltöltve!';
SELECT COUNT(*) AS Employees FROM Employees;
SELECT COUNT(*) AS Users FROM Users;
SELECT COUNT(*) AS AccessLogs FROM AccessLogs;
GO