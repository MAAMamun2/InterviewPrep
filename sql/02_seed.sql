-- Enough rows for the queries in 03_interview_queries.sql to return meaningful, non-trivial results
-- (duplicates, ties, members with zero purchases, etc.) -- a flat 2-row seed can't exercise most
-- of these patterns honestly.

INSERT INTO "Members" ("Name", "Age", "Gender", "Tier", "CreatedAtUtc") VALUES
('Alice Tan',   29, 'Female', 'Gold',     NOW() - INTERVAL '90 days'),
('Bob Lim',     41, 'Male',   'Silver',   NOW() - INTERVAL '75 days'),
('Charlie Wong',17, 'Male',   'Silver',   NOW() - INTERVAL '60 days'),
('Diana Lee',   52, 'Female', 'Platinum', NOW() - INTERVAL '45 days'),
('Alice Tan',   31, 'Female', 'Silver',   NOW() - INTERVAL '10 days'), -- deliberate name duplicate, different person
('Evan Teo',    24, 'Male',   'Silver',   NOW() - INTERVAL '5 days');  -- no purchases yet, on purpose

INSERT INTO "Purchases" ("MemberId", "Amount", "DiscountPercentage", "DiscountAmount", "FinalAmount", "CreatedAtUtc") VALUES
(1, 250.00, 18.00, 45.00, 205.00, NOW() - INTERVAL '80 days'),
(1, 600.00, 31.00, 186.00, 414.00, NOW() - INTERVAL '40 days'),
(1, 150.00, 18.00, 27.00, 123.00, NOW() - INTERVAL '3 days'),
(2, 620.00, 13.00, 80.60, 539.40, NOW() - INTERVAL '70 days'),
(2, 300.00, 5.00, 15.00, 285.00, NOW() - INTERVAL '20 days'),
(3, 80.00, 5.00, 4.00, 76.00, NOW() - INTERVAL '55 days'),
(4, 900.00, 18.00, 162.00, 738.00, NOW() - INTERVAL '30 days'),
(4, 1200.00, 18.00, 216.00, 984.00, NOW() - INTERVAL '6 days'),
(5, 400.00, 13.00, 52.00, 348.00, NOW() - INTERVAL '8 days');
