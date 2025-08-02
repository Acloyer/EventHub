-- Fix database by removing IpAddress column from ActivityLogs table
-- Execute this script in pgAdmin or any PostgreSQL client

-- Check if column exists before dropping
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'ActivityLogs' 
        AND column_name = 'IpAddress'
    ) THEN
        ALTER TABLE "ActivityLogs" DROP COLUMN "IpAddress";
        RAISE NOTICE 'IpAddress column removed from ActivityLogs table';
    ELSE
        RAISE NOTICE 'IpAddress column does not exist in ActivityLogs table';
    END IF;
END $$; 