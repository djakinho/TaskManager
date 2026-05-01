# Task Manager – Technical Interview Context

## User Story

> "As a developer, I want to manage my daily tasks so I can track what I need to do, what I'm working on, and what I've completed."

This drives every design decision. Keep it simple. Refer to it in your presentation.

---

## Tech Stack

| Layer      | Choice                       | Why                                           |
|------------|------------------------------|-----------------------------------------------|
| Backend    | .NET 8, ASP.NET Web API      | Explicitly required by test                   |
| Data       | ADO.NET (SqlConnection)      | Required — no EF, Dapper, or MediatR          |
| Database   | SQL Server (LocalDB or full) | You know it; no learning curve                |
| Auth       | JWT Bearer Token             | Simple, no Identity overhead                  |
| Tests      | xUnit + Moq                  | Modern standard; easy to explain              |
| Frontend   | React + Vite                 | Fast to scaffold; you know Angular but React  |
|            |                              | is lighter for this scope                     |

> ✅ If you prefer Angular, use it — you're more fluent. Just keep it clean.

---

## Project Structure

```
/TaskManager
  /src
    /TaskManager.Domain          → Entities, enums, no dependencies
    /TaskManager.Application     → Interfaces, services, DTOs, validators
    /TaskManager.Infrastructure  → Repositories (ADO.NET), DB config
    /TaskManager.WebApi          → Controllers, Program.cs, middleware
  /tests
    /TaskManager.Tests           → xUnit tests for all layers
  /frontend
    /task-manager-ui             → React (Vite) app
  /sql
    schema.sql                   → Table creation
    seed.sql                     → Demo user + tasks
  README.md
  context.md                     → This file (for AI tools and personal reference)
```

---

## Domain Entities

### User
```csharp
public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### TaskItem
```csharp
public class TaskItem
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TaskStatus Status { get; set; }  // Todo | InProgress | Done
    public DateTime DueDate { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum TaskStatus { Todo, InProgress, Done }
```

---

## SQL Schema

```sql
-- sql/schema.sql
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(200) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);

CREATE TABLE Tasks (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000),
    Status NVARCHAR(20) NOT NULL DEFAULT 'Todo',
    DueDate DATETIME2 NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL REFERENCES Users(Id),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);
```

---

## Seed Data

```sql
-- sql/seed.sql
-- Password: Demo@123 (BCrypt hash)
INSERT INTO Users (Id, Name, Email, PasswordHash)
VALUES (
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    'Demo User',
    'demo@taskmanager.com',
    '$2a$11$REPLACE_WITH_REAL_BCRYPT_HASH'
);

