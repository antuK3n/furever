# FurEver

A pet adoption and shelter management platform built with ASP.NET Core MVC and SQL Server. The public can browse adoptable animals and read each pet's medical history; registered adopters can favorite pets and submit adoption applications; shelter staff get a full administrative back office for managing pets, adoptions, adopters, veterinary records, vaccinations, and reporting.

## Tech stack

- **.NET 10** — ASP.NET Core MVC
- **Entity Framework Core 10** (SQL Server provider)
- **Microsoft SQL Server 2022**
- **Cookie authentication** (7-day sliding expiration)
- **BCrypt.Net-Next** for password hashing
- Razor views + Bootstrap, jQuery

## Project structure

```
FurEver.Web/        ASP.NET Core MVC application
  Areas/Admin/       Admin back office (dashboard, CRUD, reports)
  Controllers/       Public + adopter controllers
  Data/              EF Core DbContext
  Models/            Entities + view models
  Views/             Razor views and shared partials
  wwwroot/           Static assets, design system CSS, uploads
database/
  01_schema.sql      DDL, triggers, stored procedures, constraints
  02_seed.sql        Sample data
```

## Database

A relational SQL Server database whose integrity is enforced at the database level through triggers, stored procedures, and check constraints. The core workflow — a pet moving from *Available* → *Reserved* → *Adopted* — is driven automatically by triggers on the `Adoption` table.

- **7 tables:** Pet, Adopter, Adoption, Veterinary_Visit, Vaccination, Favorite, Admin
- **5 triggers** managing the adoption lifecycle and favorite cleanup
- **3 stored procedures** for available-pet lookup, monthly stats, and overdue-vaccination tracking

## Getting started

1. Start SQL Server (e.g. via Docker) and apply the scripts in `database/`:

   ```sh
   sqlcmd -S localhost -U sa -P "<password>" -i database/01_schema.sql
   sqlcmd -S localhost -U sa -P "<password>" -i database/02_seed.sql
   ```

2. Update the `FurEver` connection string in `FurEver.Web/appsettings.json`.

3. Run the app:

   ```sh
   cd FurEver.Web
   dotnet run
   ```

A default admin account (`admin@furever.com`) is seeded on first startup.
