# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this repo is

Not a product — it's a senior full-stack **interview-prep reference**: a small but real .NET +
Angular + React project where every pattern/question an interviewer might ask has actual working
code behind it, not a textbook explanation. Changes here should keep that property: prefer the
real, idiomatic way to do something over a shortcut, and keep the explanatory comments at the top
of files that exist specifically to answer an interview question (don't strip them as "unnecessary
comments").

Two docs are the entry point for *using* this repo (read these before editing if the task is
"explain X" or "add an example of Y"):
- `docs/INTERVIEW-MAP.md` — topic → exact file table (C#/OOP/SOLID, design patterns, ASP.NET Core,
  EF Core, testing, security, Angular, React, SQL, architecture).
- `docs/ARCHITECTURE.md` — system-design-level Q&A (monolith vs. microservices, scaling, CAP
  theorem, auth architecture, why CQRS) answered against this project specifically.

## Commands

### Backend (.NET, `net10.0`)
```powershell
cd src/InterviewPrep.Api
dotnet run                                    # http://localhost:5000, Swagger at /swagger (Dev)
```
Database is SQL Server. First run auto-applies EF Core migrations and seeds: users
`admin`/`Admin123!` (Admin role), `staff`/`Staff123!` (Staff role), plus two demo members with
purchase history. Local dev targets LocalDB (`(localdb)\mssqllocaldb`, see connection string in
`appsettings.json`) — ships with Visual Studio/SQL Server tooling, no container needed. Drop the
`InterviewPrepDb` LocalDB database (`sqllocaldb`/SSMS) to reset.

```powershell
dotnet test tests/InterviewPrep.Tests                              # full suite (22 tests)
dotnet test tests/InterviewPrep.Tests --filter "FullyQualifiedName~DiscountCalculatorTests"
dotnet test tests/InterviewPrep.Tests --filter "DisplayName~Login_WithInvalidPassword"
dotnet build InterviewPrep.slnx                                    # build everything
```

### Angular client (`clients/angular-login`, Angular 22, standalone + signals)
```powershell
npm install        # first time only
npm start          # ng serve --proxy-config proxy.conf.json, :4200 -> proxies /api/* to :5000
npm test           # ng test (Vitest)
npm run build
```

### React client (`clients/react-login`, React 19 + TS + Vite)
```powershell
npm install        # first time only
npm run dev         # :5173, calls the backend on :5000 DIRECTLY — exercises real CORS, no proxy
npm run build        # tsc -b && vite build
npm run lint         # oxlint
```

Both frontends log in with the same `admin`/`staff` seeded credentials against the same backend.

### Docker
```powershell
docker compose up --build    # sql (SQL Server 2022 container) + api, api on :5000 -> container :8080
```
`Dockerfile` (repo root, build context = repo root) is a multi-stage build: SDK image compiles +
publishes, ASP.NET runtime image runs it. `docker-compose.yml` also runs a real SQL Server
container (`sql` service, data in a named volume) — the `api` service's connection string is
overridden via `ConnectionStrings__DefaultConnection` to point at it (`Server=sql,1433;...`),
separate from the LocalDB connection string in `appsettings.json` used for local `dotnet run`
(LocalDB is Windows-only, not reachable from inside the Linux container).

### CI (`.github/workflows/ci.yml`)
Runs on push/PR to `main`: `backend` (dotnet build + test), `angular` (build + test), `react`
(lint + build), `docker` (builds the image, doesn't push). Keep these four jobs green before
merging — there's no deploy step, this is build/test verification only.

### SQL (`sql/`, PostgreSQL)
Run against a scratch Postgres database, in order: `01_schema.sql` → `02_seed.sql` →
`03_interview_queries.sql`. Every block in the last file is headed with the exact interview
question it answers (joins, duplicates, Nth-highest, window functions, CTEs, pagination,
indexing, transactions/ACID) — read that header before changing a query.

## Architecture

Clean Architecture, four projects, dependencies point one way (inward, toward Domain):

```
Api ──► Infrastructure ──► Application ──► Domain
```

- **`InterviewPrep.Domain`** — entities (`Member.Create()` factory + private setters, invariants
  like `UpgradeTier` only moving forward), enums, and the Strategy-pattern discount rules
  (`Discounts/IDiscountRule.cs` + 4 implementations, picked up automatically via assembly scan in
  `Infrastructure/DependencyInjection.cs` — adding a new rule requires zero changes elsewhere).
  Zero dependencies on anything else in the solution.
- **`InterviewPrep.Application`** — CQRS via MediatR: one folder per feature
  (`Members/Commands`, `Members/Queries`, `Purchases/`, `Auth/`), each handler doing exactly one
  thing. FluentValidation validators and cross-cutting pipeline behaviors
  (`Common/Behaviors/ValidationBehavior.cs`, logging, timing) wrap every command/query
  automatically — this is where new validation/logging logic for ALL handlers goes, not into
  individual handlers. Defines every interface Infrastructure implements (`IMemberRepository`,
  `IUnitOfWork`, `IJwtTokenGenerator`, ...). Depends only on Domain — never on Infrastructure.
  `PurchaseCreatedEvent` + `LoyaltyTierUpgradeHandler` is the Observer/pub-sub pattern via MediatR
  notifications.
- **`InterviewPrep.Infrastructure`** — EF Core (`Persistence/`, SQL Server), repositories implementing
  the Application interfaces, `CachedMemberRepository` (Decorator pattern, wired with Scrutor's
  `.Decorate<>()`), JWT generation + BCrypt hashing (`Auth/`), and `BackgroundJobs/TierAuditJob`
  (hosted service — see its class comment for the Scoped-in-Singleton DI-lifetime gotcha it
  deliberately avoids via `IServiceScopeFactory`). All DI wiring/lifetimes are decided in
  `DependencyInjection.cs` — check there before adding a new service registration.
- **`InterviewPrep.Api`** — controllers (thin: HTTP ⇄ MediatR translation only, no business logic),
  middleware (`ExceptionHandlingMiddleware`, `RequestTimingMiddleware`), and all composition-root
  config in `Program.cs` (JWT bearer auth, rate limiting on login, CORS, Swagger with JWT support).
  Middleware *order* in `Program.cs` matters — read the comments directly above each
  `app.UseMiddleware<...>()` call before reordering.

**Rule of thumb when adding code**: Domain never references anything; Application never
references Infrastructure (only its own interfaces); Infrastructure implements those interfaces;
Api only talks to MediatR, never directly to Infrastructure or Domain internals.

### Frontends
- **Angular** (`clients/angular-login/src`): no `NgModule` anywhere — standalone components only.
  State via signals (`core/services/auth.service.ts`), functional HTTP interceptor
  (`core/interceptors/auth.interceptor.ts`) and functional route guard
  (`core/guards/auth.guard.ts`), lazy-loaded routes (`app.routes.ts` `loadComponent`), new
  `@if`/`@for`/`@empty` control-flow syntax in templates. Talks to the backend through the dev
  proxy (`proxy.conf.json`), so API calls are same-origin in dev.
- **React** (`clients/react-login/src`): Context API for auth (`context/AuthContext.tsx` — note
  the `useMemo` on the context value, there specifically to avoid re-render storms), a generic
  typed fetch wrapper (`api/client.ts`'s `apiFetch<T>`), discriminated unions in `types.ts`
  (`ApiResult<T>`) mirroring the backend's `Result<T>` pattern. Calls the backend directly
  cross-origin (no proxy) to exercise real CORS, deliberately contrasting with Angular's approach.

### Testing (`tests/InterviewPrep.Tests`)
xUnit + Moq + FluentAssertions. Mirrors the `Domain`/`Application` structure. Pure domain tests
need no mocks (`Domain/Discounts/DiscountCalculatorTests.cs`); handler tests mock repository
interfaces (`Application/Members/CreateMemberCommandHandlerTests.cs`); there's one test exercising
the MediatR validation pipeline behavior itself (`Application/Common/ValidationBehaviorTests.cs`).
