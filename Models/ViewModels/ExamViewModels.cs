using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Maeen1_New.Models.ViewModels
{
    public class CreateExamViewModel
    {
        [Required(ErrorMessage = "عنوان الاختبار مطلوب")]
        public string Title { get; set; }

        public string Description { get; set; }

        public int TeacherId { get; set; }
    }

    public class AssignTaskViewModel
    {
        public int ExamId { get; set; }

        [Required(ErrorMessage = "اختر الطالب")]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "عنوان المهمة مطلوب")]
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime? DueDate { get; set; }

        public bool IsRequired { get; set; } = true;

        public int TeacherId { get; set; }
    }

    public class TeacherExamDashboardViewModel
    {
        public int TeacherId { get; set; }
        public List<ExamWithTasksViewModel> Exams { get; set; } = new List<ExamWithTasksViewModel>();
        public List<StudentTaskCompletionReviewViewModel> PendingApprovals { get; set; } = new List<StudentTaskCompletionReviewViewModel>();
        public List<UserBasicViewModel> Students { get; set; } = new List<UserBasicViewModel>();
    }

    public class ExamWithTasksViewModel
    {
        public int ExamId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public List<TaskViewModel> Tasks { get; set; } = new List<TaskViewModel>();
    }

    public class TaskViewModel
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string StudentName { get; set; }
        public int StudentId { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsRequired { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsApproved { get; set; }
    }

    public class StudentTaskCompletionReviewViewModel
    {
        public int CompletionId { get; set; }
        public string StudentName { get; set; }
        public string TaskTitle { get; set; }
        public string ExamTitle { get; set; }
        public string Notes { get; set; }
        public DateTime CompletedAt { get; set; }
    }

    public class UserBasicViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class StudentExamViewModel
    {
        public int StudentId { get; set; }
        public List<StudentExamItemViewModel> Exams { get; set; } = new List<StudentExamItemViewModel>();
    }

    public class StudentExamItemViewModel
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; }
        public string ExamDescription { get; set; }
        public List<StudentTaskItemViewModel> Tasks { get; set; } = new List<StudentTaskItemViewModel>();
        public bool CanAccessExam { get; set; }
        public bool HasAccessedExam { get; set; }
    }

    public class StudentTaskItemViewModel
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsRequired { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class AdminExamMonitorViewModel
    {
        public List<ExamAccessLogViewModel> AccessLogs { get; set; } = new List<ExamAccessLogViewModel>();
        public List<ExamSummaryViewModel> ExamSummaries { get; set; } = new List<ExamSummaryViewModel>();
    }

    public class ExamAccessLogViewModel
    {
        public string StudentName { get; set; }
        public string ExamTitle { get; set; }
        public DateTime AccessedAt { get; set; }
        public string IpAddress { get; set; }
    }

    public class ExamSummaryViewModel
    {
        public string ExamTitle { get; set; }
        public string TeacherName { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int StudentsWithAccess { get; set; }
    }

    // ===== Admin Exam Panel ViewModels =====

    public class AdminExamPanelViewModel
    {
        public int AdminId { get; set; }
        public List<ExamRequestItemViewModel> ExamRequests { get; set; } = new List<ExamRequestItemViewModel>();
        public List<UserBasicViewModel> Students { get; set; } = new List<UserBasicViewModel>();
        public List<UserBasicViewModel> Teachers { get; set; } = new List<UserBasicViewModel>();
        public List<StudentSubmissionItemViewModel> StudentSubmissions { get; set; } = new List<StudentSubmissionItemViewModel>();
    }

    public class StudentSubmissionItemViewModel
    {
        public int CompletionId { get; set; }
        public string StudentName { get; set; }
        public string ExamTitle { get; set; }
        public string TaskTitle { get; set; }
        public string Notes { get; set; }
        public DateTime CompletedAt { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }

    public class ExamRequestItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string PlanContent { get; set; }
        public string PlanObjectives { get; set; }
        public int? PlanDurationMinutes { get; set; }
        public string StudentName { get; set; }
        public int StudentId { get; set; }
        public string TeacherName { get; set; }
        public int? TeacherId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? ResultSentAt { get; set; }
        public bool HasResult { get; set; }

        // Student onboarding answers snapshot
        public string StudentAnswerDailyMemorizationHours { get; set; }
        public string StudentAnswerTargetJuzCount { get; set; }
        public string StudentAnswerTargetJuzNames { get; set; }
        public string StudentAnswerCompletedJuzCount { get; set; }
        public string StudentAnswerCompletedJuzNames { get; set; }
        public string StudentAnswerTajweedLevel { get; set; }
        public string StudentAnswerTargetDuration { get; set; }
    }

    public class CreateExamRequestViewModel
    {
        [Required(ErrorMessage = "عنوان الاختبار مطلوب")]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "اختر الطالب")]
        public int StudentId { get; set; }

        public int? TeacherId { get; set; }

        [Required(ErrorMessage = "محتوى الخطة مطلوب")]
        public string PlanContent { get; set; }

        public string PlanObjectives { get; set; }

        public int? PlanDurationMinutes { get; set; }

        public int AdminId { get; set; }
    }

    public class AssignTeacherToRequestViewModel
    {
        public int RequestId { get; set; }
        public int TeacherId { get; set; }
        public int AdminId { get; set; }
    }

    // ===== Teacher Exam Result ViewModels =====

    public class TeacherExamRequestsViewModel
    {
        public int TeacherId { get; set; }
        public List<TeacherExamRequestItemViewModel> AssignedRequests { get; set; } = new List<TeacherExamRequestItemViewModel>();
        public List<TeacherExamRequestItemViewModel> CompletedRequests { get; set; } = new List<TeacherExamRequestItemViewModel>();
    }

    public class TeacherExamRequestItemViewModel
    {
        public int RequestId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string StudentName { get; set; }
        public int StudentId { get; set; }
        public string PlanContent { get; set; }
        public string PlanObjectives { get; set; }
        public int? PlanDurationMinutes { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AssignedAt { get; set; }
        // Result info (if sent)
        public decimal? Score { get; set; }
        public decimal? MaxScore { get; set; }
        public string Grade { get; set; }
        public string TeacherNotes { get; set; }
        public string Recommendations { get; set; }
        public DateTime? ResultSentAt { get; set; }
        public string TeacherDecisionStatus { get; set; }
        public string TeacherDecisionLabel { get; set; }

        // Student onboarding answers snapshot (to help teacher build memorization plan)
        public int? DailyMemorizationHours { get; set; }
        public int? TargetJuzCount { get; set; }
        public int? CompletedJuzCount { get; set; }
        public string TargetJuzNames { get; set; }
        public string CompletedJuzNames { get; set; }
        public string TajweedLevel { get; set; }
        public string TargetDuration { get; set; }
        public string DeterminedLevel { get; set; }
    }

    public class SendExamResultViewModel
    {
        public int RequestId { get; set; }
        public int TeacherId { get; set; }
        public int StudentId { get; set; }
        public string TeacherDecisionStatus { get; set; }

        public decimal? Score { get; set; }
        public decimal? MaxScore { get; set; }

        [Required(ErrorMessage = "التقدير مطلوب")]
        public string Grade { get; set; }

        public string TeacherNotes { get; set; }
        public string Strengths { get; set; }
        public string Weaknesses { get; set; }
        [Required(ErrorMessage = "خطة الحفظ للطالب مطلوبة")]
        public string Recommendations { get; set; }
    }

    public class SendMemorizationPlanViewModel
    {
        public int RequestId { get; set; }
        public int TeacherId { get; set; }
        public int StudentId { get; set; }

        [Required(ErrorMessage = "خطة الحفظ للطالب مطلوبة")]
        public string Recommendations { get; set; }

        public string TeacherNotes { get; set; }
    }

    public class TeacherPlanForStudentViewModel
    {
        public int RequestId { get; set; }
        public string ExamTitle { get; set; }
        public string TeacherName { get; set; }
        public string Grade { get; set; }
        public string TeacherNotes { get; set; }
        public string MemorizationPlan { get; set; }
        public DateTime SentAt { get; set; }
    }

    // ── Admin Dashboard ──────────────────────────────────────────────────────
    public class AdminDashboardViewModel
    {
        public string AdminName { get; set; }
        public int AdminId { get; set; }

        // Counters
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalExamRequests { get; set; }
        public int PendingRequests { get; set; }
        public int AssignedRequests { get; set; }
        public int CompletedRequests { get; set; }

        // Recent exam requests (latest 20)
        public List<AdminDashboardRequestRow> RecentRequests { get; set; }
        public List<AdminTeacherDecisionNotificationViewModel> TeacherDecisionNotifications { get; set; } = new List<AdminTeacherDecisionNotificationViewModel>();
    }

    public class AdminDashboardRequestRow
    {
        public int RequestId { get; set; }
        public string Title { get; set; }
        public string StudentName { get; set; }
        public string TeacherName { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Grade { get; set; }
    }

    public class AdminTeacherDecisionNotificationViewModel
    {
        public int NotificationId { get; set; }
        public int ExamRequestId { get; set; }
        public string ExamTitle { get; set; }
        public string TeacherName { get; set; }
        public string DecisionStatus { get; set; }
        public string DecisionLabel { get; set; }
        public string DecisionNotes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ── Teacher Dashboard ──────────────────────────────────────────────────────
    public class TeacherDashboardViewModel
    {
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public int TotalStudents { get; set; }
        public int PendingExamRequests { get; set; }
        public int CompletedExamRequests { get; set; }
        public int TotalExams { get; set; }
        public int PendingApprovals { get; set; }
        public int PendingRecitationRequests { get; set; }
        public int ReadyRecitationSessions { get; set; }
        public List<TeacherDashboardStudentRow> Students { get; set; } = new List<TeacherDashboardStudentRow>();
        public List<TeacherNotificationItem> Notifications { get; set; } = new List<TeacherNotificationItem>();
    }

    public class TeacherDashboardStudentRow
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string LatestExamTitle { get; set; }
        public string Status { get; set; }
        public int CompletedTasks { get; set; }
        public int TotalTasks { get; set; }
    }

    public class TeacherLinkedStudentSessionViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public int TotalRecitationRequests { get; set; }
        public int ReadySessions { get; set; }
    }
}
