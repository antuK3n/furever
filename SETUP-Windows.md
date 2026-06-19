# 🐾 FurEver — Windows Setup Guide

Run FurEver on **Windows** using a **native SQL Server** install — no Docker needed.

> **New here?** Just follow Steps 1 → 5 in order. Each step takes a few minutes.

---

## ✅ Before you start

Install these first (one-time setup):

| Tool | Why you need it | Download |
|------|-----------------|----------|
| **Visual Studio 2022** (17.14+) or **2026** | Open & run the project | [visualstudio.microsoft.com](https://visualstudio.microsoft.com/) |
| **.NET 10 SDK** | The project targets `net10.0` | [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/10.0) |
| **SQL Server 2022** (Developer or Express) | The database — both are free | [sql-server-downloads](https://www.microsoft.com/sql-server/sql-server-downloads) |
| **SSMS** or **Azure Data Studio** | Run the database scripts | [Download SSMS](https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms) |
| **Git** or **GitHub Desktop** | Clone the repo | [git-scm.com](https://git-scm.com/) |

> **Check .NET is ready:** open a terminal and run `dotnet --version` — it should print `10.x`.

---

## 1️⃣ Get the code

**In Visual Studio:**

1. **Clone a repository**
2. Paste this URL: `https://github.com/antuK3n/furever.git`
3. Pick a folder → **Clone**

> Already cloned? Use **File → Open → Project/Solution** and pick `furever/FurEver.sln`.

---

## 2️⃣ Install & start SQL Server

1. Run the **SQL Server 2022** installer → choose **Basic** setup.
2. After it finishes, note your **instance name** — you'll need it in Step 4:

   | Edition | Instance name |
   |---------|---------------|
   | **Express** | `SQLEXPRESS` |
   | **Developer** | *default* (no suffix) |

3. SQL Server starts **automatically** as a Windows service — nothing else to launch.

> **Verify it's running:** open `services.msc` and look for **SQL Server (SQLEXPRESS)** or
> **SQL Server (MSSQLSERVER)** with status **Running**.

---

## 3️⃣ Create the database

1. Open **SSMS** and connect to your instance:
   - **Server name:** `localhost\SQLEXPRESS` (Express) or `localhost` (Developer)
   - **Authentication:** Windows Authentication
2. **File → Open → File** → open `database/01_schema.sql` → click **Execute** ▶️
   *(creates the tables, triggers, and stored procedures)*
3. Open `database/02_seed.sql` → click **Execute** ▶️
   *(loads the sample data)*

> ⚠️ **Don't skip the Execute step.** Just opening the file isn't enough — you must click
> **Execute** for each script, `01_schema.sql` **first**, then `02_seed.sql`.

---

## 4️⃣ Set the connection string

Open **`FurEver.Web/appsettings.json`** and replace the `FurEver` connection string with the
one that matches **your** instance from Step 2.

**SQL Server Express:**
```json
"ConnectionStrings": {
  "FurEver": "Server=localhost\\SQLEXPRESS;Database=FurEver;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

**Developer / default instance:**
```json
"ConnectionStrings": {
  "FurEver": "Server=localhost;Database=FurEver;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

> 💡 **Tips**
> - `Trusted_Connection=True` signs in with your Windows account — **no password to manage**.
> - In JSON, use **two backslashes** `\\` (e.g. `localhost\\SQLEXPRESS`).
> - Use the **same `Server=` value you used to connect in SSMS** (Step 3).
> - If you see a string with `User Id=sa;Password=...` — that's for a different setup. **Replace it** with one of the above.

---

## 5️⃣ Run the app

In a terminal at the project folder:

```bash
cd FurEver.Web
dotnet run
```

Then open **<http://localhost:5190>** in your browser. 🎉

> A default admin account is created automatically on first startup.

---

## 🔑 Where to log in

| URL | Purpose |
|-----|---------|
| <http://localhost:5190> | Main website |
| <http://localhost:5190/Account/Register> | Adopter (member) sign up |
| <http://localhost:5190/Account/Login> | Adopter login |
| <http://localhost:5190/Admin/Login> | Admin panel login |

**Default admin account**
- **Email:** `admin@furever.com`
- **Password:** `admin123`

> Adopters and admins use **separate** login pages. Members sign up at `/Account/Register`;
> staff sign in at `/Admin/Login`.

---

## 🛠️ Troubleshooting

<details>
<summary><strong>"Can't reach this page" / connection timed out / SqlException</strong></summary>

The app can't reach the database. Check that:
- The SQL Server service is **running** (`services.msc`).
- The `Server=...` value **exactly matches** your instance (`localhost\SQLEXPRESS` vs `localhost`).
- `TrustServerCertificate=True` is in the connection string.
- You ran **Step 3** so the `FurEver` database actually exists.
</details>

<details>
<summary><strong>"Login failed for user"</strong></summary>

You're using **Windows Authentication**, so the account running Visual Studio needs access.
With a fresh Developer/Express install, your Windows user is a sysadmin by default — make sure
you're connecting with the same account.
</details>

<details>
<summary><strong>Database is empty / "Invalid object name" / tables don't exist</strong></summary>

Re-run **Step 3**: execute `01_schema.sql` first, then `02_seed.sql`.
</details>

<details>
<summary><strong>"Port 5190 already in use"</strong></summary>

Another instance is still running. Stop it (or end the `FurEver.Web` process in Task Manager),
then run again.
</details>

<details>
<summary><strong>Build error mentioning .NET 10</strong></summary>

Install the **.NET 10 SDK** (see the table at the top), open a **new** terminal, and run again.
</details>

---

## 🧱 Tech stack

- **Framework:** ASP.NET Core MVC (.NET 10)
- **ORM:** Entity Framework Core 10 (SQL Server provider)
- **Database:** Microsoft SQL Server 2022
- **Auth:** Cookie authentication + BCrypt password hashing
- **Frontend:** Razor views + custom design-system CSS