INSERT INTO Tasks (Id, Title, Description, Status, DueDate, UserId)
VALUES
    (NEWID(), 'Design API', 'Define endpoints and contracts', 'Done',    '2025-05-01', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'),
    (NEWID(), 'Build backend', 'Implement Clean Architecture', 'InProgress', '2025-05-10', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'),
    (NEWID(), 'Write tests', 'xUnit + Moq coverage', 'Todo', '2025-05-15', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa');
```

> ⚠️ Generate the BCrypt hash before inserting:
> ```csharp
> BCrypt.Net.BCrypt.HashPassword("Demo@123")
> ```
> Or use an online BCrypt generator for the seed only.

---

## API Endpoints

### Auth API — `/api/auth`
| Method | Route      | Auth?  | Description        |
|--------|------------|--------|--------------------|
| POST   | /register  | ❌ No  | Create user        |
| POST   | /login     | ❌ No  | Returns JWT token  |
| GET    | /me        | ✅ Yes | Returns user info  |

### Tasks API — `/api/tasks`
| Method | Route    | Auth?  | Description         |
|--------|----------|--------|---------------------|
| GET    | /        | ✅ Yes | List user's tasks   |
| GET    | /{id}    | ✅ Yes | Get task by ID      |
| POST   | /        | ✅ Yes | Create task         |
| PUT    | /{id}    | ✅ Yes | Update task         |
| DELETE | /{id}    | ✅ Yes | Delete task         |
| GET    | /public  | ❌ No  | Public health/info  |

> The public `/public` endpoint satisfies the "non-authorized endpoint" requirement.

---

## Business Rules (Application Layer)

Implement and TEST these in `TaskService`:

- Title is required and max 200 chars
- DueDate must be in the future (on create)
- Status must be one of: `Todo`, `InProgress`, `Done`
- A user can only access their own tasks (filter by UserId from JWT)
- Email must be unique on registration
- Password must be at least 8 characters

---

## Authentication Flow

```
1. POST /api/auth/register → hash password with BCrypt → save user → return 201
2. POST /api/auth/login    → verify BCrypt → generate JWT → return token
3. Client sends: Authorization: Bearer {token}
4. [Authorize] endpoints extract UserId from JWT claims
```

### JWT Setup in Program.cs
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Secret"])),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();
```

### appsettings.json
```json
{
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\mssqllocaldb;Database=TaskManagerDb;Trusted_Connection=True;"
  },
  "Jwt": {
    "Secret": "your-super-secret-key-min-32-chars-long"
  }
}
```

---

## Testing Strategy

### Priority order (do these first):
1. `TaskServiceTests` — business rules (title empty, date invalid, status invalid)
2. `AuthServiceTests` — bad password, duplicate email
3. `TaskControllerTests` — happy path GET/POST

### Example test structure (xUnit + Moq):
```csharp
public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _repoMock = new();
    private TaskService CreateService() => new(_repoMock.Object);

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenTitleIsEmpty()
    {
        var service = CreateService();
        var task = new TaskItem { Title = "", DueDate = DateTime.UtcNow.AddDays(1) };

        await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(task, userId: Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenDueDateIsInThePast()
    {
        var service = CreateService();
        var task = new TaskItem { Title = "Valid", DueDate = DateTime.UtcNow.AddDays(-1) };

        await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(task, userId: Guid.NewGuid()));
    }
}
```

---

## Frontend Checklist (React/Vite)

Minimum viable UI — judge by structure, not beauty:

- [ ] `/login` — form → POST /api/auth/login → store token in memory or sessionStorage
- [ ] `/tasks` — list all tasks, protected route
- [ ] Create task — form with title, description, status, due date
- [ ] Edit task — pre-filled form, PUT request
- [ ] Delete task — button with confirmation
- [ ] Logout — clears token, redirects to login

> Token storage: use `sessionStorage` or React context. Don't use `localStorage` if the app runs in the Claude.ai artifact environment.

### Axios config tip:
```js
// api/client.js
const client = axios.create({ baseURL: 'http://localhost:5000/api' });

client.interceptors.request.use(config => {
  const token = sessionStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});
```

---

## GenAI Section — What to Document

This is an evaluated criterion. Prepare this as part of your README or presentation.

### Prompt you used (example):
```
Generate a .NET 8 Web API using Clean Architecture with the following:
- Domain: TaskItem entity (Id, Title, Description, Status enum, DueDate, UserId)
- Application: ITaskRepository interface and TaskService with validation
- Infrastructure: TaskRepository using ADO.NET SqlConnection (no EF, no Dapper)
- WebApi: TaskController with CRUD endpoints using [Authorize]
- Auth: JWT Bearer token, no Identity
- Tests: xUnit + Moq for TaskService
```

### What to show:
1. The prompt above
2. A sample of what the AI generated
3. What you corrected:
   - "Used EF Core internally — replaced with ADO.NET"
   - "Missing UserId filter on GetAll — added user isolation"
   - "No input validation — added ValidationException pattern"
   - "JWT secret too short — added length check"
4. What you kept as-is and why

---

## Presentation Outline (10–15 min)

1. **User Story** (1 min) — why this domain, what the user needs
2. **Architecture** (2 min) — 4 layers, what lives where and why
3. **Key decisions** (3 min):
   - ADO.NET over ORM — "the test required it; forced explicit data mapping"
   - JWT over Identity — "right-sized for this scope; Identity is for apps needing roles, claims pipelines, UI"
   - SQL Server — "production familiarity reduces risk in a time-boxed exercise"
4. **Demo** (3 min) — register → login → CRUD tasks via frontend or Postman
5. **Tests** (2 min) — show a failing test written first, then the implementation
6. **GenAI** (2 min) — show prompt, output, your corrections
7. **What I'd add with more time** (1 min) — pagination, refresh tokens, Docker, CI/CD

---

## Commands to Remember (refresher)

```bash
# Create solution
dotnet new sln -n TaskManager
dotnet new webapi -n TaskManager.WebApi
dotnet new classlib -n TaskManager.Domain
dotnet new classlib -n TaskManager.Application
dotnet new classlib -n TaskManager.Infrastructure
dotnet new xunit -n TaskManager.Tests

# Add projects to solution
dotnet sln add **/*.csproj

# Add references
dotnet add TaskManager.Application/TaskManager.Application.csproj reference TaskManager.Domain/TaskManager.Domain.csproj
dotnet add TaskManager.Infrastructure/TaskManager.Infrastructure.csproj reference TaskManager.Application/TaskManager.Application.csproj
dotnet add TaskManager.WebApi/TaskManager.WebApi.csproj reference TaskManager.Application/TaskManager.Application.csproj
dotnet add TaskManager.WebApi/TaskManager.WebApi.csproj reference TaskManager.Infrastructure/TaskManager.Infrastructure.csproj
dotnet add TaskManager.Tests/TaskManager.Tests.csproj reference TaskManager.Application/TaskManager.Application.csproj

# NuGet packages
dotnet add TaskManager.WebApi package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.1
dotnet add TaskManager.Infrastructure package BCrypt.Net-Next
dotnet add TaskManager.Infrastructure package System.Data.SqlClient
dotnet add TaskManager.Tests package Moq
dotnet add TaskManager.Tests package FluentAssertions

# Run
dotnet run --project TaskManager.WebApi

# Test
dotnet test
```

---

## Prompts for Claude Code / Cursor

Use these in sequence. Each builds on the previous.

### Prompt 1 — Domain
```
Create a .NET 8 class library for the Domain layer of a Task Manager app.
Include:
- TaskItem entity with: Id (Guid), Title (string), Description (string), 
  Status (enum: Todo/InProgress/Done), DueDate (DateTime), UserId (Guid), CreatedAt (DateTime)
- User entity with: Id (Guid), Name (string), Email (string), 
  PasswordHash (string), CreatedAt (DateTime)
- TaskStatus enum
No dependencies on any other project or NuGet package.
Each class should have it's on file as needed (following Clean Architecture principles of responsability)
```

### Prompt 2 — Application Interfaces
```
Create the Application layer interfaces for a Task Manager .NET 8 app.
- ITaskRepository with: GetAllByUserAsync(Guid userId), GetByIdAsync(Guid id), 
  CreateAsync(TaskItem), UpdateAsync(TaskItem), DeleteAsync(Guid id)
- IUserRepository with: GetByEmailAsync(string email), CreateAsync(User), GetByIdAsync(Guid id)
- Use Task<T> for all async methods
- Reference TaskManager.Domain only
- No EF, no Dapper, no MediatR
```

### Prompt 3 — Tests (TDD style — write BEFORE implementation, expect RED)
```
Write xUnit tests for TaskService in a .NET 8 app using Moq.
IMPORTANT: These tests should be written BEFORE the implementation exists and are expected to FAIL initially (RED phase of TDD).

Test these cases:
1. CreateAsync throws ValidationException when Title is empty
2. CreateAsync throws ValidationException when DueDate is in the past
3. CreateAsync calls repository when input is valid
4. GetAllAsync returns only tasks for the given userId
5. DeleteAsync throws UnauthorizedException when task belongs to different user

Use:
- Mock<ITaskRepository>
- FluentAssertions for assertions
- [Fact] and [Theory] where appropriate
```

### Prompt 4 — Application Services (implement until GREEN)
```
Create TaskService and AuthService for a .NET 8 Clean Architecture app.
TaskService:
- Inject ITaskRepository
- CreateAsync: validate Title not empty (max 200), DueDate in future, Status valid; throw ValidationException otherwise
- GetAllAsync(Guid userId): return only tasks belonging to user
- UpdateAsync: same validations as create
- DeleteAsync: verify task belongs to user before deleting

AuthService:
- Inject IUserRepository, IConfiguration
- Register: validate email unique, password min 8 chars; hash with BCrypt; save user
- Login: find user by email; verify BCrypt; generate JWT with userId claim; return token string
JWT secret from IConfiguration["Jwt:Secret"]
```

### Prompt 5 — Infrastructure (ADO.NET)
```
Implement TaskRepository and UserRepository for .NET 8 using ADO.NET only.
- Use SqlConnection and SqlCommand (System.Data.SqlClient)
- Connection string from IConfiguration["ConnectionStrings:Default"]
- No Entity Framework, no Dapper
- TaskRepository: implement ITaskRepository from TaskManager.Application
- UserRepository: implement IUserRepository from TaskManager.Application
- Use parameterized queries (never string concatenation)
- Map SqlDataReader manually to entities
```

### Prompt 6 — Controllers
```
Create ASP.NET Web API controllers for a Task Manager app.
TaskController (route: api/tasks):
- All endpoints require [Authorize]
- GET / → GetAllAsync for current user (extract userId from JWT claim)
- GET /{id} → GetByIdAsync
- POST / → CreateAsync (body: CreateTaskDto with Title, Description, Status, DueDate)
- PUT /{id} → UpdateAsync (body: UpdateTaskDto)
- DELETE /{id} → DeleteAsync
- GET /public → returns 200 OK with "Task Manager API v1" (no auth required)

AuthController (route: api/auth):
- POST /register → RegisterDto (Name, Email, Password) → 201 Created
- POST /login → LoginDto (Email, Password) → 200 OK with { token: string }
- GET /me → [Authorize] → returns current user info from JWT

Use proper HTTP status codes. Return ProblemDetails on validation errors (400).
```



### Prompt 7 — React Frontend
```
Create a React (Vite) frontend for a Task Manager API.
Pages:
- /login — email + password form → POST /api/auth/login → store token in sessionStorage → redirect to /tasks
- /tasks — protected route; list tasks from GET /api/tasks; show title, status, due date
- Create task button → modal or inline form
- Edit/Delete per task row

Components:
- TaskList, TaskForm (create/edit), LoginPage
- Axios client with Authorization Bearer token interceptor
- React Router for navigation
- Redirect to /login if no token

Keep it functional and clean. No heavy UI libraries — plain CSS or minimal Tailwind is fine.
```

---

## Checklist Before Demo

- [ ] SQL schema created and seed data inserted
- [ ] `appsettings.json` has connection string and JWT secret
- [ ] Register endpoint works
- [ ] Login returns a valid JWT
- [ ] Protected endpoints reject requests without token (401)
- [ ] CRUD operations work end-to-end
- [ ] Frontend login stores token and hits protected API
- [ ] At least 5 passing unit tests
- [ ] README has setup instructions and demo credentials
- [ ] No broken console errors in browser
- [ ] GenAI section documented with prompt + critique

---

## Demo Credentials

| Email                 | Password  |
|-----------------------|-----------|
| demo@taskmanager.com  | Demo@123  |
