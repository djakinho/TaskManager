# Task Manager – Frontend Context (Angular)

## Overview

This is the frontend for the Task Manager API built with .NET 8 Clean Architecture.
The frontend must integrate with the existing backend and satisfy these criteria:
- Responsive and user-friendly
- Full CRUD for tasks
- Clean component and state organization
- JWT Bearer authentication

> Update `API_URL` in `src/environments/environment.ts` to match your local API port before running.

---

## Tech Decisions

| Concern       | Choice                              |
|---------------|-------------------------------------|
| Framework     | Angular (latest stable)             |
| Style         | Plain CSS (no UI library)           |
| HTTP          | HttpClient with JWT interceptor     |
| Routing       | Angular Router with route guards    |
| State         | Services with BehaviorSubject       |
| Auth storage  | sessionStorage (token only)         |

---

## Project Structure (inside `/frontend/task-manager-ui`)

```
src/
  app/
    core/
      guards/
        auth.guard.ts           → redirect to /login if no token
      interceptors/
        auth.interceptor.ts     → attach Bearer token to every request
      services/
        auth.service.ts         → login, register, logout, token helpers
        task.service.ts         → CRUD calls to /api/tasks
    features/
      auth/
        login/
          login.component.ts
          login.component.html
          login.component.css
      tasks/
        task-list/
          task-list.component.ts
          task-list.component.html
          task-list.component.css
        task-form/
          task-form.component.ts
          task-form.component.html
          task-form.component.css
    shared/
      models/
        task.model.ts
        user.model.ts
  environments/
    environment.ts              → { apiUrl: 'https://localhost:YOURPORT' }
```

---

## Models

```typescript
// shared/models/task.model.ts
export type TaskStatus = 'Todo' | 'InProgress' | 'Done';

export interface Task {
  id: string;
  title: string;
  description: string;
  status: TaskStatus;
  dueDate: string; // ISO string
  userId: string;
  createdAt: string;
}

export interface CreateTaskDto {
  title: string;
  description: string;
  status: TaskStatus;
  dueDate: string;
}

export interface UpdateTaskDto {
  title: string;
  description: string;
  status: TaskStatus;
  dueDate: string;
}
```

```typescript
// shared/models/user.model.ts
export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterDto {
  name: string;
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
}
```

---

## API Endpoints consumed

| Method | URL                  | Auth | Used in              |
|--------|----------------------|------|----------------------|
| POST   | /api/auth/login      | ❌   | AuthService.login()  |
| POST   | /api/auth/register   | ❌   | AuthService.register() |
| GET    | /api/tasks           | ✅   | TaskService.getAll() |
| POST   | /api/tasks           | ✅   | TaskService.create() |
| PUT    | /api/tasks/:id       | ✅   | TaskService.update() |
| DELETE | /api/tasks/:id       | ✅   | TaskService.delete() |

---

## Routes

| Path       | Component          | Guard     |
|------------|--------------------|-----------|
| /login     | LoginComponent     | ❌ public |
| /tasks     | TaskListComponent  | ✅ auth   |
| /          | redirect to /tasks | —         |
| **         | redirect to /login | —         |

---

## Auth Flow

```
1. User fills login form → AuthService.login() → POST /api/auth/login
2. Response { token } → sessionStorage.setItem('token', token)
3. AuthInterceptor reads token → adds Authorization: Bearer {token} to all requests
4. AuthGuard checks sessionStorage → redirects to /login if missing
5. Logout → sessionStorage.removeItem('token') → navigate to /login
```

---

## Business Rules on the Frontend

- Title is required (match backend validation)
- DueDate must be a future date — validate on form submit, show inline error
- Status dropdown: options are exactly `Todo`, `InProgress`, `Done`
- On 401 response → clear token → redirect to /login
- On 403 response → show "You don't have permission for this action"
- On any API error → show a visible error message (no silent failures)

---

## CSS Guidelines (plain CSS)

