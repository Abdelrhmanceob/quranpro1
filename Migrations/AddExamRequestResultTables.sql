-- ============================================================
-- Maeen1 Exam Request & Result System - Supabase SQL Migration
-- Run this in Supabase SQL Editor after AddExamTaskSystem.sql
-- ============================================================

-- 1. Exam Requests table (Admin creates exam requests with plan for students)
CREATE TABLE IF NOT EXISTS public.exam_requests (
    id SERIAL PRIMARY KEY,
    student_id INTEGER NOT NULL REFERENCES public.app_users(id),
    admin_id INTEGER NOT NULL REFERENCES public.app_users(id),
    teacher_id INTEGER REFERENCES public.app_users(id),
    title VARCHAR(500) NOT NULL,
    description TEXT,
    plan_content TEXT,
    plan_objectives TEXT,
    plan_duration_minutes INTEGER,
    status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    -- Status values: Pending, AssignedToTeacher, ResultSent, Completed
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    assigned_at TIMESTAMP,
    result_sent_at TIMESTAMP
);

-- 2. Exam Results table (Teacher sends result after reviewing the plan)
CREATE TABLE IF NOT EXISTS public.exam_results (
    id SERIAL PRIMARY KEY,
    exam_request_id INTEGER NOT NULL REFERENCES public.exam_requests(id) ON DELETE CASCADE,
    teacher_id INTEGER NOT NULL REFERENCES public.app_users(id),
    student_id INTEGER NOT NULL REFERENCES public.app_users(id),
    score NUMERIC(6,2),
    max_score NUMERIC(6,2),
    grade VARCHAR(50),
    teacher_notes TEXT,
    strengths TEXT,
    weaknesses TEXT,
    recommendations TEXT,
    sent_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Indexes for performance
CREATE INDEX IF NOT EXISTS idx_exam_requests_student_id ON public.exam_requests(student_id);
CREATE INDEX IF NOT EXISTS idx_exam_requests_teacher_id ON public.exam_requests(teacher_id);
CREATE INDEX IF NOT EXISTS idx_exam_requests_admin_id ON public.exam_requests(admin_id);
CREATE INDEX IF NOT EXISTS idx_exam_requests_status ON public.exam_requests(status);
CREATE INDEX IF NOT EXISTS idx_exam_results_request_id ON public.exam_results(exam_request_id);
CREATE INDEX IF NOT EXISTS idx_exam_results_student_id ON public.exam_results(student_id);
CREATE INDEX IF NOT EXISTS idx_exam_results_teacher_id ON public.exam_results(teacher_id);
