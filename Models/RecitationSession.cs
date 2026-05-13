using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maeen1_New.Models
{
    [Table("recitation_sessions")]
    public class RecitationSession
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("recitation_request_id")]
        public int RecitationRequestId { get; set; }

        [Column("student_id")]
        public int StudentId { get; set; }

        [Column("teacher_id")]
        public int TeacherId { get; set; }

        [Column("google_meet_url")]
        public string GoogleMeetUrl { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("RecitationRequestId")]
        public virtual RecitationRequest Request { get; set; }

        [ForeignKey("StudentId")]
        public virtual User Student { get; set; }

        [ForeignKey("TeacherId")]
        public virtual User Teacher { get; set; }
    }
}