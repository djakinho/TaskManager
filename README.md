# Task Manager

A full-stack task management application built with .NET 8, ASP.NET Web API, and Angular.  
Developed as a technical interview exercise following Clean Architecture principles and TDD.

---

## User Story

> "As a developer, I want to manage my daily tasks so I can track what I need to do,  
> what I'm working on, and what I've completed — from any device."

---

## Architecture

```
TaskManager/
├── src/
│   ├── TaskManager.Domain/          # Entities, enums — no dependencies
│   ├── TaskManager.Application/     # Interfaces, services, DTOs, validation
│   ├── TaskManager.Infrastructure/  # ADO.NET repositories, DB access
│   └── TaskManager.WebApi/          # Controllers, JWT auth, Program.cs
├── tests/
│   └── TaskManager.Tests/           # xUnit + Moq unit tests
├── frontend/
│   └── task-manager-ui/             # Angular
├── sql/
│   ├── schema.sql                   # Table definitions
│   └── seed.sql                     # Demo data
└── README.md
```

### Layer responsibilities

| Layer          | Responsibility                                        | Depends on   |
|----------------|-------------------------------------------------------|--------------|
| Domain         | Pure entities and enums                               | Nothing      |
| Application    | Business rules, validation, interfaces                | Domain       |
| Infrastructure | Data access via ADO.NET (no ORM)                      | Application  |
| WebApi         | HTTP controllers, auth middleware, DI configuration   | Application  |

---

## Tech Stack

