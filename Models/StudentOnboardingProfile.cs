using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maeen1_New.Models
{
    [Table("student_onboarding_profiles")]
    public class StudentOnboardingProfile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("daily_memorization_hours")]
        public int DailyMemorizationHours { get; set; }

        [Column("target_juz_count")]
        public int TargetJuzCount { get; set; }

        [Column("completed_juz_count")]
        public int CompletedJuzCount { get; set; }

        [Column("tajweed_level")]
        public string TajweedLevel { get; set; }

        [Column("target_duration")]
        public string TargetDuration { get; set; }

        [Column("determined_level")]
        public string DeterminedLevel { get; set; }

        [Column("memorization_plan")]
        public string MemorizationPlan { get; set; }

        [Column("suggested_teacher_name")]
        public string SuggestedTeacherName { get; set; }

        [Column("recommendation_source")]
        public string RecommendationSource { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
