using System;
using System.Collections.Generic;
using System.Linq;
using Maeen1_New.Models;

namespace Maeen1_New.Services
{
    public class FallbackAssessmentRules
    {
        public AssessmentResult Build(WizardAnswers answers, List<TeacherAvailability> teachers)
        {
            var level = CalculateLevel(answers);
            var plan = BuildPlan(level, answers);
            var teacher = PickTeacher(teachers);

            return new AssessmentResult
            {
                Level = level,
                Plan = plan,
                SmartSchedule = BuildSmartSchedule(level, answers),
                SuggestedTeacherName = teacher,
                RecommendationSource = "FallbackRules"
            };
        }

        private static string CalculateLevel(WizardAnswers answers)
        {
            var score = 0;
            score += answers.DailyMemorizationHours * 2;
            score += answers.CompletedJuzCount;
            score += CalculateDifficultyScore(answers.CompletedJuzNames, true);
            score += CalculateDifficultyScore(answers.TargetJuzNames, false);

            var tajweed = (answers.TajweedLevel ?? string.Empty).Trim();
            if (tajweed.Equals("متقدم", StringComparison.OrdinalIgnoreCase))
            {
                score += 6;
            }
            else if (tajweed.Equals("متوسط", StringComparison.OrdinalIgnoreCase))
            {
                score += 3;
            }

            if (score >= 20)
            {
                return "متقدم";
            }

            if (score >= 10)
            {
                return "متوسط";
            }

            return "مبتدئ";
        }

        private static string BuildPlan(string level, WizardAnswers answers)
        {
            var targetDetails = string.IsNullOrWhiteSpace(answers.TargetJuzNames)
                ? "لم يتم تحديد الأجزاء بالاسم."
                : "الأجزاء المستهدفة: " + answers.TargetJuzNames + ".";

            if (level == "متقدم")
            {
                return "خطة متقدمة: حفظ يومي مكثف مع مراجعة دورية أسبوعية. الهدف " +
                       answers.TargetJuzCount + " جزء خلال " + answers.TargetDuration + ". " + targetDetails;
            }

            if (level == "متوسط")
            {
                return "خطة متوسطة: حفظ منتظم مع تركيز على الإتقان والتجويد. الهدف " +
                       answers.TargetJuzCount + " جزء خلال " + answers.TargetDuration + ". " + targetDetails;
            }

            return "خطة تأسيسية: حفظ تدريجي بكمية أقل يوميًا مع مراجعة ثابتة. الهدف " +
                   answers.TargetJuzCount + " جزء خلال " + answers.TargetDuration + ". " + targetDetails;
        }

        private static string PickTeacher(List<TeacherAvailability> teachers)
        {
            var availableTeacher = teachers
                .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.TeacherName));

            if (availableTeacher != null)
            {
                return availableTeacher.TeacherName;
            }

            return "سيتم ترشيح معلم قريبًا";
        }

        private static string BuildSmartSchedule(string level, WizardAnswers answers)
        {
            var dailyHours = Math.Max(1, answers.DailyMemorizationHours);
            var newMemorizationMinutes = Math.Min(90, dailyHours * 20);
            var revisionMinutes = Math.Min(90, dailyHours * 25);
            var tajweedMinutes = Math.Max(15, dailyHours * 10);

            if (level == "متقدم")
            {
                newMemorizationMinutes += 20;
                revisionMinutes += 10;
            }
            else if (level == "مبتدئ")
            {
                newMemorizationMinutes = Math.Max(20, newMemorizationMinutes - 15);
                revisionMinutes = Math.Max(25, revisionMinutes - 10);
            }

            return
                "السبت: حفظ جديد " + newMemorizationMinutes + " دقيقة + مراجعة " + revisionMinutes + " دقيقة.\n" +
                "الأحد: حفظ جديد " + newMemorizationMinutes + " دقيقة + تدريب تجويد " + tajweedMinutes + " دقيقة.\n" +
                "الاثنين: مراجعة مركزة " + (revisionMinutes + 15) + " دقيقة + تسميع ذاتي.\n" +
                "الثلاثاء: حفظ جديد " + newMemorizationMinutes + " دقيقة + مراجعة قصيرة 20 دقيقة.\n" +
                "الأربعاء: تثبيت المحفوظ + تصحيح أخطاء التلاوة (" + tajweedMinutes + " دقيقة).\n" +
                "الخميس: جلسة تسميع كاملة + مراجعة أسبوعية شاملة.\n" +
                "الجمعة: راحة نشطة (استماع وتكرار خفيف 20-30 دقيقة) + تقييم تقدم الأسبوع.\n" +
                "تركيز الأجزاء: " + (string.IsNullOrWhiteSpace(answers.TargetJuzNames) ? "حسب خطة المعلم." : answers.TargetJuzNames);
        }

        private static int CalculateDifficultyScore(string juzNamesRaw, bool completed)
        {
            if (string.IsNullOrWhiteSpace(juzNamesRaw))
            {
                return 0;
            }

            var hardKeywords = new[]
            {
                "البقرة", "آل عمران", "النساء", "المائدة", "الأنعام", "الأعراف",
                "يونس", "هود", "يوسف", "النحل", "الإسراء", "الكهف", "مريم", "طه"
            };

            var mediumKeywords = new[]
            {
                "الفرقان", "النور", "العنكبوت", "الروم", "لقمان", "السجدة",
                "يس", "الصافات", "الزمر", "غافر", "فصلت", "الشورى", "الزخرف"
            };

            var entries = juzNamesRaw
                .Split(new[] { ',', '،', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            var score = 0;
            foreach (var entry in entries)
            {
                if (hardKeywords.Any(k => entry.Contains(k)))
                {
                    score += completed ? 3 : 1;
                    continue;
                }

                if (mediumKeywords.Any(k => entry.Contains(k)))
                {
                    score += completed ? 2 : 1;
                    continue;
                }

                score += completed ? 1 : 0;
            }

            return score;
        }
    }
}
