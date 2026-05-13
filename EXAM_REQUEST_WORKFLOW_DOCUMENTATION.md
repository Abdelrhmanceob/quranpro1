# Quran Exam Request Workflow Documentation

## Overview

The Quran Exam Request Workflow is a comprehensive system that manages the complete lifecycle of student exam requests from submission through teacher assignment and exam completion. The system uses Supabase for data storage and real-time notifications.

## Workflow Status Flow

```
Student Creates Request
        ↓
    [pending]
        ↓
Admin Reviews Request
        ↓
    [approved] or [rejected]
        ↓
Admin Assigns Teacher
        ↓
    [assigned]
        ↓
Teacher Creates Exam Plan
        ↓
Student Prepares & Takes Exam
        ↓
    [completed]
```

## Database Schema

### 1. exam_requests Table
Stores all exam request data with complete workflow tracking.

**Columns:**
- `id` - Primary key
- `student_id` - Reference to student user
- `admin_id` - Reference to admin who reviewed
- `teacher_id` - Reference to assigned teacher
- `surah_name` - Name of Quranic Surah
- `ayah_range` - Range of Ayahs (e.g., "1-20")
- `difficulty_level` - Beginner, Elementary, Intermediate, Advanced, Expert
- `memorization_level` - Student's current memorization level
- `tajweed_weaknesses` - JSON array of identified weaknesses
- `student_notes` - Student's submission notes
- `submitted_answers` - JSONB of student's answers
- `status` - pending, under_review, approved, assigned, completed, rejected
- `admin_comments` - Admin's review comments
- `teacher_comments` - Teacher's feedback
- `generated_test_plan` - JSONB of generated test plan
- `success_roadmap` - JSONB of preparation roadmap
- `exam_scheduled_date` - Scheduled exam date/time
- `created_at`, `submitted_at`, `reviewed_at`, `approved_at`, `assigned_at`, `completed_at` - Timestamps

### 2. teacher_assignments Table
Tracks teacher assignments to exam requests.

**Columns:**
- `id` - Primary key
- `exam_request_id` - Reference to exam request
- `teacher_id` - Reference to assigned teacher
- `assigned_by_admin_id` - Reference to admin who made assignment
- `assignment_notes` - Notes about the assignment
- `assigned_at` - Assignment timestamp

### 3. exam_plans Table
Stores personalized exam plans created by teachers.

**Columns:**
- `id` - Primary key
- `exam_request_id` - Reference to exam request
- `teacher_id` - Reference to teacher who created plan
- `test_plan` - JSONB of complete test plan
- `tajweed_focus` - JSONB of Tajweed focus points
- `memorization_tasks` - JSONB of memorization tasks
- `preparation_notes` - Teacher's preparation notes
- `motivational_guidance` - Motivational messages for student
- `difficulty_adjustments` - JSONB of difficulty adjustments

### 4. notifications Table
Stores real-time notifications for all users.

**Columns:**
- `id` - Primary key
- `user_id` - Reference to recipient user
- `exam_request_id` - Reference to related exam request
- `notification_type` - Type of notification
- `title` - Notification title
- `message` - Notification message
- `data` - JSONB of additional data
- `is_read` - Read status
- `read_at` - When notification was read
- `created_at` - Creation timestamp

### 5. workflow_logs Table
Audit trail of all workflow status changes.

**Columns:**
- `id` - Primary key
- `exam_request_id` - Reference to exam request
- `previous_status` - Previous status
- `new_status` - New status
- `changed_by_user_id` - User who made change
- `changed_by_role` - Role of user (student, admin, teacher)
- `reason` - Reason for change
- `comments` - Additional comments
- `ip_address` - IP address of change
- `changed_at` - Change timestamp

## API Endpoints

### Student Endpoints

#### 1. Create Exam Request
**POST** `/api/exam-requests/create`

**Request Body:**
```json
{
  "studentId": 4,
  "surahName": "يوسف",
  "ayahRange": "1-20",
  "difficultyLevel": "Intermediate",
  "memorizationLevel": "متوسط",
  "tajweedWeaknesses": "أحكام النون والتنوين",
  "studentNotes": "أريد التركيز على التجويد",
  "submittedAnswers": {
    "answers": [],
    "notes": ""
  }
}
```

**Response:**
```json
{
  "success": true,
  "message": "تم إنشاء طلب الاختبار بنجاح",
  "exam_request_id": 1,
  "status": "pending"
}
```

#### 2. Get Student's Exam Requests
**GET** `/api/exam-requests/student/{studentId}`

