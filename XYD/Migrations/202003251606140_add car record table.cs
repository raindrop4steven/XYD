namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addcarrecordtable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.XYD_CarRecord",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        MessageID = c.String(),
                        ApplyUserID = c.String(),
                        ApplyDept = c.String(),
                        ApplyUser = c.String(),
                        ApplyDate = c.DateTime(nullable: false),
                        Reason = c.String(),
                        Location = c.String(),
                        DriverID = c.String(),
                        CarNo = c.String(),
                        StartMiles = c.Single(nullable: false),
                        EndMiles = c.Single(nullable: false),
                        Miles = c.Single(nullable: false),
                        CreateTime = c.DateTime(nullable: false),
                        UpdateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.XYD_CarRecord");
        }
    }
}
