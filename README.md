# 🎬 Film Studio Database System

> A fully normalized relational database system for managing a Pakistani film production studio, built with **Oracle 11g** and **PL/SQL**, with a **C# WinForms** front-end application.

---

## 👩‍💻 Developed By

| Name | Roll No |
|---|---|
| Eshal Hussain | 24F-0597 |

> **FAST – National University of Computer & Emerging Sciences**
> Database Systems Lab — Spring 2025

---

## 📋 Project Overview

This system manages the complete lifecycle of a film production — from pre-production planning to post-production wrap. It covers movies, directors, actors, crew members, shooting locations, scenes, contracts, and production schedules — all based on the **Pakistani film industry**.

---

## 🗄️ Database

- **DBMS:** Oracle 11g
- **Tables:** 10 fully normalized tables
- **Sample Data:** 15–20 Pakistani film industry records per table
- **Normalization:** Up to Third Normal Form (3NF)

### Tables

| Table | Description |
|---|---|
| `director` | Director profiles and biography |
| `movie` | Core movie records linked to directors |
| `actor` | Actor profiles and contact info |
| `crew_member` | Technical and creative staff |
| `shooting_location` | Pakistani filming venues with daily rates |
| `cast_role` | Junction table linking actors to movies |
| `contract` | Legal agreements for actors, crew, directors |
| `scene` | Individual scenes per movie |
| `crew_assignment` | Crew assigned to specific movies |
| `schedule` | Shoot dates and times per scene |

---

## ⚙️ PL/SQL Features

### 🔔 Triggers (7)

| Trigger | Event | Purpose |
|---|---|---|
| `trg_set_release_year` | BEFORE UPDATE ON movie | Auto-sets release year when status → Released |
| `trg_protect_director` | BEFORE DELETE ON director | Blocks deletion of directors with active movies |
| `trg_high_value_contract` | AFTER INSERT ON contract | Logs contracts exceeding PKR 10 million |
| `trg_movie_status_audit` | AFTER UPDATE OF status ON movie | Tracks every production phase change |
| `trg_prevent_location_overlap` | BEFORE INSERT OR UPDATE ON schedule | Prevents double-booking of shooting locations |
| `trg_check_crew_dates` | BEFORE INSERT OR UPDATE ON crew_assignment | Enforces valid start/end date order |
| `trg_check_positive_budget` | BEFORE INSERT OR UPDATE ON movie | Rejects zero or negative movie budgets |

### 🔧 Stored Procedures (6)

| Procedure | Description |
|---|---|
| `proc_get_cast(movie_id)` | Prints full cast list for a given movie |
| `proc_assign_crew(...)` | Assigns a crew member to a movie |
| `proc_movie_summary(movie_id)` | Prints full production dashboard |
| `proc_delay_production(movie_id, days)` | Shifts all schedules forward by N days |
| `proc_terminate_crew(crew_id, movie_id)` | Ends a crew member's assignment early |
| `proc_estimate_location_costs(movie_id)` | Calculates total location spend for a movie |

### 🧮 Functions (2)

| Function | Returns |
|---|---|
| `func_get_remaining_budget(movie_id)` | Budget minus total contract spend |
| `func_location_booked_days(location_id)` | Total distinct booked days for a location |

### 👁️ Views (7)

| View | Description |
|---|---|
| `vw_movie_overview` | Movie + director details in one query |
| `vw_full_cast` | All actors, characters and billing per film |
| `vw_active_schedule` | Scheduled and in-progress shoot entries |
| `vw_movie_financials` | Budget vs contract spend per movie |
| `vw_location_utilization` | Scene count and screen time per location |
| `vw_director_portfolio` | Movie count and total budget per director |
| `vw_daily_call_sheet` | Shoot dates with call time and wrap time |

---

## 🔍 SQL Queries

- **6 Joins** — Inner Join, Left Join, Multi-table Join, Full Outer Join
- **8 Subqueries** — IN, NOT IN, AVG scalar, Nested, and 5 Correlated Subqueries

---

## 🖥️ C# WinForms Application

A desktop front-end built with **C# .NET** connecting to Oracle via `Oracle.ManagedDataAccess`.

### Features
- ✅ Role-based login — Admin, Director, Actor, Crew
- ✅ Auto-detects Oracle port (1521 / 1522) and SID (xe / orcl) on startup
- ✅ Each role sees only their relevant data
- ✅ Session management with logout and re-login support

### Sample Login Credentials

| Role | Username | Password |
|---|---|---|
| Admin | `admin` | `admin123` |
| Director | `shoaib.mansoor` | `dir001` |
| Actor | `fawad.khan` | `act001` |
| Crew | `rana.kamran` | `crew001` |

---

## 🚀 How to Run

### 1. Database Setup

```sql
-- Run in Oracle SQL Developer (as script — press F5)

-- Step 1: Create all tables, insert data, views, triggers, procedures
@film_studio_complete.sql

-- Step 2: Create login users for the WinForms app
@00_add_login_table.sql
```

### 2. WinForms Application

```
1. Open FilmStudioWinForms.csproj in Visual Studio 2022
2. Make sure Oracle 11g is running on localhost
3. Build the solution (Ctrl + Shift + B)
4. Run (F5) — the app auto-detects Oracle port and SID
```

### Requirements

| Tool | Version |
|---|---|
| Oracle Database | 11g XE |
| Oracle.ManagedDataAccess | NuGet package |
| .NET | 6 or later |
| Visual Studio | 2022 |

---

## 📁 Project Structure

```
FilmStudio-OracleDB/
│
├── 📄 film_studio_complete.sql        # Full DB script — all 10 phases
├── 📄 00_add_login_table.sql          # Login user table + 46 sample users
│
├── 💻 FilmStudioWinForms.csproj       # C# project file
├── 💻 Program.cs                      # App entry point + login loop
├── 💻 LoginForm.cs                    # Login UI with role selector
├── 💻 Form1.cs                        # Main dashboard form
├── 💻 Form1.Designer.cs               # Designer-generated UI code
├── 💻 DatabaseHelper.cs               # Oracle connection + session manager
│
└── 📖 README.md                       # This file
```

---

## 📚 References

- Elmasri & Navathe — *Fundamentals of Database Systems*, 7th Edition
- Oracle 11g SQL Language Reference — docs.oracle.com
- Oracle PL/SQL Language Reference 11g Release 2
- Pakistani Film Industry — HUM Films, ARY Films, Evernew Studios Lahore

---

## ⚠️ Academic Integrity

This project was developed as part of the **Database Systems Lab** course at FAST-NUCES.
SQL and PL/SQL code was written by the group members. Data generation using AI tools was used only for sample data as permitted by course policy.

---

<div align="center">
  Made with by Eshal Hussain
</div>
