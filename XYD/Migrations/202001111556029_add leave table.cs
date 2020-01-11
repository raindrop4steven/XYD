namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addleavetable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.XYD_Leave_Record",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmplID = c.String(),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        Reason = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                        UpdateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.XYD_Leave_Record");
        }
    }
}
