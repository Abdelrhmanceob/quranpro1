using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Maeen1_New.Services
{
    /// <summary>
    /// Generates structured Quran test plans for students based on their level and progress
    /// </summary>
    public class QuranTestPlanGenerator
    {
        // Quran Surahs with their Ayah counts
        private static readonly Dictionary<string, int> QuranSurahs = new Dictionary<string, int>
        {
            { "الفاتحة", 7 },
            { "البقرة", 286 },
            { "آل عمران", 200 },
            { "النساء", 176 },
            { "المائدة", 120 },
            { "الأنعام", 165 },
            { "الأعراف", 206 },
            { "الأنفال", 75 },
            { "التوبة", 129 },
            { "يونس", 109 },
            { "هود", 123 },
            { "يوسف", 111 },
            { "الرعد", 43 },
            { "إبراهيم", 52 },
            { "الحجر", 99 },
            { "النحل", 128 },
            { "الإسراء", 111 },
            { "الكهف", 110 },
            { "مريم", 98 },
            { "طه", 135 },
            { "الأنبياء", 112 },
            { "الحج", 78 },
            { "المؤمنون", 118 },
            { "النور", 64 },
            { "الفرقان", 77 },
            { "الشعراء", 227 },
            { "النمل", 93 },
            { "القصص", 88 },
            { "العنكبوت", 69 },
            { "الروم", 60 },
            { "لقمان", 34 },
            { "السجدة", 30 },
            { "الأحزاب", 73 },
            { "سبأ", 54 },
            { "فاطر", 45 },
            { "يس", 83 },
            { "الصافات", 182 },
            { "ص", 88 },
            { "الزمر", 75 },
            { "غافر", 85 },
            { "فصلت", 54 },
            { "الشورى", 53 },
            { "الزخرف", 89 },
            { "الدخان", 59 },
            { "الجاثية", 37 },
            { "الأحقاف", 35 },
            { "محمد", 38 },
            { "الفتح", 29 },
            { "الحجرات", 18 },
            { "ق", 45 },
            { "الذاريات", 60 },
            { "الطور", 49 },
            { "النجم", 62 },
            { "القمر", 55 },
            { "الرحمن", 78 },
            { "الواقعة", 96 },
            { "الحديد", 29 },
            { "المجادلة", 22 },
            { "الحشر", 24 },
            { "الممتحنة", 13 },
            { "الصف", 14 },
            { "الجمعة", 11 },
            { "المنافقون", 11 },
            { "التغابن", 18 },
            { "الطلاق", 12 },
            { "التحريم", 12 },
            { "الملك", 30 },
            { "القلم", 52 },
            { "الحاقة", 52 },
            { "المعارج", 44 },
            { "نوح", 28 },
            { "الجن", 28 },
            { "المزمل", 20 },
            { "المدثر", 56 },
            { "القيامة", 40 },
            { "الإنسان", 31 },
            { "المرسلات", 50 },
            { "النبأ", 40 },
            { "النازعات", 46 },
            { "عبس", 42 },
            { "التكوير", 29 },
            { "الإنفطار", 19 },
            { "المطففين", 36 },
            { "الانشقاق", 25 },
            { "البروج", 22 },
            { "الطارق", 17 },
            { "الأعلى", 19 },
            { "الغاشية", 26 },
            { "الفجر", 30 },
            { "البلد", 20 },
            { "الشمس", 15 },
            { "القمر", 55 },
            { "الليل", 21 },
            { "الضحى", 11 },
            { "الشرح", 8 },
            { "التين", 8 },
            { "العلق", 19 },
            { "القدر", 5 },
            { "البينة", 8 },
            { "الزلزلة", 8 },
            { "العاديات", 11 },
            { "القارعة", 11 },
            { "التكاثر", 8 },
            { "الأسف", 11 },
            { "الهمزة", 9 },
            { "الفيل", 5 },
            { "قريش", 4 },
            { "الماعون", 7 },
            { "الكوثر", 3 },
            { "الكافرون", 6 },
            { "النصر", 3 },
            { "المسد", 5 },
            { "الإخلاص", 4 },
            { "الفلق", 5 },
            { "الناس", 6 }
        };

        // Difficulty levels
        private enum DifficultyLevel
        {
            Beginner,      // 1-5 Ayahs, simple Surahs
            Elementary,    // 5-10 Ayahs, medium Surahs
            Intermediate,  // 10-20 Ayahs, longer Surahs
            Advanced,      // 20+ Ayahs, complex Surahs
            Expert         // Full Surah or multiple Surahs
        }

        /// <summary>
        /// Generates a Quran test plan based on student level
        /// </summary>
        public JObject GenerateTestPlan(string studentLevel = "Beginner", int? studentId = null)
        {
            var difficulty = ParseDifficulty(studentLevel);
            var surah = SelectAppropriatesurah(difficulty);
            var ayahRange = GenerateAyahRange(surah, difficulty);
            var estimatedTime = CalculateEstimatedTime(ayahRange, difficulty);
            var passingScore = CalculatePassingScore(difficulty);

            var testPlan = new JObject
            {
                { "title", $"اختبار سورة {surah} - مستوى {GetDifficultyLabel(difficulty)}" },
                { "surah", surah },
                { "ayah_range", ayahRange },
                { "difficulty", GetDifficultyLabel(difficulty) },
                { "estimated_time", $"{estimatedTime} دقيقة" },
                { "passing_score", $"{passingScore}%" },
                { "tasks", GenerateTasks(surah, ayahRange, difficulty) },
                { "tajweed_focus", GenerateTajweedFocus(surah, difficulty) },
                { "teacher_notes", GenerateTeacherNotes(surah, difficulty) },
                { "student_instructions", GenerateStudentInstructions(surah, difficulty) },
                { "created_at", DateTime.UtcNow.ToString("O") },
                { "student_id", studentId ?? 0 }
            };

            return testPlan;
        }

        /// <summary>
        /// Selects an appropriate Surah based on difficulty level
        /// </summary>
        private string SelectAppropriatesurah(DifficultyLevel difficulty)
        {
            var random = new Random();
            List<string> surahs = new List<string>();

            switch (difficulty)
            {
                case DifficultyLevel.Beginner:
                    // Short Surahs (1-30 Ayahs)
                    surahs = QuranSurahs.Where(s => s.Value <= 30).Select(s => s.Key).ToList();
                    break;

                case DifficultyLevel.Elementary:
                    // Medium Surahs (30-60 Ayahs)
                    surahs = QuranSurahs.Where(s => s.Value > 30 && s.Value <= 60).Select(s => s.Key).ToList();
                    break;

                case DifficultyLevel.Intermediate:
                    // Longer Surahs (60-120 Ayahs)
                    surahs = QuranSurahs.Where(s => s.Value > 60 && s.Value <= 120).Select(s => s.Key).ToList();
                    break;

                case DifficultyLevel.Advanced:
                    // Very Long Surahs (120+ Ayahs)
                    surahs = QuranSurahs.Where(s => s.Value > 120).Select(s => s.Key).ToList();
                    break;

                case DifficultyLevel.Expert:
                    // All Surahs
                    surahs = QuranSurahs.Keys.ToList();
                    break;
            }

            return surahs.Count > 0 ? surahs[random.Next(surahs.Count)] : "الفاتحة";
        }

        /// <summary>
        /// Generates Ayah range based on difficulty
        /// </summary>
        private string GenerateAyahRange(string surah, DifficultyLevel difficulty)
        {
            if (!QuranSurahs.ContainsKey(surah))
                return "1-7";

            int totalAyahs = QuranSurahs[surah];
            int startAyah = 1;
            int endAyah = totalAyahs;

            switch (difficulty)
            {
                case DifficultyLevel.Beginner:
                    endAyah = Math.Min(5, totalAyahs);
                    break;

                case DifficultyLevel.Elementary:
                    endAyah = Math.Min(10, totalAyahs);
                    break;

                case DifficultyLevel.Intermediate:
                    endAyah = Math.Min(20, totalAyahs);
                    break;

                case DifficultyLevel.Advanced:
                    endAyah = Math.Min(totalAyahs, (int)(totalAyahs * 0.75));
                    break;

                case DifficultyLevel.Expert:
                    endAyah = totalAyahs;
                    break;
            }

            return $"{startAyah}-{endAyah}";
        }

        /// <summary>
        /// Calculates estimated time for the exam
        /// </summary>
        private int CalculateEstimatedTime(string ayahRange, DifficultyLevel difficulty)
        {
            var parts = ayahRange.Split('-');
            if (parts.Length != 2 || !int.TryParse(parts[0], out int start) || !int.TryParse(parts[1], out int end))
                return 15;

            int ayahCount = end - start + 1;
            int baseTime = ayahCount * 2; // 2 minutes per Ayah

            switch (difficulty)
            {
                case DifficultyLevel.Beginner:
                    return baseTime + 5;
                case DifficultyLevel.Elementary:
                    return baseTime + 10;
                case DifficultyLevel.Intermediate:
                    return baseTime + 15;
                case DifficultyLevel.Advanced:
                    return baseTime + 20;
                case DifficultyLevel.Expert:
                    return baseTime + 30;
                default:
                    return baseTime;
            }
        }

        /// <summary>
        /// Calculates passing score based on difficulty
        /// </summary>
        private int CalculatePassingScore(DifficultyLevel difficulty)
        {
            switch (difficulty)
            {
                case DifficultyLevel.Beginner:
                    return 80;
                case DifficultyLevel.Elementary:
                    return 75;
                case DifficultyLevel.Intermediate:
                    return 70;
                case DifficultyLevel.Advanced:
                    return 65;
                case DifficultyLevel.Expert:
                    return 60;
                default:
                    return 70;
            }
        }

        /// <summary>
        /// Generates evaluation tasks
        /// </summary>
        private JArray GenerateTasks(string surah, string ayahRange, DifficultyLevel difficulty)
        {
            var tasks = new JArray
            {
                new JObject
                {
                    { "task_id", 1 },
                    { "type", "memorization" },
                    { "description", $"تلاوة سورة {surah} من الآية {ayahRange} بدقة وتركيز" },
                    { "weight", 40 },
                    { "criteria", new JArray
                        {
                            "عدم الأخطاء في النطق",
                            "الالتزام بالحروف والحركات",
                            "عدم الإضافة أو الحذف",
                            "الثقة والطلاقة في التلاوة"
                        }
                    }
                },
                new JObject
                {
                    { "task_id", 2 },
                    { "type", "tajweed" },
                    { "description", "تطبيق أحكام التجويد بشكل صحيح" },
                    { "weight", 30 },
                    { "criteria", new JArray
                        {
                            "تطبيق أحكام النون والتنوين",
                            "تطبيق أحكام الميم الساكنة",
                            "تطبيق أحكام اللام الساكنة",
                            "الوقف والابتداء الصحيح"
                        }
                    }
                },
                new JObject
                {
                    { "task_id", 3 },
                    { "type", "fluency" },
                    { "description", "التلاوة بسلاسة وانسيابية دون توقف" },
                    { "weight", 20 },
                    { "criteria", new JArray
                        {
                            "عدم التوقف المفاجئ",
                            "التدفق الطبيعي للآيات",
                            "الربط الصحيح بين الآيات",
                            "الحفاظ على الإيقاع"
                        }
                    }
                },
                new JObject
                {
                    { "task_id", 4 },
                    { "type", "continuation" },
                    { "description", "الاستمرار من نقطة عشوائية في النص" },
                    { "weight", 10 },
                    { "criteria", new JArray
                        {
                            "القدرة على الاستمرار بسلاسة",
                            "عدم الخلط مع آيات أخرى",
                            "الحفاظ على الدقة"
                        }
                    }
                }
            };

            return tasks;
        }

        /// <summary>
        /// Generates Tajweed focus points
        /// </summary>
        private JArray GenerateTajweedFocus(string surah, DifficultyLevel difficulty)
        {
            var tajweedPoints = new JArray
            {
                "أحكام النون الساكنة والتنوين",
                "أحكام الميم الساكنة",
                "أحكام اللام الساكنة",
                "الإدغام والإظهار",
                "الإمالة والفتح",
                "الوقف والابتداء",
                "المد والقصر",
                "الهمزة والتسهيل"
            };

            // Shuffle and return based on difficulty
            var random = new Random();
            var shuffled = tajweedPoints.OrderBy(x => random.Next()).ToList();
            var count = difficulty == DifficultyLevel.Beginner ? 3 : 
                       difficulty == DifficultyLevel.Elementary ? 4 :
                       difficulty == DifficultyLevel.Intermediate ? 5 :
                       difficulty == DifficultyLevel.Advanced ? 6 : 8;

            return new JArray(shuffled.Take(count));
        }

        /// <summary>
        /// Generates teacher notes and guidance
        /// </summary>
        private string GenerateTeacherNotes(string surah, DifficultyLevel difficulty)
        {
            var notes = $@"ملاحظات المعلم لسورة {surah}:

1. التركيز على النقاط الصعبة:
   - تحديد الآيات التي قد تسبب صعوبة للطالب
   - التركيز على أحكام التجويد المهمة

2. معايير التقييم:
   - الدقة في النطق: 40%
   - تطبيق التجويد: 30%
   - الطلاقة والسلاسة: 20%
   - الاستمرار من نقطة عشوائية: 10%

3. ملاحظات خاصة:
   - تصحيح الأخطاء فوراً مع التوضيح
   - تشجيع الطالب على المحاولة مرة أخرى
   - تقديم ملاحظات بناءة وإيجابية

4. المتابعة:
   - تسجيل نقاط الضعف للمراجعة
   - تحديد الأهداف للجلسة القادمة
   - إرسال ملاحظات للطالب وولي الأمر";

            return notes;
        }

        /// <summary>
        /// Generates student instructions
        /// </summary>
        private string GenerateStudentInstructions(string surah, DifficultyLevel difficulty)
        {
            var instructions = $@"تعليمات الطالب:

1. التحضير:
   - اقرأ الآيات المطلوبة من سورة {surah} عدة مرات قبل الاختبار
   - تأكد من فهمك لمعاني الآيات
   - استعد نفسياً للتلاوة بثقة

2. أثناء الاختبار:
   - ابدأ بالبسملة
   - اقرأ بتركيز وهدوء
   - طبق أحكام التجويد بشكل صحيح
   - لا تتسرع في التلاوة

3. نقاط مهمة:
   - الدقة أهم من السرعة
   - الالتزام بالحروف والحركات
   - عدم الإضافة أو الحذف
   - الثقة في نفسك

4. بعد الاختبار:
   - استمع لملاحظات المعلم بانتباه
   - اسأل عن أي نقاط غير واضحة
   - التزم بالتحسن المستمر";

            return instructions;
        }

        /// <summary>
        /// Parses difficulty level from string
        /// </summary>
        private DifficultyLevel ParseDifficulty(string level)
        {
            return level?.ToLower() switch
            {
                "beginner" or "مبتدئ" => DifficultyLevel.Beginner,
                "elementary" or "ابتدائي" => DifficultyLevel.Elementary,
                "intermediate" or "متوسط" => DifficultyLevel.Intermediate,
                "advanced" or "متقدم" => DifficultyLevel.Advanced,
                "expert" or "خبير" => DifficultyLevel.Expert,
                _ => DifficultyLevel.Beginner
            };
        }

        /// <summary>
        /// Gets difficulty label in Arabic
        /// </summary>
        private string GetDifficultyLabel(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Beginner => "مبتدئ",
                DifficultyLevel.Elementary => "ابتدائي",
                DifficultyLevel.Intermediate => "متوسط",
                DifficultyLevel.Advanced => "متقدم",
                DifficultyLevel.Expert => "خبير",
                _ => "مبتدئ"
            };
        }
    }
}
