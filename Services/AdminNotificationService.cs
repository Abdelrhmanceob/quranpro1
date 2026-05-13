using System;
using System.Collections.Generic;
using System.Linq;
using Maeen1_New.Models;

namespace Maeen1_New.Services
{
    public class AdminNotificationService
    {
        public void CreateTeacherDecisionNotifications(
            Maeen1_NewDbContext db,
            int teacherUserId,
            string teacherName,
            int examRequestId,
            string examTitle,
            string decisionStatus,
            string decisionLabel,
            string decisionNotes)
        {
            EnsureAdminNotificationsTable(db);

            var adminIds = db.Users
                .Where(u => u.Role == "Admin")
                .Select(u => u.Id)
                .ToList();

            if (!adminIds.Any())
            {
                return;
            }

            var safeTeacherName = string.IsNullOrWhiteSpace(teacherName) ? string.Empty : teacherName.Trim();
            var safeExamTitle = string.IsNullOrWhiteSpace(examTitle) ? string.Empty : examTitle.Trim();
            var safeDecisionStatus = string.IsNullOrWhiteSpace(decisionStatus) ? string.Empty : decisionStatus.Trim();
            var safeDecisionLabel = string.IsNullOrWhiteSpace(decisionLabel) ? string.Empty : decisionLabel.Trim();
            var safeDecisionNotes = string.IsNullOrWhiteSpace(decisionNotes) ? string.Empty : decisionNotes.Trim();
            var now = DateTime.UtcNow;

            foreach (var adminId in adminIds)
            {
                db.Database.ExecuteSqlCommand(
                    @"INSERT INTO public.admin_teacher_decision_notifications
                      (admin_user_id, teacher_user_id, teacher_name, exam_request_id, exam_title, decision_status, decision_label, decision_notes, created_at, is_read)
                      VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9)",
                    adminId,
                    teacherUserId,
                    safeTeacherName,
                    examRequestId,
                    safeExamTitle,
                    safeDecisionStatus,
                    safeDecisionLabel,
                    safeDecisionNotes,
                    now,
                    false);
            }
        }

        public IList<AdminTeacherDecisionNotificationItem> GetLatestTeacherDecisionNotifications(
            Maeen1_NewDbContext db,
            int adminUserId,
            int limit = 10)
        {
            EnsureAdminNotificationsTable(db);

            var safeLimit = Math.Max(1, Math.Min(100, limit));

            var items = db.Database.SqlQuery<AdminTeacherDecisionNotificationItem>(
                @"SELECT id,
                         admin_user_id AS AdminUserId,
                         teacher_user_id AS TeacherUserId,
                         teacher_name AS TeacherName,
                         exam_request_id AS ExamRequestId,
                         exam_title AS ExamTitle,
                         decision_status AS DecisionStatus,
                         decision_label AS DecisionLabel,
                         decision_notes AS DecisionNotes,
                         created_at AS CreatedAt,
                         is_read AS IsRead
                  FROM public.admin_teacher_decision_notifications
                  WHERE admin_user_id = @p0
                  ORDER BY created_at DESC
                  LIMIT @p1",
                adminUserId,
                safeLimit);

            return items.ToList();
        }

        private void EnsureAdminNotificationsTable(Maeen1_NewDbContext db)
        {
            db.Database.ExecuteSqlCommand(
                @"CREATE TABLE IF NOT EXISTS public.admin_teacher_decision_notifications (
                    id SERIAL PRIMARY KEY,
                    admin_user_id INTEGER NOT NULL,
                    teacher_user_id INTEGER NOT NULL,
                    teacher_name TEXT NOT NULL,
                    exam_request_id INTEGER NOT NULL,
                    exam_title TEXT NOT NULL,
                    decision_status VARCHAR(100) NOT NULL,
                    decision_label VARCHAR(100) NOT NULL,
                    decision_notes TEXT,
                    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
                    is_read BOOLEAN NOT NULL DEFAULT FALSE
                )");
        }
    }
}
