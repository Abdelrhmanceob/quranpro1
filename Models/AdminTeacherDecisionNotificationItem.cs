using System;

namespace Maeen1_New.Models
{
    public class AdminTeacherDecisionNotificationItem
    {
        public int Id { get; set; }
        public int AdminUserId { get; set; }
        public int TeacherUserId { get; set; }
        public string TeacherName { get; set; }
        public int ExamRequestId { get; set; }
        public string ExamTitle { get; set; }
        public string DecisionStatus { get; set; }
        public string DecisionLabel { get; set; }
        public string DecisionNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
