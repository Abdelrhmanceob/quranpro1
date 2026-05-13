using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using Maeen1_New.Models;
using Maeen1_New.Models.ViewModels;
using Maeen1_New.Services;

namespace Maeen1_New.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult Dashboard(int? userId)
        {
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            using (var db = new Maeen1_NewDbContext())
            {
                var admin = db.Users.Find(userId.Value);
                if (admin == null || admin.Role != "Admin")
                    return RedirectToAction("Login", "Account");

                // Counters
                var totalStudents  = db.Users.Count(u => u.Role == "Student");
                var totalTeachers  = db.Users.Count(u => u.Role == "Teacher");
                var allRequests    = db.ExamRequests
                                       .Include(r => r.Student)
                                       .Include(r => r.Teacher)
                                       .Include(r => r.Results)
                                       .OrderByDescending(r => r.CreatedAt)
                                       .Take(20)
                                       .ToList();

                var rows = allRequests.Select(r =>
                {
                    var latestResult = r.Results?.OrderByDescending(res => res.SentAt).FirstOrDefault();
                    return new AdminDashboardRequestRow
                    {
                        RequestId   = r.Id,
                        Title       = r.Title,
                        StudentName = r.Student != null ? r.Student.Name : "—",
                        TeacherName = r.Teacher != null ? r.Teacher.Name : "غير معيّن",
                        Status      = r.Status,
                        CreatedAt   = r.CreatedAt,
                        Grade       = latestResult != null ? latestResult.Grade : null
                    };
                }).ToList();

                var adminNotificationService = new AdminNotificationService();
                var decisionNotifications = adminNotificationService
                    .GetLatestTeacherDecisionNotifications(db, userId.Value, 10)
                    .Select(n => new AdminTeacherDecisionNotificationViewModel
                    {
                        NotificationId = n.Id,
                        ExamRequestId = n.ExamRequestId,
                        ExamTitle = n.ExamTitle,
                        TeacherName = n.TeacherName,
                        DecisionStatus = n.DecisionStatus,
                        DecisionLabel = n.DecisionLabel,
                        DecisionNotes = n.DecisionNotes,
                        CreatedAt = n.CreatedAt
                    })
                    .ToList();

                var allCount = db.ExamRequests.Count();

                var model = new AdminDashboardViewModel
                {
                    AdminName         = admin.Name,
                    AdminId           = userId.Value,
                    TotalStudents     = totalStudents,
                    TotalTeachers     = totalTeachers,
                    TotalExamRequests = allCount,
                    PendingRequests   = db.ExamRequests.Count(r => r.Status == "Pending"),
                    AssignedRequests  = db.ExamRequests.Count(r => r.Status == "AssignedToTeacher"),
                    CompletedRequests = db.ExamRequests.Count(r => r.Status == "ResultSent"),
                    RecentRequests    = rows,
                    TeacherDecisionNotifications = decisionNotifications
                };

                return View(model);
            }
        }

        // GET: Admin/ExamMonitor
        public ActionResult ExamMonitor(int? userId)
        {
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            using (var db = new Maeen1_NewDbContext())
            {
                var admin = db.Users.Find(userId.Value);
                if (admin == null || admin.Role != "Admin")
                {
                    return RedirectToAction("Login", "Account");
                }

                ViewBag.AdminName = admin.Name;
                ViewBag.AdminId = userId.Value;

                var accessLogs = db.ExamAccessLogs
                    .Include(l => l.Exam)
                    .Include(l => l.Student)
                    .OrderByDescending(l => l.AccessedAt)
                    .Take(100)
                    .ToList()
                    .Select(l => new ExamAccessLogViewModel
                    {
                        StudentName = l.Student.Name,
                        ExamTitle = l.Exam.Title,
                        AccessedAt = l.AccessedAt,
                        IpAddress = l.IpAddress
                    }).ToList();

                var exams = db.Exams
                    .Include(e => e.Teacher)
                    .Include(e => e.Tasks)
                    .ToList();

                var examSummaries = exams.Select(e => new ExamSummaryViewModel
                {
                    ExamTitle = e.Title,
                    TeacherName = e.Teacher != null ? e.Teacher.Name : "غير معروف",
                    TotalTasks = e.Tasks.Count,
                    CompletedTasks = db.StudentTaskCompletions
                        .Count(c => c.Task.ExamId == e.Id && c.IsApproved),
                    StudentsWithAccess = db.ExamAccessLogs
                        .Where(l => l.ExamId == e.Id)
                        .Select(l => l.StudentId)
                        .Distinct()
                        .Count()
                }).ToList();

                var model = new AdminExamMonitorViewModel
                {
                    AccessLogs = accessLogs,
                    ExamSummaries = examSummaries
                };

                return View(model);
            }
        }

        // =============================================
        // GET: Admin/ExamPanel
        // =============================================
        public ActionResult ExamPanel(int? userId)
        {
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            using (var db = new Maeen1_NewDbContext())
            {
                var admin = db.Users.Find(userId.Value);
                if (admin == null || admin.Role != "Admin")
                    return RedirectToAction("Login", "Account");

                ViewBag.AdminName = admin.Name;
                ViewBag.AdminId = userId.Value;

                var requests = db.ExamRequests
                    .Include(r => r.Student)
                    .Include(r => r.Teacher)
                    .Include(r => r.Results)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToList();

                var studentIds = requests.Select(r => r.StudentId).Distinct().ToList();
                var latestProfilesByStudent = db.StudentOnboardingProfiles
                    .Where(p => studentIds.Contains(p.UserId))
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList()
                    .GroupBy(p => p.UserId)
                    .ToDictionary(g => g.Key, g => g.First());

                var students = db.Users
                    .Where(u => u.Role == "Student")
                    .OrderBy(u => u.Name)
                    .Select(u => new UserBasicViewModel { Id = u.Id, Name = u.Name })
                    .ToList();

                var teachers = db.Users
                    .Where(u => u.Role == "Teacher")
                    .OrderBy(u => u.Name)
                    .Select(u => new UserBasicViewModel
                    {
                        Id = u.Id,
                        Name = u.Email == "teacher@maeen.com" ? "teachervip" : u.Name,
                        Email = u.Email
                    })
                    .ToList();

                var studentSubmissions = db.StudentTaskCompletions
                    .Include(c => c.Student)
                    .Include(c => c.Task)
                    .Include(c => c.Task.Exam)
                    .OrderByDescending(c => c.CompletedAt)
                    .Take(100)
                    .ToList();

                var model = new AdminExamPanelViewModel
                {
                    AdminId = userId.Value,
                    Students = students,
                    Teachers = teachers,
                    StudentSubmissions = studentSubmissions.Select(c => new StudentSubmissionItemViewModel
                    {
                        CompletionId = c.Id,
                        StudentName = c.Student != null ? c.Student.Name : "غير معروف",
                        ExamTitle = c.Task != null && c.Task.Exam != null ? c.Task.Exam.Title : "غير محدد",
                        TaskTitle = c.Task != null ? c.Task.Title : "غير محدد",
                        Notes = c.Notes,
                        CompletedAt = c.CompletedAt,
                        IsApproved = c.IsApproved,
                        ApprovedAt = c.ApprovedAt
                    }).ToList(),
                    ExamRequests = requests.Select(r =>
                    {
                        StudentOnboardingProfile profile;
                        latestProfilesByStudent.TryGetValue(r.StudentId, out profile);

                        return new ExamRequestItemViewModel
                        {
                            Id = r.Id,
                            Title = r.Title,
                            Description = r.Description,
                            PlanContent = r.PlanContent,
                            PlanObjectives = r.PlanObjectives,
                            PlanDurationMinutes = r.PlanDurationMinutes,
                            StudentName = r.Student != null ? r.Student.Name : "غير معروف",
                            StudentId = r.StudentId,
                            TeacherName = r.Teacher != null ? r.Teacher.Name : "غير معين",
                            TeacherId = r.TeacherId,
                            Status = r.Status,
                            CreatedAt = r.CreatedAt,
                            AssignedAt = r.AssignedAt,
                            ResultSentAt = r.ResultSentAt,
                            HasResult = r.Results != null && r.Results.Any(),
                            StudentAnswerDailyMemorizationHours = profile != null ? profile.DailyMemorizationHours.ToString() : null,
                            StudentAnswerTargetJuzCount = profile != null ? profile.TargetJuzCount.ToString() : null,
                            StudentAnswerTargetJuzNames = profile != null ? ExtractPlanLineValue(profile.MemorizationPlan, "الأجزاء المستهدفة بالاسم:") : null,
                            StudentAnswerCompletedJuzCount = profile != null ? profile.CompletedJuzCount.ToString() : null,
                            StudentAnswerCompletedJuzNames = profile != null ? ExtractPlanLineValue(profile.MemorizationPlan, "الأجزاء المحفوظة مسبقًا بالاسم:") : null,
                            StudentAnswerTajweedLevel = profile != null ? profile.TajweedLevel : null,
                            StudentAnswerTargetDuration = profile != null ? profile.TargetDuration : null
                        };
                    }).ToList()
                };

                var studentAnswersByRequest = requests.ToDictionary(
                    r => r.Id,
                    r =>
                    {
                        StudentOnboardingProfile profile;
                        latestProfilesByStudent.TryGetValue(r.StudentId, out profile);

                        return (object)new
                        {
                            DailyMemorizationHours = profile != null ? profile.DailyMemorizationHours.ToString() : null,
                            TargetJuzCount = profile != null ? profile.TargetJuzCount.ToString() : null,
                            TargetJuzNames = profile != null ? ExtractPlanLineValue(profile.MemorizationPlan, "الأجزاء المستهدفة بالاسم:") : null,
                            CompletedJuzCount = profile != null ? profile.CompletedJuzCount.ToString() : null,
                            CompletedJuzNames = profile != null ? ExtractPlanLineValue(profile.MemorizationPlan, "الأجزاء المحفوظة مسبقًا بالاسم:") : null,
                            TajweedLevel = profile != null ? profile.TajweedLevel : null,
                            TargetDuration = profile != null ? profile.TargetDuration : null
                        };
                    });

                ViewBag.StudentAnswersByRequest = studentAnswersByRequest;

                return View(model);
            }
        }

        // POST: Admin/CreateExamRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateExamRequest(CreateExamRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "يرجى ملء جميع الحقول المطلوبة";
                return RedirectToAction("ExamPanel", new { userId = model.AdminId });
            }

            using (var db = new Maeen1_NewDbContext())
            {
                var request = new ExamRequest
                {
                    Title = model.Title,
                    Description = model.Description,
                    StudentId = model.StudentId,
                    TeacherId = model.TeacherId,
                    AdminId = model.AdminId,
                    PlanContent = model.PlanContent,
                    PlanObjectives = model.PlanObjectives,
                    PlanDurationMinutes = model.PlanDurationMinutes,
                    Status = model.TeacherId.HasValue ? "AssignedToTeacher" : "Pending",
                    CreatedAt = DateTime.UtcNow,
                    AssignedAt = model.TeacherId.HasValue ? (DateTime?)DateTime.UtcNow : null
                };

                db.ExamRequests.Add(request);
                db.SaveChanges();

                // Send email notification if a teacher was assigned at creation time
                if (model.TeacherId.HasValue)
                {
                    var teacher = db.Users.Find(model.TeacherId.Value);
                    var student = db.Users.Find(model.StudentId);
                    if (teacher != null && !string.IsNullOrWhiteSpace(teacher.Email))
                    {
                        var emailSvc = new EmailService();
                        var baseUrl = Request.Url.GetLeftPart(System.UriPartial.Authority);
                        emailSvc.NotifyTeacherAssigned(
                            teacher.Email,
                            teacher.Name,
                            model.Title,
                            student != null ? student.Name : "غير معروف",
                            request.Id,
                            baseUrl);
                    }
                }
            }

            TempData["Success"] = "تم إنشاء طلب الاختبار بنجاح";
            return RedirectToAction("ExamPanel", new { userId = model.AdminId });
        }

        // POST: Admin/AssignTeacherToRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignTeacherToRequest(AssignTeacherToRequestViewModel model)
        {
            using (var db = new Maeen1_NewDbContext())
            {
                var request = db.ExamRequests.Find(model.RequestId);
                if (request == null)
                {
                    TempData["Error"] = "الطلب غير موجود";
                    return RedirectToAction("ExamPanel", new { userId = model.AdminId });
                }

                request.TeacherId = model.TeacherId;
                request.Status = "AssignedToTeacher";
                request.AssignedAt = DateTime.UtcNow;
                db.SaveChanges();

                // Send email notification to the newly assigned teacher
                var teacher = db.Users.Find(model.TeacherId);
                var student = db.Users.Find(request.StudentId);
                if (teacher != null && !string.IsNullOrWhiteSpace(teacher.Email))
                {
                    var emailSvc = new EmailService();
                    var baseUrl = Request.Url.GetLeftPart(System.UriPartial.Authority);
                    emailSvc.NotifyTeacherAssigned(
                        teacher.Email,
                        teacher.Name,
                        request.Title,
                        student != null ? student.Name : "غير معروف",
                        request.Id,
                        baseUrl);
                }
            }

            TempData["Success"] = "تم تعيين المعلم للطلب بنجاح";
            return RedirectToAction("ExamPanel", new { userId = model.AdminId });
        }

        // POST: Admin/DeleteExamRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteExamRequest(int requestId, int adminId)
        {
            using (var db = new Maeen1_NewDbContext())
            {
                var request = db.ExamRequests
                    .Include(r => r.Results)
                    .FirstOrDefault(r => r.Id == requestId);

                if (request != null)
                {
                    db.ExamResults.RemoveRange(request.Results);
                    db.ExamRequests.Remove(request);
                    db.SaveChanges();
                }
            }

            TempData["Success"] = "تم حذف الطلب";
            return RedirectToAction("ExamPanel", new { userId = adminId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SyncLegacyStudentAnswers(int adminId)
        {
            using (var db = new Maeen1_NewDbContext())
            {
                var admin = db.Users.FirstOrDefault(u => u.Id == adminId && u.Role == "Admin");
                if (admin == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var requests = db.ExamRequests
                    .OrderByDescending(r => r.CreatedAt)
                    .ToList();

                var studentIds = requests.Select(r => r.StudentId).Distinct().ToList();
                var profilesByStudent = db.StudentOnboardingProfiles
                    .Where(p => studentIds.Contains(p.UserId))
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList()
                    .GroupBy(p => p.UserId)
                    .ToDictionary(g => g.Key, g => g.First());

                var updatedCount = 0;
                foreach (var request in requests)
                {
                    StudentOnboardingProfile profile;
                    if (!profilesByStudent.TryGetValue(request.StudentId, out profile) || profile == null)
                    {
                        continue;
                    }

                    var level = string.IsNullOrWhiteSpace(profile.DeterminedLevel) ? "غير محدد" : profile.DeterminedLevel.Trim();
                    var targetNames = ExtractPlanLineValue(profile.MemorizationPlan, "الأجزاء المستهدفة بالاسم:") ?? string.Empty;
                    var completedNames = ExtractPlanLineValue(profile.MemorizationPlan, "الأجزاء المحفوظة مسبقًا بالاسم:") ?? string.Empty;
                    var tajweed = profile.TajweedLevel ?? string.Empty;
                    var duration = profile.TargetDuration ?? string.Empty;

                    var normalizedDescription =
                        "طلب اختبار تلقائي بعد إكمال تحديد المستوى.\n" +
                        "المستوى: " + level +
                        "\nساعات الحفظ اليومية: " + profile.DailyMemorizationHours +
                        "\nالأجزاء المستهدفة: " + profile.TargetJuzCount +
                        "\nأسماء الأجزاء المستهدفة: " + targetNames +
                        "\nالأجزاء المحفوظة مسبقًا: " + profile.CompletedJuzCount +
                        "\nأسماء الأجزاء المحفوظة مسبقًا: " + completedNames +
                        "\nمستوى التجويد: " + tajweed +
                        "\nالمدة الزمنية المستهدفة: " + duration;

                    var currentDescription = request.Description ?? string.Empty;
                    if (!string.Equals(currentDescription.Trim(), normalizedDescription.Trim(), StringComparison.Ordinal))
                    {
                        request.Description = normalizedDescription;
                        updatedCount++;
                    }
                }

                if (updatedCount > 0)
                {
                    db.SaveChanges();
                }

                TempData["Success"] = "تمت مزامنة بيانات الإجابات للطلبات القديمة. عدد الطلبات المحدثة: " + updatedCount;
                return RedirectToAction("ExamPanel", new { userId = adminId });
            }
        }

        private static string ExtractPlanLineValue(string plan, string prefix)
        {
            if (string.IsNullOrWhiteSpace(plan) || string.IsNullOrWhiteSpace(prefix))
            {
                return null;
            }

            var lines = plan.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return line.Substring(prefix.Length).Trim();
                }
            }

            return null;
        }
    }
}
