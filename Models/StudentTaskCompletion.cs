using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maeen1_New.Models
{
    [Table("student_task_completions")]
    public class StudentTaskCompletion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("task_id")]
        public int TaskId { get; set; }

        [Column("student_id")]
        public int StudentId { get; set; }

        [Column("completed_at")]
        public DateTime CompletedAt { get; set; }

        [Column("notes")]
        public string Notes { get; set; }

        [Column("is_approved")]
        public bool IsApproved { get; set; }

        [Column("approved_by_teacher_id")]
        public int? ApprovedByTeacherId { get; set; }

        [Column("approved_at")]
        public DateTime? ApprovedAt { get; set; }

        [ForeignKey("TaskId")]
        public virtual ExamTask Task { get; set; }

        [ForeignKey("StudentId")]
        public virtual User Student { get; set; }
    }
}
