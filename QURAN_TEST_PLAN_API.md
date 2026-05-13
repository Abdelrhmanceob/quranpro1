# Quran Test Plan Generator API Documentation

## Overview
The Quran Test Plan Generator is a comprehensive service that creates structured, adaptive test plans for Quran memorization students. It generates detailed exam plans with tasks, evaluation criteria, and teacher/student instructions based on student proficiency levels.

## Base URL
```
http://localhost:8080/QuranTestPlan
```

## API Endpoints

### 1. Generate Single Test Plan
**Endpoint:** `GET /Generate`

**Description:** Generates a single test plan based on student level and optional student ID.

**Parameters:**
- `level` (string, optional): Student proficiency level
  - Values: `Beginner`, `Elementary`, `Intermediate`, `Advanced`, `Expert`
  - Default: `Beginner`
- `studentId` (integer, optional): ID of the student

**Example Request:**
```
GET http://localhost:8080/QuranTestPlan/Generate?level=Intermediate&studentId=4
```

**Example Response:**
```json
{
  "title": "اختبار سورة يوسف - مستوى متوسط",
  "surah": "يوسف",
  "ayah_range": "1-20",
  "difficulty": "متوسط",
  "estimated_time": "50 دقيقة",
  "passing_score": "70%",
  "tasks": [
    {
      "task_id": 1,
      "type": "memorization",
      "description": "تلاوة سورة يوسف من الآية 1-20 بدقة وتركيز",
      "weight": 40,
      "criteria": [
        "عدم الأخطاء في النطق",
        "الالتزام بالحروف والحركات",
        "عدم الإضافة أو الحذف",
        "الثقة والطلاقة في التلاوة"
      ]
    },
    {
      "task_id": 2,
      "type": "tajweed",
      "description": "تطبيق أحكام التجويد بشكل صحيح",
      "weight": 30,
      "criteria": [
        "تطبيق أحكام النون والتنوين",
        "تطبيق أحكام الميم الساكنة",
        "تطبيق أحكام اللام الساكنة",
        "الوقف والابتداء الصحيح"
      ]
    },
    {
      "task_id": 3,
      "type": "fluency",
      "description": "التلاوة بسلاسة وانسيابية دون توقف",
      "weight": 20,
      "criteria": [
        "عدم التوقف المفاجئ",
        "التدفق الطبيعي للآيات",
        "الربط الصحيح بين الآيات",
        "الحفاظ على الإيقاع"
      ]
    },
    {
      "task_id": 4,
      "type": "continuation",
      "description": "الاستمرار من نقطة عشوائية في النص",
      "weight": 10,
      "criteria": [
        "القدرة على الاستمرار بسلاسة",
        "عدم الخلط مع آيات أخرى",
        "الحفاظ على الدقة"
      ]
    }
  ],
  "tajweed_focus": [
    "أحكام النون الساكنة والتنوين",
    "أحكام الميم الساكنة",
    "الإدغام والإظهار",
    "المد والقصر",
    "الهمزة والتسهيل"
  ],
  "teacher_notes": "ملاحظات المعلم لسورة يوسف:\n\n1. التركيز على النقاط الصعبة...",
  "student_instructions": "تعليمات الطالب:\n\n1. التحضير:\n   - اقرأ الآيات المطلوبة من سورة يوسف عدة مرات قبل الاختبار...",
  "created_at": "2026-05-11T23:31:03.1234567Z",
  "student_id": 4
}
```

---

### 2. Generate Multiple Test Plans
**Endpoint:** `GET /GenerateMultiple`

**Description:** Generates multiple test plans at once.

**Parameters:**
- `count` (integer, optional): Number of test plans to generate
  - Default: `5`
  - Range: 1-20
- `level` (string, optional): Student proficiency level
  - Default: `Beginner`

**Example Request:**
```
GET http://localhost:8080/QuranTestPlan/GenerateMultiple?count=3&level=Advanced
```

