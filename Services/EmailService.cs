using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace Maeen1_New.Services
{
    public class EmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPassword;
        private readonly bool _enableSsl;
        private readonly string _fromAddress;
        private readonly string _fromName;

        public EmailService()
        {
            _smtpHost     = ConfigurationManager.AppSettings["SmtpHost"]     ?? "smtp.gmail.com";
            _smtpPort     = int.TryParse(ConfigurationManager.AppSettings["SmtpPort"], out var p) ? p : 587;
            _smtpUser     = ConfigurationManager.AppSettings["SmtpUser"]     ?? "";
            _smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"] ?? "";
            _enableSsl    = !string.Equals(ConfigurationManager.AppSettings["SmtpEnableSsl"], "false", StringComparison.OrdinalIgnoreCase);
            _fromAddress  = ConfigurationManager.AppSettings["SmtpFromAddress"] ?? _smtpUser;
            _fromName     = ConfigurationManager.AppSettings["SmtpFromName"]    ?? "منصة معين";
        }

        /// <summary>
        /// Returns true if SMTP credentials are configured.
        /// </summary>
        public bool IsConfigured()
        {
            return !string.IsNullOrWhiteSpace(_smtpUser) &&
                   !string.IsNullOrWhiteSpace(_smtpPassword) &&
                   !string.IsNullOrWhiteSpace(_smtpHost);
        }

        /// <summary>
        /// Sends an email. Returns null on success, error message on failure.
        /// </summary>
        public string Send(string toAddress, string toName, string subject, string htmlBody)
        {
            if (!IsConfigured())
                return "إعدادات البريد الإلكتروني غير مكتملة في Web.config";

            try
            {
                using (var client = new SmtpClient(_smtpHost, _smtpPort))
                {
                    client.EnableSsl   = _enableSsl;
                    client.Credentials = new NetworkCredential(_smtpUser, _smtpPassword);
                    client.Timeout     = 15000;

                    var from = new MailAddress(_fromAddress, _fromName);
                    var to   = new MailAddress(toAddress, toName);

                    using (var msg = new MailMessage(from, to))
                    {
                        msg.Subject    = subject;
                        msg.Body       = htmlBody;
                        msg.IsBodyHtml = true;

                        client.Send(msg);
                    }
                }

                return null; // success
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // ── Notification helpers ──────────────────────────────────────────────

        /// <summary>
        /// Notifies a teacher that an exam request has been assigned to them.
        /// </summary>
        public string NotifyTeacherAssigned(
            string teacherEmail, string teacherName,
            string examTitle, string studentName,
            int requestId, string appBaseUrl)
        {
            var link    = $"{appBaseUrl.TrimEnd('/')}/Teacher/ExamRequests?userId=";
            var subject = $"[معين] تم تعيينك لاختبار: {examTitle}";
            var body    = $@"
<div dir='rtl' style='font-family:Tajawal,Arial,sans-serif;max-width:600px;margin:auto;border:1px solid #e2e8f0;border-radius:12px;overflow:hidden'>
  <div style='background:linear-gradient(135deg,#064e3b,#059669);padding:24px 28px;color:white'>
    <h2 style='margin:0'>📋 طلب اختبار جديد</h2>
    <p style='margin:6px 0 0;opacity:.85'>منصة معين التعليمية</p>
  </div>
  <div style='padding:28px'>
    <p>مرحباً <strong>{teacherName}</strong>،</p>
    <p style='margin-top:12px'>تم تعيينك من قِبل الأدمن للإشراف على اختبار جديد:</p>
    <table style='width:100%;border-collapse:collapse;margin:18px 0;font-size:14px'>
      <tr style='background:#f8fafc'>
        <td style='padding:10px 14px;font-weight:bold;color:#475569;width:40%'>عنوان الاختبار</td>
        <td style='padding:10px 14px'>{examTitle}</td>
      </tr>
      <tr>
        <td style='padding:10px 14px;font-weight:bold;color:#475569'>الطالب</td>
        <td style='padding:10px 14px'>{studentName}</td>
      </tr>
      <tr style='background:#f8fafc'>
        <td style='padding:10px 14px;font-weight:bold;color:#475569'>رقم الطلب</td>
        <td style='padding:10px 14px'>#{requestId}</td>
      </tr>
    </table>
    <p style='margin-top:8px'>يرجى الدخول إلى لوحة التحكم لمراجعة خطة الاختبار وإرسال النتيجة.</p>
    <div style='margin-top:22px'>
      <a href='{link}' style='background:#059669;color:white;padding:12px 24px;border-radius:8px;text-decoration:none;font-size:15px'>
        🔗 الذهاب إلى طلبات الاختبارات
      </a>
    </div>
  </div>
  <div style='background:#f8fafc;padding:14px 28px;font-size:12px;color:#94a3b8;text-align:center'>
    منصة معين التعليمية © {DateTime.Now.Year}
  </div>
</div>";

            return Send(teacherEmail, teacherName, subject, body);
        }

        /// <summary>
        /// Notifies a teacher that a result they sent has been acknowledged.
        /// </summary>
        public string NotifyTeacherResultReceived(
            string teacherEmail, string teacherName,
            string examTitle, string studentName, string grade)
        {
            var subject = $"[معين] تم استلام نتيجة الاختبار: {examTitle}";
            var body    = $@"
<div dir='rtl' style='font-family:Tajawal,Arial,sans-serif;max-width:600px;margin:auto;border:1px solid #e2e8f0;border-radius:12px;overflow:hidden'>
  <div style='background:linear-gradient(135deg,#1e3a5f,#2563eb);padding:24px 28px;color:white'>
    <h2 style='margin:0'>✅ تم استلام النتيجة</h2>
    <p style='margin:6px 0 0;opacity:.85'>منصة معين التعليمية</p>
  </div>
  <div style='padding:28px'>
    <p>مرحباً <strong>{teacherName}</strong>،</p>
    <p style='margin-top:12px'>تم استلام نتيجة الاختبار التي أرسلتها بنجاح:</p>
    <table style='width:100%;border-collapse:collapse;margin:18px 0;font-size:14px'>
      <tr style='background:#f8fafc'>
        <td style='padding:10px 14px;font-weight:bold;color:#475569;width:40%'>عنوان الاختبار</td>
        <td style='padding:10px 14px'>{examTitle}</td>
      </tr>
      <tr>
        <td style='padding:10px 14px;font-weight:bold;color:#475569'>الطالب</td>
        <td style='padding:10px 14px'>{studentName}</td>
      </tr>
      <tr style='background:#f8fafc'>
        <td style='padding:10px 14px;font-weight:bold;color:#475569'>التقدير</td>
        <td style='padding:10px 14px'><strong style='color:#059669'>{grade}</strong></td>
      </tr>
    </table>
    <p>شكراً لجهودك!</p>
  </div>
  <div style='background:#f8fafc;padding:14px 28px;font-size:12px;color:#94a3b8;text-align:center'>
    منصة معين التعليمية © {DateTime.Now.Year}
  </div>
</div>";

            return Send(teacherEmail, teacherName, subject, body);
        }
    }
}
