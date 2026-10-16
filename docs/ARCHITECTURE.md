# Architecture & System Design — Interview Answers

Senior-level interviews often shift from "do you know X pattern" to "how would you design a
system" or "why is it built this way." This file answers the common architecture questions using
THIS project and HishabPOS as the concrete evidence, not textbook abstractions.

## 1. "Walk me through this project's architecture."

Clean Architecture, four layers, dependencies point ONE way (inward, toward Domain):

```
Api  ─────────────┐
                   ▼
Infrastructure ──► Application ──► Domain
```

- **Domain** (`InterviewPrep.Domain`): entities, enums, the Strategy-pattern discount rules. Zero
  dependencies on anything else in the solution — this is the one layer that could be copy-pasted
  into a totally different app (console, mobile backend, whatever) unchanged.
- **Application** (`InterviewPrep.Application`): CQRS commands/queries (MediatR), validators,
  pipeline behaviors, and the INTERFACES (`IMemberRepository`, `IUnitOfWork`, `IJwtTokenGenerator`...)
  that Infrastructure implements. Depends only on Domain.
- **Infrastructure** (`InterviewPrep.Infrastructure`): EF Core, the concrete repositories, JWT/BCrypt,
  the background job. Depends on Application (to implement its interfaces) and Domain.
- **Api**: controllers, middleware, DI wiring in `Program.cs`. The only layer that knows HTTP exists.

**Why this shape, concretely**: Application never references Infrastructure. That means you could
delete `InterviewPrep.Infrastructure` entirely, write a new one backed by MongoDB instead of EF
Core/SQLite, and not change a single command/query handler — because handlers only ever talk to
interfaces. That's the Dependency Inversion Principle operating at the ARCHITECTURE level, not
just inside one class. HishabPOS is built the same way, just with real PostgreSQL + Row-Level
Security instead of this demo's SQLite.

## 2. "Monolith vs. microservices — which would you choose, and why?"

Both this project AND HishabPOS are monoliths, deliberately. The honest answer, not the "buzzword"
answer:

- A monolith is the right DEFAULT for a team of this size and a product at this stage: one
  deployable, one database transaction boundary (see the `BEGIN/COMMIT` example in
  `sql/03_interview_queries.sql` — a cross-service "transaction" is a MUCH harder problem, usually
  solved with Sagas/compensating actions, not a real ACID transaction).
- Microservices earn their complexity cost when: different parts of the system need to scale
  independently, different teams need to deploy independently without blocking each other, or a
  part of the system has fundamentally different reliability/latency requirements than the rest.
  None of that is true here (or, early on, for HishabPOS).
- The CQRS + MediatR structure in this project is explicitly the STEPPING STONE toward services if
  it's ever needed: commands/queries are already small, named, independent units; the
  `PurchaseCreatedEvent` (`Application/Purchases/Events/`) is already a decoupled pub/sub boundary.
  If "Loyalty" ever needed to become its own service, the seam is already there — it wouldn't be a
  rewrite, just moving `LoyaltyTierUpgradeHandler`'s logic behind an HTTP/queue boundary instead of
  an in-process MediatR notification.

## 3. "How would you scale this if traffic grew 100x?"

In order of what you'd actually reach for first:

1. **Read replicas** for the member/purchase list queries (read-heavy) — the GetMembersQuery /
   GetPurchaseHistoryQuery handlers in this project already return `IQueryable`-composed,
   SQL-translated queries (see `IEnumerable<T>` vs `IQueryable<T>` discussion below), so they're
   already positioned to point at a replica connection string without code changes.
2. **Caching** the hot, read-heavy, rarely-changing data — `CachedMemberRepository.cs` is exactly
   this, as a Decorator. In production you'd likely move from in-process `IMemoryCache` to a
   distributed cache (Redis) the moment you run more than one API instance, because in-process
   caches go stale/inconsistent across instances.
3. **Horizontal scaling of the API** behind a load balancer — straightforward BECAUSE the API is
   stateless (the JWT carries all the session state the server needs; nothing is kept in server
   memory between requests except the demo's `TierAuditJob`, which is why that job specifically
   uses `IServiceScopeFactory` and is safe to run on every instance, or move to a single scheduled
   worker if duplicate runs become wasteful).
4. **Only then**, if a specific subsystem's load pattern genuinely diverges from the rest, consider
   splitting it into its own service.

## 4. "What's the CAP theorem, and where does this system land?"

PostgreSQL (what HishabPOS runs on) is a single-node, strongly-CONSISTENT, ACID system by default —
every read sees the latest committed write. That's the right choice for financial/sales data where
a stale read (e.g., showing a stock count that doesn't account for a just-completed sale) is a real
bug, not a cosmetic issue. The tradeoff is Availability during a partition/failover, which is an
acceptable cost for a system of this kind. A system that instead prioritizes Availability (and
tolerates Eventual Consistency) would reach for a different data store (e.g., DynamoDB/Cassandra) —
right for things like activity feeds or analytics counters, wrong for "did this payment get charged."

## 5. "How do you handle authentication vs. authorization, architecturally?"

- **Authentication** (who are you): JWT Bearer tokens, issued by `AuthController.Login` ->
  `LoginCommandHandler` -> `JwtTokenGenerator`. Stateless — the API never stores a session; the
  signed token IS the proof of identity, verified on every request via the signing key.
- **Authorization** (what are you allowed to do): role claims baked into the token
  (`ClaimTypes.Role`), checked declaratively with `[Authorize(Roles = "Admin")]` on
  `MembersController.Delete`. HishabPOS's real system goes one layer deeper: PostgreSQL
  Row-Level Security enforces tenant isolation AT THE DATABASE, so even a bug in application-level
  authorization code can't leak another tenant's rows — defense in depth, not relying on one layer
  alone to get it right.

## 6. "Why MediatR/CQRS instead of calling services directly from controllers?"

Controllers in this project are thin on purpose (`MembersController.cs` is a good example) — they
only translate HTTP <-> MediatR messages. The actual decision logic lives in handlers that are
pure C#, fully unit-testable without spinning up ASP.NET Core at all (see
`CreateMemberCommandHandlerTests.cs`). The real architectural win is the PIPELINE: validation,
logging, and performance-timing (`Application/Common/Behaviors/`) apply to EVERY command/query
automatically, with zero code duplicated across handlers — the same reason ASP.NET Core middleware
exists for HTTP, just one layer further in.

## 7. "How would you deploy this, and what does your CI/CD pipeline look like?"

Not built out in this demo project (deliberately — it's a learning project on SQLite, not a
deployed service), but the HONEST, real answer is the HishabPOS deployment you've actually shipped:
Docker images, GitHub Actions building/pushing them, DigitalOcean App Platform running them, with a
real custom-domain/DNS cutover you debugged yourself (see commits `fix(deploy): ...` in that repo's
history). That's a stronger answer than describing a pipeline you haven't actually operated.
