using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maeen1_New.Models
{
    [Table("exam_tasks")]
    public class ExamTask
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("exam_id")]
        public int ExamId { get; set; }

        [Column("student_id")]
        public int StudentId { get; set; }

        [Required]
        [Column("title")]
        public string Title { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("assigned_at")]
        public DateTime AssignedAt { get; set; }

        [Column("due_date")]
        public DateTime? DueDate { get; set; }

        [Column("is_required")]
        public bool IsRequired { get; set; }

        [ForeignKey("ExamId")]
        public virtual Exam Exam { get; set; }

        [ForeignKey("StudentId")]
        public virtual User Student { get; set; }

        public virtual ICollection<StudentTaskCompletion> Completions { get; set; }
    }
}
