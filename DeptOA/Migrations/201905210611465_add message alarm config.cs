namespace DeptOA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addmessagealarmconfig : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DEP_MessageAlarm",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        MessageID = c.String(),
                        AlarmDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DEP_MessageAlarm");
        }
    }
}
