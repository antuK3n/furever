# FurEver — Pet Adoption & Shelter Management

A full-stack web application for managing pet adoptions, built with **ASP.NET Core MVC** and **SQL Server**. The public can browse adoptable pets and read each pet's medical history, registered adopters can favorite pets and submit adoption applications, and shelter staff get a full admin back office for managing pets, adoptions, adopters, vet records, vaccinations, and reports.

---

## Getting Started (For Collaborators)

### Prerequisites

Make sure you have these installed:
- **.NET 10 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Docker Desktop** (easiest way to run SQL Server) - [Download](https://www.docker.com/products/docker-desktop/)
  - *Alternative:* a local **SQL Server 2022** install
- **Git** - [Download](https://git-scm.com/)

> Check your .NET version with `dotnet --version` — it should print `10.x`.

---

## Option 1: Clone via Command Line (CLI)

```bash
# 1. Clone the repository
git clone https://github.com/antuK3n/furever.git

# 2. Navigate into the project
cd furever
```

---

## Option 2: Clone via GitHub Desktop

1. Open **GitHub Desktop**
2. Click **File** > **Clone Repository**
3. Go to the **URL** tab
4. Paste: `https://github.com/antuK3n/furever.git`
5. Choose where to save it on your computer
6. Click **Clone**

---

## Project Setup

### Step 1: Start SQL Server (with Docker)

Run a SQL Server 2022 container. The password below matches the one already in the project's connection string, so everything will work out of the box:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=FurEver2026Db" \
  -p 1433:1433 --name furever-sql \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

Give it a few seconds to finish starting up. You can check it's running with:

```bash
docker ps
```

> Already created the container before? Just start it again with `docker start furever-sql`.

### Step 2: Create the Database

Copy the SQL scripts into the container and run them. This creates the database, tables, triggers, stored procedures, and sample data.

```bash
# 1. Copy the scripts into the container
docker cp database/01_schema.sql furever-sql:/tmp/01_schema.sql
docker cp database/02_seed.sql   furever-sql:/tmp/02_seed.sql

# 2. Run the schema (tables, triggers, stored procedures)
docker exec furever-sql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "FurEver2026Db" -C -i /tmp/01_schema.sql

# 3. Run the seed data (sample pets, adopters, etc.)
docker exec furever-sql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "FurEver2026Db" -C -i /tmp/02_seed.sql
```

> Prefer a GUI? You can also open `database/01_schema.sql` and `database/02_seed.sql` in **Azure Data Studio** or **SSMS** and run them against `localhost,1433`.

### Step 3: Check the Connection String

Open `FurEver.Web/appsettings.json` and confirm the `FurEver` connection string matches your SQL Server:

```json
"ConnectionStrings": {
  "FurEver": "Server=localhost,1433;Database=FurEver;User Id=sa;Password=FurEver2026Db;TrustServerCertificate=True;"
}
```

If you used a different password in Step 1, update it here to match.

### Step 4: Run the Application

```bash
cd FurEver.Web
dotnet run
```

The app runs at: **http://localhost:5190**

A default admin account is created automatically on first startup.

---

## Project Structure

```
furever/
├── FurEver.Web/              # ASP.NET Core MVC application
│   ├── Areas/Admin/          # Admin back office (dashboard, CRUD, reports)
│   ├── Controllers/          # Public + adopter controllers
│   ├── Data/                 # EF Core DbContext
│   ├── Models/               # Entities + view models
│   ├── Views/                # Razor views and shared partials
│   ├── wwwroot/              # Static assets, design-system CSS, uploads
│   └── appsettings.json      # Connection string + config
└── database/
    ├── 01_schema.sql         # Tables, triggers, stored procedures, constraints
    └── 02_seed.sql           # Sample data
```

---

## Access Points

| URL | Description |
|-----|-------------|
| http://localhost:5190 | Main website |
| http://localhost:5190/Account/Login | Adopter (member) login |
| http://localhost:5190/Account/Register | Adopter sign up |
| http://localhost:5190/Admin/Login | Admin panel login |

### Default Admin Login
- **Email:** admin@furever.com
- **Password:** admin123

> Adopters and admins use **separate** login pages. Sign up as an adopter at `/Account/Register`; staff sign in at `/Admin/Login`.

---

## Common Issues

### "A connection was successfully established... (provider: ... error)" / login failed
- Make sure the SQL Server container is running: `docker ps`
- Confirm the password in `appsettings.json` matches the one from Step 1
- Wait a few seconds after starting the container — SQL Server takes a moment to accept connections

### Port already in use (5190)
Another instance is still running. Stop it, then run again:
```bash
lsof -ti :5190 | xargs kill -9
```

### Database is empty / tables don't exist
Re-run Step 2 to apply `01_schema.sql` and `02_seed.sql`.

### `dotnet` command not found or wrong version
Install the **.NET 10 SDK** (see Prerequisites) and confirm with `dotnet --version`.

---

## Tech Stack

- **Framework:** ASP.NET Core MVC (.NET 10)
- **ORM:** Entity Framework Core 10 (SQL Server provider)
- **Database:** Microsoft SQL Server 2022
- **Auth:** Cookie authentication (7-day sliding expiration), BCrypt password hashing
- **Frontend:** Razor views + custom design-system CSS

---

## Database

A relational SQL Server database whose integrity is enforced at the database level through triggers, stored procedures, and check constraints. The core workflow — a pet moving from *Available* → *Reserved* → *Adopted* — is driven automatically by triggers on the `Adoption` table.

- **7 tables:** Pet, Adopter, Adoption, Veterinary_Visit, Vaccination, Favorite, Admin
- **5 triggers** managing the adoption lifecycle and favorite cleanup
- **3 stored procedures** for available-pet lookup, monthly stats, and overdue-vaccination tracking
