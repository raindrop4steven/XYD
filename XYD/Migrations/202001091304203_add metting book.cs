namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addmettingbook : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.XYD_MettingBook",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmplID = c.String(),
                        MeetingRoom = c.String(),
                        Name = c.String(),
                        StartTime = c.String(),
                        EndTime = c.String(),
                        Agreed = c.Boolean(nullable: false),
                        CreateTime = c.DateTime(nullable: false),
                        UpdateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.XYD_MettingBook");
        }
    }
}
