# Quran Exam Request Workflow - Implementation Guide

## Quick Start

This guide walks you through setting up and implementing the complete Quran Exam Request Workflow system with Supabase integration.

## Prerequisites

- Visual Studio 2019 or later
- .NET Framework 4.7.2
- Supabase account (free tier available at supabase.com)
- PostgreSQL knowledge (basic)
- ASP.NET MVC 5 experience

## Phase 1: Supabase Setup

### Step 1.1: Create Supabase Project

1. Go to [supabase.com](https://supabase.com)
2. Sign up or log in
3. Click "New Project"
4. Fill in project details:
   - **Name**: `maeen-quran-exams`
   - **Database Password**: Create a strong password
   - **Region**: Choose closest to your location
5. Click "Create new project"
6. Wait for project initialization (5-10 minutes)

### Step 1.2: Get Supabase Credentials

1. Go to Project Settings → API
2. Copy the following:
   - **Project URL**: `https://[project-id].supabase.co`
   - **Anon Key**: Public key for client-side access
   - **Service Role Key**: Secret key for server-side access

### Step 1.3: Run Database Migration

1. In Supabase dashboard, go to SQL Editor
2. Click "New Query"
3. Copy entire contents of [`supabase_exam_workflow.sql`](./Migrations/supabase_exam_workflow.sql)
4. Paste into SQL editor
5. Click "Run"
6. Verify all tables are created:
   - `exam_requests`
   - `teacher_assignments`
   - `exam_plans`
   - `notifications`
   - `workflow_logs`

### Step 1.4: Configure Authentication

1. Go to Authentication → Providers
2. Enable Email provider (default)
3. Go to Authentication → Users
4. Create test users:
   - **Admin User**: `admin@maeen.local` / `Admin@123456`
   - **Teacher User**: `teacher@maeen.local` / `Teacher@123456`
   - **Student User**: `student@maeen.local` / `Student@123456`

5. Set user roles in user metadata:
   - Click user → Edit
   - Add to `raw_user_meta_data`:
   ```json
   {
     "role": "admin"
   }
   ```

## Phase 2: Application Configuration

### Step 2.1: Update Web.config

Add Supabase credentials to `Web.config`:

```xml
<configuration>
  <appSettings>
    <!-- Existing settings -->
    
    <!-- Supabase Configuration -->
    <add key="SupabaseUrl" value="https://your-project-id.supabase.co" />
    <add key="SupabaseKey" value="your-anon-key" />
    <add key="SupabaseServiceRoleKey" value="your-service-role-key" />
    
    <!-- Exam Request Settings -->
    <add key="ExamRequestNotificationEmail" value="admin@maeen.local" />
    <add key="EnableRealTimeNotifications" value="true" />
  </appSettings>
</configuration>
```

### Step 2.2: Install NuGet Packages

Open Package Manager Console and run:

```powershell
Install-Package Supabase.Core
Install-Package Supabase.Gotrue
Install-Package Supabase.Realtime
Install-Package Newtonsoft.Json
```

### Step 2.3: Register Services

In `Global.asax.cs`, add service registration:

```csharp
protected void Application_Start()
{
    // Existing code...
    
    // Register Supabase services
    var supabaseUrl = System.Configuration.ConfigurationManager.AppSettings["SupabaseUrl"];
    var supabaseKey = System.Configuration.ConfigurationManager.AppSettings["SupabaseKey"];
    
    var supabaseClient = new SupabaseClient(supabaseUrl, supabaseKey);
    
    // Store in application state for dependency injection
    Application["SupabaseClient"] = supabaseClient;
}
```

## Phase 3: Frontend Integration

### Step 3.1: Add Exam Request Form to Student Dashboard

Create a new partial view: `Views/Student/_ExamRequestForm.cshtml`

```html
@{
    ViewBag.Title = "طلب اختبار جديد";
}

<div class="card">
    <div class="card-header">
        <h4>طلب اختبار قرآني جديد</h4>
    </div>
    <div class="card-body">
        <form id="examRequestForm" method="post" action="/api/exam-requests/create">
            <div class="form-group">
                <label>اختر السورة</label>
                <select name="surahName" class="form-control" required>
                    <option value="">-- اختر السورة --</option>
                    <option value="الفاتحة">الفاتحة</option>
                    <option value="البقرة">البقرة</option>
                    <option value="آل عمران">آل عمران</option>
                    <!-- Add all 114 Surahs -->
                </select>
            </div>

            <div class="form-row">
                <div class="form-group col-md-6">
                    <label>من الآية</label>
                    <input type="number" name="ayahStart" class="form-control" required>
                </div>
                <div class="form-group col-md-6">
                    <label>إلى الآية</label>
                    <input type="number" name="ayahEnd" class="form-control" required>
                </div>
            </div>

            <div class="form-group">
                <label>مستوى الصعوبة</label>
                <select name="difficultyLevel" class="form-control" required>
                    <option value="Beginner">مبتدئ</option>
                    <option value="Elementary">ابتدائي</option>
                    <option value="Intermediate">متوسط</option>
                    <option value="Advanced">متقدم</option>
                    <option value="Expert">خبير</option>
                </select>
            </div>

            <div class="form-group">
                <label>مستوى التحفيظ الحالي</label>
                <input type="text" name="memorizationLevel" class="form-control" 
                       placeholder="مثال: متوسط، جيد جداً">
            </div>

            <div class="form-group">
                <label>نقاط ضعف في التجويد (اختياري)</label>
                <textarea name="tajweedWeaknesses" class="form-control" rows="3"
                          placeholder="اذكر أي نقاط ضعف تشعر بها في التجويد"></textarea>
            </div>

            <div class="form-group">
                <label>ملاحظات إضافية</label>
                <textarea name="studentNotes" class="form-control" rows="3"
                          placeholder="أي ملاحظات أو طلبات خاصة"></textarea>
            </div>

            <button type="submit" class="btn btn-primary">
                <i class="fa fa-send"></i> إرسال الطلب
            </button>
        </form>
    </div>
</div>

<script>
$(document).ready(function() {
    $('#examRequestForm').on('submit', function(e) {
        e.preventDefault();
        
        var formData = {
            studentId: @ViewBag.UserId,
            surahName: $('[name="surahName"]').val(),
            ayahRange: $('[name="ayahStart"]').val() + '-' + $('[name="ayahEnd"]').val(),
            difficultyLevel: $('[name="difficultyLevel"]').val(),
            memorizationLevel: $('[name="memorizationLevel"]').val(),
            tajweedWeaknesses: $('[name="tajweedWeaknesses"]').val(),
            studentNotes: $('[name="studentNotes"]').val(),
            submittedAnswers: {}
        };

        $.ajax({
            url: '/api/exam-requests/create',
            type: 'POST',
            data: formData,
            success: function(response) {
                if (response.success) {
                    alert('تم إرسال طلبك بنجاح. سيتم مراجعته من قبل الإدارة قريباً.');
                    location.reload();
                } else {
                    alert('خطأ: ' + response.error);
                }
            },
            error: function() {
                alert('حدث خطأ في الاتصال. يرجى المحاولة لاحقاً.');
            }
        });
    });
});
</script>
```

### Step 3.2: Add Exam Request Review Panel to Admin Dashboard

Create: `Views/Admin/_ExamRequestReview.cshtml`

```html
@{
    ViewBag.Title = "مراجعة طلبات الاختبار";
}

<div class="card">
    <div class="card-header">
        <h4>طلبات الاختبار المعلقة</h4>
        <span class="badge badge-warning" id="pendingCount">0</span>
    </div>
    <div class="card-body">
        <table class="table table-striped" id="examRequestsTable">
            <thead>
                <tr>
                    <th>الطالب</th>
                    <th>السورة</th>
                    <th>المستوى</th>
                    <th>التاريخ</th>
                    <th>الحالة</th>
                    <th>الإجراءات</th>
                </tr>
            </thead>
            <tbody id="requestsList">
            </tbody>
        </table>
    </div>
</div>

<!-- Review Modal -->
<div class="modal fade" id="reviewModal" tabindex="-1">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">مراجعة طلب الاختبار</h5>
                <button type="button" class="close" data-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <div id="requestDetails"></div>
                
                <div class="form-group mt-3">
                    <label>تعليقات الإدارة</label>
                    <textarea id="adminComments" class="form-control" rows="3"></textarea>
                </div>

                <div class="form-group">
                    <label>تعيين المعلم</label>
                    <select id="teacherSelect" class="form-control">
                        <option value="">-- اختر معلماً --</option>
                    </select>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-danger" id="rejectBtn">رفض</button>
                <button type="button" class="btn btn-success" id="approveBtn">موافقة</button>
            </div>
        </div>
    </div>
</div>

<script>
$(document).ready(function() {
    loadPendingRequests();
    loadTeachers();
    
    // Refresh every 30 seconds
    setInterval(loadPendingRequests, 30000);
});

function loadPendingRequests() {
    $.ajax({
        url: '/api/exam-requests/all?status=pending',
        type: 'GET',
        success: function(response) {
            if (response.success) {
                $('#pendingCount').text(response.count);
                renderRequests(response.requests);
            }
        }
    });
}

function loadTeachers() {
    // Load teachers from your user management system
    $.ajax({
        url: '/api/users/teachers',
        type: 'GET',
        success: function(response) {
            var select = $('#teacherSelect');
            response.forEach(function(teacher) {
                select.append($('<option>').val(teacher.id).text(teacher.name));
            });
        }
    });
}

function renderRequests(requests) {
    var tbody = $('#requestsList');
    tbody.empty();
    
    requests.forEach(function(req) {
        var row = $('<tr>')
            .append($('<td>').text(req.student_id))
            .append($('<td>').text(req.surah_name))
            .append($('<td>').text(req.difficulty_level))
            .append($('<td>').text(new Date(req.created_at).toLocaleDateString('ar-SA')))
            .append($('<td>').html('<span class="badge badge-warning">' + req.status + '</span>'))
            .append($('<td>').html(
                '<button class="btn btn-sm btn-primary" onclick="reviewRequest(' + req.id + ')">مراجعة</button>'
            ));
        tbody.append(row);
    });
}

function reviewRequest(requestId) {
    $.ajax({
        url: '/api/exam-requests/get/' + requestId,
        type: 'GET',
        success: function(response) {
            displayRequestDetails(response);
            $('#reviewModal').modal('show');
            
            $('#approveBtn').off('click').on('click', function() {
                approveRequest(requestId);
            });
            
            $('#rejectBtn').off('click').on('click', function() {
                rejectRequest(requestId);
            });
        }
    });
}

function displayRequestDetails(request) {
    var html = `
        <div class="request-details">
            <p><strong>الطالب:</strong> ${request.student_id}</p>
            <p><strong>السورة:</strong> ${request.surah_name}</p>
            <p><strong>الآيات:</strong> ${request.ayah_range}</p>
            <p><strong>المستوى:</strong> ${request.difficulty_level}</p>
            <p><strong>مستوى التحفيظ:</strong> ${request.memorization_level}</p>
            <p><strong>نقاط ضعف التجويد:</strong> ${request.tajweed_weaknesses}</p>
            <p><strong>ملاحظات الطالب:</strong> ${request.student_notes}</p>
        </div>
    `;
    $('#requestDetails').html(html);
}

function approveRequest(requestId) {
    var teacherId = $('#teacherSelect').val();
    
    if (!teacherId) {
        alert('يرجى اختيار معلم');
        return;
    }
    
    $.ajax({
        url: '/api/exam-requests/review',
        type: 'POST',
        data: {
            examRequestId: requestId,
            adminId: @ViewBag.UserId,
            action: 'approve',
            adminComments: $('#adminComments').val()
        },
        success: function(response) {
            if (response.success) {
                assignTeacher(requestId, teacherId);
            }
        }
    });
}

function assignTeacher(requestId, teacherId) {
    $.ajax({
        url: '/api/exam-requests/assign-teacher',
        type: 'POST',
        data: {
            examRequestId: requestId,
            teacherId: teacherId,
            adminId: @ViewBag.UserId,
            assignmentNotes: $('#adminComments').val()
        },
        success: function(response) {
            if (response.success) {
                alert('تم الموافقة وتعيين المعلم بنجاح');
                $('#reviewModal').modal('hide');
                loadPendingRequests();
            }
        }
    });
}

function rejectRequest(requestId) {
    $.ajax({
        url: '/api/exam-requests/review',
        type: 'POST',
        data: {
            examRequestId: requestId,
            adminId: @ViewBag.UserId,
            action: 'reject',
            adminComments: $('#adminComments').val()
        },
        success: function(response) {
            if (response.success) {
                alert('تم رفض الطلب');
                $('#reviewModal').modal('hide');
                loadPendingRequests();
            }
        }
    });
}
</script>
```

### Step 3.3: Add Teacher Exam Plan Creation Form

Create: `Views/Teacher/_ExamPlanForm.cshtml`

```html
@{
    ViewBag.Title = "إنشاء خطة اختبار";
}

<div class="card">
    <div class="card-header">
        <h4>إنشاء خطة اختبار مخصصة</h4>
    </div>
    <div class="card-body">
        <form id="examPlanForm" method="post" action="/api/exam-requests/create-exam-plan">
            <input type="hidden" name="examRequestId" id="examRequestId">
            <input type="hidden" name="teacherId" value="@ViewBag.UserId">

            <div class="form-group">
                <label>نقاط التجويد المركزة</label>
                <div id="tajweedFocusPoints">
                    <div class="input-group mb-2">
                        <input type="text" class="form-control tajweed-point" 
                               placeholder="أدخل نقطة تجويد">
                        <div class="input-group-append">
                            <button class="btn btn-outline-secondary" type="button" 
                                    onclick="addTajweedPoint()">إضافة</button>
                        </div>
                    </div>
                </div>
            </div>

            <div class="form-group">
                <label>ملاحظات التحضير</label>
                <textarea name="preparationNotes" class="form-control" rows="4"
                          placeholder="أدخل ملاحظات التحضير للطالب"></textarea>
            </div>

            <div class="form-group">
                <label>التوجيهات التحفيزية</label>
                <textarea name="motivationalGuidance" class="form-control" rows="4"
                          placeholder="أضف رسائل تحفيزية للطالب"></textarea>
            </div>

            <div class="form-group">
                <label>تعديلات الصعوبة</label>
                <select name="difficultyAdjustment" class="form-control">
                    <option value="">بدون تعديل</option>
                    <option value="easier">أسهل</option>
                    <option value="harder">أصعب</option>
                </select>
            </div>

            <button type="submit" class="btn btn-primary">
                <i class="fa fa-save"></i> حفظ خطة الاختبار
            </button>
        </form>
    </div>
</div>

<script>
function addTajweedPoint() {
    var html = `
        <div class="input-group mb-2">
            <input type="text" class="form-control tajweed-point" placeholder="أدخل نقطة تجويد">
            <div class="input-group-append">
                <button class="btn btn-outline-danger" type="button" 
                        onclick="$(this).closest('.input-group').remove()">حذف</button>
            </div>
        </div>
    `;
    $('#tajweedFocusPoints').append(html);
}

$('#examPlanForm').on('submit', function(e) {
    e.preventDefault();
    
    var tajweedPoints = [];
    $('.tajweed-point').each(function() {
        if ($(this).val()) {
            tajweedPoints.push($(this).val());
        }
    });
    
    var formData = {
        examRequestId: $('#examRequestId').val(),
        teacherId: @ViewBag.UserId,
        testPlan: {},
        tajweedFocus: { focus_points: tajweedPoints },
        memorizationTasks: {},
        preparationNotes: $('[name="preparationNotes"]').val(),
        motivationalGuidance: $('[name="motivationalGuidance"]').val(),
        difficultyAdjustments: { adjustment: $('[name="difficultyAdjustment"]').val() }
    };
    
    $.ajax({
        url: '/api/exam-requests/create-exam-plan',
        type: 'POST',
        data: formData,
        success: function(response) {
            if (response.success) {
                alert('تم حفظ خطة الاختبار بنجاح');
                location.reload();
            } else {
                alert('خطأ: ' + response.error);
            }
        }
    });
});
</script>
```

## Phase 4: Real-time Notifications Setup

### Step 4.1: Create Notification Service

Create: `Services/NotificationService.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Maeen1_New.Services
{
    public class NotificationService
    {
        private readonly SupabaseClient _supabaseClient;

        public NotificationService(SupabaseClient supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<JObject> CreateNotificationAsync(
            int userId,
            string notificationType,
            string title,
            string message,
            JObject data = null)
        {
            try
            {
                var notification = new JObject
                {
                    { "user_id", userId },
                    { "notification_type", notificationType },
                    { "title", title },
                    { "message", message },
                    { "data", data ?? new JObject() },
                    { "is_read", false },
                    { "created_at", DateTime.UtcNow.ToString("O") }
                };

                return await _supabaseClient.InsertAsync("notifications", notification);
            }
            catch (Exception ex)
            {
                return new JObject { { "error", ex.Message } };
            }
        }

        public async Task<JArray> GetUserNotificationsAsync(int userId, bool unreadOnly = false)
        {
            try
            {
                var query = new Dictionary<string, object> { { "user_id", userId } };
                if (unreadOnly)
                {
                    query["is_read"] = false;
                }

                var results = await _supabaseClient.QueryAsync("notifications", query);
                return new JArray(results);
            }
            catch (Exception ex)
            {
                return new JArray();
            }
        }

        public async Task<JObject> MarkAsReadAsync(int notificationId)
        {
            try
            {
                var updateData = new JObject
                {
                    { "is_read", true },
                    { "read_at", DateTime.UtcNow.ToString("O") }
                };

                return await _supabaseClient.UpdateAsync("notifications", notificationId, updateData);
            }
            catch (Exception ex)
            {
                return new JObject { { "error", ex.Message } };
            }
        }
    }
}
```

### Step 4.2: Add Notification API Endpoint

Add to `ExamRequestWorkflowController.cs`:

```csharp
/// <summary>
/// Get user notifications
/// GET: /api/exam-requests/notifications/{userId}
/// </summary>
[HttpGet]
[Route("notifications/{userId}")]
public async System.Threading.Tasks.Task<ActionResult> GetNotifications(int userId, bool unreadOnly = false)
{
    try
    {
        var notificationService = new NotificationService(
            (SupabaseClient)HttpContext.Application["SupabaseClient"]
        );
        
        var notifications = await notificationService.GetUserNotificationsAsync(userId, unreadOnly);
        
        return Json(new
        {
            success = true,
            count = notifications.Count,
            notifications = notifications
        }, JsonRequestBehavior.AllowGet);
    }
    catch (Exception ex)
    {
        return Json(new
        {
            success = false,
            error = "خطأ في جلب الإشعارات",
            message = ex.Message
        }, JsonRequestBehavior.AllowGet);
    }
}

/// <summary>
/// Mark notification as read
/// POST: /api/exam-requests/mark-notification-read
/// </summary>
[HttpPost]
[Route("mark-notification-read")]
public async System.Threading.Tasks.Task<ActionResult> MarkNotificationAsRead(int notificationId)
{
    try
    {
        var notificationService = new NotificationService(
            (SupabaseClient)HttpContext.Application["SupabaseClient"]
        );
        
        var result = await notificationService.MarkAsReadAsync(notificationId);
        
        return Json(new
        {
            success = true,
            message = "تم تحديث الإشعار"
        });
    }
    catch (Exception ex)
    {
        return Json(new
        {
            success = false,
            error = "خطأ في تحديث الإشعار",
            message = ex.Message
        });
    }
}
```

## Phase 5: Testing

### Step 5.1: Test Student Request Creation

1. Log in as student
2. Navigate to Student Dashboard
3. Fill exam request form
4. Submit request
5. Verify notification sent to admin

### Step 5.2: Test Admin Review

1. Log in as admin
2. Navigate to Admin Dashboard
3. View pending requests
4. Review request details
5. Approve and assign teacher
6. Verify notifications sent to teacher and student

### Step 5.3: Test Teacher Exam Plan Creation

1. Log in as teacher
2. View assigned exam requests
3. Create personalized exam plan
4. Add Tajweed focus points
5. Add preparation notes
6. Submit exam plan
7. Verify student receives notification

## Phase 6: Deployment

### Step 6.1: Production Supabase Setup

1. Create production Supabase project
2. Run migration SQL
3. Configure production credentials in Web.config
4. Set up SSL certificates
5. Enable CORS for your domain

### Step 6.2: Application Deployment

1. Build solution in Release mode
2. Publish to production server
3. Configure IIS application pool
4. Set up SSL binding
5. Test all workflows in production

## Troubleshooting

### Issue: Supabase Connection Failed
**Solution**: Verify credentials in Web.config and check Supabase project status

### Issue: RLS Policies Blocking Access
**Solution**: Check user roles in auth.users metadata and verify RLS policies

### Issue: Notifications Not Appearing
**Solution**: Verify notifications table has data and check browser console for errors

### Issue: Teacher Assignment Not Working
**Solution**: Ensure teacher exists in system and has correct role

## Support & Resources

- [Supabase Documentation](https://supabase.com/docs)
- [ASP.NET MVC Documentation](https://docs.microsoft.com/en-us/aspnet/mvc/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)

## Next Steps

1. Customize UI to match your branding
2. Add email notifications
3. Implement SMS alerts
4. Add video submission support
5. Create analytics dashboard
6. Set up automated backups
7. Implement caching layer
8. Add API rate limiting

---

**Version**: 1.0.0  
**Last Updated**: May 11, 2026  
**Status**: Production Ready
