using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.IO;
using Maeen1_New.Models;
using Maeen1_New.Models.ViewModels;
using Maeen1_New.Services;

namespace Maeen1_New.Controllers
{
    public class TeacherController : Controller
    {
        private readonly Maeen1_NewDbContext _db = new Maeen1_NewDbContext();
        private readonly TeacherNotificationService _notificationService = new TeacherNotificationService();
        private readonly AdminNotificationService _adminNotificationService = new AdminNotificationService();

        // GET: Teacher
        public ActionResult Dashboard(int? userId)
        {
            if (!userId.HasValue)
            {
                var fallbackTeacher = _db.Users.FirstOrDefault(u => u.Role == "Teacher");
                if (fallbackTeacher == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                userId = fallbackTeacher.Id;
            }

            var teacher = _db.Users.FirstOrDefault(u => u.Id == userId.Value && u.Role == "Teacher");
            if (teacher == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var teacherExamIds = _db.Exams
                .Where(e => e.TeacherId == teacher.Id)
                .Select(e => e.Id)
                .ToList();

            var teacherTasks = _db.ExamTasks
                .Include(t => t.Exam)
                .Where(t => t.Exam.TeacherId == teacher.Id)
                .ToList();

            var taskIds = teacherTasks.Select(t => t.Id).ToList();
            var completions = _db.StudentTaskCompletions
                .Where(c => taskIds.Contains(c.TaskId))
                .ToList();

            var notifications = _notificationService.GetTeacherNotifications(_db, teacher.Id, 20);

            var examRequests = _db.ExamRequests
                .Include(r => r.Student)
                .Where(r => r.TeacherId == teacher.Id)
                .ToList();

            var recitationRequests = _db.RecitationRequests
                .Where(r => r.TeacherId == teacher.Id)
                .ToList();

            var latestExamPerStudent = _db.ExamRequests
                .Include(r => r.Student)
                .Where(r => r.TeacherId == teacher.Id)
                .ToList()
                .GroupBy(r => r.StudentId)
                .Select(g => g.OrderByDescending(x => x.CreatedAt).FirstOrDefault())
                .ToList();

            var model = new TeacherDashboardViewModel
            {
                TeacherId = teacher.Id,
                TeacherName = teacher.Name,
                TotalStudents = latestExamPerStudent.Select(r => r.StudentId).Distinct().Count(),
                PendingExamRequests = examRequests.Count(r => r.Status == "AssignedToTeacher"),
                CompletedExamRequests = examRequests.Count(r => r.Status == "ResultSent"),
                TotalExams = teacherExamIds.Count,
                PendingApprovals = completions.Count(c => !c.IsApproved),
                PendingRecitationRequests = recitationRequests.Count(r => r.Status == "Pending"),
                ReadyRecitationSessions = recitationRequests.Count(r => r.Status == "SessionReady"),
                Notifications = notifications.ToList(),
                Students = latestExamPerStudent
                    .Where(r => r != null)
                    .Select(r =>
                    {
                        var studentTasks = teacherTasks.Where(t => t.StudentId == r.StudentId).ToList();
                        var studentTaskIds = studentTasks.Select(t => t.Id).ToList();
                        var studentCompletions = completions.Where(c => studentTaskIds.Contains(c.TaskId)).ToList();
                        return new TeacherDashboardStudentRow
                        {
                            StudentId = r.StudentId,
                            StudentName = r.Student != null ? r.Student.Name : "غير معروف",
                            LatestExamTitle = r.Title,
                            Status = r.Status,
                            CompletedTasks = studentCompletions.Count(c => c.IsApproved),
                            TotalTasks = studentTasks.Count
                        };
                    })
                    .OrderBy(s => s.StudentName)
                    .ToList()
            };

            return View(model);
        }

        // GET: Teacher/ExamManagement
        public ActionResult ExamManagement(int? userId)
        {
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var teacher = _db.Users.FirstOrDefault(u => u.Id == userId.Value && u.Role == "Teacher");
            if (teacher == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var exams = _db.Exams
                .Include(e => e.Tasks)
                .Where(e => e.TeacherId == userId.Value)
                .OrderByDescending(e => e.CreatedAt)
                .ToList();

            var pendingApprovals = _db.StudentTaskCompletions
                .Include(c => c.Task)
                .Include(c => c.Task.Exam)
                .Include(c => c.Student)
                .Where(c => !c.IsApproved && c.Task.Exam.TeacherId == userId.Value)
                .OrderByDescending(c => c.CompletedAt)
                .ToList();

            var students = _db.Users
                .Where(u => u.Role == "Student")
                .OrderBy(u => u.Name)
                .ToList();

            var model = new TeacherExamDashboardViewModel
            {
                TeacherId = userId.Value,
                Exams = exams.Select(e => new ExamWithTasksViewModel
                {
                    ExamId = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    CreatedAt = e.CreatedAt,
                    IsActive = e.IsActive,
                    Tasks = e.Tasks.Select(t => new TaskViewModel
                    {
                        TaskId = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        StudentName = _db.Users.Where(u => u.Id == t.StudentId).Select(u => u.Name).FirstOrDefault() ?? "غير معروف",
                        StudentId = t.StudentId,
                        AssignedAt = t.AssignedAt,
                        DueDate = t.DueDate,
                        IsRequired = t.IsRequired,
                        IsCompleted = t.Completions != null && t.Completions.Any(),
                        IsApproved = t.Completions != null && t.Completions.Any(c => c.IsApproved)
                    }).ToList()
                }).ToList(),
                PendingApprovals = pendingApprovals.Select(c => new StudentTaskCompletionReviewViewModel
                {
                    CompletionId = c.Id,
                    StudentName = c.Student.Name,
                    TaskTitle = c.Task.Title,
                    ExamTitle = c.Task.Exam.Title,
                    Notes = c.Notes,
                    CompletedAt = c.CompletedAt
                }).ToList(),
                Students = students.Select(s => new UserBasicViewModel
                {
                    Id = s.Id,
                    Name = s.Name
                }).ToList()
            };

            return View(model);
        }

        // POST: Teacher/CreateExam
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateExam(CreateExamViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("ExamManagement", new { userId = model.TeacherId });
            }

            var exam = new Exam
            {
                Title = model.Title,
                Description = model.Description,
                TeacherId = model.TeacherId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _db.Exams.Add(exam);
            _db.SaveChanges();

            TempData["Success"] = "تم إنشاء الاختبار بنجاح";
            return RedirectToAction("ExamManagement", new { userId = model.TeacherId });
        }

        // POST: Teacher/AssignTask
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignTask(AssignTaskViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "يرجى ملء جميع الحقول المطلوبة";
                return RedirectToAction("ExamManagement", new { userId = model.TeacherId });
            }

            var task = new ExamTask
            {
                ExamId = model.ExamId,
                StudentId = model.StudentId,
                Title = model.Title,
                Description = model.Description,
                AssignedAt = DateTime.UtcNow,
                DueDate = model.DueDate,
                IsRequired = model.IsRequired
            };

            _db.ExamTasks.Add(task);
            _db.SaveChanges();

            TempData["Success"] = "تم تعيين المهمة للطالب بنجاح";
            return RedirectToAction("ExamManagement", new { userId = model.TeacherId });
        }

        // POST: Teacher/ApproveTask
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveTask(int completionId, int teacherId)
        {
            var completion = _db.StudentTaskCompletions.Find(completionId);
            if (completion == null)
            {
                TempData["Error"] = "لم يتم العثور على المهمة";
                return RedirectToAction("ExamManagement", new { userId = teacherId });
            }

            completion.IsApproved = true;
            completion.ApprovedByTeacherId = teacherId;
            completion.ApprovedAt = DateTime.UtcNow;
            _db.SaveChanges();

            TempData["Success"] = "تمت الموافقة على إنجاز المهمة";
            return RedirectToAction("ExamManagement", new { userId = teacherId });
        }

        // POST: Teacher/RejectTask
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RejectTask(int completionId, int teacherId)
        {
            var completion = _db.StudentTaskCompletions.Find(completionId);
            if (completion == null)
            {
                TempData["Error"] = "لم يتم العثور على المهمة";
                return RedirectToAction("ExamManagement", new { userId = teacherId });
            }

            _db.StudentTaskCompletions.Remove(completion);
            _db.SaveChanges();

            TempData["Success"] = "تم رفض الإنجاز - يمكن للطالب إعادة التقديم";
            return RedirectToAction("ExamManagement", new { userId = teacherId });
        }

        // =============================================
        // GET: Teacher/ExamRequests
        // Shows all exam requests assigned to this teacher, with the admin's plan
        // =============================================
        public ActionResult ExamRequests(int? userId)
        {
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var teacher = _db.Users.FirstOrDefault(u => u.Id == userId.Value && u.Role == "Teacher");
            if (teacher == null)
                return RedirectToAction("Login", "Account");

            ViewBag.TeacherId = userId.Value;
            ViewBag.TeacherName = teacher.Name;

            var allTeacherRequests = _db.ExamRequests
                .Include(r => r.Student)
                .Include(r => r.Results)
                .Where(r => r.TeacherId == userId.Value)
                .OrderByDescending(r => r.AssignedAt ?? r.CreatedAt)
                .ToList();

            var completed = allTeacherRequests
                .Where(r => string.Equals(r.Status, "ResultSent", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.ResultSentAt ?? r.AssignedAt ?? r.CreatedAt)
                .ToList();

            var assigned = allTeacherRequests
                .Where(r => !string.Equals(r.Status, "ResultSent", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.AssignedAt ?? r.CreatedAt)
                .ToList();

            // Defensive de-duplication in case joined rows materialize
            // repeated ExamRequest instances.
            assigned = assigned
                .GroupBy(r => r.Id)
                .Select(g => g.First())
                .ToList();

            completed = completed
                .GroupBy(r => r.Id)
                .Select(g => g.First())
                .ToList();

            var model = new TeacherExamRequestsViewModel
            {
                TeacherId = userId.Value,
                AssignedRequests = assigned.Select(r => BuildTeacherRequestItem(r)).ToList(),
                CompletedRequests = completed.Select(r => BuildTeacherRequestItem(r)).ToList()
            };

            return View(model);
        }

        // POST: Teacher/SendExamResult
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateTeacherDecision(int requestId, int teacherId, string decisionStatus, string decisionNotes)
        {
            var request = _db.ExamRequests.FirstOrDefault(r => r.Id == requestId);
            if (request == null)
            {
                TempData["Error"] = "الطلب غير موجود";
                return RedirectToAction("ExamRequests", new { userId = teacherId });
            }

            if (!request.TeacherId.HasValue || request.TeacherId.Value != teacherId)
            {
                TempData["Error"] = "غير مسموح: هذا الطلب ليس مخصصًا لهذا المعلم";
                return RedirectToAction("ExamRequests", new { userId = teacherId });
            }

            var normalizedDecision = NormalizeTeacherDecision(decisionStatus);
            if (string.IsNullOrWhiteSpace(normalizedDecision))
            {
                TempData["Error"] = "يرجى اختيار قرار صحيح";
                return RedirectToAction("ExamRequests", new { userId = teacherId });
            }

            request.Status = normalizedDecision;
            if (!request.AssignedAt.HasValue)
            {
                request.AssignedAt = DateTime.UtcNow;
            }

            var teacher = _db.Users.Find(teacherId);
            var decisionLabel = GetTeacherDecisionLabel(normalizedDecision);

            _db.SaveChanges();

            _adminNotificationService.CreateTeacherDecisionNotifications(
                _db,
                teacherId,
                teacher != null ? teacher.Name : "معلم",
                request.Id,
                request.Title,
                normalizedDecision,
                decisionLabel,
                decisionNotes);

            TempData["Success"] = "تم تحديث القرار وإرسال إشعار للأدمن";
            return RedirectToAction("ExamRequests", new { userId = teacherId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendExamResult(SendExamResultViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "يرجى ملء جميع الحقول المطلوبة";
                return RedirectToAction("ExamRequests", new { userId = model.TeacherId });
            }

            var request = _db.ExamRequests.Find(model.RequestId);
            if (request == null)
            {
                TempData["Error"] = "الطلب غير موجود";
                return RedirectToAction("ExamRequests", new { userId = model.TeacherId });
            }

            if (request.TeacherId != model.TeacherId)
            {
                TempData["Error"] = "غير مسموح: هذا الطلب ليس مخصصًا لهذا المعلم";
                return RedirectToAction("ExamRequests", new { userId = model.TeacherId });
            }

            if (!string.Equals(request.Status, "AssignedToTeacher", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(request.Status, "TeacherApproved", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "لا يمكن إرسال نتيجة لهذا الطلب في حالته الحالية";
                return RedirectToAction("ExamRequests", new { userId = model.TeacherId });
            }

            if (request.StudentId != model.StudentId)
            {
                model.StudentId = request.StudentId;
            }

            var existingResult = _db.ExamResults
                .OrderByDescending(r => r.SentAt)
                .FirstOrDefault(r => r.ExamRequestId == model.RequestId);
            if (existingResult != null)
            {
                TempData["Error"] = "تم إرسال نتيجة هذا الطلب مسبقًا";
                return RedirectToAction("ExamRequests", new { userId = model.TeacherId });
            }

            var result = new ExamResult
            {
                ExamRequestId = model.RequestId,
                TeacherId = model.TeacherId,
                StudentId = model.StudentId,
                Score = model.Score,
                MaxScore = model.MaxScore,
                Grade = model.Grade,
                TeacherNotes = model.TeacherNotes,
                Strengths = model.Strengths,
                Weaknesses = model.Weaknesses,
                Recommendations = model.Recommendations,
                SentAt = DateTime.UtcNow
            };

            _db.ExamResults.Add(result);

            request.Status = "ResultSent";
            request.ResultSentAt = DateTime.UtcNow;
            _db.SaveChanges();

            // Send confirmation email to the teacher
            var teacher = _db.Users.Find(model.TeacherId);
            var student = _db.Users.Find(model.StudentId);
            if (teacher != null && !string.IsNullOrWhiteSpace(teacher.Email))
            {
                var emailSvc = new EmailService();
                emailSvc.NotifyTeacherResultReceived(
                    teacher.Email,
                    teacher.Name,
                    request.Title,
                    student != null ? student.Name : "غير معروف",
                    model.Grade);
            }

            TempData["Success"] = "تم إرسال النتيجة بنجاح";
            return RedirectToAction("ExamRequests", new { userId = model.TeacherId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendMemorizationPlan(SendMemorizationPlanViewModel model)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Recommendations))
            {
                TempData["Error"] = "يرجى كتابة خطة الحفظ قبل الإرسال";
                return RedirectToAction("ExamRequests", new { userId = model.TeacherId });
            }

            var request = _db.ExamRequests.Find(model.RequestId);
            if (request == null)
            {
                TempData["Error"] = "الطلب غير موجود";
                return RedirectToAction("ExamRequests", new { userId = model.TeacherId });
            }

            if (!request.TeacherId.HasValue || request.TeacherId.Value != model.TeacherId)
            {
                TempData["Error"] = "غير مسموح: هذا الطلب ليس مخصصًا لهذا المعلم";
                return RedirectToAction("ExamRequests", new { userId = model.TeacherId });
            }

            var latestResult = _db.ExamResults
                .Where(r => r.ExamRequestId == model.RequestId)
                .OrderByDescending(r => r.SentAt)
                .FirstOrDefault();

            if (latestResult == null)
            {
                latestResult = new ExamResult
                {
                    ExamRequestId = model.RequestId,
                    TeacherId = model.TeacherId,
                    StudentId = request.StudentId,
                    SentAt = DateTime.UtcNow,
                    Grade = "خطة حفظ"
                };
                _db.ExamResults.Add(latestResult);
            }

            latestResult.Recommendations = model.Recommendations.Trim();
            if (!string.IsNullOrWhiteSpace(model.TeacherNotes))
            {
                latestResult.TeacherNotes = model.TeacherNotes.Trim();
            }
            latestResult.SentAt = DateTime.UtcNow;

            if (!string.Equals(request.Status, "ResultSent", StringComparison.OrdinalIgnoreCase))
            {
                request.Status = "TeacherApproved";
            }

            _db.SaveChanges();

            TempData["Success"] = "تم إرسال خطة الحفظ للطالب بنجاح";
            return RedirectToAction("ExamRequests", new { userId = model.TeacherId });
        }

        [AllowAnonymous]
        public ActionResult PendingRecitationRequests(int? userId)
        {
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var teacher = _db.Users.FirstOrDefault(u => u.Id == userId.Value && u.Role == "Teacher");
            if (teacher == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var requests = _db.RecitationRequests
                .Include(r => r.Student)
                .Where(r => r.TeacherId == userId.Value)
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
                    studentVoiceUrls[request.Id] = Url.Action("GetRecitationVoice", "Teacher", new
                    {
                        requestId = request.Id,
                        role = "student",
                        teacherId = userId.Value
                    });
                }

                if (GetExistingRecitationVoiceFile(request.Id, "teacher") != null)
                {
                    teacherVoiceUrls[request.Id] = Url.Action("GetRecitationVoice", "Teacher", new
                    {
                        requestId = request.Id,
                        role = "teacher",
                        teacherId = userId.Value
                    });
                }

                if (GetExistingRecitationScreenFile(request.Id, "student") != null)
                {
                    studentScreenUrls[request.Id] = Url.Action("GetRecitationScreen", "Teacher", new
                    {
                        requestId = request.Id,
                        role = "student",
                        teacherId = userId.Value
                    });
                }

                if (GetExistingRecitationScreenFile(request.Id, "teacher") != null)
                {
                    teacherScreenUrls[request.Id] = Url.Action("GetRecitationScreen", "Teacher", new
                    {
                        requestId = request.Id,
                        role = "teacher",
                        teacherId = userId.Value
                    });
                }
            }

            var linkedStudentsFromRequests = requests
                .Where(r => r.Student != null)
                .GroupBy(r => new { r.StudentId, r.Student.Name })
                .Select(g => new TeacherLinkedStudentSessionViewModel
                {
                    StudentId = g.Key.StudentId,
                    StudentName = g.Key.Name,
                    TotalRecitationRequests = g.Count(),
                    ReadySessions = g.Count(x => x.Session != null && !string.IsNullOrWhiteSpace(x.Session.GoogleMeetUrl))
                })
                .ToList();

            var linkedStudentsFromTasks = _db.ExamTasks
                .Include(t => t.Exam)
                .Where(t => t.Exam.TeacherId == userId.Value)
                .Select(t => t.StudentId)
                .Distinct()
                .ToList();

            foreach (var studentId in linkedStudentsFromTasks)
            {
                if (linkedStudentsFromRequests.Any(s => s.StudentId == studentId))
                {
                    continue;
                }

                var student = _db.Users.FirstOrDefault(u => u.Id == studentId && u.Role == "Student");
                if (student != null)
                {
                    linkedStudentsFromRequests.Add(new TeacherLinkedStudentSessionViewModel
                    {
                        StudentId = student.Id,
                        StudentName = student.Name,
                        TotalRecitationRequests = 0,
                        ReadySessions = 0
                    });
                }
            }

            ViewBag.TeacherId = userId.Value;
            ViewBag.StudentVoiceUrls = studentVoiceUrls;
            ViewBag.TeacherVoiceUrls = teacherVoiceUrls;
            ViewBag.StudentScreenUrls = studentScreenUrls;
            ViewBag.TeacherScreenUrls = teacherScreenUrls;
            ViewBag.LinkedStudents = linkedStudentsFromRequests
                .OrderBy(s => s.StudentName)
                .ToList();
            return View(requests);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult SaveMeetLink(int requestId, int teacherId, string meetUrl)
        {
            var teacher = _db.Users.FirstOrDefault(u => u.Id == teacherId && u.Role == "Teacher");
            if (teacher == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var request = _db.RecitationRequests
                .FirstOrDefault(r => r.Id == requestId && r.TeacherId == teacherId);

            if (request == null)
            {
                TempData["Error"] = "طلب التسميع غير موجود";
                return RedirectToAction("PendingRecitationRequests", new { userId = teacherId });
            }

            if (string.IsNullOrWhiteSpace(meetUrl) || !Uri.IsWellFormedUriString(meetUrl, UriKind.Absolute))
            {
                TempData["Error"] = "رابط Google Meet غير صالح";
                return RedirectToAction("PendingRecitationRequests", new { userId = teacherId });
            }

            var session = _db.RecitationSessions
                .FirstOrDefault(s => s.RecitationRequestId == request.Id);
            if (session == null)
            {
                session = new RecitationSession
                {
                    RecitationRequestId = request.Id,
                    StudentId = request.StudentId,
                    TeacherId = teacherId,
                    GoogleMeetUrl = meetUrl,
                    CreatedAt = DateTime.UtcNow
                };

                _db.RecitationSessions.Add(session);
            }
            else
            {
                session.GoogleMeetUrl = meetUrl;
                session.UpdatedAt = DateTime.UtcNow;
            }

            request.Status = "SessionReady";
            request.UpdatedAt = DateTime.UtcNow;

            _db.SaveChanges();

            TempData["Success"] = "تم حفظ رابط الجلسة بنجاح";
            return RedirectToAction("PendingRecitationRequests", new { userId = teacherId });
        }

        private const string TeacherRecitationEvalMarker = "\n---TEACHER_RECITATION_EVAL---\n";

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitTeacherRecitationEvaluation(
            int requestId,
            int teacherId,
            decimal? score,
            decimal? maxScore,
            string grade,
            string teacherNotes,
            string strengths,
            string weaknesses)
        {
            var teacher = _db.Users.FirstOrDefault(u => u.Id == teacherId && u.Role == "Teacher");
            if (teacher == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var request = _db.RecitationRequests
                .FirstOrDefault(r => r.Id == requestId && r.TeacherId == teacherId);
            if (request == null)
            {
                TempData["Error"] = "طلب التسميع غير موجود";
                return RedirectToAction("PendingRecitationRequests", new { userId = teacherId });
            }

            var noteLines = new List<string>();
            noteLines.Add("📋 تقييم المعلم للتسميع");
            noteLines.Add("الدرجة المحصلة: " + (score.HasValue ? score.Value.ToString("0.##") : "-"));
            noteLines.Add("الدرجة الكاملة: " + (maxScore.HasValue ? maxScore.Value.ToString("0.##") : "-"));
            noteLines.Add("التقدير: " + (string.IsNullOrWhiteSpace(grade) ? "-" : grade.Trim()));
            noteLines.Add("ملاحظات المعلم: " + (string.IsNullOrWhiteSpace(teacherNotes) ? "-" : teacherNotes.Trim()));
            noteLines.Add("نقاط القوة: " + (string.IsNullOrWhiteSpace(strengths) ? "-" : strengths.Trim()));
            noteLines.Add("نقاط الضعف: " + (string.IsNullOrWhiteSpace(weaknesses) ? "-" : weaknesses.Trim()));

            var evalBlock = string.Join("\n", noteLines);
            var existing = request.Notes ?? string.Empty;
            var baseNote = existing;
            var markerIndex = existing.IndexOf(TeacherRecitationEvalMarker, StringComparison.Ordinal);
            if (markerIndex >= 0)
            {
                baseNote = existing.Substring(0, markerIndex);
            }

            request.Notes = (baseNote ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                request.Notes += TeacherRecitationEvalMarker;
            }
            request.Notes += evalBlock;
            request.UpdatedAt = DateTime.UtcNow;
            _db.SaveChanges();

            TempData["Success"] = "تم حفظ تقييم التسميع بنجاح";
            return RedirectToAction("PendingRecitationRequests", new { userId = teacherId });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult UploadTeacherRecitationVoice(int requestId, int teacherId, HttpPostedFileBase voiceFile)
        {
            var teacher = _db.Users.FirstOrDefault(u => u.Id == teacherId && u.Role == "Teacher");
            if (teacher == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var request = _db.RecitationRequests.FirstOrDefault(r => r.Id == requestId && r.TeacherId == teacherId);
            if (request == null)
            {
                TempData["Error"] = "طلب التسميع غير موجود";
                return RedirectToAction("PendingRecitationRequests", new { userId = teacherId });
            }

            if (voiceFile == null || voiceFile.ContentLength <= 0)
            {
                TempData["Error"] = "يرجى اختيار ملف صوتي قبل الرفع";
                return RedirectToAction("PendingRecitationRequests", new { userId = teacherId });
            }

            var extension = (Path.GetExtension(voiceFile.FileName) ?? string.Empty).ToLowerInvariant();
            var allowed = new[] { ".webm", ".wav", ".mp3", ".m4a", ".ogg", ".aac" };
            if (!allowed.Contains(extension))
            {
                TempData["Error"] = "نوع الملف الصوتي غير مدعوم";
                return RedirectToAction("PendingRecitationRequests", new { userId = teacherId });
            }

            var directory = EnsureRecitationVoiceDirectory();
            DeleteExistingRecitationVoiceFiles(requestId, "teacher");
            var path = Path.Combine(directory, $"request_{requestId}_teacher{extension}");
            voiceFile.SaveAs(path);

            request.UpdatedAt = DateTime.UtcNow;
            _db.SaveChanges();

            TempData["Success"] = "تم رفع تسجيل المعلم بنجاح";
            return RedirectToAction("PendingRecitationRequests", new { userId = teacherId });
        }

        [AllowAnonymous]
        public ActionResult GetRecitationVoice(int requestId, string role, int teacherId)
        {
            role = (role ?? string.Empty).Trim().ToLowerInvariant();
            if (role != "student" && role != "teacher")
            {
                return HttpNotFound();
            }

            var request = _db.RecitationRequests.FirstOrDefault(r => r.Id == requestId && r.TeacherId == teacherId);
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
        public ActionResult UploadTeacherRecitationScreen(int requestId, int teacherId, HttpPostedFileBase screenFile)
        {
            var teacher = _db.Users.FirstOrDefault(u => u.Id == teacherId && u.Role == "Teacher");
            if (teacher == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var request = _db.RecitationRequests.FirstOrDefault(r => r.Id == requestId && r.TeacherId == teacherId);
            if (request == null)
            {
                TempData["Error"] = "طلب التسميع غير موجود";
                return RedirectToAction("PendingRecitationRequests", new { userId = teacherId });
            }

            if (screenFile == null || screenFile.ContentLength <= 0)
            {
                TempData["Error"] = "يرجى اختيار ملف تسجيل شاشة";
                return RedirectToAction("PendingRecitationRequests", new { userId = teacherId });
            }

            var extension = (Path.GetExtension(screenFile.FileName) ?? string.Empty).ToLowerInvariant();
            var allowed = new[] { ".webm", ".mp4", ".mov", ".mkv" };
            if (!allowed.Contains(extension))
            {
                TempData["Error"] = "نوع ملف تسجيل الشاشة غير مدعوم";
                return RedirectToAction("PendingRecitationRequests", new { userId = teacherId });
            }

            var directory = EnsureRecitationVoiceDirectory();
            DeleteExistingRecitationScreenFiles(requestId, "teacher");
            var path = Path.Combine(directory, $"request_{requestId}_teacher_screen{extension}");
            screenFile.SaveAs(path);

            request.UpdatedAt = DateTime.UtcNow;
            _db.SaveChanges();

            TempData["Success"] = "تم رفع تسجيل الشاشة من المعلم";
            return RedirectToAction("PendingRecitationRequests", new { userId = teacherId });
        }

        [AllowAnonymous]
        public ActionResult GetRecitationScreen(int requestId, string role, int teacherId)
        {
            role = (role ?? string.Empty).Trim().ToLowerInvariant();
            if (role != "student" && role != "teacher")
            {
                return HttpNotFound();
            }

            var request = _db.RecitationRequests.FirstOrDefault(r => r.Id == requestId && r.TeacherId == teacherId);
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

        private TeacherExamRequestItemViewModel BuildTeacherRequestItem(ExamRequest r)
        {
            var onboarding = _db.StudentOnboardingProfiles
                .Where(p => p.UserId == r.StudentId)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefault();

            var item = new TeacherExamRequestItemViewModel
            {
                RequestId = r.Id,
                Title = r.Title,
                Description = r.Description,
                StudentName = r.Student != null ? r.Student.Name : "غير معروف",
                StudentId = r.StudentId,
                PlanContent = r.PlanContent,
                PlanObjectives = r.PlanObjectives,
                PlanDurationMinutes = r.PlanDurationMinutes,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                AssignedAt = r.AssignedAt,
                ResultSentAt = r.ResultSentAt,
                DailyMemorizationHours = onboarding != null ? (int?)onboarding.DailyMemorizationHours : null,
                TargetJuzCount = onboarding != null ? (int?)onboarding.TargetJuzCount : null,
                CompletedJuzCount = onboarding != null ? (int?)onboarding.CompletedJuzCount : null,
                TajweedLevel = onboarding != null ? onboarding.TajweedLevel : null,
                TargetDuration = onboarding != null ? onboarding.TargetDuration : null,
                DeterminedLevel = onboarding != null ? onboarding.DeterminedLevel : null
            };

            var description = r.Description ?? string.Empty;
            var planText = onboarding != null ? onboarding.MemorizationPlan : null;

            item.TargetJuzNames = FirstNonEmpty(
                ExtractDescriptionLineValue(planText, "الأجزاء المستهدفة بالاسم:"),
                ExtractDescriptionLineValue(description, "الأجزاء المستهدفة بالاسم:"),
                ExtractDescriptionLineValue(description, "أسماء الأجزاء المستهدفة:"));

            item.CompletedJuzNames = FirstNonEmpty(
                ExtractDescriptionLineValue(planText, "الأجزاء المحفوظة مسبقًا بالاسم:"),
                ExtractDescriptionLineValue(description, "الأجزاء المحفوظة مسبقًا بالاسم:"),
                ExtractDescriptionLineValue(description, "أسماء الأجزاء المحفوظة مسبقًا:"));

            if (!item.DailyMemorizationHours.HasValue)
            {
                item.DailyMemorizationHours = ExtractDescriptionLineIntValue(description, "ساعات الحفظ اليومية:");
            }

            if (!item.TargetJuzCount.HasValue)
            {
                item.TargetJuzCount = ExtractDescriptionLineIntValue(description, "الأجزاء المستهدفة:");
            }

            if (!item.CompletedJuzCount.HasValue)
            {
                item.CompletedJuzCount = ExtractDescriptionLineIntValue(description, "الأجزاء المحفوظة مسبقًا:");
            }

            if (string.IsNullOrWhiteSpace(item.TajweedLevel))
            {
                item.TajweedLevel = ExtractDescriptionLineValue(description, "مستوى التجويد:");
            }

            if (string.IsNullOrWhiteSpace(item.TargetDuration))
            {
                item.TargetDuration = ExtractDescriptionLineValue(description, "المدة الزمنية المستهدفة:");
            }

            if (string.IsNullOrWhiteSpace(item.DeterminedLevel))
            {
                item.DeterminedLevel = ExtractDescriptionLineValue(description, "المستوى:");
            }

            var latestResult = r.Results?.OrderByDescending(res => res.SentAt).FirstOrDefault();
            if (latestResult != null)
            {
                item.Score = latestResult.Score;
                item.MaxScore = latestResult.MaxScore;
                item.Grade = latestResult.Grade;
                item.TeacherNotes = latestResult.TeacherNotes;
                item.Recommendations = latestResult.Recommendations;
            }

            item.TeacherDecisionStatus = NormalizeTeacherDecision(r.Status) ?? "TeacherPending";
            item.TeacherDecisionLabel = GetTeacherDecisionLabel(item.TeacherDecisionStatus);

            return item;
        }

        private static string NormalizeTeacherDecision(string status)
        {
            var value = (status ?? string.Empty).Trim();
            if (value.Equals("TeacherApproved", StringComparison.OrdinalIgnoreCase))
            {
                return "TeacherApproved";
            }

            if (value.Equals("TeacherRejected", StringComparison.OrdinalIgnoreCase))
            {
                return "TeacherRejected";
            }

            if (value.Equals("TeacherPending", StringComparison.OrdinalIgnoreCase))
            {
                return "TeacherPending";
            }

            if (value.Equals("AssignedToTeacher", StringComparison.OrdinalIgnoreCase))
            {
                return "TeacherPending";
            }

            return null;
        }

        private static string GetTeacherDecisionLabel(string decisionStatus)
        {
            var value = (decisionStatus ?? string.Empty).Trim();
            if (value.Equals("TeacherApproved", StringComparison.OrdinalIgnoreCase))
            {
                return "موافق";
            }

            if (value.Equals("TeacherRejected", StringComparison.OrdinalIgnoreCase))
            {
                return "مرفوض";
            }

            return "معلّق";
        }

        private static string ExtractDescriptionLineValue(string source, string prefix)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(prefix))
            {
                return null;
            }

            var lines = source.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
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

        private static int? ExtractDescriptionLineIntValue(string source, string prefix)
        {
            var raw = ExtractDescriptionLineValue(source, prefix);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            int parsed;
            return int.TryParse(raw.Trim(), out parsed) ? (int?)parsed : null;
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null || values.Length == 0)
            {
                return null;
            }

            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return null;
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