**Response:**
```json
{
  "success": true,
  "count": 3,
  "requests": [
    {
      "id": 1,
      "surah_name": "يوسف",
      "status": "assigned",
      "created_at": "2026-05-11T23:00:00Z",
      "teacher_id": 5
    }
  ]
}
```

### Admin Endpoints

#### 1. Review Exam Request
**POST** `/api/exam-requests/review`

**Request Body:**
```json
{
  "examRequestId": 1,
  "adminId": 9,
  "action": "approve",
  "adminComments": "طلب جيد، يمكن المتابعة"
}
```

**Response:**
```json
{
  "success": true,
  "message": "تم الموافقة على طلب الاختبار",
  "exam_request_id": 1,
  "status": "approved"
}
```

#### 2. Assign Teacher to Exam Request
**POST** `/api/exam-requests/assign-teacher`

**Request Body:**
```json
{
  "examRequestId": 1,
  "teacherId": 5,
  "adminId": 9,
  "assignmentNotes": "معلم متخصص في التجويد"
}
```

**Response:**
```json
{
  "success": true,
  "message": "تم تعيين المعلم بنجاح",
  "exam_request_id": 1,
  "teacher_id": 5,
  "status": "assigned"
}
```

#### 3. Get All Exam Requests
**GET** `/api/exam-requests/all?status=pending`

**Response:**
```json
{
  "success": true,
  "count": 5,
  "requests": [
    {
      "id": 1,
      "student_id": 4,
      "surah_name": "يوسف",
      "status": "pending",
      "created_at": "2026-05-11T23:00:00Z"
    }
  ]
}
```

#### 4. Get Workflow Statistics
**GET** `/api/exam-requests/statistics`

**Response:**
```json
{
  "success": true,
  "statistics": {
    "pending": 5,
    "under_review": 3,
    "approved": 8,
    "assigned": 12,
    "completed": 25,
    "rejected": 2
  }
}
```

### Teacher Endpoints

#### 1. Get Assigned Exam Requests
**GET** `/api/exam-requests/teacher/{teacherId}`

**Response:**
```json
{
  "success": true,
  "count": 3,
  "requests": [
    {
      "id": 1,
      "student_id": 4,
      "surah_name": "يوسف",
      "status": "assigned",
      "assigned_at": "2026-05-11T23:30:00Z"
    }
  ]
}
```

#### 2. Create Exam Plan
**POST** `/api/exam-requests/create-exam-plan`

**Request Body:**
```json
{
  "examRequestId": 1,
  "teacherId": 5,
  "testPlan": {
    "title": "اختبار سورة يوسف",
    "tasks": []
  },
  "tajweedFocus": {
    "focus_points": ["أحكام النون", "المد والقصر"]
  },
  "memorizationTasks": {
    "tasks": []
  },
  "preparationNotes": "ركز على الآيات الصعبة",
  "motivationalGuidance": "أنت قادر على النجاح",
  "difficultyAdjustments": {}
}
```

**Response:**
```json
{
  "success": true,
  "message": "تم إنشاء خطة الاختبار بنجاح",
  "exam_plan_id": 1,
  "exam_request_id": 1
}
```

### General Endpoints

#### 1. Get Exam Request Details
**GET** `/api/exam-requests/get/{id}`

**Response:**
```json
{
  "id": 1,
  "student_id": 4,
  "teacher_id": 5,
  "surah_name": "يوسف",
  "ayah_range": "1-20",
  "status": "assigned",
  "teacher_assignment": {
    "id": 1,
    "teacher_id": 5,
    "assigned_at": "2026-05-11T23:30:00Z"
  },
  "exam_plan": {
    "id": 1,
    "test_plan": {}
  }
}
```

#### 2. Generate Test Plan
**GET** `/api/exam-requests/generate-test-plan?examRequestId=1&level=Intermediate`

**Response:**
```json
{
  "success": true,
  "test_plan": {
    "title": "اختبار سورة يوسف - مستوى متوسط",
    "surah": "يوسف",
    "ayah_range": "1-20",
    "difficulty": "متوسط",
    "estimated_time": "50 دقيقة",
    "passing_score": "70%",
    "tasks": []
  }
}
```

#### 3. Health Check
**GET** `/api/exam-requests/health`

**Response:**
```json
{
  "status": "healthy",
  "service": "Exam Request Workflow",
  "version": "1.0.0",
  "timestamp": "2026-05-11T23:42:31Z"
}
```

## Row Level Security (RLS) Policies

### Students
- Can view their own exam requests
- Can create new exam requests
- Can view their assigned exam plans
- Can view notifications related to their requests

