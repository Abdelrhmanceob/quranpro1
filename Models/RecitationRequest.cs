using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maeen1_New.Models
{
    [Table("recitation_requests")]
    public class RecitationRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("student_id")]
        public int StudentId { get; set; }

        [Column("teacher_id")]
        public int TeacherId { get; set; }

        [Column("exam_id")]
        public int? ExamId { get; set; }

        [Column("notes")]
        public string Notes { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("StudentId")]
        public virtual User Student { get; set; }

        [ForeignKey("TeacherId")]
        public virtual User Teacher { get; set; }

        [ForeignKey("ExamId")]
        public virtual Exam Exam { get; set; }

        [NotMapped]
        public virtual RecitationSession Session { get; set; }
    }
}