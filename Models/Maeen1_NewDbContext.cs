using System.Data.Entity;

namespace Maeen1_New.Models
{
    public class Maeen1_NewDbContext : DbContext
    {
        public Maeen1_NewDbContext() : base("name=Maeen1ConnectionString")
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<TeacherAvailability> TeacherAvailabilities { get; set; }
    }
}