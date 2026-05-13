using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maeen1_New.Models
{
    [Table("teacher_availabilities")]
    public class TeacherAvailability
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("teacher_name")]
        public string TeacherName { get; set; }

        [Column("date")]
        public string Date { get; set; }

        [Column("time")]
        public string Time { get; set; }
    }
}
