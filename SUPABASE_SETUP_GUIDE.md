# Supabase Setup Quick Reference

## Fixed Issues

✅ **UUID Type Compatibility**: All user ID references now use UUID instead of BIGINT to match Supabase's `auth.users` table structure.

## Quick Setup Steps

### 1. Create Supabase Project
- Go to https://supabase.com
- Click "New Project"
- Fill in project details and create

### 2. Get Credentials
In Project Settings → API:
- Copy **Project URL** (e.g., `https://xxxxx.supabase.co`)
- Copy **Anon Key** (public key)
- Copy **Service Role Key** (secret key)

### 3. Run SQL Migration
1. Go to Supabase Dashboard → SQL Editor
2. Click "New Query"
3. Copy entire contents of [`Migrations/supabase_exam_workflow.sql`](./Migrations/supabase_exam_workflow.sql)
4. Paste into SQL editor
5. Click "Run"

### 4. Verify Tables Created
Check that these tables exist:
- ✅ `exam_requests`
- ✅ `teacher_assignments`
- ✅ `exam_plans`
- ✅ `notifications`
- ✅ `workflow_logs`

### 5. Create Test Users
Go to Authentication → Users:

**Admin User:**
- Email: `admin@maeen.local`
- Password: `Admin@123456`
- Metadata: `{"role": "admin"}`

**Teacher User:**
- Email: `teacher@maeen.local`
- Password: `Teacher@123456`
- Metadata: `{"role": "teacher"}`

**Student User:**
- Email: `student@maeen.local`
- Password: `Student@123456`
- Metadata: `{"role": "student"}`

### 6. Update Web.config
```xml
<add key="SupabaseUrl" value="https://your-project-id.supabase.co" />
<add key="SupabaseKey" value="your-anon-key" />
<add key="SupabaseServiceRoleKey" value="your-service-role-key" />
```

### 7. Test Connection
Call health check endpoint:
```
GET http://localhost:8080/api/exam-requests/health
```

Expected response:
```json
{
  "status": "healthy",
  "service": "Exam Request Workflow",
  "version": "1.0.0",
  "timestamp": "2026-05-12T00:07:03Z"
}
```

## Database Schema Overview

### exam_requests
- Stores all exam request data
- Tracks workflow status (pending → completed)
- Stores student submissions and answers
- Records admin and teacher comments

### teacher_assignments
- Links teachers to exam requests
- Tracks assignment metadata
- Records who assigned and when

### exam_plans
- Stores personalized exam plans
- Contains Tajweed focus points
- Includes preparation notes and guidance

### notifications
- Real-time notifications for all users
- Tracks read/unread status
- Stores notification metadata

### workflow_logs
- Complete audit trail
- Records all status changes
- Tracks who made changes and when

## API Endpoints Quick Reference

### Student
- `POST /api/exam-requests/create` - Submit exam request
- `GET /api/exam-requests/student/{studentId}` - Get my requests
- `GET /api/exam-requests/notifications/{userId}` - Get notifications

### Admin
- `POST /api/exam-requests/review` - Review request
- `POST /api/exam-requests/assign-teacher` - Assign teacher
- `GET /api/exam-requests/all` - Get all requests
- `GET /api/exam-requests/statistics` - Get statistics

### Teacher
- `GET /api/exam-requests/teacher/{teacherId}` - Get assigned requests
- `POST /api/exam-requests/create-exam-plan` - Create exam plan

### General
- `GET /api/exam-requests/get/{id}` - Get request details
- `GET /api/exam-requests/generate-test-plan` - Generate test plan
- `GET /api/exam-requests/health` - Health check

## Troubleshooting

### Error: Foreign key constraint failed
**Solution**: Ensure all user IDs are UUID type, not BIGINT. The fixed SQL file uses UUID for all auth.users references.

### Error: RLS policy denies access
**Solution**: Check user metadata has correct role:
```json
{
  "role": "admin"  // or "teacher" or "student"
}
```

### Error: Table doesn't exist
**Solution**: Verify SQL migration ran successfully. Check Supabase SQL Editor for any errors.

### Error: Connection refused
**Solution**: 
1. Verify Supabase URL in Web.config
2. Check internet connection
3. Verify Supabase project is active

## Testing Workflow

### Test 1: Create Exam Request
```bash
POST /api/exam-requests/create
{
  "studentId": 1,
  "surahName": "يوسف",
  "ayahRange": "1-20",
  "difficultyLevel": "Intermediate",
  "memorizationLevel": "متوسط",
  "tajweedWeaknesses": "أحكام النون",
  "studentNotes": "أريد التركيز على التجويد",
  "submittedAnswers": {}
}
```

### Test 2: Review Request (Admin)
```bash
POST /api/exam-requests/review
{
  "examRequestId": 1,
  "adminId": 2,
  "action": "approve",
  "adminComments": "طلب جيد"
}
```

### Test 3: Assign Teacher (Admin)
```bash
POST /api/exam-requests/assign-teacher
{
  "examRequestId": 1,
  "teacherId": 3,
  "adminId": 2,
  "assignmentNotes": "معلم متخصص"
}
```

### Test 4: Create Exam Plan (Teacher)
```bash
POST /api/exam-requests/create-exam-plan
{
  "examRequestId": 1,
  "teacherId": 3,
  "testPlan": {},
  "tajweedFocus": {"focus_points": ["أحكام النون"]},
  "memorizationTasks": {},
  "preparationNotes": "ركز على الآيات الصعبة",
  "motivationalGuidance": "أنت قادر على النجاح",
  "difficultyAdjustments": {}
}
```

## Security Checklist

- ✅ All user IDs use UUID type
- ✅ RLS policies enabled on all tables
- ✅ Role-based access control configured
- ✅ Foreign key constraints in place
- ✅ Audit trail logging enabled
- ✅ Automatic timestamp updates configured

## Next Steps

1. ✅ Create Supabase project
2. ✅ Run SQL migration
3. ✅ Create test users
4. ✅ Update Web.config
5. ✅ Test API endpoints
6. ⏳ Integrate with UI
7. ⏳ Deploy to production

## Support

- Supabase Docs: https://supabase.com/docs
- PostgreSQL Docs: https://www.postgresql.org/docs/
- Project Documentation: See [`IMPLEMENTATION_GUIDE.md`](./IMPLEMENTATION_GUIDE.md)

---

**Status**: Ready for Supabase Integration
**Last Updated**: May 12, 2026
