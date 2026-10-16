-- Common SQL interview questions, answered as runnable queries against the seeded schema above.
-- Each block names the question it answers. Run after 01_schema.sql and 02_seed.sql.

-- =====================================================================================
-- Q: "What's the difference between INNER JOIN and LEFT JOIN? Give an example."
-- =====================================================================================
-- INNER JOIN: only members that HAVE at least one purchase (Evan Teo, with zero purchases, is dropped).
SELECT m."Name", p."Amount"
FROM "Members" m
INNER JOIN "Purchases" p ON p."MemberId" = m."Id";

-- LEFT JOIN: every member, purchase columns NULL where there isn't one -- Evan Teo now appears
-- with NULL "Amount". This is the query you'd actually use for "list all members and their most
-- recent purchase, including members who've never bought anything."
SELECT m."Name", p."Amount"
FROM "Members" m
LEFT JOIN "Purchases" p ON p."MemberId" = m."Id";

-- =====================================================================================
-- Q: "Find members who have never made a purchase." (anti-join pattern)
-- =====================================================================================
SELECT m."Name"
FROM "Members" m
LEFT JOIN "Purchases" p ON p."MemberId" = m."Id"
WHERE p."Id" IS NULL;

-- Equivalent with NOT EXISTS -- usually the better-optimized form on large tables because the
-- planner can stop at the first match instead of building the full join then filtering it away.
SELECT m."Name"
FROM "Members" m
WHERE NOT EXISTS (SELECT 1 FROM "Purchases" p WHERE p."MemberId" = m."Id");

-- =====================================================================================
-- Q: "GROUP BY vs HAVING -- find members with total lifetime spend over 500."
-- =====================================================================================
-- WHERE filters ROWS before grouping; HAVING filters GROUPS after aggregation -- you cannot write
-- `WHERE SUM("FinalAmount") > 500` because WHERE runs before SUM exists yet. That's the one-line
-- answer to "why does HAVING exist when WHERE already filters."
SELECT m."Name", SUM(p."FinalAmount") AS "LifetimeSpend", COUNT(p."Id") AS "PurchaseCount"
FROM "Members" m
JOIN "Purchases" p ON p."MemberId" = m."Id"
GROUP BY m."Id", m."Name"
HAVING SUM(p."FinalAmount") > 500
ORDER BY "LifetimeSpend" DESC;

-- =====================================================================================
-- Q: "Find duplicate rows" (same Name appearing more than once -- Alice Tan, two different people)
-- =====================================================================================
SELECT "Name", COUNT(*) AS "Occurrences"
FROM "Members"
GROUP BY "Name"
HAVING COUNT(*) > 1;

-- =====================================================================================
-- Q: "Find the Nth highest value" (classic "2nd highest salary" question, here: 2nd highest purchase)
-- =====================================================================================
-- Simple approach: works, but doesn't handle ties the way most interviewers expect (it just skips
-- a row, it doesn't rank).
SELECT DISTINCT "Amount"
FROM "Purchases"
ORDER BY "Amount" DESC
LIMIT 1 OFFSET 1; -- MSSQL: OFFSET 1 ROWS FETCH NEXT 1 ROWS ONLY (no LIMIT keyword)

-- Window-function approach: DENSE_RANK() correctly treats TIES as the same rank, so "2nd highest"
-- means "2nd distinct value," not "2nd row" -- almost always the intended meaning, and the answer
-- that signals you actually understand ranking functions instead of just OFFSET.
SELECT "Amount" FROM (
    SELECT "Amount", DENSE_RANK() OVER (ORDER BY "Amount" DESC) AS rnk
    FROM "Purchases"
) ranked
WHERE rnk = 2
LIMIT 1;

-- =====================================================================================
-- Q: "What's the difference between RANK(), DENSE_RANK(), and ROW_NUMBER()?"
-- =====================================================================================
-- Demonstrated side by side on the same data so ties are visible: ROW_NUMBER never repeats a
-- number (arbitrary tiebreak); RANK repeats a number on a tie, then SKIPS the next one (1,2,2,4);
-- DENSE_RANK repeats on a tie but never skips (1,2,2,3).
SELECT m."Name", p."Amount",
    ROW_NUMBER() OVER (ORDER BY p."Amount" DESC) AS row_num,
    RANK()       OVER (ORDER BY p."Amount" DESC) AS rnk,
    DENSE_RANK() OVER (ORDER BY p."Amount" DESC) AS dense_rnk
FROM "Purchases" p
JOIN "Members" m ON m."Id" = p."MemberId"
ORDER BY p."Amount" DESC;

