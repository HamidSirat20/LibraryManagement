# LibraryManagement

A .NET 8 Web API for library reservation and loan workflows. 
Users can reserve books (FIFO queueing), cancel reservations, 
pick up reservations (creates loans), and list reservations 
(per-user and admin views). The service layer enforces business 
rules (no duplicate reservations, cannot reserve available books), 
uses `EF Core` for persistence, and maps entities to read DTOs (e.g., 
`ReservationReadDto`) to avoid leaking navigation properties.

Repository: https://github.com/HamidSirat20/LibraryManagement

## Key features (short)
- Create FIFO reservations with validations (no duplicate reservations, cannot reserve available books).  
- Cancel reservations with automatic queue reordering.  
- Fulfill (pickup) reservations → creates loans and marks reservation fulfilled.  
- Per-user and admin listing endpoints (search, sort, pagination).  
- DTO mapping (`ReservationReadDto`) to shape API responses and avoid returning navigation properties.  
- Pluggable email notifications and structured logging via `ILogger`.  
- Unit tests for core service behavior started; more unit/integration tests planned.

## Technology stack
- .NET 8 / ASP.NET Core Web API  
- Entity Framework Core (EF Core)  
- Manual mappers for DTO mapping  
- ILogger for structured logging  
- Optional SMTP/email provider abstraction via `IEmailService`  
- xUnit / NUnit (unit tests — started)

## Getting started

Prerequisites
- .NET 8 SDK
- A relational database (PostgreSQL) — connection configured in `appsettings.json`
- (Optional) SMTP credentials for email notifications
