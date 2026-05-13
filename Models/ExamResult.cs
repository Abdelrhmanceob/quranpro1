using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maeen1_New.Models
{
    [Table("exam_results")]
    public class ExamResult
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("exam_request_id")]
        public int ExamRequestId { get; set; }

        [Column("teacher_id")]
        public int TeacherId { get; set; }

        [Column("student_id")]
        public int StudentId { get; set; }

        [Column("score")]
        public decimal? Score { get; set; }

        [Column("max_score")]
        public decimal? MaxScore { get; set; }

        [Column("grade")]
        public string Grade { get; set; }

        [Column("teacher_notes")]
        public string TeacherNotes { get; set; }

        [Column("strengths")]
        public string Strengths { get; set; }

        [Column("weaknesses")]
        public string Weaknesses { get; set; }

        [Column("recommendations")]
        public string Recommendations { get; set; }

        [Column("sent_at")]
        public DateTime SentAt { get; set; }

        [ForeignKey("ExamRequestId")]
        public virtual ExamRequest ExamRequest { get; set; }

        [ForeignKey("TeacherId")]
        public virtual User Teacher { get; set; }

        [ForeignKey("StudentId")]
        public virtual User Student { get; set; }
    }
}
