-- ============================================================
-- FIX: Comprehensive database repair for Admin Dashboard 500 error
-- Problem: "42883: operator does not exist: uuid = integer"
-- Root cause: Previous migrations may have altered app_users.id to UUID
--             or left exam_results with UUID columns
-- ============================================================

-- Step 1: Drop ALL problematic tables that may have wrong column types
-- Order matters due to FK dependencies

DROP TABLE IF EXISTS student_task_completions CASCADE;
DROP TABLE IF EXISTS exam_access_logs CASCADE;
DROP TABLE IF EXISTS exam_tasks CASCADE;
DROP TABLE IF EXISTS exam_results CASCADE;
DROP TABLE IF EXISTS exam_requests CASCADE;
DROP TABLE IF EXISTS exams CASCADE;
DROP TABLE IF EXISTS student_onboarding_profiles CASCADE;
DROP TABLE IF EXISTS teacher_availabilities CASCADE;

-- Step 2: Drop and recreate app_users to ensure id is INTEGER
-- First drop the wf_ tables that might reference auth.users (not app_users)
DROP TABLE IF EXISTS wf_workflow_logs CASCADE;
DROP TABLE IF EXISTS wf_notifications CASCADE;
DROP TABLE IF EXISTS wf_exam_plans CASCADE;
DROP TABLE IF EXISTS wf_teacher_assignments CASCADE;
DROP TABLE IF EXISTS wf_exam_requests CASCADE;

-- Now safely recreate app_users
DROP TABLE IF EXISTS app_users CASCADE;

CREATE TABLE app_users (
  id SERIAL PRIMARY KEY,
  name VARCHAR(255),
  email VARCHAR(255) UNIQUE NOT NULL,
  password VARCHAR(255) NOT NULL,
  role VARCHAR(50) NOT NULL DEFAULT 'Student',
  is_onboarding_completed BOOLEAN DEFAULT FALSE,
  onboarding_completed_at TIMESTAMP WITH TIME ZONE,
  student_level VARCHAR(100)
);

-- Step 3: Recreate teacher_availabilities
CREATE TABLE teacher_availabilities (
  id SERIAL PRIMARY KEY,
  teacher_name VARCHAR(255),
  date VARCHAR(50),
  time VARCHAR(50)
);