**Example Response:**
```json
{
  "success": true,
  "count": 3,
  "level": "Advanced",
  "test_plans": [
    { /* test plan 1 */ },
    { /* test plan 2 */ },
    { /* test plan 3 */ }
  ]
}
```

---

### 3. Get Available Difficulty Levels
**Endpoint:** `GET /GetLevels`

**Description:** Returns all available difficulty levels with descriptions.

**Example Request:**
```
GET http://localhost:8080/QuranTestPlan/GetLevels
```

**Example Response:**
```json
{
  "success": true,
  "levels": [
    {
      "level": "Beginner",
      "label": "مبتدئ",
      "description": "للطلاب الجدد - 1-5 آيات من السور القصيرة"
    },
    {
      "level": "Elementary",
      "label": "ابتدائي",
      "description": "للطلاب المبتدئين - 5-10 آيات من السور المتوسطة"
    },
    {
      "level": "Intermediate",
      "label": "متوسط",
      "description": "للطلاب المتوسطين - 10-20 آية من السور الطويلة"
    },
    {
      "level": "Advanced",
      "label": "متقدم",
      "description": "للطلاب المتقدمين - 20+ آية من السور الطويلة جداً"
    },
    {
      "level": "Expert",
      "label": "خبير",
      "description": "للطلاب الخبراء - سورة كاملة أو أكثر"
    }
  ]
}
```

---

### 4. Get Available Surahs
**Endpoint:** `GET /GetSurahs`

**Description:** Returns list of all Quranic Surahs with their Ayah counts.

**Example Request:**
```
GET http://localhost:8080/QuranTestPlan/GetSurahs
```

**Example Response:**
```json
{
  "success": true,
  "total_surahs": 114,
  "sample_surahs": [
    { "name": "الفاتحة", "ayahs": 7 },
    { "name": "البقرة", "ayahs": 286 },
    { "name": "آل عمران", "ayahs": 200 },
    { "name": "النساء", "ayahs": 176 },
    { "name": "المائدة", "ayahs": 120 }
  ]
}
```

---

### 5. Get Test Plan Template
**Endpoint:** `GET /GetTemplate`

**Description:** Returns the structure/schema of a test plan for reference.

**Example Request:**
```
GET http://localhost:8080/QuranTestPlan/GetTemplate
```

**Example Response:**
```json
{
  "success": true,
  "template": {
    "title": "عنوان الاختبار",
    "surah": "اسم السورة",
    "ayah_range": "نطاق الآيات (مثال: 1-10)",
    "difficulty": "مستوى الصعوبة",
    "estimated_time": "الوقت المتوقع بالدقائق",
    "passing_score": "درجة النجاح %",
    "tasks": [
      {
        "task_id": "معرف المهمة",
        "type": "نوع المهمة (memorization, tajweed, fluency, continuation)",
        "description": "وصف المهمة",
        "weight": "وزن المهمة %",
        "criteria": ["معايير التقييم"]
      }
    ],
    "tajweed_focus": ["نقاط التجويد المركزة"],
    "teacher_notes": "ملاحظات المعلم",
    "student_instructions": "تعليمات الطالب",
    "created_at": "تاريخ الإنشاء",
    "student_id": "معرف الطالب"
  }
}
```

---

### 6. Health Check
**Endpoint:** `GET /Health`

**Description:** Checks if the service is running and healthy.

**Example Request:**
```
GET http://localhost:8080/QuranTestPlan/Health
```

**Example Response:**
```json
{
  "status": "healthy",
  "service": "Quran Test Plan Generator",
  "version": "1.0.0",
  "timestamp": "2026-05-11T23:31:03.1234567Z"
}
```

---

## Difficulty Levels Explained

### 1. **Beginner (مبتدئ)**
- **Ayah Range:** 1-5 Ayahs
- **Surahs:** Short Surahs (≤30 Ayahs)
- **Estimated Time:** 10-15 minutes
- **Passing Score:** 80%
- **Focus:** Basic memorization and pronunciation

