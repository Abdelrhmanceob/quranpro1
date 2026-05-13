-- Insert user accounts into app_users table
-- Run this in Supabase SQL Editor to create the login accounts

INSERT INTO public.app_users (name, email, password, role, is_onboarding_completed)
VALUES
  ('Admin', 'admin@maaen.com', 'Aa112233', 'Admin', true),
  ('Student', 'student1@gmail.com', 'Aa112233', 'Student', false),
  ('Teacher', 'teacher@maeen.com', 'Aa112233', 'Teacher', true)
ON CONFLICT (email) DO NOTHING;
