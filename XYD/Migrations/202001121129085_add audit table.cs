namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addaudittable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.XYD_Audit_Record",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmplID = c.String(),
                        MessageID = c.String(),
                        Operation = c.String(),
                        Opinion = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                        UpdateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.XYD_Audit_Record");
        }
    }
}