### Admins
- Can view all exam requests
- Can update exam requests (review, approve, reject)
- Can manage teacher assignments
- Can view all workflow logs
- Can view all notifications

### Teachers
- Can view exam requests assigned to them
- Can update assigned exam requests
- Can create and manage exam plans
- Can view notifications related to assigned requests

## Notification Types

1. **exam_request_pending** - New exam request submitted (to admins)
2. **exam_request_update** - Request status changed (to students)
3. **exam_request_assigned** - Teacher assigned (to teachers and students)
4. **exam_plan_ready** - Exam plan created (to students)
5. **exam_scheduled** - Exam scheduled (to students and teachers)

## Workflow Implementation Steps

### Step 1: Setup Supabase
1. Create Supabase project
2. Run the migration SQL file: `supabase_exam_workflow.sql`
3. Configure RLS policies
4. Set up authentication with roles

### Step 2: Configure Application
1. Add Supabase credentials to `Web.config`:
```xml
<add key="SupabaseUrl" value="https://your-project.supabase.co" />
<add key="SupabaseKey" value="your-anon-key" />
```

2. Register services in dependency injection container

### Step 3: Integrate with UI
1. Add exam request form to Student Dashboard
2. Add exam request review panel to Admin Dashboard
3. Add teacher assignment interface to Admin Dashboard
4. Add exam plan creation form to Teacher Dashboard
5. Add notification system to all dashboards

### Step 4: Enable Real-time Updates
1. Subscribe to exam_requests table changes
2. Subscribe to notifications table for real-time alerts
3. Update UI when status changes

## Example Workflow Scenario

### Scenario: Student Submits Exam Request

**1. Student Action (Student Dashboard)**
- Student fills exam request form
- Selects Surah: "يوسف"
- Selects Ayah range: "1-20"
- Selects difficulty: "Intermediate"
- Adds notes about Tajweed weaknesses
- Submits request

**2. System Action**
- Creates exam request with status "pending"
- Logs workflow change
- Sends notification to all admins
- Student sees confirmation message

**3. Admin Action (Admin Dashboard)**
- Admin reviews pending requests
- Reads student's submission and notes
- Approves request with comments
- Selects teacher from dropdown
- Assigns teacher to request

**4. System Action**
- Updates request status to "assigned"
- Logs workflow change
- Sends notification to teacher
- Sends notification to student

**5. Teacher Action (Teacher Dashboard)**
- Teacher receives notification
- Views assigned exam request
- Reviews student's level and weaknesses
- Generates personalized test plan
- Adds Tajweed focus points
- Adds motivational guidance
- Submits exam plan

**6. System Action**
- Creates exam plan
- Updates request status to "assigned"
- Sends notification to student

**7. Student Action (Student Dashboard)**
- Student receives notification
- Views exam plan
- Sees preparation instructions
- Accesses Tajweed focus points
- Begins preparation

## Security Considerations

1. **Authentication**: All endpoints require user authentication via Supabase Auth
2. **Authorization**: RLS policies enforce role-based access control
3. **Data Validation**: All inputs are validated before processing
4. **Audit Trail**: All changes are logged in workflow_logs table
5. **IP Tracking**: IP addresses are recorded for security auditing

## Performance Optimization

1. **Indexes**: Created on frequently queried columns
2. **Pagination**: Implement pagination for large result sets
3. **Caching**: Cache test plans and student profiles
4. **Real-time**: Use Supabase Realtime for instant updates

## Error Handling

All endpoints return structured error responses:

```json
{
  "success": false,
  "error": "خطأ في إنشاء الطلب",
  "message": "Detailed error message"
}
```

## Testing

### Unit Tests
- Test workflow status transitions
- Test permission checks
- Test notification creation

### Integration Tests
- Test complete workflow from request to completion
- Test role-based access control
- Test real-time notifications

### End-to-End Tests
- Test student request submission
- Test admin review and assignment
- Test teacher exam plan creation
- Test student notification receipt

## Future Enhancements

1. **AI-Powered Assessment**: Integrate AI for automatic evaluation
2. **Video Recording**: Support video submission of recitations
3. **Progress Tracking**: Track student progress over time
4. **Analytics Dashboard**: Detailed analytics for admins and teachers
5. **Mobile App**: Native mobile application
6. **SMS Notifications**: Send SMS alerts for important updates
7. **Email Integration**: Send detailed emails with exam plans
8. **Scheduling System**: Automated exam scheduling

## Support

For issues or questions, contact the development team or refer to the Supabase documentation.
