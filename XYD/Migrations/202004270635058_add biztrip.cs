namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addbiztrip : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.XYD_BizTrip",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        MessageID = c.String(),
                        EmplID = c.String(),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        CrateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.XYD_BizTrip");
        }
    }
}