-- =====================================================================================
-- Q: "Give an example of a window function used for a running total."
-- =====================================================================================
-- PARTITION BY resets the running total PER MEMBER (restarts at each member's first purchase);
-- without it, SUM() OVER would run across the whole result set as one global total.
SELECT m."Name", p."CreatedAtUtc", p."FinalAmount",
    SUM(p."FinalAmount") OVER (PARTITION BY m."Id" ORDER BY p."CreatedAtUtc") AS "RunningTotal"
FROM "Purchases" p
JOIN "Members" m ON m."Id" = p."MemberId"
ORDER BY m."Name", p."CreatedAtUtc";

-- =====================================================================================
-- Q: "Write a query using a CTE (WITH clause). When would you use one over a subquery?"
-- =====================================================================================
-- Same result as the HAVING query above, but named and readable -- the real reason to reach for a
-- CTE is when the SAME derived result is referenced more than once in the outer query (a plain
-- subquery would have to be repeated/duplicated), or just to break a deeply nested query into
-- named, top-to-bottom readable steps.
WITH member_spend AS (
    SELECT m."Id", m."Name", SUM(p."FinalAmount") AS "LifetimeSpend"
    FROM "Members" m
    JOIN "Purchases" p ON p."MemberId" = m."Id"
    GROUP BY m."Id", m."Name"
)
SELECT "Name", "LifetimeSpend"
FROM member_spend
WHERE "LifetimeSpend" > (SELECT AVG("LifetimeSpend") FROM member_spend); -- above-average spenders

-- =====================================================================================
-- Q: "Conditional aggregation -- count members by gender and tier in one query, pivot-style."
-- =====================================================================================
SELECT
    "Gender",
    COUNT(*) AS "TotalMembers",
    COUNT(*) FILTER (WHERE "Tier" = 'Gold') AS "GoldCount",      -- Postgres FILTER clause
    SUM(CASE WHEN "Tier" = 'Platinum' THEN 1 ELSE 0 END) AS "PlatinumCount" -- portable CASE WHEN form (works on MSSQL too)
FROM "Members"
GROUP BY "Gender";

-- =====================================================================================
-- Q: "How would you paginate results efficiently?"
-- =====================================================================================
-- OFFSET/LIMIT (what GetMembersQueryHandler.cs does via EF Core's Skip/Take) is simple but gets
-- SLOWER as the offset grows -- the database still has to scan and discard all the skipped rows.
SELECT "Id", "Name" FROM "Members" ORDER BY "Id" LIMIT 20 OFFSET 40;

-- Keyset/cursor pagination: no OFFSET at all -- resume from the last seen Id instead. Doesn't
-- degrade on deep pages, but requires a stable sort key (here, Id) and can't jump to "page 7"
-- directly, only "next page after X." The honest tradeoff to raise if asked which one you'd ship.
SELECT "Id", "Name" FROM "Members" WHERE "Id" > 40 ORDER BY "Id" LIMIT 20;

-- =====================================================================================
-- Q: "What index would you add, and why?" / EXPLAIN basics
-- =====================================================================================
-- "IX_Purchases_MemberId" (already in 01_schema.sql) exists specifically because every query above
-- joins/filters Purchases by MemberId -- without it, each of those joins would be a full table scan
-- of Purchases for every Member row. Prove it, don't just assert it:
EXPLAIN ANALYZE
SELECT * FROM "Purchases" WHERE "MemberId" = 1;
-- Look for "Index Scan using IX_Purchases_MemberId" in the output, not "Seq Scan."

-- =====================================================================================
-- Q: "What's a transaction, and why does it matter here?" (ACID, tied to CreatePurchaseCommand)
-- =====================================================================================
-- CreatePurchaseCommandHandler.cs does TWO writes (insert the Purchase, then later UpgradeTier)
-- that must succeed or fail TOGETHER -- that's the "Atomicity" in ACID, and it's exactly what
-- IUnitOfWork.SaveChangesAsync wraps in one transaction under the hood via EF Core's change
-- tracker. The manual SQL equivalent, if asked to write it directly:
BEGIN;
INSERT INTO "Purchases" ("MemberId", "Amount", "DiscountPercentage", "DiscountAmount", "FinalAmount", "CreatedAtUtc")
VALUES (2, 1000.00, 10.00, 100.00, 900.00, NOW());
UPDATE "Members" SET "Tier" = 'Gold', "UpdatedAtUtc" = NOW() WHERE "Id" = 2;
COMMIT; -- if anything between BEGIN and here throws, ROLLBACK undoes both statements, not just one
