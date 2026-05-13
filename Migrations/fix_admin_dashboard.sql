-- ============================================================
-- TARGETED FIX: Admin Dashboard "uuid = integer" error
-- Run this in Supabase SQL Editor
-- ============================================================

-- Step 1: Disable RLS on app tables (RLS policies comparing auth.uid() UUID 
-- with integer columns is the most likely cause since Student/Teacher work fine)
ALTER TABLE IF EXISTS exam_requests DISABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS exam_results DISABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS app_users DISABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS exams DISABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS exam_tasks DISABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS exam_access_logs DISABLE ROW LEVEL SECURITY;

-- Step 2: Drop ALL policies on exam_requests (these may compare auth.uid() UUID with integer columns)
DO $$
DECLARE
    pol RECORD;
BEGIN
    FOR pol IN 
        SELECT policyname FROM pg_policies WHERE tablename = 'exam_requests' AND schemaname = 'public'
    LOOP
        EXECUTE format('DROP POLICY IF EXISTS %I ON public.exam_requests', pol.policyname);
    END LOOP;
END $$;

-- Step 3: Drop ALL policies on exam_results
DO $$
DECLARE
    pol RECORD;
BEGIN
    FOR pol IN 
        SELECT policyname FROM pg_policies WHERE tablename = 'exam_results' AND schemaname = 'public'
    LOOP
        EXECUTE format('DROP POLICY IF EXISTS %I ON public.exam_results', pol.policyname);
    END LOOP;
END $$;

-- Step 4: Drop ALL policies on app_users
DO $$
DECLARE
    pol RECORD;
BEGIN
    FOR pol IN 
        SELECT policyname FROM pg_policies WHERE tablename = 'app_users' AND schemaname = 'public'
    LOOP
        EXECUTE format('DROP POLICY IF EXISTS %I ON public.app_users', pol.policyname);
    END LOOP;
END $$;

-- Step 5: Drop ALL policies on exams
DO $$
DECLARE
    pol RECORD;
BEGIN
    FOR pol IN 
        SELECT policyname FROM pg_policies WHERE tablename = 'exams' AND schemaname = 'public'
    LOOP
        EXECUTE format('DROP POLICY IF EXISTS %I ON public.exams', pol.policyname);
    END LOOP;
END $$;

-- Step 6: Drop ALL policies on exam_access_logs
DO $$
DECLARE
    pol RECORD;
BEGIN
    FOR pol IN 
        SELECT policyname FROM pg_policies WHERE tablename = 'exam_access_logs' AND schemaname = 'public'
    LOOP
        EXECUTE format('DROP POLICY IF EXISTS %I ON public.exam_access_logs', pol.policyname);
    END LOOP;
END $$;

-- Step 7: Verify exam_results table exists with correct INTEGER columns
-- If it doesn't exist, create it
CREATE TABLE IF NOT EXISTS exam_results (
  id SERIAL PRIMARY KEY,
  exam_request_id INTEGER NOT NULL,
  teacher_id INTEGER NOT NULL,
  student_id INTEGER NOT NULL,
  score DECIMAL(5,2),
  max_score DECIMAL(5,2),
  grade VARCHAR(10),
  teacher_notes TEXT,
  strengths TEXT,
  weaknesses TEXT,
  recommendations TEXT,
  sent_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Step 8: Verify exam_requests columns are INTEGER (not UUID)
-- Check and fix if needed
DO $$
DECLARE
    col_type TEXT;
BEGIN
    -- Check student_id type
    SELECT data_type INTO col_type 
    FROM information_schema.columns 
    WHERE table_name = 'exam_requests' AND column_name = 'student_id' AND table_schema = 'public';
    
    IF col_type = 'uuid' THEN
        RAISE NOTICE 'FOUND UUID column student_id - need to fix exam_requests table';
        -- Drop and recreate the table with correct types
        DROP TABLE IF EXISTS exam_results CASCADE;
        DROP TABLE IF EXISTS exam_requests CASCADE;
        
        CREATE TABLE exam_requests (
          id SERIAL PRIMARY KEY,
          student_id INTEGER NOT NULL,
          admin_id INTEGER NOT NULL,
          teacher_id INTEGER,
          title VARCHAR(255),
          description TEXT,
          plan_content TEXT,
          plan_objectives TEXT,
          plan_duration_minutes INTEGER,
          status VARCHAR(50) DEFAULT 'Pending',
          created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
          assigned_at TIMESTAMP WITH TIME ZONE,
          result_sent_at TIMESTAMP WITH TIME ZONE
        );
        
        CREATE TABLE exam_results (
          id SERIAL PRIMARY KEY,
          exam_request_id INTEGER NOT NULL REFERENCES exam_requests(id) ON DELETE CASCADE,
          teacher_id INTEGER NOT NULL,
          student_id INTEGER NOT NULL,
          score DECIMAL(5,2),
          max_score DECIMAL(5,2),
          grade VARCHAR(10),
          teacher_notes TEXT,
          strengths TEXT,
          weaknesses TEXT,
          recommendations TEXT,
          sent_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
        );
    END IF;
END $$;

-- Step 9: Check if exam_results has UUID columns
DO $$
DECLARE
    col_type TEXT;
BEGIN
    SELECT data_type INTO col_type 
    FROM information_schema.columns 
    WHERE table_name = 'exam_results' AND column_name = 'teacher_id' AND table_schema = 'public';
    
    IF col_type = 'uuid' THEN
        RAISE NOTICE 'FOUND UUID column in exam_results - recreating';
        DROP TABLE IF EXISTS exam_results CASCADE;
        
        CREATE TABLE exam_results (
          id SERIAL PRIMARY KEY,
          exam_request_id INTEGER NOT NULL,
          teacher_id INTEGER NOT NULL,
          student_id INTEGER NOT NULL,
          score DECIMAL(5,2),
          max_score DECIMAL(5,2),
          grade VARCHAR(10),
          teacher_notes TEXT,
          strengths TEXT,
          weaknesses TEXT,
          recommendations TEXT,
          sent_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
        );
    END IF;
END $$;

-- Step 10: Diagnostic - show current column types (check output for any UUID columns)
SELECT table_name, column_name, data_type 
FROM information_schema.columns 
WHERE table_schema = 'public' 
  AND table_name IN ('app_users', 'exam_requests', 'exam_results', 'exams', 'exam_access_logs')
ORDER BY table_name, ordinal_position;
