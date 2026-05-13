using System.Data.Entity;
using Npgsql;

namespace Maeen1_New.Models
{
    public class Maeen1_NewDbContext : DbContext
    {
        public Maeen1_NewDbContext() : base("name=Maeen1ConnectionString")
        {
            // Set the PostgreSQL provider
            System.Data.Entity.SqlServer.SqlProviderServices.Instance.GetType();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<TeacherAvailability> TeacherAvailabilities { get; set; }
        public DbSet<StudentOnboardingProfile> StudentOnboardingProfiles { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamTask> ExamTasks { get; set; }
        public DbSet<StudentTaskCompletion> StudentTaskCompletions { get; set; }
        public DbSet<ExamAccessLog> ExamAccessLogs { get; set; }
        public DbSet<ExamRequest> ExamRequests { get; set; }
        public DbSet<ExamResult> ExamResults { get; set; }
        public DbSet<RecitationRequest> RecitationRequests { get; set; }
        public DbSet<RecitationSession> RecitationSessions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("public");
            base.OnModelCreating(modelBuilder);
        }
    }
}
