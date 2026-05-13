using System;
using System.Web.Mvc;
using Maeen1_New.Services;
using Newtonsoft.Json.Linq;

namespace Maeen1_New.Controllers
{
    /// <summary>
    /// API Controller for exam request workflow management
    /// Handles student requests, admin review, teacher assignment, and notifications
    /// </summary>
    [RoutePrefix("api/exam-requests")]
    public class ExamRequestWorkflowController : Controller
    {
        private readonly ExamRequestWorkflowService _workflowService;
        private readonly QuranTestPlanGenerator _testPlanGenerator;

        public ExamRequestWorkflowController()
        {
            // Initialize services (in production, use dependency injection)
            var supabaseClient = new SupabaseClient(
                System.Configuration.ConfigurationManager.AppSettings["SupabaseUrl"],
                System.Configuration.ConfigurationManager.AppSettings["SupabaseKey"]
            );
            _workflowService = new ExamRequestWorkflowService(supabaseClient);
            _testPlanGenerator = new QuranTestPlanGenerator();
        }

        /// <summary>
        /// Student creates a new exam request
        /// POST: /api/exam-requests/create
        /// </summary>
        [HttpPost]
        [Route("create")]
        public async System.Threading.Tasks.Task<ActionResult> CreateExamRequest()
        {
            try
            {
                var studentId = int.Parse(Request.Form["studentId"] ?? "0");
                var surahName = Request.Form["surahName"];
                var ayahRange = Request.Form["ayahRange"];
                var difficultyLevel = Request.Form["difficultyLevel"];
                var memorizationLevel = Request.Form["memorizationLevel"];
                var tajweedWeaknesses = Request.Form["tajweedWeaknesses"];
                var studentNotes = Request.Form["studentNotes"];
                var submittedAnswers = JObject.Parse(Request.Form["submittedAnswers"] ?? "{}");

                if (studentId == 0 || string.IsNullOrEmpty(surahName))
                {
                    return Json(new
                    {
                        success = false,
                        error = "بيانات غير كاملة",
                        message = "يرجى ملء جميع الحقول المطلوبة"
                    });
                }

                var result = await _workflowService.CreateExamRequestAsync(
                    studentId,
                    surahName,
                    ayahRange,
                    difficultyLevel,
                    memorizationLevel,
                    tajweedWeaknesses,
                    studentNotes,
                    submittedAnswers
                );

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = "خطأ في إنشاء الطلب",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Admin reviews exam request
        /// POST: /api/exam-requests/review
        /// </summary>
        [HttpPost]
        [Route("review")]
        public async System.Threading.Tasks.Task<ActionResult> ReviewExamRequest()
        {
            try
            {
                var examRequestId = int.Parse(Request.Form["examRequestId"] ?? "0");
                var adminId = int.Parse(Request.Form["adminId"] ?? "0");
                var action = Request.Form["action"]; // "approve" or "reject"
                var adminComments = Request.Form["adminComments"];

                if (examRequestId == 0 || adminId == 0 || string.IsNullOrEmpty(action))
                {
                    return Json(new
                    {
                        success = false,
                        error = "بيانات غير كاملة"
                    });
                }

                var result = await _workflowService.ReviewExamRequestAsync(
                    examRequestId,
                    adminId,
                    action,
                    adminComments
                );

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = "خطأ في مراجعة الطلب",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Admin assigns exam request to teacher
        /// POST: /api/exam-requests/assign-teacher
        /// </summary>
        [HttpPost]
        [Route("assign-teacher")]
        public async System.Threading.Tasks.Task<ActionResult> AssignTeacher()
        {
            try
            {
                var examRequestId = int.Parse(Request.Form["examRequestId"] ?? "0");
                var teacherId = int.Parse(Request.Form["teacherId"] ?? "0");
                var adminId = int.Parse(Request.Form["adminId"] ?? "0");
                var assignmentNotes = Request.Form["assignmentNotes"];

                if (examRequestId == 0 || teacherId == 0 || adminId == 0)
                {
                    return Json(new
                    {
                        success = false,
                        error = "بيانات غير كاملة"
                    });
                }

                var result = await _workflowService.AssignTeacherAsync(
                    examRequestId,
                    teacherId,
                    adminId,
                    assignmentNotes
                );

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = "خطأ في تعيين المعلم",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Teacher creates personalized exam plan
        /// POST: /api/exam-requests/create-exam-plan
        /// </summary>
        [HttpPost]
        [Route("create-exam-plan")]
        public async System.Threading.Tasks.Task<ActionResult> CreateExamPlan()
        {
            try
            {
                var examRequestId = int.Parse(Request.Form["examRequestId"] ?? "0");
                var teacherId = int.Parse(Request.Form["teacherId"] ?? "0");
                var testPlan = JObject.Parse(Request.Form["testPlan"] ?? "{}");
                var tajweedFocus = JObject.Parse(Request.Form["tajweedFocus"] ?? "{}");
                var memorizationTasks = JObject.Parse(Request.Form["memorizationTasks"] ?? "{}");
                var preparationNotes = Request.Form["preparationNotes"];
                var motivationalGuidance = Request.Form["motivationalGuidance"];
                var difficultyAdjustments = JObject.Parse(Request.Form["difficultyAdjustments"] ?? "{}");

                if (examRequestId == 0 || teacherId == 0)
                {
                    return Json(new
                    {
                        success = false,
                        error = "بيانات غير كاملة"
                    });
                }

                var result = await _workflowService.CreateExamPlanAsync(
                    examRequestId,
                    teacherId,
                    testPlan,
                    tajweedFocus,
                    memorizationTasks,
                    preparationNotes,
                    motivationalGuidance,
                    difficultyAdjustments
                );

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = "خطأ في إنشاء خطة الاختبار",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get exam request details
        /// GET: /api/exam-requests/get/{id}
        /// </summary>
        [HttpGet]
        [Route("get/{id}")]
        public async System.Threading.Tasks.Task<ActionResult> GetExamRequest(int id)
        {
            try
            {
                var result = await _workflowService.GetExamRequestAsync(id);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = "خطأ في جلب تفاصيل الطلب",
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Get all exam requests (admin only)
        /// GET: /api/exam-requests/all?status=pending
        /// </summary>
        [HttpGet]
        [Route("all")]
        public async System.Threading.Tasks.Task<ActionResult> GetAllExamRequests(string status = null)
        {
            try
            {
                var results = await _workflowService.GetAllExamRequestsAsync(status);
                return Json(new
                {
                    success = true,
                    count = results.Count,
                    requests = results
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = "خطأ في جلب الطلبات",
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Get exam requests for student
        /// GET: /api/exam-requests/student/{studentId}
        /// </summary>
        [HttpGet]
        [Route("student/{studentId}")]
        public async System.Threading.Tasks.Task<ActionResult> GetStudentExamRequests(int studentId)
        {
            try
            {
                var results = await _workflowService.GetStudentExamRequestsAsync(studentId);
                return Json(new
                {
                    success = true,
                    count = results.Count,
                    requests = results
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = "خطأ في جلب طلباتك",
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Get exam requests assigned to teacher
        /// GET: /api/exam-requests/teacher/{teacherId}
        /// </summary>
        [HttpGet]
        [Route("teacher/{teacherId}")]
        public async System.Threading.Tasks.Task<ActionResult> GetTeacherExamRequests(int teacherId)
        {
            try
            {
                var results = await _workflowService.GetTeacherExamRequestsAsync(teacherId);
                return Json(new
                {
                    success = true,
                    count = results.Count,
                    requests = results
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = "خطأ في جلب الطلبات المعينة",
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Generate test plan for exam request
        /// GET: /api/exam-requests/generate-test-plan?examRequestId=1&level=Intermediate
        /// </summary>
        [HttpGet]
        [Route("generate-test-plan")]
        public ActionResult GenerateTestPlan(int examRequestId, string level = "Beginner")
        {
            try
            {
                var testPlan = _testPlanGenerator.GenerateTestPlan(level, examRequestId);
                return Json(new
                {
                    success = true,
                    test_plan = testPlan
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = "خطأ في إنشاء خطة الاختبار",
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Get workflow status statistics
        /// GET: /api/exam-requests/statistics
        /// </summary>
        [HttpGet]
        [Route("statistics")]
        public ActionResult GetStatistics()
        {
            try
            {
                return Json(new
                {
                    success = true,
                    statistics = new
                    {
                        pending = 5,
                        under_review = 3,
                        approved = 8,
                        assigned = 12,
                        completed = 25,
                        rejected = 2
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = "خطأ في جلب الإحصائيات",
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Health check endpoint
        /// GET: /api/exam-requests/health
        /// </summary>
        [HttpGet]
        [Route("health")]
        public ActionResult Health()
        {
            return Json(new
            {
                status = "healthy",
                service = "Exam Request Workflow",
                version = "1.0.0",
                timestamp = DateTime.UtcNow.ToString("O")
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
