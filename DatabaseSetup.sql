IF DB_ID(N'HospitalDB') IS NULL
BEGIN
    CREATE DATABASE HospitalDB;
END
GO

USE HospitalDB;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        UserID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        Username VARCHAR(50) NOT NULL CONSTRAINT UQ_Users_Username UNIQUE,
        PasswordHash VARCHAR(256) NOT NULL,
        FullName VARCHAR(100) NOT NULL,
        Role VARCHAR(20) NOT NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSDATETIME(),
        CONSTRAINT CK_Users_Role CHECK (Role IN ('Admin', 'Doctor', 'Receptionist'))
    );
END
GO

IF OBJECT_ID(N'dbo.Doctors', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Doctors
    (
        DoctorID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Doctors PRIMARY KEY,
        FullName VARCHAR(100) NOT NULL,
        Specialization VARCHAR(100) NOT NULL,
        Phone VARCHAR(20) NOT NULL,
        Email VARCHAR(100) NULL,
        ConsultationFee DECIMAL(10,2) NOT NULL,
        CONSTRAINT CK_Doctors_Fee CHECK (ConsultationFee >= 0)
    );
END
GO

IF OBJECT_ID(N'dbo.Patients', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Patients
    (
        PatientID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Patients PRIMARY KEY,
        FullName VARCHAR(100) NOT NULL,
        Gender VARCHAR(10) NOT NULL,
        DOB DATE NOT NULL,
        Phone VARCHAR(20) NOT NULL,
        Address VARCHAR(255) NOT NULL,
        BloodGroup VARCHAR(5) NOT NULL,
        RegisteredAt DATETIME2 NOT NULL CONSTRAINT DF_Patients_RegisteredAt DEFAULT SYSDATETIME(),
        CONSTRAINT CK_Patients_Gender CHECK (Gender IN ('Male','Female','Other'))
    );
END
GO

IF OBJECT_ID(N'dbo.Rooms', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Rooms
    (
        RoomID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Rooms PRIMARY KEY,
        RoomNumber VARCHAR(20) NOT NULL CONSTRAINT UQ_Rooms_RoomNumber UNIQUE,
        RoomType VARCHAR(50) NOT NULL,
        DailyRate DECIMAL(10,2) NOT NULL,
        Status VARCHAR(20) NOT NULL CONSTRAINT DF_Rooms_Status DEFAULT 'Available',
        CONSTRAINT CK_Rooms_Type CHECK (RoomType IN ('General','ICU','Private')),
        CONSTRAINT CK_Rooms_Rate CHECK (DailyRate >= 0),
        CONSTRAINT CK_Rooms_Status CHECK (Status IN ('Available','Occupied'))
    );
END
GO

IF OBJECT_ID(N'dbo.Appointments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Appointments
    (
        AppointmentID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Appointments PRIMARY KEY,
        PatientID INT NOT NULL,
        DoctorID INT NOT NULL,
        AppointmentDate DATETIME2 NOT NULL,
        Status VARCHAR(20) NOT NULL CONSTRAINT DF_Appointments_Status DEFAULT 'Scheduled',
        Diagnosis VARCHAR(500) NULL,
        CONSTRAINT FK_Appointments_Patients FOREIGN KEY (PatientID) REFERENCES dbo.Patients(PatientID) ON DELETE CASCADE,
        CONSTRAINT FK_Appointments_Doctors FOREIGN KEY (DoctorID) REFERENCES dbo.Doctors(DoctorID),
        CONSTRAINT CK_Appointments_Status CHECK (Status IN ('Scheduled','Completed','Cancelled'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Appointments_DoctorDate_Scheduled' AND object_id = OBJECT_ID(N'dbo.Appointments'))
BEGIN
    CREATE UNIQUE INDEX UX_Appointments_DoctorDate_Scheduled ON dbo.Appointments(DoctorID, AppointmentDate) WHERE Status = 'Scheduled';
END
GO

IF OBJECT_ID(N'dbo.Billings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Billings
    (
        BillID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Billings PRIMARY KEY,
        PatientID INT NOT NULL,
        DoctorFee DECIMAL(10,2) NOT NULL,
        RoomFee DECIMAL(10,2) NOT NULL,
        MedicineFee DECIMAL(10,2) NOT NULL,
        TotalAmount AS (DoctorFee + RoomFee + MedicineFee) PERSISTED,
        PaymentStatus VARCHAR(20) NOT NULL CONSTRAINT DF_Billings_PaymentStatus DEFAULT 'Unpaid',
        BillDate DATETIME2 NOT NULL CONSTRAINT DF_Billings_BillDate DEFAULT SYSDATETIME(),
        CONSTRAINT FK_Billings_Patients FOREIGN KEY (PatientID) REFERENCES dbo.Patients(PatientID),
        CONSTRAINT CK_Billings_Fees CHECK (DoctorFee >= 0 AND RoomFee >= 0 AND MedicineFee >= 0),
        CONSTRAINT CK_Billings_PaymentStatus CHECK (PaymentStatus IN ('Paid','Unpaid'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'admin')
BEGIN
    INSERT INTO dbo.Users (Username, PasswordHash, FullName, Role)
    VALUES ('admin', 'PBKDF2$100000$EE146728DB32CDD0E0B27BF0437AEB4F$086BAC4068D29F333FB5C45A335D212FB3B2503F812067CE65A87E29B914B297', 'System Administrator', 'Admin');
END

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'doc_john')
BEGIN
    INSERT INTO dbo.Users (Username, PasswordHash, FullName, Role)
    VALUES ('doc_john', 'PBKDF2$100000$9716702269145B5772D03E9C0B860B05$CC6D501FD0A09A2037BDAB4CCA07835DA5826BBFB6505D821995FDCF6A456457', 'Dr. John Smith', 'Doctor');
END

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'rec_alice')
BEGIN
    INSERT INTO dbo.Users (Username, PasswordHash, FullName, Role)
    VALUES ('rec_alice', 'PBKDF2$100000$D1339F297F1E27494E508EBCB3F67821$AEE16ACA3D520F15A3C10804DF036CBA061100246083DA57C3FC7045D9ACD576', 'Alice Johnson', 'Receptionist');
END

IF NOT EXISTS (SELECT 1 FROM dbo.Doctors WHERE Phone = '0911000001')
INSERT INTO dbo.Doctors (FullName, Specialization, Phone, Email, ConsultationFee) VALUES ('Dr. John Smith', 'Cardiology', '0911000001', 'john.smith@hospital.local', 800.00);
IF NOT EXISTS (SELECT 1 FROM dbo.Doctors WHERE Phone = '0911000002')
INSERT INTO dbo.Doctors (FullName, Specialization, Phone, Email, ConsultationFee) VALUES ('Dr. Sarah Lee', 'Pediatrics', '0911000002', 'sarah.lee@hospital.local', 600.00);
IF NOT EXISTS (SELECT 1 FROM dbo.Doctors WHERE Phone = '0911000003')
INSERT INTO dbo.Doctors (FullName, Specialization, Phone, Email, ConsultationFee) VALUES ('Dr. Michael Brown', 'General Medicine', '0911000003', 'michael.brown@hospital.local', 500.00);

IF NOT EXISTS (SELECT 1 FROM dbo.Rooms WHERE RoomNumber = 'G-101')
INSERT INTO dbo.Rooms (RoomNumber, RoomType, DailyRate, Status) VALUES ('G-101', 'General', 300.00, 'Available');
IF NOT EXISTS (SELECT 1 FROM dbo.Rooms WHERE RoomNumber = 'G-102')
INSERT INTO dbo.Rooms (RoomNumber, RoomType, DailyRate, Status) VALUES ('G-102', 'General', 300.00, 'Available');
IF NOT EXISTS (SELECT 1 FROM dbo.Rooms WHERE RoomNumber = 'ICU-01')
INSERT INTO dbo.Rooms (RoomNumber, RoomType, DailyRate, Status) VALUES ('ICU-01', 'ICU', 1500.00, 'Available');
IF NOT EXISTS (SELECT 1 FROM dbo.Rooms WHERE RoomNumber = 'P-201')
INSERT INTO dbo.Rooms (RoomNumber, RoomType, DailyRate, Status) VALUES ('P-201', 'Private', 1000.00, 'Available');
IF NOT EXISTS (SELECT 1 FROM dbo.Rooms WHERE RoomNumber = 'P-202')
INSERT INTO dbo.Rooms (RoomNumber, RoomType, DailyRate, Status) VALUES ('P-202', 'Private', 1000.00, 'Available');

IF NOT EXISTS (SELECT 1 FROM dbo.Patients WHERE Phone = '0912000001')
INSERT INTO dbo.Patients (FullName, Gender, DOB, Phone, Address, BloodGroup) VALUES ('Abebe Kebede', 'Male', '1995-05-12', '0912000001', 'Addis Ababa', 'O+');
IF NOT EXISTS (SELECT 1 FROM dbo.Patients WHERE Phone = '0912000002')
INSERT INTO dbo.Patients (FullName, Gender, DOB, Phone, Address, BloodGroup) VALUES ('Sara Ahmed', 'Female', '1998-08-20', '0912000002', 'Dire Dawa', 'A+');
IF NOT EXISTS (SELECT 1 FROM dbo.Patients WHERE Phone = '0912000003')
INSERT INTO dbo.Patients (FullName, Gender, DOB, Phone, Address, BloodGroup) VALUES ('Daniel Tesfaye', 'Male', '1987-11-03', '0912000003', 'Adama', 'B+');
GO
