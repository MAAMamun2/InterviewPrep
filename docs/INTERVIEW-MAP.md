# Interview Question → File Map

Every row below is a real, working piece of this project. Open the file, read the comment block at
the top of the class (that's where the "why" lives), then come back here for the one-line answer
to say out loud.

## C# / OOP / SOLID

| Question | File | What to say |
|---|---|---|
| Encapsulation, real example | `src/InterviewPrep.Domain/Entities/Member.cs` | "Private setters, the only entry point is `Member.Create()`, which enforces invariants a public setter never could — like `UpgradeTier` only ever moving up." |
| Inheritance, real example | `src/InterviewPrep.Domain/Common/BaseEntity.cs` | "Id/CreatedAtUtc/UpdatedAtUtc shared by every entity instead of repeated three times." |
| Abstraction, real example | `src/InterviewPrep.Application/Interfaces/IMemberRepository.cs` | "Application depends on this interface, never on EF Core directly — Infrastructure is swappable." |
| Polymorphism, real example | `src/InterviewPrep.Domain/Discounts/IDiscountRule.cs` + 4 implementations | "`DiscountCalculator` calls `rule.GetPercentage(...)` through the interface — doesn't know or care which concrete rule it's talking to." |
| Single Responsibility | Any one handler, e.g. `CreateMemberCommandHandler.cs` | "One handler, one job — create a member. Validation lives in a separate validator, logging in a separate behavior." |
| Open/Closed, real example | `src/InterviewPrep.Domain/Discounts/BigSpenderDiscountRule.cs` | "Added AFTER the other 3 rules existed. Zero changes anywhere else — DI's assembly scan in `DependencyInjection.cs` picks it up automatically." |
| Liskov Substitution | Any `IDiscountRule` implementation | "Every rule is fully substitutable for the interface — `DiscountCalculator` never needs to know which one it got." |
| Interface Segregation | `IMemberRepository` vs `IPurchaseRepository` vs `IUserRepository` | "Three small, focused interfaces instead of one giant `IRepository` god-interface." |
| Dependency Inversion | `src/InterviewPrep.Infrastructure/DependencyInjection.cs` | "Application defines the interfaces; Infrastructure implements them; the composition root wires concrete to abstract in one place." |
| `Task` vs `Task<T>` | any `async Task<X>` handler | "`Task<T>` derives from `Task` — a method returning `Task<T>` can be awaited anywhere a `Task` is expected." |
| `IEnumerable<T>` vs `IQueryable<T>` | `src/InterviewPrep.Application/Members/Queries/GetMembers/GetMembersQueryHandler.cs` | "Every `.Where()` builds the SAME expression tree; only `.ToListAsync()` at the end triggers ONE SQL statement. Read the comment block — it's the exact answer." |

## Design Patterns

| Pattern | File |
|---|---|
| Strategy | `src/InterviewPrep.Domain/Discounts/` (whole folder) |
| Repository | `src/InterviewPrep.Infrastructure/Persistence/Repositories/MemberRepository.cs` |
| Unit of Work | `src/InterviewPrep.Application/Interfaces/IUnitOfWork.cs` + `Infrastructure/Persistence/UnitOfWork.cs` |
| Decorator | `src/InterviewPrep.Infrastructure/Persistence/Repositories/CachedMemberRepository.cs` (wired via Scrutor's `.Decorate<>()` in `DependencyInjection.cs`) |
| Mediator (CQRS) | Every file under `src/InterviewPrep.Application/*/Commands` and `*/Queries` |
| Observer / Pub-Sub | `src/InterviewPrep.Application/Purchases/Events/PurchaseCreatedEvent.cs` + `LoyaltyTierUpgradeHandler.cs` |
| Result pattern (not exceptions for expected failures) | `src/InterviewPrep.Application/Common/Result.cs`, used by `LoginCommandHandler.cs` |

## ASP.NET Core / Web API

| Question | File |
|---|---|
| Custom middleware | `src/InterviewPrep.Api/Middleware/ExceptionHandlingMiddleware.cs`, `RequestTimingMiddleware.cs` |
| Middleware pipeline ORDER | `src/InterviewPrep.Api/Program.cs` — read the comment right above `app.UseMiddleware<...>` calls |
| JWT auth setup | `Program.cs` (`AddAuthentication().AddJwtBearer(...)`) + `Infrastructure/Auth/JwtTokenGenerator.cs` |
| Authentication vs authorization | `src/InterviewPrep.Api/Controllers/MembersController.cs` (`[Authorize]` vs `[Authorize(Roles = "Admin")]`) |
| Dependency injection lifetimes (Singleton/Scoped/Transient) | `Infrastructure/DependencyInjection.cs` — compare the `AddSingleton`/`AddScoped` choices and the comments explaining each |
| Rate limiting | `Program.cs` (`AddRateLimiter`, `[EnableRateLimiting("login")]` on `AuthController.Login`) — **and the real gotcha**: the default rejection code is 503, not 429, fixed explicitly here after I caught it by actually testing the 6th request |
| CORS | `Program.cs` (`AddCors`) — compare against the React client's DIRECT cross-origin call vs. the Angular client's dev-proxy (two different ways to solve the same problem, both demonstrated on purpose) |
| Background/hosted service, the Scoped-in-Singleton gotcha | `Infrastructure/BackgroundJobs/TierAuditJob.cs` — read the class comment, it's the exact trap explanation |
| FluentValidation + pipeline validation | `Application/Common/Behaviors/ValidationBehavior.cs` |
| Swagger/OpenAPI with JWT support | `Program.cs` (`AddSwaggerGen` security scheme block) |

## EF Core

| Question | File |
|---|---|
| Fluent configuration vs attributes | `Infrastructure/Persistence/Configurations/*.cs` |
| Enum storage gotcha | `MemberConfiguration.cs` comment — storing as `string` vs `int` |
| One-to-many relationship | `MemberConfiguration.cs` (`HasMany(...).WithOne()...`) |
| Private backing field access | `MemberConfiguration.cs` (`UsePropertyAccessMode(PropertyAccessMode.Field)`) |
| Migrations | `Infrastructure/Migrations/` (run `dotnet ef migrations add` yourself to see it happen) |
| Auto-populated audit columns | `Persistence/InterviewPrepDbContext.cs` (`SaveChangesAsync` override) |
| Seeding | `Persistence/DbInitializer.cs` |

## Testing

| Question | File |
|---|---|
| Pure unit test, no mocks | `tests/InterviewPrep.Tests/Domain/Discounts/DiscountCalculatorTests.cs` |
| Boundary/edge-case testing | Same file — `[Theory]`/`[InlineData]` at every age bracket boundary |
| Mocking a dependency (Moq) | `tests/.../Application/Members/CreateMemberCommandHandlerTests.cs` |
| State-based vs interaction-based assertions | Same file — `.Should().Be(...)` vs `.Verify(..., Times.Once)` |
| Testing a security-sensitive path (generic login error) | `tests/.../Application/Auth/LoginCommandHandlerTests.cs` |
| Testing a MediatR pipeline behavior | `tests/.../Application/Common/ValidationBehaviorTests.cs` |

## Security

| Question | Where |
|---|---|
| SQL injection prevention | `Infrastructure/Persistence/Repositories/UserRepository.cs` comment — parameterized via EF Core's LINQ translation |
| Password storage | `Infrastructure/Auth/PasswordHasher.cs` — BCrypt, salted + deliberately slow, vs. a fast hash like SHA256 |
| Information disclosure in error responses | `Api/Middleware/ExceptionHandlingMiddleware.cs` — generic message to the client, full exception only to the log |
| Username enumeration | `Application/Auth/Commands/Login/LoginCommandHandler.cs` — same error for "no such user" and "wrong password" |
| Brute-force defense | Rate limiting on `/api/auth/login` specifically (see above) |
| IDOR | `Api/Controllers/MembersController.cs` comment — and how HishabPOS's real Row-Level Security closes this at the DB layer, not just in application code |
| XSS | Angular/React both escape interpolated template values by default (`{{ }}` / `{expr}`) — neither renders raw HTML unless you explicitly opt in (`[innerHTML]` / `dangerouslySetInnerHTML`), which is the actual XSS risk surface to mention if asked |
| CSRF | JWT-in-header (not a cookie) means CSRF doesn't apply the normal way here — raise this contrast if asked, including WHY it would apply if you switched to cookie-based auth |

## Angular (`clients/angular-login`)

| Question | File |
|---|---|
| Standalone components | Any `*.component.ts` — no `NgModule` anywhere in this app |
| Signals (`signal`, `computed`) | `core/services/auth.service.ts` |
| Reactive Forms | `features/login/login.component.ts` |
| Functional HTTP interceptor | `core/interceptors/auth.interceptor.ts` |
| Functional route guard | `core/guards/auth.guard.ts` |
| RxJS `switchMap`/`debounceTime`/`distinctUntilChanged` | `features/members/members.component.ts` |
| `takeUntilDestroyed` (modern unsubscribe) | Same file |
| Lazy-loaded routes | `app.routes.ts` (`loadComponent`) |
| New control-flow syntax (`@if`/`@for`/`@empty`) | `features/members/members.component.html` |

## React (`clients/react-login`)

See **`docs/REACT-GUIDE.md`** for all of these in one consolidated file, basics through advanced,
with explanations and a quick-fire Q&A at the end — use that instead of jumping between rows here.

| Question | File |
|---|---|
| Controlled components | `pages/LoginPage.tsx` |
| Custom hook | `hooks/useDebouncedValue.ts` |
| Context API + `useContext` | `context/AuthContext.tsx` |
| `useMemo` (why it's needed here specifically) | `context/AuthContext.tsx` comment on the `value` object |
| `useEffect` cleanup / stale-response bug | `pages/MembersPage.tsx` (`cancelled` flag) |
| `useId` | `pages/LoginPage.tsx` |
| `key` prop / list-rendering antipattern | `pages/MembersPage.tsx` comment on `key={member.id}` |
| TypeScript generics | `api/client.ts` (`apiFetch<T>`) |
| TypeScript discriminated unions | `types.ts` (`ApiResult<T>`) — directly comparable to the backend's `Result<T>` and Angular's `AuthState` union |

## SQL (`sql/`)

Open `sql/03_interview_queries.sql` directly — every block is headed with the exact question it
answers (joins, duplicates, Nth-highest, window functions, CTEs, pagination strategies, indexing,
transactions/ACID).

## Architecture / System Design

See `docs/ARCHITECTURE.md` — monolith vs microservices, scaling, CAP theorem, auth architecture,
why CQRS, deployment — each answered against this project AND HishabPOS specifically.
