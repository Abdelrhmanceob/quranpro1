using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Maeen1_New.Models;

namespace Maeen1_New.Services
{
    public class TeacherNotificationService
    {
        public void CreateAssessmentNotification(
            Maeen1_NewDbContext db,
            int teacherUserId,
            int studentUserId,
            string studentName,
            string assessmentLevel,
            string assessmentSummary)
        {
            EnsureNotificationsTable(db);

            var safeSummary = string.IsNullOrWhiteSpace(assessmentSummary)
                ? string.Empty
                : assessmentSummary.Trim();

            db.Database.ExecuteSqlCommand(
                @"INSERT INTO public.teacher_notifications
                  (teacher_user_id, student_user_id, student_name, assessment_level, assessment_summary, created_at, is_read)
                  VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6)",
                teacherUserId,
                studentUserId,
                studentName ?? string.Empty,
                assessmentLevel ?? string.Empty,
                safeSummary,
                DateTime.UtcNow,
                false);
        }

        public IList<TeacherNotificationItem> GetTeacherNotifications(
            Maeen1_NewDbContext db,
            int teacherUserId,
            int limit = 20)
        {
            EnsureNotificationsTable(db);

            var safeLimit = Math.Max(1, Math.Min(100, limit));

            var items = db.Database.SqlQuery<TeacherNotificationItem>(
                @"SELECT id,
                         teacher_user_id AS TeacherUserId,
                         student_user_id AS StudentUserId,
                         student_name AS StudentName,
                         assessment_level AS AssessmentLevel,
                         assessment_summary AS AssessmentSummary,
                         created_at AS CreatedAt,
                         is_read AS IsRead
                  FROM public.teacher_notifications
                  WHERE teacher_user_id = @p0
                  ORDER BY created_at DESC
                  LIMIT @p1",
                teacherUserId,
                safeLimit);

            return items.ToList();
        }

        private void EnsureNotificationsTable(DbContext db)
        {
            db.Database.ExecuteSqlCommand(
                @"CREATE TABLE IF NOT EXISTS public.teacher_notifications (
                    id SERIAL PRIMARY KEY,
                    teacher_user_id INTEGER NOT NULL,
                    student_user_id INTEGER NOT NULL,
                    student_name TEXT NOT NULL,
                    assessment_level VARCHAR(100),
                    assessment_summary TEXT,
                    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
                    is_read BOOLEAN NOT NULL DEFAULT FALSE
                )");
        }
    }
}
