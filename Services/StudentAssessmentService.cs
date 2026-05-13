using System.Collections.Generic;
using System.Linq;
using Maeen1_New.Models;

namespace Maeen1_New.Services
{
    public class StudentAssessmentService
    {
        private readonly Maeen1_NewDbContext _db;
        private readonly AiAssessmentClient _aiClient;
        private readonly FallbackAssessmentRules _fallbackRules;

        public StudentAssessmentService(Maeen1_NewDbContext db)
        {
            _db = db;
            _aiClient = new AiAssessmentClient();
            _fallbackRules = new FallbackAssessmentRules();
        }

        public AssessmentResult Assess(WizardAnswers answers)
        {
            var aiResult = _aiClient.TryAssess(answers);
            if (aiResult != null)
            {
                if (string.IsNullOrWhiteSpace(aiResult.SuggestedTeacherName))
                {
                    aiResult.SuggestedTeacherName = GetDefaultTeacherName();
                }

                return aiResult;
            }

            var teachers = _db.TeacherAvailabilities.ToList();
            return _fallbackRules.Build(answers, teachers);
        }

        private string GetDefaultTeacherName()
        {
            var teacher = _db.TeacherAvailabilities
                .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.TeacherName));

            return teacher != null ? teacher.TeacherName : "سيتم ترشيح معلم قريبًا";
        }
    }

    public class WizardAnswers
    {
        public int DailyMemorizationHours { get; set; }
        public int TargetJuzCount { get; set; }
        public string TargetJuzNames { get; set; }
        public int CompletedJuzCount { get; set; }
        public string CompletedJuzNames { get; set; }
        public string TajweedLevel { get; set; }
        public string TargetDuration { get; set; }
    }

    public class AssessmentResult
    {
        public string Level { get; set; }
        public string Plan { get; set; }
        public string SmartSchedule { get; set; }
        public string SuggestedTeacherName { get; set; }
        public string RecommendationSource { get; set; }
    }
}
