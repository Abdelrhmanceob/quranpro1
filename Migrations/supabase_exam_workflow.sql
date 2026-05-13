-- FIX: Restore original exam_requests table and create separate workflow tables
-- The existing app uses exam_requests with INTEGER user IDs
-- Our new workflow tables use "wf_" prefix and UUID for Supabase Auth

-- Drop the incorrectly created tables from previous migration attempts
DROP TABLE IF EXISTS workflow_logs CASCADE;
DROP TABLE IF EXISTS notifications CASCADE;
DROP TABLE IF EXISTS exam_plans CASCADE;
DROP TABLE IF EXISTS teacher_assignments CASCADE;
DROP TABLE IF EXISTS exam_requests CASCADE;

-- Restore the ORIGINAL exam_requests table matching ExamRequest.cs model
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

-- Now create the NEW WORKFLOW tables with "wf_" prefix
-- These use UUID to reference auth.users for Supabase Auth integration

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

-- Indexes
CREATE INDEX idx_wf_exam_requests_student_id ON wf_exam_requests(student_id);
CREATE INDEX idx_wf_exam_requests_status ON wf_exam_requests(status);
CREATE INDEX idx_wf_exam_requests_created_at ON wf_exam_requests(created_at DESC);
CREATE INDEX idx_wf_teacher_assignments_request ON wf_teacher_assignments(exam_request_id);
CREATE INDEX idx_wf_teacher_assignments_teacher ON wf_teacher_assignments(teacher_id);
CREATE INDEX idx_wf_exam_plans_request ON wf_exam_plans(exam_request_id);
CREATE INDEX idx_wf_notifications_user ON wf_notifications(user_id);
CREATE INDEX idx_wf_notifications_read ON wf_notifications(is_read);
CREATE INDEX idx_wf_workflow_logs_request ON wf_workflow_logs(exam_request_id);

-- Enable RLS on workflow tables
ALTER TABLE wf_exam_requests ENABLE ROW LEVEL SECURITY;
ALTER TABLE wf_teacher_assignments ENABLE ROW LEVEL SECURITY;
ALTER TABLE wf_exam_plans ENABLE ROW LEVEL SECURITY;
ALTER TABLE wf_notifications ENABLE ROW LEVEL SECURITY;
ALTER TABLE wf_workflow_logs ENABLE ROW LEVEL SECURITY;

-- RLS Policies
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

-- Timestamp trigger function
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at = NOW();
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER wf_exam_requests_updated_at BEFORE UPDATE ON wf_exam_requests FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER wf_teacher_assignments_updated_at BEFORE UPDATE ON wf_teacher_assignments FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER wf_exam_plans_updated_at BEFORE UPDATE ON wf_exam_plans FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Re-insert user accounts into app_users (skip if already exist)
INSERT INTO public.app_users (name, email, password, role, is_onboarding_completed)
VALUES
  ('Admin', 'admin@maaen.com', 'Aa112233', 'Admin', true),
  ('Student', 'student1@gmail.com', 'Aa112233', 'Student', false),
  ('Teacher', 'teacher@maeen.com', 'Aa112233', 'Teacher', true)
ON CONFLICT (email) DO NOTHING;
