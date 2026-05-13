using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Maeen1_New.Models.ViewModels
{
    public class StudentOnboardingWizardViewModel : IValidatableObject
    {
        [Required]
        public int UserId { get; set; }

        [Required(ErrorMessage = "يرجى إدخال عدد ساعات الحفظ اليومية")]
        [Range(1, 12, ErrorMessage = "عدد الساعات يجب أن يكون بين 1 و 12")]
        [Display(Name = "عدد ساعات الحفظ اليومية")]
        public int DailyMemorizationHours { get; set; }

        [Required(ErrorMessage = "يرجى إدخال عدد الأجزاء المستهدفة")]
        [Range(1, 30, ErrorMessage = "عدد الأجزاء يجب أن يكون بين 1 و 30")]
        [Display(Name = "عدد الأجزاء المستهدفة")]
        public int TargetJuzCount { get; set; }

        [Required(ErrorMessage = "يرجى إدخال أسماء الأجزاء المستهدفة")]
        [StringLength(500)]
        [Display(Name = "أسماء الأجزاء المستهدفة")]
        public string TargetJuzNames { get; set; }

        [Required(ErrorMessage = "يرجى إدخال عدد الأجزاء المحفوظة مسبقًا")]
        [Range(0, 30, ErrorMessage = "عدد الأجزاء المحفوظة يجب أن يكون بين 0 و 30")]
        [Display(Name = "الأجزاء المحفوظة مسبقًا")]
        public int CompletedJuzCount { get; set; }

        [StringLength(500)]
        [Display(Name = "أسماء الأجزاء المحفوظة مسبقًا")]
        public string CompletedJuzNames { get; set; }

        [Required(ErrorMessage = "يرجى اختيار مستوى التجويد")]
        [Display(Name = "مستوى التجويد")]
        public string TajweedLevel { get; set; }

        [Required(ErrorMessage = "يرجى إدخال المدة الزمنية المستهدفة")]
        [StringLength(100)]
        [Display(Name = "المدة الزمنية المستهدفة")]
        public string TargetDuration { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(TargetJuzNames))
            {
                yield return new ValidationResult(
                    "يرجى كتابة أسماء الأجزاء المستهدفة.",
                    new[] { nameof(TargetJuzNames) });
            }

            if (CompletedJuzCount > 0 && string.IsNullOrWhiteSpace(CompletedJuzNames))
            {
                yield return new ValidationResult(
                    "يرجى كتابة أسماء الأجزاء المحفوظة مسبقًا.",
                    new[] { nameof(CompletedJuzNames) });
            }
        }
    }
}
