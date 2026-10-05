# Hospital Management System

A modular C# .NET 8 Windows Forms hospital management system using SQL Server and pure ADO.NET.

## Architecture

- `Data`: connection factory and ADO.NET execution/transaction helpers.
- `Core`: domain models, validation, exceptions, repositories, services, authentication, sessions, and event logging.
- `UI`: programmatic Windows Forms UI with role-based navigation.

## Setup

1. Install .NET 8 SDK and a SQL Server instance.
2. Open `DatabaseSetup.sql` in SQL Server Management Studio and execute it.
3. If SQL Server is not the local default instance, set the `HOSPITAL_DB_CONNECTION` environment variable to a valid SQL Server connection string.
4. Open `HospitalManagementSystem.csproj` in Visual Studio 2022+ and restore NuGet packages.
5. Build and run.

## Seed accounts

- Admin: `admin` / `admin123`
- Doctor: `doc_john` / `doc123`
- Receptionist: `rec_alice` / `rec123`

Passwords are stored as PBKDF2-SHA256 hashes. Change seeded credentials before production deployment.

## Notes

Appointments are protected by a database filtered unique index for scheduled doctor/date slots and an explicit transaction with locking. Bills are generated inside an explicit transaction. UI database calls are wrapped in exception handling and business/domain validation is performed before persistence.
