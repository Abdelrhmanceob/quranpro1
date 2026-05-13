using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maeen1_New.Models
{
    [Table("exam_requests")]
    public class ExamRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("student_id")]
        public int StudentId { get; set; }

        [Column("admin_id")]
        public int AdminId { get; set; }

        [Column("teacher_id")]
        public int? TeacherId { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("plan_content")]
        public string PlanContent { get; set; }

        [Column("plan_objectives")]
        public string PlanObjectives { get; set; }

        [Column("plan_duration_minutes")]
        public int? PlanDurationMinutes { get; set; }

        [Column("status")]
        public string Status { get; set; } // Pending, AssignedToTeacher, ResultSent, Completed

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("assigned_at")]
        public DateTime? AssignedAt { get; set; }

        [Column("result_sent_at")]
        public DateTime? ResultSentAt { get; set; }

        [ForeignKey("StudentId")]
        public virtual User Student { get; set; }

        [ForeignKey("AdminId")]
        public virtual User Admin { get; set; }

        [ForeignKey("TeacherId")]
        public virtual User Teacher { get; set; }

        public virtual ICollection<ExamResult> Results { get; set; }
    }
}