-- Step 4: Recreate student_onboarding_profiles
CREATE TABLE student_onboarding_profiles (
  id SERIAL PRIMARY KEY,
  user_id INTEGER NOT NULL REFERENCES app_users(id) ON DELETE CASCADE,
  daily_memorization_hours INTEGER DEFAULT 0,
  target_juz_count INTEGER DEFAULT 0,
  completed_juz_count INTEGER DEFAULT 0,
  tajweed_level VARCHAR(100),
  target_duration VARCHAR(100),
  determined_level VARCHAR(100),
  memorization_plan TEXT,
  suggested_teacher_name VARCHAR(255),
  recommendation_source VARCHAR(255),
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Step 5: Recreate exams
CREATE TABLE exams (
  id SERIAL PRIMARY KEY,
  title VARCHAR(255) NOT NULL,
  description TEXT,
  teacher_id INTEGER NOT NULL REFERENCES app_users(id) ON DELETE CASCADE,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  is_active BOOLEAN DEFAULT TRUE
);

-- Step 6: Recreate exam_tasks
CREATE TABLE exam_tasks (
  id SERIAL PRIMARY KEY,
  exam_id INTEGER NOT NULL REFERENCES exams(id) ON DELETE CASCADE,
  student_id INTEGER NOT NULL REFERENCES app_users(id) ON DELETE CASCADE,
  title VARCHAR(255) NOT NULL,
  description TEXT,
  assigned_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  due_date TIMESTAMP WITH TIME ZONE,
  is_required BOOLEAN DEFAULT TRUE
);

-- Step 7: Recreate student_task_completions
CREATE TABLE student_task_completions (
  id SERIAL PRIMARY KEY,
  task_id INTEGER NOT NULL REFERENCES exam_tasks(id) ON DELETE CASCADE,
  student_id INTEGER NOT NULL REFERENCES app_users(id) ON DELETE CASCADE,
  completed_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  notes TEXT,
  is_approved BOOLEAN DEFAULT FALSE,
  approved_by_teacher_id INTEGER REFERENCES app_users(id) ON DELETE SET NULL,
  approved_at TIMESTAMP WITH TIME ZONE
);

-- Step 8: Recreate exam_access_logs
CREATE TABLE exam_access_logs (
  id SERIAL PRIMARY KEY,
  exam_id INTEGER NOT NULL REFERENCES exams(id) ON DELETE CASCADE,
  student_id INTEGER NOT NULL REFERENCES app_users(id) ON DELETE CASCADE,
  accessed_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  ip_address VARCHAR(50)
);

-- Step 9: Recreate exam_requests (INTEGER IDs matching ExamRequest.cs)
CREATE TABLE exam_requests (
  id SERIAL PRIMARY KEY,
  student_id INTEGER NOT NULL REFERENCES app_users(id) ON DELETE CASCADE,
  admin_id INTEGER NOT NULL REFERENCES app_users(id) ON DELETE CASCADE,
  teacher_id INTEGER REFERENCES app_users(id) ON DELETE SET NULL,
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

-- Step 10: Recreate exam_results (INTEGER IDs matching ExamResult.cs)
CREATE TABLE exam_results (
  id SERIAL PRIMARY KEY,
  exam_request_id INTEGER NOT NULL REFERENCES exam_requests(id) ON DELETE CASCADE,
  teacher_id INTEGER NOT NULL REFERENCES app_users(id) ON DELETE CASCADE,
  student_id INTEGER NOT NULL REFERENCES app_users(id) ON DELETE CASCADE,
  score DECIMAL(5,2),
  max_score DECIMAL(5,2),
  grade VARCHAR(10),
  teacher_notes TEXT,
  strengths TEXT,
  weaknesses TEXT,
  recommendations TEXT,
  sent_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Step 11: Create indexes for performance
CREATE INDEX idx_exam_requests_student_id ON exam_requests(student_id);
CREATE INDEX idx_exam_requests_admin_id ON exam_requests(admin_id);
CREATE INDEX idx_exam_requests_teacher_id ON exam_requests(teacher_id);
CREATE INDEX idx_exam_requests_status ON exam_requests(status);
CREATE INDEX idx_exam_results_request_id ON exam_results(exam_request_id);
CREATE INDEX idx_exam_results_student_id ON exam_results(student_id);
CREATE INDEX idx_exam_tasks_exam_id ON exam_tasks(exam_id);
CREATE INDEX idx_exam_tasks_student_id ON exam_tasks(student_id);
CREATE INDEX idx_exam_access_logs_exam_id ON exam_access_logs(exam_id);
CREATE INDEX idx_student_onboarding_user_id ON student_onboarding_profiles(user_id);

-- Step 12: Insert user accounts
INSERT INTO app_users (name, email, password, role, is_onboarding_completed)
VALUES
  ('Admin', 'admin@maaen.com', 'Aa112233', 'Admin', true),
  ('Student', 'student1@gmail.com', 'Aa112233', 'Student', false),
  ('Teacher', 'teacher@maeen.com', 'Aa112233', 'Teacher', true);

-- Step 13: Now recreate the workflow tables (wf_ prefix) with UUID for Supabase Auth
CREATE TABLE wf_exam_requests (
  id BIGSERIAL PRIMARY KEY,
  student_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
  admin_id UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  teacher_id UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  surah_name VARCHAR(100) NOT NULL,
  ayah_range VARCHAR(50) NOT NULL,
  difficulty_level VARCHAR(50) NOT NULL,
  memorization_level VARCHAR(100),
  tajweed_weaknesses TEXT,
  student_notes TEXT,
  submitted_answers JSONB,
  status VARCHAR(50) NOT NULL DEFAULT 'pending',
  admin_comments TEXT,
  teacher_comments TEXT,
  generated_test_plan JSONB,
  success_roadmap JSONB,
  exam_scheduled_date TIMESTAMP WITH TIME ZONE,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  submitted_at TIMESTAMP WITH TIME ZONE,
  reviewed_at TIMESTAMP WITH TIME ZONE,
  approved_at TIMESTAMP WITH TIME ZONE,
  assigned_at TIMESTAMP WITH TIME ZONE,
  completed_at TIMESTAMP WITH TIME ZONE,
  updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE TABLE wf_teacher_assignments (
  id BIGSERIAL PRIMARY KEY,
  exam_request_id BIGINT NOT NULL REFERENCES wf_exam_requests(id) ON DELETE CASCADE,
  teacher_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
  assigned_by_admin_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE SET NULL,
  assignment_notes TEXT,
  assigned_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE TABLE wf_exam_plans (
  id BIGSERIAL PRIMARY KEY,
  exam_request_id BIGINT NOT NULL REFERENCES wf_exam_requests(id) ON DELETE CASCADE,
  teacher_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
  test_plan JSONB NOT NULL,
  tajweed_focus JSONB,
  memorization_tasks JSONB,
  preparation_notes TEXT,
  motivational_guidance TEXT,
  difficulty_adjustments JSONB,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE TABLE wf_notifications (
  id BIGSERIAL PRIMARY KEY,
  user_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
  exam_request_id BIGINT REFERENCES wf_exam_requests(id) ON DELETE CASCADE,
  notification_type VARCHAR(100) NOT NULL,
  title VARCHAR(255) NOT NULL,
  message TEXT NOT NULL,
  data JSONB,
  is_read BOOLEAN DEFAULT FALSE,
  read_at TIMESTAMP WITH TIME ZONE,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE TABLE wf_workflow_logs (
  id BIGSERIAL PRIMARY KEY,
  exam_request_id BIGINT NOT NULL REFERENCES wf_exam_requests(id) ON DELETE CASCADE,
  previous_status VARCHAR(50),
  new_status VARCHAR(50) NOT NULL,
  changed_by_user_id UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  changed_by_role VARCHAR(50),
  reason TEXT,
  comments TEXT,
  ip_address INET,
  changed_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Step 14: Indexes for workflow tables
CREATE INDEX idx_wf_exam_requests_student_id ON wf_exam_requests(student_id);
CREATE INDEX idx_wf_exam_requests_status ON wf_exam_requests(status);
CREATE INDEX idx_wf_exam_requests_created_at ON wf_exam_requests(created_at DESC);
CREATE INDEX idx_wf_teacher_assignments_request ON wf_teacher_assignments(exam_request_id);
CREATE INDEX idx_wf_teacher_assignments_teacher ON wf_teacher_assignments(teacher_id);
CREATE INDEX idx_wf_exam_plans_request ON wf_exam_plans(exam_request_id);
CREATE INDEX idx_wf_notifications_user ON wf_notifications(user_id);
CREATE INDEX idx_wf_notifications_read ON wf_notifications(is_read);
CREATE INDEX idx_wf_workflow_logs_request ON wf_workflow_logs(exam_request_id);

-- Step 15: Enable RLS on workflow tables
ALTER TABLE wf_exam_requests ENABLE ROW LEVEL SECURITY;
ALTER TABLE wf_teacher_assignments ENABLE ROW LEVEL SECURITY;
ALTER TABLE wf_exam_plans ENABLE ROW LEVEL SECURITY;
ALTER TABLE wf_notifications ENABLE ROW LEVEL SECURITY;
ALTER TABLE wf_workflow_logs ENABLE ROW LEVEL SECURITY;

-- Step 16: RLS Policies for workflow tables
CREATE POLICY "Students can view own requests" ON wf_exam_requests FOR SELECT
  USING (auth.uid() = student_id);
CREATE POLICY "Users can view related requests" ON wf_exam_requests FOR SELECT
  USING (admin_id = auth.uid() OR teacher_id = auth.uid());
CREATE POLICY "Students can create requests" ON wf_exam_requests FOR INSERT
  WITH CHECK (auth.uid() = student_id);
CREATE POLICY "Admins can update requests" ON wf_exam_requests FOR UPDATE
  USING (admin_id = auth.uid());
CREATE POLICY "Teachers can update assigned requests" ON wf_exam_requests FOR UPDATE
  USING (teacher_id = auth.uid());

CREATE POLICY "Admins can manage assignments" ON wf_teacher_assignments FOR ALL
  USING (assigned_by_admin_id = auth.uid());
CREATE POLICY "Teachers can view assignments" ON wf_teacher_assignments FOR SELECT
  USING (auth.uid() = teacher_id);

CREATE POLICY "Teachers can manage plans" ON wf_exam_plans FOR ALL
  USING (auth.uid() = teacher_id);
CREATE POLICY "Students can view plans" ON wf_exam_plans FOR SELECT
  USING (EXISTS (SELECT 1 FROM wf_exam_requests WHERE wf_exam_requests.id = wf_exam_plans.exam_request_id AND wf_exam_requests.student_id = auth.uid()));

CREATE POLICY "Users can view own notifications" ON wf_notifications FOR SELECT
  USING (auth.uid() = user_id);
CREATE POLICY "System can create notifications" ON wf_notifications FOR INSERT
  WITH CHECK (TRUE);
CREATE POLICY "Users can update own notifications" ON wf_notifications FOR UPDATE
  USING (auth.uid() = user_id);

CREATE POLICY "Users can view related logs" ON wf_workflow_logs FOR SELECT
  USING (EXISTS (SELECT 1 FROM wf_exam_requests WHERE wf_exam_requests.id = wf_workflow_logs.exam_request_id AND (wf_exam_requests.student_id = auth.uid() OR wf_exam_requests.admin_id = auth.uid() OR wf_exam_requests.teacher_id = auth.uid())));

-- Step 17: Timestamp trigger
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at = NOW();
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS wf_exam_requests_updated_at ON wf_exam_requests;
DROP TRIGGER IF EXISTS wf_teacher_assignments_updated_at ON wf_teacher_assignments;
DROP TRIGGER IF EXISTS wf_exam_plans_updated_at ON wf_exam_plans;

CREATE TRIGGER wf_exam_requests_updated_at BEFORE UPDATE ON wf_exam_requests FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER wf_teacher_assignments_updated_at BEFORE UPDATE ON wf_teacher_assignments FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER wf_exam_plans_updated_at BEFORE UPDATE ON wf_exam_plans FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