- Mobile-first: base styles for small screens, media query for `min-width: 768px`
- Use CSS custom properties for colors:
  ```css
  :root {
    --color-primary: #2563eb;
    --color-danger: #dc2626;
    --color-success: #16a34a;
    --color-bg: #f9fafb;
    --color-surface: #ffffff;
    --color-border: #e5e7eb;
    --color-text: #111827;
    --color-muted: #6b7280;
  }
  ```
- Cards for task items, clear visual separation between statuses
- Form inputs: full width, clear focus state, visible error messages in red
- Buttons: primary (blue), danger (red), subtle (gray) — no inline styles

---

## Demo Credentials

| Email                | Password  |
|----------------------|-----------|
| demo@taskmanager.com | Demo@123  |

---

## Prompts for the Agent

Use in sequence. Each builds on the previous. Paste the relevant section of this file as context before each prompt.

---

### Prompt 1 — Scaffold

```
Scaffold an Angular app (latest stable version, standalone components) for a Task Manager.
Project location: frontend/task-manager-ui

Structure:
- core/guards/auth.guard.ts
- core/interceptors/auth.interceptor.ts
- core/services/auth.service.ts
- core/services/task.service.ts
- features/auth/login/ (component)
- features/tasks/task-list/ (component)
- features/tasks/task-form/ (component)
- shared/models/task.model.ts
- shared/models/user.model.ts
- environments/environment.ts with apiUrl placeholder

Routes:
- /login → LoginComponent (public)
- /tasks → TaskListComponent (protected by AuthGuard)
- / and ** → redirect to /tasks

Do not generate any styles yet. Do not install any UI library.
Use HttpClient for HTTP. Use sessionStorage for token storage.
```

---

### Prompt 2 — Auth Service + Interceptor

```
Implement AuthService and AuthInterceptor for an Angular Task Manager app.

AuthService (core/services/auth.service.ts):
- login(dto: LoginDto): calls POST /api/auth/login, stores token in sessionStorage, returns observable
- register(dto: RegisterDto): calls POST /api/auth/register, returns observable
- logout(): removes token from sessionStorage, navigates to /login
- isLoggedIn(): returns boolean (token present in sessionStorage)
- getToken(): returns token string or null

AuthInterceptor (core/interceptors/auth.interceptor.ts):
- Reads token from sessionStorage
- Adds Authorization: Bearer {token} header to every outgoing request
- On 401 response: calls AuthService.logout()

AuthGuard (core/guards/auth.guard.ts):
- Checks AuthService.isLoggedIn()
- If false: redirects to /login and returns false

Models:
- LoginDto { email, password }
- RegisterDto { name, email, password }
- AuthResponse { token }

API base URL from environment.ts (environment.apiUrl).
```

---

### Prompt 3 — Task Service

```
Implement TaskService for an Angular Task Manager app.

TaskService (core/services/task.service.ts):
- Private BehaviorSubject<Task[]> to hold the current task list
- Public tasks$ observable from the BehaviorSubject
- getAll(): GET /api/tasks → updates BehaviorSubject, returns observable
- create(dto: CreateTaskDto): POST /api/tasks → calls getAll() to refresh, returns observable
- update(id: string, dto: UpdateTaskDto): PUT /api/tasks/:id → calls getAll() to refresh
- delete(id: string): DELETE /api/tasks/:id → calls getAll() to refresh

Error handling:
- On 401: let the interceptor handle it
- On other errors: rethrow so the component can display a message

Models (from shared/models/task.model.ts):
Task { id, title, description, status, dueDate, userId, createdAt }
CreateTaskDto { title, description, status, dueDate }
UpdateTaskDto { title, description, status, dueDate }
TaskStatus = 'Todo' | 'InProgress' | 'Done'

API base URL from environment.ts.
```

---

### Prompt 4 — Login Component

