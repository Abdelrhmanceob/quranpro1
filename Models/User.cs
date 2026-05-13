using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maeen1_New.Models
{
    [Table("app_users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("role")]
        public string Role { get; set; }

        [Column("is_onboarding_completed")]
        public bool IsOnboardingCompleted { get; set; }

        [Column("onboarding_completed_at")]
        public DateTime? OnboardingCompletedAt { get; set; }

        [Column("student_level")]
        public string StudentLevel { get; set; }
    }
}