using System;
using System.Web.Mvc;
using Maeen1_New.Services;
using Newtonsoft.Json.Linq;

namespace Maeen1_New.Controllers
{
    /// <summary>
    /// API Controller for generating Quran test plans
    /// </summary>
    public class QuranTestPlanController : Controller
    {
        private readonly QuranTestPlanGenerator _testPlanGenerator = new QuranTestPlanGenerator();

        /// <summary>
        /// Generates a test plan for a student
        /// GET: /QuranTestPlan/Generate?level=Beginner&studentId=1
        /// </summary>
        [HttpGet]
        public ActionResult Generate(string level = "Beginner", int? studentId = null)
        {
            try
            {
                var testPlan = _testPlanGenerator.GenerateTestPlan(level, studentId);
                return Json(testPlan, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    error = "خطأ في إنشاء خطة الاختبار",
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Generates multiple test plans
        /// GET: /QuranTestPlan/GenerateMultiple?count=5&level=Intermediate
        /// </summary>
        [HttpGet]
        public ActionResult GenerateMultiple(int count = 5, string level = "Beginner")
        {
            try
            {
                var testPlans = new JArray();
                for (int i = 0; i < count; i++)
                {
                    var testPlan = _testPlanGenerator.GenerateTestPlan(level);
                    testPlans.Add(testPlan);
                }

                return Json(new
                {
                    success = true,
                    count = count,
                    level = level,
                    test_plans = testPlans
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    error = "خطأ في إنشاء خطط الاختبار",
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Gets available difficulty levels
        /// GET: /QuranTestPlan/GetLevels
        /// </summary>
        [HttpGet]
        public ActionResult GetLevels()
        {
            var levels = new JArray
            {
                new JObject
                {
                    { "level", "Beginner" },
                    { "label", "مبتدئ" },
                    { "description", "للطلاب الجدد - 1-5 آيات من السور القصيرة" }
                },
                new JObject
                {
                    { "level", "Elementary" },
                    { "label", "ابتدائي" },
                    { "description", "للطلاب المبتدئين - 5-10 آيات من السور المتوسطة" }
                },
                new JObject
                {
                    { "level", "Intermediate" },
                    { "label", "متوسط" },
                    { "description", "للطلاب المتوسطين - 10-20 آية من السور الطويلة" }
                },
                new JObject
                {
                    { "level", "Advanced" },
                    { "label", "متقدم" },
                    { "description", "للطلاب المتقدمين - 20+ آية من السور الطويلة جداً" }
                },
                new JObject
                {
                    { "level", "Expert" },
                    { "label", "خبير" },
                    { "description", "للطلاب الخبراء - سورة كاملة أو أكثر" }
                }
            };

            return Json(new
            {
                success = true,
                levels = levels
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets all available Surahs
        /// GET: /QuranTestPlan/GetSurahs
        /// </summary>
        [HttpGet]
        public ActionResult GetSurahs()
        {
            var surahs = new JArray
            {
                new JObject { { "name", "الفاتحة" }, { "ayahs", 7 } },
                new JObject { { "name", "البقرة" }, { "ayahs", 286 } },
                new JObject { { "name", "آل عمران" }, { "ayahs", 200 } },
                new JObject { { "name", "النساء" }, { "ayahs", 176 } },
                new JObject { { "name", "المائدة" }, { "ayahs", 120 } },
                new JObject { { "name", "الأنعام" }, { "ayahs", 165 } },
                new JObject { { "name", "الأعراف" }, { "ayahs", 206 } },
                new JObject { { "name", "الأنفال" }, { "ayahs", 75 } },
                new JObject { { "name", "التوبة" }, { "ayahs", 129 } },
                new JObject { { "name", "يونس" }, { "ayahs", 109 } },
                new JObject { { "name", "هود" }, { "ayahs", 123 } },
                new JObject { { "name", "يوسف" }, { "ayahs", 111 } },
                new JObject { { "name", "الرعد" }, { "ayahs", 43 } },
                new JObject { { "name", "إبراهيم" }, { "ayahs", 52 } },
                new JObject { { "name", "الحجر" }, { "ayahs", 99 } },
                new JObject { { "name", "النحل" }, { "ayahs", 128 } },
                new JObject { { "name", "الإسراء" }, { "ayahs", 111 } },
                new JObject { { "name", "الكهف" }, { "ayahs", 110 } },
                new JObject { { "name", "مريم" }, { "ayahs", 98 } },
                new JObject { { "name", "طه" }, { "ayahs", 135 } },
                new JObject { { "name", "الأنبياء" }, { "ayahs", 112 } },
                new JObject { { "name", "الحج" }, { "ayahs", 78 } },
                new JObject { { "name", "المؤمنون" }, { "ayahs", 118 } },
                new JObject { { "name", "النور" }, { "ayahs", 64 } },
                new JObject { { "name", "الفرقان" }, { "ayahs", 77 } },
                new JObject { { "name", "الشعراء" }, { "ayahs", 227 } },
                new JObject { { "name", "النمل" }, { "ayahs", 93 } },
                new JObject { { "name", "القصص" }, { "ayahs", 88 } },
                new JObject { { "name", "العنكبوت" }, { "ayahs", 69 } },
                new JObject { { "name", "الروم" }, { "ayahs", 60 } }
            };

            return Json(new
            {
                success = true,
                total_surahs = 114,
                sample_surahs = surahs
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets test plan template
        /// GET: /QuranTestPlan/GetTemplate
        /// </summary>
        [HttpGet]
        public ActionResult GetTemplate()
        {
            var template = new JObject
            {
                { "title", "عنوان الاختبار" },
                { "surah", "اسم السورة" },
                { "ayah_range", "نطاق الآيات (مثال: 1-10)" },
                { "difficulty", "مستوى الصعوبة" },
                { "estimated_time", "الوقت المتوقع بالدقائق" },
                { "passing_score", "درجة النجاح %" },
                { "tasks", new JArray
                    {
                        new JObject
                        {
                            { "task_id", "معرف المهمة" },
                            { "type", "نوع المهمة (memorization, tajweed, fluency, continuation)" },
                            { "description", "وصف المهمة" },
                            { "weight", "وزن المهمة %" },
                            { "criteria", new JArray { "معايير التقييم" } }
                        }
                    }
                },
                { "tajweed_focus", new JArray { "نقاط التجويد المركزة" } },
                { "teacher_notes", "ملاحظات المعلم" },
                { "student_instructions", "تعليمات الطالب" },
                { "created_at", "تاريخ الإنشاء" },
                { "student_id", "معرف الطالب" }
            };

            return Json(new
            {
                success = true,
                template = template
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Health check endpoint
        /// GET: /QuranTestPlan/Health
        /// </summary>
        [HttpGet]
        public ActionResult Health()
        {
            return Json(new
            {
                status = "healthy",
                service = "Quran Test Plan Generator",
                version = "1.0.0",
                timestamp = DateTime.UtcNow.ToString("O")
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
