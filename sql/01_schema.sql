-- PostgreSQL schema mirroring the EF Core model in src/InterviewPrep.Infrastructure/Persistence --
-- same tables the API actually runs against, so these queries are answers you can run for real,
-- not hypothetical SQL. (HishabPOS itself runs on Postgres 17 -- this is the same dialect.)
-- MSSQL users: note T-SQL differences called out inline where they matter.

DROP TABLE IF EXISTS "Purchases";
DROP TABLE IF EXISTS "Members";
DROP TABLE IF EXISTS "Users";

CREATE TABLE "Members" (
    "Id"            SERIAL PRIMARY KEY,       -- MSSQL: IDENTITY(1,1) instead of SERIAL
    "Name"          VARCHAR(100) NOT NULL,
    "Age"           INT NOT NULL,
    "Gender"        VARCHAR(20) NOT NULL,
    "Tier"          VARCHAR(20) NOT NULL,
    "CreatedAtUtc"  TIMESTAMP NOT NULL,
    "UpdatedAtUtc"  TIMESTAMP NULL
);

CREATE TABLE "Purchases" (
    "Id"                  SERIAL PRIMARY KEY,
    "MemberId"            INT NOT NULL REFERENCES "Members"("Id") ON DELETE CASCADE,
    "Amount"              DECIMAL(18,2) NOT NULL,
    "DiscountPercentage"  DECIMAL(5,2) NOT NULL,
    "DiscountAmount"      DECIMAL(18,2) NOT NULL,
    "FinalAmount"         DECIMAL(18,2) NOT NULL,
    "CreatedAtUtc"        TIMESTAMP NOT NULL,
    "UpdatedAtUtc"        TIMESTAMP NULL
);

CREATE INDEX "IX_Purchases_MemberId" ON "Purchases" ("MemberId");

CREATE TABLE "Users" (
    "Id"            SERIAL PRIMARY KEY,
    "Username"      VARCHAR(50) NOT NULL,
    "PasswordHash"  VARCHAR(100) NOT NULL,
    "Role"          VARCHAR(20) NOT NULL,
    "CreatedAtUtc"  TIMESTAMP NOT NULL,
    "UpdatedAtUtc"  TIMESTAMP NULL
);

CREATE UNIQUE INDEX "IX_Users_Username" ON "Users" ("Username");

-- NORMALIZATION, if asked to explain this schema in those terms:
-- Members and Purchases are already in 3NF -- no column depends on anything other than the whole
-- primary key, and there's no transitive dependency (e.g. we don't store a member's name again on
-- every Purchase row, which would be a classic 2NF/3NF violation and an update-anomaly risk).