| Concern        | Choice                             |
|----------------|------------------------------------|
| Backend        | .NET 8, ASP.NET Web API            |
| Data access    | ADO.NET (SqlConnection/SqlCommand) |
| Database       | SQL Server (LocalDB)               |
| Auth           | JWT Bearer Token + BCrypt          |
| Tests          | xUnit, Moq, FluentAssertions       |
| Frontend       | Angular                            |

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Node.js 22 LTS](https://nodejs.org/)
- SQL Server LocalDB (included with Visual Studio) or SQL Server Express

Verify:
```bash
dotnet --version   # 8.x.x
node --version     # v22.x.x
sqlcmd -?          # confirm SQL tools available
```

---

## Setup

### 1. Clone and open
```bash
git clone https://github.com/YOUR_USERNAME/task-manager.git
cd task-manager
```

### 2. Database
```bash
# Run script to create Users and Tasks tables
sqlcmd -S "(localdb)\mssqllocaldb" -i sql/schema.sql

# Run seed script to populate initial data
sqlcmd -S "(localdb)\mssqllocaldb" -d TaskManagerDb -i sql/seed.sql
```

### 3. Backend configuration

Create `src/TaskManager.WebApi/appsettings.Development.json` (not committed):
```json
{
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\mssqllocaldb;Database=TaskManagerDb;Trusted_Connection=True;"
  },
  "Jwt": {
    "Secret": "your-super-secret-key-minimum-32-characters-long"
  }
}
```

Run the API:
```bash
cd src/TaskManager.WebApi
dotnet run
# API available at: https://localhost:5001
```

### 4. Frontend
```bash
cd frontend/task-manager-ui
npm install
npm start
# UI available at: http://localhost:4200
```

---

## Demo Credentials

| Email                | Password  |
|----------------------|-----------|
| demo@taskmanager.com | Demo@123  |

---

## API Reference

### Auth — `/api/auth`

| Method | Endpoint    | Auth | Description              |
|--------|-------------|------|--------------------------|
| POST   | /register   | ❌   | Register new user        |
| POST   | /login      | ❌   | Login, returns JWT token |
| GET    | /me         | ✅   | Current user info        |

### Tasks — `/api/tasks`

| Method | Endpoint    | Auth | Description              |
|--------|-------------|------|--------------------------|
| GET    | /public     | ❌   | API health info          |
| GET    | /           | ✅   | List current user tasks  |
| GET    | /{id}       | ✅   | Get task by ID           |
| POST   | /           | ✅   | Create task              |
| PUT    | /{id}       | ✅   | Update task              |
| DELETE | /{id}       | ✅   | Delete task              |

**Task payload:**
```json
{
  "title": "Design API",
  "description": "Define endpoints and contracts",
  "status": "Todo",
  "dueDate": "2025-06-01T00:00:00Z"
}
```

**Status values:** `Todo` | `InProgress` | `Done`

---

## Running Tests

```bash
dotnet test
```

Test coverage includes:
- `TaskService` — validation rules (empty title, past due date, invalid status)
- `AuthService` — registration and login edge cases
- `TaskController` — HTTP response codes and behavior

---

## Business Rules

- Title is required, maximum 200 characters
- DueDate must be in the future on creation
- Status must be one of: `Todo`, `InProgress`, `Done`
- Users can only access and modify their own tasks
- Email must be unique on registration
- Password minimum 8 characters

---

## Design Decisions

**Why ADO.NET instead of Entity Framework or Dapper?**  
Required by the exercise. Also, explicit data mapping makes the separation between layers clearer — there's no magic translating queries into objects.

**Why JWT and not ASP.NET Identity?**  
Identity adds significant complexity (role pipelines, claim transformations, cookie management) that isn't justified for this scope. JWT Bearer is right-sized: stateless, easy to validate, and straightforward to explain. The architecture allows swapping it if requirements change.

**Why SQL Server and not PostgreSQL?**  
Familiarity reduces risk in a time-boxed exercise. ADO.NET abstracts most differences — switching to `NpgsqlConnection` would require minimal changes.

**Why xUnit instead of NUnit?**  
xUnit is the current .NET standard for new projects. Cleaner test isolation (new instance per test by default) and better async support.

---

## GenAI Usage

This section documents how AI tools were used during development, per the exercise requirements.

### Tool used
Claude (claude.ai) for architecture planning and prompt engineering.  
GitHub Copilot (free tier) for code generation within VS Code.

### Prompt used for API scaffold

```
Generate a .NET 8 Web API using Clean Architecture with the following:
- Domain: TaskItem entity (Id, Title, Description, Status enum, DueDate, UserId, CreatedAt)
  and User entity (Id, Name, Email, PasswordHash, CreatedAt)
- Application: ITaskRepository and IUserRepository interfaces; TaskService with validation
  (title required, dueDate in future, valid status); AuthService with BCrypt + JWT
- Infrastructure: TaskRepository and UserRepository using ADO.NET SqlConnection only.
  No Entity Framework, no Dapper. Parameterized queries only.
- WebApi: TaskController with CRUD under [Authorize]; AuthController with /register and /login;
  one public endpoint in TaskController
- Tests: xUnit + Moq for TaskService covering empty title, past due date, user isolation
```

### What the AI generated well
- Project structure and layer separation were accurate
- Interface contracts were clean and dependency-direction was correct
- JWT configuration boilerplate was correct

### What I corrected

| Issue                                     | Fix applied                                              |
|-------------------------------------------|----------------------------------------------------------|
| Used `System.Data.SqlClient` (legacy)     | Replaced with `Microsoft.Data.SqlClient`                 |
| `GetAllAsync` returned all tasks globally | Added `UserId` filter to enforce user isolation          |
| No input validation on controller DTOs   | Added `[Required]` annotations and validation middleware  |
| JWT secret hardcoded in generated code    | Moved to `IConfiguration` with environment override      |
| Missing 401 on unauthorized task access  | Added ownership check in service, returns 403 Forbidden  |

### Takeaway
AI is effective for scaffolding boilerplate and remembering syntax. Critical thinking is required for security boundaries (user isolation, secret management) and for ensuring the output matches architectural constraints (no ORM, correct layer dependencies).

---

## What I'd Add With More Time

- Pagination on task listing (`?page=1&size=20`)
- Refresh token flow
- Docker + docker-compose for one-command setup
- GitHub Actions CI pipeline (already experienced with this from current role)
- Integration tests with a real test database
- Frontend error boundary and loading states
