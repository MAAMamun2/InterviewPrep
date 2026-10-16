# InterviewPrep

A small, working .NET + Angular + React project built to cover senior full-stack interview
questions with REAL code, not just a Q&A list. Everything in here compiles, runs, and has been
smoke-tested end to end (login → JWT → protected endpoint → discount calculation → tier upgrade).

Start with **`docs/INTERVIEW-MAP.md`** — it points you at the exact file for every topic.
Then **`docs/ARCHITECTURE.md`** for the system-design-level questions.

## Run the backend

```powershell
cd src/InterviewPrep.Api
dotnet run
```

- API: `http://localhost:5000` (Swagger UI at `/swagger` in Development)
- First run auto-applies EF Core migrations and seeds:
  - Users: `admin` / `Admin123!` (Admin role), `staff` / `Staff123!` (Staff role)
  - Two demo members with purchase history
- Database is a local SQLite file (`interviewprep.db`) — zero setup, delete the file to reset.

## Run the tests

```powershell
dotnet test tests/InterviewPrep.Tests
```

22 tests: pure domain unit tests, Moq-based handler tests, a MediatR pipeline-behavior test.

## Run the Angular login app

```powershell
cd clients/angular-login
npm install   # first time only
npm start     # serves on :4200, proxies /api/* to the backend on :5000
```

## Run the React login app

```powershell
cd clients/react-login
npm install   # first time only
npm run dev   # serves on :5173, calls the backend on :5000 DIRECTLY (exercises real CORS)
```

Log in with the same `admin`/`staff` credentials on either frontend — both talk to the same
backend and the same seeded data.

## Try the SQL questions

Point `psql` (or any Postgres client) at a scratch database and run, in order:
`sql/01_schema.sql` → `sql/02_seed.sql` → `sql/03_interview_queries.sql`. Each block in the last
file is headed with the exact interview question it answers.

## Layout

```
src/            .NET backend — Clean Architecture (Domain / Application / Infrastructure / Api)
tests/          xUnit + Moq + FluentAssertions
clients/
  angular-login/   Angular 22, standalone + signals, proxies to the API
  react-login/     React + TS (Vite), calls the API directly (CORS)
sql/            Schema + seed + annotated interview queries (PostgreSQL)
docs/
  ARCHITECTURE.md    System-design-level Q&A, grounded in this project + HishabPOS
  INTERVIEW-MAP.md   Every topic -> the exact file that demonstrates it
```
