namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addattence : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.XYD_Attence",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmplNo = c.String(),
                        EmplName = c.String(),
                        StartTime = c.DateTime(),
                        EndTime = c.DateTime(),
                        DeviceID = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.XYD_Attence");
        }
    }
}
