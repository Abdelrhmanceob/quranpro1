using System;

namespace Maeen1_New.Models
{
    public class TeacherNotificationItem
    {
        public int Id { get; set; }
        public int TeacherUserId { get; set; }
        public int StudentUserId { get; set; }
        public string StudentName { get; set; }
        public string AssessmentLevel { get; set; }
        public string AssessmentSummary { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
