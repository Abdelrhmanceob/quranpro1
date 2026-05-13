-- MVP tables for recitation requests and Google Meet sessions

CREATE TABLE IF NOT EXISTS public.recitation_requests (
    id SERIAL PRIMARY KEY,
    student_id INT NOT NULL,
    teacher_id INT NOT NULL,
    exam_id INT NULL,
    notes TEXT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NULL,
    CONSTRAINT fk_recitation_request_student FOREIGN KEY (student_id) REFERENCES public.app_users(id),
    CONSTRAINT fk_recitation_request_teacher FOREIGN KEY (teacher_id) REFERENCES public.app_users(id),
    CONSTRAINT fk_recitation_request_exam FOREIGN KEY (exam_id) REFERENCES public.exams(id)
);

CREATE TABLE IF NOT EXISTS public.recitation_sessions (
    id SERIAL PRIMARY KEY,
    recitation_request_id INT NOT NULL,
    student_id INT NOT NULL,
    teacher_id INT NOT NULL,
    google_meet_url TEXT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NULL,
    CONSTRAINT fk_recitation_session_request FOREIGN KEY (recitation_request_id) REFERENCES public.recitation_requests(id),
    CONSTRAINT fk_recitation_session_student FOREIGN KEY (student_id) REFERENCES public.app_users(id),
    CONSTRAINT fk_recitation_session_teacher FOREIGN KEY (teacher_id) REFERENCES public.app_users(id)
);

CREATE INDEX IF NOT EXISTS idx_recitation_requests_student_id ON public.recitation_requests(student_id);
CREATE INDEX IF NOT EXISTS idx_recitation_requests_teacher_id ON public.recitation_requests(teacher_id);
CREATE INDEX IF NOT EXISTS idx_recitation_sessions_request_id ON public.recitation_sessions(recitation_request_id);