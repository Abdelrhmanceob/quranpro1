using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maeen1_New.Models
{
    [Table("exam_access_logs")]
    public class ExamAccessLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("exam_id")]
        public int ExamId { get; set; }

        [Column("student_id")]
        public int StudentId { get; set; }

        [Column("accessed_at")]
        public DateTime AccessedAt { get; set; }

        [Column("ip_address")]
        public string IpAddress { get; set; }

        [ForeignKey("ExamId")]
        public virtual Exam Exam { get; set; }

        [ForeignKey("StudentId")]
        public virtual User Student { get; set; }
    }
}
