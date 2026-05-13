using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Configuration;
using System.IO;
using Maeen1_New.Models;
using Maeen1_New.Models.ViewModels;
using Maeen1_New.Services;

namespace Maeen1_New.Controllers
{
    [AllowAnonymous]
    public class StudentController : Controller
    {
        private readonly Maeen1_NewDbContext _db = new Maeen1_NewDbContext();
        private readonly TeacherNotificationService _notificationService = new TeacherNotificationService();
        private readonly AiAssessmentClient _aiAssessmentClient = new AiAssessmentClient();

        public ActionResult Dashboard(int? userId)
        {
            if (!IsDatabaseConnectionConfigured())
            {
                ViewBag.Error = "إعدادات قاعدة البيانات غير مكتملة في Web.config.";
                ViewBag.Teachers = GetTeachersForStudentDashboard();
                return View(new StudentOnboardingWizardViewModel());
            }

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _db.Users.FirstOrDefault(u => u.Id == userId.Value && u.Role == "Student");
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var profile = _db.StudentOnboardingProfiles
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefault(p => p.UserId == userId.Value);

            var studentExamRequests = _db.ExamRequests
                .Include(r => r.Teacher)
                .Include(r => r.Results)
                .Where(r => r.StudentId == userId.Value)
                .OrderByDescending(r => r.CreatedAt)
                .ToList()
                .Select(r =>
                {
                    var latestResult = r.Results != null
                        ? r.Results.OrderByDescending(x => x.SentAt).FirstOrDefault()
                        : null;

                    return new
                    {
                        RequestId = r.Id,
                        Title = r.Title,
                        Status = r.Status,
                        TeacherName = r.Teacher != null ? r.Teacher.Name : null,
                        CreatedAt = r.CreatedAt,
                        AssignedAt = r.AssignedAt,
                        ResultSentAt = r.ResultSentAt,
                        Grade = latestResult != null ? latestResult.Grade : null,
                        MemorizationPlan = latestResult != null ? latestResult.Recommendations : null
                    };
                })
                .ToList();

            var latestTeacherResult = _db.ExamResults
                .Include(r => r.Teacher)
                .Include(r => r.ExamRequest)
                .Where(r => r.StudentId == userId.Value)
                .OrderByDescending(r => r.SentAt)
                .FirstOrDefault();

            var planNotifications = _db.ExamResults
                .Include(r => r.Teacher)
                .Include(r => r.ExamRequest)
                .Where(r => r.StudentId == userId.Value && r.Recommendations != null && r.Recommendations != "")
                .OrderByDescending(r => r.SentAt)
                .Take(100)
                .ToList()
                .Where(r => !string.IsNullOrWhiteSpace(r.Recommendations))
                .Take(20)
                .Select(r => new
                {
                    TeacherName = r.Teacher != null ? r.Teacher.Name : "المعلم",
                    ExamTitle = r.ExamRequest != null ? r.ExamRequest.Title : "طلب اختبار",
                    SentAt = r.SentAt,
                    MemorizationPlan = r.Recommendations,
                    TeacherNotes = r.TeacherNotes
                })
                .ToList();

            TeacherPlanForStudentViewModel teacherPlan = null;
            if (latestTeacherResult != null)
            {
                teacherPlan = new TeacherPlanForStudentViewModel
                {
                    RequestId = latestTeacherResult.ExamRequestId,
                    ExamTitle = latestTeacherResult.ExamRequest != null ? latestTeacherResult.ExamRequest.Title : "طلب اختبار",
                    TeacherName = latestTeacherResult.Teacher != null ? latestTeacherResult.Teacher.Name : "المعلم",
                    Grade = latestTeacherResult.Grade,
                    TeacherNotes = latestTeacherResult.TeacherNotes,
                    MemorizationPlan = latestTeacherResult.Recommendations,
                    SentAt = latestTeacherResult.SentAt
                };
            }

            ViewBag.Profile = profile;
            ViewBag.StudentExamRequests = studentExamRequests;
            ViewBag.TeacherPlan = teacherPlan;
            ViewBag.PlanNotifications = planNotifications;
            ViewBag.AiConfigStatus = _aiAssessmentClient.GetConfigurationStatus();
            ViewBag.Teachers = GetTeachersForStudentDashboard();
            var model = new StudentOnboardingWizardViewModel
            {
                UserId = userId.Value
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Dashboard(StudentOnboardingWizardViewModel model)
        {
            if (!IsDatabaseConnectionConfigured())
            {
                ViewBag.Error = "إعدادات قاعدة البيانات غير مكتملة في Web.config.";
                ViewBag.Profile = null;
                ViewBag.AiConfigStatus = _aiAssessmentClient.GetConfigurationStatus();
                ViewBag.Teachers = GetTeachersForStudentDashboard();
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Profile = null;
                ViewBag.AiConfigStatus = _aiAssessmentClient.GetConfigurationStatus();
                ViewBag.Teachers = GetTeachersForStudentDashboard();
                return View(model);
            }

            var user = _db.Users.FirstOrDefault(u => u.Id == model.UserId && u.Role == "Student");
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var answers = new WizardAnswers
                {
                    DailyMemorizationHours = model.DailyMemorizationHours,
                    TargetJuzCount = model.TargetJuzCount,
                    TargetJuzNames = model.TargetJuzNames,
                    CompletedJuzCount = model.CompletedJuzCount,
                    CompletedJuzNames = model.CompletedJuzNames,
                    TajweedLevel = model.TajweedLevel,
                    TargetDuration = model.TargetDuration
                };

                var assessmentService = new StudentAssessmentService(_db);
                var assessment = assessmentService.Assess(answers);

                var profile = new StudentOnboardingProfile
                {
                    UserId = user.Id,
                    DailyMemorizationHours = model.DailyMemorizationHours,
                    TargetJuzCount = model.TargetJuzCount,
                    CompletedJuzCount = model.CompletedJuzCount,
                    TajweedLevel = model.TajweedLevel,
                    TargetDuration = model.TargetDuration,
                    DeterminedLevel = assessment.Level,
                    MemorizationPlan = BuildCombinedPlan(assessment, model),
                    SuggestedTeacherName = assessment.SuggestedTeacherName,
                    RecommendationSource = assessment.RecommendationSource,
                    CreatedAt = DateTime.UtcNow
                };

                _db.StudentOnboardingProfiles.Add(profile);

                user.IsOnboardingCompleted = true;
                user.OnboardingCompletedAt = DateTime.UtcNow;
                user.StudentLevel = assessment.Level;

                _db.SaveChanges();

                // Create an ExamRequest so it appears on Admin's ExamPanel
                var admin = _db.Users.FirstOrDefault(u => u.Role == "Admin");
                if (admin != null)
                {
                    var examRequest = new ExamRequest
                    {
                        Title = "طلب اختبار - " + user.Name,
                        Description = "طلب اختبار تلقائي بعد إكمال تحديد المستوى.\n" +
                                      "المستوى: " + assessment.Level +
                                      "\nساعات الحفظ اليومية: " + model.DailyMemorizationHours +
                                      "\nالأجزاء المستهدفة: " + model.TargetJuzCount +
                                      "\nأسماء الأجزاء المستهدفة: " + (model.TargetJuzNames ?? string.Empty) +
                                      "\nالأجزاء المحفوظة مسبقًا: " + model.CompletedJuzCount +
                                      "\nأسماء الأجزاء المحفوظة مسبقًا: " + (model.CompletedJuzNames ?? string.Empty) +
                                      "\nمستوى التجويد: " + (model.TajweedLevel ?? string.Empty) +
                                      "\nالمدة الزمنية المستهدفة: " + (model.TargetDuration ?? string.Empty),
                        StudentId = user.Id,
                        AdminId = admin.Id,
                        TeacherId = null,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow
                    };
                    _db.ExamRequests.Add(examRequest);
                    _db.SaveChanges();
                }

                var suggestedTeacher = _db.Users.FirstOrDefault(u =>
                    u.Role == "Teacher" &&
                    u.Name == assessment.SuggestedTeacherName);

                if (suggestedTeacher == null)
                {
                    suggestedTeacher = _db.Users.FirstOrDefault(u => u.Role == "Teacher");
                }

                if (suggestedTeacher != null)
                {
                    _notificationService.CreateAssessmentNotification(
                        _db,
                        suggestedTeacher.Id,
                        user.Id,
                        user.Name,
                        assessment.Level,
                        assessment.Plan);
                }

                return RedirectToAction("Dashboard", new { userId = user.Id });
            }
            catch (Exception)
            {
                ViewBag.Error = "تعذر حفظ نتائج التقييم، حاول مرة أخرى.";
                ViewBag.Profile = null;
                ViewBag.AiConfigStatus = _aiAssessmentClient.GetConfigurationStatus();
                ViewBag.Teachers = GetTeachersForStudentDashboard();
                return View(model);
            }
        }

        public ActionResult OnboardingWizard(int userId)
        {
            return RedirectToAction("Dashboard", new { userId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult OnboardingWizard(StudentOnboardingWizardViewModel model)
        {
            return Dashboard(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetAssessment(int userId)
        {
            if (!IsDatabaseConnectionConfigured())
            {
                return RedirectToAction("Dashboard", new { userId });
            }

            var user = _db.Users.FirstOrDefault(u => u.Id == userId && u.Role == "Student");
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var profiles = _db.StudentOnboardingProfiles.Where(p => p.UserId == userId).ToList();
            if (profiles.Any())
            {
                _db.StudentOnboardingProfiles.RemoveRange(profiles);
            }

            user.IsOnboardingCompleted = false;
            user.OnboardingCompletedAt = null;
            user.StudentLevel = null;

            _db.SaveChanges();

            return RedirectToAction("Dashboard", new { userId });
        }

        // GET: Student/MyExams
        public ActionResult MyExams(int? userId)
        {
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var student = _db.Users.FirstOrDefault(u => u.Id == userId.Value && u.Role == "Student");
            if (student == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get all exams that have tasks assigned to this student
            var studentTasks = _db.ExamTasks
                .Include(t => t.Exam)
                .Include(t => t.Completions)
                .Where(t => t.StudentId == userId.Value)
                .ToList();

            var examGroups = studentTasks
                .GroupBy(t => t.Exam)
                .Select(g => new StudentExamItemViewModel
                {
                    ExamId = g.Key.Id,
                    ExamTitle = g.Key.Title,
                    ExamDescription = g.Key.Description,
                    Tasks = g.Select(t => new StudentTaskItemViewModel
                    {
                        TaskId = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        DueDate = t.DueDate,
                        IsRequired = t.IsRequired,
                        IsCompleted = t.Completions != null && t.Completions.Any(),
                        IsApproved = t.Completions != null && t.Completions.Any(c => c.IsApproved),
                        CompletedAt = t.Completions != null ? t.Completions.OrderByDescending(c => c.CompletedAt).Select(c => (DateTime?)c.CompletedAt).FirstOrDefault() : null
                    }).ToList(),
                    CanAccessExam = CanStudentAccessExam(g.ToList()),
                    HasAccessedExam = _db.ExamAccessLogs.Any(l => l.ExamId == g.Key.Id && l.StudentId == userId.Value)
                }).ToList();

            var model = new StudentExamViewModel
            {
                StudentId = userId.Value,
                Exams = examGroups
            };

            return View(model);
        }

        // POST: Student/CompleteTask
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CompleteTask(int taskId, int studentId, string notes)
        {
            var task = _db.ExamTasks.FirstOrDefault(t => t.Id == taskId && t.StudentId == studentId);
            if (task == null)
            {
                TempData["Error"] = "لم يتم العثور على المهمة";
                return RedirectToAction("MyExams", new { userId = studentId });
            }

            // Check if already completed
            var existing = _db.StudentTaskCompletions.FirstOrDefault(c => c.TaskId == taskId && c.StudentId == studentId);
            if (existing != null)
            {
                TempData["Error"] = "تم تسليم هذه المهمة مسبقاً";
                return RedirectToAction("MyExams", new { userId = studentId });
            }

            var completion = new StudentTaskCompletion
            {
                TaskId = taskId,
                StudentId = studentId,
                CompletedAt = DateTime.UtcNow,
                Notes = notes,
                IsApproved = false
            };

            _db.StudentTaskCompletions.Add(completion);
            _db.SaveChanges();

            TempData["Success"] = "تم تسليم المهمة بنجاح - في انتظار موافقة المعلم";
            return RedirectToAction("MyExams", new { userId = studentId });
        }

        // POST: Student/AccessExam
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AccessExam(int examId, int studentId)
        {
            var student = _db.Users.FirstOrDefault(u => u.Id == studentId && u.Role == "Student");
            if (student == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if student has completed all required tasks for this exam
            var tasks = _db.ExamTasks
                .Include(t => t.Completions)
                .Where(t => t.ExamId == examId && t.StudentId == studentId)
                .ToList();

            if (!CanStudentAccessExam(tasks))
            {
                TempData["Error"] = "لم تكمل جميع المهام المطلوبة بعد";
                return RedirectToAction("MyExams", new { userId = studentId });
            }

            // Log the exam access
            var accessLog = new ExamAccessLog
            {
                ExamId = examId,
                StudentId = studentId,
                AccessedAt = DateTime.UtcNow,
                IpAddress = Request.UserHostAddress
            };

            _db.ExamAccessLogs.Add(accessLog);
            _db.SaveChanges();

            TempData["Success"] = "تم فتح الاختبار بنجاح";
            return RedirectToAction("TakeExam", new { examId, userId = studentId });
        }

        // GET: Student/TakeExam
        public ActionResult TakeExam(int examId, int userId)
        {
            var exam = _db.Exams.Find(examId);
            if (exam == null)
            {
                return RedirectToAction("MyExams", new { userId });
            }

            // Verify access
            var hasAccess = _db.ExamAccessLogs.Any(l => l.ExamId == examId && l.StudentId == userId);
            if (!hasAccess)
            {
                TempData["Error"] = "ليس لديك صلاحية الوصول لهذا الاختبار";
                return RedirectToAction("MyExams", new { userId });
            }

            ViewBag.ExamTitle = exam.Title;
            ViewBag.ExamDescription = exam.Description;
            ViewBag.StudentId = userId;
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ChallengeSubmit(int? studentId)
        {
            if (!studentId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            return RedirectToAction("Dashboard", new { userId = studentId.Value });
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ChallengeSubmit(int studentId, int? examId, string notes, bool redirectToRecord = false)
        {
            var student = _db.Users.FirstOrDefault(u => u.Id == studentId && u.Role == "Student");
            if (student == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int? teacherId = null;
            if (examId.HasValue)
            {
                teacherId = _db.Exams
                    .Where(e => e.Id == examId.Value)
                    .Select(e => (int?)e.TeacherId)
                    .FirstOrDefault();
            }

            if (!teacherId.HasValue)
            {
                var latestTeacherFromResults = _db.ExamResults
                    .Where(r => r.StudentId == studentId)
                    .OrderByDescending(r => r.SentAt)
                    .Select(r => (int?)r.TeacherId)
                    .FirstOrDefault();
                if (latestTeacherFromResults.HasValue)
                {
                    teacherId = latestTeacherFromResults.Value;
                }
            }

            if (!teacherId.HasValue)
            {
                var latestAssignedTeacher = _db.ExamRequests
                    .Where(r => r.StudentId == studentId && r.TeacherId.HasValue)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => r.TeacherId)
                    .FirstOrDefault();
                if (latestAssignedTeacher.HasValue)
                {
                    teacherId = latestAssignedTeacher.Value;
                }
            }

            if (!teacherId.HasValue)
            {
                teacherId = _db.Users
                    .Where(u => u.Role == "Teacher")
                    .OrderBy(u => u.Id)
                    .Select(u => (int?)u.Id)
                    .FirstOrDefault();
            }

            if (!teacherId.HasValue)
            {
                TempData["Error"] = "لا يوجد مدرس متاح حالياً لإنشاء طلب التسميع";
                return RedirectToAction("MyExams", new { userId = studentId });
            }

            var request = new RecitationRequest
            {
                StudentId = studentId,
                TeacherId = teacherId.Value,
                ExamId = examId,
                Notes = notes,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _db.RecitationRequests.Add(request);
            _db.SaveChanges();

            TempData["Success"] = "تم إرسال طلب التحدي للمعلم بنجاح";
            if (redirectToRecord)
            {
                return RedirectToAction("RecordStudentVoice", new { userId = studentId, requestId = request.Id });
            }

            return RedirectToAction("Dashboard", new { userId = studentId });
        }

        [AllowAnonymous]
        public ActionResult RecordStudentVoice(int userId, int requestId)
        {
            var student = _db.Users.FirstOrDefault(u => u.Id == userId && u.Role == "Student");
            if (student == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var req = _db.RecitationRequests
                .Include(r => r.Teacher)
                .FirstOrDefault(r => r.Id == requestId && r.StudentId == userId);
            if (req == null)
            {
                TempData["Error"] = "طلب التسميع غير موجود";
                return RedirectToAction("Dashboard", new { userId });
            }

            ViewBag.StudentId = userId;
            ViewBag.RequestId = requestId;
            ViewBag.TeacherName = req.Teacher != null ? req.Teacher.Name : "المعلم";
            ViewBag.RequestNotes = req.Notes;
            return View();
        }

        [AllowAnonymous]
        public ActionResult MyRecitationSessions(int? userId)
        {
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var student = _db.Users.FirstOrDefault(u => u.Id == userId.Value && u.Role == "Student");
            if (student == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var requests = _db.RecitationRequests
                .Include(r => r.Teacher)
                .Where(r => r.StudentId == userId.Value)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            var requestIds = requests.Select(r => r.Id).ToList();
            var sessions = _db.RecitationSessions
                .Where(s => requestIds.Contains(s.RecitationRequestId))
                .ToList()
                .GroupBy(s => s.RecitationRequestId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.CreatedAt).FirstOrDefault());

            foreach (var request in requests)
            {
                request.Session = sessions.ContainsKey(request.Id) ? sessions[request.Id] : null;
            }

            var studentVoiceUrls = new Dictionary<int, string>();
            var teacherVoiceUrls = new Dictionary<int, string>();
            var studentScreenUrls = new Dictionary<int, string>();
            var teacherScreenUrls = new Dictionary<int, string>();
            foreach (var request in requests)
            {
                if (GetExistingRecitationVoiceFile(request.Id, "student") != null)
                {
                    studentVoiceUrls[request.Id] = Url.Action("GetRecitationVoice", "Student", new
                    {
                        requestId = request.Id,
                        role = "student",
                        studentId = userId.Value
                    });
                }

                if (GetExistingRecitationVoiceFile(request.Id, "teacher") != null)
                {
                    teacherVoiceUrls[request.Id] = Url.Action("GetRecitationVoice", "Student", new
                    {
                        requestId = request.Id,
                        role = "teacher",
                        studentId = userId.Value
                    });
                }

                if (GetExistingRecitationScreenFile(request.Id, "student") != null)
                {
                    studentScreenUrls[request.Id] = Url.Action("GetRecitationScreen", "Student", new
                    {
                        requestId = request.Id,
                        role = "student",
                        studentId = userId.Value
                    });
                }

                if (GetExistingRecitationScreenFile(request.Id, "teacher") != null)
                {
                    teacherScreenUrls[request.Id] = Url.Action("GetRecitationScreen", "Student", new
                    {
                        requestId = request.Id,
                        role = "teacher",
                        studentId = userId.Value
                    });
                }
            }

            ViewBag.StudentVoiceUrls = studentVoiceUrls;
            ViewBag.TeacherVoiceUrls = teacherVoiceUrls;
            ViewBag.StudentScreenUrls = studentScreenUrls;
            ViewBag.TeacherScreenUrls = teacherScreenUrls;
            ViewBag.StudentId = userId.Value;
            return View(requests);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitRecitationEvaluation(
            int requestId,
            int studentId,
            decimal? score,
            decimal? maxScore,
            string grade,
            string teacherNotes,
            string strengths,
            string weaknesses)
        {
            var student = _db.Users.FirstOrDefault(u => u.Id == studentId && u.Role == "Student");
            if (student == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var request = _db.RecitationRequests
                .FirstOrDefault(r => r.Id == requestId && r.StudentId == studentId);
            if (request == null)
            {
                TempData["Error"] = "طلب التسميع غير موجود";
                return RedirectToAction("MyRecitationSessions", new { userId = studentId });
            }

            var noteLines = new List<string>();
            noteLines.Add("📤 إرسال نتيجة الاختبار");
            noteLines.Add("الدرجة المحصلة: " + (score.HasValue ? score.Value.ToString("0.##") : "-"));
            noteLines.Add("الدرجة الكاملة: " + (maxScore.HasValue ? maxScore.Value.ToString("0.##") : "-"));
            noteLines.Add("التقدير: " + (string.IsNullOrWhiteSpace(grade) ? "-" : grade.Trim()));
            noteLines.Add("ملاحظات المعلم: " + (string.IsNullOrWhiteSpace(teacherNotes) ? "-" : teacherNotes.Trim()));
            noteLines.Add("نقاط القوة: " + (string.IsNullOrWhiteSpace(strengths) ? "-" : strengths.Trim()));
            noteLines.Add("نقاط الضعف: " + (string.IsNullOrWhiteSpace(weaknesses) ? "-" : weaknesses.Trim()));

            var evalBlock = string.Join("\n", noteLines);
            var existing = request.Notes ?? string.Empty;
            var marker = "\n---EVALUATION---\n";
            var baseNote = existing;
            var markerIndex = existing.IndexOf(marker, StringComparison.Ordinal);
            if (markerIndex >= 0)
            {
                baseNote = existing.Substring(0, markerIndex);
            }

            request.Notes = (baseNote ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                request.Notes += marker;
            }
            request.Notes += evalBlock;
            request.UpdatedAt = DateTime.UtcNow;
            _db.SaveChanges();

            TempData["Success"] = "تم إرسال نموذج التقييم للمعلم بنجاح";
            return RedirectToAction("MyRecitationSessions", new { userId = studentId });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult UploadRecitationVoice(int requestId, int studentId, HttpPostedFileBase voiceFile)
        {
            var student = _db.Users.FirstOrDefault(u => u.Id == studentId && u.Role == "Student");
            if (student == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var request = _db.RecitationRequests.FirstOrDefault(r => r.Id == requestId && r.StudentId == studentId);
            if (request == null)
            {
                TempData["Error"] = "طلب التسميع غير موجود";
                return RedirectToAction("MyRecitationSessions", new { userId = studentId });
            }

            if (voiceFile == null || voiceFile.ContentLength <= 0)
            {
                TempData["Error"] = "يرجى اختيار ملف صوتي قبل الرفع";
                return RedirectToAction("MyRecitationSessions", new { userId = studentId });
            }

            var extension = (Path.GetExtension(voiceFile.FileName) ?? string.Empty).ToLowerInvariant();
            var allowed = new[] { ".webm", ".wav", ".mp3", ".m4a", ".ogg", ".aac" };
            if (!allowed.Contains(extension))
            {
                TempData["Error"] = "نوع الملف الصوتي غير مدعوم";
                return RedirectToAction("MyRecitationSessions", new { userId = studentId });
            }

            var directory = EnsureRecitationVoiceDirectory();
            DeleteExistingRecitationVoiceFiles(requestId, "student");
            var path = Path.Combine(directory, $"request_{requestId}_student{extension}");
            voiceFile.SaveAs(path);

            request.UpdatedAt = DateTime.UtcNow;
            _db.SaveChanges();

            TempData["Success"] = "تم رفع التسجيل الصوتي للطالب بنجاح";
            return RedirectToAction("MyRecitationSessions", new { userId = studentId });
        }

        [AllowAnonymous]
        public ActionResult GetRecitationVoice(int requestId, string role, int studentId)
        {
            role = (role ?? string.Empty).Trim().ToLowerInvariant();
            if (role != "student" && role != "teacher")
            {
                return HttpNotFound();
            }

            var request = _db.RecitationRequests.FirstOrDefault(r => r.Id == requestId && r.StudentId == studentId);
            if (request == null)
            {
                return new HttpStatusCodeResult(403);
            }

            var filePath = GetExistingRecitationVoiceFile(requestId, role);
            if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
            {
                return HttpNotFound();
            }

            return File(filePath, MimeMapping.GetMimeMapping(filePath));
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult UploadRecitationScreen(int requestId, int studentId, HttpPostedFileBase screenFile)
        {
            var student = _db.Users.FirstOrDefault(u => u.Id == studentId && u.Role == "Student");
            if (student == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var request = _db.RecitationRequests.FirstOrDefault(r => r.Id == requestId && r.StudentId == studentId);
            if (request == null)
            {
                TempData["Error"] = "طلب التسميع غير موجود";
                return RedirectToAction("MyRecitationSessions", new { userId = studentId });
            }

            if (screenFile == null || screenFile.ContentLength <= 0)
            {
                TempData["Error"] = "يرجى اختيار تسجيل شاشة قبل الرفع";
                return RedirectToAction("MyRecitationSessions", new { userId = studentId });
            }

            var extension = (Path.GetExtension(screenFile.FileName) ?? string.Empty).ToLowerInvariant();
            var allowed = new[] { ".webm", ".mp4", ".mov", ".mkv" };
            if (!allowed.Contains(extension))
            {
                TempData["Error"] = "نوع ملف تسجيل الشاشة غير مدعوم";
                return RedirectToAction("MyRecitationSessions", new { userId = studentId });
            }

            var directory = EnsureRecitationVoiceDirectory();
            DeleteExistingRecitationScreenFiles(requestId, "student");
            var path = Path.Combine(directory, $"request_{requestId}_student_screen{extension}");
            screenFile.SaveAs(path);

            request.UpdatedAt = DateTime.UtcNow;
            _db.SaveChanges();

            TempData["Success"] = "تم رفع تسجيل الشاشة للطالب بنجاح";
            return RedirectToAction("MyRecitationSessions", new { userId = studentId });
        }

        [AllowAnonymous]
        public ActionResult GetRecitationScreen(int requestId, string role, int studentId)
        {
            role = (role ?? string.Empty).Trim().ToLowerInvariant();
            if (role != "student" && role != "teacher")
            {
                return HttpNotFound();
            }

            var request = _db.RecitationRequests.FirstOrDefault(r => r.Id == requestId && r.StudentId == studentId);
            if (request == null)
            {
                return new HttpStatusCodeResult(403);
            }

            var filePath = GetExistingRecitationScreenFile(requestId, role);
            if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
            {
                return HttpNotFound();
            }

            return File(filePath, MimeMapping.GetMimeMapping(filePath));
        }

        private bool CanStudentAccessExam(List<ExamTask> tasks)
        {
            if (!tasks.Any())
                return false;

            var requiredTasks = tasks.Where(t => t.IsRequired).ToList();
            if (!requiredTasks.Any())
                return true;

            // All required tasks must be completed AND approved
            return requiredTasks.All(t =>
                t.Completions != null && t.Completions.Any(c => c.IsApproved));
        }

        private List<UserBasicViewModel> GetTeachersForStudentDashboard()
        {
            return _db.Users
                .Where(u => u.Role == "Teacher")
                .OrderBy(u => u.Name)
                .Select(u => new UserBasicViewModel
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email
                })
                .ToList();
        }

        private bool IsDatabaseConnectionConfigured()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Maeen1ConnectionString"]?.ConnectionString;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return false;
            }

            return connectionString.IndexOf("YOUR_PROJECT_REF", StringComparison.OrdinalIgnoreCase) < 0
                && connectionString.IndexOf("YOUR_PASSWORD", StringComparison.OrdinalIgnoreCase) < 0;
        }

        private static string BuildCombinedPlan(AssessmentResult assessment, StudentOnboardingWizardViewModel model)
        {
            var basePlan = assessment.Plan ?? string.Empty;
            var schedule = assessment.SmartSchedule ?? string.Empty;
            var targetNames = model.TargetJuzNames ?? string.Empty;
            var completedNames = model.CompletedJuzNames ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(targetNames))
            {
                basePlan += "\nالأجزاء المستهدفة بالاسم: " + targetNames.Trim();
            }

            if (!string.IsNullOrWhiteSpace(completedNames))
            {
                basePlan += "\nالأجزاء المحفوظة مسبقًا بالاسم: " + completedNames.Trim();
            }

            if (string.IsNullOrWhiteSpace(schedule))
            {
                return basePlan;
            }

            return basePlan + "\n\n---SMART_SCHEDULE---\n" + schedule;
        }

        private string EnsureRecitationVoiceDirectory()
        {
            var directory = Server.MapPath("~/App_Data/recitation-voices");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return directory;
        }

        private string GetExistingRecitationVoiceFile(int requestId, string role)
        {
            var directory = EnsureRecitationVoiceDirectory();
            return Directory
                .GetFiles(directory, $"request_{requestId}_{role}.*")
                .OrderByDescending(System.IO.File.GetLastWriteTimeUtc)
                .FirstOrDefault();
        }

        private void DeleteExistingRecitationVoiceFiles(int requestId, string role)
        {
            var directory = EnsureRecitationVoiceDirectory();
            var files = Directory.GetFiles(directory, $"request_{requestId}_{role}.*");
            foreach (var file in files)
            {
                System.IO.File.Delete(file);
            }
        }

        private string GetExistingRecitationScreenFile(int requestId, string role)
        {
            var directory = EnsureRecitationVoiceDirectory();
            return Directory
                .GetFiles(directory, $"request_{requestId}_{role}_screen.*")
                .OrderByDescending(System.IO.File.GetLastWriteTimeUtc)
                .FirstOrDefault();
        }

        private void DeleteExistingRecitationScreenFiles(int requestId, string role)
        {
            var directory = EnsureRecitationVoiceDirectory();
            var files = Directory.GetFiles(directory, $"request_{requestId}_{role}_screen.*");
            foreach (var file in files)
            {
                System.IO.File.Delete(file);
            }
        }
    }
}
