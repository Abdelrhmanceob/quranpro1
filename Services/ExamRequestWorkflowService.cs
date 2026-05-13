using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Maeen1_New.Services
{
    /// <summary>
    /// Service for managing exam requests workflow with Supabase
    /// Handles student requests, admin review, teacher assignment, and notifications
    /// </summary>
    public class ExamRequestWorkflowService
    {
        private readonly SupabaseClient _supabaseClient;

        public ExamRequestWorkflowService(SupabaseClient supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        /// <summary>
        /// Student creates a new exam request
        /// </summary>
        public async Task<JObject> CreateExamRequestAsync(
            int studentId,
            string surahName,
            string ayahRange,
            string difficultyLevel,
            string memorizationLevel,
            string tajweedWeaknesses,
            string studentNotes,
            JObject submittedAnswers)
        {
            try
            {
                var examRequest = new JObject
                {
                    { "student_id", studentId },
                    { "surah_name", surahName },
                    { "ayah_range", ayahRange },
                    { "difficulty_level", difficultyLevel },
                    { "memorization_level", memorizationLevel },
                    { "tajweed_weaknesses", tajweedWeaknesses },
                    { "student_notes", studentNotes },
                    { "submitted_answers", submittedAnswers },
                    { "status", "pending" },
                    { "created_at", DateTime.UtcNow.ToString("O") },
                    { "submitted_at", DateTime.UtcNow.ToString("O") }
                };

                // Insert into Supabase
                var result = await _supabaseClient.InsertAsync("exam_requests", examRequest);

                // Log workflow change
                await LogWorkflowChangeAsync(
                    result["id"].Value<int>(),
                    null,
                    "pending",
                    studentId,
                    "student",
                    "Student submitted exam request"
                );

                // Create notification for admins
                await NotifyAdminsAsync(
                    $"نموذج طلب اختبار جديد من الطالب",
                    $"الطالب {studentId} قدم طلب اختبار جديد لسورة {surahName}",
                    result
                );

                return new JObject
                {
                    { "success", true },
                    { "message", "تم إنشاء طلب الاختبار بنجاح" },
                    { "exam_request_id", result["id"] },
                    { "status", "pending" }
                };
            }
            catch (Exception ex)
            {
                return new JObject
                {
                    { "success", false },
                    { "error", "خطأ في إنشاء طلب الاختبار" },
                    { "message", ex.Message }
                };
            }
        }

        /// <summary>
        /// Admin reviews and approves/rejects exam request
        /// </summary>
        public async Task<JObject> ReviewExamRequestAsync(
            int examRequestId,
            int adminId,
            string action, // "approve" or "reject"
            string adminComments)
        {
            try
            {
                var newStatus = action == "approve" ? "approved" : "rejected";
                var updateData = new JObject
                {
                    { "status", newStatus },
                    { "admin_id", adminId },
                    { "admin_comments", adminComments },
                    { "reviewed_at", DateTime.UtcNow.ToString("O") }
                };

                if (action == "approve")
                {
                    updateData["approved_at"] = DateTime.UtcNow.ToString("O");
                }

                // Update exam request
                await _supabaseClient.UpdateAsync("exam_requests", examRequestId, updateData);

                // Get exam request details
                var examRequest = await _supabaseClient.GetAsync("exam_requests", examRequestId);

                // Log workflow change
                await LogWorkflowChangeAsync(
                    examRequestId,
                    "pending",
                    newStatus,
                    adminId,
                    "admin",
                    $"Admin {action}ed the exam request"
                );

                // Notify student
                await NotifyStudentAsync(
                    examRequest["student_id"].Value<int>(),
                    action == "approve" ? "تم الموافقة على طلب الاختبار" : "تم رفض طلب الاختبار",
                    action == "approve" 
                        ? $"تمت الموافقة على طلب اختبار سورة {examRequest["surah_name"]} - في انتظار تعيين المعلم"
                        : $"تم رفض طلب اختبار سورة {examRequest["surah_name"]}. السبب: {adminComments}",
                    examRequest
                );

                return new JObject
                {
                    { "success", true },
                    { "message", $"تم {(action == "approve" ? "الموافقة على" : "رفض")} طلب الاختبار",
                    { "exam_request_id", examRequestId },
                    { "status", newStatus }
                };
            }
            catch (Exception ex)
            {
                return new JObject
                {
                    { "success", false },
                    { "error", "خطأ في مراجعة طلب الاختبار" },
                    { "message", ex.Message }
                };
            }
        }

        /// <summary>
        /// Admin assigns exam request to a teacher
        /// </summary>
        public async Task<JObject> AssignTeacherAsync(
            int examRequestId,
            int teacherId,
            int adminId,
            string assignmentNotes)
        {
            try
            {
                // Create teacher assignment
                var assignment = new JObject
                {
                    { "exam_request_id", examRequestId },
                    { "teacher_id", teacherId },
                    { "assigned_by_admin_id", adminId },
                    { "assignment_notes", assignmentNotes },
                    { "assigned_at", DateTime.UtcNow.ToString("O") }
                };

                var assignmentResult = await _supabaseClient.InsertAsync("teacher_assignments", assignment);

                // Update exam request status
                var updateData = new JObject
                {
                    { "status", "assigned" },
                    { "teacher_id", teacherId },
                    { "assigned_at", DateTime.UtcNow.ToString("O") }
                };

                await _supabaseClient.UpdateAsync("exam_requests", examRequestId, updateData);

                // Get exam request details
                var examRequest = await _supabaseClient.GetAsync("exam_requests", examRequestId);

                // Log workflow change
                await LogWorkflowChangeAsync(
                    examRequestId,
                    "approved",
                    "assigned",
                    adminId,
                    "admin",
                    $"Assigned to teacher {teacherId}"
                );

                // Notify teacher
                await NotifyTeacherAsync(
                    teacherId,
                    "تم تعيينك لمراجعة طلب اختبار",
                    $"تم تعيينك لمراجعة طلب اختبار الطالب {examRequest["student_id"]} لسورة {examRequest["surah_name"]}",
                    examRequest
                );

                // Notify student
                await NotifyStudentAsync(
                    examRequest["student_id"].Value<int>(),
                    "تم تعيين معلمك",
                    $"تم تعيين معلم لمراجعة طلب اختبار سورة {examRequest["surah_name"]}",
                    examRequest
                );

                return new JObject
                {
                    { "success", true },
                    { "message", "تم تعيين المعلم بنجاح" },
                    { "exam_request_id", examRequestId },
                    { "teacher_id", teacherId },
                    { "status", "assigned" }
                };
            }
            catch (Exception ex)
            {
                return new JObject
                {
                    { "success", false },
                    { "error", "خطأ في تعيين المعلم" },
                    { "message", ex.Message }
                };
            }
        }

        /// <summary>
        /// Teacher creates personalized exam plan
        /// </summary>
        public async Task<JObject> CreateExamPlanAsync(
            int examRequestId,
            int teacherId,
            JObject testPlan,
            JObject tajweedFocus,
            JObject memorizationTasks,
            string preparationNotes,
            string motivationalGuidance,
            JObject difficultyAdjustments)
        {
            try
            {
                var examPlan = new JObject
                {
                    { "exam_request_id", examRequestId },
                    { "teacher_id", teacherId },
                    { "test_plan", testPlan },
                    { "tajweed_focus", tajweedFocus },
                    { "memorization_tasks", memorizationTasks },
                    { "preparation_notes", preparationNotes },
                    { "motivational_guidance", motivationalGuidance },
                    { "difficulty_adjustments", difficultyAdjustments },
                    { "created_at", DateTime.UtcNow.ToString("O") }
                };

                var result = await _supabaseClient.InsertAsync("exam_plans", examPlan);

                // Update exam request with generated plan
                var updateData = new JObject
                {
                    { "generated_test_plan", testPlan },
                    { "success_roadmap", new JObject
                        {
                            { "tajweed_focus", tajweedFocus },
                            { "memorization_tasks", memorizationTasks },
                            { "preparation_notes", preparationNotes },
                            { "motivational_guidance", motivationalGuidance }
                        }
                    }
                };

                await _supabaseClient.UpdateAsync("exam_requests", examRequestId, updateData);

                // Get exam request details
                var examRequest = await _supabaseClient.GetAsync("exam_requests", examRequestId);

                // Notify student
                await NotifyStudentAsync(
                    examRequest["student_id"].Value<int>(),
                    "خطة الاختبار جاهزة",
                    $"قام معلمك بإعداد خطة اختبار شاملة لسورة {examRequest["surah_name"]}. يمكنك الآن البدء في التحضير",
                    examRequest
                );

                return new JObject
                {
                    { "success", true },
                    { "message", "تم إنشاء خطة الاختبار بنجاح" },
                    { "exam_plan_id", result["id"] },
                    { "exam_request_id", examRequestId }
                };
            }
            catch (Exception ex)
            {
                return new JObject
                {
                    { "success", false },
                    { "error", "خطأ في إنشاء خطة الاختبار" },
                    { "message", ex.Message }
                };
            }
        }

        /// <summary>
        /// Get exam request with full details
        /// </summary>
        public async Task<JObject> GetExamRequestAsync(int examRequestId)
        {
            try
            {
                var examRequest = await _supabaseClient.GetAsync("exam_requests", examRequestId);
                
                // Get teacher assignment if exists
                var assignment = await _supabaseClient.QueryAsync(
                    "teacher_assignments",
                    new { exam_request_id = examRequestId }
                );

                // Get exam plan if exists
                var examPlan = await _supabaseClient.QueryAsync(
                    "exam_plans",
                    new { exam_request_id = examRequestId }
                );

                examRequest["teacher_assignment"] = assignment.Count > 0 ? assignment[0] : null;
                examRequest["exam_plan"] = examPlan.Count > 0 ? examPlan[0] : null;

                return examRequest;
            }
            catch (Exception ex)
            {
                return new JObject
                {
                    { "error", "خطأ في جلب تفاصيل الطلب" },
                    { "message", ex.Message }
                };
            }
        }

        /// <summary>
        /// Get all exam requests for admin
        /// </summary>
        public async Task<JArray> GetAllExamRequestsAsync(string status = null)
        {
            try
            {
                var query = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(status))
                {
                    query["status"] = status;
                }

                var results = await _supabaseClient.QueryAsync("exam_requests", query);
                return new JArray(results);
            }
            catch (Exception ex)
            {
                return new JArray();
            }
        }

        /// <summary>
        /// Get exam requests for a specific student
        /// </summary>
        public async Task<JArray> GetStudentExamRequestsAsync(int studentId)
        {
            try
            {
                var results = await _supabaseClient.QueryAsync(
                    "exam_requests",
                    new { student_id = studentId }
                );
                return new JArray(results);
            }
            catch (Exception ex)
            {
                return new JArray();
            }
        }

        /// <summary>
        /// Get exam requests assigned to a teacher
        /// </summary>
        public async Task<JArray> GetTeacherExamRequestsAsync(int teacherId)
        {
            try
            {
                var results = await _supabaseClient.QueryAsync(
                    "exam_requests",
                    new { teacher_id = teacherId }
                );
                return new JArray(results);
            }
            catch (Exception ex)
            {
                return new JArray();
            }
        }

        /// <summary>
        /// Log workflow status change
        /// </summary>
        private async Task LogWorkflowChangeAsync(
            int examRequestId,
            string previousStatus,
            string newStatus,
            int changedByUserId,
            string changedByRole,
            string reason)
        {
            try
            {
                var log = new JObject
                {
                    { "exam_request_id", examRequestId },
                    { "previous_status", previousStatus },
                    { "new_status", newStatus },
                    { "changed_by_user_id", changedByUserId },
                    { "changed_by_role", changedByRole },
                    { "reason", reason },
                    { "changed_at", DateTime.UtcNow.ToString("O") }
                };

                await _supabaseClient.InsertAsync("workflow_logs", log);
            }
            catch (Exception ex)
            {
                // Log error but don't throw
                System.Diagnostics.Debug.WriteLine($"Error logging workflow change: {ex.Message}");
            }
        }

        /// <summary>
        /// Notify admins about new exam request
        /// </summary>
        private async Task NotifyAdminsAsync(string title, string message, JObject data)
        {
            try
            {
                // Get all admin users
                var admins = await _supabaseClient.QueryAsync(
                    "auth.users",
                    new { role = "admin" }
                );

                foreach (var admin in admins)
                {
                    var notification = new JObject
                    {
                        { "user_id", admin["id"] },
                        { "notification_type", "exam_request_pending" },
                        { "title", title },
                        { "message", message },
                        { "data", data },
                        { "created_at", DateTime.UtcNow.ToString("O") }
                    };

                    await _supabaseClient.InsertAsync("notifications", notification);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error notifying admins: {ex.Message}");
            }
        }

        /// <summary>
        /// Notify student about status change
        /// </summary>
        private async Task NotifyStudentAsync(int studentId, string title, string message, JObject data)
        {
            try
            {
                var notification = new JObject
                {
                    { "user_id", studentId },
                    { "notification_type", "exam_request_update" },
                    { "title", title },
                    { "message", message },
                    { "data", data },
                    { "created_at", DateTime.UtcNow.ToString("O") }
                };

                await _supabaseClient.InsertAsync("notifications", notification);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error notifying student: {ex.Message}");
            }
        }

        /// <summary>
        /// Notify teacher about assignment
        /// </summary>
        private async Task NotifyTeacherAsync(int teacherId, string title, string message, JObject data)
        {
            try
            {
                var notification = new JObject
                {
                    { "user_id", teacherId },
                    { "notification_type", "exam_request_assigned" },
                    { "title", title },
                    { "message", message },
                    { "data", data },
                    { "created_at", DateTime.UtcNow.ToString("O") }
                };

                await _supabaseClient.InsertAsync("notifications", notification);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error notifying teacher: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Supabase client wrapper for database operations
    /// </summary>
    public class SupabaseClient
    {
        private readonly string _supabaseUrl;
        private readonly string _supabaseKey;

        public SupabaseClient(string supabaseUrl, string supabaseKey)
        {
            _supabaseUrl = supabaseUrl;
            _supabaseKey = supabaseKey;
        }

        public async Task<JObject> InsertAsync(string table, JObject data)
        {
            // Implementation would use Supabase REST API
            // For now, returning mock response
            return new JObject { { "id", 1 } };
        }

        public async Task<JObject> UpdateAsync(string table, int id, JObject data)
        {
            // Implementation would use Supabase REST API
            return new JObject { { "success", true } };
        }

        public async Task<JObject> GetAsync(string table, int id)
        {
            // Implementation would use Supabase REST API
            return new JObject();
        }

        public async Task<List<JObject>> QueryAsync(string table, object filters)
        {
            // Implementation would use Supabase REST API
            return new List<JObject>();
        }
    }
}