### 2. **Elementary (ابتدائي)**
- **Ayah Range:** 5-10 Ayahs
- **Surahs:** Medium Surahs (30-60 Ayahs)
- **Estimated Time:** 20-30 minutes
- **Passing Score:** 75%
- **Focus:** Accuracy and basic Tajweed

### 3. **Intermediate (متوسط)**
- **Ayah Range:** 10-20 Ayahs
- **Surahs:** Longer Surahs (60-120 Ayahs)
- **Estimated Time:** 40-50 minutes
- **Passing Score:** 70%
- **Focus:** Fluency and advanced Tajweed

### 4. **Advanced (متقدم)**
- **Ayah Range:** 20+ Ayahs
- **Surahs:** Very Long Surahs (120+ Ayahs)
- **Estimated Time:** 50-70 minutes
- **Passing Score:** 65%
- **Focus:** Complex Tajweed rules and continuation

### 5. **Expert (خبير)**
- **Ayah Range:** Full Surah or Multiple Surahs
- **Surahs:** All Surahs
- **Estimated Time:** 60-120 minutes
- **Passing Score:** 60%
- **Focus:** Mastery and comprehensive evaluation

---

## Task Types

### 1. **Memorization (تحفيظ)**
- Weight: 40%
- Evaluates accurate recitation without errors
- Criteria: Correct pronunciation, proper diacritics, no additions/omissions

### 2. **Tajweed (تجويد)**
- Weight: 30%
- Evaluates proper application of Quranic recitation rules
- Criteria: Noon/Tanween rules, Meem rules, Laam rules, proper stopping

### 3. **Fluency (الطلاقة)**
- Weight: 20%
- Evaluates smooth and continuous recitation
- Criteria: No sudden stops, natural flow, proper linking, rhythm maintenance

### 4. **Continuation (الاستمرار)**
- Weight: 10%
- Evaluates ability to continue from a random point
- Criteria: Smooth continuation, no confusion, accuracy maintenance

---

## Error Handling

All endpoints return appropriate HTTP status codes and error messages:

**Example Error Response:**
```json
{
  "error": "خطأ في إنشاء خطة الاختبار",
  "message": "Invalid difficulty level provided"
}
```

---

## Usage Examples

### Example 1: Generate a Beginner Test Plan
```bash
curl "http://localhost:8080/QuranTestPlan/Generate?level=Beginner&studentId=1"
```

### Example 2: Generate 5 Intermediate Test Plans
```bash
curl "http://localhost:8080/QuranTestPlan/GenerateMultiple?count=5&level=Intermediate"
```

### Example 3: Get Available Levels
```bash
curl "http://localhost:8080/QuranTestPlan/GetLevels"
```

### Example 4: Check Service Health
```bash
curl "http://localhost:8080/QuranTestPlan/Health"
```

---

## Integration with Admin Panel

The test plan generator can be integrated into the Admin Exam Panel to:

1. **Auto-generate exam requests** with structured test plans
2. **Assign appropriate difficulty levels** based on student progress
3. **Provide teacher guidance** with detailed notes and evaluation criteria
4. **Track student progress** through adaptive difficulty levels

---

## Features

✅ **114 Quranic Surahs** - Complete Quran coverage
✅ **5 Difficulty Levels** - Adaptive learning paths
✅ **4 Evaluation Tasks** - Comprehensive assessment
✅ **Tajweed Focus** - Specialized recitation rules
✅ **Teacher Notes** - Detailed guidance for instructors
✅ **Student Instructions** - Clear directions for learners
✅ **JSON Output** - Easy integration with frontend
✅ **Bilingual Support** - English and Arabic labels
✅ **Motivating Tone** - Encouraging Islamic learning ethics
✅ **Progressive Learning** - Encourages continuous improvement

---

## Notes

- All responses are in JSON format
- Timestamps are in ISO 8601 UTC format
- Arabic text is fully supported
- The service is stateless and can handle concurrent requests
- Test plans are randomly generated to provide variety
- Passing scores decrease with difficulty to maintain motivation

---

## Support

For issues or questions about the Quran Test Plan Generator API, please contact the development team or refer to the project documentation.
