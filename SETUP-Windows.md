# FurEver — Windows Setup

A step-by-step guide to running FurEver on **Windows** with a **native SQL Server**
install (no Docker required).

---

## Prerequisites

Install these first:

- **Visual Studio 2022 (17.14 or newer) or Visual Studio 2026** — used to clone the project (Step 1).
- **.NET 10 SDK** — the project targets `net10.0`.
  - Download it from <https://dotnet.microsoft.com/download/dotnet/10.0>.
  - Verify in a terminal: `dotnet --version` should print `10.x`.
- **SQL Server 2022 (Developer or Express edition)** — free.
  - Download: <https://www.microsoft.com/sql-server/sql-server-downloads>
- **SQL Server Management Studio (SSMS)** or **Azure Data Studio** to run the database scripts.
  - SSMS: <https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms>
- **Git** (or GitHub Desktop) to clone the repository.

---

## Step 1 — Get the code

**In Visual Studio:**
1. Open Visual Studio → **Clone a repository**.
2. Repository location: `https://github.com/antuK3n/furever.git`
3. Choose a folder and click **Clone**.

Already cloned? Use **File → Open → Project/Solution** and pick `furever/FurEver.sln`.

---

## Step 2 — Install and start SQL Server

1. Run the **SQL Server 2022 Developer/Express** installer and choose the **Basic** setup.
2. When it finishes, note your **instance name**:
   - **Express** edition → instance is usually `SQLEXPRESS`
   - **Developer** (default) edition → default instance (no suffix)
3. SQL Server runs automatically as a Windows service after install — nothing else to start.

> You can confirm it's running in **Services** (`services.msc`) — look for
> **SQL Server (SQLEXPRESS)** or **SQL Server (MSSQLSERVER)** with status *Running*.

---

## Step 3 — Create the database

1. Open **SSMS** (or Azure Data Studio) and connect to your instance:
   - Express: `localhost\SQLEXPRESS`
   - Default: `localhost`
   - Authentication: **Windows Authentication**
2. Open `database/01_schema.sql` (**File → Open → File**) and click **Execute**.
   This creates the tables, triggers, and stored procedures.
3. Open `database/02_seed.sql` and click **Execute** to load the sample data.

---

## Step 4 — Set the connection string

Open `FurEver.Web/appsettings.json` and update the `FurEver` connection string to point at
your local instance using **Windows Authentication** (no password needed):

**SQL Server Express:**
```json
"ConnectionStrings": {
  "FurEver": "Server=localhost\\SQLEXPRESS;Database=FurEver;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

**Default instance (Developer edition):**
```json
"ConnectionStrings": {
  "FurEver": "Server=localhost;Database=FurEver;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

> `Trusted_Connection=True` signs in with your Windows account, so there's no SA password to manage.

---

## Step 5 — Run the app

Open a terminal in the project folder and run:

```bash
cd FurEver.Web
dotnet run
```

Then open **http://localhost:5190** in your browser.

A default admin account is created automatically on first startup.

---

## Access points

| URL | Purpose |
|-----|---------|
| http://localhost:5190 | Main website |
| http://localhost:5190/Account/Register | Adopter (member) sign up |
| http://localhost:5190/Account/Login | Adopter login |
| http://localhost:5190/Admin/Login | Admin panel login |

### Default admin login
- **Email:** `admin@furever.com`
- **Password:** `admin123`

> Adopters and admins use **separate** login pages. Sign up as an adopter at
> `/Account/Register`; staff sign in at `/Admin/Login`.

---

## Common issues

### Cannot connect / "A network-related or instance-specific error"
- Make sure the SQL Server service is running (`services.msc`).
- Confirm the `Server=...` value matches your instance name exactly
  (`localhost\SQLEXPRESS` vs `localhost`).
- Keep `TrustServerCertificate=True` in the connection string.

### Login failed for user
- You're using **Windows Authentication**, so the account running Visual Studio must have
  access. With a fresh Developer/Express install, your Windows user is a sysadmin by default.

### Database is empty / tables don't exist
- Re-run **Step 3** to apply `01_schema.sql` and then `02_seed.sql`.

### Port 5190 already in use
- Another instance is still running. Stop it (or end the `FurEver.Web` process in
  Task Manager) and run again.

### Build error mentioning .NET 10
- Install the **.NET 10 SDK** (see Prerequisites), then open a new terminal and run again.

---

## Tech stack

- **Framework:** ASP.NET Core MVC (.NET 10)
- **ORM:** Entity Framework Core 10 (SQL Server provider)
- **Database:** Microsoft SQL Server 2022
- **Auth:** Cookie authentication, BCrypt password hashing
- **Frontend:** Razor views + custom design-system CSS
