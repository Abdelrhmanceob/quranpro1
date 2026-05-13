-- ============================================================
-- Maeen1 Exam/Task System - Supabase SQL Migration
-- Run this in Supabase SQL Editor to create the required tables
-- ============================================================

-- 1. Exams table
CREATE TABLE IF NOT EXISTS public.exams (
    id SERIAL PRIMARY KEY,
    title VARCHAR(500) NOT NULL,
    description TEXT,
    teacher_id INTEGER NOT NULL REFERENCES public.app_users(id),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);

-- 2. Exam Tasks table
CREATE TABLE IF NOT EXISTS public.exam_tasks (
    id SERIAL PRIMARY KEY,
    exam_id INTEGER NOT NULL REFERENCES public.exams(id) ON DELETE CASCADE,
    student_id INTEGER NOT NULL REFERENCES public.app_users(id),
    title VARCHAR(500) NOT NULL,
    description TEXT,
    assigned_at TIMESTAMP NOT NULL DEFAULT NOW(),
    due_date TIMESTAMP,
    is_required BOOLEAN NOT NULL DEFAULT TRUE
);

-- 3. Student Task Completions table
CREATE TABLE IF NOT EXISTS public.student_task_completions (
    id SERIAL PRIMARY KEY,
    task_id INTEGER NOT NULL REFERENCES public.exam_tasks(id) ON DELETE CASCADE,
    student_id INTEGER NOT NULL REFERENCES public.app_users(id),
    completed_at TIMESTAMP NOT NULL DEFAULT NOW(),
    notes TEXT,
    is_approved BOOLEAN NOT NULL DEFAULT FALSE,
    approved_by_teacher_id INTEGER REFERENCES public.app_users(id),
    approved_at TIMESTAMP
);

-- 4. Exam Access Logs table
CREATE TABLE IF NOT EXISTS public.exam_access_logs (
    id SERIAL PRIMARY KEY,
    exam_id INTEGER NOT NULL REFERENCES public.exams(id) ON DELETE CASCADE,
    student_id INTEGER NOT NULL REFERENCES public.app_users(id),
    accessed_at TIMESTAMP NOT NULL DEFAULT NOW(),
    ip_address VARCHAR(100)
);

-- Indexes for performance
CREATE INDEX IF NOT EXISTS idx_exam_tasks_exam_id ON public.exam_tasks(exam_id);
CREATE INDEX IF NOT EXISTS idx_exam_tasks_student_id ON public.exam_tasks(student_id);
CREATE INDEX IF NOT EXISTS idx_student_task_completions_task_id ON public.student_task_completions(task_id);
CREATE INDEX IF NOT EXISTS idx_student_task_completions_student_id ON public.student_task_completions(student_id);
CREATE INDEX IF NOT EXISTS idx_exam_access_logs_exam_id ON public.exam_access_logs(exam_id);
CREATE INDEX IF NOT EXISTS idx_exam_access_logs_student_id ON public.exam_access_logs(student_id);
