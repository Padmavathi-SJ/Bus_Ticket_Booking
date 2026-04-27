-- SQL Script to Update Existing Buses with Female/Male Seat Counts
-- This script updates all buses that currently have FemaleSeats = 0 and MaleSeats = 0
-- It distributes seats as: 20% Female, 20% Male, 60% General

-- Update buses with seat distribution
UPDATE "Buses"
SET 
    "FemaleSeats" = FLOOR("TotalSeats" * 0.2),
    "MaleSeats" = FLOOR("TotalSeats" * 0.2)
WHERE 
    "FemaleSeats" = 0 
    AND "MaleSeats" = 0
    AND "TotalSeats" > 0;

-- Verify the update
SELECT 
    "Id",
    "BusName",
    "BusNumber",
    "TotalSeats",
    "FemaleSeats",
    "MaleSeats",
    ("TotalSeats" - "FemaleSeats" - "MaleSeats") AS "GeneralSeats"
FROM "Buses"
ORDER BY "CreatedAt" DESC;