```
Implement the LoginComponent for an Angular Task Manager app.

File: features/auth/login/login.component.ts + .html + .css

Behavior:
- Reactive form with email (required, email format) and password (required, min 8 chars)
- On submit: call AuthService.login() → on success navigate to /tasks
- Show inline validation errors below each field
- Show a general error message if login fails (wrong credentials)
- "Don't have an account? Register" link that expands a register form inline
  (register form: name, email, password — same validations)
- Loading state on button while request is in flight

Style (plain CSS):
- Centered card on screen, max-width 400px
- Full-width inputs with visible focus ring
- Primary button (blue), full width
- Error messages in red below fields
- Responsive on mobile
```

---

### Prompt 5 — Task List Component

```
Implement the TaskListComponent for an Angular Task Manager app.

File: features/tasks/task-list/task-list.component.ts + .html + .css

Behavior:
- On init: call TaskService.getAll() to load tasks
- Display tasks as cards showing: title, description, status badge, due date
- Status badge color: Todo=gray, InProgress=blue, Done=green
- "New Task" button → opens TaskFormComponent in create mode (inline or modal)
- Each task card has Edit and Delete buttons
  - Edit → opens TaskFormComponent in edit mode pre-filled
  - Delete → confirm with window.confirm() → call TaskService.delete()
- Show loading indicator while fetching
- Show error message if fetch fails
- Logout button in header → calls AuthService.logout()

Style (plain CSS):
- Responsive grid: 1 column on mobile, 2-3 columns on desktop (min-width: 768px)
- Cards with border, subtle shadow, padding
- Status badges as colored pills
- Header bar with app title and logout button
- Empty state message when no tasks
```

---

### Prompt 6 — Task Form Component

```
Implement the TaskFormComponent for an Angular Task Manager app.

File: features/tasks/task-form/task-form.component.ts + .html + .css

Inputs:
- @Input() task?: Task  (if present → edit mode, if absent → create mode)
- @Input() visible: boolean

Outputs:
- @Output() saved = new EventEmitter<void>()
- @Output() cancelled = new EventEmitter<void>()

Behavior:
- Reactive form with: title (required, max 200), description, status (select), dueDate (required, must be future)
- In edit mode: pre-fill form with task values
- On submit:
  - create mode → TaskService.create(dto) → emit saved
  - edit mode → TaskService.update(task.id, dto) → emit saved
- On cancel: emit cancelled
- Show inline validation errors
- Show API error if request fails
- Loading state on submit button

Status options: Todo, InProgress, Done (displayed as-is)
DueDate: HTML date input, validate that selected date > today on submit

Style (plain CSS):
- Displayed as an overlay/modal when visible=true
- Form fields stacked vertically, full width
- Save (primary/blue) and Cancel (gray) buttons side by side
- Error messages in red
- Responsive
```

---

### Prompt 7 — CORS and final wiring check

```
I have an Angular frontend calling an ASP.NET Core 8 Web API.
The Angular app runs on http://localhost:4200.
The API runs on https://localhost:{PORT}.

In the ASP.NET Program.cs, add CORS policy that allows:
- Origin: http://localhost:4200
- Any header
- Any method

Apply the CORS middleware before UseAuthentication and UseAuthorization.
Show only the relevant Program.cs additions, not the full file.
```

---

## Setup Commands

```bash
# Inside /frontend
ng new task-manager-ui --routing --style=css --standalone
cd task-manager-ui

# No extra packages needed (HttpClient and Router are built-in to Angular)

# Run
ng serve
# App available at http://localhost:4200
```

---

## Checklist Before Demo

- [ ] `environment.ts` has correct API URL and port
- [ ] CORS configured on the API to allow `http://localhost:4200`
- [ ] Login works with demo credentials and stores token
- [ ] Protected route `/tasks` redirects to `/login` when no token
- [ ] Task list loads after login
- [ ] Create task works and list refreshes
- [ ] Edit task pre-fills form and saves correctly
- [ ] Delete task removes from list after confirmation
- [ ] Logout clears token and redirects to login
- [ ] No console errors in browser
- [ ] UI is usable on mobile screen width (320–375px)
