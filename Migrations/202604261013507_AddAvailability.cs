namespace Maeen1_New.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAvailability : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TeacherAvailabilities",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TeacherName = c.String(),
                        Date = c.String(),
                        Time = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TeacherAvailabilities");
        }
    }
}
